using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UCLID_FILEPROCESSINGLib;
using Extract.Interop;
using UCLID_COMUTILSLib;
using System.Runtime.InteropServices.ComTypes;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents the slideshow settings for redaction verification.
    /// </summary>
    public class SlideshowSettings
    {
        #region Constants

        /// <summary>
        /// The current version number for this object
        /// </summary>
        const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates whether the slideshow feature is enabled for use.
        /// </summary>
        readonly bool _slideshowEnabled;

        /// <summary>
        /// Indicates whether a tag should be applied to documents in which at least one page was
        /// auto-advanced.
        /// </summary>
        readonly bool _applyAutoAdvanceTag;

        /// <summary>
        /// Indicates the name of the tag that should be applied to documents in which at least one
        /// page was auto-advanced.
        /// </summary>
        readonly string _autoAdvanceTag;

        /// <summary>
        /// Indicates whether an <see cref="EActionStatus"/> of an action should be should be
        /// changed on documents in which at least one page was auto-advanced.
        /// </summary>
        readonly bool _applyAutoAdvanceActionStatus;

        /// <summary>
        /// Indicates the name of the action in which action status be should be changed.
        /// </summary>
        readonly string _autoAdvanceActionName;

        /// <summary>
        /// <see cref="EActionStatus"/> to which the action should be set when auto-advancing.
        /// </summary>
        readonly EActionStatus _autoAdvanceActionStatus;

        /// <summary>
        /// Encapsulates the settings regarding changing the <see cref="EActionStatus"/> for
        /// auto-advanced documents.
        /// </summary>
        SetFileActionStatusSettings _autoAdvanceSetFileActionStatusSettings;

        /// <summary>
        /// Indicates whether the slideshow should be automatically paused on documents which match
        /// the specified <see cref="IFAMCondition"/>.
        /// </summary>
        readonly bool _checkDocumentCondition;

        /// <summary>
        /// The <see cref="IFAMCondition"/> that determines whether documents should be
        /// paused.
        /// </summary>
        readonly ObjectWithDescription _documentCondition;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SlideshowSettings"/> class.
        /// </summary>
        public SlideshowSettings()
            : this(true, false, string.Empty, false, string.Empty, EActionStatus.kActionPending,
                false, new ObjectWithDescriptionClass())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SlideshowSettings"/> class.
        /// </summary>
        /// <param name="slideshowEnabled"><see langword="true"/> if the slideshow is enabled.
        /// </param>
        /// <param name="applyAutoAdvanceTag"><see langword="true"/> to apply a tag to
        /// auto-advanced documents.</param>
        /// <param name="autoAdvanceTag">The name of the tag to be applied.</param>
        /// <param name="applyAutoAdvanceActionStatus"><see langword="true"/> to change an
        /// <see cref="EActionStatus"/> for auto-advanced documents.</param>
        /// <param name="autoAdvanceActionName">Name of the action for which a new
        /// <see cref="EActionStatus"/> should be applied.</param>
        /// <param name="autoAdvanceActionStatus"><see cref="EActionStatus"/> to which the action
        /// should be set.</param>
        /// <param name="checkDocTypeCondition"><see langword="true"/> to automatically pause the
        /// slideshow for documents which match <see paramref="documentCondition"/>.</param>
        /// <param name="documentCondition">The <see cref="IFAMCondition"/> that determines for
        /// which documents the slideshow should be paused.</param>
        [CLSCompliant(false)]
        public SlideshowSettings(bool slideshowEnabled, bool applyAutoAdvanceTag,
            string autoAdvanceTag, bool applyAutoAdvanceActionStatus, string autoAdvanceActionName,
            EActionStatus autoAdvanceActionStatus, bool checkDocTypeCondition,
            ObjectWithDescription documentCondition)
        {
            try
            {
                _slideshowEnabled = slideshowEnabled;
                _applyAutoAdvanceTag = applyAutoAdvanceTag;
                _autoAdvanceTag = autoAdvanceTag;
                _applyAutoAdvanceActionStatus = applyAutoAdvanceActionStatus;
                _autoAdvanceActionName = autoAdvanceActionName;
                _autoAdvanceActionStatus = autoAdvanceActionStatus;
                _checkDocumentCondition = checkDocTypeCondition;
                ICopyableObject copyableDocumentCondition = (ICopyableObject)documentCondition;
                _documentCondition = (ObjectWithDescription)copyableDocumentCondition.Clone();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31126", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the whether the slideshow feature is enabled for use.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the slideshow feature is enabled; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool SlideshowEnabled
        {
            get
            {
                return _slideshowEnabled;
            }
        }

        /// <summary>
        /// Gets whether a tag should be applied to documents in which at least one page was
        /// auto-advanced.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if a tag should be applied to documents in which at least one
        /// page was auto-advanced; otherwise, <see langword="false"/>.
        /// </value>
        public bool ApplyAutoAdvanceTag
        {
            get
            {
                return _applyAutoAdvanceTag;
            }
        }

        /// <summary>
        /// Gets the name of the tag that should be applied to documents in which at least one page
        /// was auto-advanced.
        /// </summary>
        /// <value>The name of the tag that should be applied.</value>
        public string AutoAdvanceTag
        {
            get
            {
                return _autoAdvanceTag;
            }
        }

        /// <summary>
        /// Gets whether an <see cref="EActionStatus"/> of an action should be should be changed
        /// on documents in which at least one page was auto-advanced.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if an <see cref="EActionStatus"/> is to be applied; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool ApplyAutoAdvanceActionStatus
        {
            get
            {
                return _applyAutoAdvanceActionStatus;
            }
        }

        /// <summary>
        /// Gets the the name of the action in which action status be should be changed.
        /// </summary>
        /// <value>The name of the action in which action status be should be changed.</value>
        public string AutoAdvanceActionName
        {
            get
            {
                return _autoAdvanceActionName;
            }
        }

        /// <summary>
        /// Gets the <see cref="EActionStatus"/> to which the action should be set when
        /// auto-advancing.
        /// </summary>
        /// <value>The <see cref="EActionStatus"/> to which the action should be set when
        /// auto-advancing.</value>
        [CLSCompliant(false)]
        public EActionStatus AutoAdvanceActionStatus
        {
            get
            {
                return _autoAdvanceActionStatus;
            }
        }

        /// <summary>
        /// Gets whether the slideshow should be automatically paused on documents which match the
        /// specified <see cref="IFAMCondition"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the slideshow should be automatically paused for matching
        /// documents; otherwise, <see langword="false"/>.
        /// </value>
        public bool CheckDocumentCondition
        {
            get
            {
                return _checkDocumentCondition;
            }
        }

        /// <summary>
        /// Gets the <see cref="IFAMCondition"/> that determines whether documents should be paused.
        /// </summary>
        /// <value>The <see cref="IFAMCondition"/> that determines whether documents should be
        /// paused.</value>
        [CLSCompliant(false)]
        public ObjectWithDescription DocumentCondition
        {
            get
            {
                return _documentCondition;
            }
        }

        /// <summary>
        /// Gets the settings regarding changing the <see cref="EActionStatus"/> for
        /// auto-advanced documents.
        /// </summary>
        /// <value>The <see cref="SetFileActionStatusSettings"/> instance.</value>
        public SetFileActionStatusSettings AutoAdvanceSetActionStatusSettings
        {
            get
            {
                if (_autoAdvanceSetFileActionStatusSettings == null)
                {
                    _autoAdvanceSetFileActionStatusSettings = new SetFileActionStatusSettings(
                        _applyAutoAdvanceActionStatus, _autoAdvanceActionName, 
                        _autoAdvanceActionStatus);
                }

                return _autoAdvanceSetFileActionStatusSettings;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a <see cref="SlideshowSettings"/> from the specified 
        /// <see cref="IStreamReader"/>.
        /// </summary>
        /// <param name="reader">The reader from which to create the 
        /// <see cref="SlideshowSettings"/>.</param>
        /// <returns>A <see cref="SlideshowSettings"/> created from the specified 
        /// <see cref="IStreamReader"/>.</returns>
        public static SlideshowSettings ReadFrom(IStreamReader reader)
        {
            try
            {
                int version = reader.ReadInt32();
                if (version > _CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI31127",
                        "Cannot load newer version of SlideshowSettings");
                    ee.AddDebugData("Current Version", _CURRENT_VERSION, false);
                    ee.AddDebugData("Version To Load", version, false);
                    throw ee;
                }

                bool slideshowEnabled = reader.ReadBoolean();
                bool applyAutoAdvanceTag = reader.ReadBoolean();
                string autoAdvanceTag = reader.ReadString();
                bool applyAutoAdvanceActionStatus = reader.ReadBoolean();
                string autoAdvanceActionName = reader.ReadString();
                EActionStatus autoAdvanceActionStatus = (EActionStatus)reader.ReadInt32();
                bool applyDocTypeCondition = reader.ReadBoolean();
                ObjectWithDescription documentCondition =
                    (ObjectWithDescription)reader.ReadIPersistStream();

                return new SlideshowSettings(slideshowEnabled, applyAutoAdvanceTag, autoAdvanceTag,
                    applyAutoAdvanceActionStatus, autoAdvanceActionName, autoAdvanceActionStatus,
                    applyDocTypeCondition, documentCondition);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI31036", "Unable to read slideshow settings.", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="SlideshowSettings"/> to the specified 
        /// <see cref="IStreamWriter"/>.
        /// </summary>
        /// <param name="writer">The writer into which the 
        /// <see cref="SlideshowSettings"/> will be written.</param>
        public void WriteTo(IStreamWriter writer)
        {
            try
            {
                writer.Write(_CURRENT_VERSION);
                writer.Write(_slideshowEnabled);
                writer.Write(_applyAutoAdvanceTag);
                writer.Write(_autoAdvanceTag);
                writer.Write(_applyAutoAdvanceActionStatus);
                writer.Write(_autoAdvanceActionName);
                writer.Write((int)_autoAdvanceActionStatus);
                writer.Write(_checkDocumentCondition);
                writer.Write((IPersistStream)_documentCondition, true);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI31035", "Unable to write slideshow settings.", ex);
            }
        }

        #endregion Methods
    }
}
