using System;

namespace SimpleAsyncNetworking.Events
{
    /// <summary>
    /// On Listen Error Event Arguments, contains information that is passed with OnListenError.
    /// </summary>
    public class SASListenErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The exception thrown while listening for connections.
        /// </summary>
        public Exception Exception { get; private set; }

        public SASListenErrorEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }
}
