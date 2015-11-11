using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A special query node type that can be used to pass a <see cref="DbConnection"/> to a utility
    /// class method within an <see cref="ExpressionQueryNode"/>.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public class DbConnectionQueryNode : CompositeQueryNode
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DbConnectionQueryNode"/> instance.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnections">The <see cref="DbConnection"/>(s) that should be used to
        /// evaluate any SQL queries; The key is the connection name (blank for default connection).
        /// </param>
        public DbConnectionQueryNode(IAttribute rootAttribute,
            Dictionary<string, DbConnection> dbConnections)
            : base(rootAttribute, dbConnections)
        {
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Evaluates the query by combining all child <see cref="QueryNode"/>s.
        /// </summary>
        /// <param name="childQueryResults"><see cref="QueryResult"/>s representing the results of
        /// each child <see cref="QueryNode"/>.</param>
        /// <returns>
        /// A <see cref="QueryResult"/> representing the result of the query.
        /// </returns>
        protected override QueryResult Evaluate(IEnumerable<QueryResult> childQueryResults)
        {
            try
            {
                // Get the name of the connection.
                string value = base.Evaluate(childQueryResults).ToString();

                return new QueryResult(this, DatabaseConnections[value]);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39116");
            }
        }

        #endregion Overrides
    }
}
