using CRM.Application.Common;
using CRM.Application.DTOs;
using CRM.Domain.Repositories;
using System.Text.Json;

namespace CRM.Application.Queries.Customers.GetCustomerEvents;

public class GetCustomerEventsHandler : IQueryHandler<GetCustomerEventsQuery, Result<IEnumerable<EventDto>>>
{
    private readonly IEventStore _eventStore;

    public GetCustomerEventsHandler(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public async Task<Result<IEnumerable<EventDto>>> Handle(GetCustomerEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _eventStore.GetEventsAsync(request.CustomerId, cancellationToken);

        var eventDtos = events.Select(e => new EventDto(
            e.EventId,
            e.AggregateId,
            e.EventType,
            JsonSerializer.Serialize(e),
            "system",
            e.OccurredAt));

        return Result<IEnumerable<EventDto>>.Success(eventDtos);
    }
}
