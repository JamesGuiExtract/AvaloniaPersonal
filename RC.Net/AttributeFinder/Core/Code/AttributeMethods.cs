using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using ComAttribute = UCLID_AFCORELib.Attribute;
using IAttribute = UCLID_AFCORELib.IAttribute;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Represents a grouping of methods for working with COM attribute.
    /// </summary>
    [CLSCompliant(false)]
    public static class AttributeMethods
    {
        #region Constants

        /// <summary>
        /// A string representation of the GUID for <see cref="AttributeStorageManagerClass"/> 
        /// </summary>
        static readonly string _ATTRIBUTE_STORAGE_MANAGER_GUID =
            typeof(AttributeStorageManagerClass).GUID.ToString("B");

        #endregion Constants

        #region Public Methods

        /// <summary>
        /// Gets a single attribute by name from the specified vector of attributes. Throws an 
        /// exception if more than one attribute is found.
        /// </summary>
        /// <param name="attributes">The attributes to search.</param>
        /// <param name="name">The name of the attribute to find.</param>
        /// <returns>The only attribute in <paramref name="attributes"/> with the specified name; 
        /// if no such attribute exists, returns <see langword="null"/>.</returns>
        public static ComAttribute GetSingleAttributeByName(IIUnknownVector attributes, string name)
        {
            try
            {
                ComAttribute[] idAttributes = GetAttributesByName(attributes, name);

                if (idAttributes.Length == 0)
                {
                    return null;
                }
                else if (idAttributes.Length == 1)
                {
                    return idAttributes[0];
                }

                throw new ExtractException("ELI28197",
                    "More than one " + name + " attribute found.");
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28541",
                    "Unable to get attribute by name.", ex);
                ee.AddDebugData("Attribute name", name, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets an array of COM attributes that have the specified name.
        /// </summary>
        /// <param name="attributes">A vector of COM attributes to search.</param>
        /// <param name="names">The name(s) of the attributes to return.</param>
        /// <returns>An array of COM attributes in <paramref name="attributes"/> that matches one of
        /// the specified <paramref name="names"/>.</returns>
        public static ComAttribute[] GetAttributesByName(IIUnknownVector attributes, params string[] names)
        {
            try
            {
                List<ComAttribute> result = new List<ComAttribute>();

                // Iterate over each attribute
                int count = attributes.Size();
                for (int i = 0; i < count; i++)
                {
                    ComAttribute attribute = (ComAttribute)attributes.At(i);

                    // If this attribute matches the specified name, add it to the result
                    string attributeName = attribute.Name;
                    foreach (string name in names)
                    {
                        if (attributeName.Equals(name, StringComparison.OrdinalIgnoreCase))
                        {
                            result.Add(attribute);
                            break;
                        }
                    }
                }

                return result.ToArray();
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28540",
                    "Unable to get attribute by name.", ex);
                string nameList = "";
                try
                {
                    nameList = string.Join(", ", names);
                }
                catch (Exception){}
                ee.AddDebugData("Attribute names", nameList, false);
                throw ee;
            }
        }

        /// <summary>
        /// Appends attributes as children of the specified attribute.
        /// </summary>
        /// <param name="attribute">The attribute to which attributes should be appended.</param>
        /// <param name="children">The attributes to append as children to 
        /// <paramref name="attribute"/>.</param>
        public static void AppendChildren(IAttribute attribute, params ComAttribute[] children)
        {
            try
            {
                IUnknownVector subAttributes = attribute.SubAttributes;
                foreach (ComAttribute child in children)
                {
                    subAttributes.PushBack(child);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29737", ex);
            }
        }

        /// <summary>
        /// Translates all spatial <see cref="IAttribute"/> values in <see paramref="attributes"/>
        /// to be associated with the <see paramref="newDocumentName"/> where
        /// <see paramref="pageMap"/> relates each original page to a corresponding page number in
        /// <see paramref="newDocumentName"/>.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/> hierarchy to update.</param>
        /// <param name="newDocumentName">The name of the file with which the attribute values
        /// should now be associated.</param>
        /// <param name="pageMap">Each key represents a tuple of the old document name and page
        /// number while the value represents the new page number(s) in 
        /// <see paramref="newDocumentName"/> associated with that source page.</param>
        /// <param name="newSpatialPageInfos">The new spatial page infos to be associated with this string.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static void TranslateAttributesToNewDocument(IIUnknownVector attributes,
            string newDocumentName, Dictionary<Tuple<string, int>, List<int>> pageMap,
            LongToObjectMap newSpatialPageInfos)
        {
            try
            {
                foreach (IAttribute attribute in attributes.ToIEnumerable<IAttribute>())
                {
                    TranslateAttributesToNewDocument(attribute.SubAttributes,
                        newDocumentName, pageMap, newSpatialPageInfos);

                    SpatialString value = attribute.Value;
                    if (value.GetMode() == ESpatialStringMode.kSpatialMode)
                    {
                        attribute.Value = TranslateSpatialStringToNewDocument(
                            value, newDocumentName, pageMap, newSpatialPageInfos);
                    }
                    else if (value.GetMode() == ESpatialStringMode.kHybridMode)
                    {
                        attribute.Value = TranslateHybridStringToNewDocument(
                            value, newDocumentName, pageMap, newSpatialPageInfos);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39708");
            }
        }

        /// <summary>
        /// Converts <see paramref="enumerable" /> into an <see cref="IIUnknownVector" /> and
        /// saves it to <see paramref="fileName" /> using <see cref="AttributeStorageManagerClass" />
        /// </summary>
        /// <param name="enumerable">The <see cref="IEnumerable{ComAttribute}" /> to convert.</param>
        /// <param name="fileName">Full path of the file to be saved.</param>
        [CLSCompliant(false)]
        public static void SaveToIUnknownVector(this IEnumerable<ComAttribute> enumerable, string fileName)
        {
            try
            {
                enumerable.ToIUnknownVector().SaveTo(fileName, false, _ATTRIBUTE_STORAGE_MANAGER_GUID);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI40167", ex);
            }
        }

        /// <summary>
        /// Gets the GUID associated with the specified Attribute, as a string value.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <returns>GUID as string</returns>
        public static string AttributeGuidAsString(ComAttribute attribute)
        {
            string guid = ((IIdentifiableObject)attribute).InstanceGUID.ToString();
            return guid;
        }

        /// <summary>
        /// Returns an enumeration of <see paramref="attribute"/> and its subattributes, recursively
        /// </summary>
        /// <param name="attribute">The attribute root of the tree.</param>
        /// <returns>An enumeration of <see paramref="attribute"/> and its subattributes, recursively</returns>
        public static IEnumerable<IAttribute> EnumerateDepthFirst(this IAttribute attribute)
        {
            yield return attribute;
            foreach(var descendant in attribute.SubAttributes
                .ToIEnumerable<ComAttribute>()
                .SelectMany(EnumerateDepthFirst))
            {
                yield return descendant;
            }
        }

        /// <summary>
        /// Recursively updates the source document name of a vector of attributes.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <param name="newSourceDocName">New source document name.</param>
        public static void UpdateSourceDocNameOfAttributes(this IIUnknownVector attributes, string newSourceDocName)
        {
            try
            {
                foreach (var attribute in attributes.ToIEnumerable<IAttribute>()
                    .SelectMany(EnumerateDepthFirst))
                {
                    var val = attribute.Value;
                    if (!string.Equals(newSourceDocName, val.SourceDocName, StringComparison.Ordinal))
                    {
                        val.SourceDocName = newSourceDocName;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41683");
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Translates a <see cref="SpatialString"/> in <see cref="ESpatialStringMode.kSpatialMode"/>
        /// to be associated with the <see paramref="newDocumentName"/> where
        /// <see paramref="pageMap"/> relates each original page to a corresponding page number in
        /// <see paramref="newDocumentName"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpatialString"/> value to translate.</param>
        /// <param name="newDocumentName">The name of the file with which the value should now be
        /// associated.</param>
        /// <param name="pageMap">Each key represents a tuple of the old document name and page
        /// number while the value represents the new page number(s) in 
        /// <see paramref="newDocumentName"/> associated with that source page.</param>
        /// <param name="newSpatialPageInfos">The new spatial page infos to be associated with this string.</param>
        /// <returns>The translated spatial string</returns>
        static SpatialString TranslateSpatialStringToNewDocument(SpatialString value,
            string newDocumentName, Dictionary<Tuple<string, int>, List<int>> pageMap,
            LongToObjectMap newSpatialPageInfos)
        {
            ExtractException.Assert("ELI39709", "Unexpected spatial mode.",
                value.GetMode() == ESpatialStringMode.kSpatialMode);

            // https://extract.atlassian.net/browse/ISSUE-13873
            // To avoid issues with pages being output to a new document in a different order than
            // they came in, downgrade any multi-page attributes to hybrid strings so that
            // unexpected page order doesn't cause errors.
            if (value.GetFirstPageNumber() != value.GetLastPageNumber())
            {
                value.DowngradeToHybridMode();
                return TranslateHybridStringToNewDocument(
                    value, newDocumentName, pageMap, newSpatialPageInfos);
            }

            string sourceDocName = GetSourceDocName(value, pageMap);

            var updatedPages = new List<SpatialString>();
            foreach (SpatialString page in value.GetPages(false, "")
                .ToIEnumerable<SpatialString>())
            {
                int oldPageNum = page.GetFirstPageNumber();

                List<int> newPageNums = null;
                if (pageMap.TryGetValue(new Tuple<string, int>(sourceDocName, oldPageNum),
                    out newPageNums))
                {
                    // If for some reason same source page is copied to multiple destination pages,
                    // only copy attribute value to the first of those pages.
                    int newPageNum = newPageNums.Min();
                    page.SpatialPageInfos = newSpatialPageInfos;
                    page.UpdatePageNumber(newPageNum);
                    page.SourceDocName = newDocumentName;
                    updatedPages.Add(page);
                }
            }

            SpatialString updatedValue = new SpatialString();
            if (updatedPages.Count == 0)
            {
                updatedValue.CreateNonSpatialString(value.String, newDocumentName);
            }
            else
            {
                updatedValue.CreateFromSpatialStrings(
                    updatedPages.ToIUnknownVector<SpatialString>());
            }
            return updatedValue;
        }

        /// <summary>
        /// Translates a <see cref="SpatialString"/> in <see cref="ESpatialStringMode.kHybridMode"/>
        /// to be associated with the <see paramref="newDocumentName"/> where
        /// <see paramref="pageMap"/> relates each original page to a corresponding page number in
        /// <see paramref="newDocumentName"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpatialString"/> value to translate.</param>
        /// <param name="newDocumentName">The name of the file with which the value should now be
        /// associated.</param>
        /// <param name="pageMap">Each key represents a tuple of the old document name and page
        /// number while the value represents the new page number(s) in 
        /// <see paramref="newDocumentName"/> associated with that source page.</param>
        /// <param name="newSpatialPageInfos">The new spatial page infos to be associated with this string.</param>
        /// <returns>The translated spatial string</returns>
        static SpatialString TranslateHybridStringToNewDocument(SpatialString value,
            string newDocumentName, Dictionary<Tuple<string, int>, List<int>> pageMap,
            LongToObjectMap newSpatialPageInfos)
        {
            ExtractException.Assert("ELI39710", "Unexpected spatial mode.",
                value.GetMode() == ESpatialStringMode.kHybridMode);

            string sourceDocName = GetSourceDocName(value, pageMap);

            var updatedRasterZones = new List<IRasterZone>();
            foreach (IRasterZone rasterZone in value.GetOCRImageRasterZones()
                .ToIEnumerable<IRasterZone>())
            {
                int oldPageNum = rasterZone.PageNumber;
                List<int> newPageNums = null;
                if (pageMap.TryGetValue(new Tuple<string, int>(sourceDocName, oldPageNum),
                    out newPageNums))
                {
                    // If for some reason same source page is copied to multiple destination pages,
                    // only copy attribute value to the first of those pages.
                    int newPageNum = newPageNums.Min();
                    rasterZone.PageNumber = newPageNum;
                    updatedRasterZones.Add(rasterZone);
                }
            }

            SpatialString updatedValue = new SpatialString();
            if (updatedRasterZones.Count == 0)
            {
                updatedValue.CreateNonSpatialString(value.String, newDocumentName);
            }
            else
            {
                updatedValue.CreateHybridString(updatedRasterZones.ToIUnknownVector<IRasterZone>(),
                    value.String, newDocumentName, newSpatialPageInfos);
            }
            return updatedValue;
        }

        /// <summary>
        /// Gets the SourceDocName property from <see paramref="value"/>. In order to account for
        /// files that have been moved from their original location, in the case that the
        /// SourceDocName does not appear in <see paramref="pageMap"/>, it will instead return the
        /// only distinct file path from <see paramref="pageMap"/> that shares the same filename
        /// (excluding the directory).
        /// </summary>
        /// <param name="value">The <see cref="SpatialString"/> for which the SourceDocName is
        /// needed.</param>
        /// <param name="pageMap">Each key represents a tuple of the old document name and page
        /// number while the value represents the new page number(s) in 
        /// <see paramref="newDocumentName"/> associated with that source page.</param>
        /// <returns>The SourceDocName property from <see paramref="value"/>.</returns>
        static string GetSourceDocName(SpatialString value, Dictionary<Tuple<string, int>, List<int>> pageMap)
        {
            string sourceDocName = value.SourceDocName;
            if (!pageMap.Keys.Any(key => key.Item1 == sourceDocName))
            {
                string sourceFileName = Path.GetFileName(sourceDocName);
                var matchingFileNames = pageMap.Keys
                    .Select(key => key.Item1)
                    .Where(fileName => Path.GetFileName(fileName).Equals(
                        sourceFileName, StringComparison.CurrentCultureIgnoreCase))
                    .Distinct()
                    .ToArray();

                if (matchingFileNames.Length == 1)
                {
                    sourceDocName = matchingFileNames[0];
                }
            }
            return sourceDocName;
        }

        #endregion Private Methods
    }
}