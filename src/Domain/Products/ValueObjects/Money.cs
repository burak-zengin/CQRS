namespace Domain.Products.ValueObjects;

public sealed record class Money
{
    public decimal Amount { get; }

    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        var normalized = currency.Trim().ToUpperInvariant();

        if (normalized.Length != 3 || !normalized.All(char.IsLetter))
        {
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));
        }

        return new Money(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), normalized);
    }

    public bool HasSameCurrency(Money other) =>
        other is not null && string.Equals(Currency, other.Currency, StringComparison.Ordinal);

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
