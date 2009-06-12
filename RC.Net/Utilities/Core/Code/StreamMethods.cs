using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Utilities
{
    /// <summary>
    /// Utlities class containing common stream operations.
    /// </summary>
    public static class StreamMethods
    {
        /// <summary>
        /// Buffer size used for improving the stream read performance
        /// </summary>
        /// <remarks>Cannot use a BufferedStream since the side effect
        /// of this will result in the provided stream being closed
        /// after the Convert methods have returned</remarks>
        private const int _BUFFER_SIZE = 1024;

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
                ExtractException.Assert("ELI21212", "Stream must not be null!", stream != null);

                // Create a new memory stream to hold the data
                MemoryStream tempStream = new MemoryStream();

                // Wrap the memory stream in a buffered stream for better performance
                BufferedStream memoryBuffer = new BufferedStream(tempStream);

                try
                {
                    // Create a buffer to hold the data as its read
                    byte[] buffer = new byte[_BUFFER_SIZE];

                    int bytesRead = stream.Read(buffer, 0, _BUFFER_SIZE);
                    while (bytesRead > 0)
                    {
                        // Copy the read bytes into the memory stream
                        memoryBuffer.Write(buffer, 0, bytesRead);

                        // Read the next chunk of bytes
                        bytesRead = stream.Read(buffer, 0, _BUFFER_SIZE);
                    }

                    // Flush any remaining bytes from the buffer into the memory stream
                    memoryBuffer.Flush();

                    // Return the array of bytes
                    return tempStream.ToArray();
                }
                finally
                {
                    // Close the temp stream (closing the buffered stream 
                    // will close the underlying stream)
                   memoryBuffer.Close();
                }
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI21208", "Failed converting stream to byte array", e);
            }
        }

        /// <summary>
        /// Converts the specified number of bytes from the stream into a byte array.
        /// <para><b>NOTE:</b></para>
        /// Caller must close the provided <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream containing the data to be converted.
        /// Require that stream is not <see langword="null"/></param>
        /// <param name="length">The number of bytes to be returned. The length
        /// must be greater than 0.</param>
        /// <returns>An array of bytes read from <paramref name="stream"/>.  If the length of 
        /// <paramref name="stream"/> is less than <paramref name="length"/> then the array
        /// will contain all bytes from <paramref name="stream"/> and will have a length
        /// equivalent to the length of <paramref name="stream"/>.</returns>
        public static byte[] ConvertStreamToByteArray(Stream stream, long length)
        {
            try
            {
                ExtractException.Assert("ELI21215", "Stream must not be null!", stream != null);
                ExtractException.Assert("ELI21216", "Length must be > 0!", length > 0, 
                    "Length", length);

                // Create a temporary stream to hold the data
                MemoryStream tempStream = new MemoryStream();

                // Wrap the memory stream in a buffer for better performance
                BufferedStream memoryBuffer = new BufferedStream(tempStream);

                try
                {
                    long bytesRemaining = length;

                    // Create a buffer to hold data as its read
                    byte[] buffer = new byte[_BUFFER_SIZE];

                    // Initialize bytesRead to 1 so that the loop will execute at least once
                    int bytesRead = 1;

                    // Continue reading from the stream as long as their are bytes remaining
                    // and the number of bytes the user has specified to convert has not
                    // been reached
                    while (bytesRemaining > 0 && bytesRead > 0)
                    {
                        // Read either _BUFFER_SIZE or bytesRemaining # of bytes 
                        // (whichever is less)
                        bytesRead = stream.Read(buffer, 0,
                            bytesRemaining > _BUFFER_SIZE ? _BUFFER_SIZE : (int)bytesRemaining);

                        // Update the number of bytesRemaining
                        bytesRemaining -= bytesRead;

                        // Write the read bytes to the tempStream
                        memoryBuffer.Write(buffer, 0, bytesRead);
                    }

                    // Flush any remaining bytes from the buffer into the memory stream
                    memoryBuffer.Flush();

                    return tempStream.ToArray();
                }
                finally
                {
                    // Close the temp stream (closing the buffered stream 
                    // will close the underlying stream)
                    memoryBuffer.Close();
                }
            }
            catch (Exception e)
            {
                ExtractException ee = new ExtractException("ELI21209", 
                    "Failed converting stream to byte array", e);
                ee.AddDebugData("Length", length, false);
                throw ee;
            }
        }
    }
}
