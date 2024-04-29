using CommandLine;

namespace Notidar.MongoDB.Lock.Sample.Commands.LongLocks
{
    [Verb("long-locks", HelpText = "Run long locks sample")]
    public class LongLocksOptions
    {
        [Option('w', "wait", Required = false, HelpText = "Wait in seconds")]
        public int WaitSeconds { get; set; } = 300;
    }
}
