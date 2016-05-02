using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        /// number while the value represents the new page number in 
        /// <see paramref="newDocumentName"/>.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static void TranslateAttributesToNewDocument(IIUnknownVector attributes,
            string newDocumentName, Dictionary<Tuple<string, int>, int> pageMap)
        {
            try
            {
                foreach (IAttribute attribute in attributes.ToIEnumerable<IAttribute>())
                {
                    TranslateAttributesToNewDocument(attribute.SubAttributes,
                        newDocumentName, pageMap);

                    SpatialString value = attribute.Value;
                    if (value.GetMode() == ESpatialStringMode.kSpatialMode)
                    {
                        attribute.Value = TranslateSpatialStringToNewDocument(
                            value, newDocumentName, pageMap);
                    }
                    else if (value.GetMode() == ESpatialStringMode.kHybridMode)
                    {
                        attribute.Value = TranslateHybridStringToNewDocument(
                            value, newDocumentName, pageMap);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39708");
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
        /// number while the value represents the new page number in 
        /// <see paramref="newDocumentName"/>.</param>
        static SpatialString TranslateSpatialStringToNewDocument(SpatialString value,
            string newDocumentName, Dictionary<Tuple<string, int>, int> pageMap)
        {
            ExtractException.Assert("ELI39709", "Unexpected spatial mode.",
                value.GetMode() == ESpatialStringMode.kSpatialMode);

            string sourceDocName = value.SourceDocName;

            var updatedPages = new List<SpatialString>();
            foreach (SpatialString page in value.GetPages(false, "")
                .ToIEnumerable<SpatialString>())
            {
                int oldPageNum = page.GetFirstPageNumber();

                int newPageNum;
                if (pageMap.TryGetValue(new Tuple<string, int>(sourceDocName, oldPageNum), 
                        out newPageNum))
                {
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
        /// number while the value represents the new page number in 
        /// <see paramref="newDocumentName"/>.</param>
        static SpatialString TranslateHybridStringToNewDocument(SpatialString value,
            string newDocumentName, Dictionary<Tuple<string, int>, int> pageMap)
        {
            ExtractException.Assert("ELI39710", "Unexpected spatial mode.",
                value.GetMode() == ESpatialStringMode.kHybridMode);

            string sourceDocName = value.SourceDocName;

            var updatedRasterZones = new List<IRasterZone>();
            var updatedPageInfoMap = new LongToObjectMap();
            foreach (IRasterZone rasterZone in value.GetOCRImageRasterZones()
                .ToIEnumerable<IRasterZone>())
            {
                int oldPageNum = rasterZone.PageNumber;
                int newPageNum;
                if (pageMap.TryGetValue(new Tuple<string, int>(sourceDocName, oldPageNum), 
                        out newPageNum))
                {
                    rasterZone.PageNumber = newPageNum;
                    updatedRasterZones.Add(rasterZone);
                    if (!updatedPageInfoMap.Contains(newPageNum))
                    {
                        var copyable = (ICopyableObject)value.GetPageInfo(oldPageNum);
                        var newPageInfo = (SpatialPageInfo)copyable.Clone();

                        updatedPageInfoMap.Set(newPageNum, newPageInfo);
                    }
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
                    value.String, newDocumentName, updatedPageInfoMap);
            }
            return updatedValue;
        }

        #endregion Private Methods
    }
}