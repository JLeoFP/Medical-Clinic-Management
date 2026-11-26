using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clinica.Infrastructure.Persistence;

/// <summary>
/// Fábrica usada apenas pelas ferramentas <c>dotnet ef</c> (migrations e bundle). As migrations rodam com o papel dono do banco,
/// definido em <c>ConnectionStrings__Clinica</c>; a API usa o papel restrito.
/// </summary>
public sealed class ClinicaDbContextFactory : IDesignTimeDbContextFactory<ClinicaDbContext>
{
    /// <inheritdoc />
    public ClinicaDbContext CreateDbContext(string[] args)
    {
        var conexao = Environment.GetEnvironmentVariable("ConnectionStrings__Clinica")
            ?? "Host=localhost;Database=clinica;Username=postgres";
        return new ClinicaDbContext(new DbContextOptionsBuilder<ClinicaDbContext>().UseNpgsql(conexao).Options);
    }
}
