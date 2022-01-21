using System;
using System.IO;

namespace Extract.Utilities
{
    /// <summary>
    /// Utility class containing common stream operations.
    /// </summary>
    public static class StreamMethods
    {
        /// <summary>
        /// Converts a stream into an array of bytes.
        /// <para><b>NOTE:</b></para>
        /// Caller must close the provided <paramref name="stream"/>.
        /// </summary>
        /// <overloads>This method has two overloads</overloads>
        /// <param name="stream">The stream containing the data to be converted.
        /// Require that stream is not <see langword="null"/></param>
        /// <returns>An array of bytes containing all of the data from the 
        /// provided stream</returns>
        public static byte[] ConvertStreamToByteArray(Stream stream)
        {
            try
            {
                return stream.ToByteArray();
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI21208");
            }
        }

        /// <summary>
        /// Writes a stream to a file.
        /// </summary>
        /// <param name="fileName">The full file name + path + extension to write to.</param>
        /// <param name="stream">The stream to write.</param>
        public static void WriteStreamToFile(string fileName, Stream stream)
        {
            try
            {
                using FileStream outputFileStream = new(fileName, FileMode.Create);
                stream.CopyTo(outputFileStream);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53153");
            }
        }
    }
}
