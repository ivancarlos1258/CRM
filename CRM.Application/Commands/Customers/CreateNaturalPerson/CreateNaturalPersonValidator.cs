using FluentValidation;

namespace CRM.Application.Commands.Customers.CreateNaturalPerson;

public class CreateNaturalPersonValidator : AbstractValidator<CreateNaturalPersonCommand>
{
    public CreateNaturalPersonValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Cpf)
            .NotEmpty().WithMessage("CPF é obrigatório");

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Data de nascimento é obrigatória")
            .LessThan(DateTime.Today).WithMessage("Data de nascimento deve ser no passado");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Address)
            .NotNull().WithMessage("Endereço é obrigatório");

        When(x => x.Address != null, () =>
        {
            RuleFor(x => x.Address.ZipCode)
                .NotEmpty().WithMessage("CEP é obrigatório");

            RuleFor(x => x.Address.Street)
                .NotEmpty().WithMessage("Logradouro é obrigatório");

            RuleFor(x => x.Address.Number)
                .NotEmpty().WithMessage("Número é obrigatório");

            RuleFor(x => x.Address.Neighborhood)
                .NotEmpty().WithMessage("Bairro é obrigatório");

            RuleFor(x => x.Address.City)
                .NotEmpty().WithMessage("Cidade é obrigatória");

            RuleFor(x => x.Address.State)
                .NotEmpty().WithMessage("Estado é obrigatório")
                .Length(2).WithMessage("Estado deve ter 2 caracteres");
        });
    }
}
