using Extract.Interop;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="NumericSequencer"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("5643B027-B38A-4CC0-954E-EDA53FC34857")]
    [CLSCompliant(false)]
    public interface INumericSequencer : IAttributeModifyingRule, ICategorizedComponent, IConfigurableObject,
        ICopyableObject, ILicensedComponent, IPersistStream
    {
        /// <summary>
        /// Gets or sets a value indicating whether to expand hyphenated ranges, or contract
        /// individual numbers into hypenated ranges.
        /// </summary>
        /// <value><see langword="true"/> to expand all hyphenated ranges into the numbers within
        /// that range, <see langword="false"/> to consolidate any sequential series of individual
        /// numbers into a hyphenated range.
        /// </value>
        bool ExpandSequence
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to sort the output
        /// <para><b>Note</b></para>
        /// <see cref="Sort"/> cannot be <see langword="true"/> if <see cref="ExpandSequence"/> is
        /// <see langword="false"/>.
        /// </summary>
        /// <value><see langword="true"/> to sort the output; otherwise, <see langword="false"/>.
        /// </value>
        bool Sort
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to sort in ascending or descending order if
        /// <see cref="Sort"/> is <see langword="true"/>.
        /// </summary>
        /// <value><see langword="true"/> to sort in ascending order; <see langword="false"/> to
        /// sort in descending order.
        /// </value>
        bool AscendingSortOrder
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to eliminate duplicates in the output.
        /// </summary>
        /// <value><see langword="true"/> to eliminate duplicates in the output;
        /// <see langword="false"/> to preserve duplicates in the output.
        /// </value>
        bool EliminateDuplicates
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IAttributeModifyingRule"/> that can expand/contract strings that represent
    /// numeric values to/from strings that contract individual numbers into hyphenated ranges.
    /// </summary>
    [ComVisible(true)]
    [Guid("361CE46B-0B83-4C8A-A955-6005BACE9F0F")]
    [CLSCompliant(false)]
    public partial class NumericSequencer : INumericSequencer
    {
        #region Constants

        /// <summary>
        /// The description of the rule
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Numeric sequence expander/contractor";

        /// <summary>
        /// Current version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.RuleWritingCoreObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// <see langword="true"/> if changes have been made to <see cref="NumericSequencer"/>
        /// since it was created; <see langword="false"/> if no changes have been made since it was
        /// created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericSequencer"/> class.
        /// </summary>
        public NumericSequencer()
        {
            try
            {
                ExpandSequence = true;
                Sort = true;
                AscendingSortOrder = true;
                EliminateDuplicates = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33428");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericSequencer"/> class as a copy of the
        /// specified <see paramref="task"/>.
        /// </summary>
        /// <param name="numericSequencer">The <see cref="NumericSequencer"/> from which settings
        /// should be copied.</param>
        public NumericSequencer(NumericSequencer numericSequencer)
        {
            try
            {
                CopyFrom(numericSequencer);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33419");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether to expand hyphenated ranges, or contract
        /// individual numbers into hypenated ranges.
        /// </summary>
        /// <value><see langword="true"/> to expand all hyphenated ranges into the numbers within
        /// that range, <see langword="false"/> to consolidate any sequential series of individual
        /// numbers into a hyphenated range.
        /// </value>
        public bool ExpandSequence
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to sort the output
        /// <para><b>Note</b></para>
        /// <see cref="Sort"/> cannot be <see langword="true"/> if <see cref="ExpandSequence"/> is
        /// <see langword="false"/>.
        /// </summary>
        /// <value><see langword="true"/> to sort the output; otherwise, <see langword="false"/>.
        /// </value>
        public bool Sort
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to sort in ascending or descending order if
        /// <see cref="Sort"/> is <see langword="true"/>.
        /// </summary>
        /// <value><see langword="true"/> to sort in ascending order; <see langword="false"/> to
        /// sort in descending order.
        /// </value>
        public bool AscendingSortOrder
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to eliminate duplicates in the output.
        /// </summary>
        /// <value><see langword="true"/> to eliminate duplicates in the output;
        /// <see langword="false"/> to preserve duplicates in the output.
        /// </value>
        public bool EliminateDuplicates
        {
            get;
            set;
        }
        
        #endregion Properties

        #region IAttributeModifyingRule

        /// <summary>
        /// Modifies the attribute value.
        /// </summary>
        /// <param name="pAttributeToBeModified">The attribute to be modified.</param>
        /// <param name="pOriginInput">The original <see cref="AFDocument"/>.</param>
        /// <param name="pProgressStatus">A <see cref="ProgressStatus"/> instance that can be used
        /// to indicate progress.</param>
        public void ModifyValue(UCLID_AFCORELib.Attribute pAttributeToBeModified, AFDocument pOriginInput, ProgressStatus pProgressStatus)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33420", _COMPONENT_DESCRIPTION);

                SpatialString attributeValue = pAttributeToBeModified.Value;

                // Convert the attribute value into an enumeration of NumericRanges where commas
                // delimit the ranges.
                IEnumerable<NumericRange> ranges = attributeValue.String.Split(',')
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .Select(text => new NumericRange(text));

                // Expand or contract the ranges (taking the EliminateDuplicates setting into
                // account).
                ranges = ExpandSequence
                    ? Expand(ranges)
                    : Contract(ranges);

                // Sort the resulting ranges (if so configured).
                if (Sort)
                {
                    ranges = AscendingSortOrder
                        ? ranges.OrderBy(range => range)
                        : ranges.OrderByDescending(range => range);
                }

                // Convert the result into a comma delimited string and apply it to the attribute.
                string outputValue = string.Join(",", ranges.Select(range => range.ToString()));

                if (attributeValue.HasSpatialInfo())
                {
                    attributeValue.ReplaceAndDowngradeToHybrid(outputValue);
                }
                else
                {
                    attributeValue.ReplaceAndDowngradeToNonSpatial(outputValue);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33421", "Failed to expand/contract numeric sequence");
            }
        }

        #endregion IAttributeModifyingRule

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="NumericSequencer"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33422", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                NumericSequencer cloneOfThis = (NumericSequencer)Clone();

                using (NumericSequencerSettingsDialog dlg
                    = new NumericSequencerSettingsDialog(cloneOfThis))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dlg.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33423", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="NumericSequencer"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="NumericSequencer"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new NumericSequencer(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33424",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="NumericSequencer"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as NumericSequencer;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to NumericSequencer");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33425",
                    "Failed to copy '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        #endregion ICopyableObject Members

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(_LICENSE_ID);
        }

        #endregion ILicensedComponent Members

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns><see cref="HResult.Ok"/> if changes have been made;
        /// <see cref="HResult.False"/> if changes have not been made.
        /// </returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    ExpandSequence = reader.ReadBoolean();
                    Sort = reader.ReadBoolean();
                    AscendingSortOrder = reader.ReadBoolean();
                    EliminateDuplicates = reader.ReadBoolean();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33426",
                    "Failed to load '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Saves an object into the specified IStream and indicates whether the object should reset
        /// its dirty flag.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                ExtractException.Assert("ELI33433", "Cannot leave contracted list unsorted.",
                    ExpandSequence || Sort);

                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(ExpandSequence);
                    writer.Write(Sort);
                    writer.Write(AscendingSortOrder);
                    writer.Write(EliminateDuplicates);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33427",
                    "Failed to save '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            throw new NotImplementedException();
        }

        #endregion IPersistStream Members

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID AF-API Value Modifiers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.ValueModifiers);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID AF-API Value Modifiers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.ValueModifiers);
        }

        /// <summary>
        /// Copies the specified <see cref="NumericSequencer"/> instance into this one.
        /// </summary><param name="source">The <see cref="NumericSequencer"/> from which to copy.
        /// </param>
        void CopyFrom(NumericSequencer source)
        {
            ExpandSequence = source.ExpandSequence;
            Sort = source.Sort;
            AscendingSortOrder = source.AscendingSortOrder;
            EliminateDuplicates = source.EliminateDuplicates;            

            _dirty = true;
        }

        /// <summary>
        /// Expands the provided <see cref="NumericRange"/>s such that each instance contains only 1
        /// number (or text value).
        /// </summary>
        /// <param name="ranges">The <see cref="NumericRange"/>s to expand.</param>
        IEnumerable<NumericRange> Expand(IEnumerable<NumericRange> ranges)
        {
            IEnumerable<NumericRange> expandedNumbers = ranges
                .SelectMany(range => range.Expand());

            if (EliminateDuplicates)
            {
                expandedNumbers = expandedNumbers.Distinct();
            }

            return expandedNumbers;
        }

        /// <summary>
        /// Contracts the provided <see cref="NumericRange"/>s such that instances that can make
        /// an unbroken sequential range of number are combined into a single
        /// <see cref="NumericRange"/> instance.
        /// </summary>
        /// <param name="ranges">>The <see cref="NumericRange"/>s to contract.</param>
        IEnumerable<NumericRange> Contract(IEnumerable<NumericRange> ranges)
        {
            // Produce a list of only those ranges that are numeric.
            List<NumericRange> rangeList = new List<NumericRange>(ranges
                .Where(range => range.Numeric));

            // The contraction algorithm depends on the ranges being sorted to avoid O(n^2) scaling
            // with increasing number of NumericRange instances.
            rangeList.Sort();

            // Loop through each range instance.
            for (int i = 0; i < rangeList.Count - 1; i++)
            {
                NumericRange iRange = rangeList[i];
                IEnumerable<NumericRange> mergedRanges = null;

                // Loop through each subsequent range instance the "i" instance overlaps or runs up
                // against if not eliminating duplicates. (If not eliminating duplicates it's
                // possible one or more subsquent ranges overlap, but can't be merged in order to
                // preserve duplicates).
                for (int j = i + 1; j < rangeList.Count; j++)
                {
                    NumericRange jRange = rangeList[j];

                    // If the ranges do not overlap, we can move on to the next "i" instance.
                    if (iRange.EndNumber < jRange.StartNumber - 1)
                    {
                        break;
                    }

                    // Attempt to merge the ranges. null indicates the ranges cannot be merged
                    mergedRanges =
                        NumericRange.Merge(iRange, jRange, EliminateDuplicates);

                    // If the ranges can be merged, replace the merge result(s) in rangeList.
                    if (mergedRanges != null)
                    {
                        // Remove the original entries in the list.
                        rangeList.RemoveAll(range => range == iRange || range == jRange);

                        // Since NumericRange.Merge() depends on the ranges being sorted, insert the
                        // result(s) at the correct position(s).
                        foreach (NumericRange mergedRange in mergedRanges)
                        {
                            int insertionIndex = rangeList.BinarySearch(
                                i, rangeList.Count - i, mergedRange, mergedRange);
                            // If an equivalent is not found, the index will be the bitwise
                            // complement of the index where mergedRange should be inserted. 
                            insertionIndex = (insertionIndex > 0) ? insertionIndex : ~insertionIndex;
                            rangeList.Insert(insertionIndex, mergedRange);
                        }

                        // Once we've merged, the current "i" instance, restart the "i" loop in
                        // case the instance at i is now different than what was there before.
                        break;
                    }

                    // If eliminating duplicates, we only need to worry about merging the "i"
                    // instance with the next instance in the list since a merge can't fail on
                    // account of needing to preserve duplicates.
                    if (EliminateDuplicates)
                    {
                        break;
                    }
                }

                // Restart the "i" loop in case the instance at i is now different than what was
                // there before.
                if (mergedRanges != null)
                {
                    i--;
                }
            }

            // Add the non-numeric instance back into rangeList.
            if (EliminateDuplicates)
            {
                rangeList.AddRange(ranges
                    .Where(range => !range.Numeric)
                    .Distinct());
            }
            else
            {
                rangeList.AddRange(ranges
                    .Where(range => !range.Numeric));
            }

            return rangeList;
        }

        #endregion Private Members
    }
}
