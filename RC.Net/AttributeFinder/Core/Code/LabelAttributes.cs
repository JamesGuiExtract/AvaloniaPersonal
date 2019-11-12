using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using UCLID_AFUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using ComAttribute = UCLID_AFCORELib.Attribute;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Settings and method to label attributes based on queries and spatial matching
    /// </summary>
    [Serializable]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    [CLSCompliant(false)]
    public class LabelAttributes
    {
        #region Constants

        /// <summary>
        /// Current version.
        /// Version 2: Add OnlyIfAllCategoriesMatchOnSamePage property and backing field
        /// </summary>
        const int _CURRENT_VERSION = 2;

        #endregion Constants

        #region fields

        // Use one af utility object per thread to avoid COM issues
        private static ThreadLocal<IAFUtility> _aFUtility = new ThreadLocal<IAFUtility>(() => new AFUtilityClass());

        private List<CategoryQueryPair> _categoryQueryPairs = new List<CategoryQueryPair>();
        private string _attributesToLabelPath;
        private string _sourceOfLabelsPath;
        private string _destinationPath;
        private bool _createEmptylabelForNonMatching;

        [OptionalField(VersionAdded = 2)]
        private bool _onlyIfAllCategoriesMatchOnSamePage;

        /// <summary>
        /// Persist the current version in case it is needed (but currently no check is done to avoid
        /// breaking compatibility of the associated LearningMachine for the sake of this utility)
        /// </summary>
        [OptionalField(VersionAdded = 2)]
        private int _version = _CURRENT_VERSION;

        #endregion fields

        #region Properties

        /// <summary>
        /// Gets the list of category query pairs
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<CategoryQueryPair> CategoryQueryPairs
        {
            get
            {
                return _categoryQueryPairs;
            }
        }

        /// <summary>
        /// Gets or sets the path of the attributes to label VOA
        /// </summary>
        /// <remarks>
        /// This should be a path tag/function of &lt;SourceDocName&gt;
        /// </remarks>
        public string AttributesToLabelPath
        {
            get
            {
                return _attributesToLabelPath;
            }
            set
            {
                _attributesToLabelPath = string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        /// <summary>
        /// Gets or sets the source of labels VOA path.
        /// </summary>
        /// <remarks>
        /// This should be a path tag/function of &lt;SourceDocName&gt;
        /// </remarks>
        public string SourceOfLabelsPath
        {
            get
            {
                return _sourceOfLabelsPath;
            }
            set
            {
                _sourceOfLabelsPath = string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        /// <summary>
        /// Gets or sets the destination path for the VO_labeled_A
        /// </summary>
        /// <remarks>
        /// This should be a path tag/function of &lt;SourceDocName&gt;
        /// </remarks>
        public string DestinationPath
        {
            get
            {
                return _destinationPath;
            }
            set
            {
                _destinationPath = string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to create empty labels for non matching attributes
        /// </summary>
        public bool CreateEmptyLabelForNonMatching
        {
            get
            {
                return _createEmptylabelForNonMatching;
            }
            set
            {
                _createEmptylabelForNonMatching = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to only create empty labels if all non-empty categories
        /// match on the same page as the non-matching attribute
        /// </summary>
        public bool OnlyIfAllCategoriesMatchOnSamePage
        {
            get
            {
                return _onlyIfAllCategoriesMatchOnSamePage;
            }
            set
            {
                _onlyIfAllCategoriesMatchOnSamePage = value;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Creates a new instance that is a deep clone of this instance
        /// </summary>
        /// <returns>A deep clone of this instance</returns>
        public LabelAttributes DeepClone()
        {
            try
            {
                var clone = (LabelAttributes)MemberwiseClone();
                clone._categoryQueryPairs = CategoryQueryPairs
                                            .Select(p => p.ShallowClone())
                                            .ToList();
                return clone;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI41435");
            }
        }

        /// <summary>
        /// Processes the specified VOAs
        /// </summary>
        /// <param name="inputConfig">The input configuration used to drive the process</param>
        public void Process(InputConfiguration inputConfig)
        {
            Process(inputConfig, _ => { }, CancellationToken.None);
        }

        /// <summary>
        /// Processes the specified VOAs to add labels
        /// </summary>
        /// <param name="inputConfig">The input configuration used to drive the process</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        public void Process(InputConfiguration inputConfig,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                var threadLocalPathTags = new ThreadLocal<AttributeFinderPathTags>(() => new AttributeFinderPathTags());
                var imagePathsAndMaybeAnswers = inputConfig.GetImagePaths(updateStatus, cancellationToken);
                string[] imageFiles = imagePathsAndMaybeAnswers.Item1;

                var opts = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                Parallel.For(0, imageFiles.Length, opts, (i, loopState) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        loopState.Stop();
                    }

                    string imagePath = imageFiles[i];
                    var pathTags = threadLocalPathTags.Value;
                    pathTags.Document = new AFDocumentClass { Text = new SpatialStringClass { SourceDocName = imagePath } };

                    try
                    {
                        var labeled = LabelAttributesVector(imagePath, pathTags, cancellationToken);
                        var destinationPath = pathTags.Expand(DestinationPath);
                        labeled.SaveTo(destinationPath, true, typeof(AttributeStorageManagerClass).GUID.ToString("B"));
                    }
                    catch (OperationCanceledException)
                    {
                        loopState.Stop();
                    }

                    updateStatus(new StatusArgs { StatusMessage = "Files processed: {0:N0}", Int32Value = 1 });
                });

                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI41463");
            }
        }

        /// <summary>
        /// Labels the input <see paramref="attributesToLabel"/> attributes by creating AttributeType subattributes
        /// based on spatial comparison against the <see paramref="sourceOfLabels"/> attributes
        /// </summary>
        /// <param name="imagePath">Path used for the source doc name of created label attributes</param>
        /// <param name="attributesToLabel">The attributes to be labeled</param>
        /// <param name="sourceOfLabels">The attributes to be used to generate the labels</param>
        /// <param name="cancellationToken">Used to cancel processing</param>
        public void LabelAttributesVector(string imagePath, IIUnknownVector attributesToLabel,
            IUnknownVector sourceOfLabels, CancellationToken cancellationToken)
        {
            try
            {
                var sourceOfLabelsContext = new XPathContext(sourceOfLabels);
                foreach (var attributeToLabel in attributesToLabel.ToIEnumerable<ComAttribute>())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    LabelAttribute(attributeToLabel, sourceOfLabelsContext, imagePath, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45786");
            }
        }

        #endregion Public Methods

        #region Private Methods

        IUnknownVector LabelAttributesVector(string imagePath, PathTagsBase pathTags,
            CancellationToken cancellationToken)
        {
            var afUtil = _aFUtility.Value;

            var toLabelPath = pathTags.Expand(AttributesToLabelPath);
            var sourceOfLabelsPath = pathTags.Expand(SourceOfLabelsPath);

            var attributesToLabel = afUtil.GetAttributesFromFile(toLabelPath);
            attributesToLabel.ReportMemoryUsage();
            var sourceOfLabelsVOA = afUtil.GetAttributesFromFile(sourceOfLabelsPath);
            sourceOfLabelsVOA.ReportMemoryUsage();

            LabelAttributesVector(imagePath, attributesToLabel, sourceOfLabelsVOA, cancellationToken);
            return attributesToLabel;
        }

        private void LabelAttribute(ComAttribute attributeToLabel, XPathContext sourceOfLabels, string sourceDocName,
            CancellationToken cancellationToken)
        {
            // Find first category definition where there is a spatial match
            ComAttribute sourceOfLabel = null;
            bool foundMatch = false;
            var match = CategoryQueryPairs.Find(pair =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    sourceOfLabel = sourceOfLabels
                        .FindAllOfType<ComAttribute>(pair.Query)
                        .FirstOrDefault(candidate => HasSpatialOverlap(attributeToLabel, candidate));
                    foundMatch = sourceOfLabel != null;
                    return foundMatch;
                });

            bool foundMatchesForAllOnPage = false;
            if (!foundMatch && OnlyIfAllCategoriesMatchOnSamePage)
            {
                foundMatchesForAllOnPage = CategoryQueryPairs
                    .Where(pair => !string.IsNullOrEmpty(pair.Category))
                    .All(pair =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    return sourceOfLabels
                        .FindAllOfType<ComAttribute>(pair.Query)
                        .Any(candidate => AreOnSamePage(attributeToLabel, candidate));
                });
            }

            // Remove any existing labels
            _aFUtility.Value.QueryAttributes(attributeToLabel.SubAttributes,
                LearningMachineDataEncoder.CategoryAttributeName, bRemoveMatches: true);

            // Label the attribute
            if (foundMatch
                || CreateEmptyLabelForNonMatching
                    && (!OnlyIfAllCategoriesMatchOnSamePage || foundMatchesForAllOnPage))
            {
                var category = string.Empty;
                if (foundMatch)
                {
                    category = match.Category;

                    if (match.CategoryIsXPath)
                    {
                        category = sourceOfLabels
                            .FindAllAsStrings(category, sourceOfLabel)
                            .FirstOrDefault();
                    }
                }

                var value = new SpatialString();
                value.CreateNonSpatialString(category, sourceDocName);
                var label = new ComAttribute
                {
                    Name = LearningMachineDataEncoder.CategoryAttributeName,
                    Value = value
                };
                attributeToLabel.SubAttributes.PushBack(label);
            }
        }

        private static bool HasSpatialOverlap(ComAttribute a, ComAttribute b, double overlapThreshold = 0.5)
        {
            if (!(a.Value.HasSpatialInfo() && b.Value.HasSpatialInfo()))
            {
                return false;
            }
            else
            {
                var aZones = a.Value.GetOriginalImageRasterZones().ToIEnumerable<RasterZone>();
                var bZones = b.Value.GetOriginalImageRasterZones().ToIEnumerable<RasterZone>();

                return aZones.Any(aZone =>
                    bZones.Any(bZone =>
                    {
                        var overlapArea = aZone.GetAreaOverlappingWith(bZone);
                        var aArea = aZone.Area;
                        var bArea = bZone.Area;
                        var overlapRatio1 = overlapArea / aArea;
                        var overlapRatio2 = overlapArea / bArea;
                        return overlapRatio1 >= overlapThreshold
                            || overlapRatio2 >= overlapThreshold;
                    }));
            }
        }

        private static bool AreOnSamePage(ComAttribute a, ComAttribute b)
        {
            if (!(a.Value.HasSpatialInfo() && b.Value.HasSpatialInfo()))
            {
                return false;
            }
            else
            {
                return a.Value.GetFirstPageNumber() == b.Value.GetFirstPageNumber()
                    && a.Value.GetLastPageNumber() == b.Value.GetLastPageNumber();
            }
        }

        /// <summary>
        /// Called when deserializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            _onlyIfAllCategoriesMatchOnSamePage = false;
        }

        /// <summary>
        /// Called when deserialized
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_version > _CURRENT_VERSION)
            {
                (new ExtractException("ELI41827", "Loaded machine has a newer version of LabelAttributes settings. Some settings may not be correct."))
                    .Display();
            }
            _version = _CURRENT_VERSION;
        }

        #endregion Private Methods

        #region Overrides

        /// <summary>
        /// Whether this instance has equal property values to another
        /// </summary>
        /// <param name="obj">The instance to compare with</param>
        /// <returns><c>true</c> if this instance has equal property values, else <c>false<c/></returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as LabelAttributes;
            if (other == null
                || other.AttributesToLabelPath != AttributesToLabelPath
                || other.SourceOfLabelsPath != SourceOfLabelsPath
                || other.DestinationPath != DestinationPath
                || other.CreateEmptyLabelForNonMatching != CreateEmptyLabelForNonMatching
                || other.OnlyIfAllCategoriesMatchOnSamePage != OnlyIfAllCategoriesMatchOnSamePage
                || !other.CategoryQueryPairs.SequenceEqual(CategoryQueryPairs))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the hash code for this object
        /// </summary>
        /// <returns>The hash code for this object</returns>
        public override int GetHashCode()
        {
            var hash = HashCode.Start
                .Hash(AttributesToLabelPath)
                .Hash(SourceOfLabelsPath)
                .Hash(DestinationPath)
                .Hash(CreateEmptyLabelForNonMatching)
                .Hash(OnlyIfAllCategoriesMatchOnSamePage);

            foreach(var categoryQueryPair in CategoryQueryPairs)
            {
                hash = hash.Hash(categoryQueryPair);
            }

            return hash;
        }

        #endregion Overrides
    }
}