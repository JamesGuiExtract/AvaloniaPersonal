using Extract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Models;

namespace WebAPI
{
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
            HTTPError.Assert(
                eliCode, StatusCodes.Status400BadRequest,
                controller.ModelState.IsValid,
                "Invalid request format",
                controller.ModelState.GetModelStateErrorDetails().ToArray());
        }

        /// <summary>
        /// Gets an array of error details for model state violations
        /// </summary>
        /// <param name="modelState"></param>
        /// <returns></returns>
        public static IEnumerable<(string field, object error, bool visible)> GetModelStateErrorDetails(this ModelStateDictionary modelState)
        {
            return modelState
                .Where(item => item.Value.ValidationState == ModelValidationState.Invalid)
                .Select(item => (item.Key,
                    (object)string.Join("; ", item.Value.Errors.Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? error.Exception.Message
                        : error.ErrorMessage).Distinct())
                    , true));
        }

        /// <summary>
        /// Converts an exception to an HTTP error.
        /// </summary>
        /// <param name="controller">The controller in use.</param>
        /// <param name="ex">The <see cref="Exception"/>.</param>
        /// <param name="eliCode">The eli code to assign to the error.</param>
        public static IActionResult GetAsHttpError(this ControllerBase controller, Exception ex, string eliCode)
        {
            try
            {

                ex.ExtractLog(eliCode);
                var ee = ex as ExtractException;

                var errorDetails = ee?.Data
                    .OfType<DictionaryEntry>()
                    .Where(dataItem => !dataItem.Key.ToString().StartsWith("CatchELI") &&
                                       !dataItem.Key.ToString().StartsWith("CatchID") &&
                                       !dataItem.Value.ToString().StartsWith("Extract_Encrypted:"))
                    .Select(dataItem => $"{dataItem.Key}: {dataItem.Value}")
                    .ToList();

                var additionalCodes = ee?.Data
                        .OfType<DictionaryEntry>()
                        .Where(dataItem => dataItem.Key.ToString().StartsWith("CatchELI") ||
                                           dataItem.Key.ToString().StartsWith("CatchID"))
                        .Select(dataItem => dataItem.Value.ToString())
                        .ToList();

                var errorObject = new ErrorResult();
                errorObject.Error = new ErrorInfo()
                {
                    Message = ex.Message,
                    Code = ee?.EliCode,
                    AdditionalCodes = additionalCodes,
                    ErrorDetails = errorDetails
                };

                // If any exception in the stack is a request exception, the HTTP result should be a
                // bad request.
                HTTPError requestException = null;
                for (var currentException = ex;
                     currentException != null && requestException == null;
                     currentException = currentException.InnerException)
                {
                    requestException = currentException as HTTPError;
                }

                // Otherwise the error will be 500 (server error) or a manually specified error code.
                if (requestException != null)
                {
                    if (requestException.StatusCode == 0)
                    {
                        return controller.StatusCode(StatusCodes.Status500InternalServerError, errorObject);
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
