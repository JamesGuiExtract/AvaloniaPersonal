using System;
using System.Globalization;
using UCLID_RASTERANDOCRMGMTLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Represents a utility for creating COM attributes.
    /// </summary>
    [CLSCompliant(false)]
    public class AttributeCreator
    {
        #region Fields

        /// <summary>
        /// The source document of the attrributes to create
        /// </summary>
        readonly string _sourceDocument;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeCreator"/> class.
        /// </summary>
        public AttributeCreator(string sourceDocument)
        {
            _sourceDocument = sourceDocument;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Creates a COM attribute with the specified name.
        /// </summary>
        /// <param name="name">The name of the COM attribute to create.</param>
        /// <returns>A COM attribute with the specified <paramref name="name"/>.</returns>
        public ComAttribute Create(string name)
        {
            return Create(name, null, null);
        }

        /// <summary>
        /// Creates a COM attribute with the specified name and value.
        /// </summary>
        /// <param name="name">The name of the COM attribute to create.</param>
        /// <param name="value">The value of the COM attribute to create. Will be converted to a 
        /// string.</param>
        /// <returns>A COM attribute with the specified <paramref name="name"/> and 
        /// <paramref name="value"/>.</returns>
        public ComAttribute Create(string name, IConvertible value)
        {
            return Create(name, value, null);
        }

        /// <summary>
        /// Creates a COM attribute with the specified name, value, and type.
        /// </summary>
        /// <param name="name">The name of the COM attribute to create.</param>
        /// <param name="value">The value of the COM attribute to create. Will be converted to a 
        /// string.</param>
        /// <param name="type">The type of the COM attribute to create.</param>
        /// <returns>A COM attribute with the specified <paramref name="name"/>, 
        /// <paramref name="value"/>, and <paramref name="type"/>.</returns>
        public ComAttribute Create(string name, IConvertible value, string type)
        {
            try
            {
                // Create an attribute with the specified name
                ComAttribute attribute = new ComAttribute();
                attribute.Name = name;

                // Set the value if specified
                attribute.Value = CreateNonSpatialString(value ?? "");

                // Set the type if specified
                if (type != null)
                {
                    attribute.Type = type;
                }

                return attribute;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29736", ex);
            }
        }

        /// <summary>
        /// Creates a non-spatial string with the specified value.
        /// </summary>
        /// <param name="value">The value of the non-spatial string to create. Will be converted 
        /// to a string.</param>
        /// <returns>A non-spatial string with the specified <paramref name="value"/>.</returns>
        SpatialString CreateNonSpatialString(IConvertible value)
        {
            SpatialString spatialString = new SpatialString();
            string text = value.ToString(CultureInfo.CurrentCulture);
            spatialString.CreateNonSpatialString(text, _sourceDocument);

            return spatialString;
        }

        #endregion Methods
    }
}