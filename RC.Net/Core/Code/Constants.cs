namespace Extract
{
    /// <summary>
    /// Global constants for use within Extract Systems code.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The Extract Systems public key string.
        /// </summary>
        public static readonly string ExtractPublicKey = "0024000004800000940000000602000000240000525341310004000001000100D52DB94FF1A0CC337264E7A70FD9D97706667394327D7927573D59AE003BF63A47CBBFB497FCFE234F854042800A8CBCA11A35E17FE12F0A021383AC0973541FF1648921C5CE72B7476138F311DD67BEBE3B1B3360A4D17A4BEC2A92514ACA3B7962D89B8FFFD7CAE5C436B5E17720987BBA72C85DFEF30A59F64D2D459912CB";

        /// <summary>
        /// The Description value of the ExternalLogin record for the EmailFileSupplier
        /// </summary>
        public static readonly string EmailFileSupplierExternalLoginDescription = "EmailFileSupplier";

        /// <summary>
        /// One-based index of a pagination document. Used by MimeKitEmailToPdfConverter
        /// </summary>
        public static readonly string LogicalDocumentNumberPdfTag = "ExtractSystems.LogicalDocumentNumber";

        /// <summary>
        /// One-based page number of a pagination document. Used by MimeKitEmailToPdfConverter
        /// </summary>
        public static readonly string LogicalPageNumberPdfTag = "ExtractSystems.LogicalPageNumber";

        /// <summary>
        /// Placeholder text. Used by RuleSetRunMode
        /// </summary>
        public static readonly string EmptyPagePlaceholderText = "__EMPTYPAGE__";

        public const string TaskClassSplitMultipageDocument = "EF1279E8-4EC2-4CBF-9DE5-E107D97916C0";
        public const string TaskClassStoreRetrieveAttributes = "B25D64C0-6FF6-4E0B-83D4-0D5DFEB68006";
        public const string TaskClassDocumentApi = "49C8149D-38D9-4EAF-A46B-CF16EBF0882F";
        public const string TaskClassWebVerification = "FD7867BD-815B-47B5-BAF4-243B8C44AABB";
        public const string TaskClassRedactionVerification = "AD7F3F3F-20EC-4830-B014-EC118F6D4567";
        public const string TaskClassDataEntryVerification = "59496DF7-3951-49B7-B063-8C28F4CD843F";
        public const string TaskClassPaginationVerification = "DF414AD2-742A-4ED7-AD20-C1A1C4993175";
        public const string TaskClassAutoPaginate = "8ECBCC95-7371-459F-8A84-A2AFF7769800";
        public const string TaskClassSpecifiedPagination = "60409EAC-5B39-498C-BA16-E45577795960";
        public const string TaskClassRtfDivideBatches = "5F37ABA6-7D18-4AB9-9ABE-79CE0F49C903";
        public const string TaskClassRtfUpdateBatches = "4FF8821E-D98A-4B45-AD1A-5E7F62621581";
        public const string TaskClassSplitMimeFile = "A941CCD2-4BF2-4D3E-8B3F-CA17AE340D73";
	}
}
