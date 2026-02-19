using CRM.Domain.Entities;
using CRM.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence;

public class CrmDbContext : DbContext
{
    public CrmDbContext(DbContextOptions<CrmDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<EventStoreEntry> EventStore => Set<EventStoreEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new EventStoreConfiguration());

        base.OnModelCreating(modelBuilder);
    }
}
