using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Notidar.MongoDB.Lock.Sample.Database
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .Configure<DatabaseOptions>(configuration)
                .AddSingleton<IMongoClient>(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<DatabaseOptions>>();
                    return new MongoClient(options.Value.ConnectionString);
                })
                .AddSingleton<IMongoDatabase>(sp =>
                {
                    var options = sp.GetRequiredService<IOptions<DatabaseOptions>>();
                    var client = sp.GetRequiredService<IMongoClient>();
                    return client.GetDatabase(options.Value.DatabaseName);
                });
        }
    }
}
