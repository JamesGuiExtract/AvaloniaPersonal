using Extract.Imaging;
using Extract.Interop;
using Extract.Interop.Zip;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
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
                string nameList = string.Empty;
                try
                {
                    nameList = string.Join(", ", names);
                }
                catch (Exception) { }
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
        /// <see paramref="imagePages"/> specifies the source <see cref="imagePage"/> info for each
        /// successive page in <see paramref="newDocumentName"/>.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/> hierarchy to update.</param>
        /// <param name="newDocumentName">The name of the file with which the attribute values
        /// should now be associated.</param>
        /// <param name="imagePages">A sequence of <see cref="ImagePage"/>s representing the source
        /// pages for each successive page in the new output document.</param>
        /// <param name="newSpatialPageInfos">The new spatial page infos to be associated with this string.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static void TranslateAttributesToNewDocument(IIUnknownVector attributes,
            string newDocumentName, IEnumerable<ImagePage> imagePages,
            LongToObjectMap newSpatialPageInfos)
        {
            try
            {
                int destPageNumber = 1;
                var pageMap = new Dictionary<Tuple<string, int>, List<ImagePage>>();
                foreach (var page in imagePages)
                {
                    var sourcePage = new Tuple<string, int>(page.DocumentName, page.PageNumber);
                    if (!pageMap.TryGetValue(sourcePage, out List<ImagePage> destPages))
                    {
                        destPages = new List<ImagePage>();
                        pageMap[sourcePage] = destPages;
                    }

                    destPages.Add(new ImagePage(newDocumentName, destPageNumber++, page.ImageOrientation));
                }

                TranslateAttributesToNewDocument(attributes, newDocumentName, pageMap, newSpatialPageInfos);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46451");
            }
        }

        /// <summary>
        /// Saves a USS and VOA file for the specifed <see paramref="outputFileName"/> with translated
        /// spatial data based on the source document images in <see paramref="imagePages"/>.
        /// </summary>
        /// <param name="outputFileName">The name of the document to which the spatial needs to be
        /// translated.</param>
        /// <param name="attributes">The VOA data to be saved for <see paramref="outputFileName"/>.</param>
        /// <param name="imagePages">The source <see cref="ImagePage"/> that corresponds to each successive
        /// page in <see paramref="outputFileName"/>.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Voa")]
        public static void CreateUssAndVoaForPaginatedDocument(string outputFileName,
            IIUnknownVector attributes, IEnumerable<ImagePage> imagePages)
        {
            try
            {
                var newSpatialPageInfos = CreateUSSForPaginatedDocument(outputFileName, imagePages);

                if (attributes != null)
                {
                    TranslateAttributesToNewDocument(attributes, outputFileName, imagePages, newSpatialPageInfos);

                    attributes.SaveTo(outputFileName + ".voa", false, _ATTRIBUTE_STORAGE_MANAGER_GUID);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46878");
            }
        }

        /// <summary>
        /// Converts <see paramref="enumerable" /> into an <see cref="IUnknownVector" /> and
        /// saves it to <see paramref="fileName" /> using <see cref="AttributeStorageManagerClass" />
        /// </summary>
        /// <param name="enumerable">The <see cref="IEnumerable{ComAttribute}" /> to convert.</param>
        /// <param name="fileName">Full path of the file to be saved.</param>
        [CLSCompliant(false)]
        public static void SaveToIUnknownVector(this IEnumerable<IAttribute> enumerable, string fileName)
        {
            try
            {
                enumerable.ToIUnknownVector().SaveAttributes(fileName);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI40167", ex);
            }
        }

        /// <summary>
        /// Saves the <see paramref="attributes" /> to <see paramref="fileName" /> using <see cref="AttributeStorageManagerClass" />
        /// </summary>
        /// <param name="attributes">The <see cref="IIUnknownVector" /> to save</param>
        /// <param name="fileName">Full path of the file to be saved</param>
        [CLSCompliant(false)]
        public static void SaveAttributes(this IIUnknownVector attributes, string fileName)
        {
            try
            {
                attributes.SaveTo(fileName, false, _ATTRIBUTE_STORAGE_MANAGER_GUID);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI46459", ex);
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
            foreach (var descendant in attribute.SubAttributes
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

        /// <summary>
        /// Creates a new uss file for the specified <see paramref="newDocumentName"/> based upon
        /// the specified <see paramref="pageMap"/> that provides the source <see cref="ImagePage"/>
        /// for each successive page in <see paramref="newDocumentName"/>.
        /// </summary>
        /// <param name="newDocumentName">The name of the document for which the uss file is being
        /// created.</param>
        /// <param name="sourceImagePages">The source <see cref="ImagePage"/>s that correspond to
        /// each successive page in <see paramref="newDocumentName"/>.</param>
        /// <param name="rotatedPages">collection of PageAndRotation; original page number, and
        /// rotation in degrees relative to the original page orientation (= 0 degrees,
        /// so any non-zero amount indicates a rotation)</param>
        /// <returns>The spatial page info map for the output document</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "USS")]
        public static LongToObjectMap CreateUSSForPaginatedDocument(
            string newDocumentName, IEnumerable<ImagePage> sourceImagePages)
        {
            try
            {
                var sourceUSSData = sourceImagePages
                    .Select(page => page.DocumentName)
                    .Distinct()
                    .Where(sourceFileName => File.Exists(sourceFileName + ".uss"))
                    .ToDictionary(sourceFileName => sourceFileName, sourceFileName =>
                    {
                        var ussData = new SpatialString();
                        ussData.LoadFrom(sourceFileName + ".uss", false);
                        ussData.ReportMemoryUsage();
                        return ussData;
                    });

                var newSpatialPageInfos = new LongToObjectMapClass();
                int destPageCount = sourceImagePages.Count();
                var newPageDataArray = new SpatialString[destPageCount];
                bool ussFileExists = false;
                int destPageNumber = 0;
                foreach (var imagePage in sourceImagePages)
                {
                    destPageNumber++;

                    SpatialString sourceDocData;
                    if (sourceUSSData.TryGetValue(imagePage.DocumentName, out sourceDocData))
                    {
                        ussFileExists = true;
                        if (sourceDocData.HasSpatialInfo())
                        {
                            var pageData = sourceDocData.GetSpecifiedPages(imagePage.PageNumber, imagePage.PageNumber);

                            var oldPageInfos = sourceDocData.SpatialPageInfos;
                            if (oldPageInfos.Contains(imagePage.PageNumber))
                            {
                                newSpatialPageInfos.Set(destPageNumber, oldPageInfos.GetValue(imagePage.PageNumber));
                            }

                            // CreateFromSpatialStrings won't accept non-spatial strings
                            // UpdatePageNumber is only valid for spatial strings
                            if (pageData.HasSpatialInfo())
                            {
                                var oldSpatialPageInfo = (SpatialPageInfo)oldPageInfos.GetValue(imagePage.PageNumber);
                                newPageDataArray[destPageNumber - 1] = pageData;
                                pageData.UpdatePageNumber(destPageNumber);

                                if (imagePage.ImageOrientation != 0)
                                {
                                    PaginationMethods.RotatePage(
                                        destPageNumber, imagePage.ImageOrientation, newSpatialPageInfos, oldSpatialPageInfo);
                                }
                            }
                        }
                    }
                }

                var newUSSData = new SpatialString();
                if (newPageDataArray.Length > 0)
                {
                    var newPageData = newPageDataArray
                        .Where(s => s != null)
                        .Select(s =>
                        {
                            var text = s.String;
                            if (!text.EndsWith("\r\n\r\n", StringComparison.Ordinal))
                            {
                                var suffixToAdd = "\r\n";
                                if (!text.EndsWith("\r\n", StringComparison.Ordinal))
                                {
                                    suffixToAdd += suffixToAdd;
                                }
                                s.AppendString(suffixToAdd);
                            }
                            return s;
                        }).ToIUnknownVector();

                    if (newPageData.Size() > 0)
                    {
                        newUSSData.CreateFromSpatialStrings(newPageData, false);
                        newUSSData.SpatialPageInfos = newSpatialPageInfos;
                    }
                }
                if (ussFileExists)
                {
                    newUSSData.SourceDocName = newDocumentName;
                    newUSSData.SaveTo(newDocumentName + ".uss", true, false);
                }
                newUSSData.ReportMemoryUsage();

                return newSpatialPageInfos;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40253");
            }
        }

        /// <summary>
        /// Gets an <see cref="ImagePage"/> represented by <see paramref="attribute"/>.
        /// </summary>
        /// <param name="attribute">The attribute representation of and <see cref="ImagePage"/>.</param>
        public static ImagePage GetAsImagePage(this IAttribute attribute)
        {
            try
            {
                ExtractException.Assert("ELI46867", "Not a page attribute",
                        attribute.Name.Equals("Page", StringComparison.OrdinalIgnoreCase));

                var imagePage = new ImagePage(
                    documentName:
                        AttributeMethods.GetSingleAttributeByName(attribute.SubAttributes, "SourceDocument").Value.String,
                    pageNumber:
                        int.Parse(AttributeMethods.GetSingleAttributeByName(attribute.SubAttributes, "SourcePage").Value.String,
                            CultureInfo.InvariantCulture),
                    imageOrientation:
                        int.Parse(AttributeMethods.GetSingleAttributeByName(attribute.SubAttributes, "Orientation").Value.String,
                            CultureInfo.InvariantCulture),
                    deleted:
                        bool.Parse(AttributeMethods.GetSingleAttributeByName(attribute.SubAttributes, "Deleted")?.Value?.String ?? "False")
                    );

                return imagePage;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46868");
            }
        }

        /// <summary>
        /// Gets an <see cref="IAttribute"/> with sub-attributes representing the specified <see paramref="imagePage"/>.
        /// </summary>
        /// <param name="imagePage">The <see cref="ImagePage"/> to be stored as an <see cref="IAttribute."/></param>
        /// <param name="pageNumber">If specified, stores the specified page number as a destination page number
        /// in the value of the root attribute.</param>
        public static IAttribute GetAsAttribute(this ImagePage imagePage, int pageNumber = 0)
        {
            try
            {
                var pageAttribute = new AttributeClass { Name = "Page" };
                if (pageNumber > 0)
                {
                    pageAttribute.Value.ReplaceAndDowngradeToNonSpatial(
                        pageNumber.ToString(CultureInfo.InvariantCulture));
                }

                pageAttribute.AddSubAttribute("SourceDocument", imagePage.DocumentName);
                pageAttribute.AddSubAttribute("SourcePage", imagePage.PageNumber.ToString(CultureInfo.InvariantCulture));
                pageAttribute.AddSubAttribute("Orientation", imagePage.ImageOrientation.ToString(CultureInfo.InvariantCulture));
                if (imagePage.Deleted)
                {
                    pageAttribute.AddSubAttribute("Deleted", "True");
                }

                return pageAttribute;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46869");
            }
        }

        /// <summary>
        /// Removes all values in the attributesToRemove from the unknownVector of attributes including those in
        /// SubAttributes
        /// </summary>
        /// <param name="unknownVector">The IUknownVector to remove the attributes from</param>
        /// <param name="attributesToRemove">The attributes to remove</param>
        public static void RemoveAttributes(this IIUnknownVector unknownVector, HashSet<IAttribute> attributesToRemove)
        {
            try
            {
                foreach (var a in unknownVector.ToIEnumerable<IAttribute>().ToList())
                {
                    if (attributesToRemove.Contains(a))
                    {
                        unknownVector.RemoveValue(a);
                        attributesToRemove.Remove(a);
                    }
                    else
                    {
                        a.SubAttributes.RemoveAttributes(attributesToRemove);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47000");
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Translates a <see cref="SpatialString"/> in <see cref="ESpatialStringMode.kSpatialMode"/>
        /// to be associated with the <see paramref="newDocumentName"/> where
        /// <see paramref="pageMap"/> relates each original page to a corresponding
        /// <see cref="ImagePage"/> in <see paramref="newDocumentName"/>.
        /// </summary>
        /// <param name="value">The <see cref="SpatialString"/> value to translate.</param>
        /// <param name="newDocumentName">The name of the file with which the value should now be
        /// associated.</param>
        /// <param name="pageMap">Each key represents a tuple of the old document name and page
        /// number while the value represents the <see cref="ImagePage"/>s in 
        /// <see paramref="newDocumentName"/> associated with that source page.</param>
        /// <param name="newSpatialPageInfos">The new spatial page infos to be associated with this string.</param>
        /// <returns>The translated spatial string</returns>
        static SpatialString TranslateSpatialStringToNewDocument(SpatialString value,
            string newDocumentName, Dictionary<Tuple<string, int>, List<ImagePage>> pageMap,
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
                value = (SpatialString)((ICopyableObject)value).Clone();
                value.DowngradeToHybridMode();
                spatialMode = ESpatialStringMode.kHybridMode;
            }

            string sourceDocName = GetSourceDocName(value, pageMap);

            var updatedPages = new List<SpatialString>();
            foreach (SpatialString page in value.GetPages(false, string.Empty)
                .ToIEnumerable<SpatialString>())
            {
                int oldPageNum = page.GetFirstPageNumber();

                if (pageMap.TryGetValue(new Tuple<string, int>(sourceDocName, oldPageNum),
                    out var destImagePages))
                {
                    // If for some reason same source page is copied to multiple destination pages,
                    // only copy attribute value to the first of those pages.
                    var destImagePage = destImagePages.OrderBy(p => p.PageNumber).First();
                    var newPageNum = destImagePage.PageNumber;

                    bool updated = false;
                    if (destImagePage.ImageOrientation != 0)
                    {
                        var newPageInfos = new LongToObjectMapClass();
                        PaginationMethods.RotatePage(
                            newPageNum, destImagePage.ImageOrientation, newPageInfos, page.GetPageInfo(oldPageNum));
                        page.SpatialPageInfos = newPageInfos;
                        updated = true;
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
        /// Translates all spatial <see cref="IAttribute"/> values in <see paramref="attributes"/>
        /// to be associated with the <see paramref="newDocumentName"/> where
        /// <see paramref="pageMap"/> relates each original page to a corresponding
        /// <see cref="ImagePage"/> in <see paramref="newDocumentName"/>.
        /// </summary>
        /// <param name="attributes">The <see cref="IAttribute"/> hierarchy to update.</param>
        /// <param name="newDocumentName">The name of the file with which the attribute values
        /// should now be associated.</param>
        /// <param name="pageMap">Each key represents a tuple of the old document name and page
        /// number while the value represents the new <see cref="ImagePage"/>s in 
        /// <see paramref="newDocumentName"/> associated with that source page.</param>
        /// <param name="newSpatialPageInfos">The new spatial page infos to be associated with this string.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        static void TranslateAttributesToNewDocument(IIUnknownVector attributes,
            string newDocumentName, Dictionary<Tuple<string, int>, List<ImagePage>> pageMap,
            LongToObjectMap newSpatialPageInfos)
        {
            try
            {
                foreach (IAttribute attribute in attributes.ToIEnumerable<IAttribute>())
                {
                    TranslateAttributesToNewDocument(attribute.SubAttributes,
                        newDocumentName, pageMap, newSpatialPageInfos);

                    if (attribute.Value.GetMode() != ESpatialStringMode.kNonSpatialMode)
                    {
                        attribute.Value = TranslateSpatialStringToNewDocument(
                            attribute.Value, newDocumentName, pageMap, newSpatialPageInfos);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47063");
            }
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
        /// number while the value represents the <see cref="ImagePage"/>s in 
        /// <see paramref="newDocumentName"/> associated with that source page.</param>
        /// <returns>The SourceDocName property from <see paramref="value"/>.</returns>
        static string GetSourceDocName(
            SpatialString value, Dictionary<Tuple<string, int>, List<ImagePage>> pageMap)
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