using MongoDB.Bson;
using MongoDB.Driver;
using Notidar.MongoDB.Lock.Stores;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Notidar.MongoDB.Lock
{
    public static class LockExtensions
    {
        public static async Task EnsureLockCollectionExistsAsync(this IMongoDatabase database, string collectionName = Constants.DefaultCollectionName, CancellationToken cancellationToken = default)
        {
            var collections = await database.ListCollectionNamesAsync(
                options: new ListCollectionNamesOptions
                {
                    Filter = new BsonDocument("name", collectionName)
                },
                cancellationToken: cancellationToken);

            if (false == await collections.AnyAsync(cancellationToken))
            {
                await database.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);
            }
        }

        public static async Task CreateCleanupIndexAsync(this IMongoDatabase database, string collectionName = Constants.DefaultCollectionName, string cleanupIndexName = Constants.CleanupIndexName, CancellationToken cancellationToken = default)
        {
            var collection = database.GetCollection<Resource>(collectionName);
            await collection.Indexes.CreateOneAsync(
                model: new CreateIndexModel<Resource>(
                    keys: new IndexKeysDefinitionBuilder<Resource>().Ascending(resource => resource!.MaxExpiration),
                    options: new CreateIndexOptions<Resource>()
                    {
                        Name = cleanupIndexName,
                        PartialFilterExpression = Builders<Resource>.Filter.Eq(resource => resource.InfiniteLockCount, 0),
                        ExpireAfter = TimeSpan.FromMinutes(1),
                    }),
                options: null,
                cancellationToken: cancellationToken);
        }

        public static async Task DropCleanupIndexAsync(this IMongoDatabase database, string collectionName = Constants.DefaultCollectionName, string cleanupIndexName = Constants.CleanupIndexName, CancellationToken cancellationToken = default)
        {
            var collection = database.GetCollection<Resource>(collectionName);
            var existingIndexesCursor = await collection.Indexes.ListAsync(cancellationToken);
            var existingIndexes = await existingIndexesCursor.ToListAsync(cancellationToken);
            if (existingIndexes.Any(x => x.GetElement("name").Value.AsString == cleanupIndexName))
            {
                try
                {
                    await collection.Indexes.DropOneAsync(cleanupIndexName, cancellationToken);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }
    }
}
