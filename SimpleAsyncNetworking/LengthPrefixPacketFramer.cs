using SimpleAsyncNetworking.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAsyncNetworking
{
    // Based on http://blog.stephencleary.com/2009/04/sample-code-length-prefix-message.html
    /// <summary>
    /// Handles framing and unframing of application packets. Packets are framed by prefixing the packet with the length of the data being sent. 
    /// </summary>
    internal class LengthPrefixPacketFramer : IPacketFramer
    {
        private byte[] _lengthBuffer;
        private byte[] _dataBuffer;
        private byte[] _previousDataBuffer;
        private int _bytesReceived;
        private int _maxMessageSize;

        /// <summary>
        /// Creates a new instance of the packet framer. 
        /// </summary>
        /// <param name="maxMessageSize">The maximum message size</param>
        public LengthPrefixPacketFramer(int maxMessageSize)
        {
            _lengthBuffer = new byte[sizeof(int)];
            _maxMessageSize = maxMessageSize;
        }

        /// <summary>
        /// Frames a packet by prefixing the length of the data being sent.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public byte[] Frame(byte[] message)
        {
            var messageLengthPrefix = BitConverter.GetBytes(message.Length);

            var wrappedMessage = new byte[messageLengthPrefix.Length + message.Length];
            messageLengthPrefix.CopyTo(wrappedMessage, 0);
            message.CopyTo(wrappedMessage, messageLengthPrefix.Length);

            return wrappedMessage;
        }

        /// <summary>
        /// Called whenever data is asyncrhonously received.
        /// </summary>
        /// <param name="data"></param>
        /// <returns>True when a whole application message has been received, otherwise false.</returns>
        public bool DataReceived(byte[] data)
        {
            int i = 0;
            bool result = false;
            
            while (i != data.Length)
            {
                var bytesAvailable = data.Length - i;

                if (_dataBuffer != null)
                {
                    int bytesRequested = _dataBuffer.Length - _bytesReceived;

                    int bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
                    Array.Copy(data, i, _dataBuffer, _bytesReceived, bytesTransferred);
                    i += bytesTransferred;

                    result = ReadCompleted(bytesTransferred);
                }
                else
                {
                    int bytesRequested = _lengthBuffer.Length - _bytesReceived;

                    int bytesTransferred = Math.Min(bytesRequested, bytesAvailable);
                    Array.Copy(data, i, _lengthBuffer, _bytesReceived, bytesTransferred);
                    i += bytesTransferred;

                    result = ReadCompleted(bytesTransferred);
                }

                if (result) break;
            }

            return result;
        }

        /// <summary>
        /// Gets the last fully received message.
        /// </summary>
        /// <returns>The last fully received message, unframed.</returns>
        public byte[] GetMessage()
        {
            if (_previousDataBuffer != null)
                return _previousDataBuffer;
            else
                return new byte[1];
        }

        /// <summary>
        /// Called whenever we've finished processing a section of data.
        /// </summary>
        /// <param name="count"></param>
        /// <returns>True when a whole application message has been received, otherwise false.</returns>
        private bool ReadCompleted(int count)
        {
            _bytesReceived += count;

            if (_dataBuffer == null)
            {
                if (_bytesReceived == sizeof(int))
                {
                    int length = BitConverter.ToInt32(_lengthBuffer, 0);

                    if (length < 0)
                        throw new ProtocolViolationException("Message length cannot be less than zero.");

                    if (_maxMessageSize > 0 && length > _maxMessageSize)
                        throw new ProtocolViolationException(
                            String.Format("Message length {0} is larger than maximum message size {1}.", length, _maxMessageSize)
                        );

                    if (length == 0)
                    {
                        _bytesReceived = 0;
                    }
                    else
                    {
                        _dataBuffer = new byte[length];
                        _bytesReceived = 0;
                    }
                }
            }
            else
            {
                if (_bytesReceived == _dataBuffer.Length)
                {
                    _previousDataBuffer = new byte[_dataBuffer.Length];
                    Array.Copy(_dataBuffer, _previousDataBuffer, _dataBuffer.Length);

                    _dataBuffer = null;
                    _bytesReceived = 0;

                    return true;
                }
            }

            return false;
        }
    }
}
