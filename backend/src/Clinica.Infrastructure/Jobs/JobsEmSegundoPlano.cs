using Clinica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Clinica.Infrastructure.Jobs;

/// <summary>
/// <c>BackgroundService</c> único (DT-23): executa JOB-02 na inicialização e a cada 24 h; falhas são logadas e não derrubam a API.
/// </summary>
public sealed partial class JobsEmSegundoPlano(IServiceScopeFactory escopos, TimeProvider relogio, ILogger<JobsEmSegundoPlano> log)
    : BackgroundService
{
    /// <summary>Retenção de sessões expiradas ou revogadas.</summary>
    public static readonly TimeSpan RetencaoSessoes = TimeSpan.FromDays(30);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var temporizador = new PeriodicTimer(TimeSpan.FromHours(24), relogio);
        do
        {
            try
            {
                await LimparSessoesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                FalhaJob(log, ex);
            }
        }
        while (await temporizador.WaitForNextTickAsync(stoppingToken));
    }

    /// <summary>JOB-02: remove sessões expiradas ou revogadas há mais de 30 dias; idempotente.</summary>
    /// <returns>Quantidade de linhas removidas.</returns>
    public async Task<int> LimparSessoesAsync(CancellationToken ct)
    {
        var limite = relogio.GetUtcNow() - RetencaoSessoes;
        await using var escopo = escopos.CreateAsyncScope();
        var db = escopo.ServiceProvider.GetRequiredService<ClinicaDbContext>();
        var removidas = await db.Sessoes
            .Where(s => s.ExpiraEm < limite || s.ExpiraAbsolutoEm < limite || s.RevogadaEm < limite)
            .ExecuteDeleteAsync(ct);
        SessoesRemovidas(log, removidas);
        return removidas;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "JOB-02: {Quantidade} sessões antigas removidas.")]
    private static partial void SessoesRemovidas(ILogger log, int quantidade);

    [LoggerMessage(Level = LogLevel.Error, Message = "JOB-02 falhou.")]
    private static partial void FalhaJob(ILogger log, Exception ex);
}
