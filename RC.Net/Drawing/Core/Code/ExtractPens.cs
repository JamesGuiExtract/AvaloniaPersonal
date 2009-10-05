using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents a container for <see cref="Pen"/>'s. 
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

        /// <summary>
        /// Mutex object to provide exclusive access to the dashed and dotted pens
        /// </summary>
        static object _lockDashedAndDotted = new object();

        /// <summary>
        /// Mutex object to provide exclusive access to the pens collections
        /// </summary>
        static object _lockPens = new object();

        /// <summary>
        /// Mutex object to provide exclusive access to the thick dashed pens collections
        /// </summary>
        static object _lockThickDashed = new object();

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
                throw ExtractException.AsExtractException("ELI26492", ex);
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

        #endregion ExtractPens Properties
    }
}
