using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Domain.ValueObjects;
using FluentAssertions;

namespace CRM.Tests.Domain;

public class CustomerTests
{
    [Fact]
    public void CreateNaturalPerson_WithValidData_ShouldSucceed()
    {
        var birthDate = DateTime.Now.AddYears(-25);
        var address = Address.Create("01310100", "Av. Paulista", "1578", null, "Bela Vista", "São Paulo", "SP");

        var customer = Customer.CreateNaturalPerson(
            "João Silva",
            "12345678909",
            birthDate,
            "11987654321",
            "joao@example.com",
            address);

        customer.Should().NotBeNull();
        customer.PersonType.Should().Be(PersonType.NaturalPerson);
        customer.Name.Should().Be("João Silva");
        customer.Cpf.Should().NotBeNull();
        customer.Cpf!.Value.Should().Be("12345678909");
        customer.BirthDate.Should().Be(birthDate);
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateNaturalPerson_WithUnderage_ShouldThrowException()
    {
        var birthDate = DateTime.Now.AddYears(-17);
        var address = Address.Create("01310100", "Av. Paulista", "1578", null, "Bela Vista", "São Paulo", "SP");

        var action = () => Customer.CreateNaturalPerson(
            "João Silva",
            "12345678909",
            birthDate,
            "11987654321",
            "joao@example.com",
            address);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Cliente deve ter no mínimo 18 anos");
    }

    [Fact]
    public void CreateLegalEntity_WithValidData_ShouldSucceed()
    {
        var foundationDate = DateTime.Now.AddYears(-10);
        var address = Address.Create("01310100", "Av. Paulista", "1578", null, "Bela Vista", "São Paulo", "SP");

        var customer = Customer.CreateLegalEntity(
            "Empresa XYZ Ltda",
            "12345678000195",
            foundationDate,
            "1140041000",
            "contato@empresa.com",
            address,
            "123456789",
            false);

        customer.Should().NotBeNull();
        customer.PersonType.Should().Be(PersonType.LegalEntity);
        customer.Name.Should().Be("Empresa XYZ Ltda");
        customer.Cnpj.Should().NotBeNull();
        customer.Cnpj!.Value.Should().Be("12345678000195");
        customer.StateRegistration.Should().Be("123456789");
        customer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateLegalEntity_WithoutStateRegistrationAndNotExempt_ShouldThrowException()
    {
        var foundationDate = DateTime.Now.AddYears(-10);
        var address = Address.Create("01310100", "Av. Paulista", "1578", null, "Bela Vista", "São Paulo", "SP");

        var action = () => Customer.CreateLegalEntity(
            "Empresa XYZ Ltda",
            "12345678000195",
            foundationDate,
            "1140041000",
            "contato@empresa.com",
            address,
            null,
            false);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Inscrição Estadual é obrigatória ou deve ser marcada como isenta");
    }

    [Fact]
    public void CreateLegalEntity_WithExemptStateRegistration_ShouldSucceed()
    {
        var foundationDate = DateTime.Now.AddYears(-10);
        var address = Address.Create("01310100", "Av. Paulista", "1578", null, "Bela Vista", "São Paulo", "SP");

        var customer = Customer.CreateLegalEntity(
            "Empresa XYZ Ltda",
            "12345678000195",
            foundationDate,
            "1140041000",
            "contato@empresa.com",
            address,
            null,
            true);

        customer.Should().NotBeNull();
        customer.IsStateRegistrationExempt.Should().BeTrue();
        customer.StateRegistration.Should().BeNull();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateCustomer()
    {
        var birthDate = DateTime.Now.AddYears(-25);
        var address = Address.Create("01310100", "Av. Paulista", "1578", null, "Bela Vista", "São Paulo", "SP");

        var customer = Customer.CreateNaturalPerson(
            "João Silva",
            "12345678909",
            birthDate,
            "11987654321",
            "joao@example.com",
            address);

        var newAddress = Address.Create("04567890", "Rua Nova", "100", null, "Centro", "São Paulo", "SP");

        customer.Update(
            "João Silva Santos",
            "11987654322",
            "joao.novo@example.com",
            newAddress);

        customer.Name.Should().Be("João Silva Santos");
        customer.Phone.Value.Should().Be("11987654322");
        customer.Email.Value.Should().Be("joao.novo@example.com");
        customer.Address.Should().Be(newAddress);
        customer.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        var birthDate = DateTime.Now.AddYears(-25);
        var address = Address.Create("01310100", "Av. Paulista", "1578", null, "Bela Vista", "São Paulo", "SP");

        var customer = Customer.CreateNaturalPerson(
            "João Silva",
            "12345678909",
            birthDate,
            "11987654321",
            "joao@example.com",
            address);

        customer.Deactivate();

        customer.IsActive.Should().BeFalse();
        customer.UpdatedAt.Should().NotBeNull();
    }
}
