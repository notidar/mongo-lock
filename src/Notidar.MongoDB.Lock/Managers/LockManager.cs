using Microsoft.Extensions.Options;
using Notidar.MongoDB.Lock.Stores;

namespace Notidar.MongoDB.Lock.Managers
{
    public sealed class LockManager : ILockManager
    {
        private ILockStore _lockStore;
        private LockOptions _options;
        public LockManager(ILockStore lockStore, IOptions<LockOptions> options)
        {
            _lockStore = lockStore ?? throw new ArgumentNullException(nameof(lockStore));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<ILock> ExclusiveLockAsync(string resourceId, string lockId, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var resource = await _lockStore.ExclusiveLockAsync(resourceId, lockId, _options.LockExpiration, true, cancellationToken);
                if (resource != null)
                {
                    while (true)
                    {
                        if ((resource.SharedLocks?.Length ?? 0) == 0 && resource.ExclusiveLock?.Expiration != null)
                        {
                            return new ExclusiveLockHandler(_lockStore, _options, resourceId, lockId, resource.ExclusiveLock.Expiration.Value, cancellationToken);
                        }
                        await Task.Delay(_options.LockRetryDelay, cancellationToken);
                        resource = await _lockStore.ExclusiveRenewAsync(resourceId, lockId, _options.LockExpiration, cancellationToken);
                        if (resource == null)
                        {
                            break;
                        }
                    }
                }
                await Task.Delay(_options.LockRetryDelay, cancellationToken);
            }
        }

        public async Task<ILock> SharedLockAsync(string resourceId, string lockId, CancellationToken cancellationToken = default)
        {
            return await SharedLockAsync(resourceId, lockId, _options.MaxSharedLocksPerResource, cancellationToken);
        }

        public async Task<ILock> SharedLockAsync(string resourceId, string lockId, int maxLocks, CancellationToken cancellationToken = default)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var resource = await _lockStore.SharedLockAsync(resourceId, lockId, _options.LockExpiration, maxLocks, cancellationToken: cancellationToken);
                var sharedLock = resource?.SharedLocks?.SingleOrDefault(x => x.LockId == lockId);
                if (sharedLock?.Expiration != null)
                {
                    return new SharedLockHandler(_lockStore, _options, resourceId, lockId, sharedLock.Expiration.Value, cancellationToken);
                }
                resource = await _lockStore.GetResourceAsync(resourceId, cancellationToken);
                if (resource == null || resource.SharedLocks == null)
                {
                    continue;
                }
                if (resource.SharedLocks.Length < maxLocks && !resource.SharedLocks.Any(l => l.LockId == lockId))
                {
                    continue;
                }

                await Task.Delay(_options.LockRetryDelay, cancellationToken);
            }
        }
    }
}
