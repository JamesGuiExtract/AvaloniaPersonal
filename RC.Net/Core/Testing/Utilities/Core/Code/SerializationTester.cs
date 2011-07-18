using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

namespace Extract.Testing.Utilities
{
    /// <summary>
    /// Class for testing serialization of objects that implement <see cref="ISerializable"/>.
    /// </summary>
    public static class SerializationTester
    {
        /// <summary>
        /// Tests whether the specified object will serialize correctly.
        /// </summary>
        /// <typeparam name="T">The object type to test.</typeparam>
        /// <param name="value">The object to attempt to stream.</param>
        /// <returns><see langword="true"/> if the object serialized correctly and
        /// <see langword="false"/> if it does not serialize correctly.</returns>
        public static bool TestSerialization<T>(T value) where T : class, ISerializable
        {
            try
            {
                ExtractException.Assert("ELI27915", "Object must not be null.", value != null,
                        "Object Type", typeof(T));

                // Create a memory stream to stream the object to and from
                using (MemoryStream stream = new MemoryStream())
                {
                    // Create a new formatter
                    BinaryFormatter formatter = new BinaryFormatter();

                    // Stream the object
                    formatter.Serialize(stream, value);

                    // Move the stream back to the beginning
                    stream.Seek(0, SeekOrigin.Begin);

                    // Now attempt to deserialize the object
                    T test = formatter.Deserialize(stream) as T;

                    if (test == null)
                    {
                        return false;
                    }

                    // Check if item needs to be disposed
                    IDisposable disposer = test as IDisposable;
                    if (disposer != null)
                    {
                        disposer.Dispose();
                        test = null;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32925");
            }
        }
    }
}
