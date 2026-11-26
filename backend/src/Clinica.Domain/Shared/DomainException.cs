namespace Clinica.Domain.Shared;

/// <summary>
/// Violação de uma regra de negócio. A API a traduz em ProblemDetails 422 com o <see cref="Codigo"/> de domínio (DT-39).
/// </summary>
public class DomainException : Exception
{
    /// <summary>Código estável de domínio, por exemplo <c>paciente.cpf_duplicado</c>.</summary>
    public string Codigo { get; }

    /// <summary>Cria a exceção com o código de domínio e a mensagem em português.</summary>
    /// <param name="codigo">Código estável, em minúsculas e separado por ponto.</param>
    /// <param name="mensagem">Mensagem legível para o usuário.</param>
    public DomainException(string codigo, string mensagem) : base(mensagem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        Codigo = codigo;
    }
}
