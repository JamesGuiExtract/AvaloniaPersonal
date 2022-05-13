using Microsoft.Graph;
using System;
using System.Threading.Tasks;

namespace Extract.Email.GraphClient.Test.Mocks
{
    internal class ErrorGeneratingEmailManagement : EmailManagement
    {
        private readonly int _errorPercent;
        private readonly Random _rng = new();

        public ErrorGeneratingEmailManagement(EmailManagementConfiguration configuration, int errorPercent)
            : base(configuration)
        {
            _errorPercent = errorPercent;
        }

        public override Task<Message> MoveMessageToQueuedFolder(string messageID)
        {
            if (_rng.Next(0, 100) < _errorPercent)
            {
                throw new ExtractException("ELI53446", "Test MoveMessageToQueuedFolder");
            }
            return base.MoveMessageToQueuedFolder(messageID);
        }
    }
}
