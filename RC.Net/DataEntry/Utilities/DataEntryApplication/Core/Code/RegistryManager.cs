using Extract.DataEntry;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Globalization;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// Manages DataEntryApplication specific registry settings
    /// </summary>
    internal static class RegistryManager
    {
        #region RegistryManager SubKeys

        /// <summary>
        /// The sub key for DataEntry keys.
        /// </summary>
        static readonly string _DATA_ENTRY_SUB_KEY = @"Software\Extract Systems\DataEntry";

        #endregion RegistryManager SubKeys

        #region RegistryManager Keys

        /// <summary>
        /// The value to indicate whether the DataEntryApplication should launch maximized.
        /// </summary>
        static readonly string _WINDOW_MAXIMIZED_VALUE = "WindowMaximized";

        /// <summary>
        /// The value to indicate the initial horizontal position of the DataEntryApplication.
        /// </summary>
        static readonly string _WINDOW_POSITION_X = "WindowPositionX";

        /// <summary>
        /// The value to indicate the initial vertical position of the DataEntryApplication.
        /// </summary>
        static readonly string _WINDOW_POSITION_Y = "WindowPositionY";

        /// <summary>
        /// The value to indicate the initial width of the DataEntryApplication.
        /// </summary>
        static readonly string _WINDOW_WIDTH = "WindowWidth";

        /// <summary>
        /// The value to indicate the initial height of the DataEntryApplication.
        /// </summary>
        static readonly string _WINDOW_HEIGHT = "WindowHeight";

        /// <summary>
        /// The value to indicate the initial splitter position of the DataEntryApplication.
        /// </summary>
        static readonly string _SPLITTER_POSITION = "SplitterPostion";

        /// <summary>
        /// Specifies how the image viewer zoom/view is adjusted when new fields are selected.
        /// </summary>
        static readonly string _AUTO_ZOOM_MODE = "AutoZoomMode";

        /// <summary>
        /// The page space (context) that should be shown around a selected object.
        /// </summary>
        static readonly string _AUTO_ZOOM_CONTEXT = "AutoZoomContext";

        #endregion RegistryManager Keys

        #region RegistryManager Fields

        /// <summary>
        /// The current user registry sub key for DataEntry keys.
        /// </summary>     
        static RegistryKey _userDataEntrySubKey =
            Registry.CurrentUser.CreateSubKey(_DATA_ENTRY_SUB_KEY);

        #endregion RegistryManager Fields

        #region RegistryManager Properties

        /// <summary>
        /// Indicates whether the <see cref="DataEntryApplication"/> should launch maximized.
        /// </summary>
        /// <value><see langword="true"/> if <see cref="DataEntryApplication"/>  should launch
        /// maximized or <see langword="false"/> if <see cref="DataEntryApplication"/>  should
        /// launch as a normal window.</value>
        /// <returns><see langword="true"/> if <see cref="DataEntryApplication"/>  will launch
        /// maximized or <see langword="false"/> if <see cref="DataEntryApplication"/>  will
        /// launch as a normal window.</returns>
        public static bool DefaultWindowMaximized
        {
            get
            {
                try
                {
                    return Convert.ToBoolean(_userDataEntrySubKey.GetValue(
                        _WINDOW_MAXIMIZED_VALUE, false), CultureInfo.CurrentCulture);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25065", ex);
                }
            }
            set
            {
                try
                {
                    _userDataEntrySubKey.SetValue(_WINDOW_MAXIMIZED_VALUE, value);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25066", ex);
                }
            }
        }

        /// <summary>
        /// Indicates the X position ID for <see cref="DataEntryApplication"/> upon launch
        /// </summary>
        public static int DefaultWindowPositionX
        {
            get
            {
                return (int)_userDataEntrySubKey.GetValue(_WINDOW_POSITION_X, 50);
            }
            set
            {
                try
                {
                    _userDataEntrySubKey.SetValue(_WINDOW_POSITION_X, value);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25067", ex);
                }
            }
        }

        /// <summary>
        /// Indicates the Y position ID for <see cref="DataEntryApplication"/> upon launch
        /// </summary>
        public static int DefaultWindowPositionY
        {
            get
            {
                return (int)_userDataEntrySubKey.GetValue(_WINDOW_POSITION_Y, 50);
            }
            set
            {
                try
                {
                    _userDataEntrySubKey.SetValue(_WINDOW_POSITION_Y, value);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25068", ex);
                }
            }
        }

        /// <summary>
        /// Indicates the starting width for <see cref="DataEntryApplication"/>.
        /// </summary>
        public static int DefaultWindowWidth
        {
            get
            {
                return (int)_userDataEntrySubKey.GetValue(_WINDOW_WIDTH, 759);
            }
            set
            {
                try
                {
                    _userDataEntrySubKey.SetValue(_WINDOW_WIDTH, value);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25069", ex);
                }
            }
        }

        /// <summary>
        /// Indicates the starting height for <see cref="DataEntryApplication"/>.
        /// </summary>
        public static int DefaultWindowHeight
        {
            get
            {
                return (int)_userDataEntrySubKey.GetValue(_WINDOW_HEIGHT, 600);
            }
            set
            {
                try
                {
                    _userDataEntrySubKey.SetValue(_WINDOW_HEIGHT, value);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25070", ex);
                }
            }
        }

        /// <summary>
        /// Indicates the starting splitter position for <see cref="DataEntryApplication"/> or
        /// -1 if the size of the DEP should be used to initialize the splitter position.
        /// </summary>
        public static int DefaultSplitterPosition
        {
            get
            {
                return (int)_userDataEntrySubKey.GetValue(_SPLITTER_POSITION, -1);
            }
            set
            {
                try
                {
                    _userDataEntrySubKey.SetValue(_SPLITTER_POSITION, value);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25074", ex);
                }
            }
        }

        /// <summary>
        /// Specifies how the image viewer zoom/view is adjusted when new fields are selected.
        /// </summary>
        public static AutoZoomMode AutoZoomMode
        {
            get
            {
                try
                {
                    return (AutoZoomMode)_userDataEntrySubKey.GetValue(_AUTO_ZOOM_MODE,
                        (int)AutoZoomMode.ZoomOutIfNecessary);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27027", ex);
                }
            }

            set
            {
                try
                {
                    _userDataEntrySubKey.SetValue(_AUTO_ZOOM_MODE, (int)value);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27029", ex);
                }
            }
        }

        /// <summary>
        /// The page space (context) that should be shown around a selected object.
        /// </summary>
        public static double AutoZoomContext
        {
            get
            {
                try
                {
                    object value = _userDataEntrySubKey.GetValue(_AUTO_ZOOM_CONTEXT, "0.5");
                    return System.Convert.ToDouble(value, CultureInfo.CurrentCulture);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27030", ex);
                }
            }

            set
            {
                try
                {
                    string stringValue = System.Convert.ToString(value, CultureInfo.CurrentCulture);
                    _userDataEntrySubKey.SetValue(_AUTO_ZOOM_CONTEXT, stringValue);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27031", ex);
                }
            }
        }

        #endregion RegistryManager Properties
    }
}
