using Microsoft.Extensions.DependencyInjection;

namespace Notidar.MongoDB.Lock.Sample.Commands.LongLocks
{
    public static class LongLocksExtensions
    {
        public static IServiceCollection AddLongLocksCommand(this IServiceCollection services)
        {
            return services
                .AddScoped<ICommand<LongLocksOptions>, LongLocksCommand>();
        }
    }
}
