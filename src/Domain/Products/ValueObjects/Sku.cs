namespace Domain.Products.ValueObjects;

public sealed record class Sku
{
    private const int MinLength = 3;
    private const int MaxLength = 32;

    public string Value { get; }

    private Sku(string value) => Value = value;

    public static Sku Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("Sku cannot be empty.", nameof(raw));
        }

        var trimmed = raw.Trim();

        if (trimmed.Length is < MinLength or > MaxLength)
        {
            throw new ArgumentException(
                $"Sku length must be between {MinLength} and {MaxLength}.", nameof(raw));
        }

        if (!trimmed.All(c => char.IsLetterOrDigit(c) || c == '-'))
        {
            throw new ArgumentException("Sku must contain only alphanumeric characters or hyphens.", nameof(raw));
        }

        return new Sku(trimmed);
    }

    public static Sku FromPersistence(string value) => new(value);

    public override string ToString() => Value;
}
