using System;
using System.Net.Sockets;

namespace SimpleAsyncNetworking.Events
{
    /// <summary>
    /// On Client Disconnected Event Arguments, contains information that is passed with OnClientDisconnected.
    /// </summary>
    public class SASClientDisconnectedEventArgs : EventArgs
    {
        /// <summary>
        /// The client connection that disconnected from the SimpleAsyncServer.
        /// </summary>
        public TcpClient Client { get; private set; }

        public SASClientDisconnectedEventArgs(TcpClient client)
        {
            Client = client;
        }
    }
}
