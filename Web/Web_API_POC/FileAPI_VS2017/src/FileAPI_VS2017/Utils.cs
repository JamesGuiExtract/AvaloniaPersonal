using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.FormattableString;

using FileAPI_VS2017.Models;

namespace FileAPI_VS2017
{
    /// <summary>
    /// static Utils are kept here for global use
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Inv - short form of Invariant. Normally I would use the full name, but in this case the 
        /// full name is just noise, a distraction from the more important functionality. All this
        /// function does is prevent FXCop warnings!
        /// </summary>
        /// <param name="strings">strings - one or more strings to format</param>
        /// <returns></returns>
        public static string Inv(params FormattableString[] strings)
        {
            return string.Join("", strings.Select(str => FormattableString.Invariant(str)));
        }

        /// <summary>
        /// make an error info instance
        /// </summary>
        /// <param name="isError"></param>
        /// <param name="message"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static ErrorInfo MakeError(bool isError, string message = "", int code = 0)
        {
            return new ErrorInfo
            {
                ErrorOccurred = isError,
                Message = message,
                Code = code
            };
        }

        /// <summary>
        /// Make a list of Processing status, with one ProcessingStatus element.
        /// </summary>
        /// <param name="isError"></param>
        /// <param name="message"></param>
        /// <param name="status"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static List<ProcessingStatus> MakeListProcessingStatus(bool isError, 
                                                                      string message, 
                                                                      DocumentProcessingStatus status,
                                                                      int code = 0)
        {
            var ps = new ProcessingStatus
            {
                Error = MakeError(isError, message, code: 0),
                DocumentStatus = status
            };

            List<ProcessingStatus> lps = new List<ProcessingStatus>();
            lps.Add(ps);

            return lps;
        }

    }
}
