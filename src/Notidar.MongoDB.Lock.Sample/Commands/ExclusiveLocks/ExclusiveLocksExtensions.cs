using Microsoft.Extensions.DependencyInjection;

namespace Notidar.MongoDB.Lock.Sample.Commands.ExclusiveLocks
{
    public static class ExclusiveLocksExtensions
    {
        public static IServiceCollection AddExclusiveLocksCommand(this IServiceCollection services)
        {
            return services
                .AddScoped<ICommand<ExclusiveLocksOptions>, ExclusiveLocksCommand>();
        }
    }
}
