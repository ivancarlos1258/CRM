using CRM.Domain.Common;

namespace CRM.Domain.ValueObjects;

public class Address : ValueObject
{
    public string ZipCode { get; private set; }
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string? Complement { get; private set; }
    public string Neighborhood { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }

    private Address(
        string zipCode,
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state)
    {
        ZipCode = zipCode;
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state;
    }

    public static Address Create(
        string zipCode,
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state)
    {
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("CEP não pode ser vazio");

        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Logradouro não pode ser vazio");

        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Número não pode ser vazio");

        if (string.IsNullOrWhiteSpace(neighborhood))
            throw new ArgumentException("Bairro não pode ser vazio");

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("Cidade não pode ser vazia");

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("Estado não pode ser vazio");

        var cleanZipCode = new string(zipCode.Where(char.IsDigit).ToArray());

        if (cleanZipCode.Length != 8)
            throw new ArgumentException("CEP inválido");

        return new Address(cleanZipCode, street, number, complement, neighborhood, city, state);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ZipCode;
        yield return Street;
        yield return Number;
        yield return Complement;
        yield return Neighborhood;
        yield return City;
        yield return State;
    }

    public string GetFormattedZipCode()
    {
        return $"{ZipCode[..5]}-{ZipCode.Substring(5, 3)}";
    }
}
