using System;
using UCLID_AFCORELib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// Class to hold the attribute, name and test code (if found) of a specific lab test.
    /// </summary>
    internal class LabTest
    {
        #region Fields

        /// <summary>
        /// The attribute associated with this test
        /// </summary>
        readonly IAttribute _attribute;

        /// <summary>
        /// The name associated with this test
        /// </summary>
        readonly string _name;

        /// <summary>
        /// The test code for this test
        /// </summary>
        string _testCode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LabTest"/> class.
        /// </summary>
        /// <param name="attribute">The attribute associated with this object. Must
        /// not be <see langword="null"/>.</param>
        public LabTest(IAttribute attribute)
        {
            try
            {
                ExtractException.Assert("ELI26441", "Attribute cannot be null!", attribute != null);

                // Store the attribute
                _attribute = attribute;

                // Set the name based on the attributes spatial string
                SpatialString ss = _attribute.Value;
                _name = ss.String;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26442", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the attribute for this <see cref="LabTest"/>.
        /// </summary>
        /// <returns>The attribute for this <see cref="LabTest"/>.</returns>
        public IAttribute Attribute
        {
            get
            {
                return _attribute;
            }
        }

        /// <summary>
        /// Gets the name for this <see cref="LabTest"/>.
        /// </summary>
        /// <returns>The name for this <see cref="LabTest"/>.</returns>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets or sets the test code for this <see cref="LabTest"/>.
        /// </summary>
        public string TestCode
        {
            get
            {
                return _testCode;
            }
            set
            {
                _testCode = value;
            }
        }

        /// <summary>
        /// Gets or sets the sample type for this <see cref="LabTest"/>.
        /// </summary>
        public string SampleType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the score of this mapping
        /// </summary>
        public int MatchScore
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether this test-code-to-attribute mapping was chosen during the
        /// first pass of the mapping algorithm
        /// </summary>
        public bool FirstPassMapping
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether a fuzzy pattern was used to create this test-code-to-attribute mapping
        /// </summary>
        public bool FuzzyMatch
        {
            get;
            set;
        }

        #endregion Properties
    }
}
