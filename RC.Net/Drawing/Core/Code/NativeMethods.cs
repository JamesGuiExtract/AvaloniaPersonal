using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents entry points to unmanaged code.
    /// </summary>
    internal static class NativeMethods
    {
        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        struct POINT
        {
            readonly int _x;
            readonly int _y;

            POINT(int x, int y)
            {
                _x = x;
                _y = y;
            }

            public static implicit operator Point(POINT p)
            {
                return new Point(p._x, p._y);
            }

            public static implicit operator POINT(Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }

        #endregion Structs

        #region P/Invokes

        /// <summary>
        /// Creates a logical brush that has the specified solid color.
        /// </summary>
        /// <param name="color">The color of the brush.</param>
        /// <returns>If the function succeeds, the return value identifies a logical brush. If the 
        /// function fails, the return value is <see cref="IntPtr.Zero"/>. 
        /// </returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms532387.aspx">CreateSolidBrush
        /// </seealso>
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern SafeGdiHandle CreateSolidBrush(int color);

        /// <summary>
        /// Creates a logical pen that has the specified style, width, and color.
        /// </summary>
        /// <param name="fnPenStyle">The pen style.</param>
        /// <param name="nWidth">The width of the pen, in logical units. If nWidth is zero, the 
        /// pen is a single pixel wide, regardless of the current transformation.</param>
        /// <param name="crColor">A color reference for the pen color.</param>
        /// <returns>A handle that identifies a logical pen if sucessful; <see langword="null"/> 
        /// otherwise.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/dd183509%28VS.85%29.aspx"/>
        [DllImport("gdi32.dll", SetLastError = true)]
        static extern SafeGdiHandle CreatePen(int fnPenStyle, int nWidth, int crColor);

        /// <summary>
        /// Selects an object into the specified device context. The new object replaces the 
        /// previous object of the same type.
        /// </summary>
        /// <param name="hdc">Handle to the device context.</param>
        /// <param name="hgdiobj">Handle to the object to be selected.</param>
        /// <returns>A handle to the object being replaced if successful; <see langword="null"/> 
        /// otherwise.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/dd162957%28VS.85%29.aspx"/>
        [DllImport("gdi32.dll", SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, SafeGdiHandle hgdiobj);

        /// <summary>
        /// Sets the current foreground mix mode.
        /// </summary>
        /// <param name="deviceContext">Handle to the device context.</param>
        /// <param name="drawMode">Specifies the mix mode.</param>
        /// <returns>If the function succeeds, the return value specifies the previous mix mode. 
        /// If the function fails, the return value is zero.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms534912.aspx">SetROP2
        /// </seealso>
        [DllImport("gdi32.dll", SetLastError = true)]
        static extern RasterDrawMode SetROP2(IntPtr deviceContext, RasterDrawMode drawMode);

        /// <summary>
        /// Draws a series of line segments by connecting the points in the specified array.
        /// </summary>
        /// <param name="hdc">A handle to a device context.</param>
        /// <param name="lppt">An array of <see cref="POINT"/> structures, in logical units.</param>
        /// <param name="cPoints">The number of points in the array. This number must be greater 
        /// than or equal to two.</param>
        /// <returns><see langword="true"/> if successful; <see langword="false"/> otherwise.
        /// </returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/dd162815%28VS.85%29.aspx"/>
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool Polyline(IntPtr hdc, [In] POINT[] lppt, int cPoints);

        /// <summary>
        /// Fills a region by using the specified brush.
        /// </summary>
        /// <param name="deviceContext">Handle to the device context.</param>
        /// <param name="region">Handle to the region to be filled in logical units.</param>
        /// <param name="brush">Handle to the brush to be used to fill the region.</param>
        /// <returns>If the function succeeds, the return value is <see langword="true"/>. If the 
        /// function fails, the return value is <see langword="false"/>.</returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms536674(VS.85).aspx">FillRgn
        /// </seealso>
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FillRgn(IntPtr deviceContext, IntPtr region, SafeGdiHandle brush);

        /// <summary>
        /// Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system 
        /// resources associated with the object.
        /// </summary>
        /// <param name="obj">Handle to a logical pen, brush, font, bitmap, region, or palette.
        /// </param>
        /// <returns>If the function succeeds, the return value is <see langword="true"/>. If the 
        /// specified handle is not valid or is currently selected into a device context, the 
        /// return value is <see langword="false"/>.
        /// </returns>
        /// <seealso href="http://msdn.microsoft.com/en-us/library/ms533225.aspx">DeleteObject
        /// </seealso>
        [DllImport("gdi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool DeleteObject(IntPtr obj);

        #endregion P/Invokes

        #region Methods

        /// <summary>
        /// Creates a handle to GDI pen.
        /// </summary>
        /// <param name="color">A color reference for the pen color.</param>
        /// <param name="width">The width of the pen, in logical units. If nWidth is zero, the 
        /// pen is a single pixel wide, regardless of the current transformation.</param>
        /// <param name="style">The pen style.</param>
        /// <returns>A handle to a GDI pen.</returns>
        public static SafeGdiHandle CreatePenHandle(Color color, int width, DashStyle style)
        {
            // Create a colored pen
            SafeGdiHandle pen = CreatePen((int)style, width, ColorTranslator.ToWin32(color));
            if (pen.IsInvalid)
            {
                throw new ExtractException("ELI29676", "Unable to create GDI pen.",
                    new Win32Exception(Marshal.GetLastWin32Error()));
            }

            return pen;
        }

        /// <summary>
        /// Selects the specified GDI object.
        /// </summary>
        /// <param name="deviceContext">The device context on which to select the GDI object.</param>
        /// <param name="handle">A handle to GDI object to select.</param>
        public static void SelectGdiObject(IntPtr deviceContext, SafeGdiHandle handle)
        {
            if (SelectObject(deviceContext, handle) == IntPtr.Zero)
            {
                throw new ExtractException("ELI29679", "Unable to select GDI object.",
                   new Win32Exception(Marshal.GetLastWin32Error()));
            }
        }

        /// <summary>
        /// Set the current foreground mixing mode.
        /// </summary>
        /// <param name="deviceContext">The device context on which to set the mode.</param>
        /// <param name="drawMode">The drawing mode.</param>
        public static void SetDrawMode(IntPtr deviceContext, RasterDrawMode drawMode)
        {
            if (SetROP2(deviceContext, drawMode) == 0)
            {
                throw new ExtractException("ELI21113", "Unable to set draw mode for region.",
                    new Win32Exception(Marshal.GetLastWin32Error()));
            }
        }

        /// <summary>
        /// Draws a polygon defined by the specified points. 
        /// </summary>
        /// <param name="deviceContext">A handle to the device context on which to draw.</param>
        /// <param name="points">The vertices of the polygon.</param>
        public static void DrawPolygon(IntPtr deviceContext, Point[] points)
        {
            if (points == null || points.Length < 3)
            {
                throw new ExtractException("ELI29677", 
                    "Polygon must have at least three points.");
            }

            POINT[] vertices = GetPolygonVertices(points);

            bool success = Polyline(deviceContext, vertices, vertices.Length);
            if (!success)
            {
                throw new ExtractException("ELI29678", "Unable to draw polygon.",
                    new Win32Exception(Marshal.GetLastWin32Error()));
            }
        }

        /// <summary>
        /// Creates an array of <see cref="POINT"/> objects from the specified array.
        /// </summary>
        /// <param name="points">The vertices of the a polygon.</param>
        /// <returns>An array of <see cref="POINT"/> objects from the specified array.</returns>
        static POINT[] GetPolygonVertices(Point[] points)
        {
            // Determine whether the polygon shape needs to be closed
            int pointsCount = points.Length;
            bool closePolygon = points[0] != points[pointsCount - 1];

            // Copy all the points
            int resultCount = closePolygon ? pointsCount + 1: pointsCount;
            POINT[] result = new POINT[resultCount];
            for (int i = 0; i < pointsCount; i++)
            {
                result[i] = points[i];
            }

            // Close the polygon if necessary
            if (closePolygon)
            {
                result[resultCount - 1] = points[0];
            }

            return result;
        }

        /// <summary>
        /// Deletes a logical pen, brush, font, bitmap, region, or palette, freeing all system 
        /// resources associated with the object.
        /// </summary>
        /// <param name="handle">The handle to free.</param>
        public static void ReleaseGdiHandle(IntPtr handle)
        {
            if (handle != IntPtr.Zero)
            {
                bool success = DeleteObject(handle);
                if (!success)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
        }

        #endregion Methods
    }
}
