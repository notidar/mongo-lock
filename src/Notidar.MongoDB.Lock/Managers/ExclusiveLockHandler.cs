using Notidar.MongoDB.Lock.Stores;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Notidar.MongoDB.Lock.Managers
{
    public sealed class ExclusiveLockHandler : ILock
    {
        private ILockStore? _lockStore;
        private LockOptions _lockManagerOptions;
        private string _resourceId;
        private string _lockId;
        private DateTimeOffset _lockExpiration;
        private CancellationTokenSource? _cancellationTokenSource;
        private CancellationTokenSource? _combinedCancellationTokenSource;
        private Task? _task;

        public ExclusiveLockHandler(
            ILockStore lockStore,
            LockOptions lockManagerOptions,
            string resourceId, 
            string lockId, 
            DateTimeOffset lockExpiration,
            CancellationToken cancellationToken = default)
        {
            _lockStore = lockStore;
            _lockManagerOptions = lockManagerOptions;
            _resourceId = resourceId;
            _lockId = lockId;
            _lockExpiration = lockExpiration;
            _cancellationTokenSource = new CancellationTokenSource();
            if (cancellationToken != default)
            {
                _combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            }

            _task = Task.Run(async () =>
            {
                var delay = lockManagerOptions.LockRenewalPeriod;
                while (_lockStore is not null)
                {
                    await Task.Delay(delay);
                    try
                    {
                        var resource = await (_lockStore?.ExclusiveRenewAsync(_resourceId, _lockId, _lockManagerOptions.LockExpiration) ?? Task.FromResult<Resource?>(null));
                        if (resource?.ExclusiveLock?.Expiration is null)
                        {
                            _cancellationTokenSource?.Cancel();
                            break;
                        }
                        else
                        {
                            _lockExpiration = resource.ExclusiveLock.Expiration.Value;
                            delay = lockManagerOptions.LockRenewalPeriod;
                        }
                    }
                    catch (Exception)
                    {
                        var approxOperationResult = DateTimeOffset.UtcNow + _lockManagerOptions.ClockSafePeriod + _lockManagerOptions.LockRetryDelay + _lockManagerOptions.OperationTimeout;
                        if (approxOperationResult < _lockExpiration)
                        {
                            delay = _lockManagerOptions.LockRetryDelay;
                            continue;
                        }
                        else
                        {
                            _cancellationTokenSource?.Cancel();
                            break;
                        }
                    }
                }
            });
        }

        public CancellationToken HealthToken => _combinedCancellationTokenSource?.Token ?? _cancellationTokenSource?.Token ?? throw new ObjectDisposedException(null);

        public async ValueTask DisposeAsync()
        {
            _combinedCancellationTokenSource?.Dispose();
            _cancellationTokenSource?.Dispose();
            if (_lockStore is not null)
            {
                try
                {
                    await _lockStore.ExclusiveUnlockAsync(_resourceId, _lockId);
                }
                catch
                {

                }
            }
            _lockStore = null;
            _cancellationTokenSource = null;
            _combinedCancellationTokenSource = null;
            _task = null;
        }
    }
}
