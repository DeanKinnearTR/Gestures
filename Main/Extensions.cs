using System;
using System.IO;

namespace RealtimeRecognition
{
    internal static class Extensions
    {
        public static bool Contains(this string haystack, string needle, StringComparison comparison)
        {
            return haystack?.IndexOf(needle, comparison) >= 0;
        }

        public static byte[] ToByteArray(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}
