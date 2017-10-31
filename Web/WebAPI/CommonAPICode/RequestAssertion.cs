using Extract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using WebAPI.Models;

namespace WebAPI
{
    /// <summary>
    /// Represents an error to return from an API call.
    /// </summary>
    internal class RequestAssertion : ExtractException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestAssertion"/> class.
        /// </summary>
        /// <param name="eliCode">The eli code to assign on error.</param>
        /// <param name="message">The message to assign on error.</param>
        public RequestAssertion(string eliCode, string message)
            : this(eliCode, message, StatusCodes.Status400BadRequest)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestAssertion"/> class.
        /// </summary>
        /// <param name="eliCode">The eli code to assign on error.</param>
        /// <param name="message">The message to assign on error.</param>
        /// <param name="statusCode">The <see cref="StatusCodes"/> value to return on error.</param>
        public RequestAssertion(string eliCode, string message, int statusCode)
            : base(eliCode, message)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestAssertion"/> class.
        /// </summary>
        /// <param name="eliCode">The eli code to assign on error.</param>
        /// <param name="message">The message to assign on error.</param>
        /// <param name="statusCode">The HTTP status code value to return on error.</param>
        /// <param name="ex">Additional error data in the form of and <see cref="Exception"/>.</param>
        public RequestAssertion(string eliCode, string message, int statusCode, Exception ex)
            : base(eliCode, message, ex)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the The HTTP status code value to return.
        /// </summary>
        public int StatusCode
        {
            get;
            private set;
        }

        /// <summary>
        /// Validates <see paramref="value"/> has been specified for a request.
        /// </summary>
        /// <param name="eliCode">The eli code to assign on error.</param>
        /// <param name="value">The value to check</param>
        /// <param name="message">The message to assign on error.</param>
        public static void AssertSpecified(string eliCode, object value, string message)
        {
            var valueAsString = value as string;
            if (value == null ||
                (valueAsString != null && string.IsNullOrWhiteSpace(valueAsString)))
            {
                throw new RequestAssertion(eliCode, message);
            }
        }

        /// <summary>
        /// Validates a <see paramref="condition"/> has been met for a request.
        /// </summary>
        /// <param name="eliCode">The eli code to assign on error.</param>
        /// <param name="condition">The condition result.</param>
        /// <param name="message">The message to assign on error.</param>
        public static void AssertCondition(string eliCode, bool condition, string message)
        {
            if (!condition)
            {
                throw new RequestAssertion(eliCode, message);
            }
        }
    }

    /// <summary>
    /// Tools to validate API requests.
    /// </summary>
    internal static class ControllerExceptionHandler
    {
        /// <summary>
        /// Asserts that the specified <see paramref="controller"/> is valid.
        /// </summary>
        /// <param name="controller">The controller to validate.</param>
        /// <param name="eliCode">The eli code to assign on error.</param>
        public static void AssertModel(this ControllerBase controller, string eliCode)
        {
            if (!controller.ModelState.IsValid)
            {
                throw new RequestAssertion(eliCode, "Invalid request format");
            }
        }

        /// <summary>
        /// Converts an exception to an HTTP error.
        /// </summary>
        /// <param name="controller">The controller in use.</param>
        /// <param name="ex">The <see cref="Exception"/>.</param>
        /// <param name="eliCode">The eli code to assign to the error.</param>
        public static IActionResult GetAsHttpError(this ControllerBase controller, Exception ex, string eliCode)
        {
            return GetAsHttpError(controller, ex, eliCode, ex.Message);
        }

        /// <summary>
        /// Converts an exception to an HTTP error.
        /// </summary>
        /// <param name="controller">The controller in use.</param>
        /// <param name="ex">The <see cref="Exception"/>.</param>
        /// <param name="eliCode">The eli code to assign to the error.</param>
        public static IActionResult GetAsHttpError<T>(this ControllerBase controller, Exception ex, string eliCode)
            where T : IResultData, new()
        {
            IResultData errorObject = new T();
            errorObject.Error = Utils.MakeError(true, ex.Message, -1);

            return GetAsHttpError(controller, ex, eliCode, errorObject);
        }

        /// <summary>
        /// Converts an exception to an HTTP error.
        /// </summary>
        /// <param name="controller">The controller in use.</param>
        /// <param name="ex">The <see cref="Exception" />.</param>
        /// <param name="eliCode">The eli code to assign to the error.</param>
        /// <param name="errorObject">The object representing the error data</param>
        static IActionResult GetAsHttpError(this ControllerBase controller, Exception ex, string eliCode, object errorObject)
        {
            try
            {
                ex.ExtractLog(eliCode);

                // If any exception in the stack is a request exception, the HTTP result should be a
                // bad request.
                RequestAssertion requestException = null;
                for (var currentException = ex;
                     currentException != null && requestException == null;
                     currentException = currentException.InnerException)
                {
                    requestException = currentException as RequestAssertion;
                }

                // Otherwise the error will be 500 (server error) or a manually specified error code.
                if (requestException != null)
                {
                    if (requestException.StatusCode == 0)
                    {
                        return controller.BadRequest(errorObject);
                    }
                    else
                    {
                        return controller.StatusCode(requestException.StatusCode, errorObject);
                    }
                }
                else
                {
                    return controller.StatusCode(StatusCodes.Status500InternalServerError, errorObject);
                }
            }
            catch
            {
                return controller.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
