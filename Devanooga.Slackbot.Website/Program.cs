namespace Devanooga.Slackbot.Website
{
    using System.IO;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) => WebHost
            .CreateDefaultBuilder(args)
            .UseConfiguration(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddCommandLine(args)
                .AddJsonFile("appsettings.local.json", true, true)
                .Build()
            )
            .ConfigureAppConfiguration((builderContext, config) =>
            {
                config.AddJsonFile("appsettings.local.json", true, true);
            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
            })
            .UseStartup<Startup>()
            .Build();
    }
}