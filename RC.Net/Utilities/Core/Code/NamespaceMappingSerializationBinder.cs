using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Extract.Utilities
{
    /// <summary>
    /// Class used to map types saved with TypeNameHandling.Objects (full namespace prefixed to type name) to a new namespace
    /// </summary>
    /// <remarks>Currently this will only replace the prefix of the type name so it will not work for generic type parameters.</remarks>
    public class NamespaceMappingSerializationBinder : DefaultSerializationBinder
    {
        // Namespaces (type prefixes) to change (from key to value)
        private Dictionary<string, string> _namespaceMappings;

        /// <summary>
        /// Creates instance with given namespace mappings
        /// </summary>
        /// <param name="namespaceMappings">Pairs to translate namespaces (type prefixes) from/to</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public NamespaceMappingSerializationBinder(IEnumerable<KeyValuePair<string, string>> namespaceMappings)
            : base()
        {
            _namespaceMappings = namespaceMappings.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// Controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="assemblyName">Specifies the System.Reflection.Assembly name of the serialized object.</param>
        /// <param name="typeName">Specifies the System.Type name of the serialized object.</param>
        /// <returns>Specifies the System.Type name of the serialized object.</returns>
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public override Type BindToType(string assemblyName, string typeName)
        {
            string fixedTypeName = typeName;

            var from = _namespaceMappings.Keys.FirstOrDefault(key => typeName.StartsWith(key, StringComparison.Ordinal));
            if (!string.IsNullOrEmpty(from))
            {
                var to = _namespaceMappings[from];
                if (to != null)
                {
                    if (from.Length < typeName.Length)
                    {
                        fixedTypeName = to + typeName.Substring(from.Length);
                    }
                    else
                    {
                        fixedTypeName = to;
                    }
                }
            }
            return base.BindToType(assemblyName, fixedTypeName);
        }
    }
}
