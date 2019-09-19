using Extract.Utilities;
using Extract.Utilities.Parsers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Xml;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A <see cref="QueryNode"/> that is resolved by extracting matches in the query's value for
    /// the provided regex pattern.
    /// </summary>
    internal class RegexQueryNode : CompositeQueryNode
    {
        #region Fields

        /// <summary>
        /// The <see cref="DotNetRegexParser"/> used to search for regex matches.
        /// </summary>
        DotNetRegexParser _regexParser = new DotNetRegexParser();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="RegexQueryNode"/> instance.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnections">The <see cref="DbConnection"/>(s) that should be used to
        /// evaluate any SQL queries; The key is the connection name (blank for default connection).
        /// </param>
        public RegexQueryNode(IAttribute rootAttribute,
            Dictionary<string, DbConnection> dbConnections)
            : base(rootAttribute, dbConnections)
        {
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Loads the <see cref="RegexQueryNode"/> using the specified XML query string.
        /// </summary>
        /// <param name="xmlNode">The XML query string defining the query.</param>
        /// <param name="namedReferences">A communal collection of named <see cref="QueryNode"/>s
        /// available to allow referencing of named nodes.</param>
        internal override void LoadFromXml(XmlNode xmlNode,
            Dictionary<string, NamedQueryReferences> namedReferences)
        {
            try
            {
                base.LoadFromXml(xmlNode, namedReferences);

                // Changes to the attribute should trigger an update unless specified not to.
                XmlAttribute xmlAttribute = xmlNode.Attributes["Pattern"];
                ExtractException.Assert("ELI31978",
                    "Regex query node must contain a \"Pattern\" attribute.", xmlAttribute != null);

                _regexParser.Pattern = xmlAttribute.Value;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31981", ex);
            }
        }

        /// <summary>
        /// Evaluates the query by searching the results of the child query node(s) for the
        /// specified regex pattern.
        /// </summary>
        /// <param name="childQueryResults"><see cref="QueryResult"/>s representing the results of
        /// each child <see cref="QueryNode"/>.</param>
        /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
        protected override QueryResult Evaluate(IEnumerable<QueryResult> childQueryResults)
        {
            try
            {
                // The string to search is the result of the base class's evaluation.
                string childQueryString = base.Evaluate(childQueryResults).ToString();

                // Search for regex matches
                IUnknownVector regexResults =
                    _regexParser.Find(childQueryString,
                        SelectionMode == MultipleQueryResultSelectionMode.First, false, false);

                // Convert the matches to a string array.
                string[] matches = regexResults.ToIEnumerable<IObjectPair>()
                    .Select(resultPair => ((Token)(resultPair.Object1)).Value)
                    .ToArray();

                return new QueryResult(this, matches);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31980");
            }
        }

        #endregion Overrides
    }
}
