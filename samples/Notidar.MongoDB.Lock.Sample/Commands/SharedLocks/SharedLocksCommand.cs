using Microsoft.Extensions.Logging;
using Notidar.MongoDB.Lock.Services;

namespace Notidar.MongoDB.Lock.Sample.Commands.SharedLocks
{
    public sealed class SharedLocksCommand : ICommand<SharedLocksOptions>
    {
        private readonly ILockService _lockService;
        private readonly ILogger<SharedLocksCommand> _logger;
        public SharedLocksCommand(ILockService lockService, ILogger<SharedLocksCommand> logger)
        {
            _lockService = lockService ?? throw new ArgumentNullException(nameof(lockService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync(SharedLocksOptions options, CancellationToken cancellationToken = default)
        {
            int testData = 0;
            const string resourceId = "shared-lock-resource";
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
                    await using (var sharedLock = await _lockService.SharedLockAsync(resourceId, lockId, operationCancellationToken))
                    {
                        _logger.LogInformation("Operation {Index} locked", index);
                        
                        if (Interlocked.Increment(ref testData) > options.Threads)
                        {
                            _logger.LogError("Data corruption detected");
                            throw new InvalidOperationException("Data corruption detected");
                        }

                        await Task.Delay(TimeSpan.FromSeconds(options.OperationSeconds), operationCancellationToken);
                        
                        if (Interlocked.Decrement(ref testData) + 1 > options.Threads)
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
