using Clinica.Domain.Auditoria;

namespace Clinica.Application.Common;

/// <summary>Usuário autenticado da requisição corrente.</summary>
public interface IUsuarioAtual
{
    /// <summary>Id do usuário; nulo quando anônimo.</summary>
    Guid? Id { get; }

    /// <summary>Perfil principal; nulo quando anônimo.</summary>
    string? Perfil { get; }

    /// <summary>IP de origem da requisição.</summary>
    string? Ip { get; }

    /// <summary>Identificador de correlação da requisição.</summary>
    string? CorrelationId { get; }
}

/// <summary>Executa um bloco em uma transação de banco única.</summary>
public interface ITransacao
{
    /// <summary>Executa <paramref name="acao"/> e confirma somente se concluir sem exceção.</summary>
    Task<T> ExecutarAsync<T>(Func<Task<T>> acao, CancellationToken ct);
}

/// <summary>Porta de gravação da trilha de auditoria; cada chamada persiste por conta própria.</summary>
public interface IAuditoria
{
    /// <summary>Grava um registro, independentemente da transação do caso de uso.</summary>
    Task RegistrarAsync(
        AcaoAuditoria acao,
        string entidade,
        Guid? usuarioId = null,
        string? perfil = null,
        Guid? entidadeId = null,
        Guid? pacienteId = null,
        CancellationToken ct = default);
}
