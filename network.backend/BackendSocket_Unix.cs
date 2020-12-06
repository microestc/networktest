using System.Runtime.Versioning;
using System.Threading;

namespace Network.Backend
{
    [SupportedOSPlatform("Linux")]
    public class UnixBackendSocket : IBackendSocket
    {
        public void Listen(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }
    }
}