using Microsoft.AspNetCore.Identity;

namespace Clinica.Infrastructure.Identity;

/// <summary>Conta de acesso ao sistema (tabela <c>identidade.usuarios</c>).</summary>
public sealed class Usuario : IdentityUser<Guid>
{
    /// <summary>Nome de exibição.</summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>Usuário desativado não consegue entrar (RF-37).</summary>
    public bool Ativo { get; set; } = true;

    /// <summary>Exige troca de senha no primeiro acesso (DT-20).</summary>
    public bool TrocaSenhaObrigatoria { get; set; }

    /// <summary>Cria o usuário com UUID v7.</summary>
    public Usuario() => Id = Guid.CreateVersion7();
}

/// <summary>Perfil de acesso (tabela <c>identidade.perfis</c>).</summary>
public sealed class Perfil : IdentityRole<Guid>
{
    /// <summary>Nome do perfil Administrador.</summary>
    public const string Administrador = "Administrador";

    /// <summary>Nome do perfil Médico.</summary>
    public const string Medico = "Medico";

    /// <summary>Nome do perfil Recepção.</summary>
    public const string Recepcao = "Recepcao";

    /// <summary>Nome do perfil Paciente.</summary>
    public const string Paciente = "Paciente";

    /// <summary>Construtor para o EF Core.</summary>
    public Perfil() { }

    /// <summary>Cria um perfil com identificador fixo (seed).</summary>
    public Perfil(Guid id, string nome) : base(nome)
    {
        Id = id;
        NormalizedName = nome.ToUpperInvariant();
        ConcurrencyStamp = id.ToString();
    }
}

/// <summary>Sessão de autenticação gravada no servidor (tabela <c>identidade.sessoes</c>, DT-45).</summary>
public sealed class Sessao
{
    /// <summary>Chave da sessão; único dado no cookie.</summary>
    public Guid Id { get; set; } = Guid.CreateVersion7();

    /// <summary>Dono da sessão.</summary>
    public Guid UsuarioId { get; set; }

    /// <summary>Ticket serializado e protegido.</summary>
    public byte[] Ticket { get; set; } = [];

    /// <summary>Instante de criação.</summary>
    public DateTimeOffset CriadaEm { get; set; }

    /// <summary>Expiração deslizante (30 min).</summary>
    public DateTimeOffset ExpiraEm { get; set; }

    /// <summary>Limite absoluto (criação + 12 h).</summary>
    public DateTimeOffset ExpiraAbsolutoEm { get; set; }

    /// <summary>Revogação (logout, desativação, troca de senha).</summary>
    public DateTimeOffset? RevogadaEm { get; set; }
}
