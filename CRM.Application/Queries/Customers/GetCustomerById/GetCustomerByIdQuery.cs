using CRM.Application.Common;
using CRM.Application.DTOs;

namespace CRM.Application.Queries.Customers.GetCustomerById;

public record GetCustomerByIdQuery(Guid Id) : IQuery<Result<CustomerDto>>;
