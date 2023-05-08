using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using HandlebarsDotNet;

namespace Extract.ErrorHandling
{
    public static class ExceptionExtensionMethods
    {
        public static Lazy<HandlebarsTemplate<object, object>> ToHtmlTemplate = new(() =>
            { 
                // Make an explicit instance rather than using the shared static
                var hb = Handlebars.Create();

                //HTML Layout
                string source = GetEmbeddedResource("HandleBarTemplates.SourceTemplate.html");

                //ELI error section
                string errorSection = GetEmbeddedResource("HandleBarTemplates.ErrorSectionTemplate.html");
                hb.RegisterTemplate("Error", errorSection);

                //Debug info section
                string debugInfo = GetEmbeddedResource("HandleBarTemplates.DebugSectionTemplate.html");
                hb.RegisterTemplate("DebugInfo", debugInfo);

                //Stacktrace section
                string stackTraceLine = GetEmbeddedResource("HandleBarTemplates.StackTraceSectionTemplate.html");
                hb.RegisterTemplate("StackTraceLine", stackTraceLine);

                return hb.Compile(source);
            });

        public static string AsStringizedByteStream(this Exception ex)
        {
            ExtractException ee = ex as ExtractException;
            if (ee != null)
                return ee.AsStringizedByteStream();

            ee = ex.AsExtractException("ELI51689");
            return ee.AsStringizedByteStream();
        }

        public static ExtractException AsExtract(this Exception ex, string eliCode)
        {
            return ex.AsExtractException(eliCode);
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

            info.AddValue("EliCode", eliCode);
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
            var extract = new ExtractException(infoWithEmptyStackTraceString, context);
            extract.RecordStackTrace(stackTrace);

            return extract;
        }


        public static void Assert(string eliCode, string message, bool condition)
        {
            Assert(condition, eliCode, message);
        }
         
        /// <summary>
        /// Throws an ExtractException built from the provided EliCode and message
        /// if the condition provided is false, otherwise does nothing.
        /// </summary>
        /// <param name="condition">A boolean expression to test</param>
        /// <param name="eliCode">A unique Extract Systems ELI Code</param>
        /// <param name="message">The message to associate with this exception</param>
        /// <param name="configureException">Optional lambda to add debug data to the exception</param>
        public static void Assert(
#if NET6
            [DoesNotReturnIf(false)] 
#endif
            this bool condition,

            string eliCode, string message, Action<Exception> configureException = null)
        {
            if (condition)
            {
                return;
            }

            ExtractException ee = new(eliCode, message ?? "Condition not met");

            if (configureException is not null)
            {
                try
                {
                    configureException(ee);
                }
                catch (Exception ex)
                {
                    ex.AsExtract("ELI53915").Log();
                }
            }

            throw ee;
        }

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        /// <summary>
        /// Throws an ExtractException built from the provided EliCode and value name if the provided value is null
        /// if the condition provided is false, otherwise does nothing.
        /// </summary>
        /// <param name="maybeValue">A possibly null value to test</param>
        /// <param name="eliCode">A unique Extract Systems ELI Code</param>
        /// <param name="message">The message to associate with this exception</param>
        public static T AssertNotNull<T>(this T? maybeValue, string eliCode, string message)
        {
            if (maybeValue is T t)
            {
                return t;
            }

            throw new ExtractException(eliCode, message);
        }
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

        /// <summary>
        /// Builds an HTML file summarizing the current ExtractException 
        /// </summary>
        public static string ToHtml(this ExtractException exn)
        {
            //Get appropriate error data for binding
            var eliStack = getDisplayInfo(exn);
            ExceptionStackVM viewData = new ExceptionStackVM()
            {
                ELIStack = eliStack
            };

            //Generate HTML using templates and bindings
            var result = ToHtmlTemplate.Value(viewData);
            return result;
        }

        /// <summary>
        /// Gets error details from the current and any inner errors
        /// </summary>
        /// <returns>An object containing error data ready to be bound to the generated HTML</returns>
        static Stack<ExceptionVM> getDisplayInfo(this ExtractException exn)
        {
            //Get the inner exception
            ExtractException inner = exn.InnerException as ExtractException;
            Stack<ExceptionVM> exceptionDisplayData;

            //Run on inner exceptions if they exist
            exceptionDisplayData = inner?.getDisplayInfo() ?? new Stack<ExceptionVM>();

            //Get a formatted list of debug data
            var debugData = exn.Data as ExceptionData;
            List<DebugVM> debugDataList = new List<DebugVM>();
            foreach (var entry in debugData.GetFlattenedData())
            {
                Object value;
                //Check if the debug value is encrypted, decrypt if it is
                if (entry.Value.GetType() == typeof(string) && ((string)entry.Value).StartsWith(ExtractException._ENCRYPTED_PREFIX))
                {
                    value = DebugDataHelper.GetValueAsType<string>(entry.Value);

                    //If decryption doesn't work as expected, mark the field as encrypted
                    if (((string)value).StartsWith(ExtractException._ENCRYPTED_PREFIX))
                    {
                        value = "<ENCRYPTED>";
                    }
                }
                else
                {
                    value = entry.Value;
                }
                debugDataList.Add(new DebugVM()
                {
                    Key = entry.Key as string,
                    Value = value,
                    Type = entry.Value.GetType()
                });
            }

            //Get a formatted list of stack trace lines
            List<StackTraceVM> stacktraceLines = new List<StackTraceVM>();
            foreach (string line in exn.StackTraceValues)
            {
                //Check if the line is encrypted, decrypt if so
                string stacktraceLine;
                if (line.StartsWith(ExtractException._ENCRYPTED_PREFIX))
                {
                    stacktraceLine = DebugDataHelper.GetValueAsType<string>(line);

                    //If decryption doesn't work as expected, mark the field as encrypted
                    if (stacktraceLine.StartsWith(ExtractException._ENCRYPTED_PREFIX))
                    {
                        stacktraceLine = "<ENCRYPTED>";
                    }
                }
                else
                {
                    stacktraceLine = line;
                }
                stacktraceLines.Add(new StackTraceVM()
                {
                    Line = stacktraceLine
                });
            }
            int curIndex = exceptionDisplayData.Count;
            //Compile and return the collected error information
            exceptionDisplayData.Push(new ExceptionVM()
            {
                ELICode = exn.EliCode,
                ELIMessage = exn.Message,
                Index = curIndex,
                DebugDisplay = (debugDataList.Count > 0 ? "list-item" : "none"),
                StackTraceDisplay = (stacktraceLines.Count > 0 ? "list-item" : "none"),
                DebugInfo = debugDataList,
                StackTraceLines = stacktraceLines
            });
            return exceptionDisplayData;
        }

        /// <summary>
        /// Pulls file text from embedded resources
        /// </summary>
        /// <param name="docName">The name of the embedded resource file</param>
        /// <returns>The text of the embedded resource file</returns>
        private static string GetEmbeddedResource(string docName)
        {
            using var stream = typeof(ExceptionExtensionMethods).Assembly.GetManifestResourceStream(typeof(ExceptionExtensionMethods), docName);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

    }
}
