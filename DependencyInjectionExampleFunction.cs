using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureFunctionsAppDependencyInjectionExample;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

// Configure the start up class
[assembly: WebJobsStartup(typeof(CustomWebJobsStartup))]

namespace AzureFunctionsAppDependencyInjectionExample
{
    public class DependencyInjectionExampleFunction
    {
        private readonly IDataRepository _data;
        public DependencyInjectionExampleFunction(IDataRepository data)
        {
            _data = data;
        }

        [FunctionName("DIExample")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "DIExample/{number}")] HttpRequest req,
            int number,
            ILogger log)
        {
            return new OkObjectResult(await _data.GetData(number));
        }
    }

    public class CustomWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            // example on load configuration e.g. connection string etc.
            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // example on DI e.g. AddDbContext
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