using Clinica.Application.Auth;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;

namespace Clinica.Api.Endpoints;

/// <summary>Corpo do login (API-01).</summary>
/// <param name="Email">E-mail do usuário.</param>
/// <param name="Senha">Senha.</param>
public sealed record LoginRequisicao(string Email, string Senha);

/// <summary>API-01 a API-03: login, restauração de sessão e logout.</summary>
public static class AuthEndpoints
{
    /// <summary>Mapeia <c>/auth</c> no grupo da API.</summary>
    public static RouteGroupBuilder MapAuth(this RouteGroupBuilder api)
    {
        var grupo = api.MapGroup("/auth").WithTags("Autenticação");

        grupo.MapPost("/login", async (LoginRequisicao corpo, ISender mediador, IAntiforgery af, HttpContext http, CancellationToken ct) =>
        {
            var usuario = await mediador.Send(new LoginCommand(corpo.Email, corpo.Senha), ct);
            // O token é vinculado ao usuário: reemite após a sessão ser criada (HttpContext.User já é o novo usuário).
            ValidacaoAntiforgeryFilter.EmitirToken(http, af);
            return Results.Ok(usuario);
        }).AllowAnonymous();

        grupo.MapGet("/sessao", async (ISender mediador, IAntiforgery af, HttpContext http, CancellationToken ct) =>
        {
            var usuario = await mediador.Send(new ObterSessaoQuery(), ct);
            ValidacaoAntiforgeryFilter.EmitirToken(http, af);
            return usuario is null ? Results.NoContent() : Results.Ok(usuario);
        }).AllowAnonymous();

        grupo.MapPost("/logout", async (ISender mediador, IAntiforgery af, HttpContext http, CancellationToken ct) =>
        {
            await mediador.Send(new LogoutCommand(), ct);
            // Após o logout, HttpContext.User ainda é o usuário anterior; gera token anônimo na próxima chamada a /sessao.
            http.Response.Cookies.Delete(ValidacaoAntiforgeryFilter.NomeCookieToken, new CookieOptions { Secure = true, Path = "/" });
            return Results.NoContent();
        }).RequireAuthorization();

        return api;
    }
}
