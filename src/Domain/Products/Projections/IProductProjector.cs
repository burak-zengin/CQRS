using Domain.Products.IntegrationEvents;
using Domain.Products.ReadModels;

namespace Domain.Products.Projections;

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
