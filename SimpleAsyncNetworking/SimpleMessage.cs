using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleAsyncNetworking
{
    /// <summary>
    /// Provides a simple way to reading/writing byte data from/to a message.
    /// </summary>
    public class SimpleMessage
    {
        private Queue<byte> _data;

        /// <summary>
        /// Creates a new SimpleMessage
        /// </summary>
        public SimpleMessage()
        {
            _data = new Queue<byte>();
        }

        /// <summary>
        /// Creates a new SimpleMessage from an existing array of bytes (eg: a received message).
        /// </summary>
        /// <param name="data"></param>
        public SimpleMessage(byte[] data)
        {
            _data = new Queue<byte>(data);
        }

        /// <summary>
        /// Clears all data from the message
        /// </summary>
        public void Clear()
        {
            _data.Clear();
        }

        /// <summary>
        /// Writes a boolean value to the message
        /// </summary>
        /// <param name="data"></param>
        public void WriteBool(bool data)
        {
            byte value = Convert.ToByte(data);
            _data.Enqueue(value);
        }

        /// <summary>
        /// Writes a byte value to the message
        /// </summary>
        /// <param name="data"></param>
        public void WriteByte(byte data)
        {
            _data.Enqueue(data);
        }

        /// <summary>
        /// Writes a character to the message
        /// </summary>
        /// <param name="data"></param>
        public void WriteChar(char data)
        {
            byte value = Convert.ToByte(data);
            _data.Enqueue(value);
        }

        /// <summary>
        /// Writes an integer (32 bit integer) to the message
        /// </summary>
        /// <param name="data"></param>
        public void WriteInt(int data)
        {
            byte[] array = BitConverter.GetBytes(data);

            foreach (var b in array)
                _data.Enqueue(b);
        }

        /// <summary>
        /// Writes a long integer (64 bit integer) to the message
        /// </summary>
        /// <param name="data"></param>
        public void WriteLong(long data)
        {
            byte[] array = BitConverter.GetBytes(data);

            foreach (var b in array)
                _data.Enqueue(b);
        }

        /// <summary>
        /// Writes a short integer (16 bit integer) to the message
        /// </summary>
        /// <param name="data"></param>
        public void WriteShort(int data)
        {
            byte[] array = BitConverter.GetBytes(data);

            foreach (var b in array)
                _data.Enqueue(b);
        }

        /// <summary>
        /// Writes a string (UTF8 encoded) to the message
        /// </summary>
        /// <param name="str"></param>
        public void WriteString(string str)
        {
            if (str == null)
                str = String.Empty;

            byte[] stringBytes = Encoding.UTF8.GetBytes(str);
            int stringBytesLength = Encoding.UTF8.GetByteCount(str);
            
            WriteInt(stringBytesLength);
            foreach (var b in stringBytes)
                _data.Enqueue(b);
        }

        /// <summary>
        /// Reads a boolean from the message
        /// </summary>
        /// <returns></returns>
        public bool ReadBool()
        {
            return BitConverter.ToBoolean(Read(sizeof(bool)), 0);
        }

        /// <summary>
        /// Reads a byte from the message
        /// </summary>
        /// <returns></returns>
        public byte ReadByte()
        {
            return Convert.ToByte(Read(1));
        }

        /// <summary>
        /// Reads a character from the message
        /// </summary>
        /// <returns></returns>
        public char ReadChar()
        {
            return BitConverter.ToChar(Read(1), 0);
        }

        /// <summary>
        /// Reads an integer (32 bit) from the message
        /// </summary>
        /// <returns></returns>
        public int ReadInt()
        {
            return BitConverter.ToInt32(Read(4), 0);
        }

        /// <summary>
        /// Reads a long integer (64 bit) from the message
        /// </summary>
        /// <returns></returns>
        public long ReadLong()
        {
            return BitConverter.ToInt64(Read(8), 0);
        }

        /// <summary>
        /// Reads a short integer (16 bit) from the message
        /// </summary>
        /// <returns></returns>
        public short ReadShort()
        {
            return BitConverter.ToInt16(Read(2), 0);
        }

        /// <summary>
        /// Reads a string (UTF8 encoded) from the message
        /// </summary>
        /// <returns></returns>
        public string ReadString()
        {
            var stringLength = ReadInt();

            string value = Encoding.UTF8.GetString(_data.ToArray(), 0, stringLength);

            for (int i = 0; i < stringLength; i++)
                _data.Dequeue();

            return value;
        }
       
        private byte[] Read(int length, bool keepData = false)
        {
            if (length > _data.Count)
                throw new Exception("Not enough data.");

            var result = new byte[length];

            if (keepData)
            {
                var array = _data.ToArray();
                Array.Copy(array, result, length);
            }
            else
            {
                for (var i = 0; i < length; i++)
                    result[i] = _data.Dequeue();
            }

            return result;
        }

        /// <summary>
        /// Converts data from the SimpleMessage to an array of bytes
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            return _data.ToArray();
        }

        /// <summary>
        /// Converts data from the SimpleMessage to a human readable string of hex values.
        /// </summary>
        /// <returns></returns>
        public string ToHexString()
        {
            return _data.ToArray().ToHexString();
        }

        /// <summary>
        /// Index operator for bytes contained in the SimpleMessage
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte this[int index]
        {
            get
            {
                if (index >= _data.Count)
                    throw new IndexOutOfRangeException();

                var array = _data.ToArray();
                return array[index];
            }
        }

        /// <summary>
        /// Bytes contained in the SimpleMessage
        /// </summary>
        public IEnumerable<byte> Bytes
        {
            get
            {
                var array = _data.ToArray();
                for (var i = 0; i < _data.Count; i++)
                    yield return array[i];
            }
        }
    }
}
