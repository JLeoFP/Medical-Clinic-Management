using Clinica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Clinica.Infrastructure.Identity;

/// <summary>
/// Guarda o <see cref="AuthenticationTicket"/> protegido na tabela <c>sessoes</c>; o cookie leva só a chave (DT-45).
/// Aplica expiração deslizante de 30 min (renovada após metade do prazo pelo middleware) e limite absoluto de 12 h.
/// </summary>
public sealed class SessaoTicketStore(IServiceScopeFactory escopos, IDataProtectionProvider protecao, TimeProvider relogio)
    : ITicketStore
{
    /// <summary>Limite absoluto de vida da sessão.</summary>
    public static readonly TimeSpan LimiteAbsoluto = TimeSpan.FromHours(12);

    private readonly IDataProtector _protetor = protecao.CreateProtector("Clinica.Sessao.Ticket.v1");

    /// <inheritdoc />
    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var agora = relogio.GetUtcNow();
        var sessao = new Sessao
        {
            UsuarioId = Guid.Parse(ticket.Principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value),
            Ticket = Proteger(ticket),
            CriadaEm = agora,
            ExpiraAbsolutoEm = agora + LimiteAbsoluto,
        };
        sessao.ExpiraEm = Menor(ticket.Properties.ExpiresUtc ?? agora, sessao.ExpiraAbsolutoEm);

        await using var escopo = escopos.CreateAsyncScope();
        var db = escopo.ServiceProvider.GetRequiredService<ClinicaDbContext>();
        db.Sessoes.Add(sessao);
        await db.SaveChangesAsync();
        return sessao.Id.ToString();
    }

    /// <inheritdoc />
    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        if (!Guid.TryParse(key, out var id))
        {
            return;
        }

        var bytes = Proteger(ticket);
        await using var escopo = escopos.CreateAsyncScope();
        var db = escopo.ServiceProvider.GetRequiredService<ClinicaDbContext>();
        var sessao = await db.Sessoes.FirstOrDefaultAsync(s => s.Id == id);
        if (sessao is null || sessao.RevogadaEm is not null)
        {
            return;
        }

        sessao.Ticket = bytes;
        sessao.ExpiraEm = Menor(ticket.Properties.ExpiresUtc ?? sessao.ExpiraEm, sessao.ExpiraAbsolutoEm);
        await db.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        if (!Guid.TryParse(key, out var id))
        {
            return null;
        }

        await using var escopo = escopos.CreateAsyncScope();
        var db = escopo.ServiceProvider.GetRequiredService<ClinicaDbContext>();
        var sessao = await db.Sessoes.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        var agora = relogio.GetUtcNow();
        if (sessao is null || sessao.RevogadaEm is not null || sessao.ExpiraEm <= agora || sessao.ExpiraAbsolutoEm <= agora)
        {
            return null;
        }

        try
        {
            return TicketSerializer.Default.Deserialize(_protetor.Unprotect(sessao.Ticket));
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key)
    {
        if (!Guid.TryParse(key, out var id))
        {
            return;
        }

        await using var escopo = escopos.CreateAsyncScope();
        var db = escopo.ServiceProvider.GetRequiredService<ClinicaDbContext>();
        await db.Sessoes.Where(s => s.Id == id && s.RevogadaEm == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevogadaEm, relogio.GetUtcNow()));
    }

    private byte[] Proteger(AuthenticationTicket ticket) => _protetor.Protect(TicketSerializer.Default.Serialize(ticket));

    private static DateTimeOffset Menor(DateTimeOffset a, DateTimeOffset b) => a < b ? a : b;
}
