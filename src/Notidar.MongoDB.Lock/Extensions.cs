using MongoDB.Driver;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Notidar.MongoDB.Lock
{
    public static class Extensions
    {
        public static async Task EnsureLockCollectionExistsAsync(this IMongoDatabase database, string collectionName = Constants.DefaultCollectionName, CancellationToken cancellationToken = default)
        {
            var collections = await database.ListCollectionNamesAsync(cancellationToken: cancellationToken);
            if (!collections.ToEnumerable(cancellationToken: cancellationToken).Any(x => x == collectionName))
            {
                await database.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);
            }
        }
    }
}
