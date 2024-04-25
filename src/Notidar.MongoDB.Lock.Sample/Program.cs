using CommandLine;
using Notidar.MongoDB.Lock.Sample.Commands.CombinedLocks;
using Notidar.MongoDB.Lock.Sample.Commands.ExclusiveLocks;
using Notidar.MongoDB.Lock.Sample.Commands.LongLocks;
using Notidar.MongoDB.Lock.Sample.Commands.SemaphoreLocks;
using Notidar.MongoDB.Lock.Sample.Commands.SharedLocks;

namespace Notidar.MongoDB.Lock.Sample
{
    internal class Program
    {
        static int Main(string[] args)
        {
            using var cliHost = new CliHost();

            return Parser.Default
                .ParseArguments<ExclusiveLocksOptions, SharedLocksOptions, SemaphoreLocksOptions, CombinedLocksOptions, LongLocksOptions>(args)
                .MapResult(
                    (ExclusiveLocksOptions options) => cliHost.RunCommand(options),
                    (SharedLocksOptions options) => cliHost.RunCommand(options),
                    (SemaphoreLocksOptions options) => cliHost.RunCommand(options),
                    (CombinedLocksOptions options) => cliHost.RunCommand(options),
                    (LongLocksOptions options) => cliHost.RunCommand(options),
                    errs => 1);
        }
    }
}
