using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents all valid exemptions categories and codes.
    /// </summary>
    public class MasterExemptionCodeList
    {
        #region MasterExemptionCodeList Fields

        /// <summary>
        /// Maps exemption code category names to <see cref="ExemptionCategory"/>.
        /// </summary>
        readonly Dictionary<string, ExemptionCategory> _nameToCategory = 
            new Dictionary<string, ExemptionCategory>();

        #endregion MasterExemptionCodeList Fields

        #region MasterExemptionCodeList Constructors

        /// <summary>
	    /// Initializes a new instance of the <see cref="MasterExemptionCodeList"/> class.
	    /// </summary>
        /// <param name="exemptionDirectory">A directory containing xml files that describe the 
        /// valid exemption code categories.</param>
	    public MasterExemptionCodeList(string exemptionDirectory)
	    {
            // Get all the xml files in the specified directory
            string[] xmlFiles = Directory.GetFiles(exemptionDirectory, "*.xml");

		    // Add the category for each xml file
            foreach (string xmlFile in xmlFiles)
            {
                string categoryName = Path.GetFileNameWithoutExtension(xmlFile);
                _nameToCategory[categoryName] = ExemptionCategory.FromXml(xmlFile);
		    }
	    }

        #endregion MasterExemptionCodeList Constructors

        #region MasterExemptionCodeList Properties

        /// <summary>
        /// Gets the full name of all the exemption code categories.
        /// </summary>
        /// <returns>The full name of all the exemption code categories.</returns>
        public IEnumerable<string> Categories
        {
            get 
            {
                return _nameToCategory.Keys;
            }
        }

        #endregion MasterExemptionCodeList Properties

        #region MasterExemptionCodeList Methods

        /// <summary>
        /// Gets all the exemption codes in the specified category.
        /// </summary>
        /// <param name="categoryName">The category from which to obtain exemption codes.</param>
        /// <returns>All the exemption codes in <paramref name="categoryName"/>.</returns>
        public IEnumerable<ExemptionCode> GetCodesInCategory(string categoryName)
        {
            try
            {
                ExemptionCategory category;
                if (_nameToCategory.TryGetValue(categoryName, out category))
                {
                    return category.Codes;
                }

                // No exemption codes in this category.
                return new ExemptionCode[0];
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26690",
                    "Unable to get exemption codes in category.", ex);
                ee.AddDebugData("Category", categoryName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the description of the specified exemption code.
        /// </summary>
        /// <param name="categoryName">The category of the exemption code.</param>
        /// <param name="codeName">The name of the exemption code.</param>
        /// <returns>The description of the <paramref name="codeName"/> in 
        /// <paramref name="categoryName"/>.</returns>
        // This is the name of an exemption code, not a codename.
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", 
            MessageId="codeName")]
        public string GetDescription(string categoryName, string codeName)
        {
            try
            {
                // If the category doesn't exist, throw an exception
                ExemptionCategory category;
                if (!_nameToCategory.TryGetValue(categoryName, out category))
                {
                    ExtractException ee = new ExtractException("ELI26683",
                        "Exemption category doesn't exist.");
                    ee.AddDebugData("Exemption category", categoryName, false);
                    throw ee;
                }

                // Iterate over all the codes in the specified category
                foreach (ExemptionCode code in category.Codes)
                {
                    // If we found the specified code, return its description
                    if (code.Name == codeName)
                    {
                        return code.Description;
                    }
                }

                // If we reached this point the code wasn't found, throw an exception
                ExtractException ex = new ExtractException("ELI26684",
                    "Cannot find exemption code description.");
                ex.AddDebugData("Exemption category", categoryName, false);
                ex.AddDebugData("Exemption code", codeName, false);
                throw ex;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26691", ex);
            }
        }

        /// <summary>
        /// Gets the abbreviated name of the specified category.
        /// </summary>
        /// <param name="categoryName">The category for which to get the abbreviated name.</param>
        /// <returns>The abbreviated name of <paramref name="categoryName"/> or 
        /// <see cref="String.Empty"/> if <paramref name="categoryName"/> is not found.</returns>
        public string GetCategoryAbbreviation(string categoryName)
        {
            try
            {
                // If the category doesn't exist, return the empty string
                ExemptionCategory category;
                if (_nameToCategory.TryGetValue(categoryName, out category))
                {
                    return category.Abbreviation;
                }

                return "";
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26689",
                    "Unable to get category abbreviation.", ex);
                ee.AddDebugData("Category name", categoryName, false);
                throw ee;
            }
        }

         /// <summary>
        /// Gets the abbreviated name of the specified abbreviated category.
        /// </summary>
        /// <param name="abbreviation">The abbreviation for which to get the full category name.
        /// </param>
        /// <returns>The full name of <paramref name="abbreviation"/> or 
        /// <see cref="String.Empty"/> if <paramref name="abbreviation"/> is not found.</returns>
        public string GetFullCategoryName(string abbreviation)
        {
            try
            {
                // Iterate over each category
                foreach (KeyValuePair<string, ExemptionCategory> nameToCategory in _nameToCategory)
                {
                    // If the abbreviated name matches, return the full name
                    if (nameToCategory.Value.Abbreviation == abbreviation)
                    {
                        return nameToCategory.Key;
                    }
                }

                // The abbreviated category wasn't found
                return "";
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26692",
                    "Unable to get full category name.", ex);
                ee.AddDebugData("Abbreviated name", abbreviation, false);
                throw ee;
            }
        }

        /// <summary>
        /// Determines whether the specified category and code combination is valid.
        /// </summary>
        /// <param name="abbreviation">The abbreviation of the category to check.</param>
        /// <param name="code">The code to check for containment.</param>
        /// <returns><see langword="true"/> if <paramref name="code"/> is in 
        /// <paramref name="abbreviation"/>; <see langword="false"/> if 
        /// <paramref name="abbreviation"/> does not correspond to a valid category or if 
        /// <paramref name="code"/> is not in <paramref name="abbreviation"/>.</returns>
        public bool HasCode(string abbreviation, string code)
        {
            try
            {
                // Iterate over each category
                foreach (KeyValuePair<string, ExemptionCategory> nameToCategory in _nameToCategory)
                {
                    // If the abbreviated name matches, return whether it contains the code
                    if (nameToCategory.Value.Abbreviation == abbreviation)
                    {
                        return nameToCategory.Value.HasCode(code);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26936",
                    "Unable to get exemption code from category.", ex);
                ee.AddDebugData("Code", code, false);
                ee.AddDebugData("Category", abbreviation, false);
                throw ee;
            }
        }

        #endregion MasterExemptionCodeList Methods
    }
}
