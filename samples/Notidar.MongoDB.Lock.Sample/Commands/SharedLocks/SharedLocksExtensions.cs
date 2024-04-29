using Microsoft.Extensions.DependencyInjection;

namespace Notidar.MongoDB.Lock.Sample.Commands.SharedLocks
{
    public static class SharedLocksExtensions
    {
        public static IServiceCollection AddSharedLocksCommand(this IServiceCollection services)
        {
            return services
                .AddScoped<ICommand<SharedLocksOptions>, SharedLocksCommand>();
        }
    }
}
