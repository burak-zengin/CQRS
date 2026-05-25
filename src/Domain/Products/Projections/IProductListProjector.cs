using Domain.Products.ReadModels;

namespace Domain.Products.Projections;

public interface IProductListProjector
{
    ProductListReadModel Project(ProductDetailReadModel detail);
}
