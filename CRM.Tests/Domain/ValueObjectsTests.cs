using CRM.Domain.ValueObjects;
using FluentAssertions;

namespace CRM.Tests.Domain;

public class ValueObjectsTests
{
    [Theory]
    [InlineData("123.456.789-09", "12345678909")]
    [InlineData("12345678909", "12345678909")]
    public void Cpf_WithValidDocument_ShouldCreate(string input, string expected)
    {
        var cpf = Cpf.Create(input);

        cpf.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("123.456.789-00")]
    [InlineData("12345")]
    public void Cpf_WithInvalidDocument_ShouldThrowException(string invalid)
    {
        var action = () => Cpf.Create(invalid);

        action.Should().Throw<ArgumentException>()
            .WithMessage("CPF inválido");
    }

    [Theory]
    [InlineData("12.345.678/0001-95", "12345678000195")]
    [InlineData("12345678000195", "12345678000195")]
    public void Cnpj_WithValidDocument_ShouldCreate(string input, string expected)
    {
        var cnpj = Cnpj.Create(input);

        cnpj.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData("11.111.111/1111-11")]
    [InlineData("12.345.678/0001-00")]
    [InlineData("12345")]
    public void Cnpj_WithInvalidDocument_ShouldThrowException(string invalid)
    {
        var action = () => Cnpj.Create(invalid);

        action.Should().Throw<ArgumentException>()
            .WithMessage("CNPJ inválido");
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@example.co.uk")]
    public void Email_WithValidEmail_ShouldCreate(string email)
    {
        var emailObj = Email.Create(email);

        emailObj.Value.Should().Be(email.ToLowerInvariant());
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void Email_WithInvalidEmail_ShouldThrowException(string invalid)
    {
        var action = () => Email.Create(invalid);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Email inválido");
    }

    [Theory]
    [InlineData("(11) 98765-4321")]
    [InlineData("11987654321")]
    [InlineData("1140041000")]
    public void Phone_WithValidPhone_ShouldCreate(string phone)
    {
        var phoneObj = Phone.Create(phone);

        phoneObj.Should().NotBeNull();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("")]
    public void Phone_WithInvalidPhone_ShouldThrowException(string invalid)
    {
        var action = () => Phone.Create(invalid);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Address_WithValidData_ShouldCreate()
    {
        var address = Address.Create(
            "01310-100",
            "Av. Paulista",
            "1578",
            "Andar 10",
            "Bela Vista",
            "São Paulo",
            "SP");

        address.Should().NotBeNull();
        address.ZipCode.Should().Be("01310100");
        address.Street.Should().Be("Av. Paulista");
        address.Number.Should().Be("1578");
        address.Complement.Should().Be("Andar 10");
        address.Neighborhood.Should().Be("Bela Vista");
        address.City.Should().Be("São Paulo");
        address.State.Should().Be("SP");
    }

    [Fact]
    public void Address_WithInvalidZipCode_ShouldThrowException()
    {
        var action = () => Address.Create(
            "123",
            "Av. Paulista",
            "1578",
            null,
            "Bela Vista",
            "São Paulo",
            "SP");

        action.Should().Throw<ArgumentException>()
            .WithMessage("CEP inválido");
    }
}
