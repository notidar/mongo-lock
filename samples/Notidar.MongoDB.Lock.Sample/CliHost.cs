using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notidar.MongoDB.Lock.Extensions;
using Notidar.MongoDB.Lock.Services;
using Notidar.MongoDB.Lock.Sample.Commands;
using Notidar.MongoDB.Lock.Sample.Commands.CombinedLocks;
using Notidar.MongoDB.Lock.Sample.Commands.ExclusiveLocks;
using Notidar.MongoDB.Lock.Sample.Commands.LongLocks;
using Notidar.MongoDB.Lock.Sample.Commands.SemaphoreLocks;
using Notidar.MongoDB.Lock.Sample.Commands.SharedLocks;

namespace Notidar.MongoDB.Lock.Sample
{
    public sealed class CliHost : IDisposable
    {
        private readonly IHost _host;

        public CliHost(string[]? args = null)
        {
            _host = Host
                .CreateDefaultBuilder(args)
                .ConfigureServices((context, services) => ConfigureServices(services, context.Configuration))
                .Build();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddMongoDb(configuration.GetSection(nameof(DatabaseOptions)))
                .AddMongoLocks(configuration.GetSection(nameof(LockSettings)));

            services
                .AddExclusiveLocksCommand()
                .AddSharedLocksCommand()
                .AddSemaphoreLocksCommand()
                .AddCombinedLocksCommand()
                .AddLongLocksCommand();
        }

        public async Task<int> RunCommandAsync<TCommandOptions>(TCommandOptions options, CancellationToken cancellationToken = default)
        {
            await _host.StartAsync(cancellationToken);
            using var scope = _host.Services.CreateScope();
            var command = scope.ServiceProvider.GetRequiredService<ICommand<TCommandOptions>>();
            await command.ExecuteAsync(options, cancellationToken);
            await _host.StopAsync(cancellationToken);
            return 0;
            
        }

        public int RunCommand<TCommandOptions>(TCommandOptions options)
        {
            return RunCommandAsync(options).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}
