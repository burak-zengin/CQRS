namespace Domain.Products.IntegrationEvents;

public sealed record ProductVariantPayload(
    Guid Id,
    string Sku,
    string Barcode,
    string Color,
    string Size,
    decimal PriceAmount,
    string Currency);
