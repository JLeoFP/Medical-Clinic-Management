using Clinica.Application.Common;
using Clinica.Domain.Auditoria;
using FluentValidation;
using MediatR;

namespace Clinica.Application.Auth;

/// <summary>Dados do usuário da sessão (API-01 e API-02).</summary>
/// <param name="Id">Identificador do usuário.</param>
/// <param name="Nome">Nome de exibição.</param>
/// <param name="Perfis">Perfis do usuário.</param>
/// <param name="PacienteId">Paciente vinculado, quando o perfil é Paciente.</param>
/// <param name="MedicoId">Médico vinculado, quando o perfil é Médico.</param>
/// <param name="CadastroPendente">Conta de paciente ainda sem cadastro clínico (DT-44).</param>
/// <param name="TrocaSenhaObrigatoria">Exige troca de senha no primeiro acesso.</param>
public sealed record UsuarioSessaoDto(
    Guid Id,
    string Nome,
    IReadOnlyList<string> Perfis,
    Guid? PacienteId,
    Guid? MedicoId,
    bool CadastroPendente,
    bool TrocaSenhaObrigatoria);

/// <summary>Porta de autenticação implementada na infraestrutura (Identity + cookie).</summary>
public interface IServicoAutenticacao
{
    /// <summary>Valida as credenciais, cria a sessão e define o cookie; nulo se inválidas, bloqueadas ou usuário inativo.</summary>
    Task<UsuarioSessaoDto?> EntrarAsync(string email, string senha, CancellationToken ct);

    /// <summary>Revoga a sessão atual e remove o cookie.</summary>
    Task SairAsync(CancellationToken ct);

    /// <summary>Usuário da sessão corrente; nulo se não houver sessão válida.</summary>
    Task<UsuarioSessaoDto?> ObterSessaoAsync(CancellationToken ct);
}

/// <summary>API-01: login interno.</summary>
/// <param name="Email">E-mail do usuário.</param>
/// <param name="Senha">Senha em texto, nunca registrada em log.</param>
public sealed record LoginCommand(string Email, string Senha) : IRequest<UsuarioSessaoDto>;

/// <summary>Valida o formato do <see cref="LoginCommand"/>.</summary>
public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>Cria o validador.</summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(256).EmailAddress();
        RuleFor(x => x.Senha).NotEmpty().MaximumLength(128);
    }
}

/// <summary>Executa o login e audita sucesso e falha (DT-19); não usa a transação do pipeline para que a falha seja gravada.</summary>
public sealed class LoginHandler(IServicoAutenticacao autenticacao, IAuditoria auditoria)
    : IRequestHandler<LoginCommand, UsuarioSessaoDto>
{
    /// <inheritdoc />
    public async Task<UsuarioSessaoDto> Handle(LoginCommand request, CancellationToken ct)
    {
        var usuario = await autenticacao.EntrarAsync(request.Email, request.Senha, ct);
        if (usuario is null)
        {
            await auditoria.RegistrarAsync(AcaoAuditoria.LoginFalho, "Usuario", ct: ct);
            throw new NaoAutenticadoException("auth.credenciais_invalidas", "E-mail ou senha incorretos.");
        }

        await auditoria.RegistrarAsync(AcaoAuditoria.Login, "Usuario", usuario.Id, usuario.Perfis.Count > 0 ? usuario.Perfis[0] : null, usuario.Id, ct: ct);
        return usuario;
    }
}

/// <summary>API-02: restaura a sessão.</summary>
public sealed record ObterSessaoQuery : IQuery<UsuarioSessaoDto?>;

/// <summary>Resolve a sessão corrente.</summary>
public sealed class ObterSessaoHandler(IServicoAutenticacao autenticacao) : IRequestHandler<ObterSessaoQuery, UsuarioSessaoDto?>
{
    /// <inheritdoc />
    public Task<UsuarioSessaoDto?> Handle(ObterSessaoQuery request, CancellationToken ct) => autenticacao.ObterSessaoAsync(ct);
}

/// <summary>API-03: logout.</summary>
public sealed record LogoutCommand : IRequest;

/// <summary>Revoga a sessão atual.</summary>
public sealed class LogoutHandler(IServicoAutenticacao autenticacao) : IRequestHandler<LogoutCommand>
{
    /// <inheritdoc />
    public Task Handle(LogoutCommand request, CancellationToken ct) => autenticacao.SairAsync(ct);
}
