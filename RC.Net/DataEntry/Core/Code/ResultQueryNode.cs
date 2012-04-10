using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extract.DataEntry
{
    /// <summary>
    /// A <see cref="QueryNode"/> to allow referencing of named results.
    /// </summary>
    internal class ResultQueryNode : CompositeQueryNode
    {
        /// <summary>
        /// The name of the result referenced by this node.
        /// </summary>
        string _resultName;

        /// <summary>
        /// Initializes a new <see cref="ResultQueryNode"/> instance.
        /// </summary>
        public ResultQueryNode()
            : base(null, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ResultQueryNode"/> instance.
        /// </summary>
        /// <param name="resultName">The name of the result to be used as the first argument.
        /// </param>
        /// <param name="namedReferences">A communal collection of named
        /// <see cref="NamedQueryReferences"/>s available to allow referencing of named nodes.</param>
        internal ResultQueryNode(string resultName,
            Dictionary<string, NamedQueryReferences> namedReferences)
            : base(null, null)
        {
            try
            {
                _resultName = resultName;

                NamedQueryReferences namedReference =
                    GetNamedReferences(_resultName, namedReferences);
                namedReference.ReferencingQueries.Add(this);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34495");
            }
        }

        /// <summary>
        /// Evaluates the query by combining all child <see cref="QueryNode"/>s.
        /// </summary>
        /// <param name="childQueryResults"><see cref="QueryResult"/>s representing the results of
        /// each child <see cref="QueryNode"/>.</param>
        /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
        protected override QueryResult Evaluate(IEnumerable<QueryResult> childQueryResults)
        {
            try
            {
                return GetNamedResult(_resultName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34497");
            }
        }
    }
}
