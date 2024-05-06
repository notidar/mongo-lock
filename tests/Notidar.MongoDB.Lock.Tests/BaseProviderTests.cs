using Microsoft.Extensions.Time.Testing;
using MongoDB.Driver;
using Notidar.MongoDB.Lock.Stores;

namespace Notidar.MongoDB.Lock.Tests
{
    public abstract class BaseProviderTests : IAsyncLifetime
    {
        protected IMongoClient MongoClient { get; set; }
        protected IMongoDatabase MongoDatabase { get; set; }
        protected LockStore LockProvider { get; set; }
        protected FakeTimeProvider FakeTimeProvider { get; set; }
        protected TimeSpan Precision { get; set; }

        public BaseProviderTests()
        {
            Precision = TimeSpan.FromMilliseconds(1);
            FakeTimeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
            MongoClient = new MongoClient("mongodb://localhost:30000/?readPreference=primary");
            MongoDatabase = MongoClient.GetDatabase("test");
            LockProvider = new LockStore(MongoDatabase.GetCollection<Resource>(Constants.DefaultCollectionName), FakeTimeProvider);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await MongoDatabase.DropCollectionAsync(Constants.DefaultCollectionName);
            await MongoDatabase.EnsureLockCollectionExistsAsync();
            await MongoDatabase.CreateCleanupIndexAsync();
        }
    }
}
