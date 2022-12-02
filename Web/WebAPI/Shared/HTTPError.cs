using Microsoft.AspNetCore.Http;
using System;
using Extract;

namespace WebAPI
{
    /// <summary>
    /// Provides assertions and error information for exceptions intended to be sent in response to
    /// an HTTP request.
    /// </summary>
    public class HTTPError : ExtractException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HTTPError"/> class.
        /// </summary>
        /// <param name="eliCode">The eli code to assign on error.</param>
        /// <param name="message">The message to assign on error.</param>
        public HTTPError(string eliCode, string message)
            : this(eliCode, StatusCodes.Status500InternalServerError, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HTTPError"/> class.
        /// </summary>
        /// <param name="eliCode">The eli code to assign on error.</param>
        /// <param name="message">The message to assign on error.</param>
        /// <param name="statusCode">The <see cref="StatusCodes"/> value to return on error.</param>
        public HTTPError(string eliCode, int statusCode, string message)
            : base(eliCode, message)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HTTPError"/> class.
        /// </summary>
        /// <param name="eliCode">The eli code to assign on error.</param>
        /// <param name="message">The message to assign on error.</param>
        /// <param name="statusCode">The HTTP status code value to return on error.</param>
        /// <param name="ex">Additional error data in the form of and <see cref="Exception"/>.</param>
        public HTTPError(string eliCode, int statusCode, string message, Exception ex)
            : base(eliCode, message, ex)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// The HTTP status code value to return.
        /// </summary>
        public int StatusCode
        {
            get;
            private set;
        }

        /// <summary>
        /// assert that a condition is true
        /// NOTE: the statement argument is intended to be a format string. The arguments to the 
        /// format string are passed separately, so that statement expansion (formatting) only takes
        /// place when necessary.
        /// </summary>
        /// <param name="eliCode">ELI code identifying a specific violation.</param>
        /// <param name="statusCode">The HTTP status code to associate with the error.</param>
        /// <param name="condition">condition that must be true</param>
        /// <param name="message">a format string</param>
        /// <param name="debugData">The arguments.</param>
        public static void Assert(string eliCode, int statusCode, bool condition,
            string message, params (string field, object value, bool visible)[] debugData)

        {
            if (!condition)
            {
                var ee = new HTTPError(eliCode, statusCode, message);
                foreach (var dataItem in debugData)
                {
                    ee.AddDebugData(dataItem.field, dataItem.value?.ToString(), !dataItem.visible);
                }
                throw ee;
            }
        }

        /// <summary>
        /// Asserts an internal code condition (500 internal server error)
        /// NOTE: the statement argument is intended to be a format string. The arguments to the 
        /// format string are passed separately, so that statement expansion (formatting) only takes
        /// place when necessary.
        /// </summary>
        /// <param name="eliCode">ELI code identifying a specific violation.</param>
        /// <param name="condition">condition that must be true</param>
        /// <param name="message">a format string</param>
        /// <param name="debugData">The arguments.</param>
        public static void Assert(string eliCode, bool condition,
            string message, params (string field, object value, bool visible)[] debugData)

        {
            Assert(eliCode, StatusCodes.Status500InternalServerError, condition, message, debugData);
        }

        /// <summary>
        /// Asserts an expectation for a request (400 bad request)
        /// </summary>
        /// <param name="eliCode">ELI code identifying a specific violation.</param>
        /// <param name="condition">The condition to assert.</param>
        /// <param name="message">An explanation of the error.</param>
        /// <param name="debugData">The arguments.</param>
        public static void AssertRequest(string eliCode, bool condition, string message,
            params (string field, object value, bool visible)[] debugData)

        {
            Assert(eliCode, StatusCodes.Status400BadRequest,
                condition, message, debugData);
        }
    }
}
