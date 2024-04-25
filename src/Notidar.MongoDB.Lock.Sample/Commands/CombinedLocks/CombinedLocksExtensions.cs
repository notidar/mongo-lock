using Microsoft.Extensions.DependencyInjection;

namespace Notidar.MongoDB.Lock.Sample.Commands.CombinedLocks
{
    public static class CombinedLocksExtensions
    {
        public static IServiceCollection AddCombinedLocksCommand(this IServiceCollection services)
        {
            return services
                .AddScoped<ICommand<CombinedLocksOptions>, CombinedLocksCommand>();
        }
    }
}
