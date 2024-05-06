using Microsoft.Extensions.Logging;
using Notidar.MongoDB.Lock.Services;

namespace Notidar.MongoDB.Lock.Sample.Commands.CombinedLocks
{
    public sealed class CombinedLocksCommand : ICommand<CombinedLocksOptions>
    {
        private readonly ILockService _lockService;
        private readonly ILogger<CombinedLocksCommand> _logger;
        public CombinedLocksCommand(ILockService lockService, ILogger<CombinedLocksCommand> logger)
        {
            _lockService = lockService ?? throw new ArgumentNullException(nameof(lockService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync(CombinedLocksOptions options, CancellationToken cancellationToken = default)
        {
            int exclusiveLockId = -1;
            int lockCounter = 0;
            int totalOperations = options.WriteOperations * options.ReadOperationsPerWrite;
            int possibleSharedLockMax = options.SemaphoreCount > options.Threads ? options.SemaphoreCount : options.Threads;
            const string resourceId = "combined-lock-resource";

            GC.Collect();
            GC.Collect();

            await Parallel.ForAsync(
                fromInclusive: 0,
                toExclusive: totalOperations,
                parallelOptions: new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = options.Threads,
                },
                body: async (index, operationCancellationToken) =>
                {
                    bool isWriteOperation = index % options.ReadOperationsPerWrite == 0;
                    var lockId = index.ToString();
                    _logger.LogInformation("Operation {Index} started", index);

                    if (isWriteOperation)
                    {
                        await using (var exclusiveLock = await _lockService.ExclusiveLockAsync(resourceId, lockId, operationCancellationToken))
                        {
                            _logger.LogInformation("Operation {Index} locked", index);
                            if (Interlocked.Increment(ref lockCounter) > 1)
                            {
                                _logger.LogError("Data corruption detected");
                                throw new InvalidOperationException("Data corruption detected");
                            }
                            if (Interlocked.CompareExchange(ref exclusiveLockId, index, -1) != -1)
                            {
                                _logger.LogError("Data corruption detected");
                                throw new InvalidOperationException("Data corruption detected");
                            }

                            await Task.Delay(TimeSpan.FromSeconds(options.OperationSeconds), operationCancellationToken);
                            if (Interlocked.Decrement(ref lockCounter) != 0)
                            {
                                _logger.LogError("Data corruption detected");
                                throw new InvalidOperationException("Data corruption detected");
                            }
                            if (Interlocked.CompareExchange(ref exclusiveLockId, -1, index) != index)
                            {
                                _logger.LogError("Data corruption detected");
                                throw new InvalidOperationException("Data corruption detected");
                            }
                        }
                        _logger.LogInformation("Operation {Index} completed", index);
                    }
                    else
                    {
                        await using (var sharedLock = await _lockService.SharedLockAsync(resourceId, lockId, options.SemaphoreCount, operationCancellationToken))
                        {
                            _logger.LogInformation("Operation {Index} locked", index);
                            if (Interlocked.Increment(ref lockCounter) > possibleSharedLockMax)
                            {
                                _logger.LogError("Data corruption detected");
                                throw new InvalidOperationException("Data corruption detected");
                            }
                            if (exclusiveLockId != -1)
                            {
                                _logger.LogError("Data corruption detected");
                                throw new InvalidOperationException("Data corruption detected");
                            }

                            await Task.Delay(TimeSpan.FromSeconds(options.OperationSeconds), operationCancellationToken);
                            if (Interlocked.Decrement(ref lockCounter) > (possibleSharedLockMax - 1))
                            {
                                _logger.LogError("Data corruption detected");
                                throw new InvalidOperationException("Data corruption detected");
                            }
                            if (exclusiveLockId != -1)
                            {
                                _logger.LogError("Data corruption detected");
                                throw new InvalidOperationException("Data corruption detected");
                            }
                        }
                        _logger.LogInformation("Operation {Index} completed", index);
                    }
                });

            await Task.Delay(TimeSpan.FromSeconds(20));

            GC.Collect();
            GC.Collect();
        }
    }
}
