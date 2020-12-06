using System.Threading;

namespace Network.Backend
{
    public interface IBackendSocket
    {
        void Listen(CancellationToken stoppingToken);
    }
}