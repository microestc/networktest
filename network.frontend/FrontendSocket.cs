using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Network
{
    public class FrontendSocket
    {
        private readonly ManualResetEvent ManualResetEventConnect = new ManualResetEvent(false);
        private readonly ILogger<FrontendSocket> _logger;
        private readonly AppSettings _appSettings;
        private static int _sessionid = 0;

        public FrontendSocket(ILogger<FrontendSocket> logger, IOptionsMonitor<AppSettings> appSettingsOptions)
        {
            _logger = logger;
            _appSettings = appSettingsOptions.CurrentValue;
        }

        public void OnceConnect()
        {
            try
            {
                var enaddr = IPAddress.TryParse(_appSettings.HostName, out var ipaddr);
                if (!enaddr) throw new NotSupportedException("the hostname is incorrect.");
                IPEndPoint endpoint = new IPEndPoint(ipaddr, _appSettings.Port);

                Socket socket = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                socket.BeginConnect(endpoint, new AsyncCallback(ConnectCallback), socket);
                var accepted = ManualResetEventConnect.WaitOne(_appSettings.Timeout * 1000);
                if (!accepted)
                {
                    _logger.LogWarning("the connection is timeout.");
                }
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                ManualResetEventConnect.Set();
                Socket socket = (Socket)result.AsyncState;
                socket.EndConnect(result);
                var id = Interlocked.Increment(ref _sessionid);
                _logger.LogInformation("the connect {0},socket connected to {1}.", id, socket.RemoteEndPoint.ToString());
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }
    }
}