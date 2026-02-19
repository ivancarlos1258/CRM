using CRM.Application.Common;
using CRM.Application.DTOs;

namespace CRM.Application.Queries.Customers.GetAllCustomers;

public record GetAllCustomersQuery(bool OnlyActive = false) : IQuery<Result<IEnumerable<CustomerDto>>>;
