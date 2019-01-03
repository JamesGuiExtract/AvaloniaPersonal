﻿using Extract;
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
        /// Asserts that the specified controller is valid.
        /// </summary>
        /// <param name="controller">The controller to validate.</param>
        /// <param name="eliCode">The eli code to assign on error.</param>
        public static void AssertModel(this ControllerBase controller, string eliCode)
        {
            HTTPError.Assert(
                eliCode, StatusCodes.Status400BadRequest,
                controller.ModelState.IsValid,
                "Invalid request format",
                controller.ModelState.GetModelStateErrorDetails());
        }

        /// <summary>
        /// Gets an array of error details for model state violations
        /// </summary>
        /// <param name="modelState">The <see cref="ModelStateDictionary"/> defining the state of a
        /// model for which error details are needed.</param>
        /// <returns></returns>
        public static (string field, object error, bool visible)[] GetModelStateErrorDetails(this ModelStateDictionary modelState)
        {
            return modelState
                .Where(item => item.Value.ValidationState == ModelValidationState.Invalid)
                .Select(item => (item.Key,
                    (object)string.Join("; ", item.Value.Errors.Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? error.Exception.Message
                        : error.ErrorMessage).Distinct())
                    , true))
                .ToArray();
        }

        /// <summary>
        /// Converts an exception to an HTTP error.
        /// </summary>
        /// <param name="controller">The controller in use.</param>
        /// <param name="ex">The <see cref="Exception"/>.</param>
        /// <param name="eliCode">The eli code to assign to the error.</param>
        public static IActionResult GetAsHttpError(this ControllerBase controller, Exception ex, string eliCode)
        {
            int statusCode = StatusCodes.Status500InternalServerError;
            ErrorResult errorObject = null;

            try
            {
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

                errorObject = new ErrorResult();
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
                if (requestException != null && requestException.StatusCode != 0)
                {
                    statusCode = requestException.StatusCode;
                }
                else
                {
                    statusCode = StatusCodes.Status500InternalServerError;
                    // Add new top-level interal server error exception.
                    ex = new HTTPError(eliCode, statusCode, "Internal Server Error", ex);
                }
            }
            catch
            {
                statusCode = StatusCodes.Status500InternalServerError;
                // Add new top-level interal server error exception.
                ex = new HTTPError(eliCode, statusCode, "Internal Server Error", ex);
            }

            if (true != Utils.CurrentApiContext?.ExceptionLogFilter
                .Any(range => range.Contains(statusCode)))
            {
                ex.ExtractLog(eliCode);
            }

            return controller.StatusCode(statusCode, errorObject);
        }
    }
}
