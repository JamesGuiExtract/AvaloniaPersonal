using System;
using System.Collections.Generic;
using System.Text;
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
        /// Initializes a new instance of the <see cref="LabTest"/> struct.
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
                _name = ss.String.ToUpperInvariant();
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
        /// Gets the test code for this <see cref="LabTest"/>.
        /// </summary>
        /// <returns>The test code for this <see cref="LabTest"/>.</returns>
        public string TestCode
        {
            get
            {
                return _testCode;
            }
            set
            {
                _testCode = value.ToUpperInvariant();
            }
        }

        #endregion Properties
    }
}
