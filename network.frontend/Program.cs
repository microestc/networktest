using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace Network.Frontend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    var logger = LogManager.Setup()
                        .SetupExtensions(s => s.AutoLoadAssemblies(false))
                        .SetupExtensions(s => s.RegisterConfigSettings(configuration))
                        .LoadConfigurationFromSection(configuration)
                        .GetCurrentClassLogger();
                    services.AddTransient<FrontendSocket>();
                    services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
                    services.AddHostedService<Worker>();
                    services.AddLogging(builder =>
                    {
                        builder.ClearProviders();
                        builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                        builder.AddNLog(configuration);
                    });
                });
    }
}
