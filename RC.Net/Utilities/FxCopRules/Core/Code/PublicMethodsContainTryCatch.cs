using Microsoft.FxCop.Sdk;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Text;

namespace Extract.Utilities.FxCopRules
{
    /// <summary>
    /// FxCop rule for detecting if public methods contain a try catch
    /// </summary>
    public class PublicMethodsContainTryCatch : BaseIntrospectionRule
    {
        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="PublicMethodsContainTryCatch"/> class.
        /// </summary>
        public PublicMethodsContainTryCatch()
            : base("PublicMethodsContainTryCatch", "Extract.Utilities.FxCopRules.ExtractRules",
            typeof(PublicMethodsContainTryCatch).Assembly)
        {
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Set the target visibility to public methods only.
        /// </summary>
        public override TargetVisibilities TargetVisibility
        {
            get
            {
                return TargetVisibilities.ExternallyVisible;
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
            if (member.NodeType == NodeType.Property)
            {
                return this.Problems;
            }

            // Only need to check public methods (do not check constructors, methods called Dispose,
            // property getters, or operator overloads)
            Method method = member as Method;
            if (method != null && method.IsPublic && member.NodeType != NodeType.InstanceInitializer
                && !method.Name.Name.Equals("dispose", StringComparison.OrdinalIgnoreCase)
                && !method.Name.Name.StartsWith("get_", StringComparison.OrdinalIgnoreCase)
                && !method.Name.Name.StartsWith("op_", StringComparison.OrdinalIgnoreCase))
            {
                // Get the instructions collection
                InstructionCollection instructions = method.Instructions;

                // Ignore small methods (less than 20 instructions)
                if (instructions.Count < 20)
                {
                    return this.Problems;
                }

                // Loop through the instructions looking for a catch block
                bool catchFound = false;
                foreach (Instruction instruction in instructions)
                {
                    // Check for the catch block
                    if (instruction.OpCode == OpCode._Catch)
                    {
                        catchFound = true;
                        break;
                    }
                }

                // If we did not find a catch handler then add a new problem to the
                // problem collection
                if (!catchFound)
                {
                    this.Problems.Add(new Problem(this.GetResolution(), method.SourceContext));
                }

            }

            // Return the problem collection
            return this.Problems;
        }

        #endregion Overrides
    }
}
