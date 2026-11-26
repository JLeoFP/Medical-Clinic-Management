using System.Diagnostics;
using System.Reflection;
using Clinica.Domain.Auditoria;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Clinica.Application.Common.Behaviors;

/// <summary>Registra nome e duração de cada request, sem o conteúdo (RNF-01, DT-28).</summary>
public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> log)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var inicio = Stopwatch.GetTimestamp();
        try
        {
            return await next(ct);
        }
        finally
        {
            log.LogInformation("Request {Request} concluído em {Elapsed} ms",
                typeof(TRequest).Name, Stopwatch.GetElapsedTime(inicio).TotalMilliseconds);
        }
    }
}

/// <summary>Executa os validadores FluentValidation e lança <see cref="ValidationException"/> se houver falhas.</summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validadores)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var contexto = new ValidationContext<TRequest>(request);
        var resultados = await Task.WhenAll(validadores.Select(v => v.ValidateAsync(contexto, ct)));
        var falhas = resultados.SelectMany(r => r.Errors).ToList();
        if (falhas.Count > 0)
        {
            throw new ValidationException(falhas);
        }

        return await next(ct);
    }
}

/// <summary>Envolve cada <see cref="ICommand{TResponse}"/> em uma transação única.</summary>
public sealed class TransactionBehavior<TRequest, TResponse>(ITransacao transacao)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    /// <inheritdoc />
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct) =>
        request is ICommand<TResponse> ? transacao.ExecutarAsync(() => next(ct), ct) : next(ct);
}

/// <summary>Grava <see cref="AcaoAuditoria.Leitura"/> para queries marcadas com <see cref="AuditarLeituraAttribute"/> (DT-19).</summary>
public sealed class AuditoriaLeituraBehavior<TRequest, TResponse>(IAuditoria auditoria, IUsuarioAtual usuario)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private static readonly AuditarLeituraAttribute? Marca = typeof(TRequest).GetCustomAttribute<AuditarLeituraAttribute>();

    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var resposta = await next(ct);
        if (Marca is not null)
        {
            // Só registra após a leitura bem-sucedida.
            await auditoria.RegistrarAsync(AcaoAuditoria.Leitura, Marca.Entidade, usuario.Id, usuario.Perfil,
                pacienteId: (request as IReferenciaPaciente)?.PacienteId, ct: ct);
        }

        return resposta;
    }
}
