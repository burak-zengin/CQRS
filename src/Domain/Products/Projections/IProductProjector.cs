using Domain.Products.IntegrationEvents;
using Domain.Products.ReadModels;

namespace Domain.Products.Projections;

/// <summary>
/// Translates integration events into the canonical <see cref="ProductDetailReadModel"/>.
/// The list view is derived from the detail view via <see cref="IProductListProjector"/>.
/// </summary>
public interface IProductProjector
{
    ProductDetailReadModel Apply(ProductCreatedIntegrationEvent @event);

    ProductDetailReadModel Apply(ProductRenamedIntegrationEvent @event, ProductDetailReadModel existing);

    ProductDetailReadModel Apply(ProductActivatedIntegrationEvent @event, ProductDetailReadModel existing);

    ProductDetailReadModel Apply(ProductArchivedIntegrationEvent @event, ProductDetailReadModel existing);

    ProductDetailReadModel Apply(ProductVariantAddedIntegrationEvent @event, ProductDetailReadModel existing);

    ProductDetailReadModel Apply(ProductVariantRemovedIntegrationEvent @event, ProductDetailReadModel existing);

    ProductDetailReadModel Apply(ProductVariantPriceChangedIntegrationEvent @event, ProductDetailReadModel existing);
}
