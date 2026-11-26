using System.Net.Http.Json;
using System.Text.Json;
using Clinica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;
using Serilog.Core;
using Serilog.Events;

namespace Clinica.Api.IntegrationTests.Infra;

/// <summary>Sink que guarda as mensagens renderizadas para as verificações de privacidade dos logs.</summary>
public sealed class SinkEmMemoria : ILogEventSink
{
    private readonly List<string> _linhas = [];

    /// <summary>Linhas registradas (mensagem + propriedades).</summary>
    public IReadOnlyList<string> Linhas
    {
        get { lock (_linhas) { return [.. _linhas]; } }
    }

    /// <inheritdoc />
    public void Emit(LogEvent logEvent)
    {
        var texto = logEvent.RenderMessage() + " " + string.Join(' ', logEvent.Properties.Select(p => $"{p.Key}={p.Value}"));
        lock (_linhas) { _linhas.Add(texto); }
    }
}

/// <summary>O TestServer não define o IP de origem; fixa 127.0.0.1 para exercitar a gravação em <c>inet</c>.</summary>
public sealed class IpRemotoFixo : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use((contexto, proximo) =>
        {
            contexto.Connection.RemoteIpAddress ??= System.Net.IPAddress.Loopback;
            return proximo(contexto);
        });
        next(app);
    };
}

/// <summary>API em memória contra um banco PostgreSQL real, com relógio controlável.</summary>
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    public const string EmailAdmin = "admin@clinica.test";
    public const string SenhaAdmin = "Senha123";

    public ApiFactory(string banco, bool usarPapelRestrito = true, string ambiente = "Testing", bool semearAdmin = true)
    {
        Banco = banco;
        Conexao = usarPapelRestrito ? Ambiente.ConexaoApp(banco) : Ambiente.ConexaoDono(banco);
        AmbienteHost = ambiente;
        SemearAdmin = semearAdmin;
    }

    public string Banco { get; }

    public string Conexao { get; set; }

    public string AmbienteHost { get; }

    public bool SemearAdmin { get; }

    public FakeTimeProvider Relogio { get; } = new(new DateTimeOffset(2026, 11, 18, 12, 0, 0, TimeSpan.Zero));

    public SinkEmMemoria Logs { get; } = new();

    /// <summary>Registros extras de serviços (substituições, handlers de teste).</summary>
    public Action<IServiceCollection>? Servicos { get; set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(AmbienteHost);
        builder.UseSetting("ConnectionStrings:Clinica", Conexao);
        if (SemearAdmin)
        {
            builder.UseSetting("ADMIN_EMAIL", EmailAdmin);
            builder.UseSetting("ADMIN_SENHA_INICIAL", SenhaAdmin);
        }

        builder.ConfigureTestServices(s =>
        {
            s.RemoveAll<TimeProvider>();
            s.AddSingleton<TimeProvider>(Relogio);
            s.AddSingleton<ILogEventSink>(Logs);
            s.AddTransient<IStartupFilter, IpRemotoFixo>();
            Servicos?.Invoke(s);
        });
    }

    /// <summary>Executa <paramref name="acao"/> com um <see cref="ClinicaDbContext"/> do dono do banco (sem restrições).</summary>
    public async Task<T> ComoDonoAsync<T>(Func<ClinicaDbContext, Task<T>> acao)
    {
        await using var db = new ClinicaDbContext(
            new DbContextOptionsBuilder<ClinicaDbContext>().UseNpgsql(Ambiente.ConexaoDono(Banco)).Options);
        return await acao(db);
    }

    /// <summary>Cliente HTTP que mantém cookies e envia <c>X-XSRF-TOKEN</c> automaticamente nas escritas.</summary>
    public Navegador NovoNavegador() => new(CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        HandleCookies = false,
        AllowAutoRedirect = false,
    }));
}

/// <summary>Simula o navegador: guarda cookies (inclusive <c>Secure</c>) e replica o token XSRF no cabeçalho, como a SPA.</summary>
public sealed class Navegador(HttpClient http)
{
    private readonly Dictionary<string, string> _cookies = [];

    public IReadOnlyDictionary<string, string> Cookies => _cookies;

    /// <summary>Injeta um cookie (por exemplo, um de sessão já revogada).</summary>
    public void DefinirCookie(string nome, string valor) => _cookies[nome] = valor;

    public async Task<HttpResponseMessage> EnviarAsync(HttpMethod metodo, string url, object? corpo = null, bool comXsrf = true)
    {
        using var req = new HttpRequestMessage(metodo, url);
        if (corpo is not null)
        {
            req.Content = JsonContent.Create(corpo);
        }

        if (_cookies.Count > 0)
        {
            req.Headers.Add("Cookie", string.Join("; ", _cookies.Select(c => $"{c.Key}={c.Value}")));
        }

        if (comXsrf && _cookies.TryGetValue("__Host-XSRF-TOKEN", out var token))
        {
            req.Headers.Add("X-XSRF-TOKEN", token);
        }

        var resposta = await http.SendAsync(req);
        if (resposta.Headers.TryGetValues("Set-Cookie", out var definidos))
        {
            foreach (var linha in definidos)
            {
                var partes = linha.Split(';', StringSplitOptions.TrimEntries);
                var par = partes[0].Split('=', 2);
                var expirado = partes.Any(p => p.StartsWith("expires=", StringComparison.OrdinalIgnoreCase)
                    && DateTimeOffset.TryParse(p[8..], out var quando) && quando < DateTimeOffset.UtcNow);
                if (par[1].Length == 0 || expirado)
                {
                    _cookies.Remove(par[0]);
                }
                else
                {
                    _cookies[par[0]] = par[1];
                }
            }
        }

        return resposta;
    }

    public Task<HttpResponseMessage> GetAsync(string url) => EnviarAsync(HttpMethod.Get, url);

    public Task<HttpResponseMessage> PostAsync(string url, object? corpo = null, bool comXsrf = true) =>
        EnviarAsync(HttpMethod.Post, url, corpo, comXsrf);

    /// <summary>Restaura a sessão (emite o token XSRF) e faz o login.</summary>
    public async Task<HttpResponseMessage> EntrarAsync(string email, string senha)
    {
        (await GetAsync("/api/v1/auth/sessao")).Dispose();
        return await PostAsync("/api/v1/auth/login", new { email, senha });
    }

    public static async Task<JsonElement> JsonAsync(HttpResponseMessage r) =>
        (await r.Content.ReadFromJsonAsync<JsonElement>());
}
