using Domain.Products.ReadModels;
using Domain.Products.Repositories;
using MongoDB.Driver;

namespace Read.Api.Infrastructure.Repositories;

public class ProductRepository : IProductReadRepository
{
    private readonly IMongoCollection<ProductDetailReadModel> _detail;
    private readonly IMongoCollection<ProductListReadModel> _list;

    public ProductRepository(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
        var database = client.GetDatabase("Products");
        _detail = database.GetCollection<ProductDetailReadModel>("ProductDetail");
        _list = database.GetCollection<ProductListReadModel>("ProductList");
    }

    public async Task<ProductDetailReadModel?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _detail.Find(p => p.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<ProductListReadModel>> GetListAsync(CancellationToken cancellationToken)
    {
        return await _list.Find(_ => true).ToListAsync(cancellationToken);
    }
}
