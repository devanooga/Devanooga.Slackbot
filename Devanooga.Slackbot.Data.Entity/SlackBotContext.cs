namespace Devanooga.Slackbot.Data.Entity
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Models;

    public class SlackBotContext : IdentityDbContext<User, Role, int>
    {
        public SlackBotContext(DbContextOptions<SlackBotContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var types = typeof(SlackBotContext).GetTypeInfo()
                .Assembly
                .GetTypes()
                .Where(t => t.GetTypeInfo().IsClass && !t.GetTypeInfo().IsAbstract
                                                    && t.GetInterfaces().Any(i => i.GetTypeInfo().IsGenericType &&
                                                                                  i.GetGenericTypeDefinition() ==
                                                                                  typeof(IEntityTypeConfiguration<>)))
                .ToList();
            foreach (var type in types)
            {
                dynamic model = Activator.CreateInstance(type);
                modelBuilder.ApplyConfiguration(model);
            }
        }
    }
}