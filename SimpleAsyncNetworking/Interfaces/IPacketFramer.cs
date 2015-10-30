namespace SimpleAsyncNetworking.Interfaces
{
    internal interface IPacketFramer
    {
        /// <summary>
        /// Frames a message.
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns></returns>
        byte[] Frame(byte[] message);

        /// <summary>
        /// This method is called whenever data is asynchronously received.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True when the whole message has been received, otherwise false.</returns>
        bool DataReceived(byte[] data);

        /// <summary>
        /// Returns the unframed message, after the whole message has been received.
        /// </summary>
        /// <returns></returns>
        byte[] GetMessage();
    }
}
