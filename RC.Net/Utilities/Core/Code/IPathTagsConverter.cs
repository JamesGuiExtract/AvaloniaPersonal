using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace Extract.Utilities
{
    /// <summary>
    /// Represents a <see cref="TypeConverter"/> for the <see cref="IPathTags"/> interface.
    /// </summary>
    class IPathTagsConverter : TypeConverter
    {
        #region IPathTagsConverter Methods

        /// <summary>
        /// Iterates through the exported path tag types of all loaded Extract Systems assemblies.
        /// </summary>
        /// <returns>The exported path tag types of all loaded Extract Systems assemblies.</returns>
        static IEnumerable<Type> GetPathTagTypes()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // For efficiency, check only Extract Systems assemblies
                if (assembly.FullName.StartsWith("Extract.", StringComparison.Ordinal))
                {
                    foreach (Type type in assembly.GetExportedTypes())
                    {
                        if (type.IsSubclassOf(typeof(PathTagsBase)))
                        {
                            yield return type;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the display name of the specified type.
        /// </summary>
        /// <param name="type">The type from which to get the display name.</param>
        /// <returns>The display name of the specified type.</returns>
        static string GetDisplayName(Type type)
        {
            // Get the DisplayNameAttribute if it exists
            object[] attributes = type.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                DisplayNameAttribute name = (DisplayNameAttribute)attributes[0];
                return name.DisplayName;
            }

            // Return the type name as the display name
            return type.Name;
        }

        #endregion IPathTagsConverter Methods

        #region IPathTagsConverter Overrides

        /// <summary>
        /// Returns whether this object supports a standard set of values that can be picked from 
        /// a list, using the specified context. 
        /// </summary>
        /// <param name="context">The format context.</param>
        /// <returns><see langword="true"/> if <see cref="GetStandardValues"/> should be called to 
        /// find a common set of values the object supports; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns whether the collection of standard values returned from 
        /// <see cref="GetStandardValues"/> is an exclusive list of possible values, using the 
        /// specified context. 
        /// </summary>
        /// <param name="context">The format context.</param>
        /// <returns><see langword="true"/> if the set of standard values returned from 
        /// <see cref="GetStandardValues"/> is an exhaustive list of possible values; 
        /// <see langword="false"/> if other values are possible.</returns>
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns a collection of standard values for the data type this type converter is 
        /// designed for when provided with a format context.
        /// </summary>
        /// <param name="context">The format context that can be used to extract additional 
        /// information about the environment from which this converter is invoked. This parameter 
        /// or properties of this parameter can be <see langword="null"/>. </param>
        /// <returns>The standard set of valid values, or <see langword="null"/> if the data type 
        /// does not support a standard set of values.</returns>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            try
            {
                List<string> values = new List<string>();

                foreach (Type type in GetPathTagTypes())
                {
                    values.Add(GetDisplayName(type));
                }

                return new StandardValuesCollection(values);
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI26525", ex);

                return null;
            }
        }

        /// <summary>
        /// Returns whether this converter can convert the object to the specified type, using the 
        /// specified context. 
        /// </summary>
        /// <param name="context">The format context.</param>
        /// <param name="destinationType">The type to convert.</param>
        /// <returns><see langword="true"/> if this converter can perform the conversion; 
        /// otherwise, <see langword="false"/>.</returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }
            if (destinationType == typeof(string))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        /// <summary>
        /// Returns whether this converter can convert an object of the given type to the type of 
        /// this converter, using the specified context.
        /// </summary>
        /// <param name="context">The format context.</param>
        /// <param name="sourceType">The type from which to convert.</param>
        /// <returns><see langword="true"/> if this converter can perform the conversion; 
        /// otherwise, <see langword="false"/>.</returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the given value object to the specified type, using the specified context and 
        /// culture information. 
        /// </summary>
        /// <param name="context">The format context.</param>
        /// <param name="culture">A <see cref="CultureInfo"/>. If <see langword="null"/> is passed, 
        /// the current culture is assumed.</param>
        /// <param name="value">The object to convert.</param>
        /// <param name="destinationType">The type to which <paramref name="value"/> should be 
        /// converted.</param>
        /// <returns>An object that represents the converted value.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture,
            object value, Type destinationType)
        {
            try
            {
                // Check if the source value is a path tags object
                IPathTags tags = value as IPathTags;
                if (tags != null)
                {
                    // Convert to instance descriptor
                    if (destinationType == typeof(InstanceDescriptor))
                    {
                        // Get constructor
                        ConstructorInfo constructor = tags.GetType().GetConstructor(Type.EmptyTypes);
                        return new InstanceDescriptor(constructor, null);
                    }

                    // Convert to string
                    if (destinationType == typeof(string))
                    {
                        Type type = tags.GetType();
                        return GetDisplayName(type);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI26526", ex);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified context 
        /// and culture information. 
        /// </summary>
        /// <param name="context">The format context.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.
        /// </param>
        /// <param name="value">The object to convert.</param>
        /// <returns>An object that represents the converted value.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture,
            object value)
        {
            try
            {
                // If converting from a string
                string name = value as string;
                if (name != null)
                {
                    // Return an instance of the type or null if this is the default
                    foreach (Type type in GetPathTagTypes())
                    {
                        if (GetDisplayName(type) == name)
                        {
                            return Activator.CreateInstance(type);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI26527", ex);
            }

            return base.ConvertFrom(context, culture, value);
        }

        #endregion IPathTagsConverter Overrides
    }
}
