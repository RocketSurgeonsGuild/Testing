using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Rocket.Surgery.Extensions.Configuration;

namespace Rocket.Surgery.LocalDevelopment
{
    public class AzuriteConvention : IConfigurationConvention
    {
        public void Register(IConfigurationConventionContext context)
        {
            var automation = AzuriteAutomation.ForLocalDevelopment();
            automation.Start().Wait();
            context.AddInMemoryCollection(new Dictionary<string, string>()
            {
                {"AzureWebJobsStorage", automation.ConnectionString},
                {"AzureWebJobsDashboard", automation.ConnectionString},
                {"GeneralStorage:ConnectionString", automation.ConnectionString},
            });
        }
    }
}
