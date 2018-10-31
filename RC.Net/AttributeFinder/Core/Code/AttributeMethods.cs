using Extract.Interop;
using Extract.Interop.Zip;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Extract.Imaging;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using ComAttribute = UCLID_AFCORELib.Attribute;
using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;

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
                TranslateAttributesToNewDocument(attributes, newDocumentName, pageMap, null, newSpatialPageInfos);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39708");
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
        /// <param name="rotatedPages">Information about which pages have been rotated</param>
        /// <param name="newSpatialPageInfos">The new spatial page infos to be associated with this string.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static void TranslateAttributesToNewDocument(IIUnknownVector attributes,
            string newDocumentName, Dictionary<Tuple<string, int>, List<int>> pageMap,
            ReadOnlyCollection<(string documentName, int page, int rotation)> rotatedPages,
            LongToObjectMap newSpatialPageInfos)
        {
            try
            {
                foreach (IAttribute attribute in attributes.ToIEnumerable<IAttribute>())
                {
                    TranslateAttributesToNewDocument(attribute.SubAttributes,
                        newDocumentName, pageMap, rotatedPages, newSpatialPageInfos);

                    if (attribute.Value.GetMode() != ESpatialStringMode.kNonSpatialMode)
                    {
                        attribute.Value = TranslateSpatialStringToNewDocument(
                            attribute.Value, newDocumentName, pageMap, rotatedPages, newSpatialPageInfos);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46451");
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
        /// Returns an enumeration of all <see paramref="attributes"/> and their subattributes, recursively.
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s to enumerate.</param>
        /// <returns>An enumeration of <see paramref="attribute"/> and its subattributes, recursively</returns>
        public static IEnumerable<IAttribute> EnumerateDepthFirst(this IUnknownVector attributes)
        {
            foreach (var attribute in attributes.ToIEnumerable<IAttribute>())
            {
                foreach (var descendant in EnumerateDepthFirst(attribute))
                {
                    yield return descendant;
                }
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

        /// <summary>
        /// Loads the IUnknownVector of attributes from the byte array
        /// </summary>
        /// <param name="value">Byte array containing the vector of attributes</param>
        /// <returns>IUnknownVector of attributes that was in the byte array</returns>
        public static IUnknownVector GetVectorOfAttributesFromSqlBinary(byte[] value)
        {
            using (var stream = new MemoryStream(value))
            {
                return GetVectorOfAttributesFromSqlBinary(stream);
            }
        }

        /// <summary>
        /// Loads the IUnknownVector of attributes from the stream
        /// </summary>
        /// <param name="binaryAsStream">Stream containing the vector of attributes</param>
        /// <returns>IUnknownVector of attributes that was in the stream</returns>
        public static IUnknownVector GetVectorOfAttributesFromSqlBinary(Stream binaryAsStream)
        {
            try
            {
                // Check if the stream has any data
                if (binaryAsStream.Length == 0)
                {
                    return new IUnknownVector();
                }
                // Unzip the data in the stream
                var zipStream = new ManagedInflater(binaryAsStream);
                using (var unZippedStream = zipStream.InflateToStream())
                {
                    // Advance the stream past the GUID
                    unZippedStream.Seek(16, SeekOrigin.Begin);

                    // Creating the IUnknownVector with the prog id because if the UCLID_COMUTILSLib reference property for Embed interop types
                    // is set to false the IUnknownVector created with new is unable to get the IPersistStream interface
                    Type type = Type.GetTypeFromProgID("UCLIDCOMUtils.IUnknownVector");
                    IUnknownVector voa = (IUnknownVector)Activator.CreateInstance(type);
                    IPersistStream persistVOA = voa as IPersistStream;
                    ExtractException.Assert("ELI41533", "Unable to Obtain IPersistStream Interface.", persistVOA != null);

                    // Wrap the unzipped stream for loading the VOA
                    IStreamWrapper binaryIPersistStream = new IStreamWrapper(unZippedStream);
                    if (persistVOA != null)
                    {
                        persistVOA.Load(binaryIPersistStream);
                    }
                    return voa;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41546");
            }
        }

        /// <summary>
        /// Since QueryAttributes is very slow from .NET, this can be used for simple queries
        /// </summary>
        /// <param name="attributes">The attributes to query</param>
        /// <param name="query">The AFQuery</param>
        /// <param name="matching">The attributes that matched the query</param>
        /// <param name="notMatching">The attributes that didn't match the query</param>
        /// <returns><c>true</c> if the query was successfully applied</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public static bool TryDivideAttributesWithSimpleQuery(
            this IEnumerable<ComAttribute> attributes,
            string query,
            out IEnumerable<ComAttribute> matching,
            out IEnumerable<ComAttribute> notMatching)
        {
            try
            {
                if (string.IsNullOrEmpty(query))
                {
                    matching = Enumerable.Empty<ComAttribute>();
                    notMatching = attributes;
                    return true;
                }
                else if (query.IndexOfAny(new[] { '/', '{' }) > 0)
                {
                    matching = Enumerable.Empty<ComAttribute>();
                    notMatching = Enumerable.Empty<ComAttribute>();
                    return false;
                }

                // Make a test function from the query
                var isMatch = query
                    .Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select<string, Func<ComAttribute, bool>>(q =>
                    {
                    // Query is Name[@Type]
                    // '*@' = type must be empty so don't remove empty entries from split
                    var queryParts = q.Split('@');
                        var name = queryParts[0].Trim();
                        var checkName = !string.Equals(name, "*", StringComparison.Ordinal);
                        var checkType = queryParts.Length > 1;
                        var type = checkType
                            ? queryParts[1].Trim()
                            : null;
                        var typeMustBeEmpty = checkType && string.IsNullOrEmpty(type);

                        return at =>
                        {
                            var atType = at.Type;
                            return (!checkName || string.Equals(name, at.Name, StringComparison.OrdinalIgnoreCase))
                                && !(typeMustBeEmpty && !string.IsNullOrEmpty(atType))
                                && (!checkType || atType.Split('+').Any(t =>
                                        string.Equals(type, t, StringComparison.OrdinalIgnoreCase)));
                        };
                    })
                    .Aggregate((f1, f2) => at => f1(at) || f2(at));


                var m = new List<ComAttribute>();
                var not = new List<ComAttribute>();
                foreach (var at in attributes)
                {
                    if (isMatch(at))
                    {
                        m.Add(at);
                    }
                    else
                    {
                        not.Add(at);
                    }
                }
                matching = m;
                notMatching = not;
                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45540");
            }
        }

        /// <summary>
        /// Creates a new non-spatial <see cref="IAttribute"/> as a child of
        /// <see paramref="attribute"/> that has the specified <see paramref="name"/> and, optionally,
        /// the specified <see paramref="value"/> and associated <see paramref="sourceDocName"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> the new attribute should have as a parent.</param>
        /// <param name="name">The name of the new attribute.</param>
        /// <param name="value">The non-spatial string value of the new attribute (can be <c>null</c> for unspecified).</param>
        /// <param name="sourceDocName">Name of the source document the attribute is associated with
        /// (can be <c>null</c> for unspecified).</param>
        /// <returns>The newly created <see cref="IAttribute"/>.</returns>
        public static IAttribute AddSubAttribute(this IAttribute attribute, string name, string value = null, string sourceDocName = null)
        {
            try
            {
                return attribute.SubAttributes.AddSubAttribute(name, value, sourceDocName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45735");
            }
        }

        /// <summary>
        /// Creates a new non-spatial <see cref="IAttribute"/> as a member of
        /// <see paramref="attributes"/> that has the specified <see paramref="name"/> and, optionally,
        /// the specified <see paramref="value"/> and associated <see paramref="sourceDocName"/>.
        /// </summary>
        /// <param name="attributes">The <see cref="IIUnknownVector"/> the new attribute should be added to.</param>
        /// <param name="name">The name of the new attribute.</param>
        /// <param name="value">The non-spatial string value of the new attribute (can be <c>null</c> for unspecified).</param>
        /// <param name="sourceDocName">Name of the source document the attribute is associated with
        /// (can be <c>null</c> for unspecified).</param>
        /// <returns>The newly created <see cref="IAttribute"/>.</returns>
        public static IAttribute AddSubAttribute(this IIUnknownVector attributes, string name, string value = null, string sourceDocName = null)
        {
            try
            {
                var attributeValue = new SpatialString();
                attributeValue.CreateNonSpatialString(value, sourceDocName);

                var subAttribute = new UCLID_AFCORELib.Attribute();
                subAttribute.Name = name;
                subAttribute.Value = attributeValue;

                attributes.PushBack(subAttribute);

                return subAttribute;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45748");
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
        /// <param name="rotatedPages">Information about which pages have been rotated</param>
        /// <param name="newSpatialPageInfos">The new spatial page infos to be associated with this string.</param>
        /// <returns>The translated spatial string</returns>
        static SpatialString TranslateSpatialStringToNewDocument(SpatialString value,
            string newDocumentName, Dictionary<Tuple<string, int>, List<int>> pageMap,
            ReadOnlyCollection<(string documentName, int page, int rotation)> rotatedPages,
            LongToObjectMap newSpatialPageInfos)
        {
            var spatialMode = value.GetMode();

            ExtractException.Assert("ELI39709", "Unexpected spatial mode.",
                spatialMode != ESpatialStringMode.kNonSpatialMode);

            // https://extract.atlassian.net/browse/ISSUE-13873
            // To avoid issues with pages being output to a new document in a different order than
            // they came in, downgrade any multi-page attributes to hybrid strings so that
            // unexpected page order doesn't cause errors.
            if (spatialMode == ESpatialStringMode.kSpatialMode
                && value.GetFirstPageNumber() != value.GetLastPageNumber())
            {
                // Copy the value so as not to mutate the input
                value = (SpatialString)((ICopyableObject) value).Clone();
                value.DowngradeToHybridMode();
                spatialMode = ESpatialStringMode.kHybridMode;
            }

            string sourceDocName = GetSourceDocName(value, pageMap);

            var updatedPages = new List<SpatialString>();
            foreach (SpatialString page in value.GetPages(false, "")
                .ToIEnumerable<SpatialString>())
            {
                int oldPageNum = page.GetFirstPageNumber();

                if (pageMap.TryGetValue(new Tuple<string, int>(sourceDocName, oldPageNum),
                    out var newPageNums))
                {
                    // If for some reason same source page is copied to multiple destination pages,
                    // only copy attribute value to the first of those pages.
                    int newPageNum = newPageNums.Min();

                    bool updated = false;
                    if (rotatedPages != null)
                    {
                        var pageInfoCollection = rotatedPages
                            .Where(info => info.page == oldPageNum
                                           && info.documentName == sourceDocName);
                        var (_, _, rotation) = pageInfoCollection.FirstOrDefault();

                        if (rotation != 0)
                        {
                            var newPageInfos = new LongToObjectMapClass();
                            PaginationMethods.RotatePage(newPageNum, rotation, newPageInfos, page.GetPageInfo(oldPageNum));
                            page.SpatialPageInfos = newPageInfos;
                            updated = true;
                        }
                    }

                    // UpdatePageNumber won't change the spatial page infos if there
                    // are more than one so that call alone is not sufficient to guarantee a
                    // valid collection
                    if (!updated)
                    {
                        var oldPageInfo = page.GetPageInfo(oldPageNum);
                        var newPageInfos = new LongToObjectMapClass();
                        newPageInfos.Set(newPageNum, oldPageInfo);
                        page.SpatialPageInfos = newPageInfos;
                    }

                    page.UpdatePageNumber(newPageNum);
                    page.SourceDocName = newDocumentName;

                    // Set to the shared collection of spatial page infos if the page info is the same
                    // (this can reduce VOA size)
                    if (page.SpatialPageInfos.Contains(newPageNum) && newSpatialPageInfos.Contains(newPageNum))
                    {
                        var oldInfo = page.GetPageInfo(newPageNum);
                        var newInfo = (SpatialPageInfo)newSpatialPageInfos.GetValue(newPageNum);
                        if (oldInfo.Equal(newInfo, false))
                        {
                            page.SpatialPageInfos = newSpatialPageInfos;
                        }
                    }

                    updatedPages.Add(page);
                }
            }

            if (updatedPages.Count == 0)
            {
                SpatialString updatedValue = new SpatialString();
                updatedValue.CreateNonSpatialString(value.String, newDocumentName);

                return updatedValue;
            }

            if (spatialMode == ESpatialStringMode.kHybridMode)
            {
                // If each page's info was compatible with the shared collection
                // then use that
                LongToObjectMap pageInfos;
                if (updatedPages.All(s => s.SpatialPageInfos == newSpatialPageInfos))
                {
                    pageInfos = newSpatialPageInfos;
                }
                // else, build a new map with info that has been adjusted for rotation
                else
                {
                    pageInfos = new LongToObjectMapClass();
                    foreach (var page in updatedPages)
                    {
                        var pageNum = page.GetFirstPageNumber();
                        pageInfos.Set(pageNum, page.GetPageInfo(pageNum));
                    }
                }

                var zones = updatedPages
                    .SelectMany(s => s.GetOCRImageRasterZones().ToIEnumerable<ComRasterZone>())
                    .ToIUnknownVector();

                SpatialString updatedValue = new SpatialString();
                updatedValue.CreateHybridString(zones, value.String, newDocumentName, pageInfos);

                return updatedValue;
            }

            // There will only be one page if mode is not hybrid so just return that
            return updatedPages.Single();
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