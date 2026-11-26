using Clinica.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Clinica.Infrastructure.Seed;

/// <summary>
/// Cria o administrador inicial a partir de <c>ADMIN_EMAIL</c> e <c>ADMIN_SENHA_INICIAL</c>, sem duplicar (DT-20, TDD §9.3).
/// </summary>
public sealed partial class SeedAdministrador(UserManager<Usuario> usuarios, ILogger<SeedAdministrador> log)
{
    /// <summary>Executa a semeadura; seguro para repetir a cada inicialização.</summary>
    public async Task ExecutarAsync(string? email, string? senha)
    {
        if ((await usuarios.GetUsersInRoleAsync(Perfil.Administrador)).Count > 0)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
        {
            AvisoSemVariaveis(log);
            return;
        }

        var admin = new Usuario
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Nome = "Administrador",
            TrocaSenhaObrigatoria = true,
        };
        var criado = await usuarios.CreateAsync(admin, senha);
        if (criado.Succeeded)
        {
            await usuarios.AddToRoleAsync(admin, Perfil.Administrador);
            AdminCriado(log);
        }
        else
        {
            // Descrições dos erros não contêm a senha nem o e-mail.
            ErroAoCriar(log, string.Join("; ", criado.Errors.Select(e => e.Code)));
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "ADMIN_EMAIL/ADMIN_SENHA_INICIAL não definidos e não há administrador: nenhum usuário criado.")]
    private static partial void AvisoSemVariaveis(ILogger log);

    [LoggerMessage(Level = LogLevel.Information, Message = "Administrador inicial criado.")]
    private static partial void AdminCriado(ILogger log);

    [LoggerMessage(Level = LogLevel.Error, Message = "Falha ao criar o administrador inicial: {Codigos}")]
    private static partial void ErroAoCriar(ILogger log, string codigos);
}
