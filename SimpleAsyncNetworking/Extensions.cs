using System;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAsyncNetworking
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        internal static async void FireAndForget(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception){}
        }

        /// <summary>
        /// Converts an array of bytes into a human readable string of hex values.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] array)
        {
            StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
            string hexAlphabet = "0123456789ABCDEF";

            foreach (byte b in array)
            {
                stringBuilder.Append(hexAlphabet[(int)(b >> 4)]);
                stringBuilder.Append(hexAlphabet[(int)(b & 0xF)]);
                stringBuilder.Append(" ");
            }

            return stringBuilder.ToString();
        }
    }
}
