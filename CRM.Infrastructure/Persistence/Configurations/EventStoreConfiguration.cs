using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class EventStoreConfiguration : IEntityTypeConfiguration<EventStoreEntry>
{
    public void Configure(EntityTypeBuilder<EventStoreEntry> builder)
    {
        builder.ToTable("EventStore");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventId)
            .IsRequired();

        builder.Property(e => e.AggregateId)
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.EventData)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(e => e.UserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.OccurredAt)
            .IsRequired();

        builder.HasIndex(e => e.AggregateId)
            .HasDatabaseName("IX_EventStore_AggregateId");

        builder.HasIndex(e => e.EventType)
            .HasDatabaseName("IX_EventStore_EventType");

        builder.HasIndex(e => e.OccurredAt)
            .HasDatabaseName("IX_EventStore_OccurredAt");
    }
}

public class EventStoreEntry
{
    public long Id { get; set; }
    public Guid EventId { get; set; }
    public Guid AggregateId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
