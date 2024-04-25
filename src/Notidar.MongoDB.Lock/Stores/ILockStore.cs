namespace Notidar.MongoDB.Lock.Stores
{
    public interface ILockStore
    {
        Task<Resource?> GetResourceAsync(string resourceId, CancellationToken cancellationToken = default);
        Task<Resource?> SharedLockAsync(string resourceId, string lockId, TimeSpan expiration, int maxSharedLock, CancellationToken cancellationToken = default);
        Task<Resource?> ExclusiveLockAsync(string resourceId, string lockId, TimeSpan expiration, bool ignoreSharedLocks, CancellationToken cancellationToken = default);
        Task<Resource?> ExclusiveUnlockAsync(string resourceId, string lockId, CancellationToken cancellationToken = default);
        Task<Resource?> SharedUnlockAsync(string resourceId, string lockId, CancellationToken cancellationToken = default);
        Task<Resource?> ExclusiveRenewAsync(string resourceId, string lockId, TimeSpan expiration, CancellationToken cancellationToken = default);
        Task<Resource?> SharedRenewAsync(string resourceId, string lockId, TimeSpan expiration, CancellationToken cancellationToken = default);
    }
}
