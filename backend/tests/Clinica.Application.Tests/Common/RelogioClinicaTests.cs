using Clinica.Application.Common;
using Microsoft.Extensions.Time.Testing;

namespace Clinica.Application.Tests.Common;

[TestClass]
public sealed class RelogioClinicaTests
{
    [TestMethod]
    public void Converte_utc_para_sao_paulo()
    {
        var relogio = new FakeTimeProvider(new DateTimeOffset(2026, 11, 18, 2, 30, 0, TimeSpan.Zero));
        var clinica = new RelogioClinica(relogio);

        clinica.Agora.Offset.ShouldBe(TimeSpan.FromHours(-3));
        clinica.Hoje.ShouldBe(new DateOnly(2026, 11, 17)); // 23:30 do dia anterior em Brasília
    }
}
