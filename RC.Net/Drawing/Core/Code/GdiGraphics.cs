using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Extract.Licensing;
using System.Drawing.Drawing2D;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents a graphics device interface (GDI) drawing surface.
    /// </summary>
    public class GdiGraphics : IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(GeometryMethods).ToString();

        /// <summary>
        /// The elements of the identity Matrix. Used to test if a matrix is the identity.
        /// </summary>
        static readonly float[] _IDENTITY_MATRIX_ELEMENTS = new float[] { 1F, 0F, 0F, 1F, 0F, 0F };

        #endregion Constants

        #region Fields

        /// <summary>
        /// The graphics object from which to create GDI handles.
        /// </summary>
        readonly Graphics _graphics;

        /// <summary>
        /// A <see cref="Matrix"/> that should be used to transform coordinates before drawing.
        /// </summary>
        Matrix _transform;

        /// <summary>
        /// The raster drawing mode to use when drawing GDI figures.
        /// </summary>
        RasterDrawMode _drawMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GdiGraphics"/> class.
        /// </summary>
        /// <param name="graphics">The graphics object from which to create a 
        /// <see cref="GdiGraphics"/>. Cannot be null.</param>
        /// <param name="drawMode">The drawing mode associated with the <see cref="GdiGraphics"/>.
        /// </param>
        public GdiGraphics(Graphics graphics, RasterDrawMode drawMode)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI29675",
                        _OBJECT_NAME);

                _graphics = graphics;
                _drawMode = drawMode;

                // Use the same transform as the supplied graphics. This is a copy or the original
                // and needs to be disposed of.
                _transform = _graphics.Transform;

                // However, if the transform is the identity matrix (and thus would have no effect),
                // go ahead and dispose of it right away since it won't be needed.
                // NOTE: For some reason that I don't understand, the IsIdentity property returns false
                // even when the matrix appears to be the identity. Therefore, compare each matrix
                // element to the identity matrix elements
                float[] elements = _transform.Elements;
                int i;
                for (i = 0; i < 6; i++)
                {
                    if (elements[i] != _IDENTITY_MATRIX_ELEMENTS[i])
                    {
                        // _transform is not the identity matrix.
                        break;
                    }
                }

                if (i == 6)
                {
                    // _transform is the identity matrix; it is not needed.
                    _transform.Dispose();
                    _transform = null;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31455", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the drawing mode associated with the <see cref="GdiGraphics"/>.
        /// </summary>
        /// <value>The drawing mode associated with the <see cref="GdiGraphics"/>.</value>
        public RasterDrawMode DrawMode
        {
            get
            {
                return _drawMode;
            }
            set
            {
                _drawMode = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Draws a polygon defined by the specified points. 
        /// </summary>
        /// <param name="pen">Pen that determines the color, width, and style of the polygon.</param>
        /// <param name="points">The vertices of the polygon.</param>
        public void DrawPolygon(GdiPen pen, Point[] points)
        {
            try
            {
                if (_transform != null)
                {
                    _transform.TransformPoints(points);
                }

                DrawPolygon_Internal(pen, points);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29680", ex);
            }
        }

        /// <summary>
        /// Draws a polygon defined by the specified points. 
        /// </summary>
        /// <param name="pen">Pen that determines the color, width, and style of the polygon.</param>
        /// <param name="points">The vertices of the polygon.</param>
        public void DrawPolygon(GdiPen pen, PointF[] points)
        {
            try
            {
                if (_transform != null)
                {
                    _transform.TransformPoints(points);
                }

                Point[] rounded = new Point[points.Length];

                for (int i = 0; i < points.Length; i++)
                {
                    rounded[i] = Point.Round(points[i]);
                }

                DrawPolygon_Internal(pen, rounded);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29693", ex);
            }
        }

        /// <summary>
        /// Draws the specified region using the specified color.
        /// </summary>
        /// <param name="region">The region to draw. Cannot be <see langword="null"/>.</param>
        /// <param name="color">The color of the region to draw.</param>
        /// <permission cref="SecurityPermission">Demands permission for unmanaged code.
        /// </permission>
        public void FillRegion(Region region, Color color)
        {
            // No need to draw transparent or empty regions
            if (color == Color.Transparent || region.IsEmpty(_graphics))
            {
                return;
            }

            // Create handles for the the brush, region, and device context
            SafeGdiHandle brush = null;
            IntPtr regionHandle = IntPtr.Zero;
            IntPtr deviceContext = IntPtr.Zero;

            // In try..finally to ensure the handles are released
            try
            {
                // Create a colored brush to draw highlights
                brush = NativeMethods.CreateSolidBrush(ColorTranslator.ToWin32(color));
                if (brush.IsInvalid)
                {
                    throw new ExtractException("ELI21591", "Unable to create brush for region.",
                        new Win32Exception(Marshal.GetLastWin32Error()));
                }

                // Get the region handle
                regionHandle = region.GetHrgn(_graphics);

                // Get the device context
                deviceContext = _graphics.GetHdc();

                // Set the draw mode
                NativeMethods.SetDrawMode(deviceContext, _drawMode);

                // Draw the region
                if (!NativeMethods.FillRgn(deviceContext, regionHandle, brush))
                {
                    throw new ExtractException("ELI21961", "Unable to draw region.",
                        new Win32Exception(Marshal.GetLastWin32Error()));
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29674", ex);
            }
            finally
            {
                // Release the handles
                if (brush != null)
                {
                    brush.Dispose();
                }
                if (regionHandle != IntPtr.Zero)
                {
                    region.ReleaseHrgn(regionHandle);
                }
                if (deviceContext != IntPtr.Zero)
                {
                    _graphics.ReleaseHdc(deviceContext);
                }
            }
        }

        #endregion Methods

        /// <summary>
        /// Draws a polygon defined by the specified points. 
        /// </summary>
        /// <param name="pen">Pen that determines the color, width, and style of the polygon.</param>
        /// <param name="points">The vertices of the polygon.</param>
        void DrawPolygon_Internal(GdiPen pen, Point[] points)
        {
            IntPtr deviceContext = IntPtr.Zero;
            try
            {
                deviceContext = _graphics.GetHdc();

                NativeMethods.SetDrawMode(deviceContext, _drawMode);

                NativeMethods.SelectGdiObject(deviceContext, pen.Handle);

                NativeMethods.DrawPolygon(deviceContext, points);
            }
            finally
            {
                if (deviceContext != IntPtr.Zero)
                {
                    _graphics.ReleaseHdc(deviceContext);
                }
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="GdiGraphics"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="GdiGraphics"/>.
        /// </overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="GdiGraphics"/>. 
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                if (_transform != null)
                {
                    _transform.Dispose();
                    _transform = null;
                }
            }

            // Dispose of ummanaged resources
        }

        #endregion IDisposable Members
    }
}