using FluentValidation;
using ListaTarefas.Api.Dtos;

namespace ListaTarefas.Api.Validators;

public class TarefaPatchDtoValidator: AbstractValidator<TarefaPatchDto>
{
    public TarefaPatchDtoValidator()
    {
        RuleFor(t => t.Nome)
            .MaximumLength(30).WithMessage("O nome deve ter no máximo 30 caracteres.");

        RuleFor(t => t.Descricao)
            .MaximumLength(250).WithMessage("A descrição deve ter no máximo 250 caracteres.");

        RuleFor(t => t.Status)
            .IsInEnum().When(x => x.Status.HasValue)
            .WithMessage("Status inválido.");
    }
}