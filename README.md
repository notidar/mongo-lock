# mongo-lock
Distributed lock library for C#/.NET using MongoDB.

`Notidar.MongoDB.Lock` is a core library that provides a simple way to create distributed locks for simple apps. `LockStore` can be used for direct lock management, while `LockService` provides a high level abstraction.

`Notidar.MongoDB.Lock.Extensions` is a library that provides all required extensions to use the library with `Microsoft.Extensions.DependencyInjection` including creating collection and indexes.

## Features

- Distributed locks using MongoDB
  - Exclusive locks
  - Shared locks
  - Semaphore usage flow
- MongoDB collection cleanup

## Usage

### With `IServiceCollection` with high level manipulations
```csharp
...
services
    .AddMongoDb(configuration.GetSection(nameof(DatabaseOptions))) // if MongoDB not registered
    .AddMongoLocks(configuration.GetSection(nameof(LockSettings))); // add all required services for locks
...

// resolve lock service
var lockService = sp.GetRequiredService<ILockService>();

await using (var sharedLock1 = await _lockService.SharedLockAsync(resourceId, lockId1, operationCancellationToken))
{
    // shared lock1 acquired
    await using (var sharedLock2 = await _lockService.SharedLockAsync(resourceId, lockId2, operationCancellationToken))
    {
        // shared lock2 acquired
    }
    // shared lock2 released
}
// shared lock1 released

await using (var exclusiveLock = await _lockService.ExclusiveLockAsync(resourceId, exclusiveLockId, operationCancellationToken))
{
    // exclusive lock acquired
}
// exclusive lock released

```

### Simple console app with low level manipulations

```csharp
var client = new MongoClient(connectionString);
var database = client.GetDatabase("cli-test");

var lockStore = new LockStore(database.GetCollection<Resource>("locks")); // Resource defined in Notidar.MongoDB.Lock.Stores
var lockService = new LockService(lockStore); // in case you need high level manipulations

// low level manipulation
var resourceResult = await lockStore.ExclusiveLockAsync("resourceId-1", "lockId-1", TimeSpan.FromSeconds(60)); // exlusive lock
if (resource?.ExclusiveLock?.LockId == "lockId-1")
{
    // exclusive locked aquired
    await lockStore.ExclusiveUnlockAsync("resourceId-1", "lockId-1"); // exlusive release
}

var resourceFirstSharedResult = await lockStore.SharedLockAsync("resourceId-1", "lockId-2", TimeSpan.FromSeconds(60));
var resourceSecondSharedResult = await lockStore.SharedLockAsync("resourceId-1", "lockId-3", TimeSpan.FromSeconds(60), sharedLockLimit: 10);
if (resourceFirstSharedResult?.SharedLocks?.Any(x => x.LockId == "lockId-2") && resourceSecondSharedResult?.SharedLocks?.Any(x => x.LockId == "lockId-3"))
{
    // both shared locked aquired
    await lockStore.SharedUnlockAsync("resourceId-1", "lockId-2"); // shared release
    await lockStore.SharedUnlockAsync("resourceId-1", "lockId-3"); // shared release
}

```

See samples folder for more examples.

## Roadmap (Contributions are welcome)

Contributions are welcome! If you are interested in contributing towards a new or existing issue, please let me know via comments on the issue so that I can help you get started and avoid wasted effort on your part.

- [ ] Add support for infinite locks (locks that never expire)
- [ ] Add support for optional lock re-entry
- [ ] Add logging support
- [ ] Add CLI for managing locks. (e.g. `mongo-lock --list`, `mongo-lock --delete <lockId>`)
- [ ] Add support for MongoDB unlock detection. Based either [MongoDB Capped Collections](https://www.mongodb.com/docs/manual/core/capped-collections/) with signals or on [MongoDB Change Streams](https://www.mongodb.com/docs/manual/changeStreams/) (e.g. `LockService.WaitForUnlockAsync(lockId, timeout)`)
- [ ] Improve unit test coverage

## License

MIT License