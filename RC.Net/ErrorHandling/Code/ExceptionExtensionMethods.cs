using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Extract.ErrorHandling
{
    public static class ExceptionExtensionMethods
    {
        public static string AsStringizedByteStream(this Exception ex)
        {
            ExtractException ee = ex as ExtractException;
            if (ee != null)
                return ee.AsStringizedByteStream();

            ee = ex.AsExtractException("ELI51689");
            return ee.AsStringizedByteStream();
        }

        public static ExtractException AsExtractException(this Exception ex, string eliCode)
        {
            ExtractException ee = ex as ExtractException;
            if (ee != null)
                return ee;

            string stackTrace = ex?.StackTrace;

            var context = new StreamingContext(StreamingContextStates.All);
            var info = new SerializationInfo(typeof(ExtractException), new FormatterConverter());
            ex.GetObjectData(info, context);

            info.AddValue("ELICode", eliCode);
            info.AddValue("Version", ExtractException.CurrentVersion);
            info.AddValue("StackTraceRecorded", false);
            info.AddValue("StackTraceValues", new Stack<string>());

            var infoWithEmptyStackTraceString = 
                new SerializationInfo(typeof(ExtractException), new FormatterConverter());

            foreach(var s in info)
            {
                if (s.Name == "StackTraceString")
                {
                    infoWithEmptyStackTraceString.AddValue("StackTraceString", "");
                    continue;
                }
                infoWithEmptyStackTraceString.AddValue(s.Name, s.Value);
            }
            var extract =  new ExtractException(infoWithEmptyStackTraceString, context);
            if (string.IsNullOrEmpty(stackTrace))
            {
                extract.RecordStackTrace(stackTrace);
            }
            return extract;
        }
    }
}
