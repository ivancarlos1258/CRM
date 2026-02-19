using CRM.Domain.Common;
using CRM.Domain.Enums;

namespace CRM.Domain.Entities;

public class CustomerCreatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public Guid AggregateId { get; }
    public string EventType => nameof(CustomerCreatedEvent);
    public string CustomerName { get; }
    public PersonType PersonType { get; }

    public CustomerCreatedEvent(Guid aggregateId, string customerName, PersonType personType)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        AggregateId = aggregateId;
        CustomerName = customerName;
        PersonType = personType;
    }
}

public class CustomerUpdatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public Guid AggregateId { get; }
    public string EventType => nameof(CustomerUpdatedEvent);
    public string CustomerName { get; }
    public PersonType PersonType { get; }
    public string OldData { get; }
    public string NewData { get; }

    public CustomerUpdatedEvent(
        Guid aggregateId,
        string customerName,
        PersonType personType,
        string oldData,
        string newData)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        AggregateId = aggregateId;
        CustomerName = customerName;
        PersonType = personType;
        OldData = oldData;
        NewData = newData;
    }
}

public class CustomerDeactivatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public Guid AggregateId { get; }
    public string EventType => nameof(CustomerDeactivatedEvent);
    public string CustomerName { get; }
    public PersonType PersonType { get; }

    public CustomerDeactivatedEvent(Guid aggregateId, string customerName, PersonType personType)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        AggregateId = aggregateId;
        CustomerName = customerName;
        PersonType = personType;
    }
}

public class CustomerActivatedEvent : IDomainEvent
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public Guid AggregateId { get; }
    public string EventType => nameof(CustomerActivatedEvent);
    public string CustomerName { get; }
    public PersonType PersonType { get; }

    public CustomerActivatedEvent(Guid aggregateId, string customerName, PersonType personType)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        AggregateId = aggregateId;
        CustomerName = customerName;
        PersonType = personType;
    }
}
