using Azure.Identity;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using YoutubeCategories.Repository;

[assembly: FunctionsStartup(typeof(YoutubeCategories.ReconcileVideoCategories.StartUp))]

namespace YoutubeCategories.ReconcileVideoCategories
{
    public class StartUp: FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().AddAzureKeyVault(new Uri("https://ccproject.vault.azure.net/"), new DefaultAzureCredential()).Build();
            string connectionString = Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTIONSTRING");
            var youTubeDataApiKey = configuration.GetValue<string>("youtubedataapikey") ?? Environment.GetEnvironmentVariable("youtubedataapikey");
            builder.Services.AddDbContext<ytvideoContext>(
                options => SqlServerDbContextOptionsExtensions.UseSqlServer(options, connectionString));
            
            var youtubeservice = new YouTubeService(new BaseClientService.Initializer
            {
                ApplicationName = "My test application",
                ApiKey = youTubeDataApiKey
            });

            builder.Services.AddSingleton(youtubeservice);
        }

    }
}
