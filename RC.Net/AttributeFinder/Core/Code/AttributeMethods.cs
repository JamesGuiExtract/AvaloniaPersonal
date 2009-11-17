using System;
using System.Collections.Generic;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Represents a grouping of methods for working with COM attribute.
    /// </summary>
    public static class AttributeMethods
    {
        /// <summary>
        /// Gets a single attribute by name from the specified vector of attributes. Throws an 
        /// exception if more than one attribute is found.
        /// </summary>
        /// <param name="attributes">The attributes to search.</param>
        /// <param name="name">The name of the attribute to find.</param>
        /// <returns>The only attribute in <paramref name="attributes"/> with the specified name; 
        /// if no such attribute exists, returns <see langword="null"/>.</returns>
        [CLSCompliant(false)]
        public static ComAttribute GetSingleAttributeByName(IIUnknownVector attributes, string name)
        {
            try
            {
                ComAttribute[] idAttributes = GetAttributesByName(attributes, name);

                if (idAttributes.Length == 0)
                {
                    return null;
                }
                else if (idAttributes.Length == 1)
                {
                    return idAttributes[0];
                }

                throw new ExtractException("ELI28197",
                    "More than one " + name + " attribute found.");
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28541",
                    "Unable to get attribute by name.", ex);
                ee.AddDebugData("Attrbiute name", name, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets an array of COM attributes that have the specified name.
        /// </summary>
        /// <param name="attributes">A vector of COM attributes to search.</param>
        /// <param name="name">The name of the attributes to return.</param>
        /// <returns>An array of COM attributes in <paramref name="attributes"/> that have the 
        /// specified <paramref name="name"/>.</returns>
        [CLSCompliant(false)]
        public static ComAttribute[] GetAttributesByName(IIUnknownVector attributes, string name)
        {
            try
            {
                List<ComAttribute> result = new List<ComAttribute>();

                // Iterate over each attribute
                int count = attributes.Size();
                for (int i = 0; i < count; i++)
                {
                    ComAttribute attribute = (ComAttribute)attributes.At(i);

                    // If this attribute matches the specified name, add it to the result
                    string attributeName = attribute.Name;
                    if (attributeName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Add(attribute);
                    }
                }

                return result.ToArray();
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28540",
                    "Unable to get attribute by name.", ex);
                ee.AddDebugData("Attribute name", name, false);
                throw ee;
            }
        }
    }
}