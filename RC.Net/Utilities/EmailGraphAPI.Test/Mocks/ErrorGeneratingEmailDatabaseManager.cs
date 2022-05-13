using System;

namespace Extract.Email.GraphClient.Test.Mocks
{
    internal class ErrorGeneratingEmailDatabaseManager : EmailDatabaseManager
    {
        private readonly int _errorPercent;
        private readonly Random _rng = new();

        public ErrorGeneratingEmailDatabaseManager(EmailManagementConfiguration configuration, int errorPercent)
            : base(configuration)
        {
            _errorPercent = errorPercent;
        }

        public override void ClearPendingMoveFromEmailFolder(string messageID)
        {
            if (_rng.Next(0, 100) < _errorPercent)
            {
                throw new ExtractException("ELI53437", "Test ClearPendingMoveFromEmailFolder");
            }
            base.ClearPendingMoveFromEmailFolder(messageID);
        }

        public override void ClearPendingNotifyFromEmailFolder(string messageID)
        {
            if (_rng.Next(0, 100) < _errorPercent)
            {
                throw new ExtractException("ELI53438", "Test ClearPendingNotifyFromEmailFolder");
            }
            base.ClearPendingNotifyFromEmailFolder(messageID);
        }
    }
}
