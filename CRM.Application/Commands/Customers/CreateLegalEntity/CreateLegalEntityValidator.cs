using FluentValidation;

namespace CRM.Application.Commands.Customers.CreateLegalEntity;

public class CreateLegalEntityValidator : AbstractValidator<CreateLegalEntityCommand>
{
    public CreateLegalEntityValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Razão Social é obrigatória")
            .MaximumLength(200).WithMessage("Razão Social deve ter no máximo 200 caracteres");

        RuleFor(x => x.Cnpj)
            .NotEmpty().WithMessage("CNPJ é obrigatório");

        RuleFor(x => x.FoundationDate)
            .NotEmpty().WithMessage("Data de fundação é obrigatória")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Data de fundação não pode ser no futuro");

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

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.StateRegistration) || x.IsStateRegistrationExempt)
            .WithMessage("Inscrição Estadual é obrigatória ou deve ser marcada como isenta");
    }
}
