namespace Domain.Products.ValueObjects;

public sealed record class VariantId
{
    public Guid Value { get; }

    private VariantId(Guid value) => Value = value;

    public static VariantId New() => new(Guid.NewGuid());

    public static VariantId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("VariantId cannot be empty.", nameof(value));
        }

        return new VariantId(value);
    }

    public override string ToString() => Value.ToString();
}
