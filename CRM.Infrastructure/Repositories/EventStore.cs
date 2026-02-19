using CRM.Domain.Common;
using CRM.Domain.Entities;
using CRM.Domain.Repositories;
using CRM.Infrastructure.Persistence;
using CRM.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CRM.Infrastructure.Repositories;

public class EventStore : IEventStore
{
    private readonly CrmDbContext _context;

    public EventStore(CrmDbContext context)
    {
        _context = context;
    }

    public async Task SaveEventAsync(IDomainEvent domainEvent, string userId, CancellationToken cancellationToken = default)
    {
        var eventEntry = new EventStoreEntry
        {
            EventId = domainEvent.EventId,
            AggregateId = domainEvent.AggregateId,
            EventType = domainEvent.EventType,
            EventData = JsonSerializer.Serialize(domainEvent),
            UserId = userId,
            OccurredAt = domainEvent.OccurredAt
        };

        await _context.EventStore.AddAsync(eventEntry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        var eventEntries = await _context.EventStore
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.OccurredAt)
            .ToListAsync(cancellationToken);

        return eventEntries.Select(DeserializeEvent).Where(e => e != null).Cast<IDomainEvent>();
    }

    public async Task<IEnumerable<IDomainEvent>> GetAllEventsAsync(CancellationToken cancellationToken = default)
    {
        var eventEntries = await _context.EventStore
            .OrderBy(e => e.OccurredAt)
            .ToListAsync(cancellationToken);

        return eventEntries.Select(DeserializeEvent).Where(e => e != null).Cast<IDomainEvent>();
    }

    private IDomainEvent? DeserializeEvent(EventStoreEntry entry)
    {
        try
        {
            return entry.EventType switch
            {
                nameof(CustomerCreatedEvent) => JsonSerializer.Deserialize<CustomerCreatedEvent>(entry.EventData),
                nameof(CustomerUpdatedEvent) => JsonSerializer.Deserialize<CustomerUpdatedEvent>(entry.EventData),
                nameof(CustomerDeactivatedEvent) => JsonSerializer.Deserialize<CustomerDeactivatedEvent>(entry.EventData),
                nameof(CustomerActivatedEvent) => JsonSerializer.Deserialize<CustomerActivatedEvent>(entry.EventData),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }
}
