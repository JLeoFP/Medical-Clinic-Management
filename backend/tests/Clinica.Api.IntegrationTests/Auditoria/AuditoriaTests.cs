using Clinica.Api.IntegrationTests.Infra;
using Clinica.Application.Common;
using Clinica.Domain.Auditoria;
using Clinica.Domain.Configuracao;
using Clinica.Domain.Shared;
using Clinica.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clinica.Api.IntegrationTests.Auditoria;

[AuditarLeitura("Paciente")]
public sealed record ConsultaAuditadaDeTeste(Guid? PacienteId) : IQuery<string>, IReferenciaPaciente;

public sealed class ConsultaAuditadaHandler : IRequestHandler<ConsultaAuditadaDeTeste, string>
{
    public Task<string> Handle(ConsultaAuditadaDeTeste request, CancellationToken ct) => Task.FromResult("dados");
}

public sealed record ComandoQueFalha : ICommand<string>;

public sealed class ComandoQueFalhaHandler(ClinicaDbContext db) : IRequestHandler<ComandoQueFalha, string>
{
    public async Task<string> Handle(ComandoQueFalha request, CancellationToken ct)
    {
        (await db.Parametros.FindAsync([Parametro.JanelaAgendaMeses], ct))!.Alterar("99");
        await db.SaveChangesAsync(ct); // grava parcialmente...
        throw new DomainException("teste.falha", "Falha após gravar."); // ...e falha: a transação deve desfazer
    }
}

[TestClass]
public sealed class AuditoriaTests
{
    private static async Task<ApiFactory> NovaFabricaAsync() => new(await Ambiente.NovoBancoAsync())
    {
        Servicos = s =>
        {
            s.AddTransient<IRequestHandler<ConsultaAuditadaDeTeste, string>, ConsultaAuditadaHandler>();
            s.AddTransient<IRequestHandler<ComandoQueFalha, string>, ComandoQueFalhaHandler>();
        },
    };

    [TestMethod]
    public async Task Login_do_admin_gera_linha_Login_e_falha_gera_LoginFalho()
    {
        await using var fabrica = await NovaFabricaAsync();
        var nav = fabrica.NovoNavegador();

        (await nav.EntrarAsync(ApiFactory.EmailAdmin, "Errada123")).Dispose();
        (await nav.EntrarAsync(ApiFactory.EmailAdmin, ApiFactory.SenhaAdmin)).Dispose();

        var adminId = await fabrica.ComoDonoAsync(db => db.Users.Select(u => u.Id).SingleAsync());
        var linhas = await fabrica.ComoDonoAsync(db => db.Auditoria.OrderBy(a => a.OcorridoEm).ToListAsync());
        var falho = linhas.Single(l => l.Acao == AcaoAuditoria.LoginFalho);
        falho.DadosAntes.ShouldBeNull();
        falho.DadosDepois.ShouldBeNull();
        var login = linhas.Single(l => l.Acao == AcaoAuditoria.Login);
        login.UsuarioId.ShouldBe(adminId);
        login.Perfil.ShouldBe("Administrador");
        login.Ip.ShouldNotBeNull();
        login.CorrelationId.ShouldNotBeNullOrEmpty();
        login.OcorridoEm.ShouldBe(fabrica.Relogio.GetUtcNow());
    }

    [TestMethod]
    public async Task Alteracao_de_parametro_grava_apenas_as_colunas_alteradas()
    {
        await using var fabrica = await NovaFabricaAsync();
        using var inicio = fabrica.CreateClient();
        await using (var escopo = fabrica.Services.CreateAsyncScope())
        {
            var db = escopo.ServiceProvider.GetRequiredService<ClinicaDbContext>();
            (await db.Parametros.FindAsync(Parametro.JanelaAgendaMeses))!.Alterar("3");
            await db.SaveChangesAsync();
        }

        var linha = await fabrica.ComoDonoAsync(db => db.Auditoria.SingleAsync(a => a.Acao == AcaoAuditoria.Alteracao));
        linha.Entidade.ShouldBe("Parametro:janela_agenda_meses");
        // O jsonb normaliza espaços; compara sem eles.
        linha.DadosAntes!.Replace(" ", "").ShouldBe("""{"valor":"2"}""");
        linha.DadosDepois!.Replace(" ", "").ShouldBe("""{"valor":"3"}""");
    }

    [TestMethod]
    public async Task Query_marcada_gera_linha_Leitura()
    {
        await using var fabrica = await NovaFabricaAsync();
        using var inicio = fabrica.CreateClient();
        var paciente = Guid.CreateVersion7();
        await using (var escopo = fabrica.Services.CreateAsyncScope())
        {
            (await escopo.ServiceProvider.GetRequiredService<ISender>().Send(new ConsultaAuditadaDeTeste(paciente))).ShouldBe("dados");
        }

        var linha = await fabrica.ComoDonoAsync(db => db.Auditoria.SingleAsync(a => a.Acao == AcaoAuditoria.Leitura));
        linha.Entidade.ShouldBe("Paciente");
        linha.PacienteId.ShouldBe(paciente);
    }

    [TestMethod]
    public async Task Command_que_falha_desfaz_tudo_que_gravou()
    {
        await using var fabrica = await NovaFabricaAsync();
        using var inicio = fabrica.CreateClient();
        await using (var escopo = fabrica.Services.CreateAsyncScope())
        {
            await Should.ThrowAsync<DomainException>(() =>
                escopo.ServiceProvider.GetRequiredService<ISender>().Send(new ComandoQueFalha()));
        }

        (await fabrica.ComoDonoAsync(db => db.Parametros.Where(p => p.Chave == Parametro.JanelaAgendaMeses).Select(p => p.Valor).SingleAsync()))
            .ShouldBe("2");
        (await fabrica.ComoDonoAsync(db => db.Auditoria.CountAsync(a => a.Acao == AcaoAuditoria.Alteracao))).ShouldBe(0);
    }
}
