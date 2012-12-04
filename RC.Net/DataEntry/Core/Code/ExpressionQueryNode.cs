using Extract.DataEntry.Properties;
using Extract.Utilities;
using Spring.Core.TypeConversion;
using Spring.Core.TypeResolution;
using Spring.Expressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using UCLID_AFCORELib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A <see cref="QueryNode"/> that is resolved by evaluating an expression using the Spring.Net
    /// expression evaluation engine.
    /// </summary>
    internal class ExpressionQueryNode : CompositeQueryNode
    {
        #region Statics

        /// <summary>
        /// A cache of compiled queries that are frequently used and/or expensive.
        /// </summary>
        static DataCache<string, CachedQueryData<IExpression>> _cachedExpressions =
            new DataCache<string, CachedQueryData<IExpression>>(
                QueryNode.QueryCacheLimit, CachedQueryData<IExpression>.GetScore);

        #endregion Statics

        #region Constructors

        /// <overrides>
        /// Initializes a new <see cref="CompositeQueryNode"/> instance.
        /// </overrides>
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionQueryNode"/> class.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> that should be considered the
        /// root of any attribute query.</param>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that should be used to
        /// evaluate any SQL queries.</param>
        public ExpressionQueryNode(IAttribute rootAttribute, DbConnection dbConnection)
            : base(rootAttribute, dbConnection)
        {
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Evaluates the query by combining evaluating the expression.
        /// </summary>
        /// <param name="childQueryResults"><see cref="QueryResult"/>s representing the results of
        /// each child <see cref="QueryNode"/>.</param>
        /// <returns>A <see cref="QueryResult"/> representing the result of the query.</returns>
        protected override QueryResult Evaluate(IEnumerable<QueryResult> childQueryResults)
        {
            StringBuilder expressionBuilder = new StringBuilder();

            try
            {
                Dictionary<string, object> variables = new Dictionary<string, object>();

                int variableNum = 0;
                foreach (QueryResult childQueryResult in childQueryResults)
                {
                    // Add any unparameterized node as part of the expression string itself.
                    if (!childQueryResult.QueryNode.Parameterize)
                    {
                        expressionBuilder.Append(childQueryResult);
                    }
                    // Any parameterized nodes should be be treated as variables of a specific type
                    else
                    {
                        // Create a unique name for the variable.
                        variableNum++;
                        string variableName = string.Format(CultureInfo.InvariantCulture,
                            "Variable{0}", variableNum);

                        string stringValue = childQueryResult.ToString();
                        object value = null;

                        // Determine the type to which the chileNode's result should be cast at
                        // evaluation time.
                        Type type = null;
                        string typeName;
                        if (childQueryResult.QueryNodeProperties.TryGetValue("Type", out typeName))
                        {
                            type = TypeResolutionUtils.ResolveType(typeName);
                        }

                        // If not specified, treat as a string.
                        if (type == null)
                        {
                            value = stringValue;
                        }
                        // Otherwise, try to cast to the specified type.
                        else
                        {
                            try
                            {
                                // If the result implements IEnumerable (but is not a simple string),
                                // cast each instance of the value to parameterize separately.
                                if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type))
                                {
                                    value = TypeConversionUtils.ConvertValueIfNecessary(
                                        type, childQueryResult.ToStringArray(), variableName);
                                }
                                else
                                {
                                    value = TypeConversionUtils.ConvertValueIfNecessary(type, stringValue, variableName);
                                }
                            }
                            catch
                            {
                                // If the cast failed, use a default value (if one is provided)
                                string defaultValue;
                                if (childQueryResult.QueryNodeProperties.TryGetValue("Default", out defaultValue))
                                {
                                    value = TypeConversionUtils.ConvertValueIfNecessary(type, defaultValue, variableName);
                                }
                                // Otherwise, use the default value of the type.
                                else if (type.IsValueType)
                                {
                                    value = Activator.CreateInstance(type);
                                }
                            }
                        }

                        variables[variableName] = value;

                        // Plug the variable name into the expression.
                        expressionBuilder.Append("#");
                        expressionBuilder.Append(variableName);
                    }
                }

                if (FlushCache)
                {
                    _cachedExpressions.Clear();
                }

                // If there is a cached compiled IExpression for this expression, retrieve it.
                string expressionText = expressionBuilder.ToString();
                IExpression expression;
                CachedQueryData<IExpression> cachedExpression;
                if (_cachedExpressions.TryGetData(expressionText, out cachedExpression))
                {
                    expression = cachedExpression.Data;
                }
                // Otherwise, compile the query and submit the results for caching.
                else
                {
                    DateTime startTime = DateTime.Now;

                    expression = Expression.Parse(expressionText);

                    if (AllowCaching && !FlushCache)
                    {
                        double executionTime = (DateTime.Now - startTime).TotalMilliseconds;
                        cachedExpression = new CachedQueryData<IExpression>(expression, executionTime);

                        _cachedExpressions.CacheData(expressionText, cachedExpression);
                    }
                }

                // Evaluate the expression.
                object result = expression.GetValue(expression, variables);
                IEnumerable enumberableResult = result as IEnumerable;
                if (!(result is string) && enumberableResult != null)
                {
                    // If the result implements IEnumerable (but is not a simple string), return
                    // each element of the enumeration as a separate result.
                    string[] resultStrings = enumberableResult
                        .Cast<object>()
                        .Select(value => ((object)value).ToString())
                        .ToArray();

                    return new QueryResult(this, resultStrings);
                }
                else
                {
                    return new QueryResult(this, result.ToString());
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI31983");
                ee.AddDebugData("Expression", expressionBuilder.ToString(), false);
                throw ee;
            }
        }

        #endregion Overrides
    }
}
