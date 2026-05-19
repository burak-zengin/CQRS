using Domain.Products.ReadModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Read.Api.Infrastructure.Persistence;

public static class MongoConfiguration
{
    public static void RegisterClassMaps()
    {
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(BsonType.String));

        if (!BsonClassMap.IsClassMapRegistered(typeof(ProductDetailVariantItem)))
        {
            BsonClassMap.RegisterClassMap<ProductDetailVariantItem>(cm =>
            {
                cm.AutoMap();
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(ProductDetailReadModel)))
        {
            BsonClassMap.RegisterClassMap<ProductDetailReadModel>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(ProductListReadModel)))
        {
            BsonClassMap.RegisterClassMap<ProductListReadModel>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id);
            });
        }
    }
}
