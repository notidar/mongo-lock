using Microsoft.Extensions.DependencyInjection;

namespace Notidar.MongoDB.Lock.Sample.Commands.SemaphoreLocks
{
    public static class SemaphoreLocksExtensions
    {
        public static IServiceCollection AddSemaphoreLocksCommand(this IServiceCollection services)
        {
            return services
                .AddScoped<ICommand<SemaphoreLocksOptions>, SemaphoreLocksCommand>();
        }
    }
}
