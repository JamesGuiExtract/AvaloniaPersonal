using Extract.Utilities;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.Email.GraphClient
{
    /// <summary>
    /// When used as an inner handler to the built-in Graph RetryHandler this will log information about retry attemps
    /// as well any wep request failure responses. Exceptions are not logged here.
    /// </summary>
    public class HttpRetryLogger : DelegatingHandler
    {
        const string RETRY_ATTEMPT = "Retry-Attempt";
        private readonly string _failureMessage;
        private readonly string _retryAttemptMessage;

        public HttpRetryLogger(string name)
        {
            _failureMessage = UtilityMethods.FormatInvariant($"Application trace: ({name}) web request failure");
            _retryAttemptMessage = UtilityMethods.FormatInvariant($"Application trace: ({name}) web request retry attempt");
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            LogRetryException(request);

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            LogFailureException(response);

            return response;
        }

        private void LogRetryException(HttpRequestMessage request)
        {
            try
            {
                if (request.Headers.Contains(RETRY_ATTEMPT))
                {
                    var uex = new ExtractException("ELI53397", _retryAttemptMessage);
                    uex.AddDebugData("Request path", request.RequestUri.AbsolutePath);
                    uex.AddDebugData("Retry attempt", request.Headers.GetValues(RETRY_ATTEMPT).FirstOrDefault());
                    uex.Log();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI53407");
            }
        }

        private void LogFailureException(HttpResponseMessage response)
        {
            try
            {
                if (!response.IsSuccessStatusCode)
                {
                    var uex = new ExtractException("ELI53398", _failureMessage);
                    uex.AddDebugData("Request path", response.RequestMessage.RequestUri.AbsolutePath);
                    if (response.RequestMessage.Headers.Contains(RETRY_ATTEMPT))
                    {
                        uex.AddDebugData("Retry attempt", response.RequestMessage.Headers.GetValues(RETRY_ATTEMPT).FirstOrDefault());
                    }

                    uex.AddDebugData("Status code", response.StatusCode);
                    uex.AddDebugData("Reason phrase", response.ReasonPhrase);

                    uex.Log();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI53408");
            }
        }

    }
}
