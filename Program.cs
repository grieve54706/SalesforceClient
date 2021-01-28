using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;
using sf_demo.Salesforce;

namespace sf_demo
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            using (var svcs = ConfigureServices(new ServiceCollection(), ReadFromAppSettings()).BuildServiceProvider())
            {
                App app = svcs.GetRequiredService<App>();
                await app.Run();
            }
        }

        public static IConfigurationRoot ReadFromAppSettings()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional:true)
                .Build();
        }

        private static IServiceCollection ConfigureServices(IServiceCollection services, IConfigurationRoot config)
        {
            return services
            .AddTransient<App>()
            .AddSalesforceClient(config);
        }
    }
}
