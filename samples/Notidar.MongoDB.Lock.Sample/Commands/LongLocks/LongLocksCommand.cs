using Microsoft.Extensions.Logging;
using Notidar.MongoDB.Lock.Services;
using Notidar.MongoDB.Lock.Stores;

namespace Notidar.MongoDB.Lock.Sample.Commands.LongLocks
{
    public sealed class LongLocksCommand : ICommand<LongLocksOptions>
    {
        private readonly ILockService _lockService;
        private readonly ILockStore _lockStore;
        private readonly ILogger<LongLocksCommand> _logger;
        public LongLocksCommand(ILockService lockService, ILockStore lockStore, ILogger<LongLocksCommand> logger)
        {
            _lockService = lockService ?? throw new ArgumentNullException(nameof(lockService));
            _lockStore = lockStore ?? throw new ArgumentNullException(nameof(lockStore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync(LongLocksOptions options, CancellationToken cancellationToken = default)
        {
            const string resourceId = "long-lock-resource";
            const string sharedLockId = "shared-lock-id";
            const string exclusiveLockId = "exclusive-lock-id";

            using var sharedLockCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(options.WaitSeconds));
            using var sharedCombinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, sharedLockCancellationTokenSource.Token);

            await using (var sharedLock = await _lockService.SharedLockAsync(resourceId, sharedLockId, sharedCombinedCancellationTokenSource.Token))
            {
                _logger.LogInformation("{lock} locked", sharedLockId);
                try
                {
                    await Task.Delay(-1, sharedLock.HealthToken);
                }
                catch (Exception)
                {
                    _logger.LogInformation("{lock} lock expired", sharedLockId);
                }
                var resource = await _lockStore.GetResourceAsync(resourceId, cancellationToken);
                if (resource!.SharedLocks!.Single(x => x.LockId == sharedLockId).Expiration < DateTimeOffset.UtcNow)
                {
                    _logger.LogError("Lock was not prolonged");
                    throw new InvalidOperationException("Lock was not prolonged");
                }
            }

            using var exclusiveLockCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(options.WaitSeconds));
            using var exclusiveCombinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, exclusiveLockCancellationTokenSource.Token);

            await using (var exclusiveLock = await _lockService.ExclusiveLockAsync(resourceId, exclusiveLockId, exclusiveCombinedCancellationTokenSource.Token))
            {
                _logger.LogInformation("{lock} locked", exclusiveLockId);
                try
                {
                    await Task.Delay(-1, exclusiveLock.HealthToken);
                }
                catch (Exception)
                {
                    _logger.LogInformation("{lock} lock expired", exclusiveLockId);
                }
                var resource = await _lockStore.GetResourceAsync(resourceId, cancellationToken);
                if (resource!.ExclusiveLock!.LockId != exclusiveLockId || resource!.ExclusiveLock!.Expiration < DateTimeOffset.UtcNow)
                {
                    _logger.LogError("Lock was not prolonged");
                    throw new InvalidOperationException("Lock was not prolonged");
                }
            }
        }
    }
}
