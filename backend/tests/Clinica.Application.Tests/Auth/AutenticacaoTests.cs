using Clinica.Application.Auth;
using Clinica.Application.Common;
using Clinica.Domain.Auditoria;
using NSubstitute;

namespace Clinica.Application.Tests.Auth;

[TestClass]
public sealed class AutenticacaoTests
{
    private static readonly UsuarioSessaoDto Admin = new(Guid.CreateVersion7(), "Admin", ["Administrador"], null, null, false, true);

    [TestMethod]
    public async Task Login_valido_audita_login_e_devolve_usuario()
    {
        var servico = Substitute.For<IServicoAutenticacao>();
        var auditoria = Substitute.For<IAuditoria>();
        servico.EntrarAsync("a@b.com", "x", Arg.Any<CancellationToken>()).Returns(Admin);

        var r = await new LoginHandler(servico, auditoria).Handle(new LoginCommand("a@b.com", "x"), default);

        r.ShouldBe(Admin);
        await auditoria.Received(1).RegistrarAsync(AcaoAuditoria.Login, "Usuario", Admin.Id, "Administrador", Admin.Id, null, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Login_invalido_audita_falha_sem_senha_e_lanca_401()
    {
        var servico = Substitute.For<IServicoAutenticacao>();
        var auditoria = Substitute.For<IAuditoria>();

        var ex = await Should.ThrowAsync<NaoAutenticadoException>(() =>
            new LoginHandler(servico, auditoria).Handle(new LoginCommand("a@b.com", "errada"), default));

        ex.Codigo.ShouldBe("auth.credenciais_invalidas");
        await auditoria.Received(1).RegistrarAsync(AcaoAuditoria.LoginFalho, "Usuario", null, null, null, null, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public void Validador_rejeita_email_e_senha_vazios()
    {
        var r = new LoginCommandValidator().Validate(new LoginCommand("nao-e-email", ""));
        r.Errors.Select(e => e.PropertyName).ShouldBe(["Email", "Senha"], ignoreOrder: true);
    }
}
