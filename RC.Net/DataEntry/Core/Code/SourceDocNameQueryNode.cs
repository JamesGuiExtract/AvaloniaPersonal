using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Extract.DataEntry
{
    /// <summary>
    /// A <see cref="QueryNode"/> to be resolved using the current source doc name.
    /// </summary>
    internal class SourceDocNameQueryNode : QueryNode
    {
        /// <summary>
        /// <see langword="true"/> if the full path of the source doc name should be used or
        /// <see langword="false"/> to use just the filename.
        /// </summary>
        bool _useFullPath = true;

        /// <summary>
        /// Initializes a new <see cref="SourceDocNameQueryNode"/> instance.
        /// </summary>
        public SourceDocNameQueryNode()
            : base()
        {
        }

        /// <summary>
        /// Loads the <see cref="SourceDocNameQueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="namedReferences">A communal collection of named
        /// <see cref="NamedQueryReferences"/>s available to allow referencing of named nodes.</param>
        internal override void LoadFromXml(XmlNode xmlNode,
            Dictionary<string, NamedQueryReferences> namedReferences)
        {
            try
            {
                base.LoadFromXml(xmlNode, namedReferences);

                // Use the full path of the document unless specified not to.
                XmlAttribute xmlAttribute = xmlNode.Attributes["UseFullPath"];
                if (xmlAttribute != null)
                {
                    _useFullPath = xmlAttribute.Value.ToBoolean();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28957", ex);
            }
        }

        /// <summary>
        /// Evaluates the query using the current source doc name.
        /// </summary>
        /// <returns>The current source doc name</returns>
        public override QueryResult Evaluate()
        {
            try
            {
                if (_useFullPath)
                {
                    return new QueryResult(this, AttributeStatusInfo.SourceDocName);
                }
                else
                {
                    return new QueryResult(this,
                        Path.GetFileName(AttributeStatusInfo.SourceDocName));
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI34485", ex);
                ee.AddDebugData("Query node type", GetType().Name, true);
                ee.AddDebugData("Query", QueryText ?? "null", true);
                throw ee;
            }
        }
    }
}
