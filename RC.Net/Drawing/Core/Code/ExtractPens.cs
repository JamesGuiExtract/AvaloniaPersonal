using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;

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
        static ThreadLocal<Dictionary<Color, Pen>> _pens =
            new ThreadLocal<Dictionary<Color, Pen>>(() =>
                new Dictionary<Color, Pen>());

        /// <summary>
        /// A collection of thick pens, keyed by color.
        /// </summary>
        static readonly ThreadLocal<Dictionary<Color, Pen>> _thickPens =
            new ThreadLocal<Dictionary<Color, Pen>>(() =>
                new Dictionary<Color, Pen>());

        /// <summary>
        /// A collection of GDI pens, keyed by color and width.
        /// </summary>
        static readonly ThreadLocal<Dictionary<KeyValuePair<Color, int>, GdiPen>> _gdiPens =
            new ThreadLocal<Dictionary<KeyValuePair<Color, int>, GdiPen>>(() =>
                new Dictionary<KeyValuePair<Color, int>, GdiPen>());

        /// <summary>
        /// A collection of thick dashed pens, keyed by color.
        /// </summary>
        static readonly ThreadLocal<Dictionary<Color, Pen>> _thickDashedPens = 
            new ThreadLocal<Dictionary<Color, Pen>>(() =>
                new Dictionary<Color, Pen>());

        /// <summary>
        /// A pen that draws a dashed black line.
        /// </summary>
        static ThreadLocal<Pen> _dashedBlack =
            new ThreadLocal<Pen>(() =>
            {
                var pen = new Pen(Color.Black, 1);
                pen.DashStyle = DashStyle.Dash;
                return pen;
            });

        static ThreadLocal<Pen> _dashedGray =
            new ThreadLocal<Pen>(() =>
            {
                var pen = new Pen(Color.FromArgb(191, 191, 191), 1);
                pen.DashStyle = DashStyle.Dash;
                return pen;
            });

        /// <summary>
        /// A pen that draws a dotted black line.
        /// </summary>
        static ThreadLocal<Pen> _dottedBlack =
            new ThreadLocal<Pen>(() =>
            {
                var pen = new Pen(Color.Black, 1);
                pen.DashStyle = DashStyle.Dot;
                return pen;
            });

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
                return _dashedBlack.Value;
            }
        }

        public static Pen DashedGray
        {
            get
            {
                return _dashedGray.Value;
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
                return _dottedBlack.Value;
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
                Pen pen = null;
                if (!_pens.Value.TryGetValue(color, out pen))
                {
                    pen = new Pen(color);
                    _pens.Value.Add(color, pen);
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
                Pen thickPen = null;
                if (!_thickPens.Value.TryGetValue(color, out thickPen))
                {
                    thickPen = new Pen(color, ThickPenWidth);
                    _thickPens.Value.Add(color, thickPen);
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
                GdiPen gdiPen = null;
                var key = new KeyValuePair<Color, int>(color, width);
                if (!_gdiPens.Value.TryGetValue(key, out gdiPen))
                {
                    gdiPen = new GdiPen(color, width);
                    _gdiPens.Value.Add(key, gdiPen);
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
                Pen dashedPen = null;
                if (!_thickDashedPens.Value.TryGetValue(color, out dashedPen))
                {
                    dashedPen = new Pen(color, 2);
                    dashedPen.DashStyle = DashStyle.Dash;
                    _thickDashedPens.Value.Add(color, dashedPen);
                }

                return dashedPen;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28972", ex);
            }
        }

        #endregion Properties
    }
}
