using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Extract.Web.Shared
{
    public static class XMLSerializer
    {
        public static string Serialize<T>(this T value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            try
            {
                var xmlserializer = new XmlSerializer(typeof(T));
                using var stringWriter = new StringWriter();
                using var writer = XmlWriter.Create(stringWriter);
                xmlserializer.Serialize(writer, value);
                return stringWriter.ToString();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53711");
            }
        }

        public static T Deserialize<T>(this string value)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));

            return (T)xmlSerializer.Deserialize(new StringReader(value));
        }
    }
}
