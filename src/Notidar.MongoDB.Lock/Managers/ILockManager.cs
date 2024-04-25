namespace Notidar.MongoDB.Lock.Managers
{
    public interface ILockManager
    {
        Task<ILock> SharedLockAsync(string resourceId, string lockId, CancellationToken cancellationToken = default);
        Task<ILock> SharedLockAsync(string resourceId, string lockId, int maxLocks, CancellationToken cancellationToken = default);
        Task<ILock> ExclusiveLockAsync(string resourceId, string lockId, CancellationToken cancellationToken = default);
    }
}
