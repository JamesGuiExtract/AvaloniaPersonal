using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Extract.ErrorHandling
{
    public static class ExtractConvertExtensions
    {
        public static Byte[] FromHexString(this string hexString)
        {
            var bytes = new byte[hexString.Length / 2];
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        public static string ToHexString(this byte[] bytes, bool upperCase = false)
        {
            var sb = new StringBuilder();

            foreach (var t in bytes)
            {
                sb.Append(t.ToString((upperCase) ? "X2":"x2"));
            }
            return sb.ToString();
        }

        public static Int64 ToUnixTime(this DateTime dateTime)
        {
            DateTime originDate = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = dateTime.ToUniversalTime() - originDate;
            return (Int64)Math.Floor(diff.TotalSeconds);
        }

        public static T[] ToArray<T>(this ICollection<T> collection, int Count) 
        {
            var array = new T[Count];
            int i = 0;
            foreach (var item in collection)
            {
                array[i++] = item;
            }
            while (i < Count)
            {
                array[i++] = default(T);
            }

            return array;
        }
    }
}
