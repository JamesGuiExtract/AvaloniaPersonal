using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Extract.Imaging.Forms;

namespace Extract.DataEntry
{
    #region Enums

    /// <summary>
    /// Specifies how the image viewer zoom/view is adjusted when new fields are selected.
    /// </summary>
    public enum AutoZoomMode
    {
        /// <summary>
        /// If the selected object is not completely visible, the image will be scrolled to place
        /// the center of the object in the center of the screen, but the zoom level will not be
        /// changed (either in or out).
        /// </summary>
        NoZoom = 0,

        /// <summary>
        /// If the selected object is not completely visible, the image will be scrolled. If the
        /// selected object cannot be fit at the current zoom level, the zoom level will be
        /// expanded. For items that can be fit, zoom will be returned as close as possible to the
        /// last manually set zoom level while still displaying the entire selected object.
        /// </summary>
        ZoomOutIfNecessary = 1,

        /// <summary>
        /// Zoom and position will be automatically centered around the current selection with a
        /// user-specified amount of page context displayed around the object.
        /// </summary>
        AutoZoom = 2
    }

    #endregion Enums

    #region IDataEntryApplication

    /// <summary>
    /// Represents application-wide properties and events required by the data enty framework
    /// </summary>
    public interface IDataEntryApplication
    {
        /// <summary>
        /// The title of the current DataEntry application.
        /// </summary>
        string ApplicationTitle
        {
            get;
        }

        /// <summary>
        /// Specifies how the image viewer zoom/view is adjusted when new fields are selected.
        /// </summary>
        AutoZoomMode AutoZoomMode
        {
            get;
        }

        /// <summary>
        /// The page space (context) that should be shown around an object selected when AutoZoom
        /// mode is active. 0 indicates no context space should be shown around the current
        /// selection where 1 indicates the maximum context space should be shown.
        /// </summary>
        double AutoZoomContext
        {
            get;
        }

        /// <summary>
        /// Indicates whether tabbing should allow groups (rows) of attributes to be selected at a
        /// time for controls in which group tabbing is enabled.
        /// </summary>
        bool AllowTabbingByGroup
        {
            get;
        }

        /// <summary>
        /// Gets or sets whether highlights for all data mapped to an <see cref="IDataEntryControl"/>
        /// should be displayed in the <see cref="ImageViewer"/> or whether only highlights relating
        /// to the currently selected fields should be displayed.
        /// </summary>
        bool ShowAllHighlights
        {
            get;
        }

        /// <summary>
        /// Gets or sets the comment for the current file that is stored in the file processing
        /// database.
        /// </summary>
        string DatabaseComment
        {
            get;
            set;
        }

        #region Events

        /// <summary>
        /// This event indicates the value of <see cref="ShowAllHighlights"/> has
        /// changed.
        /// </summary>
        event EventHandler<EventArgs> ShowAllHighlightsChanged;

        #endregion Events
    }

    #endregion IDataEntryApplication
}
