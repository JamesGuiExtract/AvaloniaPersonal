using System;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents settings for the <see cref="VerificationTaskForm"/> user interface.
    /// </summary>
    public class VerificationOptions
    {
        #region VerificationOptions Fields

        readonly bool _autoZoom;
        readonly int _autoZoomScale;
        readonly AutoTool _autoTool;

        #endregion VerificationOptions Fields

        #region VerificationOptions Constructors

        /// <overloads>Initializes a new instance of the <see cref="VerificationOptions"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationOptions"/> class with default 
        /// settings.
        /// </summary>
        public VerificationOptions()
            : this(false, 3, AutoTool.None)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VerificationOptions"/>.
        /// </summary>
        public VerificationOptions(bool autoZoom, int autoZoomScale, AutoTool autoTool)
        {
            _autoZoom = autoZoom;
            _autoZoomScale = autoZoomScale;
            _autoTool = autoTool;
        }

        #endregion VerificationOptions Constructors

        #region VerificationOptions Properties

        /// <summary>
        /// Gets whether to automatically set the zoom setting when redactions are selected.
        /// </summary>
        /// <value><see langword="true"/> if the zoom settings should be automatically adjusted 
        /// when a redaction is selected; <see langword="false"/> if it should not.</value>
        public bool AutoZoom
        {
            get
            {
                return _autoZoom;
            }
        }

        /// <summary>
        /// Gets the zoom setting when a redaction is selected and <see cref="AutoZoom"/> is 
        /// <see langword="true"/>.
        /// </summary>
        /// <value>The zoom setting when a redaction is selected and <see cref="AutoZoom"/> is 
        /// <see langword="true"/>.</value>
        public int AutoZoomScale
        {
            get
            {
                return _autoZoomScale;
            }
        }

        /// <summary>
        /// Gets the <see cref="Extract.Imaging.Forms.CursorTool"/> that is automatically selected 
        /// when a redaction has been manually added.
        /// </summary>
        /// <value>The <see cref="Extract.Imaging.Forms.CursorTool"/> that is automatically 
        /// selected when a redaction has been manually added.</value>
        public AutoTool AutoTool
        {
            get
            {
                return _autoTool;
            }
        }

        #endregion VerificationOptions Properties

        #region VerificationOptions Methods

        /// <summary>
        /// Creates <see cref="VerificationOptions"/> from the specified 
        /// <see cref="InitializationSettings"/>.
        /// </summary>
        /// <param name="settings">The settings from which to create the options.</param>
        /// <returns>Options created from the specified <paramref name="settings"/>.</returns>
        public static VerificationOptions ReadFrom(InitializationSettings settings)
        {
            try
            {
                bool autoZoom = settings.AutoZoom;
                int autoZoomScale = settings.AutoZoomScale;
                AutoTool autoTool = settings.AutoTool;

                return new VerificationOptions(autoZoom, autoZoomScale, autoTool);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27720",
                    "Unable to read verification options.", ex);
            }
        }

        /// <summary>
        /// Writes the <see cref="VerificationOptions"/> to the specified 
        /// <see cref="InitializationSettings"/>.
        /// </summary>
        /// <param name="settings">The settings from which to create the options.</param>
        /// <returns>Options created from the specified <paramref name="settings"/>.</returns>
        public void WriteTo(InitializationSettings settings)
        {
            try
            {
                settings.AutoZoom = _autoZoom;
                settings.AutoZoomScale = _autoZoomScale;
                settings.AutoTool = _autoTool;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27721",
                    "Unable to write verification options.", ex);
            }
        }

        #endregion VerificationOptions Methods
    }
}
