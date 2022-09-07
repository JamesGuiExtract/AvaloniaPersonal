using Extract.ErrorHandling.Encryption;
using System;
using System.DirectoryServices.ActiveDirectory;
using System.Reflection;

namespace Extract.ErrorHandling
{
    static public class DebugDataHelper
    {
        // This is to only allow decryption on a computer on our network
        static readonly string _DOMAIN = "extract.local";

        static private void verifyCaller(AssemblyName callingAssemblyName)
        {
            string callerPublicKey = callingAssemblyName.GetPublicKey().ToHexString(true);

            if (!callerPublicKey.Equals(Constants.ExtractPublicKey))
            {
                throw new ExtractException("ELI53548", "Invalid Caller");
            }
        }

        public static T GetValueAsType<T>(object obj)
        {
            T value = default(T);
            if (obj is string)
            {
                var s = (string)obj;
                if (!s.Contains(ExtractException._ENCRYPTED_PREFIX) || Domain.GetComputerDomain().Name != _DOMAIN)
                {
                    return (T)obj;
                }
                verifyCaller(Assembly.GetCallingAssembly().GetName());
                s = s.Replace(ExtractException._ENCRYPTED_PREFIX, "");
                var output = new ByteArrayManipulator(new byte[s.Length / 2]).GetBytes(8);
                var input = new ByteArrayManipulator(s.FromHexString());
                EncryptionEngine.Decrypt(input.GetBytes(8), ExtractException.CreateK(), output);
                ByteArrayManipulator outputStream = new(output);
                return ConvertFromString<T>(outputStream.ReadString());
            }

            value = (T)obj;
            return value;
        }

        private static T ConvertFromString<T>(string stringValue)
        {
            T value = default(T);
            var type = typeof(T);
            // only types allowed are the ones that can be saved
            switch (type.Name)
            {
                case "String":
                    value = (T)(object)stringValue;
                    break;
                case "Boolean":
                    value = (T)(object)bool.Parse(stringValue);
                    break;
                case "Int16":
                    value = (T)(object)Int16.Parse(stringValue);
                    break;
                case "Int32":
                    value = (T)(object)Int32.Parse(stringValue);
                    break;
                case "Int64":
                    value = (T)(object)Int64.Parse(stringValue);
                    break;
                case "UInt32":
                    value = (T)(object)UInt32.Parse(stringValue);
                    break;
                case "DateTime":
                    value = (T)(object)DateTime.Parse(stringValue);
                    break;
                case "Guid":
                    value = (T)(object)Guid.Parse(stringValue);
                    break;
                case "Double":
                    value = (T)(object)Double.Parse(stringValue);
                    break;
                default:
                    value = (T)(object)stringValue;
                    break;
            }
            return value;
        }
    }
}
