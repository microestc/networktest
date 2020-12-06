using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Network.Backend
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly BackendSocket _backend;

        public Worker(ILogger<Worker> logger, BackendSocket backend)
        {
            _logger = logger;
            _backend = backend;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _backend.Listen(stoppingToken);
            return Task.CompletedTask;
        }
    }
}
