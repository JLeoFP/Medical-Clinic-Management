using Clinica.Application.Auth;
using Clinica.Application.Common;
using Clinica.Infrastructure.Auditoria;
using Clinica.Infrastructure.Identity;
using Clinica.Infrastructure.Jobs;
using Clinica.Infrastructure.Persistence;
using Clinica.Infrastructure.Seed;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Clinica.Infrastructure;

/// <summary>Registro dos serviços da infraestrutura.</summary>
public static class DependencyInjection
{
    /// <summary>Nome do cookie de sessão (prefixo <c>__Host-</c> exige Secure, Path=/ e sem Domain).</summary>
    public const string NomeCookieSessao = "__Host-clinica-sessao";

    /// <summary>Registra persistência (conexão <c>ConnectionStrings:Clinica</c>, papel restrito da aplicação), Identity com sessão no servidor, auditoria e jobs.</summary>
    /// <param name="servicos">Coleção de serviços.</param>
    public static IServiceCollection AddInfraestrutura(this IServiceCollection servicos)
    {
        servicos.AddScoped<AuditoriaInterceptor>();
        servicos.AddDbContextFactory<ClinicaDbContext>(
            (sp, o) => o.UseNpgsql(sp.GetRequiredService<IConfiguration>().GetConnectionString("Clinica")).AddInterceptors(sp.GetRequiredService<AuditoriaInterceptor>()),
            ServiceLifetime.Scoped);
        servicos.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<ClinicaDbContext>>().CreateDbContext());

        servicos.AddIdentity<Usuario, Perfil>(o =>
            {
                o.Password.RequiredLength = 8;
                o.Password.RequireDigit = false; // validado em SenhaComLetraEDigitoValidator (letra e número)
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
                o.Lockout.MaxFailedAccessAttempts = 5;
                o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                o.Lockout.AllowedForNewUsers = true;
                o.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ClinicaDbContext>()
            .AddClaimsPrincipalFactory<ClinicaClaimsPrincipalFactory>()
            .AddPasswordValidator<SenhaComLetraEDigitoValidator>();

        servicos.AddDataProtection().SetApplicationName("Clinica").PersistKeysToDbContext<ClinicaDbContext>();

        servicos.AddSingleton<SessaoTicketStore>();
        servicos.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
            .Configure<SessaoTicketStore, TimeProvider>((o, store, relogio) =>
            {
                o.SessionStore = store;
                o.TimeProvider = relogio;
                o.Cookie.Name = NomeCookieSessao;
                o.Cookie.HttpOnly = true;
                o.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
                o.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                o.Cookie.Path = "/";
                o.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                o.SlidingExpiration = true;
                // API: 401/403 em vez de redirecionar para páginas HTML.
                o.Events.OnRedirectToLogin = c => { c.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; };
                o.Events.OnRedirectToAccessDenied = c => { c.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; };
            });

        servicos.AddHttpContextAccessor();
        servicos.AddScoped<IServicoAutenticacao, ServicoAutenticacao>();
        servicos.AddScoped<GestaoAcesso>();
        servicos.AddScoped<IAuditoria, AuditoriaServico>();
        servicos.AddScoped<ITransacao, TransacaoEf>();
        servicos.AddScoped<SeedAdministrador>();
        servicos.AddSingleton<JobsEmSegundoPlano>();
        servicos.AddHostedService(sp => sp.GetRequiredService<JobsEmSegundoPlano>());
        return servicos;
    }
}
