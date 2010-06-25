using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Extract.ExceptionHelper
{
    /// <summary>
    /// A class containing helper methods for serializing and deserializing exceptions to
    /// a hex string.
    /// </summary>
    internal static class ExceptionHelperMethods
    {
        /// <summary>
        /// Deserializes an exception that has been serialized using a binary formatter
        /// and then converted to a hex string.
        /// </summary>
        /// <param name="hexException">The hex string version of the serialized exception.</param>
        /// <returns>The deserialized verison of the exception.</returns>
        public static Exception DeserializeExceptionFromHexString(string hexException)
        {
            try
            {
                // Convert the hex string back to bytes
                byte[] bytes = StringMethods.ConvertHexStringToBytes(hexException);

                // Deserialize the exception and return it.
                using (MemoryStream stream = new MemoryStream(bytes))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    stream.Position = 0;

                    // Read the exception from the stream
                    Exception e = (Exception)formatter.Deserialize(stream);

                    return e;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30290", ex);
            }
        }
    }
}
