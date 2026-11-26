using Clinica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Clinica.Api.IntegrationTests.Infra;

/// <summary>
/// Um contêiner PostgreSQL 17 por execução; o esquema é migrado uma vez em um banco-modelo e cada teste
/// recebe uma cópia (<c>CREATE DATABASE ... TEMPLATE</c>), o que é muito mais rápido que migrar de novo (DT-30).
/// </summary>
[TestClass]
public static class Ambiente
{
    /// <summary>Usuário de conexão da aplicação (papel restrito, sem UPDATE/DELETE em auditoria).</summary>
    public const string UsuarioApp = "clinica_api";

    /// <summary>Senha do usuário da aplicação, válida apenas no contêiner de teste.</summary>
    public const string SenhaApp = "senha_teste_clinica";

    private const string BancoModelo = "clinica_modelo";
    private static PostgreSqlContainer? _container;

    private static PostgreSqlContainer Container => _container ?? throw new InvalidOperationException("Ambiente não iniciado.");

    [AssemblyInitialize]
    public static async Task IniciarAsync(TestContext _)
    {
        _container = new PostgreSqlBuilder("postgres:17").Build();
        await _container.StartAsync();

        await ExecutarAdminAsync($"CREATE DATABASE {BancoModelo}", "postgres");
        await using (var db = new ClinicaDbContext(
            new DbContextOptionsBuilder<ClinicaDbContext>().UseNpgsql(ConexaoDono(BancoModelo)).Options))
        {
            await db.Database.MigrateAsync();
        }

        // O papel de grupo clinica_app nasce na migration; o login da aplicação herda dele.
        await ExecutarAdminAsync($"CREATE ROLE {UsuarioApp} LOGIN PASSWORD '{SenhaApp}' IN ROLE clinica_app", "postgres");
        NpgsqlConnection.ClearAllPools();
    }

    [AssemblyCleanup]
    public static async Task EncerrarAsync()
    {
        if (_container is not null)
        {
            await _container.DisposeAsync();
        }
    }

    /// <summary>Cria um banco novo a partir do modelo migrado e devolve seu nome.</summary>
    public static async Task<string> NovoBancoAsync()
    {
        var nome = $"t_{Guid.NewGuid():N}";
        await ExecutarAdminAsync($"CREATE DATABASE {nome} TEMPLATE {BancoModelo}", "postgres");
        return nome;
    }

    /// <summary>Cria um banco vazio, sem migrations.</summary>
    public static async Task<string> NovoBancoVazioAsync()
    {
        var nome = $"v_{Guid.NewGuid():N}";
        await ExecutarAdminAsync($"CREATE DATABASE {nome}", "postgres");
        return nome;
    }

    /// <summary>Conexão com o papel dono (superusuário do contêiner), usada por migrations e verificações.</summary>
    public static string ConexaoDono(string banco) =>
        new NpgsqlConnectionStringBuilder(Container.GetConnectionString()) { Database = banco, Pooling = false }.ConnectionString;

    /// <summary>Conexão com o papel restrito da aplicação.</summary>
    public static string ConexaoApp(string banco) =>
        new NpgsqlConnectionStringBuilder(Container.GetConnectionString())
        {
            Database = banco,
            Username = UsuarioApp,
            Password = SenhaApp,
            Pooling = false,
        }.ConnectionString;

    private static async Task ExecutarAdminAsync(string sql, string banco)
    {
        await using var conexao = new NpgsqlConnection(ConexaoDono(banco));
        await conexao.OpenAsync();
        await using var cmd = new NpgsqlCommand(sql, conexao);
        await cmd.ExecuteNonQueryAsync();
    }
}
