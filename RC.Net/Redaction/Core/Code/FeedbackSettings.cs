using Extract.Interop;
using System;

namespace Extract.Redaction
{
    /// <summary>
	/// Specifies the sets of documents for which feedback should be collected.
	/// </summary>
    [Flags]
	public enum CollectionTypes
	{
		/// <summary>
		/// Don't collect feedback for any documents.
		/// </summary>
		None = 0,
      
		/// <summary>
		/// Collect feedback for documents that contain redactions.
		/// </summary>
		Redacted = 1,
		
		/// <summary>
		/// Collect feedback for documents that contain user corrections.
		/// </summary>
		Corrected = 2,

        /// <summary>
        /// Collect feedback for documents that do not contain redactions and were not corrected 
        /// by the user.
        /// </summary>
        Other = 4,

        /// <summary>
        /// Collect feedback for all documents.
        /// </summary>
        All = 7
	}

    /// <summary>
    /// Represents the settings for collecting feedback about verification.
    /// </summary>
    public class FeedbackSettings
    {
        #region FeedbackSettings Fields

        /// <summary>
        /// <see langword="true"/> if feedback should be collected; 
        /// <see langword="false"/> if it should not.
        /// </summary>
        readonly bool _collectFeedback;

        /// <summary>
        /// The path to ID Shield data file folder. May contain tags.
        /// </summary>
        readonly string _dataFolder;

        /// <summary>
        /// <see langword="true"/> if the original document should be collected; 
        /// <see langword="false"/> if the original document should not be collected.
        /// </summary>
        readonly bool _collectOriginalDocument;

        /// <summary>
        /// <see langword="true"/> if original file names should be used; <see langword="false"/> 
        /// if unique file names should be generated.
        /// </summary>
        readonly bool _useOriginalFileNames;

        /// <summary>
        /// Describes the documents for which feedback should be collected.
        /// </summary>
        readonly CollectionTypes _collectionTypes;

        #endregion FeedbackSettings Fields

        #region FeedbackSettings Constructors

        /// <summary>
	    /// Initializes a new instance of the <see cref="FeedbackSettings"/> class with default 
        /// settings.
	    /// </summary>
	    public FeedbackSettings() : this(false, null, true, true, CollectionTypes.All)
	    {

	    }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackSettings"/> class.
        /// </summary>
        public FeedbackSettings(bool collectFeedback, string dataFolder, 
            bool collectOriginalDocument, bool useOriginalFileNames, CollectionTypes collectionTypes)
        {
            _collectFeedback = collectFeedback;
            _dataFolder = dataFolder ?? @"$DirOf(<SourceDocName>)\FeedbackData";
            _collectOriginalDocument = collectOriginalDocument;
            _useOriginalFileNames = useOriginalFileNames;
            _collectionTypes = collectionTypes;
        }

        #endregion FeedbackSettings Constructors

        #region FeedbackSettings Properties

        /// <summary>
        /// Gets whether feedback should be collected.
        /// </summary>
        /// <returns><see langword="true"/> if feedback should be collected;
        /// <see langword="false"/> if it should not be collected.</returns>
        public bool Collect
        {
            get
            {
                return _collectFeedback;
            }
        }

        /// <summary>
        /// Gets the path to the ID Shield data file folder. May contain tags.
        /// </summary>
        /// <returns>The path to the ID Shield data file folder. May contain tags.</returns>
        public string DataFolder
        {
            get
            {
                return _dataFolder;
            }
        }

        /// <summary>
        /// Gets whether the original document should be collected.
        /// </summary>
        /// <returns><see langword="true"/> if the original document should be collected;
        /// <see langword="false"/> if the original document should not be collected.</returns>
        public bool CollectOriginalDocument
        {
            get
            {
                return _collectOriginalDocument;
            }
        }

        /// <summary>
        /// Gets whether to use original file names.
        /// </summary>
        /// <returns><see langword="true"/> if original file names should be used;
        /// <see langword="false"/> if unique file names should be generated.</returns>
        public bool UseOriginalFileNames
        {
            get
            {
                return _useOriginalFileNames;
            }
        }

        /// <summary>
        /// Gets the document types for which feedback should be collected.
        /// </summary>
        /// <returns>The document types for which feedback should be collected.</returns>
        public CollectionTypes CollectionTypes
        {
            get
            {
                return _collectionTypes;
            }
        }

        #endregion FeedbackSettings Properties

        #region FeedbackSettings Methods

        /// <summary>
        /// Creates a <see cref="FeedbackSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="FeedbackSettings"/>.</param>
        /// <returns>A <see cref="FeedbackSettings"/> created from the specified 
        /// <see cref="IStreamReader"/>.</returns>
        public static FeedbackSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                bool collectFeedback = reader.ReadBoolean();
                string dataFolder = reader.ReadString();
                bool collectOriginalDocument = reader.ReadBoolean();
                bool useOriginalFileNames = reader.ReadBoolean();
                CollectionTypes collectionTypes = (CollectionTypes)reader.ReadInt32();

                return new FeedbackSettings(collectFeedback, dataFolder, collectOriginalDocument,
                    useOriginalFileNames, collectionTypes);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26514",
                    "Unable to read verification feedback settings.", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="FeedbackSettings"/> to the specified 
        /// <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="writer">The writer into which the 
        /// <see cref="FeedbackSettings"/> will be written.</param>
        public void WriteTo(IStreamWriter writer)
        {
            try
            {
                writer.Write(_collectFeedback);
                writer.Write(_dataFolder);
                writer.Write(_collectOriginalDocument);
                writer.Write(_useOriginalFileNames);
                writer.Write((int)_collectionTypes);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI26515",
                    "Unable to write verification feedback settings.", ex);
            }
        }

        #endregion FeedbackSettings Methods
    }
}
