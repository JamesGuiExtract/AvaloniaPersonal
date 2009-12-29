using Extract.Utilities;
using Extract.Utilities.Forms;
using System.Globalization;
using System.Xml;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents the saved state of the <see cref="VerificationTaskForm"/> user interface.
    /// </summary>
    internal class VerificationTaskFormMemento : IXmlConvertible
    {
        #region Fields

        /// <summary>
        /// The saved state of the form.
        /// </summary>
        readonly FormMemento _formMemento;

        /// <summary>
        /// The distance of the splitter from the top of the data window in client pixels.
        /// </summary>
        readonly int _splitterDistance;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationTaskFormMemento"/> class.
        /// </summary>
        public VerificationTaskFormMemento(FormMemento formMemento, int splitterDistance)
        {
            _formMemento = formMemento;
            _splitterDistance = splitterDistance;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the saved state of the form.
        /// </summary>
        /// <value>The saved state of the form.</value>
        public FormMemento FormMemento
        {
            get
            {
                return _formMemento;
            }
        }

        /// <summary>
        /// Gets the distance of the splitter from the top of the data window in client pixels.
        /// </summary>
        /// <value>The distance of the splitter from the top of the data window in client pixels.</value>
        public int SplitterDistance
        {
            get
            {
                return _splitterDistance;
            }
        }


        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a <see cref="VerificationTaskFormMemento"/> from the specified XML element.
        /// </summary>
        /// <param name="element">The element from which to create the memento.</param>
        /// <returns>A <see cref="VerificationTaskFormMemento"/> from the specified XML element.
        /// </returns>
        public static VerificationTaskFormMemento FromXmlElement(XmlElement element)
        {
            // Get the form memento
            FormMemento formMemento = FormMemento.FromXmlElement(element);

            // Get the splitter distance
            int splitterDistance = 0;
            XmlNode dataWindow = GetChildNodeByName(element, "DataWindow");
            if (dataWindow != null)
            {
                XmlElement splitter = GetChildNodeByName(dataWindow, "Splitter") as XmlElement;
                if (splitter != null)
                {
                    splitterDistance = int.Parse(splitter.GetAttribute("Distance"), CultureInfo.InvariantCulture);
                }
            }

            return new VerificationTaskFormMemento(formMemento, splitterDistance);
        }

        /// <summary>
        /// Gets the child node with specified name from the specified node.
        /// </summary>
        /// <param name="parent">The node with children.</param>
        /// <param name="name">The name of the child node to retrieve.</param>
        /// <returns>The child node of <paramref name="parent "/>with specified 
        /// <paramref name="name"/>.</returns>
        static XmlNode GetChildNodeByName(XmlNode parent, string name)
        {
            foreach (XmlNode child in parent.ChildNodes)
            {
                if (child.Name == name)
                {
                    return child;
                }
            }

            return null;
        }

        #endregion Methods

        #region IXmlConvertible

        /// <summary>
        /// Creates an XML element that represents the instance of this object.
        /// </summary>
        /// <param name="document">The XML document to use when creating the XML element.</param>
        /// <returns>An XML element the represents the instance of this object.</returns>
        public XmlElement ToXmlElement(XmlDocument document)
        {
            // Get the form as XML
            XmlElement form = _formMemento.ToXmlElement(document);

            // Get the data window as XML
            XmlElement dataWindow = document.CreateElement("DataWindow");

            // Get the splitter as XML
            XmlElement splitter = document.CreateElement("Splitter");
            splitter.SetAttribute("Distance", _splitterDistance.ToString(CultureInfo.InvariantCulture));

            // Append the XML together
            dataWindow.AppendChild(splitter);
            form.AppendChild(dataWindow);

            return form;
        }

        #endregion IXmlConvertible
    }
}
