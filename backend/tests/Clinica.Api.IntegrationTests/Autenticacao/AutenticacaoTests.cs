using System.Net;
using Clinica.Api.IntegrationTests.Infra;
using Clinica.Infrastructure.Identity;
using Clinica.Infrastructure.Jobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clinica.Api.IntegrationTests.Autenticacao;

[TestClass]
public sealed class AutenticacaoTests
{
    private const string Login = "/api/v1/auth/login";
    private const string Sessao = "/api/v1/auth/sessao";
    private const string Logout = "/api/v1/auth/logout";

    private static async Task<ApiFactory> NovaFabricaAsync() => new(await Ambiente.NovoBancoAsync());

    [TestMethod]
    public async Task Login_valido_devolve_usuario_define_cookies_e_novo_token()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();
        (await nav.GetAsync(Sessao)).Dispose();
        var tokenAnonimo = nav.Cookies["__Host-XSRF-TOKEN"];

        var r = await nav.PostAsync(Login, new { email = ApiFactory.EmailAdmin, senha = ApiFactory.SenhaAdmin });

        r.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await Navegador.JsonAsync(r);
        json.GetProperty("perfis")[0].GetString().ShouldBe("Administrador");
        json.GetProperty("cadastroPendente").GetBoolean().ShouldBeFalse();
        json.GetProperty("trocaSenhaObrigatoria").GetBoolean().ShouldBeTrue();
        nav.Cookies.ShouldContainKey("__Host-clinica-sessao");
        nav.Cookies["__Host-XSRF-TOKEN"].ShouldNotBe(tokenAnonimo);

        // Atributos do cookie de sessão (DT-45).
        r.Headers.GetValues("Set-Cookie").Single(c => c.StartsWith("__Host-clinica-sessao", StringComparison.Ordinal))
            .ToLowerInvariant().ShouldSatisfyAllConditions(
                c => c.ShouldContain("httponly"), c => c.ShouldContain("secure"),
                c => c.ShouldContain("samesite=strict"), c => c.ShouldContain("path=/"), c => c.ShouldNotContain("domain="));
    }

    [TestMethod]
    public async Task Login_invalido_responde_401_sem_criar_sessao()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();

        var r = await nav.EntrarAsync(ApiFactory.EmailAdmin, "SenhaErrada1");

        r.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        nav.Cookies.ShouldNotContainKey("__Host-clinica-sessao");
        (await fabrica.ComoDonoAsync(db => db.Sessoes.CountAsync())).ShouldBe(0);
    }

    [TestMethod]
    public async Task Usuario_desativado_nao_entra()
    {
        await using var fabrica = await NovaFabricaAsync();
        using var inicio = fabrica.CreateClient();
        await fabrica.ComoDonoAsync(async db =>
        {
            await db.Users.ExecuteUpdateAsync(u => u.SetProperty(x => x.Ativo, false));
            return 0;
        });
        var nav = fabrica.NovoNavegador();

        (await nav.EntrarAsync(ApiFactory.EmailAdmin, ApiFactory.SenhaAdmin)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await fabrica.ComoDonoAsync(db => db.Sessoes.CountAsync())).ShouldBe(0);
    }

    [TestMethod]
    public async Task Cinco_falhas_bloqueiam_a_conta_mesmo_com_a_senha_correta()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();

        for (var i = 0; i < 5; i++)
        {
            (await nav.EntrarAsync(ApiFactory.EmailAdmin, "SenhaErrada1")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        (await nav.EntrarAsync(ApiFactory.EmailAdmin, ApiFactory.SenhaAdmin)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    public async Task Politica_de_senha_rejeita_curta_sem_letra_e_sem_numero()
    {
        await using var fabrica = await NovaFabricaAsync();
        using var inicio = fabrica.CreateClient();
        using var escopo = fabrica.Services.CreateScope();
        var usuarios = escopo.ServiceProvider.GetRequiredService<UserManager<Usuario>>();

        foreach (var fraca in new[] { "Ab1", "12345678", "abcdefgh" })
        {
            var r = await usuarios.CreateAsync(new Usuario { UserName = $"{fraca}@x.com", Email = $"{fraca}@x.com" }, fraca);
            r.Succeeded.ShouldBeFalse(fraca);
        }

        (await usuarios.CreateAsync(new Usuario { UserName = "ok@x.com", Email = "ok@x.com" }, "abcdefg1")).Succeeded.ShouldBeTrue();
    }

    [TestMethod]
    public async Task Sessao_devolve_200_com_cookie_e_204_sem_cookie()
    {
        await using var fabrica = await NovaFabricaAsync();
        var anonimo = fabrica.NovoNavegador();
        var logado = fabrica.NovoNavegador();
        (await logado.EntrarAsync(ApiFactory.EmailAdmin, ApiFactory.SenhaAdmin)).Dispose();

        var semSessao = await anonimo.GetAsync(Sessao);
        semSessao.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        anonimo.Cookies.ShouldContainKey("__Host-XSRF-TOKEN");
        (await logado.GetAsync(Sessao)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task Logout_revoga_a_sessao_e_a_requisicao_seguinte_recebe_401()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();
        (await nav.EntrarAsync(ApiFactory.EmailAdmin, ApiFactory.SenhaAdmin)).Dispose();
        var cookieAntigo = nav.Cookies["__Host-clinica-sessao"];

        (await nav.PostAsync(Logout)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await fabrica.ComoDonoAsync(db => db.Sessoes.CountAsync(s => s.RevogadaEm != null))).ShouldBe(1);

        // Repete a chamada com o cookie antigo: a sessão foi revogada no servidor.
        var velho = fabrica.NovoNavegador();
        velho.DefinirCookie("__Host-clinica-sessao", cookieAntigo);
        var req = await velho.EnviarAsync(HttpMethod.Post, Logout, comXsrf: false);
        req.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    public async Task Escrita_sem_token_antiforgery_responde_400_com_codigo()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();
        (await nav.GetAsync(Sessao)).Dispose();

        var r = await nav.PostAsync(Login, new { email = ApiFactory.EmailAdmin, senha = ApiFactory.SenhaAdmin }, comXsrf: false);

        r.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await Navegador.JsonAsync(r)).GetProperty("code").GetString().ShouldBe("seguranca.antiforgery_invalido");
        nav.Cookies.ShouldNotContainKey("__Host-clinica-sessao");
    }

    [TestMethod]
    public async Task Inatividade_de_31_minutos_expira_a_sessao()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();
        (await nav.EntrarAsync(ApiFactory.EmailAdmin, ApiFactory.SenhaAdmin)).Dispose();

        fabrica.Relogio.Advance(TimeSpan.FromMinutes(31));

        (await nav.GetAsync(Sessao)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task Atividade_apos_metade_do_prazo_renova_a_expiracao_deslizante()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();
        (await nav.EntrarAsync(ApiFactory.EmailAdmin, ApiFactory.SenhaAdmin)).Dispose();

        fabrica.Relogio.Advance(TimeSpan.FromMinutes(16));
        (await nav.GetAsync(Sessao)).StatusCode.ShouldBe(HttpStatusCode.OK); // renova
        fabrica.Relogio.Advance(TimeSpan.FromMinutes(20)); // 36 min desde o login, 20 desde a renovação

        (await nav.GetAsync(Sessao)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task Limite_absoluto_de_12_horas_vale_mesmo_com_atividade_continua()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();
        (await nav.EntrarAsync(ApiFactory.EmailAdmin, ApiFactory.SenhaAdmin)).Dispose();

        for (var i = 0; i < 23; i++) // 23 x 29 min = 11h07: sempre ativo
        {
            fabrica.Relogio.Advance(TimeSpan.FromMinutes(29));
            (await nav.GetAsync(Sessao)).StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        fabrica.Relogio.Advance(TimeSpan.FromMinutes(29 * 2)); // ultrapassa 12 h
        (await nav.GetAsync(Sessao)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task Desativar_usuario_e_trocar_senha_revogam_sessoes_imediatamente()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();
        (await nav.EntrarAsync(ApiFactory.EmailAdmin, ApiFactory.SenhaAdmin)).Dispose();
        var id = await fabrica.ComoDonoAsync(db => db.Users.Select(u => u.Id).SingleAsync());

        using (var escopo = fabrica.Services.CreateScope())
        {
            var gestao = escopo.ServiceProvider.GetRequiredService<GestaoAcesso>();
            (await gestao.TrocarSenhaAsync(id, ApiFactory.SenhaAdmin, "NovaSenha456")).Succeeded.ShouldBeTrue();
        }

        (await nav.GetAsync(Sessao)).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var segunda = fabrica.NovoNavegador();
        (await segunda.EntrarAsync(ApiFactory.EmailAdmin, "NovaSenha456")).StatusCode.ShouldBe(HttpStatusCode.OK);
        using (var escopo = fabrica.Services.CreateScope())
        {
            (await escopo.ServiceProvider.GetRequiredService<GestaoAcesso>().DesativarAsync(id)).ShouldBeTrue();
        }

        (await segunda.GetAsync(Sessao)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [TestMethod]
    public async Task Job_02_remove_sessoes_antigas_e_e_idempotente()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();
        (await nav.EntrarAsync(ApiFactory.EmailAdmin, ApiFactory.SenhaAdmin)).Dispose();
        (await nav.PostAsync(Logout)).Dispose(); // sessão revogada hoje

        var job = fabrica.Services.GetRequiredService<JobsEmSegundoPlano>();
        (await job.LimparSessoesAsync(default)).ShouldBe(0); // recente: permanece

        // Avançar 31 dias também dispara o ciclo diário do BackgroundService, que pode limpar antes da chamada abaixo.
        fabrica.Relogio.Advance(TimeSpan.FromDays(31));
        await job.LimparSessoesAsync(default);
        (await job.LimparSessoesAsync(default)).ShouldBe(0);
        (await fabrica.ComoDonoAsync(db => db.Sessoes.CountAsync())).ShouldBe(0);
    }
}
