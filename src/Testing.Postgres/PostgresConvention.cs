using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.LocalDevelopment;
using Rocket.Surgery.Conventions;
using Rocket.Surgery.Extensions.Configuration;
using Rocket.Surgery.Extensions.DependencyInjection;

namespace Rocket.Surgery.LocalDevelopment
{
    public class DatabaseConvention : IConfigurationConvention
    {
        public void Register(IConfigurationConventionContext context)
        {
            var automation = PostgresAutomation.ForLocalDevelopment();
            automation.Start().Wait();
            RoundhouseAutomation.ForLocalDevelopment().Start(automation.ConnectionString.ToString()).Wait();
            context.AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"postgres:connectionstring", automation.ConnectionString.ToString()},
            });
        }
    }
}
