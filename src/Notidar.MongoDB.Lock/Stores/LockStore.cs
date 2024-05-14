using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Notidar.MongoDB.Lock.Stores
{
    public sealed class LockStore : ILockStore
    {
        private readonly IMongoCollection<Resource> _lockCollection;
        private readonly TimeProvider _timeProvider;
        public LockStore(IMongoCollection<Resource> lockCollection, TimeProvider timeProvider)
        {
            _lockCollection = lockCollection ?? throw new ArgumentNullException(nameof(lockCollection));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        }

        public LockStore(IMongoCollection<Resource> lockCollection) : this(lockCollection, TimeProvider.System)
        {
        }

        public async Task<Resource?> GetResourceAsync(string resourceId, CancellationToken cancellationToken = default)
        {
            var utcNow = _timeProvider.GetUtcNow();
            var result = await _lockCollection.FindOneAndUpdateAsync(
                filter: Builders<Resource>.Filter
                    .Eq(r => r.ResourceId, resourceId),
                update: Builders<Resource>.Update
                    .PullFilter(r => r.SharedLocks, Builders<SharedLock>.Filter.Lt(l => l.Expiration, utcNow)),
                options: new FindOneAndUpdateOptions<Resource>
                {
                    IsUpsert = false,
                    ReturnDocument = ReturnDocument.After
                },
                cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Resource?> SharedLockAsync(string resourceId, string lockId, TimeSpan expiration, int? sharedLockLimit, CancellationToken cancellationToken = default)
        {
            try
            {
                var utcNow = _timeProvider.GetUtcNow();
                var @lock = new SharedLock
                {
                    LockId = lockId,
                    Expiration = utcNow.Add(expiration),
                };
                var lockLimitFilter = sharedLockLimit.HasValue
                    ? Builders<Resource>.Filter.Exists(r => r.SharedLocks![sharedLockLimit.Value - 1], false)
                    : Builders<Resource>.Filter.Empty;
                var result = await _lockCollection.FindOneAndUpdateAsync(
                    filter: Builders<Resource>.Filter.And(
                        Builders<Resource>.Filter.Eq(r => r.ResourceId, resourceId),
                        Builders<Resource>.Filter.Where(r => r.SharedLocks!.Any(r => r.LockId == lockId) == false),
                        lockLimitFilter,
                        Builders<Resource>.Filter.Or(
                            Builders<Resource>.Filter.Exists(r => r.ExclusiveLock, false),
                            Builders<Resource>.Filter.Lt(r => r.ExclusiveLock!.Expiration, utcNow))),
                    update: Builders<Resource>.Update
                        .Push(r => r.SharedLocks, @lock)
                        .Max(r => r.MaxExpiration, @lock.Expiration)
                        .SetOnInsert(r => r.InfiniteLockCount, 0),
                    options: new FindOneAndUpdateOptions<Resource>
                    {
                        IsUpsert = true,
                        ReturnDocument = ReturnDocument.After
                    },
                    cancellationToken: cancellationToken);
                return result;
            }
            catch (MongoCommandException e) when (e.Code == 11000)
            {
                return null;
            }
        }

        public async Task<Resource?> ExclusiveLockAsync(string resourceId, string lockId, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            try
            {
                var utcNow = _timeProvider.GetUtcNow();
                var @lock = new ExclusiveLock
                {
                    LockId = lockId,
                    Expiration = utcNow.Add(expiration),
                };
                var result = await _lockCollection.FindOneAndUpdateAsync(
                    filter: Builders<Resource>.Filter.And(
                        Builders<Resource>.Filter.Eq(r => r.ResourceId, resourceId),
                        Builders<Resource>.Filter.Or(
                            Builders<Resource>.Filter.Exists(r => r.ExclusiveLock, false),
                            Builders<Resource>.Filter.Lt(r => r.ExclusiveLock!.Expiration, utcNow))),
                    update: Builders<Resource>.Update
                        .Set(r => r.ExclusiveLock, @lock)
                        .Max(r => r.MaxExpiration, @lock.Expiration)
                        .SetOnInsert(r => r.InfiniteLockCount, 0),
                    options: new FindOneAndUpdateOptions<Resource>
                    {
                        IsUpsert = true,
                        ReturnDocument = ReturnDocument.After
                    },
                    cancellationToken: cancellationToken);
                return result;
            }
            catch (MongoCommandException e) when (e.Code == 11000)
            {
                return null;
            }
        }

        public async Task<Resource?> ExclusiveUnlockAsync(string resourceId, string lockId, CancellationToken cancellationToken = default)
        {
            var result = await _lockCollection.FindOneAndUpdateAsync(
                filter: Builders<Resource>.Filter.And(
                    Builders<Resource>.Filter.Eq(r => r.ResourceId, resourceId),
                    Builders<Resource>.Filter.Eq(r => r.ExclusiveLock!.LockId, lockId)),
                update: Builders<Resource>.Update.Unset(r => r.ExclusiveLock),
                options: new FindOneAndUpdateOptions<Resource>
                {
                    ReturnDocument = ReturnDocument.After
                },
                cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Resource?> SharedUnlockAsync(string resourceId, string lockId, CancellationToken cancellationToken = default)
        {
            var result = await _lockCollection.FindOneAndUpdateAsync(
                filter: Builders<Resource>.Filter.And(
                    Builders<Resource>.Filter.Eq(r => r.ResourceId, resourceId),
                    Builders<Resource>.Filter.ElemMatch(r => r.SharedLocks, l => l.LockId == lockId)),
                update: Builders<Resource>.Update.PullFilter(r => r.SharedLocks, l => l.LockId == lockId),
                options: new FindOneAndUpdateOptions<Resource>
                {
                    IsUpsert = false,
                    ReturnDocument = ReturnDocument.After
                },
                cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Resource?> ExclusiveRenewAsync(string resourceId, string lockId, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var utcNow = _timeProvider.GetUtcNow();
            var expirationUtc = utcNow.Add(expiration);
            var result = await _lockCollection.FindOneAndUpdateAsync(
                filter: Builders<Resource>.Filter.And(
                    Builders<Resource>.Filter.Eq(r => r.ResourceId, resourceId),
                    Builders<Resource>.Filter.Eq(r => r.ExclusiveLock!.LockId, lockId),
                    Builders<Resource>.Filter.Gt(r => r.ExclusiveLock!.Expiration, utcNow)),
                update: Builders<Resource>.Update
                    .Set(r => r.ExclusiveLock!.Expiration, expirationUtc)
                    .Max(r => r.MaxExpiration, expirationUtc)
                    .PullFilter(r => r.SharedLocks, Builders<SharedLock>.Filter.Lt(l => l.Expiration, utcNow)),
                options: new FindOneAndUpdateOptions<Resource>
                {
                    IsUpsert = false,
                    ReturnDocument = ReturnDocument.After
                },
                cancellationToken: cancellationToken);
            return result;
        }

        public async Task<Resource?> SharedRenewAsync(string resourceId, string lockId, TimeSpan expiration, CancellationToken cancellationToken = default)
        {
            var utcNow = _timeProvider.GetUtcNow();
            var expirationUtc = utcNow.Add(expiration);
            var result = await _lockCollection.FindOneAndUpdateAsync(
                filter: Builders<Resource>.Filter.And(
                    Builders<Resource>.Filter.Eq(r => r.ResourceId, resourceId),
                    Builders<Resource>.Filter.ElemMatch(r => r.SharedLocks, x => x.LockId == lockId && x.Expiration > utcNow)),
                update: Builders<Resource>.Update
                    .Set(r => r.SharedLocks.FirstMatchingElement().Expiration, expirationUtc)
                    .Max(r => r.MaxExpiration, expirationUtc),
                options: new FindOneAndUpdateOptions<Resource>
                {
                    IsUpsert = false,
                    ReturnDocument = ReturnDocument.After
                },
                cancellationToken: cancellationToken);
            return result;
        }
    }
}
