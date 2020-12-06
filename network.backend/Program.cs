using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Network.Backend
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
                    services.AddSingleton<BackendSocket>();
                    services.AddOptions<AppSettings>("AppSettings");
                    services.AddHostedService<Worker>();
                });
    }
}
