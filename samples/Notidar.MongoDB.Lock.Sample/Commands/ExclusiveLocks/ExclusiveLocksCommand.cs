using Microsoft.Extensions.Logging;
using Notidar.MongoDB.Lock.Services;

namespace Notidar.MongoDB.Lock.Sample.Commands.ExclusiveLocks
{
    public sealed class ExclusiveLocksCommand : ICommand<ExclusiveLocksOptions>
    {
        private readonly ILockService _lockService;
        private readonly ILogger<ExclusiveLocksCommand> _logger;
        public ExclusiveLocksCommand(ILockService lockService, ILogger<ExclusiveLocksCommand> logger)
        {
            _lockService = lockService ?? throw new ArgumentNullException(nameof(lockService));
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
                    await using (var exclusiveLock = await _lockService.ExclusiveLockAsync(resourceId, lockId, operationCancellationToken))
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
