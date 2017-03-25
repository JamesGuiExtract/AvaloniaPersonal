using Extract;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace DocumentAPI
{
    /// <summary>
    /// A simple log class
    /// </summary>
    public class Log
    {
        /// <summary>
        /// write text to a log
        /// </summary>
        /// <param name="text">text to write - may optionally contain format items (e.g. {0})</param>
        /// NOTE: The following optional arguments are not meant to be explicitly set - let them default and
        /// they will have the correct, intended values.
        /// <param name="eliCode">an optional ELI code, by default set to the "Web API ELI code"</param>
        /// <param name="memberName">DO NOT SET! - optional name of caller - normally this is specified by the compiler</param>
        /// <param name="filePath">DO NOT SET! - The optional source file of the calling method</param>
        /// <param name="sourceLineNumber">DO NOT SET! - optional caller line number - normally this is specified by the compiler</param>
        public static void WriteLine(string text,
                                     string eliCode = "ELI42111",                     
                                     [CallerMemberName] string memberName = "",
                                     [CallerFilePath] string filePath = "",
                                     [CallerLineNumber] int sourceLineNumber = 0)
        {
            var message = Utils.Inv($"{text}, Source file: {filePath}, Function: {memberName}, line number: {sourceLineNumber}");
            ExtractException.Log(eliCode, message);
        }

        /// <summary>
        /// write text to a log
        /// </summary>
        /// <param name="ee">ExtractException to write</param>
        public static void WriteLine(ExtractException ee)
        {
            ee.Log();
        }
    }
}
