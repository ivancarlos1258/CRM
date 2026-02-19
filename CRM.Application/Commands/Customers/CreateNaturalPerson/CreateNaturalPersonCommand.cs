using CRM.Application.Common;
using CRM.Application.DTOs;

namespace CRM.Application.Commands.Customers.CreateNaturalPerson;

public record CreateNaturalPersonCommand(
    string Name,
    string Cpf,
    DateTime BirthDate,
    string Phone,
    string Email,
    CreateAddressDto Address
) : ICommand<Result<CustomerDto>>;
