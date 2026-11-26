using Clinica.Api;
using Clinica.Api.Endpoints;
using Clinica.Application;
using Clinica.Application.Common;
using Clinica.Infrastructure;
using Clinica.Infrastructure.Persistence;
using Clinica.Infrastructure.Seed;
using Serilog;
using Serilog.Formatting.Compact;

var builder = WebApplication.CreateBuilder(args);

// Logs estruturados em JSON; o corpo das requisições nunca é registrado (DT-28, RNF-01).
builder.Host.UseSerilog((contexto, servicos, configuracao) =>
{
    configuracao
        .ReadFrom.Configuration(contexto.Configuration)
        .ReadFrom.Services(servicos)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(new CompactJsonFormatter());
    var seq = contexto.Configuration["Seq:ServerUrl"];
    if (contexto.HostingEnvironment.IsDevelopment() && !string.IsNullOrWhiteSpace(seq))
    {
        configuracao.WriteTo.Seq(seq);
    }
});

builder.Services.AddAplicacao(builder.Configuration["MediatR:LicenseKey"]);
builder.Services.AddInfraestrutura();
builder.Services.AddApi();

var app = builder.Build();

// Somente desenvolvimento local em http://localhost: os cookies __Host- exigem Secure e o antiforgery recusa requisição
// não-HTTPS; os navegadores aceitam cookies Secure em localhost. Em produção o esquema vem do proxy (Fase 5).
if (app.Configuration.GetValue<bool>("Seguranca:TratarComoHttps"))
{
    app.Use((contexto, proximo) =>
    {
        contexto.Request.Scheme = "https";
        return proximo(contexto);
    });
}

app.UseMiddleware<CorrelacaoMiddleware>();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSerilogRequestLogging(o => o.EnrichDiagnosticContext = (diagnostico, http) =>
{
    var usuario = http.RequestServices.GetRequiredService<IUsuarioAtual>();
    diagnostico.Set("CorrelationId", usuario.CorrelationId);
    diagnostico.Set("UserId", usuario.Id);
    diagnostico.Set("Perfil", usuario.Perfil);
});

// Documentação somente em Development (DT-27).
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(o => o.SwaggerEndpoint("/openapi/v1.json", "Clínica v1"));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = r => r.Tags.Contains("ready") });

var api = app.MapGroup("/api/v1").AddEndpointFilter<ValidacaoAntiforgeryFilter>();
api.MapAuth();

await SemearAdministradorAsync(app);
await app.RunAsync();

// A API nunca aplica migrations (TDD §9.3); se o esquema ainda não existe, apenas avisa.
static async Task SemearAdministradorAsync(WebApplication app)
{
    await using var escopo = app.Services.CreateAsyncScope();
    try
    {
        await escopo.ServiceProvider.GetRequiredService<SeedAdministrador>()
            .ExecutarAsync(app.Configuration["ADMIN_EMAIL"], app.Configuration["ADMIN_SENHA_INICIAL"]);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Não foi possível semear o administrador (esquema ainda não migrado?).");
    }
}

/// <summary>Ponto de entrada exposto aos testes de integração.</summary>
public partial class Program;
