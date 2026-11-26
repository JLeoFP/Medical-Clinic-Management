using System.Text.Json;
using Clinica.Application.Common;
using Clinica.Domain.Auditoria;
using Clinica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Clinica.Infrastructure.Auditoria;

/// <summary>
/// Grava <c>Criacao</c>/<c>Alteracao</c> das entidades <see cref="IAuditavel"/> na mesma transação do <c>SaveChanges</c>,
/// com apenas as colunas alteradas (DT-19).
/// </summary>
public sealed class AuditoriaInterceptor(IUsuarioAtual usuario, TimeProvider relogio) : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } db)
        {
            Registrar(db);
        }

        return ValueTask.FromResult(result);
    }

    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is { } db)
        {
            Registrar(db);
        }

        return result;
    }

    private void Registrar(DbContext db)
    {
        db.ChangeTracker.DetectChanges();
        var entradas = db.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditavel && e.State is EntityState.Added or EntityState.Modified)
            .ToList();

        foreach (var e in entradas)
        {
            var criacao = e.State == EntityState.Added;
            var props = e.Properties.Where(p => criacao || (p.IsModified && !p.Metadata.IsPrimaryKey())).ToList();
            if (props.Count == 0)
            {
                continue;
            }

            var chave = e.Metadata.FindPrimaryKey()!.Properties[0];
            var valorChave = e.Property(chave.Name).CurrentValue;
            var entidade = e.Metadata.ClrType.Name;
            Guid? entidadeId = null;
            if (valorChave is Guid g)
            {
                entidadeId = g;
            }
            else
            {
                entidade = $"{entidade}:{valorChave}";
            }

            var antes = criacao ? null : Serializar(props, p => p.OriginalValue);
            var depois = Serializar(props, p => p.CurrentValue);
            db.Set<RegistroAuditoria>().Add(new RegistroAuditoria(
                relogio.GetUtcNow(), criacao ? AcaoAuditoria.Criacao : AcaoAuditoria.Alteracao, entidade,
                usuario.Id, usuario.Perfil, entidadeId, dadosAntes: antes, dadosDepois: depois,
                ip: usuario.Ip, correlationId: usuario.CorrelationId));
        }
    }

    private static string Serializar(IEnumerable<PropertyEntry> props, Func<PropertyEntry, object?> valor) =>
        JsonSerializer.Serialize(props.ToDictionary(p => ClinicaDbContext.ParaSnakeCase(p.Metadata.Name), valor));
}

/// <summary>Grava registros de auditoria em um contexto próprio, independente da transação do caso de uso.</summary>
public sealed class AuditoriaServico(IDbContextFactory<ClinicaDbContext> fabrica, IUsuarioAtual atual, TimeProvider relogio) : IAuditoria
{
    /// <inheritdoc />
    public async Task RegistrarAsync(
        AcaoAuditoria acao, string entidade, Guid? usuarioId = null, string? perfil = null,
        Guid? entidadeId = null, Guid? pacienteId = null, CancellationToken ct = default)
    {
        await using var db = await fabrica.CreateDbContextAsync(ct);
        db.Auditoria.Add(new RegistroAuditoria(
            relogio.GetUtcNow(), acao, entidade, usuarioId ?? atual.Id, perfil ?? atual.Perfil, entidadeId, pacienteId,
            ip: atual.Ip, correlationId: atual.CorrelationId));
        await db.SaveChangesAsync(ct);
    }
}

/// <summary>Transação única por command (DT-02).</summary>
public sealed class TransacaoEf(ClinicaDbContext db) : ITransacao
{
    /// <inheritdoc />
    public async Task<T> ExecutarAsync<T>(Func<Task<T>> acao, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var resultado = await acao();
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return resultado;
    }
}
