namespace Clinica.Domain.Shared;

/// <summary>
/// Marca um fato relevante ocorrido no domínio. Publicado pela camada de aplicação após a persistência.
/// </summary>
public interface IDomainEvent
{
    /// <summary>Instante UTC em que o evento ocorreu.</summary>
    DateTimeOffset OcorridoEm { get; }
}
