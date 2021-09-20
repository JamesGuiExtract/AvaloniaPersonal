using System;

namespace Extract.Utilities
{
    /// Handle encoding/decoding password complexity requirements from/to a string
    public class PasswordComplexityRequirements
    {
        public int LengthRequirement { get; }
        public bool RequireUppercase { get; }
        public bool RequireLowercase { get; }
        public bool RequireDigit { get; }
        public bool RequirePunctuation { get; }

        /// Creates requirements with length of 8 and at least one upper, lower and digit char
        public PasswordComplexityRequirements()
            : this(8, true, true, true, false)
        { }

        /// Decode requirements encoded as #+[U|L|D|P]+
        ///	i.e., one or more digits specifying the minimum length followed by letters denoting the required character categories
        ///	where U = Uppercase, L = Lowercase, D = Digit and P = Punctuation
        /// If encodedRequirements is empty then the only requirement is length > 0
        /// If encodedRequirements is invalid then 8ULDP will be used (require length >= 8 and at least one of each category)
        public PasswordComplexityRequirements(string encodedRequirements)
            : this(DecodeRequirements(encodedRequirements))
        { }

        public PasswordComplexityRequirements(PasswordComplexityRequirements copyFrom)
            : this(lengthRequirement: copyFrom.LengthRequirement,
                   requireUppercase: copyFrom.RequireUppercase,
                   requireLowercase: copyFrom.RequireLowercase,
                   requireDigit: copyFrom.RequireDigit,
                   requirePunctuation: copyFrom.RequirePunctuation)
        { }

        public PasswordComplexityRequirements(
            int lengthRequirement,
            bool requireUppercase,
            bool requireLowercase,
            bool requireDigit,
            bool requirePunctuation)
        {
            ExtractException.Assert("ELI51872", "Length requirement must be at least 1", lengthRequirement > 0);

            RequireUppercase = requireUppercase;
            RequireLowercase = requireLowercase;
            RequireDigit = requireDigit;
            RequirePunctuation = requirePunctuation;
            LengthRequirement = lengthRequirement;
        }

        /// Encode the password complexity as a string of the form #+[U|L|D|P]+
        ///	i.e., one or more digits specifying the minimum length followed by letters denoting the required character categories
        ///	where U = Uppercase, L = Lowercase, D = Digit and P = Punctuation
        public string EncodeRequirements()
        {
            try
            {
                string pwdReq = UtilityMethods.FormatInvariant($"{LengthRequirement}");
                if (RequireUppercase)   pwdReq += "U";
                if (RequireLowercase)   pwdReq += "L";
                if (RequireDigit)       pwdReq += "D";
                if (RequirePunctuation) pwdReq += "P";
                return pwdReq;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51879");
            }
        }

        private static PasswordComplexityRequirements DecodeRequirements(string pwdReq)
        {
            NativeMethods.decodePasswordComplexityRequirements(
                complexityRequirements: pwdReq,
                lengthRequirement: out int lengthRequirement,
                requireUppercase: out bool requireUppercase,
                requireLowercase: out bool requireLowercase,
                requireDigit: out bool requireDigit,
                requirePunctuation: out bool requirePunctuation);

            return new(
                lengthRequirement: lengthRequirement,
                requireUppercase: requireUppercase,
                requireLowercase: requireLowercase,
                requireDigit: requireDigit,
                requirePunctuation: requirePunctuation);
        }
    }
}
