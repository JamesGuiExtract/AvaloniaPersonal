using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents a container for mouse cursors common to Extract Systems applications.
    /// </summary>
    public static class ExtractPens
    {
        #region ExtractPens Fields

        /// <summary>
        /// A collection of pens, keyed by color.
        /// </summary>
        static Dictionary<Color, Pen> _pens = new Dictionary<Color, Pen>();

        /// <summary>
        /// A collection of thick dashed pens, keyed by color.
        /// </summary>
        static Dictionary<Color, Pen> _thickDashedPens = new Dictionary<Color, Pen>();

        /// <summary>
        /// A pen that draws a dashed black line.
        /// </summary>
        static Pen _dashedBlack;

        /// <summary>
        /// A pen that draws a dotted black line.
        /// </summary>
        static Pen _dottedBlack;

        #endregion ExtractPens Fields

        #region ExtractPens Properties

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
                    _dashedBlack = new Pen(Color.Black, 1);
                    _dashedBlack.DashStyle = DashStyle.Dash;
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
                    _dottedBlack = new Pen(Color.Black, 1);
                    _dottedBlack.DashStyle = DashStyle.Dot;
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
                // Check if the pen has already been created
                Pen pen;
                if (!_pens.TryGetValue(color, out pen))
                {
                    // Create the pen
                    pen = new Pen(color);
                    _pens.Add(color, pen);
                }

                return pen;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26491", ex);
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
                // Check if the pen has already been created
                Pen dashedPen;
                if (!_thickDashedPens.TryGetValue(color, out dashedPen))
                {
                    // Create the pen
                    dashedPen = new Pen(color, 2);
                    dashedPen.DashStyle = DashStyle.Dash;
                    _thickDashedPens.Add(color, dashedPen);
                }

                return dashedPen;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26492", ex);
            }
        }

        #endregion ExtractPens Properties
    }
}
