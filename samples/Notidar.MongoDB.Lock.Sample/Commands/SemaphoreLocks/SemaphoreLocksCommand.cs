using Microsoft.Extensions.Logging;
using Notidar.MongoDB.Lock.Managers;

namespace Notidar.MongoDB.Lock.Sample.Commands.SemaphoreLocks
{
    public sealed class SemaphoreLocksCommand : ICommand<SemaphoreLocksOptions>
    {
        private readonly ILockManager _lockManager;
        private readonly ILogger<SemaphoreLocksCommand> _logger;
        public SemaphoreLocksCommand(ILockManager lockManager, ILogger<SemaphoreLocksCommand> logger)
        {
            _lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync(SemaphoreLocksOptions options, CancellationToken cancellationToken = default)
        {
            int testData = 0;
            const string resourceId = "semaphore-lock-resource";
            await Parallel.ForAsync(
                fromInclusive: 0,
                toExclusive: options.OperationsCount,
                parallelOptions: new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = options.Threads,
                },
                body: async (index, operationCancellationToken) =>
                {
                    var lockId = index.ToString();
                    _logger.LogInformation("Operation {Index} started", index);
                    await using (var exclusiveLock = await _lockManager.SharedLockAsync(resourceId, lockId, options.SemaphoreCount, operationCancellationToken))
                    {
                        _logger.LogInformation("Operation {Index} locked", index);
                        
                        if (Interlocked.Increment(ref testData) > options.SemaphoreCount)
                        {
                            _logger.LogError("Data corruption detected");
                            throw new InvalidOperationException("Data corruption detected");
                        }

                        await Task.Delay(TimeSpan.FromSeconds(options.OperationSeconds), operationCancellationToken);
                        
                        if (Interlocked.Decrement(ref testData) + 1 > options.SemaphoreCount)
                        {
                            _logger.LogError("Data corruption detected");
                            throw new InvalidOperationException("Data corruption detected");
                        }
                    }
                    _logger.LogInformation("Operation {Index} completed", index);
                });
        }
    }
}
