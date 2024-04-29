using Microsoft.Extensions.Logging;
using Notidar.MongoDB.Lock.Managers;

namespace Notidar.MongoDB.Lock.Sample.Commands.ExclusiveLocks
{
    public sealed class ExclusiveLocksCommand : ICommand<ExclusiveLocksOptions>
    {
        private readonly ILockManager _lockManager;
        private readonly ILogger<ExclusiveLocksCommand> _logger;
        public ExclusiveLocksCommand(ILockManager lockManager, ILogger<ExclusiveLocksCommand> logger)
        {
            _lockManager = lockManager ?? throw new ArgumentNullException(nameof(lockManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ExecuteAsync(ExclusiveLocksOptions options, CancellationToken cancellationToken = default)
        {
            int testData = -1;
            const string resourceId = "exclusive-lock-resource";
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
                    await using (var exclusiveLock = await _lockManager.ExclusiveLockAsync(resourceId, lockId, operationCancellationToken))
                    {
                        _logger.LogInformation("Operation {Index} locked", index);
                        testData = index;
                        await Task.Delay(TimeSpan.FromSeconds(options.OperationSeconds), operationCancellationToken);
                        if (testData != index)
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
