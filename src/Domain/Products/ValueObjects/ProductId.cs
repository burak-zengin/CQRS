namespace Domain.Products.ValueObjects;

public sealed record class ProductId
{
    public Guid Value { get; }

    private ProductId(Guid value) => Value = value;

    public static ProductId New() => new(Guid.NewGuid());

    public static ProductId From(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ProductId cannot be empty.", nameof(value));
        }

        return new ProductId(value);
    }

    public override string ToString() => Value.ToString();
}
