namespace Clinica.Domain.Shared;

/// <summary>
/// Entidade com identidade própria (UUID v7, DT-13). A igualdade é pelo <see cref="Id"/>.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    /// <summary>Identificador da entidade.</summary>
    public Guid Id { get; protected set; }

    /// <summary>Cria a entidade com um novo UUID v7.</summary>
    protected Entity() => Id = Guid.CreateVersion7();

    /// <summary>Cria a entidade com um identificador já existente (reidratação).</summary>
    protected Entity(Guid id) => Id = id;

    /// <inheritdoc />
    public bool Equals(Entity? other) =>
        other is not null && (ReferenceEquals(this, other) || (GetType() == other.GetType() && Id == other.Id));

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as Entity);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
