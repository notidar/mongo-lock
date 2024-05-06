using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Notidar.MongoDB.Lock.Services;
using Notidar.MongoDB.Lock.Stores;
using System;

namespace Notidar.MongoDB.Lock.Extensions
{
    public static class LockExtensions
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
                    var options = sp.GetRequiredService<IOptions<LockOptions>>();
                    var database = sp.GetRequiredService<IMongoDatabase>();
                    return database.GetCollection<Resource>(options.Value.CollectionName ?? Constants.DefaultCollectionName);
                })
                .AddHostedService<LockCollectionService>()
                .AddSingleton<ILockStore, LockStore>()
                .AddSingleton<ILockService, LockService>(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<LockOptions>>();
                    var lockStore = sp.GetRequiredService<ILockStore>();
                    return new LockService(lockStore, options.Value);
                });
        }

        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .Configure<DatabaseOptions>(configuration)
                .AddMongoDb();
        }

        public static IServiceCollection AddMongoDb(this IServiceCollection services, Action<DatabaseOptions> configureOptions)
        {
            return services
                .Configure(configureOptions)
                .AddMongoDb();
        }

        public static IServiceCollection AddMongoDb(this IServiceCollection services)
        {
            return services
                .AddSingleton<IMongoClient>(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<DatabaseOptions>>();
                    var mongoUrl = new MongoUrl(options.Value.ConnectionString ?? throw new InvalidOperationException("Connection string was not provided."));
                    return new MongoClient(mongoUrl);
                })
                .AddSingleton<IMongoDatabase>(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<DatabaseOptions>>();
                    var mongoUrl = new MongoUrl(options.Value.ConnectionString ?? throw new InvalidOperationException("Connection string was not provided."));
                    var client = sp.GetRequiredService<IMongoClient>();
                    var databaseName = mongoUrl.DatabaseName ?? options.Value.DatabaseName ?? throw new InvalidOperationException("Database name was not provided.");
                    return client.GetDatabase(databaseName);
                });
        }
    }
}
