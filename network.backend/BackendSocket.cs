using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Network.Backend
{
    public class BackendSocket
    {
        private static ManualResetEvent ManualResetEventTask = new ManualResetEvent(false);
        private readonly ILogger<BackendSocket> _logger;
        private readonly AppSettings _appSettings;
        private static int _sessionid = 0;

        public BackendSocket(ILogger<BackendSocket> logger, IOptionsMonitor<AppSettings> appSettingsOptions)
        {
            _logger = logger;
            _appSettings = appSettingsOptions.CurrentValue;
        }

        public void Listen(CancellationToken stoppingToken)
        {
            IPHostEntry host = Dns.GetHostEntry(_appSettings.HostName);
            IPAddress ipaddr = host.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipaddr, _appSettings.Port);

            Socket socket = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Bind(endpoint);
                socket.Listen(100);

                while (!stoppingToken.IsCancellationRequested)
                {
                    ManualResetEventTask.Reset();
                    _logger.LogInformation("Waiting for a connection...");
                    socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);

                    var accepted = ManualResetEventTask.WaitOne(_appSettings.Timeout * 1000);
                    if (!accepted) _logger.LogError("the connection is timeout.");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        private void AcceptCallback(IAsyncResult result)
        {
            ManualResetEventTask.Set();
            var id = Interlocked.Increment(ref _sessionid);
            Socket socket = (Socket)result.AsyncState;
            socket.EndAccept(out var buffer, out var transferred, result);
            _logger.LogInformation("{0} -> the connection {1} ,Accepted data transferred is {2} byte.", DateTimeOffset.Now.ToString(), id, transferred);
        }
    }
}