using Extract.AttributeFinder.Rules.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Extract.AttributeFinder.Rules.Json
{
    /// <summary>
    /// Picks DTO type to deserialize Dto.ObjectWithTypeJson.Object based on Dto.ObjectWithType.Type
    /// </summary>
    public class ObjectWithTypeJsonConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(Dto.ObjectWithType);
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
                object obj = null;
                if (typeName is string && RuleObjectConverter.TryGetDtoTypeFromTypeName(typeName, out var type))
                {
                    obj = jobj["Object"].ToObject(type, serializer);
                }

                return new Dto.ObjectWithType(typeName, obj);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49671");
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
