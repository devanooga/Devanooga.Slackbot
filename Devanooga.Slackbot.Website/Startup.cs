namespace Devanooga.Slackbot.Website
{
    using System;
    using System.Threading.Tasks;
    using Authorization;
    using Data.Entity;
    using DependencyInjection;
    using Hangfire;
    using Hangfire.PostgreSql;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using RollbarDotNet.Configuration;
    using RollbarDotNet.Core;
    using RollbarDotNet.Logger;
    using Slack;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services
                .AddHangfire(x => x.UsePostgreSqlStorage(Configuration.GetConnectionString("Default")))
                .AddRollbarWeb()
                .AddOptions()
                .Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = 
                        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    options.KnownNetworks.Clear();
                    options.KnownProxies.Clear();
                })
                .Configure<SlackBotOptions>(o => Configuration.GetSection("SlackBot").Bind(o))
                .Configure<RollbarOptions>(o => Configuration.GetSection("Rollbar").Bind(o))
                .AddDevanoogaSlackbot(Configuration)
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddRazorPagesOptions(options =>
                {
                    options.AllowAreas = true;
                    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
                });
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });
            return services
                .BuildServiceProvider();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<SlackBotContext>())
                {
                    context.Database.Migrate();
                }

                var slackBotClient = serviceScope.ServiceProvider.GetService<SlackBotClient>();
                Task.Run(async () => await slackBotClient.Connect());
            }

            app
                .UseForwardedHeaders();
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app
                .UseHangfireServer()
                .UseHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] {new HangfireAuthorizationFilter()}
                })
                .UseHttpsRedirection()
                .UseStaticFiles()
                .UseAuthentication()
                .UseMvc();
            loggerFactory.AddRollbarDotNetLogger(app.ApplicationServices);
        }
    }
}