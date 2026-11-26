namespace Clinica.Application.Common;

/// <summary>
/// Converte o relógio (UTC) para o fuso da clínica, <c>America/Sao_Paulo</c> (DT-26).
/// </summary>
public sealed class RelogioClinica(TimeProvider relogio)
{
    private static readonly TimeZoneInfo Fuso = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>Instante atual em UTC.</summary>
    public DateTimeOffset AgoraUtc => relogio.GetUtcNow();

    /// <summary>Instante atual no fuso da clínica.</summary>
    public DateTimeOffset Agora => ParaLocal(AgoraUtc);

    /// <summary>Data de calendário atual na clínica.</summary>
    public DateOnly Hoje => DateOnly.FromDateTime(Agora.DateTime);

    /// <summary>Converte um instante UTC para o fuso da clínica.</summary>
    public static DateTimeOffset ParaLocal(DateTimeOffset utc) => TimeZoneInfo.ConvertTime(utc, Fuso);
}
