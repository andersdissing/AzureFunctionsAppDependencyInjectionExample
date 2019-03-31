using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureFunctionsAppDependencyInjectionExample2;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Description;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;

// Configure the start up class
[assembly: WebJobsStartup(typeof(CustomWebJobsStartup))]

namespace AzureFunctionsAppDependencyInjectionExample2
{
    public static class DependencyInjectionExampleFunction2
    {
        [FunctionName("DIExample2")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "DIExample2/{number}")] HttpRequest req,
            int number,
            [Config] IConfiguration config,
            [DataRepository] IDataRepository data,
            ILogger log)
        {
            return new OkObjectResult(await data.GetData(number));
        }
    }

    public class CustomWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            var configurationBuilder = new ConfigurationBuilder();
            var descriptor = builder.Services.FirstOrDefault(d => d.ServiceType == typeof(IConfiguration));
            if (descriptor?.ImplementationInstance is IConfigurationRoot configuration)
            {
                configurationBuilder.AddConfiguration(configuration);
            }

            // example on load configuration e.g. connection string etc.
            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // example on DI e.g. AddDbContext
            var serviceProvider = builder.Services
                .AddSingleton<IDataRepository, DataRepository>()
                .BuildServiceProvider(true);

            builder.Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));

            builder.AddExtension<DataRepositoryProvider>();
            builder.AddExtension<ConfigProvider>();
            
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

    public class ConfigProvider : IExtensionConfigProvider
    {
        private readonly IConfiguration _config;

        public ConfigProvider(IConfiguration config)
        {
            _config = config;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            context.AddBindingRule<ConfigAttribute>().BindToInput(_ => _config);
        }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    [Binding]
    public sealed class ConfigAttribute : Attribute
    {

    }

    public class DataRepositoryProvider : IExtensionConfigProvider
    {
        private readonly IDataRepository _dataRepository;

        public DataRepositoryProvider(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        public void Initialize(ExtensionConfigContext context)
        {
            context.AddBindingRule<DataRepositoryAttribute>().BindToInput(s => s.DataRepository = _dataRepository);
        }
    }

    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    [Binding]
    public sealed class DataRepositoryAttribute : Attribute
    {
        public IDataRepository DataRepository;
    }
}