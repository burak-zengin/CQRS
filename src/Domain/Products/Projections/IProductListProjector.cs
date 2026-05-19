using Domain.Products.ReadModels;

namespace Domain.Products.Projections;

/// <summary>
/// Derives the lean list-view projection from the canonical detail projection.
/// Keeps the two read models consistent without duplicating event-handling logic.
/// </summary>
public interface IProductListProjector
{
    ProductListReadModel Project(ProductDetailReadModel detail);
}
