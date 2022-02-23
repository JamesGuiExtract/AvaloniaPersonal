using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Extract.Utilities
{
    /// <summary>
    /// Interface for objects that can create an <see cref="IDataTransferObject"/> implementation
    /// </summary>
    public interface IDomainObject
    {
        /// <summary>
        /// Convert this instance to the corresponding DTO type, wrapped in a <see cref="DataTransferObjectWithType"/>
        /// </summary>
        DataTransferObjectWithType CreateDataTransferObject();
    }

    /// <summary>
    /// Interface for a DTO that can create a domain object
    /// </summary>
    public interface IDataTransferObject
    {
        /// <summary>
        /// Convert this instance to the corresponding domain type
        /// </summary>
        IDomainObject CreateDomainObject();
    }

    /// <summary>
    /// Wrapper for a DTO that includes its type
    /// </summary>
    public sealed class DataTransferObjectWithType : IDataTransferObject
    {
        readonly IDataTransferObject _dto;

        /// <summary>
        /// The Name or FullName of the DTO type, used for deserializing
        /// </summary>
        public string TypeName => _dto.GetType().FullName;

        /// <summary>
        /// The wrapped DTO
        /// </summary>
        public IDataTransferObject DataTransferObject => _dto;

        /// <inheritdoc/>
        public IDomainObject CreateDomainObject() => _dto.CreateDomainObject();

        /// <summary>
        /// Create an instance wrapping the supplied DTO
        /// </summary>
        public DataTransferObjectWithType(IDataTransferObject dataTransferObject)
        {
            _dto = dataTransferObject ?? throw new ArgumentNullException(nameof(dataTransferObject));
        }
    }

    /// <summary>
    /// JSON serializer/deserializer for object graphs that use <see cref="DataTransferObjectWithType"/>
    /// to wrap <see cref="IDataTransferObject"/> implementations
    /// </summary>
    public sealed class DataTransferObjectSerializer
    {
        // Settings that include the custom ConverterContractResolver for DataTransferObject
        readonly JsonSerializerSettings _settings;

        /// <summary>
        /// Create a serializer that will be able to deserialize <see cref="DataTransferObjectWithType"/>
        /// that wrap <see cref="IDataTransferObject"/> implementations defined in the specified assemblies
        /// </summary>
        public DataTransferObjectSerializer(params Assembly[] assemblies)
        {
            _settings = new()
            {
                ContractResolver = new ConverterContractResolver(assemblies),
                Converters = new[] { new StringEnumConverter() },
                Formatting = Formatting.Indented,

                // Prevent developer accidentally adding new fields to a DTO that would silently fail
                // to work correctly on an old version of the software
                MissingMemberHandling = MissingMemberHandling.Error,
            };
        }

        /// <summary>
        /// Create a <see cref="DataTransferObjectWithType"/> from a json string
        /// </summary>
        public DataTransferObjectWithType Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<DataTransferObjectWithType>(json, _settings);
        }

        /// <summary>
        /// Create a json string from a <see cref="DataTransferObjectWithType"/>
        /// </summary>
        public string Serialize(DataTransferObjectWithType dataTransferObject)
        {
            return JsonConvert.SerializeObject(dataTransferObject, _settings);
        }

        // Provide special handling for deserializing DataTransferObjectWithType
        class ConverterContractResolver : DefaultContractResolver
        {
            readonly Assembly[] _assemblies;

            public ConverterContractResolver(Assembly[] assemblies)
            {
                _assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
            }

            protected override JsonContract CreateContract(Type objectType)
            {
                JsonContract contract = base.CreateContract(objectType);

                if (objectType == typeof(DataTransferObjectWithType))
                {
                    contract.Converter = new DataTransferObjectJsonConverter(_assemblies);
                }

                return contract;
            }
        }
    }

    /// <summary>
    /// Converter that handles deserializing <see cref="DataTransferObjectWithType"/>
    /// </summary>
    public sealed class DataTransferObjectJsonConverter : JsonConverter
    {
        // Map from type name to dto type
        readonly Dictionary<string, Type> _fullNameToDtoType;
        readonly Dictionary<string, Type> _nameToDtoType;

        /// <summary>
        /// Create a converter that will be able to deserialize <see cref="DataTransferObjectWithType"/>
        /// that wrap <see cref="IDataTransferObject"/> implementations defined in the specified assemblies
        /// </summary>
        public DataTransferObjectJsonConverter(params Assembly[] assemblies)
        {
            // Look for all available IDataTransferObject implementations
            List<Type> converters = assemblies
                .SelectMany(assm => assm.GetTypes())
                .Where(type => !type.IsInterface && !type.IsAbstract)
                .Where(typeof(IDataTransferObject).IsAssignableFrom)
                .ToList();

            // Map first matching full name
            _fullNameToDtoType = converters
                .GroupBy(type => type.FullName)
                .ToDictionary(group => group.Key, group => group.First());

            // Map first matching short name
            _nameToDtoType = converters
                .GroupBy(type => type.Name)
                .ToDictionary(group => group.Key, group => group.First());
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DataTransferObjectWithType);
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
                var jsonObject = JObject.Load(reader);
                var typeName = (string)jsonObject["TypeName"];

                if (typeName is null)
                {
                    return null;
                }

                // First try the full name then try short name (e.g., to handle namespace changes since this type was serialized)
                if (_fullNameToDtoType.TryGetValue(typeName, out var dtoType)
                    || _nameToDtoType.TryGetValue(Regex.Replace(typeName, @".*\.", ""), out dtoType))
                {
                    var dto = (IDataTransferObject)jsonObject["DataTransferObject"].ToObject(dtoType, serializer);
                    return new DataTransferObjectWithType(dto);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53221");
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
