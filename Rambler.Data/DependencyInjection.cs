using Microsoft.Extensions.DependencyInjection;

namespace Rambler.Data
{
    public class DependencyInjection
    {
        public static void ConfigureDependencies(IServiceCollection services)
        {
            services.AddTransient<TwitchAPIv5>();
        }

    }
}
