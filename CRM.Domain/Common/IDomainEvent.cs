namespace CRM.Domain.Common;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
    Guid AggregateId { get; }
    string EventType { get; }
}
