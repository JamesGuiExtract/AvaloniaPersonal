using Extract;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    #region Enums

    /// <summary>
    /// Used in cases where multiple <see cref="IAttribute"/>s are provided to
    /// a <see cref="IDataEntryControl"/> to determine which 
    /// <see cref="IAttribute"/>(s), if any, should be mapped to the control.
    /// </summary>
    public enum MultipleMatchSelectionMode
    {
        /// <summary>
        /// The first <see cref="IAttribute"/> in the vector should be used.
        /// </summary>
        First,

        /// <summary>
        /// The last <see cref="IAttribute"/> in the vector should be used.
        /// </summary>
        Last,

        /// <summary>
        /// All provided <see cref="IAttribute"/>s should be used.
        /// </summary>
        All,

        /// <summary>
        /// None of the <see cref="IAttribute"/>s should be used.
        /// </summary>
        None
    }

    #endregion Enums
 
    /// <summary>
    /// Static utility functions for classes in the <see cref="Extract.DataEntry"/> namespace.
    /// </summary>
    public static class DataEntryMethods
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DataEntryMethods).ToString();

        /// <summary>
        /// [DataEntry:273]
        /// A representation of the carriage return line feed combination so that the unprintable
        /// boxes don't appear in the control.
        /// </summary>
        internal static readonly string _CRLF_REPLACEMENT = "\xA0";

        /// <summary>
        /// A tag to represent the Program Files\Extract Systems directory
        /// </summary>
        internal static readonly string _PFES_FOLDER = "[PFESFolder]";

        #endregion Constants

        #region Fields

        /// <summary>
        /// A shared <see cref="IAFUtility"/> instance to be used within 
        /// <see cref="Extract.DataEntry"/>.
        /// </summary>
        private static IAFUtility _afUtility;

        /// <summary>
        /// The location to use as the root directory when resolving relative paths.
        /// </summary>
        private static string _solutionRootDirectory =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        #endregion Fields

        /// <summary>
        /// Gets or sets the location the DataEntry framework should use as the root directory
        /// when resolving relative paths.
        /// </summary>
        /// <value>The location to use as the root directory when resolving relative paths.</value>
        /// <returns>The location to use as the root directory when resolving relative paths.</returns>
        // No need for try catch, just sets a string value
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public static string SolutionRootDirectory
        {
            get
            {
                return _solutionRootDirectory;
            }

            set
            {
                if (!Directory.Exists(value))
                {
                    ExtractException ee = new ExtractException("ELI25429",
                        "Invalid SolutionRootDirectory specified!");
                    ee.AddDebugData("SolutionRootDirectory", value, false);
                    throw ee;
                }

                _solutionRootDirectory = value;
            }
        }

        /// <summary>
        /// A shared <see cref="IAFUtility"/> instance to be used within 
        /// <see cref="Extract.DataEntry"/>.
        /// <para><b>Requirements:</b></para>
        /// This property must be set to <see langword="null"/> between form sessions; accessing
        /// it from a different thread that it was created will throw an exception assuming the
        /// primary form is running in a single threaded apartment.
        /// </summary>
        /// <value>A shared <see cref="IAFUtility"/> instance to be used within 
        /// <see cref="Extract.DataEntry"/>.</value>
        /// <returns>A shared <see cref="IAFUtility"/> instance to be used within 
        /// <see cref="Extract.DataEntry"/>.</returns>
        public static IAFUtility AFUtility
        {
            get
            {
                try
                {
                    if (_afUtility == null)
                    {
                        _afUtility = (IAFUtility)new AFUtilityClass();
                    }

                    return _afUtility;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25614", ex);
                }
            }

            set
            {
                _afUtility = value;
            }
        }

        #region Static Methods

        /// <summary>
        /// Attempts to obtain a single <see cref="IAttribute"/> from the provided
        /// <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s based on the provided 
        /// attribute name and <see cref="MultipleMatchSelectionMode"/>. Removes from the source 
        /// attribute vector any matching attributes that the control is not displaying. Provides 
        /// a vector of attributes that were removed from sourceAttributes.
        /// </summary>
        /// <param name="attributeName">The name any resulting <see cref="IAttribute"/> must have.
        /// </param>
        /// <param name="selectionMode">If more than one <see cref="IAttribute"/> is found that 
        /// matches the provided attribute name, this parameter will determine which of the matches 
        /// (if any) to use.</param>
        /// <param name="createIfNotFound"><see langref="true"/> if a new attribute should be 
        /// created when a matching attribute is not found, <see langref="false"/> to return 
        /// <see langref="null"/>.</param>
        /// <param name="sourceAttributes">The <see cref="IUnknownVector"/> instance of 
        /// <see cref="IAttribute"/>s from which to find the <see cref="IAttribute"/>.</param>
        /// <param name="removedMatches">An <see cref="IUnknownVector"/> of 
        /// <see cref="IAttribute"/>s to be populated with the the set of attributes removed
        /// from sourceAttributes. (These are attributes that will have matched the specified
        /// attribute name, but are not kept due to the selectionMode parameter). Can be 
        /// <see langword="null"/> if the caller does not care to know which 
        /// <see cref="IAttribute"/>s were removed.</param>
        /// <param name="owningControl">The <see cref="IDataEntryControl"/> that will display this
        /// <see cref="IAttribute"/>.</param>
        /// <param name="displayOrder">An <see langword="integer"/> indicating the order in which
        /// the initialized <see cref="IAttribute"/> should be viewed in the 
        /// <see cref="DataEntryControlHost"/> relative to other <see cref="IAttribute"/>s in the
        /// same control. (attributes assigned lower numbers are to be viewed before attributes 
        /// assigned higher numbers).  Can be <see langword="null"/> if the no display order should 
        /// be applied. (any existing display order will be kept)</param>
        /// <param name="considerPropagated"><see langword="true"/> to consider the 
        /// <see cref="IAttribute"/> already propagated; <see langword="false"/> otherwise.</param>
        /// <param name="validatorTemplate">A template to be used as the master for any per-attribute
        /// <see cref="IDataEntryValidator"/> created to validate the attribute's data.
        /// Can be <see langword="null"/> to keep the existing validator or if data validation is
        /// not required.</param>
        /// <param name="tabStopMode">A <see cref="TabStopMode"/> under what circumstances the
        /// attribute should serve as a tab stop (<see langword="null"/> to keep any existing
        /// tabStopMode setting).</param>
        /// <param name="autoUpdateQuery">A query which will cause an attribute's value to
        /// automatically be updated using values from other <see cref="IAttribute"/>'s and/or a
        /// database query.</param>
        /// <param name="validationQuery">A query which will cause the validation list for the 
        /// validator associated with the attribute to be updated using values from other
        /// <see cref="IAttribute"/>'s and/or a database query.</param>
        /// <returns>An <see cref="IAttribute"/> instance if a match was found that satisfies the 
        /// provided parameters.  <see langword="null"/> if such an <see cref="IAttribute"/> cannot 
        /// be found or multiple instances are found.</returns>
        internal static IAttribute InitializeAttribute(string attributeName,
            MultipleMatchSelectionMode selectionMode, bool createIfNotFound,
            IUnknownVector sourceAttributes, IUnknownVector removedMatches, 
            IDataEntryControl owningControl, int? displayOrder, bool considerPropagated,
            TabStopMode? tabStopMode, IDataEntryValidator validatorTemplate, string autoUpdateQuery,
            string validationQuery)
        {
            try
            {
                ExtractException.Assert("ELI23693",
                    "MultipleMatchSelectionMode.All is not valid for selecting one attribute!",
                            selectionMode != MultipleMatchSelectionMode.All);

                // Retrieve a vector of attributes satisfying the provided parameters.
                IUnknownVector attributes = DataEntryMethods.InitializeAttributes(attributeName,
                    selectionMode, sourceAttributes, removedMatches, owningControl, displayOrder,
                    considerPropagated, tabStopMode, validatorTemplate, autoUpdateQuery,
                    validationQuery);

                ExtractException.Assert("ELI23694", "Expected one attribute, but got many!",
                    attributes.Size() <= 1);

                if (attributes.Size() == 1)
                {
                    // If the vector contains a single attribute, the return value from this method is
                    // meaningful. Return the one and only member.
                    return (IAttribute) attributes.Front();
                }
                else if (createIfNotFound)
                {
                    ExtractException.Assert("ELI23918", 
                        "Cannot create an attribute; name is not specified!", 
                        !string.IsNullOrEmpty(attributeName));

                    // If requested, create a new attribute using the specified attribute name.
                    IAttribute attribute = (IAttribute) new AttributeClass();
                    attribute.Name = attributeName;

                    AttributeStatusInfo.Initialize(attribute, sourceAttributes, owningControl,
                        displayOrder, considerPropagated, tabStopMode, validatorTemplate,
                        autoUpdateQuery, validationQuery);

                    // Add the created attributes to the sourceAttribute vector so that it is included
                    // in any output (and ordered according to displayOrder).
                    if (sourceAttributes != null)
                    {
                        ReorderAttributes(sourceAttributes, AttributeAsVector(attribute));
                    }

                    return attribute;
                }
                else
                {
                    // Either no matching attributes were found, or multiple matching attributes
                    // were found
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23972", ex);
            }
        }

        /// <summary>
        /// Attempts to obtain an <see cref="IUnknownVector"/> of 
        /// <see cref="IAttribute"/>s from the provided <see cref="IUnknownVector"/> 
        /// based on the provided attribute name and <see cref="MultipleMatchSelectionMode"/>. 
        /// Removes from the source attribute vector any matching attributes that the control 
        /// is not displaying.
        /// </summary>
        /// <param name="attributeName">The name any resulting <see cref="IAttribute"/> must have.
        /// </param>
        /// <param name="selectionMode">If more than one <see cref="IAttribute"/>
        /// is found that matches the provided attribute name, this parameter will determine which 
        /// of the matches (if any) to return.</param>
        /// <param name="sourceAttributes">The <see cref="IUnknownVector"/> instance of 
        /// <see cref="IAttribute"/>s from which to find the <see cref="IAttribute"/>.</param>
        /// <param name="removedMatches">An <see cref="IUnknownVector"/> of 
        /// <see cref="IAttribute"/>s to be populated with the the set of attributes removed
        /// from sourceAttributes. (These are attributes that will have matched the provided 
        /// attribute name, but are not kept due to the selectionMode parameter). Can be 
        /// <see langword="null"/> if the caller does not care to know which 
        /// <see cref="IAttribute"/>s were removed.
        /// </param>
        /// <param name="owningControl">The <see cref="IDataEntryControl"/> that will display this
        /// <see cref="IAttribute"/>.</param>
        /// <param name="displayOrder">An <see langword="integer"/> indicating the order in which
        /// the initialized <see cref="IAttribute"/>s should be viewed in the 
        /// <see cref="DataEntryControlHost"/> relative to their sibling <see cref="IAttribute"/>s.
        /// (attributes assigned lower numbers are to be viewed before attributes assigned higher
        /// numbers). Can be <see langword="null"/> if the no display order should 
        /// be applied. (any existing display order will be kept)</param>
        /// <param name="considerPropagated"><see langword="true"/> to consider the 
        /// <see cref="IAttribute"/> already propagated; <see langword="false"/> otherwise.</param>
        /// <param name="validatorTemplate">A template to be used as the master for any per-attribute
        /// <see cref="IDataEntryValidator"/> created to validate the attribute's data.
        /// Can be <see langword="null"/> to keep the existing validator or if data validation is
        /// not required.</param>
        /// <param name="tabStopMode">A <see cref="TabStopMode"/> under what circumstances the
        /// attributes should serve as a tab stop (<see langword="null"/> to keep any existing
        /// tabStopMode setting).</param>
        /// <param name="autoUpdateQuery">A query which will cause a contained cell's value to
        /// automatically be updated using values from other <see cref="IAttribute"/>'s and/or a
        /// database query.</param>
        /// <param name="validationQuery">A query which will cause the validation list for the 
        /// validator associated with the attribute to be updated using values from other
        /// <see cref="IAttribute"/>'s and/or a database query.</param>
        /// <returns>An <see cref="IUnknownVector"/> instance of <see cref="IAttribute"/>s 
        /// that satisfy the provided parameters. The vector will be empty if no such 
        /// <see cref="IAttribute"/> can be found.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        internal static IUnknownVector InitializeAttributes(string attributeName,
            MultipleMatchSelectionMode selectionMode, IUnknownVector sourceAttributes,
            IUnknownVector removedMatches, IDataEntryControl owningControl,
            int? displayOrder, bool considerPropagated, TabStopMode? tabStopMode,
            IDataEntryValidator validatorTemplate, string autoUpdateQuery, string validationQuery)
        {
            try
            {
                IUnknownVector attributes;

                if (sourceAttributes != null)
                {
                    // Use AFUtility to query for matching attributes
                    attributes = AFUtility.QueryAttributes(sourceAttributes, attributeName, false);
                }
                else
                {
                    attributes = (IUnknownVector) new IUnknownVectorClass();
                }

                // If an existing vector was not supplied to receive the set of attributes removed
                // from sourceAttributes, we need to create a local instance for processing.
                if (removedMatches == null)
                {
                    removedMatches = (IUnknownVector)new IUnknownVectorClass();
                }

                int attributeCount = attributes.Size();
                if (attributeCount > 1)
                {
                    // If multiple attributes were found, remove attributes from the vector as dictated
                    // by selectionMode

                    if (selectionMode == MultipleMatchSelectionMode.First)
                    {
                        // Remove all but the first attribute.
                        removedMatches.Append(attributes);
                        removedMatches.Remove(0);
                        attributes.RemoveRange(1, attributeCount - 1);
                    }
                    else if (selectionMode == MultipleMatchSelectionMode.Last)
                    {
                        // Remove all but the last attribute.
                        removedMatches.Append(attributes);
                        removedMatches.PopBack();
                        attributes.RemoveRange(0, attributeCount - 2);
                    }
                    else if (selectionMode == MultipleMatchSelectionMode.None)
                    {
                        // Remove all attributes leaving an empty vector
                        removedMatches.Append(attributes);
                        attributes.Clear();
                    }
                }

                // Use the attributes in removedMatches to modify the sourceAttributes appropriately.
                int removedCount = removedMatches.Size();

                ExtractException.Assert("ELI24009", "Internal Logic Error!",
                    removedCount == 0 || sourceAttributes != null);
               
                for (int i = 0; i < removedCount; i++)
                {
                    // [DataEntry:693]
                    // Since these attributes will no longer be accessed by the DataEntry, they need
                    // to be released with FinalReleaseComObject to prevent handle leaks.
                    // Call DeleteAttribute rather than ReleaseAttribute so proper events are raised to
                    // remove references to the attribute from the DatEntry framework.
                    AttributeStatusInfo.DeleteAttribute((IAttribute)removedMatches.At(i));
                }

                attributeCount = attributes.Size();
                for (int i = 0; i < attributeCount; i++)
                {
                    IAttribute attribute = (IAttribute)attributes.At(i);
                    ExtractException.Assert("ELI24453", "Missing attribute data!", attribute != null);

                    // If the attribute did not already have the status info provided, trigger the 
                    // attributes in sourceAttributes to be reordered according to displayOrder.
                    AttributeStatusInfo.Initialize(attribute, sourceAttributes,
                        owningControl, displayOrder, considerPropagated, tabStopMode,
                        validatorTemplate, autoUpdateQuery, validationQuery);
                }

                return attributes;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23971", ex);
            }
        }

        /// <summary>
        /// Inserts into the provided <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s the
        /// specified new attribute. If an old <see cref="IAttribute"/> specified, the new 
        /// <see cref="IAttribute"/> will replace the old one (at the same position in the vector)
        /// except if insertBeforeAttribute is specified in which case the new attribute will always
        /// be positioned there.
        /// </summary>
        /// <param name="attributeVector">The <see cref="IUnknownVector"/> to which the
        /// <see cref="IAttribute"/> will be added.</param>
        /// <param name="newAttribute">The <see cref="IAttribute"/> to add to the vector.</param>
        /// <param name="oldAttribute">If not <see langword="null"/>, this attribute will be replaced
        /// by <see paramref="newAttribute"/>.</param>
        /// <param name="insertBeforeAttribute">If not <see langword="null"/>, the new 
        /// <see paramref="newAttribute"/> will be inserted before this <see cref="IAttribute"/>.
        /// </param>
        /// <returns><see langword="true"/> If <see paramref="oldAttribute"/> was specified and
        /// replaced or <see langword="false"/> if <see paramref="newAttribute"/> was added without
        /// replacing any existing <see cref="IAttribute"/> in the vector.</returns>
        internal static bool InsertOrReplaceAttribute(IUnknownVector attributeVector, 
            IAttribute newAttribute, IAttribute oldAttribute, IAttribute insertBeforeAttribute)
        {
            try
            {
                ExtractException.Assert("ELI25434", "Null argument exception!", newAttribute != null);
                ExtractException.Assert("ELI25435", "Null argument exception!", attributeVector != null);

                bool oldAttributeRemoved = false;
                int index = -1;

                // If oldAttribute was specified, attempt to find its location in the the vector and,
                // if found, remove it.
                if (oldAttribute != null)
                {
                    attributeVector.FindByReference(oldAttribute, 0, ref index);
                    if (index != -1)
                    {
                        // Remove the old attribute from the vector.
                        // Use AttributeStatusInfo to delete it so the data entry framework is
                        // notified of its deletion.
                        AttributeStatusInfo.DeleteAttribute(oldAttribute);
                        oldAttributeRemoved = true;
                    }
                }

                // Find the position of the insertBeforeAttribute in the attributeVector (if 
                // specified) so that the new attribute can be added at the that position in the
                // vector.
                if (insertBeforeAttribute != null && insertBeforeAttribute != oldAttribute)
                {
                    attributeVector.FindByReference(insertBeforeAttribute, 0, ref index);
                }

                // Add the attribute to attributeVector (at the specified index, if provided)
                if (index >= 0)
                {
                    // Remove the attribute being added from the vector if it is not at the index
                    // were we are trying to add it.
                    int currentIndex = -1;
                    if (attributeVector.Size() > 0)
                    {
                        attributeVector.FindByReference(newAttribute, 0, ref currentIndex);
                        
                        if (currentIndex == index)
                        {
                            // If the attribute already exists at the targe location, there
                            // is nothing to do.
                            currentIndex = -1;
                            index = -1;
                        }
                        else if (currentIndex >= 0)
                        {
                            // If the attribute exists at a another location in the vector, remove
                            // it from there.
                            attributeVector.Remove(currentIndex);

                            // If the attribute was removed from a previous position in the vector
                            // adjust the target index accordingly.
                            if (currentIndex < index)
                            {
                                index--;
                            }
                        }
                    }

                    // As long as the attribute doesn't already exist at this index, insert it.
                    if (index >= 0)
                    {
                        attributeVector.Insert(index, newAttribute);
                    }
                }
                else
                {
                    attributeVector.PushBackIfNotContained(newAttribute);
                }

                // The attribute must be re-initialized to update the position of the attribute
                // in the overall attribute hierarchy.
                IDataEntryControl owningControl = AttributeStatusInfo.GetOwningControl(newAttribute);
                AttributeStatusInfo.Initialize(newAttribute, attributeVector, owningControl);

                return oldAttributeRemoved;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25433", ex);
            }
        }

        /// <summary>
        /// Returns an <see cref="IUnknownVector"/> populated only with the provided
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> that should be used to populate the 
        /// return vector.</param>
        /// <returns>An <see cref="IUnknownVector"/> instance containing the provided
        /// <see cref="IAttribute"/>.</returns>
        internal static IUnknownVector AttributeAsVector(IAttribute attribute)
        {
            try
            {
                IUnknownVector vector = (IUnknownVector) new IUnknownVectorClass();

                if (attribute != null)
                {
                    vector.PushBack(attribute);
                }

                return vector;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23974", ex);
            }
        }

        /// <summary>
        /// Converts the provided <see cref="IEnumerable{T}"/> of <see cref="IAttribute"/>s to an
        /// <see cref="IEnumerable{T}"/> that optionally includes all sub-attributes as well.
        /// </summary>
        /// <param name="attributes">The source<see cref="IEnumerable{T}"/> of
        /// <see cref="IAttribute"/>s.</param>
        /// <param name="includeSubAttributes"><see langword="true"/> to include all sub-attributes
        /// of the provided attributes, <see langword="false"/> to return the source enumeration.
        /// If included, sub-attributes will occur earlier in the enumeration that their parents.
        /// </param>
        /// <returns>The resulting <see cref="IEnumerable{T}"/> of <see cref="IAttribute"/>s.</returns>
        static internal IEnumerable<IAttribute> ToAttributeEnumerable(IEnumerable<IAttribute> attributes,
            bool includeSubAttributes)
        {
            foreach (IAttribute attribute in attributes)
            {
                if (includeSubAttributes)
                {
                    foreach (IAttribute subAttribute in
                        ToAttributeEnumerable(attribute.SubAttributes, true))
                    {
                        yield return subAttribute;
                    }
                }

                yield return attribute;
            }
        }

        /// <summary>
        /// Converts the provided <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s to an
        /// <see cref="IEnumerable{T}"/> that optionally includes all sub-attributes as well.
        /// </summary>
        /// <param name="attributes">The source <see cref="IUnknownVector"/> of
        /// <see cref="IAttribute"/>s.</param>
        /// <param name="includeSubAttributes"><see langword="true"/> to include all sub-attributes
        /// of the provided attributes, <see langword="false"/> to return the source enumeration.
        /// If included, sub-attributes will occur earlier in the enumeration that their parents.
        /// </param>
        /// <returns>The resulting <see cref="IEnumerable{T}"/> of <see cref="IAttribute"/>s.</returns>
        static internal IEnumerable<IAttribute> ToAttributeEnumerable(IUnknownVector attributes,
            bool includeSubAttributes)
        {
            ExtractException.Assert("ELI25170", "Null argument exception!", attributes != null);

            // Loop through each attribute, but return sub-attributes first, if requested.
            int attributeCount = attributes.Size();
            for (int i = 0; i < attributeCount; i++)
            {
                // Can be null, so don't use explicit cast.
                IAttribute attribute = attributes.At(i) as IAttribute;

                if (attribute == null)
                {
                    continue;
                }

                // If the supplied attribute is not null and we are including sub-attributes, 
                // collect the raster zones from this attribute's children.
                if (includeSubAttributes)
                {
                    foreach (IAttribute subAttribute in
                        ToAttributeEnumerable(attribute.SubAttributes, true))
                    {
                        yield return subAttribute;
                    }
                }

                yield return attribute;
            }
        }

        /// <overloads>Processes the supplied text with the supplied formatting rule.</overloads>
        /// <summary>
        /// Creates an <see cref="IAttribute"/> using the supplied text and formatting rule.
        /// </summary>
        /// <param name="rule">The <see cref="IRuleSet"/> to use to process the supplied text.
        /// Must not be <see langword="null"/>.
        /// </param>
        /// <param name="inputText">The <see cref="SpatialString"/> to process with the supplied
        /// rule.</param>
        /// <param name="attributeName">The name of the attribute for which the rule should be run.
        /// Can be <see langword="null"/> to choose from <see cref="IAttribute"/>s of all names.
        /// </param>
        /// <param name="selectionMode">The <see cref="MultipleMatchSelectionMode"/> used to choose
        /// from multiple results. <see cref="MultipleMatchSelectionMode.All"/> is not a valid
        /// option for this call.</param>
        /// <returns>An single <see cref="IAttribute"/> found using the formatting rule or
        /// <see langword="null"/> if no <see cref="IAttribute"/> meeting the qualifications was
        /// found.
        /// </returns>
        internal static IAttribute RunFormattingRule(IRuleSet rule, SpatialString inputText,
            string attributeName, MultipleMatchSelectionMode selectionMode)
        {
            try
            {
                ExtractException.Assert("ELI26765", "Rule was not specified!", rule != null);
                ExtractException.Assert("ELI26766", "Invalid selection mode!",
                    selectionMode != MultipleMatchSelectionMode.All);

                IUnknownVector results = RunFormattingRule(rule, inputText, attributeName);

                // Choose the appropriate resulting attribute based on selectionMode
                int resultsCount = results.Size();
                if (resultsCount > 0)
                {
                    if (resultsCount == 1 || selectionMode == MultipleMatchSelectionMode.First)
                    {
                        return (IAttribute)results.At(0);
                    }
                    else if (selectionMode == MultipleMatchSelectionMode.Last)
                    {
                        return (IAttribute)results.At(resultsCount - 1);
                    }
                }

                // No attribute could be found using the supplied parameters.
                return null;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26767", ex);
            }
        }

        /// <summary>
        /// Processes the supplied text with the supplied formatting rule.
        /// </summary>
        /// <param name="rule">The <see cref="IRuleSet"/> to use to process the supplied text.
        /// Must not be <see langword="null"/>.
        /// </param>
        /// <param name="inputText">The <see cref="SpatialString"/> to process with the supplied
        /// rule.</param>
        /// <param name="attributeName">The name of the attribute for which the rule should be run.
        /// Can be <see langword="null"/> to choose from <see cref="IAttribute"/>s of all names.
        /// </param>
        /// <returns>The result of running the formatting rule on the supplied text. 
        /// </returns>
        internal static IUnknownVector RunFormattingRule(IRuleSet rule, SpatialString inputText,
            string attributeName)
        {
            try
            {
                ExtractException.Assert("ELI24322", "Rule was not specified!", rule != null);

                // If a formatting rule is specified, use the text to create document a document
                // to be processed by the rule.
                AFDocumentClass afDoc = new AFDocumentClass();
                afDoc.Text = inputText;

                // Prepare a variant vector to specify the desired attribute name.
                VariantVector attributeNames = null;
                if (!string.IsNullOrEmpty(attributeName))
                {
                    attributeNames = (VariantVector)new VariantVectorClass();
                    attributeNames.PushBack(attributeName);
                }

                // Format the data into attribute(s) using the rule.
                return rule.ExecuteRulesOnText(afDoc, attributeNames, null);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24310", ex);
            }
        }

        /// <summary>
        /// Inserts into the selected portion of the existing <see cref="SpatialString"/> the
        /// specified provided <see cref="SpatialString"/>.
        /// </summary>
        /// <param name="textBoxControl">A <see cref="TextBoxBase"/> whose current selection
        /// indicates which part of the existing text is to be replaced.</param>
        /// <param name="existingText">The existing <see cref="SpatialString"/> associated with 
        /// <see paramref="textBoxControl"/>.
        /// <para><b>Note</b></para>
        /// This value will be modified (it is the same value that is returned).</param>
        /// <param name="newText">The <see cref="SpatialString"/> that is to replace the current
        /// selection.</param>
        /// <returns></returns>
        internal static SpatialString InsertSpatialStringIntoSelection(TextBoxBase textBoxControl,
            SpatialString existingText, SpatialString newText)
        {
            try
            {
                // If the current selection doesn't include all text, combine the existing
                // text with the swiped text.
                if (textBoxControl.SelectionLength != textBoxControl.Text.Length)
                {
                    // So that SpatialString::Insert/Append works properly ensure the source doc
                    // names are the same.
                    existingText.SourceDocName = newText.SourceDocName;

                    // If both SpatialStrings are spatial we need to normalize them before they can
                    // be combined.
                    if (existingText.HasSpatialInfo() && newText.HasSpatialInfo())
                    {
                        // TODO: (DataEntry:909)
                        // SpatialString::MergeAsHybridString could be now be used (it was based on
                        // the below code).

                        // [DataEntry:831] 
                        // Simply appending true spatial info will cause unexpected results if the text
                        // does not fall on the same line. When combining different spatial string
                        // values, convert each to a hybrid and use the hybrid raster zones to
                        // create a hybrid result.
                        existingText.DowngradeToHybridMode();
                        newText.DowngradeToHybridMode();

                        // A unified spatial page infos needs to be created. Start with newText's
                        // spatial page infos, and replace any shared pages with existingText's
                        // spatial page infos.
                        ICopyableObject copyThis = (ICopyableObject)newText.SpatialPageInfos;
                        LongToObjectMap unifiedSpatialPageInfos = (LongToObjectMap)copyThis.Clone();

                        VariantVector existingSpatialPages = existingText.SpatialPageInfos.GetKeys();
                        int count = existingSpatialPages.Size;
                        for (int i = 0; i < count; i++)
                        {
                            int page = (int)existingSpatialPages[i];
                            unifiedSpatialPageInfos.Set(
                                page, existingText.SpatialPageInfos.GetValue(page));
                        }

                        // In order for newText's rasterZones to show up in the correct spot if the
                        // spatial page infos differed at all, we need to convert newText's raster
                        // zones into the unifiedSpatialPageInfo coordinate system.
                        IUnknownVector translatedRasterZones =
                            (IUnknownVector)newText.GetTranslatedImageRasterZones(
                                unifiedSpatialPageInfos);

                        // Recreate newText using the translated raster zones and
                        // unifiedSpatialPageInfos. The two spatial strings are now able to be
                        // merged.
                        newText.CreateHybridString(translatedRasterZones, newText.String,
                            existingText.SourceDocName, unifiedSpatialPageInfos); 
                    }


                    // If the current selection is not at the end of the existing text,
                    // insert the new text in place of the current selection.
                    int selectionStart = textBoxControl.SelectionStart;
                    if (selectionStart < textBoxControl.Text.Length)
                    {
                        // Remove the existing selection if necessary.
                        if (textBoxControl.SelectionLength > 0)
                        {
                            existingText.Remove(selectionStart,
                                selectionStart + textBoxControl.SelectionLength - 1);
                        }

                        existingText.Insert(selectionStart, newText);
                    }
                    // If the caret is currently at the end of the existing text, just append the
                    // swiped text.
                    else
                    {
                        existingText.Append(newText);
                    }

                    newText = existingText;
                }

                return newText;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28834", ex);
            }
        }

        /// <summary>
        /// Returns a <see langword="string"/> representing the tab index of the provided control 
        /// as a number prefixed by period separated numbers representing all parent controls
        /// beneath the <see cref="DataEntryControlHost"/>.  For example, if the control has a 
        /// <see cref="Control.TabIndex"/> of 2, a parent with a <see cref="Control.TabIndex"/> 
        /// of 5, and a grandparent with a <see cref="Control.TabIndex"/> of 3, the return value
        /// would be "3.5.2"
        /// </summary>
        /// <param name="control">The <see cref="Control"/> for which a tab index value is needed.
        /// </param>
        /// <returns>A <see langword="string"/> representing the tab index values of it and its 
        /// ancestors.</returns>
        internal static string GetTabIndex(Control control)
        {
            try
            {
                if (control.Parent != null && !(control.Parent is DataEntryControlHost))
                {
                    // Recursively include the tab index of all ancestors beneath the
                    // DataEntryControlHost
                    return GetTabIndex(control.Parent) + "." +
                        control.TabIndex.ToString(CultureInfo.CurrentCulture);
                }
                else
                {
                    // There are no parents, just return a string representing this control's
                    // TabIndex.
                    return control.TabIndex.ToString(CultureInfo.CurrentCulture);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24683", ex);
            }
        }

        /// <summary>
        /// Causes the specified <see cref="IAttribute"/>s to be re-ordered within the 
        /// specified sourceAttributes using the <see cref="AttributeStatusInfo.DisplayOrder"/>
        /// (lowest to highest).
        /// <para><b>Note</b></para>
        /// Attributes with a <see cref="AttributeStatusInfo.DisplayOrder"/> value will be placed
        /// before attributes that don't have the value specified.
        /// </summary>
        /// <param name="sourceAttributes">An <see cref="IUnknownVector"/> of 
        /// <see cref="IAttribute"/>s in which the specified attribute(s) are to be re-ordered.
        /// </param>
        /// <param name="attributesToReorder">The <see cref="IAttribute"/>s to be reordered within
        /// the source attribute vector.
        /// <para><b>Note</b></para>
        /// It is not required that these attributes are already contained in sourceAttributes. 
        /// It is assumed that all these attributes share the same 
        /// <see cref="AttributeStatusInfo.DisplayOrder"/> value.</param>
        internal static void ReorderAttributes(IUnknownVector sourceAttributes,
            IUnknownVector attributesToReorder)
        {
            try
            {
                // The display order by which the attributesToReorder should be ordered.
                string reorderByValue = null;

                // In case the attributes already exist in sourceAttributes, remove them.
                int attributeCount = attributesToReorder.Size();
                for (int i = 0; i < attributeCount; i++)
                {
                    // Obtain the DisplayOrder value to reorder the attributes with.
                    IAttribute attribute = (IAttribute)attributesToReorder.At(i);

                    // The first time through, obtain the display order value to use in reordering
                    // the attributes.
                    if (i == 0)
                    {
                        reorderByValue = AttributeStatusInfo.GetStatusInfo(attribute).DisplayOrder;
                    }
                    // Verify that subsequent attributes have the same display order.
                    else
                    {
                        Extract.ExtractException.Assert("ELI24905",
                            "Unexpected error sorting attributes!",
                             string.Compare(
                                reorderByValue, 
                                AttributeStatusInfo.GetStatusInfo(attribute).DisplayOrder, 
                                StringComparison.CurrentCulture) == 0);
                    }

                    if (string.IsNullOrEmpty(reorderByValue))
                    {
                        // If no display order is specified, add the attribute if necessary, but
                        // keep the attributes where they are if the attribute is already contained.
                        sourceAttributes.PushBackIfNotContained(attribute);
                        break;
                    }
                    else
                    {
                        // RemoveValue doesn't complain about missing values. (and we don't care to
                        // have it complain here)
                        sourceAttributes.RemoveValue(attribute);
                    }
                }

                if (!string.IsNullOrEmpty(reorderByValue))
                {
                    // Loop through the source attributes looking for the correct position in which
                    // to insert the attributes.
                    int index;
                    int sourceAttributeCount = sourceAttributes.Size();
                    for (index = 0; index < sourceAttributeCount; index++)
                    {
                        IAttribute attribute = (IAttribute)sourceAttributes.At(index);
                        string sourceDisplayOrder =
                            AttributeStatusInfo.GetStatusInfo(attribute).DisplayOrder;

                        // If the next attribute does not have a DisplayOrder specified or has a larger
                        // DisplayOrder value, we've found the place.
                        if (string.IsNullOrEmpty(sourceDisplayOrder) || string.Compare(
                                reorderByValue, sourceDisplayOrder, StringComparison.CurrentCulture) < 0)
                        {
                            break;
                        }
                    }

                    // Insert the attributes.
                    sourceAttributes.InsertVector(index, attributesToReorder);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24685", ex);
            }
        }

        /// <summary>
        /// Obtains the absolute path for the specified path relative to the location of
        /// the Extract.DataEntry assembly.
        /// </summary>
        /// <param name="pathName">The path name to resolve.</param>
        /// <returns>The absolute path for the specified path relative to the location of
        /// the Extract.DataEntry assembly.</returns>
        public static string ResolvePath(string pathName)
        {
            try
            {
                pathName = pathName.Replace(_PFES_FOLDER, FileSystemMethods.ExtractSystemsPath);

                return FileSystemMethods.GetAbsolutePath(pathName, _solutionRootDirectory);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24757", ex);
            }
        }

        // TODO: Create CreateDBCommand overrides for all database types to be supported.

        /// <summary>
        /// Generates a <see cref="DbCommand"/> based on the specified query, parameters and database
        /// connection.
        /// </summary>
        /// <param name="sqlCEConnection">The <see cref="SqlCeConnection"/> for which the command is
        /// to apply.</param>
        /// <param name="query">The <see cref="DbCommand"/>'s <see cref="DbCommand.CommandText"/>
        /// value.</param>
        /// <param name="parameters">A <see cref="Dictionary{T, T}"/> of parameter names and values
        /// that need to be parameterized for the command if specified, <see langword="null"/> if
        /// parameters are not being used. Note that if parameters are being used, the parameter
        /// names must have already been inserted into <see paramref="query"/>.</param>
        /// <returns>The generated <see cref="DbCommand"/>.</returns>
        public static DbCommand CreateDBCommand(SqlCeConnection sqlCEConnection, string query,
            Dictionary<string, string> parameters)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI26727", _OBJECT_NAME);

                ExtractException.Assert("ELI26731", "Null argument exception!",
                    sqlCEConnection != null);
                ExtractException.Assert("ELI26732", "Null argument exception!",
                    !string.IsNullOrEmpty(query));

                SqlCeCommand sqlCeCommand = new SqlCeCommand(query, sqlCEConnection);

                // If parameters are being used, specify them.
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, string> parameter in parameters)
                    {
                        sqlCeCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                return sqlCeCommand;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26730", ex);
                ee.AddDebugData("Query", query, false);
                throw ee;
            }
        }

        /// <summary>
        /// Executes a query against the specified database connection and returns the
        /// result as a string array.
        /// </summary>
        /// <param name="dbCommand">The <see cref="DbCommand"/> defining the query to be applied.
        /// </param>
        /// <param name="columnSeparator">The string used to separate multiple column results.
        /// (Will not be included in any result with less than 2 columns)</param>
        /// <returns>An array of <see cref="string"/>s, each representing a result row from the
        /// query.</returns>
        public static string[] ExecuteDBQuery(DbCommand dbCommand, string columnSeparator)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI26758", _OBJECT_NAME);

                ExtractException.Assert("ELI26151", "Null argument exception!", dbCommand != null);

                using (DbDataReader sqlReader = dbCommand.ExecuteReader())
                {
                    List<string> results = new List<string>();

                    // Loop throw each row of the results.
                    while (sqlReader.Read())
                    {
                        StringBuilder result = new StringBuilder();

                        // Keep track of all column delimiters that are appended. They are
                        // only added once it is confirmed that there is more data in the
                        // row.
                        StringBuilder pendingColumnDelimiters = new StringBuilder();

                        for (int i = 0; i < sqlReader.FieldCount; i++)
                        {
                            // If not the first column result, a column separator may be needed.
                            if (i > 0)
                            {
                                pendingColumnDelimiters.Append(columnSeparator);
                            }

                            // Append a result only if there is a value to append.
                            if (!sqlReader.IsDBNull(i))
                            {
                                string columnValue = sqlReader.GetString(i);

                                if (!string.IsNullOrEmpty(columnValue))
                                {
                                    // If there is data to write, go ahead and commit all pending
                                    // column delimiters.
                                    result.Append(pendingColumnDelimiters.ToString());

                                    // Reset the pending column delimiters
                                    pendingColumnDelimiters = new StringBuilder();

                                    result.Append(columnValue);
                                }
                            }
                        }

                        results.Add(result.ToString());
                    }

                    return results.ToArray();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee =
                    new ExtractException("ELI26150", "Database query failed.", ex);

                if (dbCommand != null)
                {
                    ee.AddDebugData("Query", dbCommand.CommandText, false);

                    try
                    {
                        foreach (DbParameter parameter in dbCommand.Parameters)
                        {
                            ee.AddDebugData("Parameter " + parameter.ParameterName,
                                parameter.Value.ToString(), false);
                        }
                    }
                    catch (Exception ex2)
                    {
                        ExtractException.Log("ELI27106", ex2);
                    }
                }

                throw ee;
            }
        }

        /// <summary>
        /// Updates the provided auto-complete parameters using the provided
        /// <see cref="IDataEntryValidator"/> (if an update is required).
        /// </summary>
        /// <param name="validator">The <see cref="IDataEntryValidator"/> that is to provide the
        /// autocomplete values.</param>
        /// <param name="autoCompleteMode">The <see cref="AutoCompleteMode"/> field to be updated.
        /// </param>
        /// <param name="autoCompleteSource">The <see cref="AutoCompleteSource"/> field to be
        /// updated.</param>
        /// <param name="autoCompleteList">The <see cref="AutoCompleteStringCollection"/> of
        /// auto-complete values. A copy of every value will be prefixed with a space to enable the
        /// space bar to open the auto-complete list.</param>
        /// <param name="autoCompleteValues">The auto-complete values provided by the
        /// <see paramref="validator"/>. Note: This does not include copies of the values prefixed
        /// with a space as with <see paramref="autoCompleteList"/>.</param>
        /// <returns><see langword="true"/> if any of the auto-complete settings were updated,
        /// <see langword="false"/> if the auto-complete settings are untouched.</returns>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "2#")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "3#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "4#")]
        public static bool UpdateAutoCompleteList(IDataEntryValidator validator,
            ref AutoCompleteMode autoCompleteMode, ref AutoCompleteSource autoCompleteSource,
            ref AutoCompleteStringCollection autoCompleteList, out string[] autoCompleteValues)
        {
            try
            {
                autoCompleteValues = new string[] { };

                // If there is no active validator, simply turn off auto-complete if it is not
                // already off.
                if (validator == null)
                {
                    if (autoCompleteMode != AutoCompleteMode.None)
                    {
                        autoCompleteMode = AutoCompleteMode.None;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                // If available, use the validation list values to initialize the
                // auto-complete values.
                autoCompleteValues = validator.GetAutoCompleteValues();
                if (autoCompleteValues != null)
                {
                    // [DataEntry:443]
                    // Add each item from the auto-complete values to the auto-complete list twice,
                    // once as it is in the validation list, and the second time with a leading
                    // space. This way, a user can press space in an empty cell to see all possible
                    // values.
                    AutoCompleteStringCollection newAutoCompleteList =
                        new AutoCompleteStringCollection();
                    for (int i = 0; i < autoCompleteValues.Length; i++)
                    {
                        newAutoCompleteList.Add(" " + autoCompleteValues[i]);
                    }
                    newAutoCompleteList.AddRange(autoCompleteValues);

                    // If autoCompleteMode or autoCompleteSource have changed, an update is needed.
                    bool updateRequired = (autoCompleteMode != AutoCompleteMode.SuggestAppend ||
                                           autoCompleteSource != AutoCompleteSource.CustomSource);

                    // Otherwise, compare the existing list with the new list... if they are
                    // different, an update is required.
                    if (!updateRequired)
                    {
                        // If they are not the same size, an update is required.
                        if (newAutoCompleteList.Count != autoCompleteList.Count)
                        {
                            updateRequired = true;
                        }
                        // ...or if the new list differs at all from the old list, an update is
                        // required.
                        else if (autoCompleteList.Count != newAutoCompleteList
                                    .Cast<string>()
                                    .Intersect(autoCompleteList.Cast<string>())
                                    .Count())
                        {
                            updateRequired = true;
                        }
                    }

                    // Only change auto-complete settings if required... the act of doing so
                    // triggers GDI object leaks (at least on Windows XP).
                    // https://connect.microsoft.com/VisualStudio/feedback/details/116641
                    if (updateRequired)
                    {
                        // Initialize auto-complete mode in case validation lists are used.
                        autoCompleteMode = AutoCompleteMode.SuggestAppend;
                        autoCompleteSource = AutoCompleteSource.CustomSource;
                        autoCompleteList = newAutoCompleteList;

                        return true;
                    }
                }
                // If a null list was provided, simply turn off auto-complete if it is not already
                // off.
                else if (autoCompleteMode != AutoCompleteMode.None)
                {
                    autoCompleteMode = AutoCompleteMode.None;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30115", ex);
            }
        }

        #endregion Static Methods
    }
}
