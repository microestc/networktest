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
        private static Semaphore SemaphoreTask = new Semaphore(1, 100);
        private readonly ILogger<UnixBackendSocket> _logger;
        private readonly AppSettings _appSettings;

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

            var socket = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Bind(endpoint);
                socket.Listen(100);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var acceptEventArg = new SocketAsyncEventArgs();
                    acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(Accepting);

                    SemaphoreTask.WaitOne();
                    var status = socket.AcceptAsync(acceptEventArg);
                    if (!status)
                    {
                        AcceptCompleted(acceptEventArg);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        private void Accepting(object sender, SocketAsyncEventArgs e)
        {
            AcceptCompleted(e);
        }

        private void AcceptCompleted(SocketAsyncEventArgs e)
        {
            if(e.SocketError == SocketError.Success)
            {
                _logger.LogInformation("接收客户端请求成功,连接已建立,接收到数据 {0}.",e.BytesTransferred);
                CloseSocket(e);
            }
        }

        private void CloseSocket(SocketAsyncEventArgs e)
        {
            try
            {
                e.AcceptSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {
                e.AcceptSocket.Close();
                SemaphoreTask.Release();
            }
        }
    }
}