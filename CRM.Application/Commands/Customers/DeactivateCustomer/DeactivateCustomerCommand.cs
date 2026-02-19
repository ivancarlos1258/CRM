using CRM.Application.Common;
using CRM.Application.DTOs;

namespace CRM.Application.Commands.Customers.DeactivateCustomer;

public record DeactivateCustomerCommand(Guid Id) : ICommand<Result<CustomerDto>>;
