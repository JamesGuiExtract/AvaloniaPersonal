using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Redaction.Verification
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
            _dataFolder = dataFolder ?? @"$DirOf(<SourceDocName>)\ExpectedRedactions";
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
    }
}
