using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Notidar.MongoDB.Lock.Extensions
{
    internal class LockCollectionService : IHostedService
    {
        private readonly IMongoDatabase _database;
        private readonly LockOptions _options;
        public LockCollectionService(IMongoDatabase database, IOptions<LockOptions> options)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var collectionName = _options.CollectionName ?? Constants.DefaultCollectionName;
            await _database.EnsureLockCollectionExistsAsync(collectionName, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
