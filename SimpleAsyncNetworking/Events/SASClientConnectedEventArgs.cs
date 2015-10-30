using System;
using System.Net.Sockets;

namespace SimpleAsyncNetworking.Events
{
    /// <summary>
    /// On Client Connected Event Arguments, contains information that is passed with OnClientConnected.
    /// </summary>
    public class SASClientConnectedEventArgs : EventArgs
    {
        /// <summary>
        /// The client connection that connected to the SimpleAsyncServer.
        /// </summary>
        public TcpClient Client { get; private set; }

        public SASClientConnectedEventArgs(TcpClient client)
        {
            Client = client;
        }
    }
}
