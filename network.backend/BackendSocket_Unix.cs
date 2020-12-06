using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Network.Backend
{
    [SupportedOSPlatform("Linux")]
    public class UnixBackendSocket : IBackendSocket
    {
        private static ManualResetEvent ManualResetEventTask = new ManualResetEvent(false);
        private readonly ILogger<UnixBackendSocket> _logger;
        private readonly AppSettings _appSettings;
        private static int _sessionid = 0;

        public UnixBackendSocket(ILogger<UnixBackendSocket> logger, IOptionsMonitor<AppSettings> appSettingsOptions)
        {
            _logger = logger;
            _appSettings = appSettingsOptions.CurrentValue;
        }

        public void Listen(CancellationToken stoppingToken)
        {
            var enaddr = IPAddress.TryParse(_appSettings.HostName, out var ipaddr);
            if (!enaddr) throw new NotSupportedException("the hostname is incorrect.");
            IPEndPoint endpoint = new IPEndPoint(ipaddr, _appSettings.Port);

            Socket socket = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Bind(endpoint);
                socket.Listen(100);

                while (!stoppingToken.IsCancellationRequested)
                {
                    ManualResetEventTask.Reset();
                    var acceptEventArg = new SocketAsyncEventArgs();
                    acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptEventArgCompleted);

                    var accepted = ManualResetEventTask.WaitOne(_appSettings.Timeout * 1000);
                    if (!accepted) _logger.LogError("the connection is timeout.");
                    accepted = socket.AcceptAsync(acceptEventArg);
                    if (!accepted) ProcessAccept(acceptEventArg);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        private void AcceptEventArgCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            var id = Interlocked.Increment(ref _sessionid);
            _logger.LogInformation("the connection {0} ,Accepted data transferred is {1} byte.", id, transferred);
            

            
        }
    }
}