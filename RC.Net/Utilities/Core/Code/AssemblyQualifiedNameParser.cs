using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extract.Utilities
{
    /// <summary>
    /// Parses a qualified name into its components.
    /// </summary>
    public class AssemblyQualifiedNameParser
    {
        #region Fields

        /// <summary>
        /// The type (optional) and assembly names parsed from the qualified name.
        /// </summary>
        string[] _typeAndAssemblyParts;

        /// <summary>
        /// The named parts parsed from the qualified name.
        /// </summary>
        Dictionary<string, string> _namedParts;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyQualifiedNameParser"/> class.
        /// </summary>
        /// <param name="qualifiedName">The qualified name to parse.</param>
        public AssemblyQualifiedNameParser(string qualifiedName)
        {
            try
            {
                // Parse out the separate parts of the qualified name.
                var parts = qualifiedName.Split(',')
                    .Select(part => part.Trim())
                    .ToArray();

                // The leading parts that do not contain equal signs are the type (optional) and
                // assembly names.
                _typeAndAssemblyParts = parts
                    .TakeWhile(part => !part.Contains('='))
                    .ToArray();

                ExtractException.Assert("ELI36826", "Invalid type",
                    _typeAndAssemblyParts.Length >= 1 && _typeAndAssemblyParts.Length <= 2);

                // Populate a dictionary with all of the named parts
                _namedParts = parts
                    .Select(attribute => attribute.Split(new[] { '=' }, 2))
                    .Where(attributeParts => attributeParts.Length == 2)
                    .ToDictionary(
                        attributeParts => attributeParts[0].Trim(),
                        attributeParts => attributeParts[1].Trim());
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI36827", "Unable to parse qualified name", ex);
                ee.AddDebugData("QualifiedName", qualifiedName, false);
                throw ee;
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the type name.
        /// </summary>
        public string TypeName
        {
            get
            {
                try
                {
                    return (_typeAndAssemblyParts.Length == 1)
                        ? null
                        : _typeAndAssemblyParts[0];
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36828");
                }
            }
        }

        /// <summary>
        /// Gets the namespace.
        /// </summary>
        public string Namespace
        {
            get
            {
                try
                {
                    string typeName = TypeName;
                    if (typeName.Contains('.'))
                    {
                        return typeName.Substring(0, typeName.LastIndexOf('.'));
                    }

                    return null;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36828");
                }
            }
        }

        /// <summary>
        /// Gets the assembly name.
        /// </summary>
        public string AssemblyName
        {
            get
            {
                try
                {
                    return (_typeAndAssemblyParts.Length == 1)
                        ? _typeAndAssemblyParts[0]
                        : _typeAndAssemblyParts[1];
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36829");
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="Version"/>.
        /// </summary>
        public Version Version
        {
            get
            {
                try
                {
                    string versionString;
                    if (_namedParts.TryGetValue("Version", out versionString))
                    {
                        return new Version(versionString);
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36830");
                }
            }
        }

        /// <summary>
        /// Gets the culture.
        /// </summary>
        public string Culture
        {
            get
            {
                try
                {
                    string cultureString;
                    if (_namedParts.TryGetValue("Culture", out cultureString))
                    {
                        return cultureString;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36831");
                }
            }
        }

        /// <summary>
        /// Gets the public key token.
        /// </summary>
        public string PublicKeyToken
        {
            get
            {
                try
                {
                    string publicKeyTokenString;
                    if (_namedParts.TryGetValue("PublicKeyToken", out publicKeyTokenString))
                    {
                        return publicKeyTokenString;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36832");
                }
            }
        }

        #endregion Properties
    }
}
