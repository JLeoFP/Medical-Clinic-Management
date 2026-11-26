using System.Net;
using System.Text;
using Clinica.Domain.Auditoria;
using Clinica.Domain.Configuracao;
using Clinica.Infrastructure.Identity;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Clinica.Infrastructure.Persistence;

/// <summary>
/// Contexto único do EF Core: schemas <c>clinica</c> e <c>identidade</c>, nomes em snake_case (DT-12, DT-37).
/// </summary>
public sealed class ClinicaDbContext(DbContextOptions<ClinicaDbContext> opcoes)
    : IdentityDbContext<Usuario, Perfil, Guid>(opcoes), IDataProtectionKeyContext
{
    /// <summary>Schema das tabelas de negócio.</summary>
    public const string SchemaClinica = "clinica";

    /// <summary>Schema das tabelas de identidade e sessão.</summary>
    public const string SchemaIdentidade = "identidade";

    /// <summary>Sessões de autenticação.</summary>
    public DbSet<Sessao> Sessoes => Set<Sessao>();

    /// <summary>Parâmetros da clínica.</summary>
    public DbSet<Parametro> Parametros => Set<Parametro>();

    /// <summary>Trilha de auditoria.</summary>
    public DbSet<RegistroAuditoria> Auditoria => Set<RegistroAuditoria>();

    /// <summary>Chaves do Data Protection.</summary>
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.HasDefaultSchema(SchemaClinica);

        b.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios", SchemaIdentidade);
            e.Property(x => x.Nome).HasMaxLength(150);
        });
        b.Entity<Perfil>(e =>
        {
            e.ToTable("perfis", SchemaIdentidade);
            e.HasData(
                new Perfil(Guid.Parse("0196a000-0000-7000-8000-000000000001"), Perfil.Administrador),
                new Perfil(Guid.Parse("0196a000-0000-7000-8000-000000000002"), Perfil.Medico),
                new Perfil(Guid.Parse("0196a000-0000-7000-8000-000000000003"), Perfil.Recepcao),
                new Perfil(Guid.Parse("0196a000-0000-7000-8000-000000000004"), Perfil.Paciente));
        });
        b.Entity<IdentityUserRole<Guid>>().ToTable("usuarios_perfis", SchemaIdentidade);
        b.Entity<IdentityUserClaim<Guid>>().ToTable("usuarios_claims", SchemaIdentidade);
        b.Entity<IdentityUserLogin<Guid>>().ToTable("usuarios_logins", SchemaIdentidade);
        b.Entity<IdentityUserToken<Guid>>().ToTable("usuarios_tokens", SchemaIdentidade);
        b.Entity<IdentityRoleClaim<Guid>>().ToTable("perfis_claims", SchemaIdentidade);

        b.Entity<DataProtectionKey>().ToTable("chaves_protecao_dados", SchemaIdentidade);

        b.Entity<Sessao>(e =>
        {
            e.ToTable("sessoes", SchemaIdentidade);
            e.HasKey(x => x.Id);
            e.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.UsuarioId);
            e.HasIndex(x => x.ExpiraEm);
        });

        b.Entity<Parametro>(e =>
        {
            e.ToTable("parametros");
            e.HasKey(x => x.Chave);
            e.Property(x => x.Chave).HasMaxLength(60);
            e.Property(x => x.Valor).HasMaxLength(100);
            e.HasData(
                new Parametro(Parametro.JanelaAgendaMeses, "2"),
                new Parametro(Parametro.DescontoDinheiroPercentual, "10"),
                new Parametro(Parametro.AntecedenciaCancelamentoHoras, "24"));
        });

        b.Entity<RegistroAuditoria>(e =>
        {
            e.ToTable("auditoria", t => t.HasCheckConstraint(
                "ck_auditoria_acao", "acao in ('Leitura','Criacao','Alteracao','Estorno','Login','LoginFalho')"));
            e.HasKey(x => x.Id);
            e.Property(x => x.Acao).HasConversion<string>().HasMaxLength(15);
            e.Property(x => x.Perfil).HasMaxLength(20);
            e.Property(x => x.Entidade).HasMaxLength(60);
            e.Property(x => x.DadosAntes).HasColumnType("jsonb");
            e.Property(x => x.DadosDepois).HasColumnType("jsonb");
            e.Property(x => x.Ip).HasColumnType("inet")
                .HasConversion(v => v == null ? null : IPAddress.Parse(v), v => v == null ? null : v.ToString());
            e.Property(x => x.CorrelationId).HasMaxLength(64);
            e.HasIndex(x => new { x.PacienteId, x.OcorridoEm });
            e.HasIndex(x => new { x.UsuarioId, x.OcorridoEm });
            e.HasIndex(x => new { x.Entidade, x.EntidadeId });
        });

        AplicarSnakeCase(b);
    }

    private static void AplicarSnakeCase(ModelBuilder b)
    {
        foreach (var propriedade in b.Model.GetEntityTypes().SelectMany(t => t.GetProperties()))
        {
            propriedade.SetColumnName(ParaSnakeCase(propriedade.GetColumnName()));
        }
    }

    /// <summary>Converte <c>UserName</c> em <c>user_name</c>.</summary>
    internal static string ParaSnakeCase(string nome)
    {
        var sb = new StringBuilder(nome.Length + 4);
        for (var i = 0; i < nome.Length; i++)
        {
            if (char.IsUpper(nome[i]) && i > 0 && nome[i - 1] != '_')
            {
                sb.Append('_');
            }

            sb.Append(char.ToLowerInvariant(nome[i]));
        }

        return sb.ToString();
    }
}
