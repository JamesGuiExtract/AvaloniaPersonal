using Extract.Licensing;
using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Extract.Utilities
{
    /// <summary>
    /// A class containing utility helper methods
    /// </summary>
    public static class UtilityMethods
    {
        #region Constants

        /// <summary>
        /// Object name used for license validation calls.
        /// </summary>
        readonly static string _OBJECT_NAME = typeof(UtilityMethods).ToString();

        #endregion Constants

        /// <summary>
        /// Swaps two value types in place.
        /// </summary>
        /// <typeparam name="T">The type of objects being swapped.</typeparam>
        /// <param name="valueOne">The first value to swap.</param>
        /// <param name="valueTwo">The second value to swap.</param>
        // These values are pass by reference because we are 'swapping' them in place. The
        // result of the swap method is that the two values are swapped. In order for this
        // to be reflected after the call to this method the objects must be passed as a
        // reference.
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#")]
        public static void Swap<T>(ref T valueOne, ref T valueTwo) where T : struct
        {
            try
            {
                T c = valueOne;
                valueOne = valueTwo;
                valueTwo = c;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30146", ex);
            }
        }

        /// <summary>
        /// Alls the types that implement interface.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>An array of <see cref="Type"/> objects that implement the
        /// specified interface <paramref name="interfaceType"/>.</returns>
        public static Type[] AllTypesThatImplementInterface(Type interfaceType,
            params Assembly[] assemblies)
        {
            try
            {
                if (!interfaceType.IsInterface)
                {
                    throw new ArgumentException("Type to find must be an interface.",
                        "interfaceType");
                }
                if (assemblies == null)
                {
                    throw new ArgumentNullException("assemblies");
                }

                return assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(p => p.IsClass && interfaceType.IsAssignableFrom(p))
                    .ToArray();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31131", ex);
            }
        }

        /// <summary>
        /// Creates the type from type name.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns>An instance of the specified type.</returns>
        public static object CreateTypeFromTypeName(string typeName)
        {
            try
            {
                // Build name of assembly from typename (i.e. this assumes that type name follows
                // Extract standards - Extract.Test.FakeAssembly.FakeType
                // is in Extract.Test.FakeAssembly.dll)

                // Build the name to the assembly containing the type
                var sb = new StringBuilder();
                var names = typeName.Split(new char[] { '.' },
                    StringSplitOptions.RemoveEmptyEntries);
                if (names.Length > 0)
                {
                    sb.Append(names[0]);
                }
                for (int i = 1; i < names.Length - 1; i++)
                {
                    sb.Append(".");
                    sb.Append(names[i]);
                }
                var assemblyName = new AssemblyName();
                assemblyName.Name = sb.ToString();

                // Load the assembly if needed
                var assembly = LoadAssemblyIfNotLoaded(assemblyName);

                // Create the type and return it
                return assembly.CreateInstance(typeName, true);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31149", ex);
            }
        }

        /// <summary>
        /// Loads the assembly if not loaded.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly to load.</param>
        /// <returns>The loaded assembly.</returns>
        public static Assembly LoadAssemblyIfNotLoaded(AssemblyName assemblyName)
        {
            try
            {
                string shortName = assemblyName.Name;
                Assembly assembly = null;
                foreach (var loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (shortName.Equals(loadedAssembly.GetName().Name,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        assembly = loadedAssembly;
                        break;
                    }
                }

                // If the assembly is not loaded, load it
                if (assembly == null)
                {
                    assembly = Assembly.Load(assemblyName);
                }

                return assembly;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31153", ex);
            }
        }

        /// <summary>
        /// Creates the type from assembly.
        /// </summary>
        /// <typeparam name="T">The type to load from the assembly.</typeparam>
        /// <param name="assemblyFileName">Name of the assembly file.</param>
        /// <returns>A new instance of <typeparamref name="T"/>.</returns>
        public static T CreateTypeFromAssembly<T>(string assemblyFileName) where T : class, new()
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyFileName);
                if (!LicenseUtilities.VerifyAssemblyData(assembly))
                {
                    var ee = new ExtractException("ELI31150",
                        "Unable to load assembly, verification failed.");
                    ee.AddDebugData("Assembly File", assemblyFileName, true);
                    throw ee;
                }

                T value = null;
                // Using reflection, iterate the classes in the assembly looking for one that 
                // implements DataEntryControlHost
                foreach (var type in assembly.GetTypes())
                {
                    if (type.BaseType == typeof(T))
                    {
                        if (value != null)
                        {
                            var ee = new ExtractException("ELI31151",
                                "Assembly contains multiple implementations of specified type.");
                            ee.AddDebugData("Type", typeof(T).ToString(), false);
                            throw ee;
                        }

                        // Create and instance of the DEP class.
                        value = (T)assembly.CreateInstance(type.ToString());

                        // Keep searching to ensure there are not multiple implementations
                    }
                }

                return value;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31152", ex);
            }
        }

        /// <summary>
        /// Used to validate names for XML elements.
        /// </summary>
        static RegexStringValidator _xmlNameValidator;

        /// <summary>
        /// Validates an XML name element name per the specifications here:
        /// http://www.w3.org/TR/REC-xml/#NT-S
        /// </summary>
        /// <param name="name">The name to be validated.</param>
        public static void ValidateXmlElementName(string name)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI31717", _OBJECT_NAME);

                if (_xmlNameValidator == null)
                {
                    string nameStartChar = @":A-Z_a-z\xC0-\xD6\xD8-\xF6\xF8-\u02FF\u0370-\u037D\u037F-\u1FFF\u200C-\u200D\u2070-\u218F\u2C00-\u2FEF\u3001-\uD7FF\uF900-\uFDCF\uFDF0-\uFFFD";
                    string nameChar = @"-.0-9\xB7\u0300-\u036F\u203F-\u2040" + nameStartChar;

                    _xmlNameValidator = new RegexStringValidator("^[" + nameStartChar + "][" + nameChar + "]+$");
                }

                _xmlNameValidator.Validate(name);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31702");
            }
        }
    }
}
