using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.Email.GraphClient.Test.Utilities
{
    /// <summary>
    /// Generate OperationCanceledException, FKA timeout exceptions
    /// https://extract.atlassian.net/browse/ISSUE-18227
    /// https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/pull/109
    /// </summary>
    internal class TimeoutGeneratingHandler : DelegatingHandler
    {
        private readonly TimeSpan _alwaysTimeout = TimeSpan.FromMilliseconds(10);
        private readonly TimeSpan _normalTimeout = TimeSpan.FromSeconds(100);
        private readonly Random _rng = new();
        private readonly int _percentTimeouts;

        public TimeoutGeneratingHandler(int percentTimeouts)
        {
            _percentTimeouts = percentTimeouts;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var shouldTimeout = _rng.Next(100) < _percentTimeouts;
            using CancellationTokenSource cts = new();
            cts.CancelAfter(shouldTimeout ? _alwaysTimeout : _normalTimeout);
            var timeoutToken = cts.Token;

            using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken);

            return await base.SendAsync(request, linkedToken.Token).ConfigureAwait(false);
        }
    }
}
