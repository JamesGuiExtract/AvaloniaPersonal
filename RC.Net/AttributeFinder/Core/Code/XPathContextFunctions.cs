using Extract.Utilities;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Encapsulates custom functions to be used by <see cref="XPathContext"/>.
    /// </summary>
    class XPathContextFunctions : IXsltContextFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XPathContextFunctions"/> class.
        /// </summary>
        /// <param name="minArgs">The min args.</param>
        /// <param name="maxArgs">The max args.</param>
        /// <param name="returnType">Type of the return.</param>
        /// <param name="argTypes">The supplied XML Path Language (XPath) types for the function's
        /// argument list. This information can be used to discover the signature of the function
        /// which allows you to differentiate between overloaded functions.</param>
        /// <param name="functionName">The name of the function referenced by this instance.</param>
        public XPathContextFunctions(int minArgs, int maxArgs,
            XPathResultType returnType, XPathResultType[] argTypes, string functionName)
        {
            try
            {
                Minargs = minArgs;
                Maxargs = maxArgs;
                ReturnType = returnType;
                ArgTypes = argTypes;
                FunctionName = functionName;
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI39414");
            }
        }

        /// <summary>
        /// Gets the maximum number of arguments for the function. This enables the user to
        /// differentiate between overloaded functions.
        /// </summary>
        /// <returns>The maximum number of arguments for the function.</returns>
        public int Maxargs
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the minimum number of arguments for the function. This enables the user to
        /// differentiate between overloaded functions.
        /// </summary>
        /// <returns>The minimum number of arguments for the function.</returns>
        public int Minargs
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="T:XPathResultType"/> representing the XPath type returned by the function.
        /// </summary>
        /// <returns>An <see cref="T:XPathResultType"/> representing the XPath type returned by the
        /// function.</returns>
        public XPathResultType ReturnType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the supplied XML Path Language (XPath) types for the function's argument list.
        /// This information can be used to discover the signature of the function which allows you
        /// to differentiate between overloaded functions.
        /// </summary>
        /// <returns>An array of <see cref="T:XPathResultType"/> representing the
        /// types for the function's argument list.</returns>
        public XPathResultType[] ArgTypes
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the function referenced by this instance.
        /// </summary>
        /// <value>
        /// The name of the function referenced by this instance.
        /// </value>
        public string FunctionName
        {
            get;
            private set;
        }

        /// <summary>
        /// Provides the method to invoke the function with the given arguments in the given context.
        /// </summary>
        /// <param name="xsltContext">The XSLT context for the function call.</param>
        /// <param name="args">The arguments of the function call. Each argument is an element in
        /// the array.</param>
        /// <param name="docContext">The context node for the function call.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> representing the return value of the function.
        /// </returns>
        public object Invoke(XsltContext xsltContext, object[] args, XPathNavigator docContext)
        {
            try
            {
                if (FunctionName == "Levenshtein")
                {
                    return UtilityMethods.LevenshteinDistance((string)args[0], (string)args[1]);
                }

                return null;
            }
            catch (System.Exception ex)
            {
                throw ex.AsExtract("ELI39415");
            }
        }
    }
}
