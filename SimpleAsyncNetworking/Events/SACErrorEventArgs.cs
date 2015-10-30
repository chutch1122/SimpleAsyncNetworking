using System;

namespace SimpleAsyncNetworking.Events
{
    /// <summary>
    /// On Error Event Arguments, contains information that is passed with OnError.
    /// </summary>
    public class SACErrorEventArgs : EventArgs
    {
        /// <summary>
        /// The exception thrown while managing the connection.
        /// </summary>
        public Exception Exception { get; private set; }

        public SACErrorEventArgs(Exception ex)
        {
            Exception = ex;
        }
    }
}
