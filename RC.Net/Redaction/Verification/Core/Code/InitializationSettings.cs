using Extract.Imaging.Forms;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents ID Shield settings read from an initialization file (ini).
    /// </summary>
    public class InitializationSettings
    {
        #region InitializationSettings Constants

        static readonly string _GENERAL_SECTION = "General";

        static readonly string _LEVEL_SECTION_PREFIX = "Level";

        static readonly string _AUTO_ZOOM_KEY = "AutoZoom";

        static readonly string _AUTO_ZOOM_SCALE_KEY = "AutoZoomScale";

        static readonly string _AUTO_TOOL_KEY = "AutoPan";

        static readonly string _REDACTION_COLOR_KEY = "RedactionColorInOutputFile";

        static readonly string _LEVEL_COUNT_KEY = "NumConfidenceLevels";

        static readonly string _LONG_NAME_KEY = "LongName";

        static readonly string _SHORT_NAME_KEY = "ShortName";

        static readonly string _QUERY_KEY = "Query";

        static readonly string _COLOR_KEY = "Color";

        static readonly string _OUTPUT_KEY = "Output";

        #endregion InitializationSettings Constants

        #region InitializationSettings Fields

        /// <summary>
        /// The confidence levels of ID Shield attributes.
        /// </summary>
        readonly ConfidenceLevelsCollection _levels;

        /// <summary>
        /// <see langword="true"/> if selected attributes should be displayed at a particular 
        /// zoom setting; <see langword="false"/> if zoom setting should be unchanged when 
        /// displaying selected attributes.
        /// </summary>
        readonly bool _autoZoom;

        /// <summary>
        /// The scale at which selected attributes should be displayed if <see cref="_autoZoom"/> 
        /// is <see langword="true"/>.
        /// </summary>
        readonly int _autoZoomScale;

        /// <summary>
        /// The tool to automatically select after the user manually creates a redaction.
        /// </summary>
        readonly AutoTool _autoTool;

        /// <summary>
        /// The color of redactions in redacted images.
        /// </summary>
        readonly RedactionColor _outputRedactionColor;

        #endregion InitializationSettings Fields

        #region InitializationSettings Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InitializationSettings"/> class.
        /// </summary>
        public InitializationSettings()
        {
            InitializationFile iniFile = new InitializationFile("IDShield.ini");

            _levels = GetConfidenceLevels(iniFile);
            _autoZoom = iniFile.ReadInt32(_GENERAL_SECTION, _AUTO_ZOOM_KEY) == 1;
            _autoZoomScale = iniFile.ReadInt32(_GENERAL_SECTION, _AUTO_ZOOM_SCALE_KEY);
            _autoTool = (AutoTool)iniFile.ReadInt32(_GENERAL_SECTION, _AUTO_TOOL_KEY);
            _outputRedactionColor = GetRedactionColor(iniFile);
        }

        #endregion InitializationSettings Constructors

        #region InitializationSettings Properties

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
        /// Gets whether selected attributes should be displayed at a particular zoom setting.
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
        }

        /// <summary>
        /// Gets the scale at which selected attributes should be displayed if 
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
        }

        /// <summary>
        /// Gets the <see cref="CursorTool"/> to select after manually creating a redaction.
        /// </summary>
        /// <value>The <see cref="CursorTool"/> to select after manually creating a redaction.</value>
        public AutoTool AutoTool
        {
            get
            {
                return _autoTool;
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

        #endregion InitializationSettings Properties

        #region InitializationSettings Methods

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
                string longName = iniFile.ReadString(section, _LONG_NAME_KEY);
                string shortName = iniFile.ReadString(section, _SHORT_NAME_KEY);
                string query = iniFile.ReadString(section, _QUERY_KEY);
                Color color = GetColor(iniFile, section, _COLOR_KEY);
                bool output = iniFile.ReadInt32(section, _OUTPUT_KEY) != 0;

                // Store the confidence level
                levels[i - 1] = new ConfidenceLevel(longName, shortName, query, color, output);
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

        #endregion InitializationSettings Methods
    }
}
