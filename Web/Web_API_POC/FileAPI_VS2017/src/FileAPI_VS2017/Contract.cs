using System;

namespace FileAPI_VS2017
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
        public static void Assert(bool condition, string statement, params object[] args)
        {
            if (!condition)
            {
                var issue = String.Format(statement, args);
                Log.WriteLine(issue);

                throw new InvalidOperationException(issue);
            }
        }

        /// <summary>
        /// contract has been violated
        /// </summary>
        /// <param name="statement"></param>
        public static void Violated(string statement)
        {
            Log.WriteLine(statement);
            throw new InvalidOperationException(statement);
        }
    }
}
