namespace Devanooga.Slackbot.DependencyInjection
{
    using Data.Entity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Slack;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDevanoogaSlackbot(this IServiceCollection services, IConfiguration configuration)
        {
            return services
                .AddEntityFrameworkNpgsql()
                .AddDbContext<SlackBotContext>(options =>
                    options.UseNpgsql(
                        configuration.GetConnectionString("Default"),
                        o => o.MigrationsAssembly("Devanooga.Slackbot.Data.Entity")))
                .AddSingleton<SlackBotClient>();
        }
    }
}