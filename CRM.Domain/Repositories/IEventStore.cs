using CRM.Domain.Common;

namespace CRM.Domain.Repositories;

public interface IEventStore
{
    Task SaveEventAsync(IDomainEvent domainEvent, string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<IDomainEvent>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default);
    Task<IEnumerable<IDomainEvent>> GetAllEventsAsync(CancellationToken cancellationToken = default);
}
