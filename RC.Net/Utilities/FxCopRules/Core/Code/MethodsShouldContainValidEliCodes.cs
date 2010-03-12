using Microsoft.FxCop.Sdk;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Extract.Utilities.FxCopRules
{
    /// <summary>
    /// FxCop rule for detecting if an invalid ELI code has been specified.
    /// </summary>
    public class MethodsShouldContainValidEliCodes : BaseIntrospectionRule
    {
        #region Fields

        /// <summary>
        /// The <see cref="TypeNode"/> value for a <see cref="System.String"/>
        /// </summary>
        TypeNode _stringType;

        /// <summary>
        /// Regular expression matcher for valid ELI code strings.
        /// </summary>
        Regex _regex = new Regex(@"\bELI\d{5}\b");

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="PublicMethodsContainTryCatch"/> class.
        /// </summary>
        public MethodsShouldContainValidEliCodes()
            : base("MethodsShouldContainValidEliCodes", "Extract.Utilities.FxCopRules.ExtractRules",
            typeof(MethodsShouldContainValidEliCodes).Assembly)
        {
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Initialization to be performed before analysis starts.
        /// </summary>
        public override void BeforeAnalysis()
        {
            _stringType = FrameworkAssemblies.Mscorlib.GetType(
                Identifier.For("System"), Identifier.For("String"));
            base.BeforeAnalysis();
        }

        /// <summary>
        /// Set the target visibility to all methods.
        /// </summary>
        public override TargetVisibilities TargetVisibility
        {
            get
            {
                return TargetVisibilities.All;
            }
        }

        /// <summary>
        /// Checks each member of the target assembly.
        /// </summary>
        /// <param name="member">The member to check.</param>
        /// <returns>The problem collection.</returns>
        // This class is an FxCop rule implementation, it will not be released externally
        // to Extract and so there is no need to try to hide the stack trace (which is
        // the main purpose behind wrapping all exceptions as ExtractExceptions). It is
        // safe to suppress this message here and just allow any exceptions to bubble
        // out to be handled by FxCop.
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public override ProblemCollection Check(Member member)
        {
            // Only need to visit statements contained in methods
            Method method = member as Method;
            if (method != null)
            {
                // Visit each statement (this will trigger the introspection model
                // to visit each statement and call into the Visit methods are overriding)
                VisitStatements(method.Body.Statements);
            }

            // Return the problem collection
            return this.Problems;
        }

        /// <summary>
        /// Visits each literal in the code and checks its value
        /// </summary>
        /// <param name="literal">The literal whose value will be examined.</param>
        // This class is an FxCop rule implementation, it will not be released externally
        // to Extract and so there is no need to try to hide the stack trace (which is
        // the main purpose behind wrapping all exceptions as ExtractExceptions). It is
        // safe to suppress this message here and just allow any exceptions to bubble
        // out to be handled by FxCop.
        // This method contains the string "ELI" not followed by any digits so that
        // only strings that start with ELI will be checked by the regular expression.
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        [SuppressMessage("ExtractRules", "ES0002:MethodsShouldContainValidEliCodes")]
        public override void VisitLiteral(Literal literal)
        {
            // Check if this literal is a string type
            if (literal.Type == _stringType)
            {
                // Get the string value from the literal
                string value = literal.Value as string;

                // 1. If the value is not null AND
                // 2. It starts with ELI AND
                // 3. It does not match the expression \bELI\d{5}\b then it is invalid
                if (value != null
                    && value.StartsWith("ELI", StringComparison.CurrentCulture)
                    && !_regex.IsMatch(value))
                {
                    // Invalid ELI code, add a new Problem and add the source context
                    this.Problems.Add(new Problem(this.GetResolution(), literal.SourceContext));
                }
            }

            // Call the base class
            base.VisitLiteral(literal);
        }

        #endregion Overrides
    }
}
