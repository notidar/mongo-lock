using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Notidar.MongoDB.Lock.Managers;
using Notidar.MongoDB.Lock.Stores;
using System;

namespace Notidar.MongoDB.Lock
{
    public static class Extensions
    {
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
                .AddSingleton<ILockManager, LockManager>(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<LockOptions>>();
                    var lockStore = sp.GetRequiredService<ILockStore>();
                    return new LockManager(lockStore, options.Value);
                });
        }
    }
}
