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
        #region Constants

        /// <summary>
        /// Methods that are exempt from this rule (since exceptions should never throw from them)
        /// </summary>
        static readonly string[] _METHODS_TO_SKIP = new string[]
        {
            // Note: It is important that these are alphabetized, because a binary search is used.
            "CompareTo",
            "Dispose",
            "Equals",
            "GetHashCode",
            "ToString"
        };

        #endregion Constants

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
            // Skip properties and constructors
            if (member.NodeType == NodeType.Property || 
                member.NodeType == NodeType.InstanceInitializer)
            {
                return this.Problems;
            }

            // Only need to check public methods (do not check constructors, methods called Dispose,
            // property getters, or operator overloads)
            Method method = member as Method;
            if (MethodRequiresTryCatch(method))
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

        /// <summary>
        /// Determines whether a try catch should be required for a method of this type.
        /// </summary>
        /// <param name="method">The method to check.</param>
        /// <returns><see langword="true"/> if <paramref name="method"/> should contain a try catch
        /// block; <see langword="false"/> if try catch block is unnecessary.</returns>
        static bool MethodRequiresTryCatch(Method method)
        {
            // Must be a method
            if (method == null)
            {
                return false;
            }

            // Only public methods require try catch
            if (!method.IsPublic)
            {
                return false;
            }

            // Skip event accessors (They shouldn't throw exceptions)
            if (method.DeclaringMember is EventNode)
            {
                return false;
            }

            // Skip methods that shouldn't throw exceptions.
            string methodName = method.Name.Name;
            if (!MethodShouldThrow(methodName))
            {
                return false;
            }

            // Skip property getters (They shouldn't throw exceptions)
            if (methodName.StartsWith("get_", StringComparison.Ordinal))
	        {
        		return false;
	        }

            // The explicit cast operator is the only operator that should throw exceptions
            if (methodName.StartsWith("op_", StringComparison.Ordinal) &&
                !methodName.Equals("op_Explicit", StringComparison.Ordinal))
	        {
                return false;
	        }

            return true;
        }

        /// <summary>
        /// Determines whether a method with the specified name should throw exceptions.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <returns><see langword="true"/> if <paramref name="methodName"/> is not one of the 
        /// excluded method names in <see cref="_METHODS_TO_SKIP"/>; <see langword="false"/> if 
        /// <paramref name="methodName"/> is a method that should never throw exceptions.</returns>
        static bool MethodShouldThrow(string methodName)
        {
            int index =
                Array.BinarySearch<string>(_METHODS_TO_SKIP, methodName, StringComparer.Ordinal);
            return index < 0;
        }

        #endregion Overrides
    }
}
