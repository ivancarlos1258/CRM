using CRM.Application.Common;
using CRM.Application.DTOs;

namespace CRM.Application.Queries.Customers.GetCustomerEvents;

public record GetCustomerEventsQuery(Guid CustomerId) : IQuery<Result<IEnumerable<EventDto>>>;
