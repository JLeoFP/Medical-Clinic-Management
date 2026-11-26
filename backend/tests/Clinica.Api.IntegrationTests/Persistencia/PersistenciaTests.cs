using Clinica.Api.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Clinica.Api.IntegrationTests.Persistencia;

[TestClass]
public sealed class PersistenciaTests
{
    private static async Task<T> EscalarAsync<T>(string conexao, string sql)
    {
        await using var c = new NpgsqlConnection(conexao);
        await c.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, c);
        return (T)(await cmd.ExecuteScalarAsync())!;
    }

    [TestMethod]
    public async Task Migration_em_banco_vazio_cria_tabelas_perfis_e_parametros()
    {
        var banco = await Ambiente.NovoBancoVazioAsync();
        await using (var db = new Infrastructure.Persistence.ClinicaDbContext(
            new DbContextOptionsBuilder<Infrastructure.Persistence.ClinicaDbContext>().UseNpgsql(Ambiente.ConexaoDono(banco)).Options))
        {
            await db.Database.MigrateAsync();
        }

        var dono = Ambiente.ConexaoDono(banco);
        var tabelas = await EscalarAsync<long>(dono,
            "select count(*) from information_schema.tables where (table_schema, table_name) in " +
            "(('identidade','usuarios'),('identidade','perfis'),('identidade','usuarios_perfis'),('identidade','sessoes')," +
            "('identidade','chaves_protecao_dados'),('clinica','parametros'),('clinica','auditoria'))");
        tabelas.ShouldBe(7);
        (await EscalarAsync<long>(dono, "select count(*) from identidade.perfis")).ShouldBe(4);
        (await EscalarAsync<string>(dono, "select valor from clinica.parametros where chave = 'janela_agenda_meses'")).ShouldBe("2");
        (await EscalarAsync<string>(dono, "select valor from clinica.parametros where chave = 'desconto_dinheiro_percentual'")).ShouldBe("10");
        (await EscalarAsync<string>(dono, "select valor from clinica.parametros where chave = 'antecedencia_cancelamento_horas'")).ShouldBe("24");
    }

    [TestMethod]
    public async Task Papel_da_aplicacao_insere_mas_nao_altera_nem_apaga_auditoria()
    {
        var banco = await Ambiente.NovoBancoAsync();
        await using var c = new NpgsqlConnection(Ambiente.ConexaoApp(banco));
        await c.OpenAsync();

        await using (var ins = new NpgsqlCommand(
            "insert into clinica.auditoria (id, ocorrido_em, acao, entidade) values (gen_random_uuid(), now(), 'Login', 'Usuario')", c))
        {
            (await ins.ExecuteNonQueryAsync()).ShouldBe(1);
        }

        foreach (var sql in new[] { "update clinica.auditoria set perfil = 'x'", "delete from clinica.auditoria" })
        {
            await using var cmd = new NpgsqlCommand(sql, c);
            var ex = await Should.ThrowAsync<PostgresException>(() => cmd.ExecuteNonQueryAsync());
            ex.SqlState.ShouldBe("42501"); // insufficient_privilege
        }
    }

    [TestMethod]
    public async Task Api_sobe_contra_esquema_migrado_sem_alterar_o_esquema()
    {
        var banco = await Ambiente.NovoBancoAsync();
        await using var fabrica = new ApiFactory(banco);
        using var cliente = fabrica.CreateClient();

        (await cliente.GetAsync("/health/ready")).IsSuccessStatusCode.ShouldBeTrue();
        var pendentes = await fabrica.ComoDonoAsync(db => db.Database.GetPendingMigrationsAsync());
        pendentes.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task Api_nao_aplica_migrations_ao_iniciar()
    {
        var banco = await Ambiente.NovoBancoVazioAsync();
        await using var fabrica = new ApiFactory(banco, usarPapelRestrito: false);
        using var cliente = fabrica.CreateClient();

        (await cliente.GetAsync("/health")).IsSuccessStatusCode.ShouldBeTrue();
        (await EscalarAsync<long>(Ambiente.ConexaoDono(banco),
            "select count(*) from information_schema.tables where table_schema in ('clinica','identidade')")).ShouldBe(0);
    }
}
