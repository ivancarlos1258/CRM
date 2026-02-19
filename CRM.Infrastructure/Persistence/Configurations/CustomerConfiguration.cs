using CRM.Domain.Entities;
using CRM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRM.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.PersonType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.BirthDate)
            .HasColumnType("date");

        builder.Property(c => c.FoundationDate)
            .HasColumnType("date");

        builder.Property(c => c.StateRegistration)
            .HasMaxLength(20);

        builder.Property(c => c.IsStateRegistrationExempt)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt);

        builder.OwnsOne(c => c.Cpf, cpf =>
        {
            cpf.Property(x => x.Value)
                .HasColumnName("Cpf")
                .HasMaxLength(11);

            cpf.HasIndex(x => x.Value)
                .HasDatabaseName("IX_Customers_Cpf")
                .IsUnique();
        });

        builder.OwnsOne(c => c.Cnpj, cnpj =>
        {
            cnpj.Property(x => x.Value)
                .HasColumnName("Cnpj")
                .HasMaxLength(14);

            cnpj.HasIndex(x => x.Value)
                .HasDatabaseName("IX_Customers_Cnpj")
                .IsUnique();
        });

        builder.OwnsOne(c => c.Phone, phone =>
        {
            phone.Property(x => x.Value)
                .HasColumnName("Phone")
                .HasMaxLength(11)
                .IsRequired();
        });

        builder.OwnsOne(c => c.Email, email =>
        {
            email.Property(x => x.Value)
                .HasColumnName("Email")
                .HasMaxLength(200)
                .IsRequired();

            email.HasIndex(x => x.Value)
                .HasDatabaseName("IX_Customers_Email")
                .IsUnique();
        });

        builder.OwnsOne(c => c.Address, address =>
        {
            address.Property(x => x.ZipCode)
                .HasColumnName("ZipCode")
                .HasMaxLength(8)
                .IsRequired();

            address.Property(x => x.Street)
                .HasColumnName("Street")
                .HasMaxLength(200)
                .IsRequired();

            address.Property(x => x.Number)
                .HasColumnName("Number")
                .HasMaxLength(20)
                .IsRequired();

            address.Property(x => x.Complement)
                .HasColumnName("Complement")
                .HasMaxLength(100);

            address.Property(x => x.Neighborhood)
                .HasColumnName("Neighborhood")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(x => x.City)
                .HasColumnName("City")
                .HasMaxLength(100)
                .IsRequired();

            address.Property(x => x.State)
                .HasColumnName("State")
                .HasMaxLength(2)
                .IsRequired();
        });

        builder.Ignore(c => c.DomainEvents);
    }
}
