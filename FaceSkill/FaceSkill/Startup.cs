using FaceSkill;
using FaceSkill.Models;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

[assembly: FunctionsStartup(typeof(Startup))]
namespace FaceSkill
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            ConfigureServices(builder.Services);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();

            var config = new ConfigurationBuilder()
                    .SetBasePath(GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)  // common settings go here.
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)  // environment specific settings go here
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

            services.Configure<AppSettings>(config.GetSection(nameof(AppSettings)));
        }

        private static string GetCurrentDirectory()
        {
            var currentDirectory = string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")) ? Environment.CurrentDirectory
                : $@"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";

            return currentDirectory;
        }
    }
}
