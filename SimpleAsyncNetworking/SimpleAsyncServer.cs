using SimpleAsyncNetworking.Events;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleAsyncNetworking
{
    /// <summary>
    /// Implements an asynchronous TCP server
    /// </summary>
    public class SimpleAsyncServer : IDisposable
    {
        #region Delegates & Events
        public delegate void SASEventHandler<TEventArgs>(SimpleAsyncServer server, TEventArgs args);
        public delegate void SASEventHandler(SimpleAsyncServer server);

        /// <summary>
        /// On Start Listening Event, called when a SimpleAsyncServer starts listening.
        /// </summary>
        public event SASEventHandler<SASStartListeningEventArgs> OnStartListening;

        /// <summary>
        /// On Stop Listening Event, called when a SimpleAsyncServer stops listening.
        /// </summary>
        public event SASEventHandler OnStopListening;

        /// <summary>
        /// On Listen Error Event, called when a SimpleAsyncServer encounters an error while listening.
        /// </summary>
        public event SASEventHandler<SASListenErrorEventArgs> OnListenError;

        /// <summary>
        /// On Client Connected Event, called when a client connects to a SimpleAsyncServer.
        /// </summary>
        public event SASEventHandler<SASClientConnectedEventArgs> OnClientConnected;

        /// <summary>
        /// On Client Disconnected Event, called when a client disconnects from a SimpleAsyncServer.
        /// </summary>
        public event SASEventHandler<SASClientDisconnectedEventArgs> OnClientDisconnected;

        /// <summary>
        /// On Client Message Received Event, called when a message is completely received from a client.
        /// </summary>
        public event SASEventHandler<SASClientMessageReceivedEventArgs> OnClientMessageReceived;

        /// <summary>
        /// On Client Error Event, called when a SimpleAsyncServer encounters an error while handling a client connection.
        /// </summary>
        public event SASEventHandler<SASClientErrorEventArgs> OnClientError;
        #endregion

        private TcpListener _tcpListener;
        private CancellationTokenSource _cancellation;
        private int _bufferSize;
        private int _backlog;

        /// <summary>
        /// The local endpoint of the SimpleAsyncServer
        /// </summary>
        public IPEndPoint LocalEndPoint
        {
            get
            {
                return (IPEndPoint)_tcpListener.Server.LocalEndPoint;
            }
        }

        
        /// <summary>
        /// Creates a new SimpleAsyncServer that will listen on the specified port.
        /// </summary>
        /// <param name="port"></param>
        public SimpleAsyncServer(int port) : this(IPAddress.Any, port) { }

        /// <summary>
        /// Creates a new SimpleAsyncServer that will listen on the specified local IP address and port.
        /// </summary>
        /// <param name="address">IP to listen on</param>
        /// <param name="port">Port to listen on</param>
        /// <param name="bufferSize">Maximum buffer size. Also the max message size.</param>
        /// <param name="backlog">Maximum length of the pending connections queue.</param>
        public SimpleAsyncServer(IPAddress address, int port, int bufferSize = 8192, int backlog = 50)
        {
            _tcpListener = new TcpListener(address, port);
            _cancellation = new CancellationTokenSource();
            _bufferSize = bufferSize;
            _backlog = backlog;
            _tcpListener.Server.ReceiveBufferSize = _bufferSize;
            _tcpListener.Server.SendBufferSize = _bufferSize;
        }

        /// <summary>
        /// Begins listening for incoming connection requests and start accepting clients asynchronously.
        /// </summary>
        public void StartListening()
        {
            _tcpListener.Start(_backlog);
            
            if (OnStartListening != null)
                OnStartListening.Invoke(this, new SASStartListeningEventArgs(LocalEndPoint));

            Task.Run(ListenAsync).ConfigureAwait(false);
        }
        /// <summary>
        /// Sends data to the remote host and automatically frames the message.
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="message"></param>
        public void Send(TcpClient tcpClient, byte[] message)
        {
            var framer = new LengthPrefixPacketFramer(_bufferSize);
            var framedMessage = framer.Frame(message);
            SendAsync(tcpClient, framedMessage).FireAndForget();
        }

        /// <summary>
        /// Asynchronous task for sending data to the specified client
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task SendAsync(TcpClient tcpClient, byte[] message)
        {
            NetworkStream netStream = tcpClient.GetStream();
            await netStream.WriteAsync(message, 0, message.Length);
        }

        /// <summary>
        /// Stops listening for incoming connection requests and closes all existing connections from clients.
        /// </summary>
        public void Stop()
        {
            _cancellation.Cancel();
            _tcpListener.Stop();
        }

        /// <summary>
        /// Starts accepting clients asynchronously. 
        /// </summary>
        /// <returns></returns>
        private async Task ListenAsync()
        {
            try
            {
                while (!_cancellation.Token.IsCancellationRequested)
                {
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();

                    if (tcpClient == null) continue;
                
                    HandleTcpClientAsync(tcpClient).FireAndForget();
                }
            }
            catch (Exception exception)
            {
                if (OnListenError != null)
                    OnListenError.Invoke(this, new SASListenErrorEventArgs(exception));
            }
        }

        /// <summary>
        /// This method asynchronously handles one client.
        /// It is responsible for reading data from the client and invokes events as they occur.
        /// Automatically unframes framed messages.
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <returns></returns>
        private async Task HandleTcpClientAsync(TcpClient tcpClient)
        {
            try
            {
                if (OnClientConnected != null)
                    OnClientConnected.Invoke(this, new SASClientConnectedEventArgs(tcpClient));

                using (NetworkStream netStream = tcpClient.GetStream())
                {
                    var framer = new LengthPrefixPacketFramer(_bufferSize);

                    while (tcpClient.Connected && !_cancellation.Token.IsCancellationRequested)
                    {
                        var buffer = new byte[_bufferSize];
                        var bytesRead = await netStream.ReadAsync(buffer, 0, buffer.Length);

                        bool messageReceived = framer.DataReceived(buffer);
                        if (messageReceived)
                        {
                            if (OnClientMessageReceived != null)
                                OnClientMessageReceived.Invoke(this, new SASClientMessageReceivedEventArgs(tcpClient, framer.GetMessage(), framer.GetMessage().Length));
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (OnClientError != null)
                    OnClientError.Invoke(this, new SASClientErrorEventArgs(tcpClient, exception));
            }
            finally
            {
                if (OnClientDisconnected != null)
                    OnClientDisconnected.Invoke(this, new SASClientDisconnectedEventArgs(tcpClient));

                tcpClient.Close();
            }
        }

        public void Dispose()
        {
            Stop();
        }

    }
}
