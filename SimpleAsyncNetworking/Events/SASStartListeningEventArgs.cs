using System;
using System.Net;

namespace SimpleAsyncNetworking.Events
{
    /// <summary>
    /// On Start Listening Event Arguments, contains information that is passed with OnStartListening.
    /// </summary>
    public class SASStartListeningEventArgs : EventArgs
    {
        /// <summary>
        /// The local endpoint the calling SimpleAsyncServer is listening on.
        /// </summary>
        public IPEndPoint LocalEndPoint { get; private set; }

        public SASStartListeningEventArgs(IPEndPoint localEndPoint)
        {
            LocalEndPoint = localEndPoint;
        }
    }
}
