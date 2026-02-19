using CRM.Application.Common;
using CRM.Application.DTOs;

namespace CRM.Application.Commands.Customers.ActivateCustomer;

public record ActivateCustomerCommand(Guid Id) : ICommand<Result<CustomerDto>>;
