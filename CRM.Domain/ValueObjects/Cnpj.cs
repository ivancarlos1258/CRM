using CRM.Domain.Common;

namespace CRM.Domain.ValueObjects;

public class Cnpj : ValueObject
{
    public string Value { get; private set; }

    private Cnpj(string value)
    {
        Value = value;
    }

    public static Cnpj Create(string cnpj)
    {
        var cleanCnpj = CleanDocument(cnpj);

        if (!IsValid(cleanCnpj))
            throw new ArgumentException("CNPJ inválido");

        return new Cnpj(cleanCnpj);
    }

    private static string CleanDocument(string document)
    {
        return new string(document.Where(char.IsDigit).ToArray());
    }

    private static bool IsValid(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        if (cnpj.Length != 14)
            return false;

        if (cnpj.All(c => c == cnpj[0]))
            return false;

        var multipliers1 = new int[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var multipliers2 = new int[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var sum = 0;
        for (int i = 0; i < 12; i++)
            sum += int.Parse(cnpj[i].ToString()) * multipliers1[i];

        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;

        if (int.Parse(cnpj[12].ToString()) != digit1)
            return false;

        sum = 0;
        for (int i = 0; i < 13; i++)
            sum += int.Parse(cnpj[i].ToString()) * multipliers2[i];

        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;

        return int.Parse(cnpj[13].ToString()) == digit2;
    }

    public string GetFormatted()
    {
        return $"{Value[..2]}.{Value.Substring(2, 3)}.{Value.Substring(5, 3)}/{Value.Substring(8, 4)}-{Value.Substring(12, 2)}";
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
