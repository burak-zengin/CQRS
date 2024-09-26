using Domain.Products;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Write.Api.Domain.Products;

namespace Consumer.Infrustructure.Repositories;

public class ProductRepository : IProductWriteRepository
{
    private readonly IMongoCollection<Product> _collection;

    public ProductRepository(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
        var database = client.GetDatabase("Products");
        _collection = database.GetCollection<Product>("ProductCollection");
    }

    public async Task<int> CreateAsync(Product product)
    {
        await _collection.InsertOneAsync(product);

        return product.Id;
    }
}
