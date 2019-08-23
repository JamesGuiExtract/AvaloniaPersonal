using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DataEntry
{
    public partial class AttributeStatusInfo
    {
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
        /// <typeparam name="T">The data type of value accessed by <see cref="AccessorMethod"/></typeparam>
        class AttributeScanner<T>
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
            public AccessorMethod<T> _accessorMethod;

            /// <summary>
            /// Either specifies the value an <see cref="AttributeStatusInfo"/> field should be set
            /// to or specifies the value a field is required to be.
            /// </summary>
            public T _value;

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
            AttributeScanner(IUnknownVector attributes, AccessorMethod<T> accessorMethod,
                T value, bool forward, Stack<IAttribute> resultAttributeGenealogy)
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
                Stack<IAttribute> startingPoint, AccessorMethod<T> accessorMethod, T value,
                bool forward, bool loop, Stack<IAttribute> resultAttributeGenealogy)
            {
                try
                {
                    // Validate the license
                    LicenseUtilities.ValidateLicense(
                        LicenseIdName.DataEntryCoreComponents, "ELI26132", _OBJECT_NAME);

                    // Create a node in charge of scanning the root-level attributes.
                    AttributeScanner<T> rootScanNode = new AttributeScanner<T>(attributes, accessorMethod,
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
            AttributeScanner<T> GetScanNode(IUnknownVector attributes, 
                Stack<IAttribute> startingPoint, bool firstPass)
            {
                // Create and initialize the child AttributeScanner instance.
                AttributeScanner<T> childStatusinfo = new AttributeScanner<T>(attributes, _accessorMethod,
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
                    bool atStartIndex = false;

                    // If this is the starting attribute apply the attribute's display order as the
                    // cutoff value.
                    if (i == _startAttributeIndex)
                    {
                        currentCutoffDisplayOrder = statusInfo.DisplayOrder;
                        appliedCutoffDisplayOrder = currentCutoffDisplayOrder;
                        atStartIndex = true;
                    }
                    // If the starting attribute is not a decendent of this specific attribute, ignore
                    // the specified starting point.
                    else if (i != _startIndex)
                    {
                        _startingPoint = null;
                    }

                    AttributeScanner<T> childScanNode = null;

                    // [DataEntry:1106]
                    // Iff:
                    // - Scanning forward or this isn't the starting point.
                    // - This attribute has sub-attributes, perform a scan of the sub-attributes
                    if ((_forward || !atStartIndex) && attribute.SubAttributes.Size() > 0)
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
        /// <typeparam name="T">The data type of value accessed.</typeparam>
        delegate bool AccessorMethod<T>(IAttribute attribute, AttributeStatusInfo statusInfo,
            T value);

        #endregion Delegates

        #region AssessorMethods

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
        /// An <see cref="AccessorMethod"/> implementation to test that an attribute's
        /// data validity is not included in <see paramref="targetValidity"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> in question.</param>
        /// <param name="statusInfo">The <see cref="AttributeStatusInfo"/> instance containing
        /// the status information for the attribute in question.</param>
        /// <param name="targetValidity">The <see cref="DataValidity"/> values that should
        /// trigger the scan to stop on a particular attribute.</param>
        /// <returns><see langword="true"/> to continue traversing the attribute tree, 
        /// <see langword="false"/> to return <see langword="false"/> from an attribute scan 
        /// without traversing any more attributes.</returns>
        static bool DataValidityDoesNotMatch(IAttribute attribute,
            AttributeStatusInfo statusInfo, DataValidity targetValidity)
        {
            return (!statusInfo._isViewable || (targetValidity & statusInfo._dataValidity) == 0);
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
                                (statusInfo._dataValidity != DataValidity.Valid))
                            {
                                isTabStop = true;
                            }
                        }
                        break;

                    case TabStopMode.OnlyWhenInvalid:
                        {
                            if (statusInfo._dataValidity != DataValidity.Valid)
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
        /// An <see cref="AccessorMethod"/> implementation used to check whether the specified
        /// <see cref="IAttribute"/> represents a tab stop or group.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> in question.</param>
        /// <param name="statusInfo">The <see cref="AttributeStatusInfo"/> instance containing
        /// the status information for the attribute in question.</param>
        /// <param name="value"><see langword="true"/> to confirm the attribute tabstop status
        /// matches the provided value, <see langword="false"/> if it does not.</param>
        /// <returns><see langword="true"/> to continue traversing the attribute tree, 
        /// <see langword="false"/> to return <see langword="false"/> from an attribute scan 
        /// without traversing any more attributes.</returns>
        static bool ConfirmIsTabStopOrGroup(IAttribute attribute, AttributeStatusInfo statusInfo,
            bool value)
        {
            // Default the attribute as not being a tab stop or group.
            bool isTabStopOrGroup = false;

            if (ConfirmIsTabStop(attribute, statusInfo, true))
            {
                isTabStopOrGroup = true;
            }
            else if (ConfirmIsTabGroup(attribute, statusInfo, true))
            {
                isTabStopOrGroup = true;
            }

            return (isTabStopOrGroup == value);
        }

        /// <summary>
        /// An <see cref="AccessorMethod"/> implementation used to check whether the specified
        /// <see cref="IAttribute"/> represents a tab group.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> in question.</param>
        /// <param name="statusInfo">The <see cref="AttributeStatusInfo"/> instance containing
        /// the status information for the attribute in question.</param>
        /// <param name="value"><see langword="true"/> to confirm the attribute tab group status
        /// matches the provided value or if the attribute is a tab stop in a control that does not
        /// support tab groups, <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> to continue traversing the attribute tree, 
        /// <see langword="false"/> to return <see langword="false"/> from an attribute scan 
        /// without traversing any more attributes.</returns>
        static bool ConfirmIsTabGroup(IAttribute attribute, AttributeStatusInfo statusInfo,
            bool value)
        {
            bool isTabGroup;

            // If _tabGroup is null, the control does not support tab groups and for the purposes
            // of this method should be considered a tab group if it is a tab stop.
            if (statusInfo._tabGroup == null)
            {
                isTabGroup = ConfirmIsTabStop(attribute, statusInfo, true);
            }
            else
            {
                isTabGroup = (statusInfo._tabGroup.Count > 0);
            }

            return (isTabGroup == value);
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

        #endregion AssessorMethods
    }
}
