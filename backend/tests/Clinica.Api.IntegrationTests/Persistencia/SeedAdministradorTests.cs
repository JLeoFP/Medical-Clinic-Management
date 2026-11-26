using Clinica.Api.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Api.IntegrationTests.Persistencia;

[TestClass]
public sealed class SeedAdministradorTests
{
    [TestMethod]
    public async Task Primeira_execucao_cria_e_reinicio_nao_duplica()
    {
        var banco = await Ambiente.NovoBancoAsync();

        await using (var primeira = new ApiFactory(banco))
        {
            using var _ = primeira.CreateClient(); // força a inicialização do host
        }

        await using var segunda = new ApiFactory(banco);
        using var cliente = segunda.CreateClient();

        var (usuarios, nome, trocar, confirmado) = await segunda.ComoDonoAsync(async db =>
        {
            var u = await db.Users.SingleAsync();
            return (await db.Users.CountAsync(), u.Email, u.TrocaSenhaObrigatoria, u.EmailConfirmed);
        });
        usuarios.ShouldBe(1);
        nome.ShouldBe(ApiFactory.EmailAdmin);
        trocar.ShouldBeTrue();
        confirmado.ShouldBeTrue();
    }

    [TestMethod]
    public async Task Sem_variaveis_registra_aviso_e_nao_cria_usuario()
    {
        var banco = await Ambiente.NovoBancoAsync();
        await using var fabrica = new ApiFactory(banco, semearAdmin: false);
        using var cliente = fabrica.CreateClient();

        (await fabrica.ComoDonoAsync(db => db.Users.CountAsync())).ShouldBe(0);
        fabrica.Logs.Linhas.ShouldContain(l => l.Contains("ADMIN_EMAIL"));
    }
}
