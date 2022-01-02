using System;
using System.Threading.Tasks;
using Logging.Services.Abstractions;
using Logging.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Logging
{
    public class Startup
    {
        public async Task Run()
        {
            var serviceProvider = ConfigureServices();
            var starter = serviceProvider?.GetService<Application>();
            await starter?.Run();
        }

        private IServiceProvider ConfigureServices()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IConfigServices, ConfigServices>();
            serviceCollection.AddSingleton<ILogger, Logger>();
            serviceCollection.AddSingleton<IBackupServices, BackupServices>();
            serviceCollection.AddTransient<IFileServices, FileServices>();
            serviceCollection.AddTransient<Application>();
            return serviceCollection.BuildServiceProvider();
        }
    }
}
