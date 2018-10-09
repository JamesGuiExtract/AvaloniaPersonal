using System;

namespace Extract
{
    static class ExtractException
    {
        public static Exception AsExtract(this Exception ex, string eli)
        {
            return new Exception(eli + ":" + ex.Message, ex);
        }

        public static void Assert(string eliCode, string message, bool condition)
        {
            if (!condition)
            {
                // Create the new ExtractException
                Exception ex;
                if (string.IsNullOrEmpty(message))
                {
                    ex = new Exception("Condition not met.");
                }
                else
                {
                    ex = new Exception(message);
                }

                // Throw the new exception
                throw ex.AsExtract(eliCode);
            }
        }

        public static void Display(this Exception ex)
        {
            Extract64.Core.ExceptionMethods.DisplayException(ex);
        }

        public static void Log(this Exception ex)
        {
            Extract64.Core.ExceptionMethods.LogException(ex);
        }

        public static void Log(this Exception ex, string fileName)
        {
            Extract64.Core.ExceptionMethods.LogException(ex, fileName);
        }

        /// <summary>
        /// Returns a quoted version of the supplied string if quotes are needed, else the original string.
        /// <example>For quote char as single-quote and the input value is "Hello World"
        /// then the result will be "Hello World" but if the input is "Hello World's Best Dad" then
        /// the output will be "'Hello World''s Best Dad'".</example>
        /// </summary>
        /// <param name="value">The <see cref="String"/> to quote.</param>
        /// <param name="quote">The char to use for quoting</param>
        /// <param name="delimiter">A string the presence of which requires the string be quoted (e.g., comma in CSV)</param>
        /// <returns>A quoted version of the input string.</returns>
        public static string QuoteIfNeeded(this string value, string quote, string delimiter)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                {
                    return value;
                }
                else if (string.IsNullOrWhiteSpace(value))
                {
                    return quote + value + quote;
                }
                else if (value.Contains(quote) || value.Contains(delimiter))
                {
                    return quote + value.Replace(quote, quote + quote) + quote;
                }
                else
                {
                    return value;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46332");
            }
        }
    }
}
