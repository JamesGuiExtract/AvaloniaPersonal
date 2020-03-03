using Extract.AttributeFinder.Rules.Domain;
using Extract.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Extract.AttributeFinder.Rules.Json
{
    /// <summary>
    /// Picks DTO type to deserialize Dto.ObjectWithDescription.Object based on Dto.ObjectWithDescription.Type
    /// </summary>
    public class ObjectWithDescriptionJsonConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dto.ObjectWithDescription);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The Newtonsoft.Json.JsonReader to read from</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                var jobj = JObject.Load(reader);
                var typeName = (string)jobj["Type"];
                var description = (string)jobj["Description"];
                var enabled = (bool)jobj["Enabled"];
                object obj = null;
                if (typeName is string)
                {
                    if (RuleObjectConverter.TryGetDtoTypeFromTypeName(typeName, out var type))
                    {
                        obj = jobj["Object"].ToObject(type, serializer);
                    }
                    else
                    {
                        throw new ExtractException("ELI49672", UtilityMethods.FormatInvariant($"Unknown type {typeName}"));
                    }
                }

                return new Dto.ObjectWithDescription(typeName, description, enabled, obj);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49670");
            }
        }

        /// <summary>
        /// False so that the default converter will be used to write
        /// </summary>
        public override bool CanWrite => false;

        /// <summary>
        /// Not implemented
        /// </summary>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
