namespace Clinica.Application.Common;

/// <summary>Falha de autenticação; a API responde 401 com <see cref="Codigo"/>.</summary>
public sealed class NaoAutenticadoException(string codigo, string mensagem) : Exception(mensagem)
{
    /// <summary>Código estável do erro.</summary>
    public string Codigo { get; } = codigo;
}
