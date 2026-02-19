using CRM.Application.Common;
using CRM.Application.DTOs;

namespace CRM.Application.Commands.Customers.UpdateCustomer;

public record UpdateCustomerCommand(
    Guid Id,
    string Name,
    string Phone,
    string Email,
    CreateAddressDto Address,
    string? StateRegistration,
    bool? IsStateRegistrationExempt
) : ICommand<Result<CustomerDto>>;
