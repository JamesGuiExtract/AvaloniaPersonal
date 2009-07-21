using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Text;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Represents a vector of attributes (VOA) file.
    /// </summary>
    [CLSCompliant(false)]
    public static class AttributesFile
    {
        #region AttributesFile Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(AttributesFile).ToString();

        #endregion AttributesFile Constants

        #region AttributesFile Methods

        /// <summary>
        /// Reads all the attributes from the specified voa file.
        /// </summary>
        /// <param name="fileName">The file name from which to read attributes.</param>
        /// <returns>An array of all the attributes in the specified voa file.</returns>
        public static ComAttribute[] ReadAll(string fileName)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleWritingCoreObjects,
                    "ELI26791", _OBJECT_NAME);

                // Load the attributes from the voa file
                IUnknownVector vector = new IUnknownVector();
                vector.LoadFrom(fileName, false);

                // Create an array of COM attributes
                ComAttribute[] attributes = new ComAttribute[vector.Size()];
                for (int i = 0; i < attributes.Length; i++)
                {
                    attributes[i] = (ComAttribute)vector.At(i);
                }

                return attributes;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26762",
                    "Unable to read voa file.", ex);
                ee.AddDebugData("Voa file", fileName, false);
                throw ee;
            }
        }

        #endregion AttributesFile Methods
    }
}
