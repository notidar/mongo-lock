using Notidar.MongoDB.Lock.Stores;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Notidar.MongoDB.Lock.Services
{
    public sealed class LockService : ILockService
    {
        private readonly ILockStore _lockStore;
        private readonly LockSettings _settings;

        public LockService(ILockStore lockStore, LockSettings settings)
        {
            _lockStore = lockStore ?? throw new ArgumentNullException(nameof(lockStore));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public LockService(ILockStore lockStore) : this(lockStore, new LockSettings())
        {
        }

        public async Task<ILock> ExclusiveLockAsync(string resourceId, string lockId, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var resource = await _lockStore.ExclusiveLockAsync(resourceId, lockId, _settings.LockExpirationPeriod, cancellationToken);
                if (resource != null)
                {
                    while (true)
                    {
                        if ((resource.SharedLocks?.Length ?? 0) == 0 && resource.ExclusiveLock?.Expiration != null)
                        {
                            return new ExclusiveLockHandler(_lockStore, _settings, resourceId, lockId, resource.ExclusiveLock.Expiration.Value, cancellationToken);
                        }
                        await Task.Delay(_settings.LockRetryDelay, cancellationToken);
                        resource = await _lockStore.ExclusiveRenewAsync(resourceId, lockId, _settings.LockExpirationPeriod, cancellationToken);
                        if (resource == null)
                        {
                            break;
                        }
                    }
                }
                await Task.Delay(_settings.LockRetryDelay, cancellationToken);
            }
        }

        public Task<ILock> ExclusiveLockAsync(string resourceId, CancellationToken cancellationToken = default)
        {
            return ExclusiveLockAsync(resourceId, Guid.NewGuid().ToString(), cancellationToken);
        }

        public async Task<ILock> SharedLockAsync(string resourceId, string lockId, CancellationToken cancellationToken = default)
        {
            return await SharedLockInternalAsync(resourceId, lockId, null, cancellationToken);
        }

        public Task<ILock> SharedLockAsync(string resourceId, CancellationToken cancellationToken = default)
        {
            return SharedLockInternalAsync(resourceId, Guid.NewGuid().ToString(), null, cancellationToken);
        }

        public async Task<ILock> SharedLockAsync(string resourceId, string lockId, int sharedLockLimit, CancellationToken cancellationToken = default)
        {
            return await SharedLockInternalAsync(resourceId, lockId, sharedLockLimit, cancellationToken);
        }

        public Task<ILock> SharedLockAsync(string resourceId, int sharedLockLimit, CancellationToken cancellationToken = default)
        {
            return SharedLockInternalAsync(resourceId, Guid.NewGuid().ToString(), sharedLockLimit, cancellationToken);
        }

        private async Task<ILock> SharedLockInternalAsync(string resourceId, string lockId, int? sharedLockLimit, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var resource = await _lockStore.SharedLockAsync(resourceId, lockId, _settings.LockExpirationPeriod, sharedLockLimit, cancellationToken: cancellationToken);
                var sharedLock = resource?.SharedLocks?.SingleOrDefault(x => x.LockId == lockId);
                if (sharedLock?.Expiration != null)
                {
                    return new SharedLockHandler(_lockStore, _settings, resourceId, lockId, sharedLock.Expiration.Value, cancellationToken);
                }
                resource = await _lockStore.GetResourceAsync(resourceId, cancellationToken);
                if (resource == null || resource.SharedLocks == null)
                {
                    continue;
                }
                if ((sharedLockLimit == null || resource.SharedLocks.Length < sharedLockLimit) && !resource.SharedLocks.Any(l => l.LockId == lockId))
                {
                    continue;
                }

                await Task.Delay(_settings.LockRetryDelay, cancellationToken);
            }
        }
    }
}
