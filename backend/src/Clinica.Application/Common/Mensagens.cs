using MediatR;

namespace Clinica.Application.Common;

/// <summary>Comando que altera estado; executa em uma única transação (<see cref="Behaviors.TransactionBehavior{TRequest, TResponse}"/>).</summary>
/// <typeparam name="TResponse">Tipo da resposta.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>;

/// <summary>Consulta sem efeito colateral.</summary>
/// <typeparam name="TResponse">Tipo da resposta.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>;

/// <summary>
/// Marca uma query que deve gerar registro de auditoria de leitura (DT-19).
/// </summary>
/// <param name="entidade">Nome da entidade lida, gravado em <c>auditoria.entidade</c>.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class AuditarLeituraAttribute(string entidade) : Attribute
{
    /// <summary>Nome da entidade lida.</summary>
    public string Entidade { get; } = entidade;
}

/// <summary>Implementada pela query auditável que identifica um paciente.</summary>
public interface IReferenciaPaciente
{
    /// <summary>Paciente cujos dados são lidos.</summary>
    Guid? PacienteId { get; }
}
