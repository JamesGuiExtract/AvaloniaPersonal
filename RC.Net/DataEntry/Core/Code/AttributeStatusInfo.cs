using Extract;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Specifies whether an <see cref="IAttribute"/> is a hint and, if so, what type of
    /// hint that it is.
    /// </summary>
    public enum HintType
    {
        /// <summary>
        /// The <see cref="IAttribute"/> is truely spatial (not a hint).
        /// </summary>
        None,

        /// <summary>
        /// The <see cref="IAttribute"/> is a direct hint (specifies the region one would expect to
        /// find the data related to this hint.
        /// </summary>
        Direct,

        /// <summary>
        /// The <see cref="IAttribute"/> is an indirect hint (specifies spatial clues that will help
        /// determine where the <see cref="IAttribute"/>'s data may be found, but not necessarily
        /// where one would expect to actually find the data).
        /// </summary>
        Indirect
    }

    /// <summary>
    /// Specifies under what circumstances an <see cref="IAttribute"/> should be included in the tab
    /// order.
    /// </summary>
    public enum TabStopMode
    {
        /// <summary>
        /// The <see cref="IAttribute"/> should always be in the tab order (assuming it is viewable).
        /// </summary>
        Always,

        /// <summary>
        /// The <see cref="IAttribute"/> should only be included in the tab order if it is populated
        /// with a non-empty value or its data is marked as invalid.
        /// </summary>
        OnlyWhenPopulatedOrInvalid,

        /// <summary>
        /// The <see cref="IAttribute"/> should only be included in the tab order if its data is
        /// marked as invalid (regardless of whether the value is non-empty).
        /// </summary>
        OnlyWhenInvalid,

        /// <summary>
        /// The <see cref="IAttribute"/> should never be included in the tab order.
        /// </summary>
        Never
    }

    /// <summary>
    /// An object that represents the current state of a particular <see cref="IAttribute"/>
    /// This includes whether the attribute's data has been validated, fully propagated and whether it
    /// has been viewed by the user. The object is intended to occupy the 
    /// <see cref="IAttribute.DataObject"/> field of the <see cref="IAttribute"/> it is associated
    /// with.
    /// </summary>
    [Guid("BC86C004-F2C7-4a90-80F7-F6C49B201AD4")]
    [ProgId("Extract.DataEntry.AttributeStatusInfo")]
    [ComVisible(true)]
    public class AttributeStatusInfo : IPersistStream, ICopyableObject
    {
        #region Delegates

        /// <summary>
        /// Used as a parameter for an attribute scan to define what should be done for each 
        /// attribute in the tree.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> in question.</param>
        /// <param name="statusInfo">The <see cref="AttributeStatusInfo"/> instance associated with 
        /// the <see cref="IAttribute"/> in question.</param>
        /// <param name="value">A value to either be set or confirmed depending upon the purpose of
        /// each specific AccessorMethod implementation.</param>
        /// <returns><see langword="true"/> to continue traversing the attribute tree, 
        /// <see langword="false"/> to return <see langword="false"/> from an attribute scan 
        /// without traversing any more attributes.</returns>
        delegate bool AccessorMethod(IAttribute attribute, AttributeStatusInfo statusInfo, 
            bool value);

        #endregion Delegates

        #region AttributeScanner

        /// <summary>
        /// A helper class for <see cref="AttributeStatusInfo"/> that allows an 
        /// <see cref="IAttribute"/> tree to be scanned to either apply a status info change or to
        /// look for specific status values.  The attribute tree will be scanned in logical viewing
        /// order (either forward or backward).
        /// <para>Requirements:</para>
        /// The scanning will work only as long as the following assumptions are true:
        /// <list type="bullet">
        /// <bullet>All attributes mapped to owning controls have had their
        /// <see cref="AttributeStatusInfo.DisplayOrder"/> value set according to the order in
        /// which the corresponding controls appear in the form.</bullet>
        /// <bullet>All sibling attributes have been sorted according to their DisplayOrder and
        /// all sibilings with equal DisplayOrder values are ordered in the order they appear in the
        /// form.</bullet>
        /// <bullet>No more that two generations of attributes share the same DisplayOrder. (This
        /// is currently assured by the fact that no <see cref="IDataEntryControl"/> implementations
        /// control more that two generations of attributes.</bullet>
        /// </list>
        /// </summary>
        class AttributeScanner
        {
            #region Fields

            /// <summary>
            /// The set of attributes to be scanned.
            /// </summary>
            public IUnknownVector _attributes;

            /// <summary>
            /// A genealogy of attributes describing the point at which the scan should be 
            /// started.  The scan will start with the first attribute after the target attribute 
            /// (the target being the attribute at the bottom of the stack) and will end with
            /// the target attribute (assuming the scan isn't aborted somewhere in between).
            /// </summary>
            public Stack<IAttribute> _startingPoint;

            /// <summary>
            /// An attribute genealogy specifying the attribute that caused a scan to abort based
            /// on the <see cref="AttributeStatusInfo.AccessorMethod"/> returning
            /// <see langword="false"/>.
            /// </summary>
            public Stack<IAttribute> _resultAttributeGenealogy;

            /// <summary>
            /// <see langword="true"/> if scanning from the specified starting point to the end of  
            /// the vector of attributes, <see langword="false"/> if looping back from the beginning of
            /// the attribute vector to the specified starting point.
            /// </summary>
            public bool _firstPass = true;

            /// <summary>
            /// <see langword="true"/> if scanning forward through the attribute hierarchy,
            /// <see langword="false"/> if scanning backward.
            /// </summary>
            public bool _forward = true;

            /// <summary>
            /// The index of the attribute where the scan should start for the current pass.
            /// </summary>
            public int _startIndex = -1;

            /// <summary>
            /// The index of the attribute where the scan should end for the current pass.
            /// </summary>
            public int _endIndex = -1;

            /// <summary>
            /// The index of the attribute described by the _startingPoint genealogy. This is only
            /// set if the specified attribute at the bottom of the _startingPoint stack is one of
            /// the attributes to search in this node (it is not set if the starting attribute is 
            /// a descendent to one of the attributes to scan).
            /// </summary>
            public int _startAttributeIndex = -1;

            /// <summary>
            /// The <see cref="AttributeStatusInfo.DisplayOrder"/> of the attribute that triggered
            /// the scan to end.
            /// </summary>
            public string _resultDisplayOrder;

            /// <summary>
            /// <see langword="true"/> if all attributes were scanned without any triggered the
            /// scan to be aborted (with a return value of <see langword="false"/> from the 
            /// <see cref="AttributeStatusInfo.AccessorMethod"/>), <see langword="false"/> if the
            /// scan was aborted.
            /// </summary>
            public bool _result = true;

            /// <summary>
            /// A method used to get or set an <see cref="AttributeStatusInfo"/> field for each 
            /// attribute.
            /// </summary>
            public AccessorMethod _accessorMethod;

            /// <summary>
            /// Either specifies the value an <see cref="AttributeStatusInfo"/> field should be set
            /// to or specifies the value a field is required to be.
            /// </summary>
            public bool _value;

            #endregion Fields

            #region Constructors

            /// <summary>
            /// Initializes a new <see cref="AttributeScanner"/> instance used to scan a generation
            /// of <see cref="IAttribute"/>s.
            /// <para><b>Note:</b></para>
            /// This constructor is private. Instances are constructed only via the Scan method.
            /// </summary>
            /// <param name="attributes">An <see cref="IUnknownVector"/> of 
            /// <see cref="IAttribute"/>s to scan.</param>
            /// <param name="accessorMethod">A method used to get or set an 
            /// <see cref="AttributeStatusInfo"/> field for each attribute.</param>
            /// <param name="value">Either specifies the value an <see cref="AttributeStatusInfo"/>
            /// field should be set to or specifies the value a field is  required to be.</param>
            /// <param name="resultAttributeGenealogy">An attribute genealogy specifying the 
            /// attribute that caused a scan to abort based on the 
            /// <see cref="AttributeStatusInfo.AccessorMethod"/> returning <see langword="false"/>.
            /// </param>
            /// <param name="forward"><see langword="true"/> if scanning forward through the 
            /// attribute hierarchy, <see langword="false"/> if scanning backward.</param>
            AttributeScanner(IUnknownVector attributes, AccessorMethod accessorMethod,
                bool value, bool forward, Stack<IAttribute> resultAttributeGenealogy)
            {
                try
                {
                    _attributes = attributes;
                    _accessorMethod = accessorMethod;
                    _value = value;
                    _forward = forward;
                    _resultAttributeGenealogy = resultAttributeGenealogy;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24854", ex);
                }
            }

            #endregion Constructors

            #region Methods

            /// <summary>
            /// Performs a scan of the specified <see cref="IAttribute"/>s.
            /// </summary>
            /// <param name="attributes">The <see cref="IUnknownVector"/> of 
            /// <see cref="IAttribute"/>s to scan.</param>
            /// <param name="startingPoint">A genealogy of <see cref="IAttribute"/>s describing 
            /// the point at which the scan should be started with each attribute further down the
            /// the stack being a descendent to the previous <see cref="IAttribute"/> in the stack.
            /// The scan will start with the first attribute after the target attribute (the target 
            /// being the attribute at the bottom of the stack) and will end with the target attribute 
            /// (assuming the scan isn't aborted somewhere in between).</param>
            /// <param name="accessorMethod">A method used to get or set an 
            /// <see cref="AttributeStatusInfo"/> field for each attribute.</param>
            /// <param name="value">Either specifies the value an <see cref="AttributeStatusInfo"/>
            /// field should be set to or specifies the value a field is  required to be.</param>
            /// <param name="forward"><see langword="true"/> if scanning forward through the 
            /// attribute hierarchy, <see langword="false"/> if scanning backward.</param>
            /// <param name="loop"><see langword="true"/> to resume scanning from the beginning of
            /// the <see cref="IAttribute"/>s (back to the starting point) if the end was reached 
            /// successfully, <see langword="false"/> to end the scan once the end of the 
            /// <see cref="IAttribute"/> vector is reached.</param>
            /// <param name="resultAttributeGenealogy">A genealogy of <see cref="IAttribute"/>s 
            /// specifying the attribute that caused a scan to abort based on the 
            /// <see cref="AttributeStatusInfo.AccessorMethod"/> returning <see langword="false"/>
            /// </param>
            public static bool Scan(IUnknownVector attributes,
                Stack<IAttribute> startingPoint, AccessorMethod accessorMethod, bool value,
                bool forward, bool loop, Stack<IAttribute> resultAttributeGenealogy)
            {
                try
                {
                    // Validate the license
                    _licenseCache.Validate("ELI26132");

                    // Create a node in charge of scanning the root-level attributes.
                    AttributeScanner rootScanNode = new AttributeScanner(attributes, accessorMethod,
                        value, forward, resultAttributeGenealogy);

                    // Initialize the scan node for the first pass of the scan (from the starting
                    // point to the end of the attribute vector.
                    rootScanNode = rootScanNode.GetScanNode(attributes, startingPoint, true);
                    rootScanNode.Scan(null);

                    if (loop && rootScanNode._result)
                    {
                        // If looping is requested and the first pass of the scan completed (was not
                        // aborted), initialize the scan node for the second pass of the scan (from
                        // the beginning of the attribute vector back to the starting point)
                        rootScanNode = rootScanNode.GetScanNode(attributes, startingPoint, false);
                        rootScanNode.Scan(null);
                    }

                    return rootScanNode._result;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24855", ex);
                }
            }

            #endregion Methods

            #region Private Members

            /// <summary>
            /// Retrieves a child <see cref="AttributeScanner"/> instance responsible for scanning
            /// the specified attributes (which are descendents to an attribute in the calling 
            /// instance).
            /// </summary>
            /// <param name="attributes">The <see cref="IUnknownVector"/> of 
            /// <see cref="IAttribute"/>s to scan.</param>
            /// <param name="startingPoint">A genealogy of <see cref="IAttribute"/>s describing 
            /// the point at which the scan should be started with each attribute further down the
            /// the stack being a descendent to the previous <see cref="IAttribute"/> in the stack.
            /// </param>
            /// <param name="firstPass"><see langword="true"/> if scanning from the specified 
            /// starting point to the end of the vector of attributes, <see langword="false"/> if 
            /// looping back from the beginning of the attribute vector to the specified starting 
            /// point.</param>
            /// <returns>A <see cref="AttributeScanner"/> instance.</returns>
            AttributeScanner GetScanNode(IUnknownVector attributes, 
                Stack<IAttribute> startingPoint, bool firstPass)
            {
                // Create and initialize the child AttributeScanner instance.
                AttributeScanner childStatusinfo = new AttributeScanner(attributes, _accessorMethod,
                    _value, _forward, _resultAttributeGenealogy);
                childStatusinfo._attributes = attributes;
                childStatusinfo._firstPass = firstPass;

                // Determine the initial starting and ending attribute indexes based on the scan
                // direction.
                childStatusinfo._startIndex = (_forward ? 0 : attributes.Size() - 1);
                childStatusinfo._endIndex = (_forward ? attributes.Size() : -1);

                // Adjust the start or end index to reflect the starting point (if specified).
                if (startingPoint != null && startingPoint.Count > 0)
                {
                    // Create a copy of the starting point attribute stack.
                    childStatusinfo._startingPoint =
                        CollectionMethods.CopyStack<IAttribute>(startingPoint);
     
                    // Since this instance is to scan the next generation, remove the first
                    // generation from the stack.
                    IAttribute startingAttribute = childStatusinfo._startingPoint.Pop();

                    // Locate the starting point (if specified)
                    int index = -1;
                    if (attributes.Size() > 0)
                    {
                        attributes.FindByReference(startingAttribute, 0, ref index);
                    }

                    // If no starting point was found, or the starting point stack is now empty, 
                    // clear the startingPoint.
                    if (index == -1)
                    {
                        childStatusinfo._startingPoint = null;
                    }
                    // If a starting point was found and this is the first pass, adjust the
                    // starting index appropriately.
                    else if (firstPass)
                    {
                        childStatusinfo._startIndex = index;

                        if (childStatusinfo._startingPoint.Count == 0)
                        {
                            childStatusinfo._startAttributeIndex = childStatusinfo._startIndex;
                            childStatusinfo._startingPoint = null;
                        }
                    }
                    // If a starting point was found and this is the second pass, adjust the
                    // ending index appropriately.
                    else
                    {
                        childStatusinfo._endIndex = (_forward ? index + 1 : index - 1);
                    }
                }

                return childStatusinfo;
            }

            /// <summary>
            /// Performs a scan of the current <see cref="AttributeScanner"/> node.
            /// </summary>
            /// <param name="cutoffDisplayOrder">The <see cref="AttributeStatusInfo.DisplayOrder"/>
            /// value to use as a cutoff value to prevent attributes before or after the starting 
            /// point from processing (depending upon whether this is the first or second pass).
            /// </param>
            /// <returns>Any <see cref="AttributeStatusInfo.DisplayOrder"/> that was applied during
            /// processing as a result of encountering the starting point.</returns>
            string Scan(string cutoffDisplayOrder)
            {
                // Initialized the applied cutoff value to null.
                string appliedCutoffDisplayOrder = null;

                // Loop through the provided attributes.
                for (int i = _startIndex;
                    (_forward && i < _endIndex) || (!_forward && i > _endIndex);
                    i += (_forward ? 1 : -1))
                {
                    // Obtain the status info for each attribute.
                    IAttribute attribute = (IAttribute)_attributes.At(i);
                    AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(attribute);

                    // Initialize the current cutoff to use to null.
                    string currentCutoffDisplayOrder = null;

                    // If this is the starting attribute apply the attribute's display order as the
                    // cutoff value.
                    if (i == _startAttributeIndex)
                    {
                        currentCutoffDisplayOrder = statusInfo.DisplayOrder;
                        appliedCutoffDisplayOrder = currentCutoffDisplayOrder;
                    }
                    // If the starting attribute is not a decendent of this specific attribute, ignore
                    // the specified starting point.
                    else if (i != _startIndex)
                    {
                        _startingPoint = null;
                    }

                    AttributeScanner childScanNode = null;

                    // If this attribute has sub-attributes, perform a scan of the sub-attributes
                    if (attribute.SubAttributes.Size() > 0)
                    {
                        childScanNode = GetScanNode(attribute.SubAttributes, _startingPoint, _firstPass);
                        currentCutoffDisplayOrder = childScanNode.Scan(currentCutoffDisplayOrder);

                        // If a cutoff was applied, use it.
                        if (currentCutoffDisplayOrder != null)
                        {
                            appliedCutoffDisplayOrder = currentCutoffDisplayOrder;
                        }
                    }

                    // If a cutoff hasn't been applied, use the cutoff value provided by the caller.
                    if (string.IsNullOrEmpty(currentCutoffDisplayOrder))
                    {
                        currentCutoffDisplayOrder = cutoffDisplayOrder;
                    }

                    // Call the accessor method unless the attribute should be skipped per the
                    // order cutoff.
                    if (!this.SkipAttribute(i, statusInfo.DisplayOrder, currentCutoffDisplayOrder) &&
                        !_accessorMethod(attribute, statusInfo, _value))
                    {
                        // If accessMethod returned false, check to see if this an existing result
                        // from a sub-attribute should be used in place of this result.
                        if (childScanNode == null || childScanNode._result ||
                            IsBefore(statusInfo.DisplayOrder, childScanNode._resultDisplayOrder))
                        {
                            // Apply this result.
                            _result = false;
                            _resultDisplayOrder = statusInfo.DisplayOrder;

                            // Clear any current result genealogy.
                            if (_resultAttributeGenealogy.Count > 0)
                            {
                                _resultAttributeGenealogy.Clear();
                            }
                        }
                    }

                    // If the current accessor method was skipped or succeeded, but a sub attribute
                    // failed the accessor method, use the child's result.
                    if (_result && childScanNode != null && !childScanNode._result)
                    {
                        _result = false;
                        _resultDisplayOrder = childScanNode._resultDisplayOrder;
                    }

                    // In the case of a negative result, apply the current attribute to the result's
                    // geneology and stop scanning.
                    if (!_result)
                    {
                        _resultAttributeGenealogy.Push(attribute);
                        break;
                    }
                }

                return appliedCutoffDisplayOrder;
            }

            /// <summary>
            /// Determines whether the <see cref="AccessorMethod"/> for the <see cref="IAttribute"/>
            /// at the specified attribute should be skipped based on the cutoff value.
            /// </summary>
            /// <param name="index">The index of the <see cref="IAttribute"/> to check.</param>
            /// <param name="displayOrder">The display order of teh <see cref="IAttribute"/>.</param>
            /// <param name="cutoffDisplayOrder">The cutoff display order to compare against.
            /// </param>
            /// <returns><see langword="true"/> if the <see cref="IAttribute"/> should be skipped,
            /// <see langword="false"/> if the accessory method should be called.</returns>
            bool SkipAttribute(int index, string displayOrder, string cutoffDisplayOrder)
            {
                // If this is the attribute specified by the starting point, skip it.
                if (index == _startAttributeIndex)
                {
                    return true;
                }

                // Check to see if a cutoff display order has been specified.
                if (!string.IsNullOrEmpty(cutoffDisplayOrder))
                {
                    // If the display order value comes before the cutoff on the first pass, skip it.
                    if (_firstPass && IsBefore(displayOrder, cutoffDisplayOrder))
                    {
                        return true;
                    }

                    // If the display order value does not come before the cutoff on the second pass, 
                    // skip it.
                    if (!_firstPass && !IsBefore(displayOrder, cutoffDisplayOrder))
                    {
                        return true;
                    }
                }

                // Don't skip; call the accessor method.
                return false;
            }

            /// <summary>
            /// Checks to see whether one display order is before another display order in the scan
            /// order.
            /// </summary>
            /// <param name="displayOrder1">The first display order</param>
            /// <param name="displayOrder2">The display order to compare against.</param>
            /// <returns>Returns <see langword="true"/> if displayOrder1 comes before displayOrder2,
            /// <see langword="false"/> if they are equal or displayOrder2 comes first.</returns>
            bool IsBefore(string displayOrder1, string displayOrder2)
            {
                if (_forward)
                {
                    // When scanning forward, check to see displayOrder1 < displayOrder2
                    return string.Compare(displayOrder1, displayOrder2,
                        StringComparison.CurrentCultureIgnoreCase) < 0;
                }
                else
                {
                    // When scanning backward, check to see displayOrder2 < displayOrder1
                    return string.Compare(displayOrder2, displayOrder1,
                        StringComparison.CurrentCultureIgnoreCase) < 0;
                }
            }

            #endregion Private Members
        }

        #endregion AttributeScanner

        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(AttributeStatusInfo).ToString();

        /// <summary>
        /// The current version of this object.
        /// <para><b>Versions 2:</b></para>
        /// Added persistence of _isAccepted.
        /// <para><b>Versions 3:</b></para>
        /// Added persistence of _hintEnabled.
        /// </summary>
        const int _CURRENT_VERSION = 3;
        
        #endregion Constants

        #region Fields

        /// <summary>
        /// The filename of the currently open document.
        /// </summary>
        static string _sourceDocName;

        /// <summary>
        /// The active attribute hierarchy.
        /// </summary>
        static IUnknownVector _attributes;

        /// <summary>
        /// A database available for use in validation or auto-update queries.
        /// </summary>
        static DbConnection _dbConnection;

        /// <summary>
        /// Caches the info object for each <see cref="IAttribute"/> for quick reference later on.
        /// </summary>
        static Dictionary<IAttribute, AttributeStatusInfo> _statusInfoMap =
            new Dictionary<IAttribute, AttributeStatusInfo>();

        /// <summary>
        /// A dictionary that keeps track of which attribute collection each attribute belongs to.
        /// Used to help in assigning _parentAttribute fields.
        /// </summary>
        static Dictionary<IUnknownVector, IAttribute> _subAttributesToParentMap =
            new Dictionary<IUnknownVector, IAttribute>();

        /// <summary>
        /// A dictionary of auto-update triggers that exist on the attributes stored in the keys of
        /// this dictionary.
        /// </summary>
        static Dictionary<IAttribute, AutoUpdateTrigger> _autoUpdateTriggers =
            new Dictionary<IAttribute, AutoUpdateTrigger>();

        /// <summary>
        /// A dictionary of validation triggers that exist on the attributes stored in the keys of
        /// this dictionary.
        /// </summary>
        static Dictionary<IAttribute, AutoUpdateTrigger> _validationTriggers =
            new Dictionary<IAttribute, AutoUpdateTrigger>();

        /// <summary>
        /// Keeps track of the attributes that have been modified since the last time EndEdit was
        /// called. Each modified attribute is assiged a KeyValuePair which keeps track of whether
        /// the spatial information has changed and what the original attribute value was in case
        /// it needs to be reverted.
        /// </summary>
        static Dictionary<IAttribute, KeyValuePair<bool, SpatialString>> _attributesBeingModified =
            new Dictionary<IAttribute, KeyValuePair<bool, SpatialString>>();

        /// <summary>
        /// Keeps track of whether EndEdit is currently being processed.
        /// </summary>
        static bool _endEditInProgress;

        /// <summary>
        /// Indicates whether the object has been modified since being loaded via the 
        /// IPersistStream interface. This is an int because that is the return type of 
        /// IPersistStream::IsDirty in order to support COM values of <see cref="HResult.Ok"/> and 
        /// <see cref="HResult.False"/>.
        /// </summary>
        int _dirty;

        /// <summary>
        /// The control in charge of displaying the attribute.
        /// </summary>
        IDataEntryControl _owningControl;

        /// <summary>
        /// The validator used to validate the attribute's data.
        /// </summary>
        IDataEntryValidator _validator;

        /// <summary>
        /// Indicates whether the user has viewed the attribute's data.
        /// </summary>
        bool _hasBeenViewed;

        /// <summary>
        /// Whether this attribute has been propagated (ie, its children have been mapped to
        /// any dependent child controls.
        /// </summary>
        bool _hasBeenPropagated;

        /// <summary>
        /// <see langword="true"/> if the attribute's data is viewable in the DEP, 
        /// <see langword="false"/> otherwise.
        /// </summary>
        bool _isViewable;

        /// <summary>
        /// <see langword="true"/> if the attribute's data is currently know to be valid.
        /// <see langword="false"/> otherwise.
        /// </summary>
        bool _dataIsValid = true;

        /// <summary>
        /// <see langword="true"/> if data validation should be performed on the attribute,
        /// <see langword="false"/> if the attribute should always be considered valid.
        /// </summary>
        bool _validationEnabled = true;

        /// <summary>
        /// A string that allows attributes to be sorted by compared to other display order values.
        /// </summary>
        string _displayOrder;

        /// <summary>
        /// Specifies whether the <see cref="IAttribute"/> is a hint and, if so, what type
        /// of hint that it is.
        /// </summary>
        HintType _hintType;

        /// <summary>
        /// Specifies the spatial area that defines a hint.
        /// </summary>
        IEnumerable<Extract.Imaging.RasterZone> _hintRasterZones;

        /// <summary>
        /// Specifies whether the user has accepted the highlight associated with the attribute.
        /// </summary>
        bool _isAccepted;

        /// <summary>
        /// Specifies whether hints are enabled for the attribute. The fact that hints are enabled
        /// doesn't necessarily mean the attribute has one.
        /// </summary>
        bool _hintEnabled = true;

        /// <summary>
        /// Specifies the parent of this attribute (if one exists).
        /// </summary>
        IAttribute _parentAttribute;

        /// <summary>
        /// A query which will cause the attribute's value to automatically be updated using values
        /// from other attributes and/or a database query.
        /// </summary>
        string _autoUpdateQuery;

        /// <summary>
        /// A query which will cause the validation list to be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        string _validationQuery;

        /// <summary>
        /// Keeps track of if an AttributeValueModified value event is currently being raised to
        /// prevent recursion via autoUpdateQueries.
        /// </summary>
        bool _raisingAttributeValueModified;

        /// <summary>
        /// Specifies the full path (from the attribute hierarchy root).  Used to assist registering
        /// AutoUpdateTrigger attributes more efficiently.
        /// </summary>
        string _fullPath;
        
        /// <summary>
        /// Specifies under what circumstances the attribute should serve as a tab stop.
        /// </summary>
        TabStopMode _tabStopMode = TabStopMode.Always;

        /// <summary>
        /// Specifies whether the attribute should be persisted in output.
        /// </summary>
        bool _persistAttribute = true;

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.DataEntryCoreComponents, _OBJECT_NAME);

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="AttributeStatusInfo"/> instance.  This contructor should never
        /// be called directly by an outside class except via the <see cref="IPersistStream"/>
        /// COM interface.
        /// </summary>
        public AttributeStatusInfo()
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                _licenseCache.Validate("ELI24485");
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24493", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the <see cref="IDataEntryControl"/> in charge of displaying the associated
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <value>The <see cref="IDataEntryControl"/> in charge of displaying the associated
        /// <see cref="IAttribute"/>.</value>
        /// <returns>The <see cref="IDataEntryControl"/> in charge of displaying the associated
        /// <see cref="IAttribute"/>.</returns>
        public IDataEntryControl OwningControl
        {
            get
            {
                return _owningControl;
            }

            set
            {
                _owningControl = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IDataEntryValidator"/> used to validate the associated 
        /// <see cref="IAttribute"/>'s data.
        /// </summary>
        /// <value>The <see cref="IDataEntryValidator"/> used to validate the associated 
        /// <see cref="IAttribute"/>'s data.</value>
        /// <returns>The <see cref="IDataEntryValidator"/> used to validate the associated 
        /// <see cref="IAttribute"/>'s data.</returns>
        public IDataEntryValidator Validator
        {
            get
            {
                return _validator;
            }

            set
            {
                _validator = value;
            }
        }

        /// <summary>
        /// Specifies under what circumstances the <see cref="IAttribute"/> should serve as a tab
        /// stop.
        /// </summary>
        /// <value>A <see cref="TabStopMode"/> value indicating when the attribute should serve as a
        /// tab stop.</value>
        /// <returns>A <see cref="TabStopMode"/> value indicating when the attribute will serve as a
        /// tab stop.</returns>
        public TabStopMode TabStopMode
        {
            get
            {
                return _tabStopMode;
            }

            set
            {
                _tabStopMode = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the attribute should be persisted in output.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="IAttribute"/> should be persisted in
        /// output; <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if the <see cref="IAttribute"/> will be persisted in
        /// output; <see langword="false"/> otherwise.</returns>
        public bool PersistAttribute
        {
            get
            {
                return _persistAttribute;
            }

            set
            {
                _persistAttribute = value;
            }
        }

        /// <summary>
        /// Gets or sets a <see langword="string"/> that allows <see cref="IAttribute"/>s to be
        /// sorted by compared to other <see cref="IAttribute"/>'s <see cref="DisplayOrder"/>values.
        /// <see cref="IAttribute"/>s will be sorted from lowest to highest using the result of
        /// <see cref="String.Compare(string, string, StringComparison)"/>.
        /// </summary>
        /// <value>A <see langword="string"/> that allows <see cref="IAttribute"/>s to be
        /// sorted by compared to other <see cref="IAttribute"/>'s <see cref="DisplayOrder"/>values.
        /// </value>
        /// <returns>A <see langword="string"/> that allows <see cref="IAttribute"/>s to be
        /// sorted by compared to other <see cref="IAttribute"/>'s <see cref="DisplayOrder"/>values.
        /// </returns>
        public string DisplayOrder
        {
            get
            {
                return _displayOrder;
            }

            set
            {
                _displayOrder = value;
            }
        }

        /// <summary>
        /// A query which will cause the <see cref="IAttribute"/>'s value to automatically be
        /// updated using values from other <see cref="IAttribute"/>s and/or a database query.
        /// </summary>
        public string AutoUpdateQuery
        {
            get
            {
                return _autoUpdateQuery;
            }

            set
            {
                _autoUpdateQuery = value;
            }
        }

        /// <summary>
        /// A query which will cause the <see cref="IAttribute"/>'s validation list to be
        /// automatically updated using values from other <see cref="IAttribute"/>s and/or a
        /// database query.
        /// </summary>
        public string ValidationQuery
        {
            get
            {
                return _validationQuery;
            }

            set
            {
                _validationQuery = value;
            }
        }

        /// <summary>
        /// Specifies the full path (from the attribute hierarchy root). Used to assist registering
        /// <see cref="AutoUpdateTrigger"/> attributes more efficiently.
        /// </summary>
        /// <returns></returns>
        public string FullPath
        {
            get
            {
                return _fullPath;
            }

            set
            {
                _fullPath = value;
            }
        }

        #endregion Properties

        #region Static Members

        /// <summary>
        /// Gets the filename of the currently open document.
        /// </summary>
        /// <returns>The filename of the currently open document.</returns>
        [ComVisible(false)]
        public static string SourceDocName
        {
            get
            {
                return _sourceDocName;
            }
        }

        /// <summary>
        /// Returns the <see cref="AttributeStatusInfo"/> object associated with the provided
        /// <see cref="IAttribute"/>.  A new <see cref="AttributeStatusInfo"/> instance is
        /// created if necessary.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which an 
        /// <see cref="AttributeStatusInfo"/> instance is needed.</param>
        /// <returns>The <see cref="AttributeStatusInfo"/> instance associated with the provided
        /// <see cref="IAttribute"/>.</returns>
        [ComVisible(false)]
        public static AttributeStatusInfo GetStatusInfo(IAttribute attribute)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI26109");

                AttributeStatusInfo statusInfo;
                if (!_statusInfoMap.TryGetValue(attribute, out statusInfo))
                {
                    statusInfo = attribute.DataObject as AttributeStatusInfo;
                    
                    if (statusInfo == null)
                    {
                        statusInfo = new AttributeStatusInfo();
                        attribute.DataObject = statusInfo;
                    }

                    _statusInfoMap[attribute] = statusInfo;
                }

                return statusInfo;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24477", ex);
            }
        }

        /// <summary>
        /// Clears the internal cache used for efficient lookups of <see cref="AttributeStatusInfo"/>
        /// objects. This should be every time new data is loaded and called with
        /// <see langword="null"/> every time a document is closed (or <see cref="IAttribute"/>s are
        /// otherwise unloaded).
        /// </summary>
        /// <param name="sourceDocName">The name of the currently open document.</param>
        /// <param name="attributes">The active <see cref="IAttribute"/> hierarchy.</param>
        /// <param name="dbConnection">A compact SQL database available for use in validation or
        /// auto-update queries. (Can be <see langword="null"/> if not required).</param>
        [ComVisible(false)]
        public static void ResetData(string sourceDocName, IUnknownVector attributes,
            DbConnection dbConnection)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI26133");

                _sourceDocName = sourceDocName;
                _attributes = attributes;
                _dbConnection = dbConnection;
                _statusInfoMap.Clear();
                _subAttributesToParentMap.Clear();
                _attributesBeingModified.Clear();
                _endEditInProgress = false;

                foreach (AutoUpdateTrigger autoUpdateTrigger in _autoUpdateTriggers.Values)
                {
                    autoUpdateTrigger.Dispose();
                }
                _autoUpdateTriggers.Clear();

                foreach (AutoUpdateTrigger validationTrigger in _validationTriggers.Values)
                {
                    validationTrigger.Dispose();
                }
                _validationTriggers.Clear();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25624", ex);
            }
        }

        /// <summary>
        /// Initializes a <see cref="AttributeStatusInfo"/> instance for the specified
        /// <see cref="IAttribute"/> with the specified parameters.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which an 
        /// <see cref="AttributeStatusInfo"/> instance is being initialized.</param>
        /// <param name="sourceAttributes">The vector of <see cref="IAttribute"/>s to which the
        /// specified <see cref="IAttribute"/> is a member.</param>
        /// <param name="owningControl">The <see cref="IDataEntryControl"/> in charge of displaying 
        /// the specified <see cref="IAttribute"/>.</param>
        /// <param name="displayOrder">A <see langword="string"/> that allows the 
        /// <see cref="IAttribute"/> to be sorted by compared to other <see cref="IAttribute"/>'s 
        /// <see cref="DisplayOrder"/>values. Specify <see langword="null"/> to allow the 
        /// <see cref="IAttribute"/> to keep any display order it already has.</param>
        /// <param name="considerPropagated"><see langword="true"/> to consider the 
        /// <see cref="IAttribute"/> already propagated; <see langword="false"/> otherwise.</param>
        /// <param name="validator">An object to be used to validate the data contained in the
        /// <see cref="IAttribute"/>. Can be <see langword="null"/> to keep the existing validator
        /// or if data validation is not required.</param>
        /// <param name="tabStopMode">A <see cref="TabStopMode"/> value indicatng under what
        /// circumstances the attribute should serve as a tab stop. Can be <see langword="null"/> to
        /// keep the existing tabStopMode settin.</param>
        /// <param name="autoUpdateQuery">A query which will cause the <see cref="IAttribute"/>'s
        /// value to automatically be updated using values from other <see cref="IAttribute"/>s
        /// and/or a database query.</param>
        /// <param name="validationQuery">A query which will cause the validation list for the 
        /// validator associated with the attribute to be updated using values from other
        /// <see cref="IAttribute"/>'s and/or a database query.</param>
        [ComVisible(false)]
        public static void Initialize(IAttribute attribute, IUnknownVector sourceAttributes, 
            IDataEntryControl owningControl, int? displayOrder, bool considerPropagated,
            TabStopMode? tabStopMode, IDataEntryValidator validator, string autoUpdateQuery,
            string validationQuery)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI26134");

                // Create a new statusInfo instance (or retrieve an existing one).
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                // Set/update the owningControl value if necessary.
                if (statusInfo._owningControl != owningControl)
                {
                    statusInfo._owningControl = owningControl;
                }

                // Check to see if the display order should be set.
                bool reorder = false;
                if (displayOrder != null)
                {
                    // Set/update the displayOrder value if necessary.
                    string fullDisplayOrder = DataEntryMethods.GetTabIndex((Control)owningControl) +
                        "." + ((int)displayOrder).ToString(CultureInfo.CurrentCulture);

                    // If the displayOrder value has changed, sourceAttributes need to be reordered.
                    if (statusInfo._displayOrder != fullDisplayOrder)
                    {
                        reorder = true;
                        statusInfo._displayOrder = fullDisplayOrder;
                    }
                }

                // If the display order hasn't changed, but the attribute doesn't yet exist in
                // sourceAttributes, reordering needs to happen so that it gets added.
                if (!reorder)
                {
                    if (sourceAttributes.Size() == 0)
                    {
                        reorder = true;
                    }
                    else
                    {
                        int index = -1;
                        sourceAttributes.FindByReference(attribute, 0, ref index);
                        if (index < 0)
                        {
                            reorder = true;
                        }
                    }
                }

                if (reorder)
                {
                    DataEntryMethods.ReorderAttributes(sourceAttributes,
                            DataEntryMethods.AttributeAsVector(attribute));
                }

                // Set/update the propogated status if necessary.
                if (considerPropagated && !statusInfo._hasBeenPropagated)
                {
                    statusInfo._hasBeenPropagated = considerPropagated;
                }

                // Set/update the validator if necessary.
                if (validator != null && statusInfo._validator != validator)
                {
                    statusInfo._validator = validator;
                }

                // Update the tabStopMode if necessary
                if (tabStopMode != null && tabStopMode.Value != statusInfo._tabStopMode)
                {
                    statusInfo._tabStopMode = tabStopMode.Value;

                    if (tabStopMode == TabStopMode.OnlyWhenInvalid)
                    {
                        // If an attribute will be skipped in the tab order unless invalid, mark
                        // it as viewed. Otherwise tabbing through all fields in a document will
                        // leave fields using OnlyWhenInvalid mode as unread. 
                        statusInfo._hasBeenViewed = true;
                    }
                }

                // If an entry doesn't exist in _subAttributesToParentMap for this attribute's
                // sub-attributes, this is the first time it has been initialized.
                bool previouslyInitialized = 
                    _subAttributesToParentMap.ContainsKey(attribute.SubAttributes);

                // If the attribute's source attributes are have known parent, use it to generate
                // the attribute to parent mapping.
                IAttribute parentAttribute;
                if (_subAttributesToParentMap.TryGetValue(sourceAttributes, out parentAttribute))
                {
                    statusInfo._parentAttribute = parentAttribute;
                }

                // Add a mapping for the attribute's subattributes for future reference.
                _subAttributesToParentMap[attribute.SubAttributes] = attribute;

                if (autoUpdateQuery != null && autoUpdateQuery != statusInfo._autoUpdateQuery)
                {
                    // Dispose of any previously existing auto-update trigger.
                    AutoUpdateTrigger existingAutoUpdateTrigger;
                    if (_autoUpdateTriggers.TryGetValue(attribute, out existingAutoUpdateTrigger))
                    {
                        existingAutoUpdateTrigger.Dispose();
                        _autoUpdateTriggers.Remove(attribute);
                    }

                    statusInfo._autoUpdateQuery = autoUpdateQuery;

                    if (!string.IsNullOrEmpty(autoUpdateQuery))
                    {
                        // We need to ensure that the attribute is a part of the sourceAttributes
                        // in order for AutoUpdateTrigger to creation to work. When creating a new
                        // attribute, this won't be the case.  Add it now, even though it will still
                        // need to be re-ordered later.
                        sourceAttributes.PushBackIfNotContained(attribute);

                        _autoUpdateTriggers[attribute] =
                            new AutoUpdateTrigger(attribute, autoUpdateQuery, _dbConnection, false);
                    }
                }

                foreach (AutoUpdateTrigger autoUpdateTrigger in _autoUpdateTriggers.Values)
                {
                    if (!autoUpdateTrigger.GetIsFullyResolved())
                    {
                        // We need to ensure that the attribute is a part of the sourceAttributes
                        // in order for RegisterTriggerCandidate to work. When creating a new
                        // attribute, this won't be the case.  Add it now, even though it will still
                        // need to be re-ordered later.
                        sourceAttributes.PushBackIfNotContained(attribute);

                        autoUpdateTrigger.RegisterTriggerCandidate(attribute);
                    }
                }

                if (validationQuery != null && validationQuery != statusInfo._validationQuery)
                {
                    // Dispose of any previously existing validation trigger.
                    AutoUpdateTrigger existingValidationTrigger;
                    if (_validationTriggers.TryGetValue(attribute, out existingValidationTrigger))
                    {
                        existingValidationTrigger.Dispose();
                        _validationTriggers.Remove(attribute);
                    }

                    statusInfo._validationQuery = validationQuery;

                    if (!string.IsNullOrEmpty(validationQuery))
                    {
                        // We need to ensure that the attribute is a part of the sourceAttributes
                        // in order for AutoUpdateTrigger to creation to work. When creating a new
                        // attribute, this won't be the case.  Add it now, even though it will still
                        // need to be re-ordered later.
                        sourceAttributes.PushBackIfNotContained(attribute);

                        _validationTriggers[attribute] =
                            new AutoUpdateTrigger(attribute, validationQuery, _dbConnection, true);
                    }
                }
                else
                {
                    // If a validation trigger is in place, use it to update the control's
                    // validationlist now since by virtue of the fact that the attribute is being
                    // re-initialized, the control was likely previously displaying a different
                    // attribute with a different validation list.
                    AutoUpdateTrigger validationTrigger = null;
                    if (_validationTriggers.TryGetValue(attribute, out validationTrigger))
                    {
                        validationTrigger.UpdateValue();
                    }
                }

                foreach (AutoUpdateTrigger validationTrigger in _validationTriggers.Values)
                {
                    if (!validationTrigger.GetIsFullyResolved())
                    {
                        // We need to ensure that the attribute is a part of the sourceAttributes
                        // in order for RegisterTriggerCandidate to work. When creating a new
                        // attribute, this won't be the case.  Add it now, even though it will still
                        // need to be re-ordered later.
                        sourceAttributes.PushBackIfNotContained(attribute);

                        validationTrigger.RegisterTriggerCandidate(attribute);
                    }
                }

                // [DataEntry:173] Trim any whitespace from the beginning and end.
                // [DataEntry:167]
                // Accessing the value also ensures the value is accessed and, thus, created.
                attribute.Value.Trim(" \t\r\n", " \t\r\n");

                // Raise the AttributeInitialized event if it hasn't already been raised for this
                // attribute.
                if (!previouslyInitialized)
                {
                    OnAttributeInitialized(attribute, sourceAttributes, owningControl);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24684", ex);
            }
        }

        /// <overloads>Applies the specified value to the specified <see cref="IAttribute"/> and
        /// raises <see cref="AttributeValueModified"/> when appropriate.</overloads>
        /// <summary>
        /// Applies the specified value to the specified <see cref="IAttribute"/> and raises
        /// <see cref="AttributeValueModified"/> when appropriate.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose value is to be updated.
        /// </param>
        /// <param name="value">The <see cref="SpatialString"/> value to apply to the
        /// <see cref="IAttribute"/>.</param>
        /// <param name="acceptSpatialInfo"><see langword="true"/> if the attribute's value should
        /// be marked as accepted, <see langword="false"/> if it should be left as-is.</param>
        /// <param name="endOfEdit"><see langword="true"/> if this change represents the end of the
        /// current edit, <see langword="false"/> if it is or may be part of an ongoing edit.
        /// </param>
        [ComVisible(false)]
        public static void SetValue(IAttribute attribute, SpatialString value,
            bool acceptSpatialInfo, bool endOfEdit)
        {
            // In case of an error applying the new value, keep track of the original value.
            SpatialString originalValue = null;

            try
            {
                // Validate the license
                _licenseCache.Validate("ELI27093");

                // Make a copy of the original value.
                ICopyableObject copySource = (ICopyableObject)attribute.Value;
                originalValue = (SpatialString)copySource.Clone();

                attribute.Value = value;

                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                if (_endEditInProgress)
                {
                    // If EndEdit is currently being processed (and this is a result of it), raise
                    // the non-incremental modification event now.
                    statusInfo.OnAttributeValueModified(attribute, false, acceptSpatialInfo, true);
                }
                else
                {
                    if (!endOfEdit)
                    {
                        // If not the end of the edit raise an incremental modification event and
                        // queue the value for an eventual non-incremental event.
                        statusInfo.OnAttributeValueModified(attribute, true, acceptSpatialInfo, true);
                    }

                    KeyValuePair<bool, SpatialString> existingModification;
                    if (_attributesBeingModified.TryGetValue(attribute, out existingModification))
                    {
                        // If the attribute already exists in _attributesBeingModified, but is
                        // marked as a non-spatial change, mark it as a spatial change instead.
                        if (existingModification.Key == false)
                        {
                            _attributesBeingModified[attribute] =
                                new KeyValuePair<bool, SpatialString>(
                                    true, existingModification.Value);
                        }
                    }
                    else
                    {
                        // If the attribute has not been added to _attributesBeingModified, add it.
                        _attributesBeingModified[attribute] =
                            new KeyValuePair<bool, SpatialString>(true, originalValue);
                    }

                    // After queing the modification, call EndEdit if directed.
                    if (endOfEdit)
                    {
                        EndEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                // If there was an exception applying the value, restore the original value to
                // prevent exceptions from continuously being generated.
                if (originalValue != null)
                {
                    try
                    {
                        attribute.Value = originalValue;

                        // After setting the value, refresh the value and raise
                        // AttributeValueModified to notify the host of the change.
                        AttributeStatusInfo statusInfo =
                            AttributeStatusInfo.GetStatusInfo(attribute);
                        statusInfo.OwningControl.RefreshAttributes(new IAttribute[] { attribute }, 
                            true);
                        statusInfo.OnAttributeValueModified(attribute, false, false, true);
                    }
                    catch (Exception ex2)
                    {
                        ExtractException.Log("ELI27094", ex2);
                    }
                }

                throw ExtractException.AsExtractException("ELI26090", ex);
            }
        }

        /// <summary>
        /// Applies the specified value to the specified <see cref="IAttribute"/> and raises
        /// <see cref="AttributeValueModified"/> when appropriate.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose value is to be updated.
        /// </param>
        /// <param name="value">The <see langword="string"/> value to apply to the
        /// <see cref="IAttribute"/>.</param>
        /// <param name="acceptSpatialInfo"><see langword="true"/> if the attribute's value should
        /// be marked as accepted, <see langword="false"/> if it should be left as-is.</param>
        /// <param name="endOfEdit"><see langword="true"/> if this change represents the end of the
        /// current edit, <see langword="false"/> if it is or may be part of an ongoing edit.
        /// </param>
        [ComVisible(false)]
        public static void SetValue(IAttribute attribute, string value, bool acceptSpatialInfo,
            bool endOfEdit)
        {
            // In case of an error applying the new value, keep track of the original value.
            SpatialString originalValue = null;

            try
            {
                // Validate the license
                _licenseCache.Validate("ELI26135");

                // Don't do anything if the specified value matches the existing value.
                if (attribute.Value.String != value)
                {
                    // Make a copy of the original value.
                    ICopyableObject copySource = (ICopyableObject)attribute.Value;
                    originalValue = (SpatialString)copySource.Clone();

                    // If the attribute doesn't contain any spatial information, just
                    // change the text.
                    if (attribute.Value.GetMode() == ESpatialStringMode.kNonSpatialMode)
                    {
                        attribute.Value.ReplaceAndDowngradeToNonSpatial(value);
                    }
                    // If the attribute contains spatial information, it needs to be converted to
                    // hybrid mode to update the text.
                    else
                    {
                        attribute.Value.ReplaceAndDowngradeToHybrid(value);
                    }

                    AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                    if (_endEditInProgress)
                    {
                        // If EndEdit is currently being processed (and this is a result of it), raise
                        // the non-incremental modification event now.
                        statusInfo.OnAttributeValueModified(attribute, false, acceptSpatialInfo, false);
                    }
                    else
                    {
                        // Raise an incremental modification event and queue the value for an
                        // eventual non-incremental event.
                        statusInfo.OnAttributeValueModified(attribute, true, acceptSpatialInfo, false);

                        if (!_attributesBeingModified.ContainsKey(attribute))
                        {
                            _attributesBeingModified[attribute] =
                                new KeyValuePair<bool, SpatialString>(false, originalValue);
                        }
                    }
                }

                // Call EndEdit if directed (whether or not the value actually changed).
                if (endOfEdit)
                {
                    EndEdit();
                }
            }
            catch (Exception ex)
            {
                // If there was an exception applying the value, restore the original value to
                // prevent exceptions from continuously being generated.
                if (originalValue != null)
                {
                    try
                    {
                        attribute.Value = originalValue;

                        // After setting the value, refresh the value and raise
                        // AttributeValueModified to notify the host of the change.
                        AttributeStatusInfo statusInfo =
                            AttributeStatusInfo.GetStatusInfo(attribute);
                        statusInfo.OwningControl.RefreshAttributes(new IAttribute[] { attribute },
                            false);
                        statusInfo.OnAttributeValueModified(attribute, false, false, false);
                    }
                    catch (Exception ex2)
                    {
                        ExtractException.Log("ELI27095", ex2);
                    }
                }

                throw ExtractException.AsExtractException("ELI26093", ex);
            }
        }

        /// <summary>
        /// Deletes the specified attribute from the DataEntry framework. This releases all mappings
        /// and events tied to the attribute.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to delete from the system.</param>
        [ComVisible(false)]
        public static void DeleteAttribute(IAttribute attribute)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI26136");

                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                // Dispose of any auto-update trigger for the attribute.
                AutoUpdateTrigger autoUpdateTrigger = null;
                if (_autoUpdateTriggers.TryGetValue(attribute, out autoUpdateTrigger))
                {
                    _autoUpdateTriggers.Remove(attribute);
                    autoUpdateTrigger.Dispose();
                }

                // Dispose of any validation trigger for the attribute.
                AutoUpdateTrigger validationTrigger = null;
                if (_validationTriggers.TryGetValue(attribute, out validationTrigger))
                {
                    _validationTriggers.Remove(attribute);

                    validationTrigger.Dispose();
                }

                IUnknownVector subAttributes = attribute.SubAttributes;
                _subAttributesToParentMap.Remove(attribute.SubAttributes);

                // Recursively process each sub-attribute and process it as well.
                int count = subAttributes.Size();
                for (int i = 0; i < count; i++)
                {
                    // Since each attribute will be removed from subAttributes when DeleteAttributes
                    // is called, always delete from the first index since the vector will be one
                    // smaller with each iteration.
                    DeleteAttribute((IAttribute)subAttributes.At(0));
                }

                // Remove the attribute from the overall attribute heirarchy.
                if (statusInfo._parentAttribute != null)
                {
                    statusInfo._parentAttribute.SubAttributes.RemoveValue(attribute);
                }
                else
                {
                    _attributes.RemoveValue(attribute);
                }

                // Raise the AttributeDeleted event last otherwise it can cause the hosts' count
                // of invalid and unviewed attributes to be off.
                statusInfo.OnAttributeDeleted(attribute);

                // [DataEntry:693]
                // Since the attribute will no longer be accessed by the DataEntry, it needs to be
                // released with FinalReleaseComObject to prevent handle leaks.
                Marshal.FinalReleaseComObject(attribute);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26107", ex);
            }
        }

        /// <summary>
        /// Checks to see if the specified <see cref="IAttribute"/>'s data has been viewed by the
        /// user. (This usually means that the control displaying the attribute has received focus).
        /// <para><b>NOTE:</b></para>
        /// An <see cref="IAttribute"/> is always considered as "viewed" if it is un-viewable.
        /// (See <see cref="IsViewable"/>).
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to be checked for whether it has
        /// been viewed.</param>
        /// <param name="recursive"><see langword="false"/> to check only the specified
        /// <see cref="IAttribute"/>, <see langword="true"/> to check all descendant
        /// <see cref="IAttribute"/>s as well.</param>
        /// <returns><see langword="true"/> if the <see cref="IAttribute"/> has been viewed, or
        /// <see langword="false"/> if it has not.</returns>
        [ComVisible(false)]
        public static bool HasBeenViewed(IAttribute attribute, bool recursive)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                if (statusInfo._isViewable && !statusInfo._hasBeenViewed)
                {
                    // This attribute has not been viewed.
                    return false;
                }
                else if (recursive)
                {
                    // Check to see that all subattributes have been viewed as well.
                    return AttributeScanner.Scan(attribute.SubAttributes, null,
                        ConfirmDataViewed, true, true, true, null);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24480", ex);
            }
        }

        /// <summary>
        /// Checks to see if the specified <see cref="IUnknownVector"/> of 
        /// <see cref="IAttribute"/>'s (and their sub-attributes) have been viewed by the user. If 
        /// not, the first <see cref="IAttribute"/> that has not been viewed is specified.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/> to be checked for whether it has
        /// been viewed.</param>
        /// <param name="startingPoint">A genealogy of <see cref="IAttribute"/>s describing 
        /// the point at which the scan should be started with each attribute further down the
        /// the stack being a descendent to the previous <see cref="IAttribute"/> in the stack.
        /// </param>
        /// <param name="forward"><see langword="true"/> to scan forward through the attribute 
        /// hierarchy, <see langword="false"/> to scan backward.</param>
        /// <param name="loop"><see langword="true"/> to resume scanning from the beginning of
        /// the <see cref="IAttribute"/>s (back to the starting point) if the end was reached 
        /// successfully, <see langword="false"/> to end the scan once the end of the 
        /// <see cref="IAttribute"/> vector is reached.</param>
        /// <returns>A stack of <see cref="IAttribute"/>s
        /// where the first attribute in the stack represents the root-level attribute
        /// the unviewed attribute is descended from, and each successive attribute represents
        /// a sub-attribute to the previous until the final attribute is the first unviewed 
        /// attribute.</returns>
        [ComVisible(false)]
        public static Stack<IAttribute> FindNextUnviewedAttribute(IUnknownVector attributes, 
            Stack<IAttribute> startingPoint, bool forward, bool loop)
        {
            try
            {
                Stack<IAttribute> unviewedAttributes = new Stack<IAttribute>();

                if (!AttributeScanner.Scan(attributes, startingPoint, ConfirmDataViewed, true,
                    forward, loop, unviewedAttributes))
                {
                    return unviewedAttributes;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24647", ex);
            }
        }

        /// <summary>
        /// Marks the specified <see cref="IAttribute"/> as viewed or not viewed.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> which should be marked as viewed
        /// or not viewed.</param>
        /// <param name="hasBeenViewed"><see langword="true"/> to indicate that the 
        /// <see cref="IAttribute"/> has been viewed, <see langword="false"/> to indicate it has
        /// not been viewed.</param>
        [ComVisible(false)]
        public static void MarkAsViewed(IAttribute attribute, bool hasBeenViewed)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                // If the viewed status of the attribute has changed from its previous value,
                // raise the ViewedStateChanged event to notify listeners of the new status.
                if (statusInfo._isViewable && statusInfo._hasBeenViewed != hasBeenViewed)
                {
                    statusInfo._hasBeenViewed = hasBeenViewed;

                    OnViewedStateChanged(attribute, hasBeenViewed);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24481", ex);
            }
        }

        /// <summary>
        /// Checks to see if the specified <see cref="IUnknownVector"/> of 
        /// <see cref="IAttribute"/>'s (and their sub-attributes) have passed data validation. If 
        /// not, the first invalid <see cref="IAttribute"/> is specified.
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s 
        /// to be checked for whether its data is valid.</param>
        /// <param name="startingPoint">A genealogy of <see cref="IAttribute"/>s describing 
        /// the point at which the scan should be started with each attribute further down the
        /// the stack being a descendent to the previous <see cref="IAttribute"/> in the stack.
        /// </param>
        /// <param name="forward"><see langword="true"/> to scan forward through the attribute 
        /// hierarchy, <see langword="false"/> to scan backward.</param>
        /// <param name="loop"><see langword="true"/> to resume scanning from the beginning of
        /// the <see cref="IAttribute"/>s (back to the starting point) if the end was reached 
        /// successfully, <see langword="false"/> to end the scan once the end of the 
        /// <see cref="IAttribute"/> vector is reached.</param>
        /// <returns><see langword="true"/> if the data <see cref="IAttribute"/>s' data 
        /// and the data of all sub-<see cref="IAttribute"/>s is know to be valid; 
        /// <see langword="false"/> if any of descendants contain invalid data or if
        /// the validity of their data has not yet been checked.</returns>
        /// <returns>A stack of <see cref="IAttribute"/>s
        /// where the first attribute in the stack represents the root-level attribute
        /// the first invalid attribute is descended from, and each successive attribute represents
        /// a sub-attribute to the previous until the final attribute is the first invalid attribute.
        /// </returns>
        [ComVisible(false)]
        public static Stack<IAttribute> FindNextInvalidAttribute(IUnknownVector attributes,
            Stack<IAttribute> startingPoint, bool forward, bool loop)
        {
            try
            {
                Stack<IAttribute> invalidAttributes = new Stack<IAttribute>();

                if (!AttributeScanner.Scan(attributes, startingPoint, ConfirmDataValidity, true,
                    forward, loop, invalidAttributes))
                {
                    return invalidAttributes;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24410", ex);
            }
        }

        /// <summary>
        /// Mark the data associated with the specified attribute as valid or invalid.  This applies
        /// only to the associated attribute's data, not to any subattributes.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the data should be marked
        /// valid or invalid.</param>
        /// <param name="dataIsValid"><see langword="true"/> if the attribute's data should be
        /// considered valid; <see langword="false"/> otherwise.</param>
        [ComVisible(false)]
        public static void MarkDataAsValid(IAttribute attribute, bool dataIsValid)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                // If the validation status of the attribute has changed from its previous value,
                // raise the ValidationStateChanged event to notify listeners of the new status.
                if (statusInfo._isViewable && statusInfo._dataIsValid != dataIsValid)
                {
                    statusInfo._dataIsValid = dataIsValid;

                    OnValidationStateChanged(attribute, dataIsValid);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24494", ex);
            }
        }

        /// <summary>
        /// Checks to see if the specified <see cref="IAttribute"/>'s data is valid.
        /// <para><b>NOTE:</b></para>
        /// An <see cref="IAttribute"/> is never considered invalid if it is not viewable.
        /// (See <see cref="IsViewable"/>).
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to be checked.</param>
        /// <returns><see langword="true"/> if the <see cref="IAttribute"/>'s data is valid or 
        /// <see langword="false"/> if it is not valid.</returns>
        [ComVisible(false)]
        public static bool IsDataValid(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return (!statusInfo._isViewable || statusInfo._dataIsValid);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24919", ex);
            }
        }

        /// <summary>
        /// Enables or disables data validation on the specified attribute.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the data validation
        /// should be enabled or disabled.</param>
        /// <param name="enable"><see langword="true"/> if the data validation should be enabled;
        /// <see langword="false"/> otherwise.</param>
        [ComVisible(false)]
        public static void EnableValidation(IAttribute attribute, bool enable)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                statusInfo._validationEnabled = enable;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26963", ex);
            }
        }

        /// <summary>
        /// Gets whether data validation is enabled on the specified <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to be checked.</param>
        /// <returns><see langword="true"/> if data validation is enabled <see langword="false"/>
        /// if it is not.</returns>
        [ComVisible(false)]
        public static bool IsValidationEnabled(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._validationEnabled;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26964", ex);
            }
        }

        /// <summary>
        /// Checks to see whether the associated <see cref="IAttribute"/> has been fully propagated
        /// into <see cref="IDataEntryControl"/>s.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/> to be checked for whether it has
        /// been propagated.</param>
        /// <param name="startingPoint">A genealogy of <see cref="IAttribute"/>s describing 
        /// the point at which the scan should be started with each attribute further down the
        /// the stack being a descendent to the previous <see cref="IAttribute"/> in the stack.
        /// </param>
        /// <param name="unPropagatedAttributes">A stack of <see cref="IAttribute"/>s
        /// where the first attribute in the stack represents the root-level attribute
        /// the first unpropagated attribute is descended from, and each successive attribute 
        /// represents a sub-attribute to the previous until the final attribute is the first 
        /// unpropagated attribute.
        /// </param>
        /// <returns><see langword="true"/> if the <see cref="IAttribute"/> and all subattributes
        /// have been flagged as propagated; <see langword="false"/> otherwise.</returns>
        [ComVisible(false)]
        public static bool HasBeenPropagated(IUnknownVector attributes,
            Stack<IAttribute> startingPoint, Stack<IAttribute> unPropagatedAttributes)
        {
            try
            {
                return AttributeScanner.Scan(attributes, startingPoint, ConfirmHasBeenPropagated,
                    true, true, true, unPropagatedAttributes);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24450", ex);
            }
        }

        /// <summary>
        /// Mark the data associated with the specified <see cref="IAttribute"/> as propagated.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the data should be marked
        /// propagated or not propagated.</param>
        /// <param name="propagated"><see langword="true"/> to indicate the 
        /// <see cref="IAttribute"/> has been propagated, <see langword="false"/> to indicate the
        /// <see cref="IAttribute"/> has not been propagated.</param>
        /// <param name="recursive">If <see langword="false"/> only the specified 
        /// <see cref="IAttribute"/> will be marked as propagated.  If <see langword="true"/> all 
        /// descendents <see cref="IAttribute"/>s will be marked as propagated as well.</param>
        [ComVisible(false)]
        public static void MarkAsPropagated(IAttribute attribute, bool propagated, bool recursive)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                statusInfo._hasBeenPropagated = propagated;

                if (recursive)
                {
                    // Mark all descendant attributes as propagated as well.
                    AttributeScanner.Scan(attribute.SubAttributes, null, MarkAsPropagated,
                        propagated, true, true, null);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24478", ex);
            }
        }

        /// <summary>
        /// Specifies whether the specified <see cref="IAttribute"/> is viewable or not.
        /// "Viewable" means the value can be displayed, but is not necessarily currently displayed
        /// depending on which attributes are propagated.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose viewable status is to be
        /// set.</param>
        /// <param name="isViewable"><see langword="true"/> if the <see cref="IAttribute"/> is
        /// viewable, <see langword="false"/> if it is not.</param>
        /// <returns><see langword="true"/> in all cases to continue the scan.</returns>
        [ComVisible(false)]
        public static bool MarkAsViewable(IAttribute attribute, bool isViewable)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                if (statusInfo._isViewable != isViewable)
                {
                    // Don't allow any attribute that belongs to a disabled data entry control to
                    // be marked as viewable since the value will not be selectable or editable.
                    if (isViewable &&
                        statusInfo._owningControl != null && statusInfo._owningControl.Disabled)
                    {
                        return true;
                    }

                    statusInfo._isViewable = isViewable;

                    // If the attribute was not previously viewable, it would not have been
                    // considered unviewed.  Now it will be considered unviewed so the 
                    // ViewedStateChanged event needs to be raised (assuming the attribute
                    // is actually unviewed).
                    if (!statusInfo._hasBeenViewed)
                    {
                        OnViewedStateChanged(attribute, false);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24856", ex);
            }
        }

        /// <summary>
        /// Indicates whether the specified <see cref="IAttribute"/> is viewable in the 
        /// <see cref="DataEntryControlHost"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose viewable status is to be
        /// checked.</param>
        /// <returns><see langword="true"/> if the specified <see cref="IAttribute"/> is viewable;
        /// <see langword="false"/> if it is not.</returns>
        [ComVisible(false)]
        public static bool IsViewable(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._isViewable;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25138", ex);
            }
        }
        
        /// <summary>
        /// Finds the first <see cref="IAttribute"/> that is a tabstop in the DEP after the
        /// specified starting point.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/>s from which a tabstop
        /// <see cref="IAttribute"/> is to be sought.</param>
        /// <param name="startingPoint">A genealogy of <see cref="IAttribute"/>s describing 
        /// the point at which the scan should be started with each attribute further down the
        /// the stack being a descendent to the previous <see cref="IAttribute"/> in the stack.
        /// </param>
        /// <param name="forward"><see langword="true"/> to scan forward through the
        /// <see cref="IAttribute"/>s, <see langword="false"/> to scan backward.</param>
        /// <returns>A genealogy of <see cref="IAttribute"/>s specifying the next tabstop 
        /// <see cref="IAttribute"/>.</returns>
        [ComVisible(false)]
        public static Stack<IAttribute> GetNextTabStopAttribute(IUnknownVector attributes,
            Stack<IAttribute> startingPoint, bool forward)
        {
            try
            {
                Stack<IAttribute> nextTabStopAttributeGenealogy = new Stack<IAttribute>();

                if (!AttributeScanner.Scan(
                        attributes, startingPoint, ConfirmIsTabStop, false, forward, true,
                        nextTabStopAttributeGenealogy))
                {
                    return nextTabStopAttributeGenealogy;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24984", ex);
            }
        }

        /// <summary>
        /// Updates the <see cref="AttributeStatusInfo.DisplayOrder"/> value associated with the 
        /// specified <see cref="IAttribute"/> using the provided displayOrder value.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose 
        /// <see cref="AttributeStatusInfo.DisplayOrder"/> value is to be updated.</param>
        /// <param name="displayOrder">An <see langword="integer"/> representing the display order
        /// of the <see cref="IAttribute"/> within its immediate containing control.</param>
        [ComVisible(false)]
        public static void UpdateDisplayOrder(IAttribute attribute, int displayOrder)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);
                ExtractException.Assert("ELI24688",
                    "Cannot update display order without owning control!",
                    statusInfo._owningControl != null);

                statusInfo._displayOrder = 
                    DataEntryMethods.GetTabIndex((Control)statusInfo._owningControl) + "." +
                    displayOrder.ToString(CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24687", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="HintType"/> associated with the <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose hint type is to be checked.
        /// </param>
        /// <returns>The <see cref="HintType"/> associated with the <see cref="IAttribute"/>.
        /// </returns>
        [ComVisible(false)]
        public static HintType GetHintType(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._hintType;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25230", ex);
            }
        }

        /// <summary>
        /// Sets the <see cref="HintType"/> associated with the <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose hint type is to be set.
        /// </param>
        /// <param name="hintType">The <see cref="HintType"/> to be associated with the 
        /// <see cref="IAttribute"/>.</param>
        [ComVisible(false)]
        public static void SetHintType(IAttribute attribute, HintType hintType)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                statusInfo._hintType = hintType;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25359", ex);
            }
        }

        /// <summary>
        /// Gets whether the user has accepted the value of the <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to be checked.</param>
        /// <returns><see langword="true"/> if the user has accepted the value of the
        /// <see cref="IAttribute"/>; <see langword="false"/> otherwise.
        /// </returns>
        [ComVisible(false)]
        public static bool IsAccepted(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._isAccepted;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25388", ex);
            }
        }

        /// <summary>
        /// Sets whether the <see cref="IAttribute"/>'s spatial info has been accepted by the user.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose spatial info is to be 
        /// accepted/unaccepted.</param>
        /// <param name="accept"><see langword="true"/> to indicated the value of the
        /// <see cref="IAttribute"/> has been accepted by the user; <see langword="false"/>
        /// otherwise.</param>
        [ComVisible(false)]
        public static void AcceptValue(IAttribute attribute, bool accept)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                statusInfo._isAccepted = accept;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25389", ex);
            }
        }

        /// <summary>
        /// Gets whether hints are enabled for the <see cref="IAttribute"/>. The fact that hints
        /// are enabled doesn't necessarily mean the attribute has one.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose whose hint enabled status is
        /// to be checked.</param>
        /// <returns><see langword="true"/> if hints are enabled for the specified
        /// <see cref="IAttribute"/>; <see langword="false"/> otherwise.
        /// </returns>
        [ComVisible(false)]
        public static bool HintEnabled(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._hintEnabled;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25979", ex);
            }
        }

        /// <summary>
        /// Sets whether hints are enabled for the <see cref="IAttribute"/>. The fact that hints
        /// are enabled doesn't necessarily mean the attribute has one.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose whose hint enabled status is
        /// to be set.</param>
        /// <param name="hintEnabled"><see langword="true"/> to enable hints for the specified
        /// <see cref="IAttribute"/>; <see langword="false"/> otherwise.
        /// </param>
        [ComVisible(false)]
        public static void EnableHint(IAttribute attribute, bool hintEnabled)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                statusInfo._hintEnabled = hintEnabled;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25980", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="IDataEntryControl"/> that the <see cref="IAttribute"/> is associated
        /// with.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose owning control is to be
        /// checked.</param>
        /// <returns>The <see cref="IDataEntryControl"/> that the specified <see cref="IAttribute"/>
        /// is associated with.
        /// </returns>
        [ComVisible(false)]
        public static IDataEntryControl GetOwningControl(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._owningControl;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25350", ex);
            }
        }

        /// <summary>
        /// Sets the <see cref="Extract.Imaging.RasterZone"/>s that define a hint for the specified
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the spatial hint is
        /// defined.</param>
        /// <param name="rasterZones">A list of <see cref="Extract.Imaging.RasterZone"/>s 
        /// that define the spatial hint.</param>
        [ComVisible(false)]
        public static void SetHintRasterZones(IAttribute attribute, 
            IEnumerable<Extract.Imaging.RasterZone> rasterZones)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                statusInfo._hintRasterZones = rasterZones;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25501", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="Extract.Imaging.RasterZone"/>s that define a hint for the specified
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the spatial hint is needed.
        /// </param>
        /// <returns>A list of <see cref="Extract.Imaging.RasterZone"/>s that define the spatial hint.
        /// </returns>
        [ComVisible(false)]
        public static IEnumerable<Extract.Imaging.RasterZone> GetHintRasterZones(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._hintRasterZones;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25506", ex);
            }
        }

        /// <summary>
        /// Retrieves the parent <see cref="IAttribute"/> of the specified attribute.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose parent is needed.</param>
        /// <returns>The parent <see cref="IAttribute"/> if there is one, <see langword="null"/> if
        /// there is not.</returns>
        [ComVisible(false)]
        public static IAttribute GetParentAttribute(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._parentAttribute;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26097", ex);
            }
        }

        /// <summary>
        /// Indicates whether the attribute will be persisted in output.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose persistence status is needed.
        /// </param>
        /// <returns><see langword="true"/> if the <see cref="IAttribute"/> will be persisted in
        /// output; <see langword="false"/> otherwise.</returns>
        [ComVisible(false)]
        public static bool IsAttributePersistable(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                return statusInfo._persistAttribute;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26588", ex);
            }
        }
        
        /// <summary>
        /// Specifies whether the attribute should be persisted in output.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose persistence status is to be
        /// changed.</param>
        /// <param name="persistAttribute"><see langword="true"/> if the <see cref="IAttribute"/>
        /// should be persisted in output; <see langword="false"/> otherwise.</param>
        [ComVisible(false)]
        public static void SetAttributeAsPersistable(IAttribute attribute, bool persistAttribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                statusInfo._persistAttribute = persistAttribute;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26589", ex);
            }
        }

        /// <summary>
        /// Removes all spatial info associated with the <see cref="IAttribute"/> (including any
        /// hint).
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose spatial info is to be
        /// removed.</param>
        /// <returns><see langword="true"/> if spatial info was removed, <see langword="false"/>
        /// if there was no spatial info to remove.</returns>
        [ComVisible(false)]
        public static bool RemoveSpatialInfo(IAttribute attribute)
        {
            try
            {
                AttributeStatusInfo statusInfo = GetStatusInfo(attribute);

                bool spatialInfoRemoved = false;

                // If the attribute has spatial information (a highlight), remove it and
                // flag the attribute so that hints are not created in its place.
                if (attribute.Value.HasSpatialInfo())
                {
                    attribute.Value.DowngradeToNonSpatialMode();

                    spatialInfoRemoved = true;
                }
                // If the attribute has an associated hint, remove the hint and flag the
                // attribute so that hints are not re-created. 
                else if (statusInfo._hintType != HintType.None)
                {
                    spatialInfoRemoved = true;
                }

                if (spatialInfoRemoved)
                {
                    statusInfo._hintEnabled = false;

                    // Notify listeners that spatial info has changed.
                    statusInfo.OnAttributeValueModified(attribute, true, false, true);
                }

                return spatialInfoRemoved;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27320", ex);
            }
        }

        /// <overloads>Obtains the full path of the <see cref="IAttribute"/> from the root of the
        /// hierarchy.</overloads>
        /// <summary>
        /// Obtains the full path of the <see cref="IAttribute"/> from the root of the hierarchy.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which a path is needed.</param>
        /// <returns>The full path of the <see cref="IAttribute"/> of blank if the specified
        /// attribute is <see langword="null"/>.</returns>
        [ComVisible(false)]
        public static string GetFullPath(IAttribute attribute)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI26138");

                // If the specified attribute is null, just return blank.
                if (attribute == null)
                {
                    return "";
                }

                // Obtain the path of this attribute's parent.
                string parentPath = GetFullPath(GetStatusInfo(attribute)._parentAttribute);

                // If the parent doesn't exist, just return this attribute's name.
                if (string.IsNullOrEmpty(parentPath))
                {
                    return attribute.Name;
                }
                // Otherwise, append the name of this attribute to the parent attribute's path.
                else
                {
                    return parentPath + "/" + attribute.Name;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26137", ex);
            }
        }

        /// <summary>
        /// Obtains the full path of the specified path when the specified query is applied to it.
        /// </summary>
        /// <param name="startingPath">The starting path.</param>
        /// <param name="query">The attribute query to apply to <see paramref="startingPath"/>.
        /// </param>
        /// <returns>The resulting full path.</returns>
        [ComVisible(false)]
        public static string GetFullPath(string startingPath, string query)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI26139");

                // Tokenize the query and process each element in order.
                string[] pathTokens =
                         query.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string pathToken in pathTokens)
                {
                    // If the parent is specified, remove the last level from the startingPath.
                    if (pathToken == "..")
                    {
                        ExtractException.Assert("ELI26105", "Invalid attribute path!",
                            !string.IsNullOrEmpty(startingPath));

                        // If the starting path has any remaining slashes, remove the end of the
                        // paths (starting with the last slash).
                        int lastSlash = startingPath.LastIndexOf('/');
                        if (lastSlash >= 0)
                        {
                            startingPath = startingPath.Substring(0, lastSlash);
                        }
                        // Otherwise the starting path is now empty.
                        else
                        {
                            startingPath = "";
                        }
                    }
                    // Add the next specified attribute from the query to the path.
                    else if (pathToken != ".")
                    {
                        if (!string.IsNullOrEmpty(startingPath))
                        {
                            startingPath += "/";
                        }

                        startingPath += pathToken;
                    }
                }

                return startingPath;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26110", ex);
            }
        }

        /// <summary>
        /// Returns the a list of the <see cref="IAttribute"/>s which match the specified query
        /// applied to the specified root attribute.
        /// </summary>
        /// <param name="rootAttribute">The <see cref="IAttribute"/> on which the query is to be
        /// executed.</param>
        /// <param name="query">The query to execute on the specified <see cref="IAttribute"/>.
        /// </param>
        /// <returns>The <see cref="IUnknownVector"/> of all <see cref="IAttribute"/>s matching the
        /// query.</returns>
        [ComVisible(false)]
        // Returning a list so the count can be checked results can be accessed by index.
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static List<IAttribute> ResolveAttributeQuery(IAttribute rootAttribute, string query)
        {
            try
            {
                List<IAttribute> results = new List<IAttribute>();

                // Trim off any leading slash.
                if (!string.IsNullOrEmpty(query) && query[0] == '/')
                {
                    query = query.Remove(0, 1);
                }

                // If there is nothing left in the query, return the root attribute.
                if (string.IsNullOrEmpty(query) || query == ".")
                {
                    ExtractException.Assert("ELI26140", "Invalid attribute query!",
                        rootAttribute != null);

                    // Return the rootAttribute if the query is null.
                    results.Add(rootAttribute);
                    return results;
                }
                // If the parent attribute is specified, return the result of running the remaining
                // query on the parent attribute.
                else if (query.Substring(0, 2) == "..")
                {
                    ExtractException.Assert("ELI26141", "Invalid attribute query!",
                        rootAttribute != null);

                    results.AddRange(ResolveAttributeQuery(GetStatusInfo(rootAttribute)._parentAttribute,
                        (query.Length > 3) ? query.Substring(3) : null));
                }
                // Otherwise, apply the next element of the query.
                else
                {
                    // Determine the remaining query after this element.
                    string nextQuery = null;
                    int queryEnd = query.IndexOf('/');
                    if (queryEnd != -1)
                    {
                        nextQuery = query.Substring(queryEnd);
                        query = query.Substring(0, queryEnd);
                    }

                    // If we are at the root of the heirarchy, _attributes needs to be used for the
                    // query as there will be no rootAttribute.
                    IUnknownVector attributesToQuery =
                        (rootAttribute == null) ? _attributes : rootAttribute.SubAttributes;

                    // Apply the remaining query to all attributes matching the current query.
                    int count = attributesToQuery.Size();
                    for (int i = 0; i < count; i++)
                    {
                        IAttribute attribute = (IAttribute)attributesToQuery.At(i);
                        if (query.Equals(attribute.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            results.AddRange(
                                ResolveAttributeQuery(attribute, nextQuery));
                        }
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26102", ex);
                ee.AddDebugData("rootAttribute", 
                    (rootAttribute == null) ? "null" : rootAttribute.Name, false);
                ee.AddDebugData("query", query, false);
                throw ee;
            }
        }

        /// <summary>
        /// End the current edit by raising AttributeValueModified with IncrementalUpdate = false.
        /// </summary>
        [ComVisible(false)]
        public static void EndEdit()
        {
            try
            {
                if (_endEditInProgress)
                {
                    return;
                }

                // If attributes have been modified since the last end edit, raise 
                // IncrementalUpdate == false AttributeValueModified events now.
                if (_attributesBeingModified.Count > 0)
                {
                    try
                    {
                        _endEditInProgress = true;

                        foreach (KeyValuePair<IAttribute, KeyValuePair<bool, SpatialString>>
                            modifiedAttribute in _attributesBeingModified)
                        {
                            AttributeStatusInfo statusInfo = GetStatusInfo(modifiedAttribute.Key);
                            statusInfo.OnAttributeValueModified(modifiedAttribute.Key, false, false,
                                modifiedAttribute.Value.Key);
                        }

                        _attributesBeingModified.Clear();
                    }
                    finally
                    {
                        _endEditInProgress = false;
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    // Keep track of all attributes whose values need to be restored due to a failed
                    // update.
                    Dictionary<IDataEntryControl, List<IAttribute>> controlsToRefresh =
                        new Dictionary<IDataEntryControl, List<IAttribute>>();
                    bool refreshSpatialInfo = false;

                    // If there was an exception applying any value, restore the original value for
                    // all modified attributes to prevent exceptions from continuously being
                    // generated.
                    foreach (KeyValuePair<IAttribute, KeyValuePair<bool, SpatialString>> 
                        modifiedAttribute in _attributesBeingModified)
                    {
                        IAttribute attribute = modifiedAttribute.Key;
                        attribute.Value = modifiedAttribute.Value.Value;

                        // Record whether spatial info has changed for any of the attributes.
                        refreshSpatialInfo |= modifiedAttribute.Value.Key;

                        // After setting the value, refresh the value and raise
                        // AttributeValueModified to notify the host of the change.
                        AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(attribute);

                        List<IAttribute> attributeCollection = null;
                        if (!controlsToRefresh.TryGetValue(statusInfo.OwningControl,
                            out attributeCollection))
                        {
                            attributeCollection = new List<IAttribute>();
                            controlsToRefresh[statusInfo.OwningControl] = attributeCollection;
                        }

                        attributeCollection.Add(attribute);
                        statusInfo.OnAttributeValueModified(attribute, false, false, true);
                    }

                    // Refresh all attributes whose values have been restored.
                    foreach (KeyValuePair<IDataEntryControl, List<IAttribute>>
                        controlToRefresh in controlsToRefresh)
                    {
                        controlToRefresh.Key.RefreshAttributes(controlToRefresh.Value.ToArray(),
                            refreshSpatialInfo);
                    }
                }
                catch (Exception ex2)
                {
                    ExtractException.Log("ELI27096", ex2);
                }

                // If an exception occured, clear any pending modifications so they are not applied
                // on a subsequent edit event.
                _attributesBeingModified.Clear();

                throw ExtractException.AsExtractException("ELI26118", ex);
            }  
        }

        /// <summary>
        /// Releases all <see cref="IAttribute"/> COM objects in the supplied vectory by calling
        /// FinalReleaseComObject on each. This needs to be done due to the assignment of
        /// <see cref="AttributeStatusInfo"/> objects as the DataObject member of an attribute.
        /// (If this is not done, handles are leaked).
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> containing the
        /// <see cref="IAttribute"/>s that need to be freed.
        /// </param>
        [ComVisible(false)]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void ReleaseAttributes(IUnknownVector attributes)
        {
            try
            {
                int count = attributes.Size();
                for (int i = 0; i < count; i++)
                {
                    IAttribute attribute = (IAttribute)attributes.At(i);
                    if (attribute.SubAttributes.Size() != 0)
                    {
                        ReleaseAttributes(attribute.SubAttributes);
                    }

                    Marshal.FinalReleaseComObject(attribute);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27846", ex);
            }
        }

        #endregion Static Members

        #region Events

        /// <summary>
        /// An event that indicates an attribute is being initialized into an 
        /// <see cref="IDataEntryControl"/>.
        /// </summary>
        public static event EventHandler<AttributeInitializedEventArgs> AttributeInitialized;

        /// <summary>
        /// Fired to notify listeners that an <see cref="IAttribute"/> that was previously marked 
        /// as unviewed has now been marked as viewed (or vice-versa).
        /// </summary>
        public static event EventHandler<ViewedStateChangedEventArgs> ViewedStateChanged;

        /// <summary>
        /// Fired to notify listeners that an <see cref="IAttribute"/> that was previously marked 
        /// as having invalid data has now been marked as valid (or vice-versa).
        /// </summary>
        public static event EventHandler<ValidationStateChangedEventArgs> ValidationStateChanged;

        /// <summary>
        /// Raised to notify listeners that an Attribute's value was modified.
        /// </summary>
        public event EventHandler<AttributeValueModifiedEventArgs> AttributeValueModified;

        /// <summary>
        /// Raised to notify listeners that an Attribute was deleted.
        /// </summary>
        public event EventHandler<AttributeDeletedEventArgs> AttributeDeleted;

        #endregion Events

        #region Private Methods

        /// <summary>
        /// An <see cref="AccessorMethod"/> implementation used to confirm all attributes
        /// have been viewed (or not viewed)
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> in question.</param>
        /// <param name="statusInfo">The <see cref="AttributeStatusInfo"/> instance containing
        /// the status information for the attribute in question.</param>
        /// <param name="value"><see langword="true"/> to confirm all attributes have been viewed
        /// or <see langword="false"/> to confirm all attributes have not been viewed.</param>
        /// <returns><see langword="true"/> to continue traversing the attribute tree, 
        /// <see langword="false"/> to return <see langword="false"/> from an attribute scan 
        /// without traversing any more attributes.</returns>
        static bool ConfirmDataViewed(IAttribute attribute, AttributeStatusInfo statusInfo,
            bool value)
        {
            return (!statusInfo._isViewable || statusInfo._hasBeenViewed == value);
        }

        /// <summary>
        /// An <see cref="AccessorMethod"/> implementation used to confirm all attributes
        /// have valid data.
        /// <para><b>Note</b></para>
        /// An attribute that does not have an <see cref="OwningControl"/> 
        /// specified will pass this test (return <see langword="true"/>) whether the specfied 
        /// value is <see langword="true"/> or <see langword="false"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> in question.</param>
        /// <param name="statusInfo">The <see cref="AttributeStatusInfo"/> instance containing
        /// the status information for the attribute in question.</param>
        /// <param name="value"><see langword="true"/> to confirm all attributes have valid data
        /// or <see langword="false"/> to confirm all attributes do not have valid data.</param>
        /// <returns><see langword="true"/> to continue traversing the attribute tree, 
        /// <see langword="false"/> to return <see langword="false"/> from an attribute scan 
        /// without traversing any more attributes.</returns>
        static bool ConfirmDataValidity(IAttribute attribute, AttributeStatusInfo statusInfo,
            bool value)
        {
            return (statusInfo._owningControl == null || !statusInfo._isViewable ||
                statusInfo._dataIsValid == value);
        }

        /// <summary>
        /// An <see cref="AccessorMethod"/> implementation used to confirm all attributes
        /// have been propagated into <see cref="IDataEntryControl"/>s.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> in question.</param>
        /// <param name="statusInfo">The <see cref="AttributeStatusInfo"/> instance containing
        /// the status information for the attribute in question.</param>
        /// <param name="value"><see langword="true"/> to confirm all attributes have been propagated
        /// or <see langword="false"/> to confirm all attributes have not been propagated.</param>
        /// <returns><see langword="true"/> to continue traversing the attribute tree, 
        /// <see langword="false"/> to return <see langword="false"/> from an attribute scan 
        /// without traversing any more attributes.</returns>
        static bool ConfirmHasBeenPropagated(IAttribute attribute,
            AttributeStatusInfo statusInfo, bool value)
        {
            return (statusInfo._owningControl == null || statusInfo._hasBeenPropagated == value);
        }

        /// <summary>
        /// An <see cref="AccessorMethod"/> implementation used to check whether attributes are
        /// tabstops or not.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> in question.</param>
        /// <param name="statusInfo">The <see cref="AttributeStatusInfo"/> instance containing
        /// the status information for the attribute in question.</param>
        /// <param name="value"><see langword="true"/> to confirm the attribute tabstop status
        /// matches the provided value, <see langword="false"/> if it does not.</param>
        /// <returns><see langword="true"/> to continue traversing the attribute tree, 
        /// <see langword="false"/> to return <see langword="false"/> from an attribute scan 
        /// without traversing any more attributes.</returns>
        static bool ConfirmIsTabStop(IAttribute attribute, AttributeStatusInfo statusInfo,
            bool value)
        {
            // Default the attribute as not being a tab stop.
            bool isTabStop = false;

            // The attribute can only be a tab stop if it is viewable and mapped to a control.
            if (statusInfo._isViewable && statusInfo._owningControl != null)
            {
                switch (statusInfo.TabStopMode)
                {
                    case TabStopMode.Always:
                        {
                            isTabStop = true;
                        }
                        break;

                    case TabStopMode.OnlyWhenPopulatedOrInvalid:
                        {
                            if (!string.IsNullOrEmpty(attribute.Value.String) ||
                                !statusInfo._dataIsValid)
                            {
                                isTabStop = true;
                            }
                        }
                        break;

                    case TabStopMode.OnlyWhenInvalid:
                        {
                            if (!statusInfo._dataIsValid)
                            {
                                isTabStop = true;
                            }
                        }
                        break;

                    // In the case of TabStopMode.Never, isTabStop will remain false.
                }
            }
            
            return (isTabStop == value);
        }

        /// <summary>
        /// An <see cref="AccessorMethod"/> implementation used to mark all attributes
        /// as propagated or not propagated.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> in question.</param>
        /// <param name="statusInfo">The <see cref="AttributeStatusInfo"/> instance containing
        /// the status information for the attribute in question.</param>
        /// <param name="value"><see langword="true"/> to mark all attributes as propagated,
        /// <see langword="false"/> to mark all attributes as not propagated.</param>
        /// <returns><see langword="true"/> to continue traversing the attribute tree, 
        /// <see langword="false"/> to return <see langword="false"/> from an attribute scan 
        /// without traversing any more attributes.</returns>
        static bool MarkAsPropagated(IAttribute attribute, AttributeStatusInfo statusInfo,
            bool value)
        {
            statusInfo._hasBeenPropagated = value;
            return true;
        }

        /// <summary>
        /// Raises the <see cref="ViewedStateChanged"/> event.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> associated with the event.</param>
        /// <param name="dataIsViewed"><see langword="true"/> if the <see cref="IAttribute"/>'s data
        /// has now been marked as viewed, <see langword="false"/> if it has now been marked as 
        /// unviewed.</param>
        static void OnViewedStateChanged(IAttribute attribute, bool dataIsViewed)
        {
            if (AttributeStatusInfo.ViewedStateChanged != null)
            {
                ViewedStateChanged(null,
                    new ViewedStateChangedEventArgs(attribute, dataIsViewed));
            }
        }

        /// <summary>
        /// Raises the <see cref="ValidationStateChanged"/> event. 
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> associated with the event.</param>
        /// <param name="dataIsValid"><see langword="true"/> if the <see cref="IAttribute"/>'s data
        /// has now been marked as valid, <see langword="false"/> if it has now been marked as 
        /// invalid.</param>
        static void OnValidationStateChanged(IAttribute attribute, bool dataIsValid)
        {
            if (AttributeStatusInfo.ValidationStateChanged != null)
            {
                ValidationStateChanged(null, 
                    new ValidationStateChangedEventArgs(attribute, dataIsValid));
            }
        }

        /// <summary>
        /// Raises the <see cref="AttributeInitialized"/> event.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> being initialized.</param>
        /// <param name="sourceAttributes">The <see cref="IUnknownVector"/> of 
        /// <see cref="IAttribute"/>s from which the attribute is from.</param>
        /// <param name="dataEntryControl">The <see cref="IDataEntryControl"/> that the
        /// <see cref="IAttribute"/> is associated with.</param>
        static void OnAttributeInitialized(IAttribute attribute,
            IUnknownVector sourceAttributes, IDataEntryControl dataEntryControl)
        {
            if (AttributeStatusInfo.AttributeInitialized != null)
            {
                AttributeInitialized(null,
                    new AttributeInitializedEventArgs(attribute, sourceAttributes, dataEntryControl));
            }
        }
        
        /// <summary>
        /// Raises the AttributeValueModified event.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose value was modified.</param>
        /// <param name="incrementalUpdate">see langword="true"/>if the modification is part of an
        /// ongoing edit; <see langword="false"/> if the edit has finished.</param>
        /// <param name="acceptSpatialInfo"><see langword="true"/> if the modification should
        /// trigger the <see cref="IAttribute"/>'s spatial info to be accepted,
        /// <see langword="false"/> if the spatial info acceptance state should be left as is.
        /// </param>
        /// <param name="spatialInfoChanged"><see langword="true"/> if the spatial info for the
        /// <see cref="IAttribute"/> has changed, <see langword="false"/> if only the text has
        /// changed.</param>
        void OnAttributeValueModified(IAttribute attribute, bool incrementalUpdate,
            bool acceptSpatialInfo, bool spatialInfoChanged)
        {
            // Don't raise the event if it is already being raised (prevents recursion).
            if (this.AttributeValueModified != null && !_raisingAttributeValueModified)
            {
                try
                {
                    _raisingAttributeValueModified = true;

                    AttributeValueModified(this,
                        new AttributeValueModifiedEventArgs(
                            attribute, incrementalUpdate, acceptSpatialInfo, spatialInfoChanged));
                }
                finally
                {
                    _raisingAttributeValueModified = false;
                }
            }
        }

        /// <summary>
        /// Raises the AttributeDeleted event.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> that was deleted.</param>
        void OnAttributeDeleted(IAttribute attribute)
        {
            if (this.AttributeDeleted != null)
            {
                AttributeDeleted(this, new AttributeDeletedEventArgs(attribute));
            }
        }

        #endregion Private Methods

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = this.GetType().GUID;
        }

        /// <summary>
        /// Checks if the object for changes since it was last saved.
        /// </summary>
        /// <returns><see langword="true"/> if the object has changes since it was last saved;
        /// <see langword="false"/> otherwise.</returns>
        public int IsDirty()
        {
            return _dirty;
        }

        /// <summary>
        /// Initializes an object from the <see cref="IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    // Read the settings from the memory stream
                    _hasBeenViewed = reader.ReadBoolean();

                    if (reader.Version >= 2)
                    {
                        _isAccepted = reader.ReadBoolean();
                    }

                    if (reader.Version >= 3)
                    {
                        _hintEnabled = reader.ReadBoolean();
                    }
                }

                _dirty = HResult.False;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI24395", 
                    "Error loading AttributeStatusInfo settings.", ex);
            }
        }

        /// <summary>
        /// Saves the <see cref="AttributeStatusInfo"/> object into the specified 
        /// <see cref="IStream"/> and indicates whether the object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langref="true"/>, the flag should be cleared. If 
        /// <see langref="false"/>, the flag should be left unchanged.</param>
        public void Save(IStream stream, bool clearDirty)
        {
            MemoryStream memoryStream = null;

            try
            {
                ExtractException.Assert("ELI24397", "Memory stream is null!", stream != null);

                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    // Save the settings that cannot be reconstructed using the DataEntryControlHost
                    writer.Write(_hasBeenViewed);
                    writer.Write(_isAccepted);
                    writer.Write(_hintEnabled);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = HResult.False;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI24398", 
                    "Error saving data entry application settings.", ex);
            }
            finally
            {
                if (memoryStream != null)
                {
                    memoryStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// <para>NOTE: Not implemented.</para>
        /// </summary>
        /// <param name="size">Will always be <see cref="HResult.NotImplemented"/> to indicate this
        /// method is not implemented.
        /// </param>
        public void GetSizeMax(out long size)
        {
            size = HResult.NotImplemented;
        }

        #endregion IPersistStream Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the current <see cref="AttributeStatusInfo"/> instance.
        /// </summary>
        /// <returns>A copy of the current <see cref="AttributeStatusInfo"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                AttributeStatusInfo clone = new AttributeStatusInfo();

                clone.CopyFrom(this);

                return clone;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI24909", "Clone failed.", ex);
            }
        }

        /// <summary>
        /// Copies the value of the provided <see cref="AttributeStatusInfo"/> instance into the 
        /// current one.
        /// </summary>
        /// <param name="pObject">The object to copy from.</param>
        /// <exception cref="ExtractException">If the supplied object is not of type
        /// <see cref="AttributeStatusInfo"/>.</exception>
        public void CopyFrom(object pObject)
        {
            try
            {
                ExtractException.Assert("ELI24983", "Cannot copy from an object of a different type!",
                    pObject.GetType() == this.GetType());

                AttributeStatusInfo source = (AttributeStatusInfo)pObject;

                // Copy fields from source
                _hasBeenPropagated = source._hasBeenPropagated;
                _isViewable = source._isViewable;
                _displayOrder = source._displayOrder;
                _hasBeenViewed = source._hasBeenViewed;
                _dataIsValid = source._dataIsValid;
                _owningControl = source._owningControl;
                _hintType = source._hintType;
                _hintRasterZones = source._hintRasterZones;
                _isAccepted = source._isAccepted;
                _hintEnabled = source._hintEnabled;
                _parentAttribute = source._parentAttribute;
                _fullPath = source._fullPath;
                _tabStopMode = source._tabStopMode;
                _persistAttribute = source._persistAttribute;
                
                // _raisingAttributeValueModified is intentionally not copied.
                // _validator cannot be copied; Each control instance must supply its own.
                // Do not copy _autoUpdateQuery or _validationQuery since a query is specified to
                // its location in the attribute hierarchy and persisting the query values will
                // prevent new triggers from being created when the attribute is re-initialized.
                _raisingAttributeValueModified = false;
                _validator = null;
                _autoUpdateQuery = null;
                _validationQuery = null;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI24911", 
                    "Unable to copy AttributeStatusInfo", ex);
            }
        }

        #endregion ICopyableObject Members
    }
}
