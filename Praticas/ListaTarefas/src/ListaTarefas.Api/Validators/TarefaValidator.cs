using FluentValidation;
using ListaTarefas.Api.Entities;

namespace ListaTarefas.Api.Validators;

public class TarefaValidator: AbstractValidator<Tarefa>
{
    public TarefaValidator()
    {
        RuleFor(t => t.Nome).NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(30).WithMessage("O nome deve ter no máximo 30 caracteres.");
        RuleFor(t => t.Descricao).MaximumLength(250).WithMessage("O nome deve ter no máximo 30 caracteres.");
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status inválido.");
    }
}