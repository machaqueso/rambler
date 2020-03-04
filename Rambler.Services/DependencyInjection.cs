using Microsoft.Extensions.DependencyInjection;

namespace Rambler.Services
{
    public class DependencyInjection
    {
        public static void ConfigureDependencies(IServiceCollection services)
        {
            services.AddTransient<ConfigurationService>();
            services.AddTransient<ChannelService>();
            services.AddTransient<AccountService>();
            services.AddTransient<PasswordService>();
            services.AddTransient<BotService>();
            services.AddTransient<TwitchManager>();
            services.AddTransient<AuthorService>();
            services.AddTransient<WordFilterService>();
            services.AddTransient<ChatRulesService>();
            services.AddTransient<ChatMessageService>();
            services.AddTransient<MessageTemplateService>();
        }

    }
}
