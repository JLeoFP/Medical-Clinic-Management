using Clinica.Domain.Shared;

namespace Clinica.Domain.Tests.Shared;

[TestClass]
public sealed class SharedTests
{
    private sealed class Exemplo : AggregateRoot
    {
        public Exemplo() { }
        public Exemplo(Guid id) : base(id) { }
        public void Disparar() => Registrar(new Evento());
    }

    private sealed record Evento : IDomainEvent
    {
        public DateTimeOffset OcorridoEm { get; } = DateTimeOffset.UnixEpoch;
    }

    [TestMethod]
    public void Entidade_nova_recebe_uuid_v7()
    {
        var e = new Exemplo();
        e.Id.Version.ShouldBe(7);
    }

    [TestMethod]
    public void Entidades_com_mesmo_id_sao_iguais()
    {
        var id = Guid.CreateVersion7();
        new Exemplo(id).ShouldBe(new Exemplo(id));
        new Exemplo().Equals(new Exemplo()).ShouldBeFalse();
    }

    [TestMethod]
    public void Raiz_acumula_e_limpa_eventos()
    {
        var raiz = new Exemplo();
        raiz.Disparar();
        raiz.EventosDeDominio.Count.ShouldBe(1);
        raiz.LimparEventos();
        raiz.EventosDeDominio.ShouldBeEmpty();
    }

    [TestMethod]
    public void DomainException_exige_codigo()
    {
        new DomainException("a.b", "msg").Codigo.ShouldBe("a.b");
        Should.Throw<ArgumentException>(() => new DomainException(" ", "msg"));
    }

    [TestMethod]
    public void Domain_nao_referencia_outros_projetos()
    {
        typeof(Entity).Assembly.GetReferencedAssemblies()
            .Where(a => a.Name!.StartsWith("Clinica", StringComparison.Ordinal))
            .ShouldBeEmpty();
    }
}
