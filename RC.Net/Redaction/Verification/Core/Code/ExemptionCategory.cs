using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a category of exemption codes.
    /// </summary>
    public class ExemptionCategory
    {
        #region ExemptionCategory Fields

        /// <summary>
        /// The abbreviated name of the category.
        /// </summary>
        readonly string _abbreviation;

        /// <summary>
        /// The exemption codes in this category.
        /// </summary>
        readonly ExemptionCode[] _codes;

        #endregion ExemptionCategory Fields

        #region ExemptionCategory Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExemptionCategory"/> class.
        /// </summary>
        ExemptionCategory(string abbreviation, ExemptionCode[] codes)
        {
            _abbreviation = abbreviation;
            _codes = codes;
        }

        #endregion ExemptionCategory Constructors

        #region ExemptionCategory Properties

        /// <summary>
        /// Gets the abbreviated name of the exemption category.
        /// </summary>
        /// <returns>The abbreviated name of the exemption category.</returns>
        public string Abbreviation
        {
            get
            {
                return _abbreviation;
            }
        }

        /// <summary>
        /// Gets the exemption codes in <see cref="ExemptionCategory"/>.
        /// </summary>
        /// <returns>The exemption codes in <see cref="ExemptionCategory"/>.</returns>
        public IEnumerable<ExemptionCode> Codes
        {
            get
            {
                return _codes;
            }
        }

        #endregion ExemptionCategory Properties

        #region ExemptionCategory Methods

        /// <summary>
        /// Creates an <see cref="ExemptionCategory"/> from the specified xml file.
        /// </summary>
        /// <param name="xmlFile">The xml exemption code category file.</param>
        /// <returns>An <see cref="ExemptionCategory"/> created from the specified 
        /// <paramref name="xmlFile"/>.
        /// </returns>
        public static ExemptionCategory FromXml(string xmlFile)
        {
            try
            {
                // Get the exemption category node
                XmlElement category = GetExemptionCategoryNode(xmlFile);

                // Get the abbreviated name of this category
                string abbreviation = GetNamedAttributeValue("Name", category.Attributes);

                // Get the exemption codes
                ExemptionCode[] codes = GetCodesFromCategoryXml(category);

                return new ExemptionCategory(abbreviation, codes);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26688",
                    "Unable to read exemption codes from xml file.", ex);
                ee.AddDebugData("Xml file name", xmlFile, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the &lt;ExemptionCategory&gt; xml node from the specified xml file.
        /// </summary>
        /// <param name="xmlFile">The xml file that contains the &lt;ExemptionCategory&gt; node.
        /// </param>
        /// <returns>The &lt;ExemptionCategory&gt; xml node in <paramref name="xmlFile"/>.</returns>
        static XmlElement GetExemptionCategoryNode(string xmlFile)
        {
            // Load the XML File
            XmlDocument document = new XmlDocument();
            document.Load(xmlFile);

            // Get the exemption category from the xml file
            XmlElement category = document.DocumentElement;
            if (category.Name != "ExemptionCategory")
            {
                // Throw an exception
                ExtractException ee = new ExtractException("ELI26687",
                    "Invalid root node.");
                ee.AddDebugData("XML file name", xmlFile, false);
                ee.AddDebugData("Root node", category.Name, false);
                throw ee;
            }
            return category;
        }

        /// <summary>
        /// Gets an array of exemption codes from the specified &lt;ExemptionCategory&gt; xml node.
        /// </summary>
        /// <param name="category">The &lt;ExemptionCategory&gt; node.</param>
        /// <returns>An array of exemption codes from the specified &lt;ExemptionCategory&gt; xml 
        /// node.</returns>
        static ExemptionCode[] GetCodesFromCategoryXml(XmlElement category)
        {
            XmlNodeList exemptionList = category.GetElementsByTagName("Exemption");

            // Iterate through each exemption
            List<ExemptionCode> exemptionCodes = new List<ExemptionCode>(exemptionList.Count);
            foreach (XmlNode exemption in exemptionList)
            {
                // Get the attributes of this exemption
                XmlAttributeCollection exemptionAttributes = exemption.Attributes;

                // Get the code, summary, and description
                string code = GetNamedAttributeValue("Code", exemptionAttributes);
                string summary = GetNamedAttributeValue("Summary", exemptionAttributes);
                string description = GetNamedAttributeValue("Description", exemptionAttributes);

                // Add this exemption code information to the vector
                ExemptionCode exemptionCode = new ExemptionCode(code, summary, description);
                exemptionCodes.Add(exemptionCode);
            }

            return exemptionCodes.ToArray();
        }

        /// <summary>
        /// Gets the value of the specified attribute.
        /// </summary>
        /// <param name="name">The name of the attribute to get the value.</param>
        /// <param name="attributes">The attributes that contain the value to get.</param>
        /// <returns>The value of the attribute in <paramref name="attributes"/> with the 
        /// specified <paramref name="name"/>.</returns>
        static string GetNamedAttributeValue(string name, XmlAttributeCollection attributes)
        {
            // Get the attribute with the specified name
            XmlNode attribute = attributes.GetNamedItem(name);

            // Return the text of the attribute
            return attribute.Value;
        }

        /// <summary>
        /// Determines whether the specified code belongs to this category.
        /// </summary>
        /// <param name="code">The code to check for containment.</param>
        /// <returns><see langword="true"/> if the specified <paramref name="code"/> is in this 
        /// category; <see langword="false"/> if the specified <paramref name="code"/> is not in 
        /// this category.</returns>
        public bool HasCode(string code)
        {
            try
            {
                foreach (ExemptionCode exemption in _codes)
                {
                    if (exemption.Name == code)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26934",
                    "Unable to find exemption code in category.", ex);
                ee.AddDebugData("Code", code, false);
                ee.AddDebugData("Category", _abbreviation, false);
                throw ee;
            }
        }

        #endregion ExemptionCategory Methods
    }
}
