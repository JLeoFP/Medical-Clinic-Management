namespace Clinica.Domain.Auditoria;

/// <summary>Tipos de ação registrados na trilha de auditoria (CHECK em <c>auditoria.acao</c>).</summary>
public enum AcaoAuditoria
{
    /// <summary>Leitura de dado sensível.</summary>
    Leitura,

    /// <summary>Criação de registro.</summary>
    Criacao,

    /// <summary>Alteração de registro.</summary>
    Alteracao,

    /// <summary>Estorno de cobrança.</summary>
    Estorno,

    /// <summary>Login bem-sucedido.</summary>
    Login,

    /// <summary>Tentativa de login rejeitada.</summary>
    LoginFalho,
}
