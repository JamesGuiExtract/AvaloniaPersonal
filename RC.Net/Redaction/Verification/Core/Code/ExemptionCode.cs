namespace Extract.Redaction
{
    /// <summary>
    /// Represents an exemption code and its associated summary and description.
    /// </summary>
    public class ExemptionCode
    {
        #region ExemptionCode Fields

        /// <summary>
        /// The exemption code.
        /// </summary>
        readonly string _code;

        /// <summary>
        /// A summary of the exemption code in a few words.
        /// </summary>
        readonly string _summary;

        /// <summary>
        /// A detailed description of the exemption code.
        /// </summary>
        readonly string _description;

        #endregion ExemptionCode Fields

        #region ExemptionCode Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExemptionCode"/> class.
        /// </summary>
        public ExemptionCode(string code, string summary, string description)
        {
            _code = code;
            _summary = summary;
            _description = description;
        }

        #endregion ExemptionCode Constructors

        #region ExemptionCode Properties

        /// <summary>
        /// Gets the exemption code.
        /// </summary>
        /// <returns>The exemption code.</returns>
        public string Name
        {
            get
            {
                return _code;
            }
        }

        /// <summary>
        /// Gets the brief summary of the exemption code.
        /// </summary>
        /// <returns>The brief summary of the exemption code.</returns>
        public string Summary
        {
            get
            {
                return _summary;
            }
        }

        /// <summary>
        /// Gets the full description of the exemption code.
        /// </summary>
        /// <returns>The full description of the exemption code.</returns>
        public string Description
        {
            get
            {
                return _description;
            }
        }

        #endregion ExemptionCode Properties
    }
}
