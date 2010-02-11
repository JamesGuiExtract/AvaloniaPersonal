using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents a pen used to draw graphics device interface (GDI) lines and curves.
    /// </summary>
    public sealed class GdiPen : IDisposable
    {
        #region Fields

        readonly int _width;
        SafeGdiHandle _handle;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GdiPen"/> class.
        /// </summary>
        public GdiPen(Color color, int width)
            : this(color, width, DashStyle.Solid)
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GdiPen"/> class.
        /// </summary>
        public GdiPen(Color color, int width, DashStyle style)
        {
            _width = width;
            _handle = NativeMethods.CreatePenHandle(color, width, style);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the width of the <see cref="GdiPen"/> in units of the <see cref="GdiGraphics"/> 
        /// object used to draw it.
        /// </summary>
        /// <value>The width of the <see cref="GdiPen"/> in units of the <see cref="GdiGraphics"/> 
        /// object used to draw it.</value>
        public int Width
        {
            get
            {
                return _width;
            }
        }

        /// <summary>
        /// Gets the a GDI handle to the <see cref="GdiPen"/>.
        /// </summary>
        /// <value>The a GDI handle to the <see cref="GdiPen"/>.</value>
        internal SafeGdiHandle Handle
        {
            get
            {
                return _handle;
            }
        }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="GdiPen"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="GdiPen"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="GdiPen"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>		
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_handle != null)
                {
                    _handle.Dispose();
                    _handle = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}