using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Extract.Utilities
{
    /// <summary>
    /// Utility to handle parsing commandline arguments for a WinExe (display or log help/errors)
    /// </summary>
    public static class CommandLineHandler
    {
        /// <summary>
        /// Parse args and run a program that returns a <see cref="Result{TSuccess}"/>
        /// </summary>
        /// <typeparam name="TOptions">The type that defines the commandline parameters</typeparam>
        /// <typeparam name="TSuccessResult">The type contained in a successful result object</typeparam>
        /// <param name="args">The commandline args</param>
        /// <param name="program">The function that will be run with the parsed parameters</param>
        /// <returns>A <see cref="Result{TSuccess}"/> that represents the outcome of the program</returns>
        /// <remarks>
        /// If /ef ExceptionFile argument is given then usage help/error messages will be logged to ExceptionFile
        /// else they will be displayed in a window
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static Result<TSuccessResult> HandleCommandLine<TOptions, TSuccessResult>(
            string[] args, Func<TOptions, Result<TSuccessResult>> program)
        {
            return new CommandLineHandler<TOptions, TSuccessResult>()
                .HandleCommandLine(args, program);
        }
    }

    // Implementation of the commandline handler
    internal class CommandLineHandler<TOptions, TSuccessResult>
    {
        readonly StringBuilder helpTextBuilder = new();
        bool displayErrors;
        bool logErrors;

        // Parse args and run a program
        internal Result<TSuccessResult> HandleCommandLine(string[] args, Func<TOptions, Result<TSuccessResult>> program)
        {
            args = FilterExceptionFileArgument(args ?? Array.Empty<string>(), out string maybeExceptionFile);

            logErrors = maybeExceptionFile is not null;
            displayErrors = !logErrors;

            using StringWriter helpTextWriter = new(helpTextBuilder, CultureInfo.CurrentCulture);
            using Parser parser = new(settings =>
            {
                settings.HelpWriter = helpTextWriter;
                settings.MaximumDisplayWidth = 60;
            });
            ParserResult<TOptions> parserResult = parser.ParseArguments<TOptions>(args);

            Result<TSuccessResult> result = parserResult.MapResult(program, HandleParseError);

            if (result.ResultType == ResultType.Failure)
            {
                if (logErrors)
                {
                    result.Exception.Log(maybeExceptionFile);
                }
                else if (displayErrors)
                {
                    result.Exception.Display();
                }
            }

            return result;
        }

        // Search the arguments for /ef <ExceptionFile> and return the other arguments as a new array
        private static string[] FilterExceptionFileArgument(string[] args, out string maybeExceptionFile)
        {
            maybeExceptionFile = null;

            bool nextArgIsExceptionFile = false;
            List<string> filteredArgs = new();
            foreach (string arg in args)
            {
                if (nextArgIsExceptionFile)
                {
                    maybeExceptionFile = arg;
                    nextArgIsExceptionFile = false;
                }
                else if (string.Equals("/ef", arg, StringComparison.OrdinalIgnoreCase))
                {
                    nextArgIsExceptionFile = true;
                }
                else
                {
                    filteredArgs.Add(arg);
                }
            }

            return filteredArgs.ToArray();
        }

        // Display or log an error/usage message
        private Result<TSuccessResult> HandleParseError(IEnumerable<Error> errors)
        {
            try
            {
                bool isHelpRequested = errors.Any(error => error.Tag == ErrorType.HelpRequestedError
                    || error.Tag == ErrorType.HelpVerbRequestedError);
                string caption = isHelpRequested ? "Help requested" : "Bad arguments";

                if (displayErrors)
                {
                    string usageMessage = helpTextBuilder.ToString();
                    UtilityMethods.ShowMessageBox(usageMessage, caption, !isHelpRequested);
                }

                var uex = new ExtractException("ELI53214", caption);
                foreach (var error in errors)
                {
                    uex.AddDebugData("Error", error.ToString());
                }

                return new(uex);
            }
            catch (Exception ex)
            {
                return new(ex.AsExtract("ELI53215"));
            }
        }
    }

    /// <summary>
    /// Tag used to keep track of the result category
    /// </summary>
    public enum ResultType
    {
        Nothing, // No result, e.g., no action needed
        Success, // The action was successful
        Failure, // The action failed with an exception
    }

    /// <summary>
    /// Helper class used to construct generic <see cref="Result{T}"/> instances
    /// </summary>
    public static class Result
    {
        /// <summary>
        /// Create a successful Result, wrapping a value of some kind
        /// </summary>
        public static Result<TSuccess> CreateSuccess<TSuccess>(TSuccess successValue)
        {
            return new Result<TSuccess>(successValue);
        }

        /// <summary>
        /// Create a failure Result, wrapping an ExtractException
        /// </summary>
        public static Result<TSuccess> CreateFailure<TSuccess>(ExtractException exception)
        {
            return new Result<TSuccess>(exception);
        }

        /// <summary>
        /// Create a Nothing result
        /// </summary>
        public static Result<TSuccess> CreateNothing<TSuccess>()
        {
            return new Result<TSuccess>();
        }

        /// <summary>
        /// Create a result from a JSON string
        /// </summary>
        public static Result<TSuccess> FromString<TSuccess>(string json)
        {
            return JsonConvert.DeserializeObject<Result<TSuccess>>(json);
        }
    }

    /// <summary>
    /// Serializable type that represents outcome of a program with three possible categories
    /// </summary>
    public class Result<TSuccess>
    {
        /// <summary>
        /// The tag that indicates the type of the result
        /// </summary>
        public ResultType ResultType { get; }

        /// <summary>
        /// Value that is available when the result type is Success
        /// </summary>
        public TSuccess SuccessValue { get; }

        /// <summary>
        /// Exception that is available when the result type is Failure
        /// </summary>
        [JsonIgnore]
        public ExtractException Exception { get; }

        /// <summary>
        /// String form of the exception that is available when the result type is Failure
        /// </summary>
        public string SerializedException { get; }

        /// <summary>
        /// Construct a Nothing result
        /// </summary>
        public Result()
            : this(ResultType.Nothing, default, default)
        { }

        /// <summary>
        /// Construct a Success result
        /// </summary>
        public Result(TSuccess successValue)
            : this(ResultType.Success, successValue, default)
        { }

        /// <summary>
        /// Construct a Failure result
        /// </summary>
        public Result(ExtractException exception)
        {
            ResultType = ResultType.Failure;
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            SerializedException = exception.AsStringizedByteStream();
        }

        /// <summary>
        /// Construct a result
        /// </summary>
        [JsonConstructor]
        public Result(ResultType resultType, TSuccess successValue, string serializedException)
        {
            ResultType = resultType;

            if (resultType == ResultType.Success)
            {
                SuccessValue = successValue;
            }
            else if (ResultType == ResultType.Failure)
            {
                SerializedException = serializedException ?? throw new ArgumentNullException(nameof(serializedException));
                Exception = ExtractException.FromStringizedByteStream("ELI53217", serializedException);
            }
        }

        /// <summary>
        /// Convert this instance to a JSON string
        /// </summary>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
