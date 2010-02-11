using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Extract.Licensing;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents a graphics device interface (GDI) drawing surface.
    /// </summary>
    public class GdiGraphics
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(GeometryMethods).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The graphics object from which to create GDI handles.
        /// </summary>
        readonly Graphics _graphics;

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
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI29675",
                _OBJECT_NAME);

            _graphics = graphics;
            _drawMode = drawMode;
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
            IntPtr deviceContext = IntPtr.Zero;
            try
            {
                deviceContext = _graphics.GetHdc();

                NativeMethods.SetDrawMode(deviceContext, _drawMode);

                NativeMethods.SelectGdiObject(deviceContext, pen.Handle);

                NativeMethods.DrawPolygon(deviceContext, points);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29680", ex);
            }
            finally
            {
                if (deviceContext != IntPtr.Zero)
                {
                    _graphics.ReleaseHdc(deviceContext);
                }
            }
        }

        /// <summary>
        /// Draws the specified region using the specified color.
        /// </summary>
        /// <param name="region">The region to draw. Cannot be <see langword="null"/>.</param>
        /// <param name="color">The color of the region to draw.</param>
        /// <permission cref="SecurityPermission">Demands permission for unmanaged code.
        /// </permission>
        [SecurityPermission(SecurityAction.LinkDemand,
            Flags = SecurityPermissionFlag.UnmanagedCode)]
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
    }
}