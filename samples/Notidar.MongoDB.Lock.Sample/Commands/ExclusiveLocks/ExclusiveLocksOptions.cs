using CommandLine;

namespace Notidar.MongoDB.Lock.Sample.Commands.ExclusiveLocks
{
    [Verb("exclusive-locks", HelpText = "Run exclusive locks sample")]
    public class ExclusiveLocksOptions
    {
        [Option('o', "operations", Required = false, HelpText = "Amount of operations to run")]
        public int OperationsCount { get; set; } = 10;
        [Option('d', "delay", Required = false, HelpText = "Time in seconds for operations")]
        public int OperationSeconds { get; set; } = 1;
        [Option('t', "threads", Required = false, HelpText = "Threads to run")]
        public int Threads { get; set; } = 3;
    }
}
