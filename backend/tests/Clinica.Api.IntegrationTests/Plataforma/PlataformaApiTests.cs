using System.Net;
using Clinica.Api.IntegrationTests.Infra;
using Clinica.Application.Auth;
using Clinica.Domain.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace Clinica.Api.IntegrationTests.Plataforma;

[TestClass]
public sealed class PlataformaApiTests
{
    private static async Task<ApiFactory> NovaFabricaAsync(string ambiente = "Testing", Action<IServiceCollection>? servicos = null) =>
        new(await Ambiente.NovoBancoAsync(), ambiente: ambiente) { Servicos = servicos };

    [TestMethod]
    public async Task Rota_fora_de_api_v1_responde_404_em_problemdetails()
    {
        await using var fabrica = await NovaFabricaAsync();
        var r = await fabrica.NovoNavegador().GetAsync("/qualquer-coisa");

        r.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var json = await Navegador.JsonAsync(r);
        json.GetProperty("status").GetInt32().ShouldBe(404);
        json.GetProperty("code").GetString().ShouldBe("http.404");
        json.TryGetProperty("traceId", out _).ShouldBeTrue();
    }

    [TestMethod]
    public async Task Validacao_responde_400_com_errors_por_campo()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();
        (await nav.GetAsync("/api/v1/auth/sessao")).Dispose();

        var r = await nav.PostAsync("/api/v1/auth/login", new { email = "nao-e-email", senha = "" });

        r.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var json = await Navegador.JsonAsync(r);
        json.GetProperty("code").GetString().ShouldBe("validacao.invalida");
        json.GetProperty("errors").TryGetProperty("email", out _).ShouldBeTrue();
        json.GetProperty("errors").TryGetProperty("senha", out _).ShouldBeTrue();
    }

    [TestMethod]
    public async Task Regra_de_dominio_responde_422_com_codigo()
    {
        await using var fabrica = await NovaFabricaAsync(servicos: s =>
        {
            var falso = Substitute.For<IServicoAutenticacao>();
            falso.EntrarAsync(default!, default!, default).ReturnsForAnyArgs<UsuarioSessaoDto?>(_ => throw new DomainException("teste.regra", "Regra violada."));
            s.Replace(ServiceDescriptor.Scoped(_ => falso));
        });
        var nav = fabrica.NovoNavegador();
        (await nav.GetAsync("/api/v1/auth/sessao")).Dispose();

        var r = await nav.PostAsync("/api/v1/auth/login", new { email = "a@b.com", senha = "x" });

        r.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        (await Navegador.JsonAsync(r)).GetProperty("code").GetString().ShouldBe("teste.regra");
    }

    [TestMethod]
    public async Task Excecao_nao_tratada_responde_500_com_traceid_sem_detalhes_internos()
    {
        await using var fabrica = await NovaFabricaAsync(servicos: s =>
        {
            var falso = Substitute.For<IServicoAutenticacao>();
            falso.EntrarAsync(default!, default!, default).ReturnsForAnyArgs<UsuarioSessaoDto?>(_ => throw new InvalidOperationException("segredo interno"));
            s.Replace(ServiceDescriptor.Scoped(_ => falso));
        });
        var nav = fabrica.NovoNavegador();
        (await nav.GetAsync("/api/v1/auth/sessao")).Dispose();

        var r = await nav.PostAsync("/api/v1/auth/login", new { email = "a@b.com", senha = "x" });

        r.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        var texto = await r.Content.ReadAsStringAsync();
        texto.ShouldContain("traceId");
        texto.ShouldNotContain("segredo interno");
        texto.ShouldNotContain("InvalidOperationException");
    }

    [TestMethod]
    public async Task Health_responde_200_e_ready_verifica_o_banco()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();

        (await nav.GetAsync("/health")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await nav.GetAsync("/health/ready")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task Com_banco_indisponivel_ready_responde_503_e_health_continua_200()
    {
        await using var fabrica = await NovaFabricaAsync();
        // Porta sem servidor: simula o PostgreSQL fora do ar.
        fabrica.Conexao = "Host=127.0.0.1;Port=1;Database=x;Username=x;Password=x;Timeout=2;Pooling=false";
        var nav = fabrica.NovoNavegador();

        (await nav.GetAsync("/health")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await nav.GetAsync("/health/ready")).StatusCode.ShouldBe(HttpStatusCode.ServiceUnavailable);
    }

    [TestMethod]
    public async Task Openapi_e_swagger_apenas_em_development()
    {
        await using var dev = await NovaFabricaAsync("Development");
        await using var prod = await NovaFabricaAsync("Production");

        (await dev.NovoNavegador().GetAsync("/openapi/v1.json")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await dev.NovoNavegador().GetAsync("/swagger/index.html")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await prod.NovoNavegador().GetAsync("/openapi/v1.json")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await prod.NovoNavegador().GetAsync("/swagger/index.html")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task Log_de_login_nao_contem_email_nem_senha()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();
        const string email = "pessoa.privada@exemplo.com";
        const string senha = "SenhaSecreta999";

        (await nav.EntrarAsync(email, senha)).Dispose();

        var logs = fabrica.Logs.Linhas;
        logs.ShouldNotBeEmpty();
        logs.ShouldNotContain(l => l.Contains(email) || l.Contains(senha) || l.Contains("SenhaSecreta"));
        logs.ShouldContain(l => l.Contains("/api/v1/auth/login") && l.Contains("Elapsed") && l.Contains("CorrelationId"));
    }
}
