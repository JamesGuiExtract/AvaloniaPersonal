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
    }
}
