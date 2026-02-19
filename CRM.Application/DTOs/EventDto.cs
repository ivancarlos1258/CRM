namespace CRM.Application.DTOs;

public record EventDto(
    Guid EventId,
    Guid AggregateId,
    string EventType,
    string EventData,
    string UserId,
    DateTime OccurredAt
);
