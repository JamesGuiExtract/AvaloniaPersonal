using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents a container for <see cref="Pen"/>'s. 
    /// </summary>
    public static class ExtractPens
    {
        #region Fields

        /// <summary>
        /// A collection of pens, keyed by color.
        /// </summary>
        static readonly Dictionary<Color, Pen> _pens = new Dictionary<Color, Pen>();

        /// <summary>
        /// A collection of thick pens, keyed by color.
        /// </summary>
        static readonly Dictionary<Color, Pen> _thickPens = new Dictionary<Color, Pen>();

        /// <summary>
        /// A collection of GDI pens, keyed by color and width.
        /// </summary>
        static readonly Dictionary<KeyValuePair<Color, int>, GdiPen> _gdiPens =
            new Dictionary<KeyValuePair<Color, int>, GdiPen>();

        /// <summary>
        /// A collection of thick dashed pens, keyed by color.
        /// </summary>
        static readonly Dictionary<Color, Pen> _thickDashedPens = new Dictionary<Color, Pen>();

        /// <summary>
        /// A pen that draws a dashed black line.
        /// </summary>
        static Pen _dashedBlack;

        /// <summary>
        /// A pen that draws a dotted black line.
        /// </summary>
        static Pen _dottedBlack;

        /// <summary>
        /// Mutex object to provide exclusive access to the dashed and dotted pens
        /// </summary>
        static readonly object _lockDashedAndDotted = new object();

        /// <summary>
        /// Mutex object to provide exclusive access to the pens collections
        /// </summary>
        static readonly object _lockPens = new object();

        /// <summary>
        /// Mutex object to provide exclusive access to the thick pens collections
        /// </summary>
        static readonly object _lockThick = new object();

        /// <summary>
        /// Mutex object to provide exclusive access to the GDI pens collections
        /// </summary>
        static readonly object _lockGdi = new object();

        /// <summary>
        /// Mutex object to provide exclusive access to the thick dashed pens collections
        /// </summary>
        static readonly object _lockThickDashed = new object();

        /// <summary>
        /// The width of thick pens.
        /// </summary>
        public static readonly int ThickPenWidth = 4;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets a pen that draws a dashed black line.
        /// </summary>
        /// <returns>A pen that draws a dashed black line.</returns>
        public static Pen DashedBlack
        {
            get
            {
                if (_dashedBlack == null)
                {
                    lock (_lockDashedAndDotted)
                    {
                        if (_dashedBlack == null)
                        {
                            _dashedBlack = new Pen(Color.Black, 1);
                            _dashedBlack.DashStyle = DashStyle.Dash;
                        }
                    }
                }

                return _dashedBlack;
            }
        }

        /// <summary>
        /// Gets a pen that draws a dotted black line.
        /// </summary>
        /// <returns>A pen that draws a dotted black line.</returns>
        public static Pen DottedBlack
        {
            get
            {
                if (_dottedBlack == null)
                {
                    lock (_lockDashedAndDotted)
                    {
                        if (_dottedBlack == null)
                        {
                            _dottedBlack = new Pen(Color.Black, 1);
                            _dottedBlack.DashStyle = DashStyle.Dot;
                        }
                    }
                }

                return _dottedBlack;
            }
        }

        /// <summary>
        /// Gets a <see cref="Pen"/> that draws a solid line of the specified color.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> of the desired <see cref="Pen"/>.</param>
        /// <returns>A <see cref="Pen"/> that draws a solid line of the specified color.</returns>
        public static Pen GetPen(Color color)
        {
            try
            {
                // Mutex around collection to prevent multiple reads and writes
                Pen pen;
                lock (_lockPens)
                {
                    // Check if the pen has already been created
                    if (!_pens.TryGetValue(color, out pen))
                    {
                        // Create the pen
                        pen = new Pen(color);
                        _pens.Add(color, pen);
                    }
                }

                return pen;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26491", ex);
            }
        }

        /// <summary>
        /// Creates a thick pen from the specified color.
        /// </summary>
        /// <param name="color">The color of the pen to create.</param>
        /// <returns>A thick pen from the specified color.</returns>
        public static Pen GetThickPen(Color color)
        {
            try
            {
                // Mutex around collection to prevent multiple reads and writes
                Pen thickPen;
                lock (_lockThick)
                {
                    // Check if the pen has already been created
                    if (!_thickPens.TryGetValue(color, out thickPen))
                    {
                        // Create the pen
                        thickPen = new Pen(color, ThickPenWidth);
                        _thickPens.Add(color, thickPen);
                    }
                }

                return thickPen;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26492", ex);
            }
        }

        /// <summary>
        /// Creates a GDI pen from the specified color.
        /// </summary>
        /// <param name="color">The color of the GDI pen to create.</param>
        /// <param name="width">The width of the pen to create.</param>
        /// <returns>A GDI pen from the specified color.</returns>
        public static GdiPen GetGdiPen(Color color, int width)
        {
            try
            {
                // Mutex around collection to prevent multiple reads and writes
                GdiPen gdiPen;
                lock (_lockGdi)
                {
                    // The key for the _gdiPens dictionary is a combination of color and width.
                    var key = new KeyValuePair<Color, int>(color, width);

                    // Check if the pen has already been created
                    if (!_gdiPens.TryGetValue(key, out gdiPen))
                    {
                        // Create the pen
                        gdiPen = new GdiPen(color, width);
                        _gdiPens.Add(key, gdiPen);
                    }
                }

                return gdiPen;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI32174", ex);
            }
        }

        /// <summary>
        /// Creates a thick GDI pen from the specified color.
        /// </summary>
        /// <param name="color">The color of the GDI pen to create.</param>
        /// <returns>A thick GDI pen from the specified color.</returns>
        public static GdiPen GetThickGdiPen(Color color)
        {
            try
            {
                return GetGdiPen(color, ThickPenWidth);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29704", ex);
            }
        }

        /// <summary>
        /// Creates a dashed pen with a certain thickness from the specified color.
        /// </summary>
        /// <param name="color">The color of the dashed pen to create.</param>
        /// <returns>A dashed pen with a certain thickness from the specified color.</returns>
        public static Pen GetThickDashedPen(Color color)
        {
            try
            {
                // Mutex around collection to prevent multiple reads and writes
                Pen dashedPen;
                lock (_lockThickDashed)
                {
                    // Check if the pen has already been created
                    if (!_thickDashedPens.TryGetValue(color, out dashedPen))
                    {
                        // Create the pen
                        dashedPen = new Pen(color, 2);
                        dashedPen.DashStyle = DashStyle.Dash;
                        _thickDashedPens.Add(color, dashedPen);
                    }
                }

                return dashedPen;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28972", ex);
            }
        }

        /// <summary>
        /// Disposes of all cached pen objects and clears out the collections.
        /// </summary>
        public static void ClearAndDisposeAllPens()
        {
            try
            {
                lock (_lockDashedAndDotted)
                {
                    if (_dashedBlack != null)
                    {
                        _dashedBlack.Dispose();
                        _dashedBlack = null;
                    }
                    if (_dottedBlack != null)
                    {
                        _dottedBlack.Dispose();
                        _dottedBlack = null;
                    }
                }

                lock (_lockThick)
                {
                    CollectionMethods.ClearAndDispose(_thickPens);
                }

                lock (_lockThickDashed)
                {
                    CollectionMethods.ClearAndDispose(_thickDashedPens);
                }

                lock (_lockPens)
                {
                    CollectionMethods.ClearAndDispose(_pens);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27859", ex);
            }
        }

        #endregion Properties
    }
}
