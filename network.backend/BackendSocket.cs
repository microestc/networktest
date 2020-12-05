using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Network.Backend
{
    public class BackendSocket
    {
        public static ManualResetEvent ManualResetEventTask = new ManualResetEvent(false);
        private static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
        private readonly ILogger<BackendSocket> _logger;

        public void Listen()
        {
            IPHostEntry iphost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipaddr = iphost.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(ipaddr, 11000);

            Socket socket = new Socket(ipaddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                socket.Bind(endpoint);
                socket.Listen(100);

                while (true)
                {
                    ManualResetEventTask.Reset();
                    _logger.LogInformation("Waiting for a connection...");
                    socket.BeginAccept(new AsyncCallback(AcceptCallback), socket);

                    ManualResetEventTask.WaitOne();
                }

            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        public void AcceptCallback(IAsyncResult result)
        {
            ManualResetEventTask.Set();
            Socket socket = (Socket)result.AsyncState;
            Socket handler = socket.EndAccept(out var buffer, out var transferred, result);
            _logger.LogInformation("Accepted data transferred is {0} byte.", transferred);
            var state = new StateContainer(handler);
            handler.BeginReceive(state.Buffer, 0, StateContainer.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }

        public static void ReceiveCallback(IAsyncResult result)
        {
            var content = string.Empty;
            var state = (StateContainer)result.AsyncState;
            Socket socket = state.Socket;
            int bytesRead = socket.EndReceive(result);

            if (bytesRead > 0)
            {
                var readspan = new ReadOnlySpan<byte>(state.Buffer);
                var data = JsonSerializer.Deserialize<NetworkPacket>(readspan, JsonSerializerOptions);
                content = state.sb.ToString();
                if (content.IndexOf("<EOF>") > -1)
                {
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}", content.Length, content);
                }
                else
                {
                    socket.BeginReceive(state.Buffer, 0, StateContainer.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }
        }

        private void Send(Socket socket, NetworkPacket data)
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes<NetworkPacket>(data, JsonSerializerOptions);
            socket.BeginSend(bytes, 0, bytes.Length, 0, new AsyncCallback(SendCallback), socket);
        }

        private void SendCallback(IAsyncResult result)
        {
            try
            {
                Socket socket = (Socket)result.AsyncState;
                int bytesSent = socket.EndSend(result);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString());
            }
        }

        public class StateContainer
        {
            public Socket Socket = null;
            public const int BufferSize = 1024;
            public byte[] Buffer = new byte[BufferSize];
            public StringBuilder sb = new StringBuilder();

            public StateContainer(Socket socket)
            {
                Socket = socket;
            }
        }
    }
}