using Notidar.MongoDB.Lock.Stores;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Notidar.MongoDB.Lock.Services
{
    public sealed class SharedLockHandler : ILock
    {
        private ILockStore? _lockStore;
        private readonly LockSettings _lockServiceOptions;
        private readonly string _resourceId;
        private readonly string _lockId;
        private DateTimeOffset _lockExpiration;
        private CancellationTokenSource? _cancellationTokenSource;
        private CancellationTokenSource? _combinedCancellationTokenSource;
        private Task? _task;

        public SharedLockHandler(
            ILockStore? lockStore,
            LockSettings lockServiceOptions,
            string resourceId,
            string lockId,
            DateTimeOffset lockExpiration,
            CancellationToken cancellationToken = default)
        {
            _lockStore = lockStore;
            _lockServiceOptions = lockServiceOptions;
            _resourceId = resourceId;
            _lockId = lockId;
            _lockExpiration = lockExpiration;
            _cancellationTokenSource = new CancellationTokenSource();
            if (cancellationToken != default)
            {
                _combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, cancellationToken);
            }

            var internalCancellationToken = _combinedCancellationTokenSource?.Token ?? _cancellationTokenSource.Token;

            _task = Task.Run(async () =>
            {
                var delay = lockServiceOptions.LockRenewalPeriod;
                while (_lockStore is not null)
                {
                    await Task.Delay(delay, internalCancellationToken);
                    try
                    {
                        var resource = await (_lockStore?.SharedRenewAsync(_resourceId, _lockId, _lockServiceOptions.LockExpirationPeriod, internalCancellationToken) ?? Task.FromResult<Resource?>(null));
                        var sharedLock = resource?.SharedLocks?.SingleOrDefault(x => x.LockId == _lockId);
                        
                        if (sharedLock?.Expiration is null)
                        {
                            _cancellationTokenSource?.Cancel();
                            break;
                        }
                        else
                        {
                            _lockExpiration = sharedLock.Expiration.Value;
                            delay = lockServiceOptions.LockRenewalPeriod;
                        }
                    }
                    catch (Exception)
                    {
                        var approxOperationResult = DateTimeOffset.UtcNow + _lockServiceOptions.ClockSafePeriod + _lockServiceOptions.LockRetryDelay + _lockServiceOptions.OperationTimeout;
                        if (approxOperationResult < _lockExpiration)
                        {
                            delay = _lockServiceOptions.LockRetryDelay;
                            continue;
                        }
                        else
                        {
                            _cancellationTokenSource?.Cancel();
                            break;
                        }
                    }
                }
            }, internalCancellationToken);
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
                    await _lockStore.SharedUnlockAsync(_resourceId, _lockId);
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
