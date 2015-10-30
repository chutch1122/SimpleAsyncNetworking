using System;
using System.Net.Sockets;

namespace SimpleAsyncNetworking.Events
{
    /// <summary>
    /// On Client Message Received Event Arguments, contains information that is passed with OnClientMessageReceived.
    /// </summary>
    public class SASClientMessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// The client connection that sent the message being received.
        /// </summary>
        public TcpClient Client { get; private set; }

        /// <summary>
        /// The message data from the received message
        /// </summary>
        public byte[] MessageData { get; private set; }

        /// <summary>
        /// The length of the received message
        /// </summary>
        public long MessageLength { get; private set; }

        public SASClientMessageReceivedEventArgs(TcpClient client, byte[] data, long bytesRead)
        {
            Client = client;
            MessageData = data;
            MessageLength = bytesRead;
        }
    }
}
