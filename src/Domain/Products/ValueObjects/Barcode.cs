namespace Domain.Products.ValueObjects;

public sealed record class Barcode
{
    private const int MinLength = 8;
    private const int MaxLength = 14;

    public string Value { get; }

    private Barcode(string value) => Value = value;

    public static Barcode Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("Barcode cannot be empty.", nameof(raw));
        }

        var trimmed = raw.Trim();

        if (trimmed.Length is < MinLength or > MaxLength)
        {
            throw new ArgumentException(
                $"Barcode length must be between {MinLength} and {MaxLength}.", nameof(raw));
        }

        if (!trimmed.All(char.IsLetterOrDigit))
        {
            throw new ArgumentException("Barcode must be alphanumeric.", nameof(raw));
        }

        return new Barcode(trimmed);
    }

    public static Barcode FromPersistence(string value) => new(value);

    public override string ToString() => Value;
}
