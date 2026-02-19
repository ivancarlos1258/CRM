using CRM.Domain.Common;

namespace CRM.Domain.ValueObjects;

public class Phone : ValueObject
{
    public string Value { get; private set; }

    private Phone(string value)
    {
        Value = value;
    }

    public static Phone Create(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Telefone não pode ser vazio");

        var cleanPhone = CleanPhone(phone);

        if (!IsValid(cleanPhone))
            throw new ArgumentException("Telefone inválido");

        return new Phone(cleanPhone);
    }

    private static string CleanPhone(string phone)
    {
        return new string(phone.Where(char.IsDigit).ToArray());
    }

    private static bool IsValid(string phone)
    {
        return phone.Length >= 10 && phone.Length <= 11;
    }

    public string GetFormatted()
    {
        if (Value.Length == 11)
            return $"({Value[..2]}) {Value[2]}{Value.Substring(3, 4)}-{Value.Substring(7, 4)}";

        return $"({Value[..2]}) {Value.Substring(2, 4)}-{Value.Substring(6, 4)}";
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
