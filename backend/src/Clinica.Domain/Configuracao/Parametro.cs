using Clinica.Domain.Auditoria;
using Clinica.Domain.Shared;

namespace Clinica.Domain.Configuracao;

/// <summary>
/// Parâmetro de configuração da clínica, identificado por chave (TDD §9.2, <c>parametros</c>).
/// </summary>
public sealed class Parametro : IAuditavel
{
    /// <summary>Chave do parâmetro de janela da agenda, em meses.</summary>
    public const string JanelaAgendaMeses = "janela_agenda_meses";

    /// <summary>Chave do percentual de desconto para pagamento em dinheiro.</summary>
    public const string DescontoDinheiroPercentual = "desconto_dinheiro_percentual";

    /// <summary>Chave da antecedência mínima de cancelamento, em horas.</summary>
    public const string AntecedenciaCancelamentoHoras = "antecedencia_cancelamento_horas";

    /// <summary>Chave única, em snake_case.</summary>
    public string Chave { get; private set; }

    /// <summary>Valor textual.</summary>
    public string Valor { get; private set; }

    /// <summary>Construtor para o EF Core.</summary>
    private Parametro() => (Chave, Valor) = (string.Empty, string.Empty);

    /// <summary>Cria um parâmetro.</summary>
    public Parametro(string chave, string valor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chave);
        ArgumentException.ThrowIfNullOrWhiteSpace(valor);
        (Chave, Valor) = (chave, valor);
    }

    /// <summary>Altera o valor do parâmetro.</summary>
    /// <exception cref="DomainException">Quando o valor é vazio.</exception>
    public void Alterar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new DomainException("parametro.valor_invalido", "O valor do parâmetro é obrigatório.");
        }

        Valor = valor;
    }
}
