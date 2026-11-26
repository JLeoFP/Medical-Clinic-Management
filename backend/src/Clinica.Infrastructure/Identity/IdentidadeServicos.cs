using System.Security.Claims;
using Clinica.Application.Auth;
using Clinica.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Clinica.Infrastructure.Identity;

/// <summary>Acrescenta o nome de exibição às claims do usuário.</summary>
public sealed class ClinicaClaimsPrincipalFactory(
    UserManager<Usuario> usuarios,
    RoleManager<Perfil> perfis,
    IOptions<IdentityOptions> opcoes)
    : UserClaimsPrincipalFactory<Usuario, Perfil>(usuarios, perfis, opcoes)
{
    /// <summary>Tipo da claim com o nome de exibição.</summary>
    public const string ClaimNome = "nome";

    /// <inheritdoc />
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(Usuario user)
    {
        var identidade = await base.GenerateClaimsAsync(user);
        identidade.AddClaim(new Claim(ClaimNome, user.Nome));
        return identidade;
    }
}

/// <summary>Exige letra e número na senha (DT-18); o tamanho mínimo é configurado em <see cref="IdentityOptions"/>.</summary>
public sealed class SenhaComLetraEDigitoValidator : IPasswordValidator<Usuario>
{
    /// <inheritdoc />
    public Task<IdentityResult> ValidateAsync(UserManager<Usuario> manager, Usuario user, string? password)
    {
        var senha = password ?? string.Empty;
        var erros = new List<IdentityError>();
        if (!senha.Any(char.IsLetter))
        {
            erros.Add(new IdentityError { Code = "SenhaSemLetra", Description = "A senha deve conter ao menos uma letra." });
        }

        if (!senha.Any(char.IsDigit))
        {
            erros.Add(new IdentityError { Code = "SenhaSemNumero", Description = "A senha deve conter ao menos um número." });
        }

        return Task.FromResult(erros.Count == 0 ? IdentityResult.Success : IdentityResult.Failed([.. erros]));
    }
}

/// <summary>Implementa <see cref="IServicoAutenticacao"/> sobre o Identity e o cookie de sessão.</summary>
public sealed class ServicoAutenticacao(
    UserManager<Usuario> usuarios,
    SignInManager<Usuario> entrada,
    IHttpContextAccessor http) : IServicoAutenticacao
{
    /// <inheritdoc />
    public async Task<UsuarioSessaoDto?> EntrarAsync(string email, string senha, CancellationToken ct)
    {
        var usuario = await usuarios.FindByEmailAsync(email);
        if (usuario is null || !usuario.Ativo)
        {
            return null;
        }

        var resultado = await entrada.PasswordSignInAsync(usuario, senha, isPersistent: false, lockoutOnFailure: true);
        return resultado.Succeeded ? await ParaDtoAsync(usuario) : null;
    }

    /// <inheritdoc />
    public async Task SairAsync(CancellationToken ct) => await entrada.SignOutAsync();

    /// <inheritdoc />
    public async Task<UsuarioSessaoDto?> ObterSessaoAsync(CancellationToken ct)
    {
        var principal = http.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var usuario = await usuarios.GetUserAsync(principal);
        return usuario is { Ativo: true } ? await ParaDtoAsync(usuario) : null;
    }

    private async Task<UsuarioSessaoDto> ParaDtoAsync(Usuario u) =>
        new(u.Id, u.Nome, [.. await usuarios.GetRolesAsync(u)], PacienteId: null, MedicoId: null,
            CadastroPendente: false, u.TrocaSenhaObrigatoria);
}

/// <summary>Operações de acesso que invalidam sessões (desativar usuário, trocar senha) — RNF-02, DT-45.</summary>
public sealed class GestaoAcesso(UserManager<Usuario> usuarios, ClinicaDbContext db, TimeProvider relogio)
{
    /// <summary>Desativa o usuário e revoga todas as suas sessões.</summary>
    /// <returns><c>false</c> se o usuário não existe.</returns>
    public async Task<bool> DesativarAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var usuario = await usuarios.FindByIdAsync(usuarioId.ToString());
        if (usuario is null)
        {
            return false;
        }

        usuario.Ativo = false;
        await usuarios.UpdateAsync(usuario);
        await RevogarSessoesAsync(usuarioId, ct);
        return true;
    }

    /// <summary>Troca a senha (validando a política) e revoga todas as sessões.</summary>
    public async Task<IdentityResult> TrocarSenhaAsync(Guid usuarioId, string senhaAtual, string novaSenha, CancellationToken ct = default)
    {
        var usuario = await usuarios.FindByIdAsync(usuarioId.ToString())
            ?? throw new InvalidOperationException("Usuário não encontrado.");
        var resultado = await usuarios.ChangePasswordAsync(usuario, senhaAtual, novaSenha);
        if (resultado.Succeeded)
        {
            usuario.TrocaSenhaObrigatoria = false;
            await usuarios.UpdateAsync(usuario);
            await RevogarSessoesAsync(usuarioId, ct);
        }

        return resultado;
    }

    private Task<int> RevogarSessoesAsync(Guid usuarioId, CancellationToken ct) =>
        db.Sessoes.Where(s => s.UsuarioId == usuarioId && s.RevogadaEm == null)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.RevogadaEm, relogio.GetUtcNow()), ct);
}
