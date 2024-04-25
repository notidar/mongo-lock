using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Notidar.MongoDB.Lock.Managers;
using Notidar.MongoDB.Lock.Stores;

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

        public static IServiceCollection AddMongoLocks(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .Configure<LockOptions>(configuration)
                .AddMongoLocks();
        }

        public static IServiceCollection AddMongoLocks(this IServiceCollection services, Action<LockOptions> configureOptions)
        {
            return services
                .Configure(configureOptions)
                .AddMongoLocks();
        }

        public static IServiceCollection AddMongoLocks(this IServiceCollection services)
        {
            return services
                .AddSingleton<IMongoCollection<Resource>>(sp => 
                {
                    var database = sp.GetRequiredService<IMongoDatabase>();
                    return database.GetCollection<Resource>(Constants.DefaultCollectionName);
                })
                .AddSingleton<ILockStore, LockStore>()
                .AddSingleton<ILockManager, LockManager>();
        }
    }
}
