using System.Threading;
using System.Threading.Tasks;

namespace Notidar.MongoDB.Lock.Services
{
    public interface ILockService
    {
        Task<ILock> SharedLockAsync(string resourceId, CancellationToken cancellationToken = default);
        Task<ILock> SharedLockAsync(string resourceId, string lockId, CancellationToken cancellationToken = default);
        Task<ILock> SharedLockAsync(string resourceId, int sharedLockLimit, CancellationToken cancellationToken = default);
        Task<ILock> SharedLockAsync(string resourceId, string lockId, int sharedLockLimit, CancellationToken cancellationToken = default);
        Task<ILock> ExclusiveLockAsync(string resourceId, string lockId, CancellationToken cancellationToken = default);
        Task<ILock> ExclusiveLockAsync(string resourceId, CancellationToken cancellationToken = default);
    }
}
