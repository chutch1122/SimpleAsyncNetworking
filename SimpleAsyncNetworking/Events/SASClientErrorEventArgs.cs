using System;
using System.Net.Sockets;

namespace SimpleAsyncNetworking.Events
{
    /// <summary>
    /// On Client Error Event Arguments, contains information that is passed with OnClientError.
    /// </summary>
    public class SASClientErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The client connection that caused the error.
        /// </summary>
        public TcpClient Client { get; private set; }

        /// <summary>
        /// The exception thrown while managing the connection.
        /// </summary>
        public Exception Exception { get; private set; }

        public SASClientErrorEventArgs(TcpClient client, Exception ex)
        {
            Client = client;
            Exception = ex;
        }
    }
}
