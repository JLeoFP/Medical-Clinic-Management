using Clinica.Domain.Shared;

namespace Clinica.Domain.Auditoria;

/// <summary>
/// Linha imutável da trilha de auditoria (somente inserção, RNF-03).
/// </summary>
public sealed class RegistroAuditoria : Entity
{
    /// <summary>Instante UTC da ocorrência.</summary>
    public DateTimeOffset OcorridoEm { get; private set; }

    /// <summary>Usuário responsável; nulo para o job.</summary>
    public Guid? UsuarioId { get; private set; }

    /// <summary>Perfil do usuário no momento da ação.</summary>
    public string? Perfil { get; private set; }

    /// <summary>Ação registrada.</summary>
    public AcaoAuditoria Acao { get; private set; }

    /// <summary>Nome da entidade afetada.</summary>
    public string Entidade { get; private set; }

    /// <summary>Identificador da entidade afetada.</summary>
    public Guid? EntidadeId { get; private set; }

    /// <summary>Paciente relacionado, para consultar quem acessou seus dados.</summary>
    public Guid? PacienteId { get; private set; }

    /// <summary>JSON com os valores anteriores (apenas colunas alteradas).</summary>
    public string? DadosAntes { get; private set; }

    /// <summary>JSON com os novos valores (apenas colunas alteradas).</summary>
    public string? DadosDepois { get; private set; }

    /// <summary>Endereço IP de origem.</summary>
    public string? Ip { get; private set; }

    /// <summary>Identificador de correlação da requisição.</summary>
    public string? CorrelationId { get; private set; }

    /// <summary>Construtor para o EF Core.</summary>
    private RegistroAuditoria() => Entidade = string.Empty;

    /// <summary>Cria um registro de auditoria.</summary>
    public RegistroAuditoria(
        DateTimeOffset ocorridoEm,
        AcaoAuditoria acao,
        string entidade,
        Guid? usuarioId = null,
        string? perfil = null,
        Guid? entidadeId = null,
        Guid? pacienteId = null,
        string? dadosAntes = null,
        string? dadosDepois = null,
        string? ip = null,
        string? correlationId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entidade);
        (OcorridoEm, Acao, Entidade, UsuarioId, Perfil) = (ocorridoEm, acao, entidade, usuarioId, perfil);
        (EntidadeId, PacienteId, DadosAntes, DadosDepois, Ip, CorrelationId) =
            (entidadeId, pacienteId, dadosAntes, dadosDepois, ip, correlationId);
    }
}
