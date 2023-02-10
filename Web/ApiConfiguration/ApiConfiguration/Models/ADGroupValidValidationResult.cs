using System;
using System.Collections.Generic;
using System.Linq;

namespace Extract.Web.ApiConfiguration.Models
{
    internal class ADGroupValidValidationResult
    {
        public static ADGroupValidValidationResult Failure { get; } =
            new ADGroupValidValidationResult(false, Array.Empty<string>());

        public static ADGroupValidValidationResult Success(IList<string> invalidGroups = null)
        {
            return new ADGroupValidValidationResult(true, invalidGroups ?? Array.Empty<string>());
        }

        private ADGroupValidValidationResult(
            bool validationSucceeded,
            IList<string> invalidGroups)
        {
            ValidationSucceeded = validationSucceeded;
            IsValid = validationSucceeded && !invalidGroups.Any();
            InvalidGroups = invalidGroups;
        }

        public bool IsValid { get; }

        public bool ValidationSucceeded { get; }

        public IList<string> InvalidGroups { get; }
    }
}
