using Domain.Products.ReadModels;
using Domain.Products.Repositories;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Consumer.Infrastructure.Repositories;

public sealed class ProductProjectionRepository : IProductProjectionRepository
{
    private readonly IMongoCollection<ProductDetailReadModel> _detail;
    private readonly IMongoCollection<ProductListReadModel> _list;
    private readonly IMongoCollection<ProcessedEventDocument> _processedEvents;

    public ProductProjectionRepository(IConfiguration configuration)
    {
        var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
        var database = client.GetDatabase("Products");
        _detail = database.GetCollection<ProductDetailReadModel>("ProductDetail");
        _list = database.GetCollection<ProductListReadModel>("ProductList");
        _processedEvents = database.GetCollection<ProcessedEventDocument>("ProcessedEvents");
    }

    public async Task<ProductDetailReadModel?> GetDetailAsync(Guid productId, CancellationToken cancellationToken)
    {
        return await _detail
            .Find(p => p.Id == productId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task UpsertDetailAsync(ProductDetailReadModel detail, CancellationToken cancellationToken)
    {
        var filter = Builders<ProductDetailReadModel>.Filter.Eq(p => p.Id, detail.Id);
        var options = new ReplaceOptions { IsUpsert = true };
        await _detail.ReplaceOneAsync(filter, detail, options, cancellationToken);
    }

    public async Task UpsertListAsync(ProductListReadModel list, CancellationToken cancellationToken)
    {
        var filter = Builders<ProductListReadModel>.Filter.Eq(p => p.Id, list.Id);
        var options = new ReplaceOptions { IsUpsert = true };
        await _list.ReplaceOneAsync(filter, list, options, cancellationToken);
    }

    public async Task DeleteAsync(Guid productId, CancellationToken cancellationToken)
    {
        await _detail.DeleteOneAsync(
            Builders<ProductDetailReadModel>.Filter.Eq(p => p.Id, productId),
            cancellationToken);

        await _list.DeleteOneAsync(
            Builders<ProductListReadModel>.Filter.Eq(p => p.Id, productId),
            cancellationToken);
    }

    public async Task<bool> HasProcessedAsync(Guid eventId, CancellationToken cancellationToken)
    {
        var doc = await _processedEvents
            .Find(p => p.Id == eventId)
            .FirstOrDefaultAsync(cancellationToken);
        return doc is not null;
    }

    public async Task MarkProcessedAsync(Guid eventId, CancellationToken cancellationToken)
    {
        try
        {
            await _processedEvents.InsertOneAsync(
                new ProcessedEventDocument { Id = eventId, ProcessedAt = DateTimeOffset.UtcNow },
                options: null,
                cancellationToken);
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
        }
    }

    internal sealed class ProcessedEventDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; init; }

        public DateTimeOffset ProcessedAt { get; init; }
    }
}
