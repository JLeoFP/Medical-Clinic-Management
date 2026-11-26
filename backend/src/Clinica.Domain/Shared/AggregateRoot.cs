namespace Clinica.Domain.Shared;

/// <summary>
/// Raiz de agregado: acumula eventos de domínio até a aplicação publicá-los.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _eventos = [];

    /// <summary>Cria a raiz com um novo UUID v7.</summary>
    protected AggregateRoot() { }

    /// <summary>Cria a raiz com um identificador já existente (reidratação).</summary>
    protected AggregateRoot(Guid id) : base(id) { }

    /// <summary>Eventos ainda não publicados.</summary>
    public IReadOnlyCollection<IDomainEvent> EventosDeDominio => _eventos;

    /// <summary>Registra um evento para publicação posterior.</summary>
    protected void Registrar(IDomainEvent evento) => _eventos.Add(evento);

    /// <summary>Descarta os eventos após a publicação.</summary>
    public void LimparEventos() => _eventos.Clear();
}
