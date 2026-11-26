using Clinica.Application.Common;
using Clinica.Application.Common.Behaviors;
using Clinica.Domain.Auditoria;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Clinica.Application.Tests.Common;

[TestClass]
public sealed class BehaviorsTests
{
    private sealed record Cmd(string Nome) : ICommand<string>;

    private sealed record Consulta : IQuery<string>;

    [AuditarLeitura("Paciente")]
    private sealed record ConsultaAuditada(Guid? PacienteId) : IQuery<string>, IReferenciaPaciente;

    private sealed class CmdValidator : AbstractValidator<Cmd>
    {
        public CmdValidator() => RuleFor(x => x.Nome).NotEmpty().WithMessage("Informe o nome.");
    }

    [TestMethod]
    public async Task Validacao_lanca_excecao_com_erros_por_campo_e_nao_chama_o_handler()
    {
        var comportamento = new ValidationBehavior<Cmd, string>([new CmdValidator()]);
        var chamou = false;

        var ex = await Should.ThrowAsync<ValidationException>(() =>
            comportamento.Handle(new Cmd(""), _ => { chamou = true; return Task.FromResult("ok"); }, default));

        chamou.ShouldBeFalse();
        ex.Errors.Single().PropertyName.ShouldBe("Nome");
    }

    [TestMethod]
    public async Task Validacao_valida_segue_para_o_handler()
    {
        var comportamento = new ValidationBehavior<Cmd, string>([new CmdValidator()]);
        (await comportamento.Handle(new Cmd("Ana"), _ => Task.FromResult("ok"), default)).ShouldBe("ok");
    }

    [TestMethod]
    public async Task Transacao_envolve_commands_mas_nao_queries()
    {
        var transacao = Substitute.For<ITransacao>();
        transacao.ExecutarAsync(Arg.Any<Func<Task<string>>>(), Arg.Any<CancellationToken>())
            .Returns(c => c.Arg<Func<Task<string>>>()());

        await new TransactionBehavior<Cmd, string>(transacao).Handle(new Cmd("a"), _ => Task.FromResult("x"), default);
        await new TransactionBehavior<Consulta, string>(transacao).Handle(new Consulta(), _ => Task.FromResult("x"), default);

        await transacao.Received(1).ExecutarAsync(Arg.Any<Func<Task<string>>>(), Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Logging_repassa_a_resposta_e_propaga_excecao()
    {
        var comportamento = new LoggingBehavior<Consulta, string>(NullLogger<LoggingBehavior<Consulta, string>>.Instance);
        (await comportamento.Handle(new Consulta(), _ => Task.FromResult("r"), default)).ShouldBe("r");
        await Should.ThrowAsync<InvalidOperationException>(() =>
            comportamento.Handle(new Consulta(), _ => throw new InvalidOperationException(), default));
    }

    [TestMethod]
    public async Task Auditoria_de_leitura_grava_linha_para_query_marcada()
    {
        var auditoria = Substitute.For<IAuditoria>();
        var usuario = Substitute.For<IUsuarioAtual>();
        var uid = Guid.CreateVersion7();
        var paciente = Guid.CreateVersion7();
        usuario.Id.Returns(uid);
        usuario.Perfil.Returns("Medico");

        await new AuditoriaLeituraBehavior<ConsultaAuditada, string>(auditoria, usuario)
            .Handle(new ConsultaAuditada(paciente), _ => Task.FromResult("x"), default);

        await auditoria.Received(1).RegistrarAsync(AcaoAuditoria.Leitura, "Paciente", uid, "Medico", null, paciente, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task Auditoria_de_leitura_ignora_query_sem_marca()
    {
        var auditoria = Substitute.For<IAuditoria>();
        await new AuditoriaLeituraBehavior<Consulta, string>(auditoria, Substitute.For<IUsuarioAtual>())
            .Handle(new Consulta(), _ => Task.FromResult("x"), default);

        await auditoria.DidNotReceiveWithAnyArgs().RegistrarAsync(default, default!);
    }
}
