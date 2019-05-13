using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureFunctionsAppDependencyInjectionExample1;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Description;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

// Configure the start up class
[assembly: FunctionsStartup(typeof(CustomWebJobsStartup))]

namespace AzureFunctionsAppDependencyInjectionExample1
{
    public class DependencyInjectionExampleFunction1
    {
        private readonly IDataRepository _data;
        public DependencyInjectionExampleFunction1(IDataRepository data)
        {
            _data = data;
        }

        [FunctionName("DIExample1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "DIExample1/{number}")] HttpRequest req,
            int number,
            ILogger log)
        {
            return new OkObjectResult(await _data.GetData(number));
        }
    }

    public class CustomWebJobsStartup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // example on load configuration e.g. connection string etc.
            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // example on DI e.g. AddDbContext Or AddAzureKeyVault
            builder.Services
                .AddScoped<IDataRepository, DataRepository>()
                .BuildServiceProvider(true);
        }
    }

    public interface IDataRepository
    {
        Task<int> GetData(int number);
    }

    public class DataRepository : IDataRepository
    {
        public Task<int> GetData(int number)
        {
            return Task.FromResult(number + 1);
        }
    }
}