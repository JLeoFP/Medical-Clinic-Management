using Clinica.Application.Common;
using Clinica.Application.Common.Behaviors;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Clinica.Application;

/// <summary>Registro dos serviços da camada de aplicação.</summary>
public static class DependencyInjection
{
    /// <summary>Registra MediatR com o pipeline (Logging → Validation → Transaction → AuditoriaLeitura), validadores e relógio.</summary>
    /// <param name="servicos">Coleção de serviços.</param>
    /// <param name="chaveLicencaMediatR">Chave de licença do MediatR 14 (DT-20); opcional em desenvolvimento.</param>
    public static IServiceCollection AddAplicacao(this IServiceCollection servicos, string? chaveLicencaMediatR = null)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        servicos.AddMediatR(c =>
        {
            c.LicenseKey = chaveLicencaMediatR;
            c.RegisterServicesFromAssembly(assembly);
            c.AddOpenBehavior(typeof(LoggingBehavior<,>));
            c.AddOpenBehavior(typeof(ValidationBehavior<,>));
            c.AddOpenBehavior(typeof(TransactionBehavior<,>));
            c.AddOpenBehavior(typeof(AuditoriaLeituraBehavior<,>));
        });
        servicos.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);
        servicos.AddSingleton(TimeProvider.System);
        servicos.AddSingleton<RelogioClinica>();
        return servicos;
    }
}
