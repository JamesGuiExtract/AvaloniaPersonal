using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Extract.Utilities
{
    /// <summary>
    /// Defines a method for converting an instance to an <see cref="XmlElement"/>.
    /// </summary>
    public interface IXmlConvertible
    {
        /// <summary>
        /// Creates an XML element that represents the instance of this object.
        /// </summary>
        /// <param name="document">The XML document to use when creating the XML element.</param>
        /// <returns>An XML element the represents the instance of this object.</returns>
        // The concrete type is necessary for the implementation
        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", 
            MessageId="System.Xml.XmlNode")]
        XmlElement ToXmlElement(XmlDocument document);
    }
}