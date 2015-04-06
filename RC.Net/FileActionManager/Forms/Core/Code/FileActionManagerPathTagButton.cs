using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.ComponentModel;

namespace Extract.FileActionManager.Forms
{
    /// <summary>
    /// Represents a button with a drop down that allows the user to select
    /// <see cref="FileActionManagerPathTags"/>.
    /// </summary>
    [CLSCompliant(false)]
    public class FileActionManagerPathTagButton : PathTagsButton
    {
        #region Fields

        /// <summary>
        /// The list of document tags to be displayed in the context menu drop down.
        /// </summary>
        IPathTags _pathTags;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileActionManagerPathTagButton"/> class.
        /// </summary>
        public FileActionManagerPathTagButton()
            : base()
        {
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Gets or sets the path tags that are available for selection.
        /// </summary>
        /// <value>The path tags that are available for selection.</value>
        /// <returns>The path tags that are available for selection.</returns>
        [Category("Behavior")]
        [Description("The path tags that are available for selection.")]
        public override IPathTags PathTags
        {
            get
            {
                if (_pathTags == null)
                {
                    _pathTags = new FileActionManagerPathTags();
                }

                return _pathTags;
            }
            set
            {
                _pathTags = value;
            }
        }

        #endregion Overrides
    }
}
