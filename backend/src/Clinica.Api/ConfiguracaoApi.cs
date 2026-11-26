using System.Security.Claims;
using Clinica.Application.Auth;
using Clinica.Application.Common;
using Clinica.Domain.Shared;
using Clinica.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Clinica.Api;

/// <summary>Registro dos serviços específicos da API.</summary>
public static class ConfiguracaoApi
{
    /// <summary>Perfis e policies de autorização (TDD §10.1).</summary>
    public static class Policies
    {
        /// <summary>Administrador, Médico ou Recepção.</summary>
        public const string Interno = "Interno";

        /// <summary>Apenas Médico.</summary>
        public const string ConteudoClinico = "ConteudoClinico";
    }

    /// <summary>Registra ProblemDetails, anti-CSRF, autorização, OpenAPI e health checks.</summary>
    public static IServiceCollection AddApi(this IServiceCollection servicos)
    {
        servicos.AddProblemDetails(o => o.CustomizeProblemDetails = c =>
        {
            c.ProblemDetails.Extensions["traceId"] = c.HttpContext.TraceIdentifier;
            c.ProblemDetails.Extensions.TryAdd("code", $"http.{c.ProblemDetails.Status}");
        });
        servicos.AddExceptionHandler<TratadorDeExcecoes>();

        servicos.AddAntiforgery(o =>
        {
            o.HeaderName = ValidacaoAntiforgeryFilter.NomeCabecalho;
            o.Cookie.Name = "__Host-clinica-antiforgery";
            o.Cookie.HttpOnly = true;
            o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            o.Cookie.SameSite = SameSiteMode.Strict;
            o.Cookie.Path = "/";
        });

        servicos.AddAuthorizationBuilder()
            .AddPolicy("Administrador", p => p.RequireRole("Administrador"))
            .AddPolicy("Medico", p => p.RequireRole("Medico"))
            .AddPolicy("Recepcao", p => p.RequireRole("Recepcao"))
            .AddPolicy("Paciente", p => p.RequireRole("Paciente"))
            .AddPolicy(Policies.Interno, p => p.RequireRole("Administrador", "Medico", "Recepcao"))
            .AddPolicy(Policies.ConteudoClinico, p => p.RequireRole("Medico"));

        servicos.AddScoped<IUsuarioAtual, UsuarioAtualHttp>();
        servicos.AddOpenApi();
        servicos.AddHealthChecks().AddDbContextCheck<ClinicaDbContext>("postgres", tags: ["ready"]);
        return servicos;
    }
}

/// <summary>Lê o usuário, o IP e a correlação da requisição corrente.</summary>
public sealed class UsuarioAtualHttp(IHttpContextAccessor http) : IUsuarioAtual
{
    private HttpContext? Contexto => http.HttpContext;

    /// <inheritdoc />
    public Guid? Id => Guid.TryParse(Contexto?.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <inheritdoc />
    public string? Perfil => Contexto?.User.FindFirstValue(ClaimTypes.Role);

    /// <inheritdoc />
    public string? Ip
    {
        get
        {
            var ip = Contexto?.Connection.RemoteIpAddress;
            // Remove o escopo de IPv6 e o mapeamento IPv4, para caber na coluna inet.
            return ip is null ? null : (ip.IsIPv4MappedToIPv6 ? ip.MapToIPv4() : ip).ToString().Split('%')[0];
        }
    }

    /// <inheritdoc />
    public string? CorrelationId => Contexto?.Items[CorrelacaoMiddleware.ChaveItem] as string ?? Contexto?.TraceIdentifier;
}

/// <summary>Propaga <c>X-Correlation-Id</c> (ou gera um novo) para logs, auditoria e resposta.</summary>
public sealed class CorrelacaoMiddleware(RequestDelegate proximo)
{
    /// <summary>Cabeçalho de correlação.</summary>
    public const string Cabecalho = "X-Correlation-Id";

    /// <summary>Chave em <c>HttpContext.Items</c>.</summary>
    public const string ChaveItem = "CorrelationId";

    /// <summary>Executa o middleware.</summary>
    public async Task InvokeAsync(HttpContext contexto)
    {
        var recebido = contexto.Request.Headers[Cabecalho].ToString();
        // Limita o tamanho e descarta valores vazios para não poluir logs nem estourar a coluna (varchar 64).
        var id = string.IsNullOrWhiteSpace(recebido) || recebido.Length > 64 ? Guid.CreateVersion7().ToString("N") : recebido;
        contexto.Items[ChaveItem] = id;
        contexto.Response.Headers[Cabecalho] = id;
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", id))
        {
            await proximo(contexto);
        }
    }
}

/// <summary>Traduz exceções em ProblemDetails com <c>code</c> e <c>traceId</c> (DT-39).</summary>
public sealed class TratadorDeExcecoes(IProblemDetailsService problemas, ILogger<TratadorDeExcecoes> log) : IExceptionHandler
{
    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext contexto, Exception excecao, CancellationToken ct)
    {
        var (status, titulo, codigo) = excecao switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Dados inválidos", "validacao.invalida"),
            DomainException d => (StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada", d.Codigo),
            NaoAutenticadoException n => (StatusCodes.Status401Unauthorized, "Não autenticado", n.Codigo),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno", "erro.interno"),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            log.LogError(excecao, "Exceção não tratada em {Path}", contexto.Request.Path.Value);
        }

        var detalhe = status == StatusCodes.Status500InternalServerError ? "Ocorreu um erro inesperado." : excecao.Message;
        var problema = new ProblemDetails { Status = status, Title = titulo, Detail = detalhe };
        problema.Extensions["code"] = codigo;
        if (excecao is ValidationException v)
        {
            problema.Detail = "Um ou mais campos são inválidos.";
            problema.Extensions["errors"] = v.Errors
                .GroupBy(e => char.ToLowerInvariant(e.PropertyName[0]) + e.PropertyName[1..])
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        contexto.Response.StatusCode = status;
        return await problemas.TryWriteAsync(new ProblemDetailsContext { HttpContext = contexto, ProblemDetails = problema });
    }
}

/// <summary>
/// Exige <c>X-XSRF-TOKEN</c> válido em POST, PUT, PATCH e DELETE sob <c>/api/v1</c>, inclusive o login (DT-45).
/// </summary>
public sealed class ValidacaoAntiforgeryFilter(IAntiforgery antiforgery) : IEndpointFilter
{
    /// <summary>Cabeçalho que leva o token de requisição.</summary>
    public const string NomeCabecalho = "X-XSRF-TOKEN";

    /// <summary>Cookie legível pela SPA com o token de requisição.</summary>
    public const string NomeCookieToken = "__Host-XSRF-TOKEN";

    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext contexto, EndpointFilterDelegate proximo)
    {
        var http = contexto.HttpContext;
        if (HttpMethods.IsPost(http.Request.Method) || HttpMethods.IsPut(http.Request.Method)
            || HttpMethods.IsPatch(http.Request.Method) || HttpMethods.IsDelete(http.Request.Method))
        {
            try
            {
                await antiforgery.ValidateRequestAsync(http);
            }
            catch (AntiforgeryValidationException)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Token anti-CSRF ausente ou inválido",
                    detail: "Envie o cabeçalho X-XSRF-TOKEN com o valor do cookie __Host-XSRF-TOKEN.",
                    extensions: new Dictionary<string, object?> { ["code"] = "seguranca.antiforgery_invalido" });
            }
        }

        return await proximo(contexto);
    }

    /// <summary>Emite o cookie legível com um token novo, vinculado ao usuário corrente.</summary>
    public static void EmitirToken(HttpContext http, IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(http);
        http.Response.Cookies.Append(NomeCookieToken, tokens.RequestToken!, new CookieOptions
        {
            HttpOnly = false, // a SPA precisa ler para devolver no cabeçalho
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            IsEssential = true,
        });
    }
}
