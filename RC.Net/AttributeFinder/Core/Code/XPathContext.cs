using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Defines a specialized derivative of <see cref="XsltContext"/> that can be used to evaluate
    /// XPath expressions against an <see cref="IAttribute"/> hierarchy.
    /// </summary>
    [CLSCompliant(false)]
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    [SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface")]
    public sealed class XPathContext : XsltContext
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(XPathContext).ToString();

        /// <summary>
        /// The URI for this context's namespace.
        /// </summary>
        const string _XPATH_NAMESPACE_URI = "http://extractSystemsXPath";

        internal const string LevenshteinFunction = "Levenshtein";
        internal const string BitmapFunction = "Bitmap";

        const string _BITMAP_PATTERN = @"(?in)\ABitmap: (?'width'\d+) x (?'height'\d+) = (?'data'-?\d+(\.\d+)?(,-?\d+(\.\d+)?)*)\z";

        public static readonly object RootNode = new object();

        #endregion Constants

        #region XPathIterator

        /// <summary>
        /// A helper class for <see cref="XPathContext"/> that maintains a current position in the
        /// iteration of an XPath query to use as the basis for derivative queries.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        public class XPathIterator
        {
            /// <summary>
            /// The <see cref="XPathContext"/> to which this instance relates.
            /// </summary>
            XPathContext _owningContext;

            /// <summary>
            /// Initializes a new instance of the <see cref="XPathIterator"/> class. For use only by
            /// <see cref="XPathContext"/>.
            /// </summary>
            /// <param name="iterator">The <see cref="XPathNodeIterator"/> defining the iteration.
            /// </param>
            /// <param name="owningContext"> The <see cref="XPathContext"/> to which this instance
            /// relates.</param>
            internal XPathIterator(XPathNodeIterator iterator, XPathContext owningContext)
            {
                XPathNodeIterator = iterator;
                _owningContext = owningContext;
            }

            /// <summary>
            /// Gets or sets the <see cref="XPathNodeIterator"/>. For use only by
            /// <see cref="_owningContext"/>.
            /// </summary>
            /// <value>
            /// The X path node iterator.
            /// </value>
            internal XPathNodeIterator XPathNodeIterator
            {
                get;
                set;
            }

            /// <summary>
            /// Gets the count of elements in the iteration.
            /// </summary>
            public int Count
            {
                get
                {
                    return XPathNodeIterator.Count;
                }
            }

            /// <summary>
            /// Gets the <see cref="IAttribute"/> represented by the current position in the
            /// iteration.
            /// </summary>
            /// <returns>The <see cref="IAttribute"/> represented by the current position in the
            /// iteration or <see langword="null"/> if the node does not correspond to an attribute.
            /// </returns>
            public IAttribute CurrentAttribute
            {
                get
                {
                    IAttribute attribute = null;
                    var currentNode = (IHasXmlNode)XPathNodeIterator.Current;
                    _owningContext._nodeToAttributeMap.TryGetValue(currentNode.GetNode(), out attribute);
                    
                    return attribute;
                }
            }

            /// <summary>
            /// Gets a value indicating whether the current position is the top-level, Document node
            /// </summary>
            /// <value>
            /// <c>true</c> if the current position is the root element; otherwise, <c>false</c>.
            /// </value>
            public bool IsAtRootElement
            {
                get
                {
                    return IsAtRoot(XPathNodeIterator);
                }
            }

            /// <summary>
            /// Advances to the next position in the iteration.
            /// </summary>
            /// <returns><see langword="true"/> if there was another element in the iteration to
            /// advance to; <see langword="false"/> if we have moved past the end of the iteration.
            /// </returns>
            public bool MoveNext()
            {
                return XPathNodeIterator.MoveNext();
            }
        }

        #endregion XPathIterator

        #region Fields

        /// <summary>
        /// The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s representing the hierarchy
        /// of attributes to be queried.
        /// </summary>
        IUnknownVector _attributes;

        /// <summary>
        /// The XML representation of <see cref="_attributes"/>.
        /// </summary>
        XmlDocument _xmlDocument;

        /// <summary>
        /// The <see cref="XPathNavigator"/> used for queries not based on an active iteration.
        /// </summary>
        XPathNavigator _navigator;

        /// <summary>
        /// Links each <see cref="XmlNode"/> in <see cref="_xmlDocument"/> to the
        /// <see cref="IAttribute"/> it represents.
        /// </summary>
        Dictionary<XmlNode, IAttribute> _nodeToAttributeMap;

        /// <summary>
        /// Links each <see cref="IAttribute"/> to the <see cref="XmlNode"/> that represents it in
        /// the <see cref="_xmlDocument"/>.
        /// </summary>
        Dictionary<IAttribute, XmlNode> _attributeToNodeMap;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathContext"/> class.
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s
        /// representing the hierarchy of attributes to be queried.</param>
        public XPathContext(IUnknownVector attributes)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleWritingCoreObjects, "ELI39416",
                    _OBJECT_NAME);

                _attributes = attributes;

                AddNamespace("es", _XPATH_NAMESPACE_URI);

                Refresh();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39417");
            }
        }

        #endregion Constructors

        #region Public Members

        /// <summary>
        /// Rebuilds the XML representation of the specified <see cref="IAttribute"/>s (will reflect
        /// any changes to the attribute hierarchy or attribute values)
        /// </summary>
        public void Refresh()
        {
            try
            {
                _xmlDocument = new XmlDocument();
                _xmlDocument.AppendChild(_xmlDocument.CreateElement("root"));

                _nodeToAttributeMap = new Dictionary<XmlNode, IAttribute>();
                _attributeToNodeMap = new Dictionary<IAttribute,XmlNode>();
                BuildXML(_attributes, _xmlDocument.DocumentElement,
                    _nodeToAttributeMap, _attributeToNodeMap);

                _navigator = _xmlDocument.CreateNavigator();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39418");
            }
        }

        /// <summary>
        /// Gets an <see cref="XPathIterator"/> that allows iterating through the attributes in the
        /// hierarchy selected by <see paramref="xpath"/>.
        /// </summary>
        /// <param name="xpath">The XPath query that defines the attributes to select.</param>
        /// <requires><see paramref="xpath"/> must selected nodes (attributes), not a string,
        /// number, etc.</requires>
        /// <returns>A <see cref="XPathIterator"/> that allows iterating the selected attributes.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "xpath")]
        public XPathIterator GetIterator(string xpath)
        {
            try
            {
                var expression = XPathExpression.Compile(xpath);
                expression.SetContext(this);

                var iterator = _navigator.Evaluate(expression) as XPathNodeIterator;

                ExtractException.Assert("ELI39419",
                    "Unexpected return type for XPath iterator initialization.",
                    iterator != null);

                return new XPathIterator(iterator, this);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39420");
            }
        }

        /// <summary>
        /// Evaluates the specified <see paramref="xpath"/>. Will be evaluated against the current
        /// position of the specified <see paramref="iterator"/>.
        /// </summary>
        /// <param name="iterator">The <see cref="XPathIterator"/> whose current position should
        /// serve as the basis for <see paramref="xpath"/>'s evaluation.</param>
        /// <param name="xpath">The XPath expression to evaluate</param>
        /// <returns>A <see cref="object"/> representing the XPath result. This will be a
        /// <see cref="List{T}"/> of objects for queries that return sequences. Any selected element
        /// in the result that represents an XML node will return the corresponding
        /// <see cref="IAttribute"/>.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "xpath")]
        public object Evaluate(XPathIterator iterator, string xpath)
        {
            try
            {
                return InternalEvaluate(iterator.XPathNodeIterator.Current, xpath);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39422");
            }
        }

        /// <summary>
        /// Evaluates the specified <see paramref="xpath"/>. Will be evaluated using the specified
        /// <see paramref="attribute"/> as the basis or the root of the hierarchy if it is null.
        /// </summary>
        ///<param name="xpath">The XPath expression to evaluate</param>
        /// <param name="attribute">The <see cref="IAttribute"/> which should serve as the basis for
        /// <see paramref="xpath"/>'s evaluation.</param>
        /// <returns>A <see cref="object"/> representing the XPath result. This will be a
        /// <see cref="List{T}"/> of objects for queries that return sequences. Any selected element
        /// in the result that represents an XML node will return the corresponding
        /// <see cref="IAttribute"/>.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "xpath")]
        public object Evaluate(string xpath, IAttribute attribute = null)
        {
            try
            {
                XPathNavigator nav = attribute == null
                    ? null
                    : _attributeToNodeMap[attribute].CreateNavigator();

                return InternalEvaluate(nav, xpath);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39423");
            }
        }

        /// <summary>
        /// Evaluates the specified <see paramref="xpathQuery"/> once against every attribute
        /// selected by <see paramref="xpathIteration"/>.
        /// </summary>
        /// <param name="xpathIteration">The XPath expression that selects each attribute to run
        /// <see paramref="xpathQuery"/> against.</param>
        /// <param name="xpathQuery">The XPath query to run against each attribute returned by
        /// <see paramref="xpathIteration"/>.</param>
        /// <returns>An enumeration of <see cref="object"/>s representing the results for each
        /// attribute selected by <see paramref="xpathIteration"/>. Each element in the enumeration
        /// may itself be a <see cref="List{T}"/> of objects for an <see paramref="xpathQuery"/>
        /// that returns sequences. Any individual result that represents an XML node will return
        /// the corresponding <see cref="IAttribute"/>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "xpath")]
        public IEnumerable<object> EvaluateOver(string xpathIteration, string xpathQuery)
        {
            var nodeIterator = _navigator.Select(xpathIteration);

            foreach (var navigator in nodeIterator.Cast<XPathNavigator>())
            {
                yield return PackageResult(navigator.Evaluate(xpathQuery));
            }
        }

        /// <summary>
        /// Evaluates the specified <see paramref="xpath"/> and filters the result to be a collection
        /// of the specified type.
        /// </summary>
        /// <param name="xpath">The XPath expression to evaluate</param>
        /// <param name="fromAttribute">The attribute to use as context node. If null then the expression
        /// will be evaluated against the root of the attribute hierarchy.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> representing the XPath result.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "xpath")]
        public IEnumerable<T> FindAllOfType<T>(string xpath, IAttribute fromAttribute = null)
        {
            try
            {
                object result = Evaluate(xpath, fromAttribute);

                if (result is T)
                {
                    return Enumerable.Repeat((T)result, 1);
                }

                return (result as IEnumerable<object>)?.OfType<T>()
                    ?? Enumerable.Empty<T>();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41461");
            }
        }

        /// <summary>
        /// Evaluates the specified <see paramref="xpath"/> and converts the result to a collection
        /// of strings.
        /// </summary>
        /// <param name="xpath">The XPath expression to evaluate</param>
        /// <param name="fromAttribute">The attribute to use as context node. If null then the expression
        /// will be evaluated against the root of the attribute hierarchy.</param>
        /// <returns>An <see cref="IEnumerable{string}"/> representing the XPath result.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "xpath")]
        public IEnumerable<string> FindAllAsStrings(string xpath, IAttribute fromAttribute = null)
        {
            try
            {
                object result = Evaluate(xpath, fromAttribute);

                // Check if the value(s) are already strings
                string element = result as string;
                var unforced = element != null
                    ? Enumerable.Repeat(element, 1)
                    : result as IEnumerable<string>;
                if (unforced != null)
                {
                    return unforced;
                }

                // Convert value(s) to string(s)
                var objects = result as IEnumerable<object>;
                if (objects != null)
                {
                    return objects.Select(o => (o as IAttribute)?.Value.String ?? o.ToString());
                }

                return Enumerable.Repeat(result.ToString(), 1);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41462");
            }
        }

        /// <summary>
        /// Removes attributes matching <see paramref="xpath"/> from the xml and the original VOA.
        /// </summary>
        /// <param name="xpath">The xpath query.</param>
        /// <param name="fromAttribute">The attribute to use as context node. If null then the expression
        /// will be evaluated against the root of the attribute hierarchy.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "xpath")]
        public void RemoveMatchingAttributes(string xpath, IAttribute fromAttribute = null)
        {
            try
            {
                XPathNavigator navigator = fromAttribute == null
                    ? null
                    : _attributeToNodeMap[fromAttribute].CreateNavigator();
                var expression = XPathExpression.Compile(xpath);

                // Select will fail if the xpath expression return type is not a node set
                ExtractException.Assert("ELI41508", "XPath must return a node-set (e.g., not a string)",
                    expression.ReturnType == XPathResultType.NodeSet);

                expression.SetContext(this);
                XPathNodeIterator it = (navigator ?? _navigator).Select(expression);
                var attributeToRemoveFromAttributeList = it.Cast<XPathNavigator>()
                    .Select(nav =>
                    {
                        // Convert any node result to the corresponding attribute
                        var currentHasNode = nav as IHasXmlNode;
                        XmlNode node = null;
                        IAttribute attribute = null;
                        if (currentHasNode != null
                            && _nodeToAttributeMap.TryGetValue(node = currentHasNode.GetNode(), out attribute))
                        {
                            // If there is an attribute associated with this node, it will have a parent
                            IAttribute parent = null;
                            nav.MoveToParent();
                            var parentNode = currentHasNode.GetNode();
                            _nodeToAttributeMap.TryGetValue(parentNode, out parent);

                            return new { attribute, node, parent, parentNode };
                        }
                        return null;
                    })
                    .Where(x => x != null)
                    .ToList();

                // Remove selected attributes from original collection
                // as well as from node maps. Also remove the corresponding nodes
                foreach (var x in attributeToRemoveFromAttributeList)
                {
                    // Remove from XML
                    x.parentNode.RemoveChild(x.node);

                    // Remove from maps
                    foreach (var attribute in x.attribute.EnumerateDepthFirst())
                    {
                        if (_attributeToNodeMap.ContainsKey(attribute))
                        {
                            _nodeToAttributeMap.Remove(_attributeToNodeMap[attribute]);
                            _attributeToNodeMap.Remove(attribute);
                        }
                    }

                    // Remove from original hierarchy
                    if (x.parent != null)
                    {
                        x.parent.SubAttributes.RemoveValue(x.attribute);
                    }
                    else
                    {
                        _attributes.RemoveValue(x.attribute);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41499");
            }
        }

        /// <summary>
        /// Parses the string output of the es:Bitmap() function into discrete, numerical data.
        /// </summary>
        /// <remarks>Validates the bitmap dimensions</remarks>
        /// <param name="bmp">The string representation of a bitmap</param>
        /// <param name="width">The width of the bitmap</param>
        /// <param name="height">The data array</param>
        /// <param name="data">The data array</param>
        /// <returns><c>true</c> if parsing was successful, else <c>false</c></returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public static bool TryGetBitmapDataFromString(string bmp, out int width, out int height, out double[] data)
        {
            try
            {
                var match = Regex.Match(bmp, _BITMAP_PATTERN);
                if (match.Success
                    && int.TryParse(match.Groups["width"].Value, out width)
                    && int.TryParse(match.Groups["height"].Value, out height))
                {
                    data = match.Groups["data"].Value
                        .Split(new[] { ',' })
                        .Select(s => double.TryParse(s, out double d) ? d : 0.0)
                        .ToArray();

                    // Validate dimensions
                    int dataSize = data.Length;
                    ExtractException.Assert("ELI45138", "Invalid bitmap data", dataSize == width * height);
                    return true;
                }
                else
                {
                    width = height = 0;
                    data = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45139");
            }
        }

        #endregion Public Members

        #region Overrides

        /// <summary>
        /// Resolves a function reference and returns an
        /// <see cref="T:IXsltContextFunction"/> representing the function. The
        /// <see cref="T:IXsltContextFunction"/> is used at execution time to get the return value
        /// of the function.
        /// </summary>
        /// <param name="prefix">The prefix of the function as it appears in the XPath expression.
        /// </param>
        /// <param name="name">The name of the function.</param>
        /// <param name="ArgTypes">An array of argument types for the function being resolved. This
        /// allows you to select between methods with the same name (for example, overloaded
        /// methods).</param>
        /// <returns>An <see cref="T:IXsltContextFunction"/> representing the function.
        /// </returns>
        public override IXsltContextFunction ResolveFunction(string prefix, string name,
            XPathResultType[] ArgTypes)
        {
            try
            {
                if (LookupNamespace(prefix) == _XPATH_NAMESPACE_URI)
                {
                    switch (name)
                    {
                        case LevenshteinFunction:
                            return new XPathContextFunctions(2, 2, XPathResultType.Number,
                                ArgTypes, LevenshteinFunction, this);
                        case BitmapFunction:
                            return new XPathContextFunctions(3, 5, XPathResultType.String,
                                ArgTypes, BitmapFunction, this);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39424");
            }
        }

        /// <summary>
        /// When overridden in a derived class, resolves a variable reference and returns an
        /// <see cref="T:IXsltContextVariable"/> representing the variable.
        /// </summary>
        /// <param name="prefix">The prefix of the variable as it appears in the XPath expression.
        /// </param>
        /// <param name="name">The name of the variable.</param>
        /// <returns>
        /// An <see cref="T:IXsltContextVariable"/> representing the variable at runtime.
        /// </returns>
        public override IXsltContextVariable ResolveVariable(string prefix, string name)
        {
            // No custom variables implemented.
            return null;
        }

        /// <summary>
        /// When overridden in a derived class, evaluates whether to preserve white space nodes or
        /// strip them for the given context.
        /// </summary>
        /// <param name="node">The white space node that is to be preserved or stripped in the
        /// current context.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the white space is to be preserved or
        /// <see langword="false"/> if the white space is to be stripped.
        /// </returns>
        public override bool PreserveWhitespace(System.Xml.XPath.XPathNavigator node)
        {
            // No custom implementation.
            return false;
        }

        /// <summary>
        /// When overridden in a derived class, compares the base Uniform Resource Identifiers
        /// (URIs) of two documents based upon the order the documents were loaded by the XSLT
        /// processor (that is, the <see cref="T:XslTransform"/> class).
        /// </summary>
        /// <param name="baseUri">The base URI of the first document to compare.</param>
        /// <param name="nextbaseUri">The base URI of the second document to compare.</param>
        /// <returns>
        /// An integer value describing the relative order of the two base URIs: -1 if
        /// <paramref name="baseUri"/> occurs before <paramref name="nextbaseUri"/>; 0 if the two
        /// base URIs are identical; and 1 if <paramref name="baseUri"/> occurs after
        /// <paramref name="nextbaseUri"/>.
        /// </returns>
        public override int CompareDocument(string baseUri, string nextbaseUri)
        {
            // No custom implementation.
            return 0;
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether to include white
        /// space nodes in the output.
        /// </summary>
        /// <returns><see langword="true"/> to check white space nodes in the source document for inclusion in the
        /// output; <see langword="false"/> to not evaluate white space nodes. The default is
        /// <see langword="true"/>.</returns>
        public override bool Whitespace
        {
            get
            {
                return true;
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Builds up an <see cref="XmlDocument"/> via recursive calls which begin with a call in
        /// which <see paramref="parentElement"/> is the <see cref="XmlDocument.DocumentElement"/>
        /// and <see paramref="attributes"/> are the root-level of the attribute hierarchy to query.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/>s which are to be added to
        /// <see paramref="parentElement"/>.</param>
        /// <param name="parentElement">The <see cref="XmlElement"/> currently being populated.
        /// </param>
        /// <param name="nodeToAttributeMap">Links each <see cref="XmlNode"/> (implemented as 
        /// <see cref="XmlElement"/>) in the document to the <see cref="IAttribute"/> it represents.
        /// </param>
        /// <param name="attributeToNodeMap">Links each <see cref="IAttribute"/> to
        /// the <see cref="XmlNode"/> (implemented as <see cref="XmlElement"/>) that represents it.
        /// </param>
        static void BuildXML(IUnknownVector attributes, XmlElement parentElement,
            Dictionary<XmlNode, IAttribute> nodeToAttributeMap, 
            Dictionary<IAttribute, XmlNode> attributeToNodeMap)
        {
            foreach (var attribute in attributes.ToIEnumerable<IAttribute>())
            {
                var xmlElement = parentElement.OwnerDocument.CreateElement(attribute.Name);
                if (!string.IsNullOrEmpty(attribute.Value.String))
                {
                    xmlElement.InnerText = attribute.Value.String;
                }
                parentElement.AppendChild(xmlElement);
                nodeToAttributeMap[xmlElement] = attribute;
                attributeToNodeMap[attribute] = xmlElement;

                if (!string.IsNullOrEmpty(attribute.Type))
                {
                    xmlElement.SetAttribute("Type", attribute.Type);
                }

                BuildXML(attribute.SubAttributes, xmlElement, nodeToAttributeMap, attributeToNodeMap);
            }
        }

        /// <summary>
        /// Evaluates the specified <see paramref="xpath"/>. Will be evaluated against the current
        /// position of <see paramref="navigator"/> if specified (non-<see langword="null"/>).
        /// </summary>
        /// <param name="navigator">The <see cref="XPathNavigator"/> which should serve as the basis
        /// for <see paramref="xpath"/>'s evaluation.</param>
        /// <param name="xpath">The XPath expression to evaluate</param>
        /// <returns>A <see cref="object"/> representing the XPath result. This will be a
        /// <see cref="List{T}"/> of objects for queries that return sequences. Any selected element
        /// in the result that represents an XML node will return the corresponding
        /// <see cref="IAttribute"/>.
        /// </returns>
        object InternalEvaluate(XPathNavigator navigator, string xpath)
        {
            var expression = XPathExpression.Compile(xpath);
            expression.SetContext(this);

            var result = (navigator ?? _navigator).Evaluate(expression);
            return PackageResult(result);
        }

        /// <summary>
        /// Formats the specified XPath evaluation <see paramref="result"/> for consumption by a
        /// caller that is dealing with an <see cref="IAttribute"/> hierarchy rather than XML.
        /// </summary>
        /// <param name="result">The raw result of an XPath evaluation.</param>
        /// <returns>A <see cref="object"/> where an object list will replace
        /// <see cref="XPathNodeIterator"/>s for queries that return sequences and where any
        /// individual result that represents an XML node will return the corresponding
        /// <see cref="IAttribute"/> instead.</returns>
        object PackageResult(object result)
        {
            var nodeIterator = result as XPathNodeIterator;
            if (nodeIterator == null)
            {
                // A singular result.
                // I don't think it is possible for an XPath query to return a node on its own
                // (rather than 1 node in a node collection), but just in case...
                var node = result as IHasXmlNode;
                if (node != null)
                {
                    // Convert any node result to the corresponding attribute (if there is one)
                    IAttribute attribute = null;
                    if (_nodeToAttributeMap.TryGetValue(node.GetNode(), out attribute))
                    {
                        return attribute;
                    }
                }

                // Otherwise just return the raw result
                return result;
            }
            else
            {
                List<object> objectList = new List<object>();
                while (nodeIterator.MoveNext())
                {
                    if (IsAtRoot(nodeIterator))
                    {
                        objectList.Add(RootNode);
                    }
                    else
                    {
                        object value = nodeIterator.Current.Value;
                        IAttribute attribute = null;

                        var currentNode = nodeIterator.Current as IHasXmlNode;
                        if (currentNode != null)
                        {
                            // Convert any node result to the corresponding attribute (if there is one).
                            _nodeToAttributeMap.TryGetValue(currentNode.GetNode(), out attribute);
                        }

                        objectList.Add(attribute ?? value);
                    }
                }

                return objectList;
            }
        }

        static bool IsAtRoot(XPathNodeIterator nodeIterator)
        {
            return nodeIterator.Current.NodeType == XPathNodeType.Element
                && ((IHasXmlNode)nodeIterator.Current)
                .GetNode()
                .ParentNode
                .NodeType == XmlNodeType.Document;
        }

        #endregion Private Members
    }
}
