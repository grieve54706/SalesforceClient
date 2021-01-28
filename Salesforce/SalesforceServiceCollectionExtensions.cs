using System;
using System.Text;
using Microsoft.Extensions.Configuration;
using sf_demo.Model;
using Microsoft.Extensions.DependencyInjection;

namespace sf_demo.Salesforce
{
    public static class SalesforceServiceCollectionExtensions
    {
        public static IServiceCollection AddSalesforceClient(this IServiceCollection services, IConfigurationRoot config)
        {
            SFEnvSetting sfenv = config.GetSection("sfenv").Get<SFEnvSetting>();

            if (sfenv == null)
                throw new System.Exception("No Salesforce Env Setting found");

            string pwd = Encoding.UTF8.GetString(Convert.FromBase64String(sfenv.Code));

            SalesforceClient.LoginEndpoint = "Prod".Equals(sfenv.Env) ? SalesforceClient.ProdLoginEndpoint : SalesforceClient.TestLoginEndpoint;
            SalesforceClient.ApiEndpoint = String.Format(SalesforceClient.ApiEndpoint, sfenv.ApiVersion);
            SalesforceClient.Username = sfenv.Username;
            SalesforceClient.Password = pwd;
            SalesforceClient.ClientId = sfenv.ClientId;
            SalesforceClient.ClientSecret = sfenv.ClientSecret;

            services.AddTransient(typeof(SalesforceClient));

            return services;
        }

    }
}