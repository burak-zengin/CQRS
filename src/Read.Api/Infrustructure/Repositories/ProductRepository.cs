using Domain.Products;
using MongoDB.Driver;
using Write.Api.Domain.Products;

namespace Read.Api.Infrustructure.Repositories;

public class ProductRepository : IProductReadRepository
{
    private readonly IMongoCollection<Product> _collection;

    public ProductRepository(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
        var database = client.GetDatabase("Products");
        _collection = database.GetCollection<Product>("ProductCollection");
    }

    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public async Task<Product> GetAsync(int id, CancellationToken cancellationToken)
    {
        return await _collection.Find(_ => _.Id == id).FirstOrDefaultAsync(cancellationToken);
    }
}
