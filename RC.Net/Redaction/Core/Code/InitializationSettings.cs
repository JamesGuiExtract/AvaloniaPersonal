using Extract.Imaging.Forms;
using Extract.Utilities;
using System;
using System.Drawing;
using System.Globalization;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents ID Shield settings read from an initialization file (ini).
    /// </summary>
    public class InitializationSettings
    {
        #region Constants

        const string _GENERAL_SECTION = "General";

        const string _LEVEL_SECTION_PREFIX = "Level";

        const string _AUTO_ZOOM_KEY = "AutoZoom";

        const string _AUTO_ZOOM_SCALE_KEY = "AutoZoomScale";

        const string _AUTO_TOOL_KEY = "AutoPan";

        const string _REDACTION_COLOR_KEY = "RedactionColorInOutputFile";

        const string _LEVEL_COUNT_KEY = "NumConfidenceLevels";

        const string _SHORT_NAME_KEY = "ShortName";

        const string _QUERY_KEY = "Query";

        const string _COLOR_KEY = "Color";

        const string _OUTPUT_KEY = "Output";

        const string _WARN_IF_REDACT = "WarnIfRedact";

        const string _WARN_IF_NON_REDACT = "WarnIfNonRedact";

        const string _REDACTION_TYPES_SECTION = "RedactionDataTypes";

        const string _REDACTION_TYPE_COUNT_KEY = "NumRedactionDataTypes";

        const string _REDACTION_TYPE_KEY_PREFIX = "RedactionDataType";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Initialization (ini) file containing the settings.
        /// </summary>
        readonly InitializationFile _iniFile = new InitializationFile("IDShield.ini");

        /// <summary>
        /// The confidence levels of ID Shield attributes.
        /// </summary>
        readonly ConfidenceLevelsCollection _levels;

        /// <summary>
        /// A list of valid redaction types.
        /// </summary>
        readonly string[] _types;

        /// <summary>
        /// <see langword="true"/> if selected attributes should be displayed at a particular 
        /// zoom setting; <see langword="false"/> if zoom setting should be unchanged when 
        /// displaying selected attributes.
        /// </summary>
        bool _autoZoom;

        /// <summary>
        /// The scale at which selected attributes should be displayed if <see cref="_autoZoom"/> 
        /// is <see langword="true"/>.
        /// </summary>
        int _autoZoomScale;

        /// <summary>
        /// The tool to automatically select after the user manually creates a redaction.
        /// </summary>
        AutoTool _autoTool;

        /// <summary>
        /// The color of redactions in redacted images.
        /// </summary>
        readonly RedactionColor _outputRedactionColor;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializationSettings"/> class.
        /// </summary>
        public InitializationSettings()
        {
            try
            {
                _levels = GetConfidenceLevels(_iniFile);
                _types = GetRedactionTypes(_iniFile);
                _autoZoom = GetAutoZoom(_iniFile);
                _autoZoomScale = GetAutoZoomScale(_iniFile);
                _autoTool = GetAutoTool(_iniFile);
                _outputRedactionColor = GetRedactionColor(_iniFile);
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI27719",
                    "Unable to read IDShield settings.", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the confidence levels of ID Shield attributes.
        /// </summary>
        /// <value>The confidence levels of ID Shield attributes.</value>
        public ConfidenceLevelsCollection ConfidenceLevels
        {
            get
            {
                return _levels;
            }
        }

        /// <summary>
        /// Get or sets whether selected attributes should be displayed at a particular zoom setting.
        /// </summary>
        /// <value><see langword="true"/> if selected attributes should be displayed at a 
        /// particular zoom setting; <see langword="false"/> if the zoom setting should be 
        /// unchanged when displaying selected attributes.</value>
        public bool AutoZoom
        {
            get
            {
                return _autoZoom;
            }
            set
            {
                _iniFile.WriteInt32(_GENERAL_SECTION, _AUTO_ZOOM_KEY, value ? 1 : 0);
                _autoZoom = value;
            }
        }

        /// <summary>
        /// Get or sets the scale at which selected attributes should be displayed if 
        /// <see cref="AutoZoom"/> is <see langword="true"/>.
        /// </summary>
        /// <value>The scale at which selected attributes should be displayed if 
        /// <see cref="AutoZoom"/> is <see langword="true"/>.</value>
        public int AutoZoomScale
        {
            get
            {
                return _autoZoomScale;
            }
            set
            {
                _iniFile.WriteInt32(_GENERAL_SECTION, _AUTO_ZOOM_SCALE_KEY, value);
                _autoZoomScale = value;
            }
        }

        /// <summary>
        /// Get or sets the <see cref="CursorTool"/> to select after manually creating a redaction.
        /// </summary>
        /// <value>The <see cref="CursorTool"/> to select after manually creating a redaction.</value>
        public AutoTool AutoTool
        {
            get
            {
                return _autoTool;
            }
            set
            {
                _iniFile.WriteInt32(_GENERAL_SECTION, _AUTO_TOOL_KEY, (int)value);
                _autoTool = value;
            }
        }
        
        /// <summary>
        /// Gets the color of redactions in redacted output images.
        /// </summary>
        /// <value>The color of redactions in redacted output images.</value>
        public RedactionColor OutputRedactionColor
        {
            get
            {
                return _outputRedactionColor;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets an array of the default redaction types.
        /// </summary>
        /// <returns>An array of the default redaction types.</returns>
        public string[] GetRedactionTypes()
        {
            return (string[])_types.Clone();
        }

        /// <summary>
        /// Gets an array of the default redaction types.
        /// </summary>
        /// <returns>An array of the default redaction types.</returns>
        static string[] GetRedactionTypes(InitializationFile iniFile)
        {
            // Get the number of types from the ini file
            int typeCount = iniFile.ReadInt32(_REDACTION_TYPES_SECTION, _REDACTION_TYPE_COUNT_KEY);
            string[] types = new string[typeCount];

            // Get each type
            for (int i = 1; i <= typeCount; i++)
            {
                string key = _REDACTION_TYPE_KEY_PREFIX + i.ToString(CultureInfo.InvariantCulture);
                types[i - 1] = iniFile.ReadString(_REDACTION_TYPES_SECTION, key);
            }

            return types;
        }

        /// <summary>
        /// Gets the confidence levels contained in the specified initialization file.
        /// </summary>
        /// <param name="iniFile">The initialization file containing confidence levels.</param>
        /// <returns>The confidence levels contained in <paramref name="iniFile"/>.</returns>
        static ConfidenceLevelsCollection GetConfidenceLevels(InitializationFile iniFile)
        {
            // Read the number of confidence levels
            int levelCount = iniFile.ReadInt32(_GENERAL_SECTION, _LEVEL_COUNT_KEY);

            // Iterate over each confidence level
            ConfidenceLevel[] levels = new ConfidenceLevel[levelCount];
            for (int i = 1; i <= levelCount; i++)
            {
                // Get the section for this confidence level
                string section = _LEVEL_SECTION_PREFIX + i.ToString(CultureInfo.InvariantCulture);

                // Get the data associated with this section
                string shortName = iniFile.ReadString(section, _SHORT_NAME_KEY);
                string query = iniFile.ReadString(section, _QUERY_KEY);
                Color color = GetColor(iniFile, section, _COLOR_KEY);
                bool output = iniFile.ReadInt32(section, _OUTPUT_KEY) != 0;
                bool warnIfRedact = iniFile.ReadInt32(section, _WARN_IF_REDACT) != 0;
                bool warnIfNonRedact = iniFile.ReadInt32(section, _WARN_IF_NON_REDACT) != 0;

                // Store the confidence level
                levels[i - 1] = new ConfidenceLevel(shortName, query, color, output, 
                    warnIfRedact, warnIfNonRedact);
            }

            return new ConfidenceLevelsCollection(levels);
        }

        /// <summary>
        /// Gets the color from the specified section and key of an initialization file.
        /// </summary>
        /// <param name="iniFile">The initialization file containing color information.</param>
        /// <param name="section">The section of the initialization file to read.</param>
        /// <param name="key">The key containing the color information.</param>
        /// <returns>The color specified by the <paramref name="key"/> of the 
        /// <paramref name="section"/> in the specified <paramref name="iniFile"/>.</returns>
        static Color GetColor(InitializationFile iniFile, string section, string key)
        {
            string value = null;
            try
            {
                // Get the value of the section as a string
                value = iniFile.ReadString(section, key);

                // Split into red, green, blue
                string[] rgb = value.Split('.');
                if (rgb.Length != 3)
                {
                    throw new ExtractException("ELI27287", "Invalid color format.");
                }

                // Parse each part of the color
                int red = int.Parse(rgb[0], CultureInfo.InvariantCulture);
                int green = int.Parse(rgb[1], CultureInfo.InvariantCulture);
                int blue = int.Parse(rgb[2], CultureInfo.InvariantCulture);

                return Color.FromArgb(red, green, blue);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27286",
                    "Invalid color in initialization file.", ex);
                ee.AddDebugData("Section", section, false);
                ee.AddDebugData("Key", key, false);
                ee.AddDebugData("Value", value, false);
                throw ee;
            }
        }

        /// <summary>
        /// Get the auto zoom setting from the specified initialization file.
        /// </summary>
        /// <param name="iniFile">The initialization file containing the auto zoom setting.</param>
        /// <returns>The auto zoom setting from <paramref name="iniFile"/>.</returns>
        static bool GetAutoZoom(InitializationFile iniFile)
        {
            return iniFile.ReadInt32(_GENERAL_SECTION, _AUTO_ZOOM_KEY) == 1;
        }

        /// <summary>
        /// Get the auto zoom scale from the specified initialization file.
        /// </summary>
        /// <param name="iniFile">The initialization file containing the auto zoom scale.</param>
        /// <returns>The auto zoom scale from <paramref name="iniFile"/>.</returns>
        static int GetAutoZoomScale(InitializationFile iniFile)
        {
            int scale = iniFile.ReadInt32(_GENERAL_SECTION, _AUTO_ZOOM_SCALE_KEY);

            // Auto zoom scale must be a number between 1 and 10
            if (scale < 1 || scale > 10)
            {
                scale = 5;
            }

            return scale;
        }

        /// <summary>
        /// Get the auto tool from the specified initialization file.
        /// </summary>
        /// <param name="iniFile">The initialization file containing the auto tool.</param>
        /// <returns>The auto tool from <paramref name="iniFile"/>.</returns>
        static AutoTool GetAutoTool(InitializationFile iniFile)
        {
            int autoTool = iniFile.ReadInt32(_GENERAL_SECTION, _AUTO_TOOL_KEY);

            // Ensure auto tool has a defined value
            if (autoTool < 0 || autoTool > 3)
            {
                return AutoTool.None;
            }

            return (AutoTool) autoTool;
        }

        /// <summary>
        /// Gets the color of redactions in output images from the specified initialization file.
        /// </summary>
        /// <param name="iniFile">The initialization file from which to read.</param>
        /// <returns>The color of redactions in output images.</returns>
        static RedactionColor GetRedactionColor(InitializationFile iniFile)
        {
            // The color is either white or black
            string color = iniFile.ReadString(_GENERAL_SECTION, _REDACTION_COLOR_KEY);
            bool isWhite = color.Equals("White", StringComparison.OrdinalIgnoreCase);

            return isWhite ? RedactionColor.White : RedactionColor.Black;
        }

        #endregion Methods
    }
}
