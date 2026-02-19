using CRM.Application.Common;
using CRM.Application.DTOs;

namespace CRM.Application.Commands.Customers.CreateLegalEntity;

public record CreateLegalEntityCommand(
    string Name,
    string Cnpj,
    DateTime FoundationDate,
    string Phone,
    string Email,
    CreateAddressDto Address,
    string? StateRegistration,
    bool IsStateRegistrationExempt
) : ICommand<Result<CustomerDto>>;
