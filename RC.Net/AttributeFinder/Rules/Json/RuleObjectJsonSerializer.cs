using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Extract.AttributeFinder.Rules.Json
{
    /// <summary>
    /// Utilities to serialize/deserialize rule objects to/from JSON
    /// </summary>
    public static class RuleObjectJsonSerializer
    {
        #region Public Members

        /// <summary>
        /// Serialization settings needed to be able to deserialize rule objects
        /// </summary>
        public static JsonSerializerSettings Settings { get; } =
            new JsonSerializerSettings
            {
                ContractResolver = new ConverterContractResolver(),
                Converters = new [] { new StringEnumConverter() },
                Formatting = Formatting.Indented
            };

        /// <summary>
        /// Serialize a rule object to JSON
        /// </summary>
        /// <typeparam name="TDomain">The type of the rule object to be serialized</typeparam>
        /// <param name="rule">The rule object to be serialized</param>
        /// <returns>The json form of the rule object</returns>
        public static string Serialize<TDomain>(TDomain rule)
        {
            try
            {
                var (json, _) = Serialize<TDomain, object>(rule);
                return json;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49662");
            }
        }

        /// <summary>
        /// Deserialize from JSON to a rule object
        /// </summary>
        /// <typeparam name="TDomain">The _class_ type of the rule object to be deserialized</typeparam>
        /// <param name="json">The json string to be deserialized</param>
        /// <returns>The deserialized rule object</returns>
        /// <remarks><see paramref="TDomain"/>must correspond to the <see cref="IRuleObjectConverter.convertsDomain"/>
        /// type of a <see cref="IRuleObjectConverter"/></remarks>
        public static TDomain Deserialize<TDomain>(string json)
        {
            try
            {
                var (domain, _) = DeserializeIncludeIntermediateObject<TDomain>(json);
                return domain;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49664");
            }
        }

        /// <summary>
        /// Serialize a rule object to JSON and return the intermedidate DTO object as well
        /// </summary>
        /// <typeparam name="TDomain">The type of the rule object to be serialized</typeparam>
        /// <typeparam name="TDto">The type of intermediate rule object to use</typeparam>
        /// <param name="rule">The rule object to be serialized</param>
        /// <returns>The json form of the rule object and the intermediate DTO object</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dto")]
        public static (string, TDto) Serialize<TDomain, TDto>(TDomain rule)
        {
            try
            {
                var dto = (TDto)Domain.RuleObjectConverter.ConvertToDto(rule);
                var json = JsonConvert.SerializeObject(dto, Formatting.Indented, Settings);

                return (json, dto);
            }
            catch (Exception ex)
            {
                var uex = new ExtractException("ELI49633", "Could not convert rule object to DTO", ex);
                uex.AddDebugData("Type", typeof(TDomain).Name);
                throw uex;
            }
        }

        /// <summary>
        /// Deserialize from JSON to a rule object
        /// </summary>
        /// <typeparam name="TDomain">The _class_ type of the rule object to be deserialized</typeparam>
        /// <typeparam name="TDto">The type to cast the intermediate rule object to (not used for deserializing)</typeparam>
        /// <param name="json">The json string to be deserialized</param>
        /// <returns>The deserialized rule object and the intermediate DTO object</returns>
        /// <remarks><see paramref="TDomain"/>must correspond to the <see cref="IRuleObjectConverter.convertsDomain"/>
        /// type of a <see cref="IRuleObjectConverter"/>
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dto")]
        public static (TDomain, object) DeserializeIncludeIntermediateObject<TDomain>(string json)
        {
            try
            {
                if (Domain.RuleObjectConverter.TryGetDtoTypeFromDomainType(typeof(TDomain), out Type dtoType))
                {
                    return DeserializeIncludeIntermediateObject<TDomain>(json, dtoType);
                }
                else
                {
                    var uex = new ExtractException("ELI49663", "Could not resolve DTO for specified type!");
                    uex.AddDebugData("Specified type", typeof(TDomain).Name);
                    throw uex;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49692");
            }
        }


        /// <summary>
        /// Deserialize from JSON to a rule object
        /// </summary>
        /// <typeparam name="TDomain">The _class_ type of the rule object to be deserialized</typeparam>
        /// <typeparam name="TDto">The type to use for the intermediate DTO object</typeparam>
        /// <param name="json">The json string to be deserialized</param>
        /// <returns>The deserialized rule object and the intermediate DTO object</returns>
        /// <remarks><see paramref="TDomain"/>must correspond to the <see cref="IRuleObjectConverter.convertsDomain"/>
        /// type of a <see cref="IRuleObjectConverter"/> and <see paramref="TDto"/>must correspond to the
        /// <see cref="IRuleObjectConverter.convertsDto"/> type of a <see cref="IRuleObjectConverter"/>
        /// </remarks>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dto")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "dto")]
        public static (TDomain, TDto) DeserializeIncludeIntermediateObject<TDomain, TDto>(string json)
        {
            try
            {
                var (domain, dto) = DeserializeIncludeIntermediateObject<TDomain>(json, typeof(TDto));
                return (domain, (TDto)dto);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49693");
            }
        }

        #endregion

        #region Private Methods

        static (TDomain, object) DeserializeIncludeIntermediateObject<TDomain>(string json, Type dtoType)
        {
            var dto = JsonConvert.DeserializeObject(json, dtoType, Settings);

            try
            {
                return ((TDomain)Domain.RuleObjectConverter.ConvertFromDto(dto), dto);
            }
            catch (Exception ex)
            {
                var uex = new ExtractException("ELI49663", "Could not convert DTO to specified type!", ex);
                uex.AddDebugData("DTO type", dtoType.Name);
                uex.AddDebugData("Specified type", typeof(TDomain).Name);
                throw uex;
            }
        }

        #endregion

        #region ConverterContractResolver

        class ConverterContractResolver : DefaultContractResolver
        {
            protected override JsonContract CreateContract(Type objectType)
            {
                JsonContract contract = base.CreateContract(objectType);

                if (objectType == typeof(Dto.ObjectWithDescription))
                {
                    contract.Converter = new ObjectWithDescriptionJsonConverter();
                }
                else if (objectType == typeof(Dto.ObjectWithType))
                {
                    contract.Converter = new ObjectWithTypeJsonConverter();
                }

                return contract;
            }
        }

        #endregion
    }
}
