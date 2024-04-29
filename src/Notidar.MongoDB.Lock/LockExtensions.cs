using MongoDB.Bson;
using MongoDB.Driver;
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
    }
}
