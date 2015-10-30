using System;

namespace SimpleAsyncNetworking.Events
{
    /// <summary>
    /// On Message Received Event Arguments, contains information that is passed with OnMessageReceived.
    /// </summary>
    public class SACMessageReceivedEventArgs : EventArgs
    {

        /// <summary>
        /// The message data from the received message
        /// </summary>
        public byte[] MessageData { get; private set; }

        /// <summary>
        /// The length of the received message
        /// </summary>
        public long MessageLength { get; private set; }

        public SACMessageReceivedEventArgs(byte[] data, long bytesRead)
        {
            MessageData = data;
            MessageLength = bytesRead;
        }
    }
}
