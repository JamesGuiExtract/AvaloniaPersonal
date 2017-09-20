using Extract;
using System;

namespace WebAPI
{
    /// <summary>
    /// class for contract support - assert and violated
    /// </summary>
    public class Contract
    {
        /// <summary>
        /// assert that a condition is true
        /// NOTE: the statement argument is intended to be a format string. The arguments to the 
        /// format string are passed separately, so that statement expansion (formatting) only takes
        /// place when necessary.
        /// </summary>
        /// <param name="condition">condition that must be true</param>
        /// <param name="statement">a format string</param>
        /// <param name="args">optional arguments for the format string</param>
        public static void Assert(bool condition, 
                                  string statement, 
                                  params object[] args)

        {
            if (!condition)
            {
                var issue = String.Format(statement, args);
                var ee = new ExtractException("ELI43256", issue);
                throw ee;
            }
        }

        /// <summary>
        /// contract has been violated
        /// </summary>
        /// <param name="statement">description of the contract violation</param>
        public static void Violated(string statement)
        {
            var ee = new ExtractException("ELI43257", statement);
            throw ee;
        }
    }
}
