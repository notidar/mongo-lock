using CommandLine;

namespace Notidar.MongoDB.Lock.Sample.Commands.CombinedLocks
{
    [Verb("combined-locks", HelpText = "Run combined locks sample")]
    public class CombinedLocksOptions
    {
        [Option('w', "writes", Required = false, HelpText = "Amount of write operations to run")]
        public int WriteOperations { get; set; } = 10;

        [Option('r', "reads", Required = false, HelpText = "Amount of read operations per write to run")]
        public int ReadOperationsPerWrite { get; set; } = 10;
        [Option('d', "delay", Required = false, HelpText = "Time in seconds for operation")]
        public int OperationSeconds { get; set; } = 1;
        [Option('t', "threads", Required = false, HelpText = "Threads to run")]
        public int Threads { get; set; } = 3;
        [Option('s', "semaphore", Required = false, HelpText = "Semaphore limit")]
        public int SemaphoreCount { get; set; } = 2;
    }
}
