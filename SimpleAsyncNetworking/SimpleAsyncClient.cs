using SimpleAsyncNetworking.Events;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleAsyncNetworking
{
    /// <summary>
    /// Implements an asynchronous TCP client
    /// </summary>
    public class SimpleAsyncClient : IDisposable
    {
        #region Delegates & Events
        public delegate void SACEventHandler<TEventArgs>(SimpleAsyncClient client, TEventArgs args);
        public delegate void SACEventHandler(SimpleAsyncClient client);

        /// <summary>
        /// On Connect Event, called when a SimpleAsyncClient connects to a remote host.
        /// </summary>
        public event SACEventHandler OnConnect;

        /// <summary>
        /// On Disconnect Event, called when a SimpleAsyncClient disconnects from the remote host.
        /// </summary>
        public event SACEventHandler OnDisconnect;

        /// <summary>
        /// On Message Received Event, called when a message is completely received from the remote host.
        /// </summary>
        public event SACEventHandler<SACMessageReceivedEventArgs> OnMessageReceived;

        /// <summary>
        /// On Error Event, called when a SimpleAsyncClient encounters an error while managing the connection.
        /// </summary>
        public event SACEventHandler<SACErrorEventArgs> OnError;
        #endregion

        private TcpClient _tcpClient;
        private CancellationTokenSource _cancellation;
        private int _bufferSize;
        
        /// <summary>
        /// The remote endpoint the SimpleAsyncClient is connected to.
        /// </summary>
        public IPEndPoint RemoteEndPoint
        {
            get
            {
                return (IPEndPoint)_tcpClient.Client.RemoteEndPoint;
            }
        }

        /// <summary>
        /// The local endpoint of the SimpleAsyncClient
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get
            {
                return (IPEndPoint)_tcpClient.Client.LocalEndPoint;
            }
        }
        
        /// <summary>
        /// Creates a new SimpleAsyncClient with the specified buffer size.
        /// </summary>
        /// <param name="bufferSize"></param>
        public SimpleAsyncClient(int bufferSize = 8192)
        {
            _cancellation = new CancellationTokenSource();
            _bufferSize = bufferSize;
        }

        /// <summary>
        /// Connects to the remote host.
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public void Connect(string hostname, int port)
        {
            _tcpClient = new TcpClient();
            ClientTask(hostname, port).FireAndForget();
        }

        /// <summary>
        /// Disconnects from the remote host.
        /// </summary>
        public void Disconnect()
        {
            _cancellation.Cancel();
        }

        /// <summary>
        /// Sends data to the remote host and automatically frames the message.
        /// </summary>
        /// <param name="message"></param>
        public void Send(byte[] message)
        {
            var framer = new LengthPrefixPacketFramer(_bufferSize);
            var framedMessage = framer.Frame(message);
            SendAsync(framedMessage).FireAndForget();
        }

        /// <summary>
        /// Asynchronous task for sending data to the remote host
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task SendAsync(byte[] message)
        {
            NetworkStream netStream = _tcpClient.GetStream();
            await netStream.WriteAsync(message, 0, message.Length);
        }

        /// <summary>
        /// Asynchronous task for handling the connection (connection, data receive loop, disconnect, error handling).
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        private async Task ClientTask(string hostname, int port)
        {
            try
            {
                await _tcpClient.ConnectAsync(hostname, port);

                if (OnConnect != null)
                    OnConnect.Invoke(this);

                using (NetworkStream netStream = _tcpClient.GetStream())
                {
                    var framer = new LengthPrefixPacketFramer(_bufferSize);

                    while (!_cancellation.Token.IsCancellationRequested)
                    {
                        var buffer = new byte[_bufferSize];
                        var bytesRead = await netStream.ReadAsync(buffer, 0, buffer.Length);

                        bool messageReceived = framer.DataReceived(buffer);
                        if (messageReceived)
                        {
                            if (OnMessageReceived != null)
                                OnMessageReceived.Invoke(this, new SACMessageReceivedEventArgs(framer.GetMessage(), framer.GetMessage().Length));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (OnError != null)
                    OnError.Invoke(this, new SACErrorEventArgs(exception));
            }
            finally
            {
                if (OnDisconnect != null)
                    OnDisconnect.Invoke(this);
                
                _tcpClient.Close();
                _tcpClient = null;
                _cancellation.Dispose();
                _cancellation = new CancellationTokenSource();
            }
        }

        public void Dispose()
        {
            _cancellation.Cancel();
            _cancellation.Dispose();

            if (_tcpClient != null)
                _tcpClient.Close();
        }
    }
}
