using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Text;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents a grouping of methods for drawing.
    /// </summary>
    public static class DrawingMethods
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(DrawingMethods).ToString();

        #endregion Constants

        /// <overloads>Draws the specified text string with the specified rotation.</overloads>
        /// <summary>
        /// Draws the specified text string with the specified rotation.
        /// </summary>
        /// <param name="graphics">The graphics object with which to draw the text.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The font to use to draw the text.</param>
        /// <param name="brush">The brush that determines the color and texture of the drawn text.
        /// </param>
        /// <param name="left">The left source coordinate of the text to draw.</param>
        /// <param name="top">The top source coordinate of the text to draw.</param>
        /// <param name="angle">The angle of rotation to apply to the text.</param>
        public static void DrawRotatedString(Graphics graphics, string text, Font font, Brush brush,
            float left, float top, float angle)
        {
            DrawRotatedString(graphics, text, font, brush, new RectangleF(left, top, 0F, 0F),
                null, angle);
        }

        /// <summary>
        /// Draws the specified text string with the specified rotation and the specified 
        /// formatting attributes.
        /// </summary>
        /// <param name="graphics">The graphics object with which to draw the text.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The font to use to draw the text.</param>
        /// <param name="brush">The brush that determines the color and texture of the drawn text.
        /// </param>
        /// <param name="left">The left source coordinate of the text to draw.</param>
        /// <param name="top">The top source coordinate of the text to draw.</param>
        /// <param name="format">The formatting attributes, such as line spacing and alignment, 
        /// that are applied to the drawn text.</param>
        /// <param name="angle">The angle of rotation to apply to the text.</param>
        public static void DrawRotatedString(Graphics graphics, string text, Font font, Brush brush,
            float left, float top, StringFormat format, float angle)
        {
            DrawRotatedString(graphics, text, font, brush, new RectangleF(left, top, 0F, 0F),
                format, angle);
        }

        /// <summary>
        /// Draws the specified text string with the specified rotation.
        /// </summary>
        /// <param name="graphics">The graphics object with which to draw the text.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The font to use to draw the text.</param>
        /// <param name="brush">The brush that determines the color and texture of the drawn text.
        /// </param>
        /// <param name="leftTop">The left-top source coordinate of the text to draw.</param>
        /// <param name="angle">The angle of rotation to apply to the text.</param>
        public static void DrawRotatedString(Graphics graphics, string text, Font font, Brush brush,
            PointF leftTop, float angle)
        {
            DrawRotatedString(graphics, text, font, brush, new RectangleF(leftTop, SizeF.Empty),
                null, angle);
        }

        /// <summary>
        /// Draws the specified text string with the specified rotation.
        /// </summary>
        /// <param name="graphics">The graphics object with which to draw the text.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The font to use to draw the text.</param>
        /// <param name="brush">The brush that determines the color and texture of the drawn text.
        /// </param>
        /// <param name="layoutRectangle">The rectangle in source coordinates that describes 
        /// where the text should be drawn.</param>
        /// <param name="angle">The angle of rotation to apply to the text.</param>
        public static void DrawRotatedString(Graphics graphics, string text, Font font, Brush brush,
            RectangleF layoutRectangle, float angle)
        {
            DrawRotatedString(graphics, text, font, brush, layoutRectangle, null, angle);
        }

        /// <summary>
        /// Draws the specified text string with the specified rotation and the specified 
        /// formatting attributes.
        /// </summary>
        /// <param name="graphics">The graphics object with which to draw the text.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The font to use to draw the text.</param>
        /// <param name="brush">The brush that determines the color and texture of the drawn text.
        /// </param>
        /// <param name="leftTop">The left-top source coordinate of the text to draw.</param>
        /// <param name="format">The formatting attributes, such as line spacing and alignment, 
        /// that are applied to the drawn text.</param>
        /// <param name="angle">The angle of rotation to apply to the text.</param>
        public static void DrawRotatedString(Graphics graphics, string text, Font font, Brush brush,
            PointF leftTop, StringFormat format, float angle)
        {
            DrawRotatedString(graphics, text, font, brush, new RectangleF(leftTop, SizeF.Empty),
                format, angle);
        }

        /// <summary>
        /// Draws the specified text string with the specified rotation and the specified 
        /// formatting attributes.
        /// </summary>
        /// <param name="graphics">The graphics object with which to draw the text.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="font">The font to use to draw the text.</param>
        /// <param name="brush">The brush that determines the color and texture of the drawn text.
        /// </param>
        /// <param name="layoutRectangle">The rectangle in source coordinates that describes 
        /// where the text should be drawn.</param>
        /// <param name="format">The formatting attributes, such as line spacing and alignment, 
        /// that are applied to the drawn text.</param>
        /// <param name="angle">The angle of rotation to apply to the text.</param>
        public static void DrawRotatedString(Graphics graphics, string text, Font font, Brush brush,
            RectangleF layoutRectangle, StringFormat format, float angle)
        {
            Matrix originalTransform = graphics.Transform.Clone();
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23161",
                    _OBJECT_NAME);

                // Determine the size of the layout rectangle if necessary
                if (layoutRectangle.Size.IsEmpty)
                {
                    // Get the size of the layout rectangle in source coordinates
                    layoutRectangle.Size = graphics.MeasureString(text, font);
                }

                // Determine the center of the layout rectangle in source coordinates
                PointF[] center = new PointF[] { new PointF(
                    layoutRectangle.X + layoutRectangle.Width / 2F,
                    layoutRectangle.Y + layoutRectangle.Height / 2F) };

                // Offset the layout rectangle
                layoutRectangle.Offset(-center[0].X, -center[0].Y);

                // Rotate the transformation matrix so that the text will be drawn at an angle
                graphics.TranslateTransform(center[0].X, center[0].Y);
                graphics.RotateTransform(angle);

                // Draw the string
                graphics.DrawString(text, font, brush, layoutRectangle, format);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23098", ex);
            }
            finally
            {
                // Restore the original transformation matrix
                if (graphics.Transform != null)
                {
                    graphics.Transform.Dispose();
                }
                graphics.Transform = originalTransform;
            }
        }

        /// <summary>
        /// Computes the bounding rectangle for the specified text with the specified
        /// alignment, font and orientation.
        /// </summary>
        /// <param name="text">The text to compute the bounding rectangle for.</param>
        /// <param name="graphics">The graphics object used to measure the string.</param>
        /// <param name="font">The font with which the text will be drawn.</param>
        /// <param name="borderPadding">The amount of padding to have around the text.</param>
        /// <param name="orientation">The orientation of the text.</param>
        /// <param name="anchorPoint">The anchor point for the text</param>
        /// <param name="alignment">The anchor alignment of the text (how the text
        /// is aligned relative to the <paramref name="anchorPoint"/>.</param>
        /// <returns>The bounding <see cref="Rectangle"/> for the text.</returns>
        public static Rectangle ComputeStringBounds(string text, Graphics graphics, Font font,
            int borderPadding, float orientation, Point anchorPoint, AnchorAlignment alignment)
        {
            // Store the original text rendering hint
            TextRenderingHint? originalHint = null;
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28009",
                    _OBJECT_NAME);

                // Store the original rendering hint
                originalHint = graphics.TextRenderingHint;

                // Set the rendering hint to anti-alias and measure the string
                graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
                Size size = Size.Round(graphics.MeasureString(text, font, PointF.Empty,
                    StringFormat.GenericTypographic));

                if (borderPadding > 0)
                {
                    int padding = borderPadding * 2;
                    size.Height += padding;
                    size.Width += padding;
                }

                // Calculate the top left coordinate based on the anchor alignment
                Point[] point = { anchorPoint };

                // The text layer object may be drawn in a rotated coordinate system.  Rotate the
                // anchor point into that coordinate system.
                using (Matrix transform = new Matrix())
                {
                    transform.Rotate(-orientation);
                    transform.TransformPoints(point);
                }

                // Set the location
                Point location = point[0];

                // Check the alignment and offset the location based on the alignment
                switch (alignment)
                {
                    case AnchorAlignment.LeftBottom:
                        location.Offset(0, -size.Height);
                        break;

                    case AnchorAlignment.Bottom:
                        location.Offset(-size.Width / 2, -size.Height);
                        break;

                    case AnchorAlignment.RightBottom:
                        location -= size;
                        break;

                    case AnchorAlignment.Left:
                        location.Offset(0, -size.Height / 2);
                        break;

                    case AnchorAlignment.Center:
                        location.Offset(-size.Width / 2, -size.Height / 2);
                        break;

                    case AnchorAlignment.Right:
                        location.Offset(-size.Width, -size.Height / 2);
                        break;

                    case AnchorAlignment.LeftTop:
                        // Do nothing - anchor point is the left top coordinate
                        break;

                    case AnchorAlignment.Top:
                        location.Offset(-size.Width / 2, 0);
                        break;

                    case AnchorAlignment.RightTop:
                        location.Offset(-size.Width, 0);
                        break;

                    default:
                        ExtractException ee = new ExtractException("ELI27975",
                            "Unexpected anchor alignment.");
                        ee.AddDebugData("Anchor alignment", alignment, false);
                        throw ee;
                }

                // Return a new rectangle representing the bounds
                return new Rectangle(location, size);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27976", ex);
            }
            finally
            {
                // Restore the original hint 
                if (originalHint != null)
                {
                    graphics.TextRenderingHint = originalHint.Value;
                }
            }
        }

        /// <summary>
        /// Draws the specified text on the specified graphics object.
        /// </summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="graphics">The graphics object to draw on.</param>
        /// <param name="transform">The transformation to apply to the text.</param>
        /// <param name="font">The font to draw the text with.</param>
        /// <param name="borderPadding">The padding to apply to the bounds of the text.</param>
        /// <param name="orientation">The rotation to apply to the text.</param>
        /// <param name="bounds">The bounding rectangle for the text.</param>
        /// <param name="backgroundColor">The background color to fill the text region with.
        /// if <see langword="null"/> no background color will be applied.</param>
        /// <param name="borderColor">The border color for the text. If <see langword="null"/>
        /// then no border will be drawn.</param>
        public static void DrawString(string text, Graphics graphics, Matrix transform, Font font,
            int borderPadding, float orientation, Rectangle bounds,
            Color? backgroundColor, Color? borderColor)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28010",
                    _OBJECT_NAME);

                // Store the original graphics settings
                Matrix originalTransform = graphics.Transform;
                SmoothingMode originalSmoothingMode = graphics.SmoothingMode;
                TextRenderingHint originalTextRenderingHint = graphics.TextRenderingHint;

                try
                {
                    // Set a new transformation matrix, so that the text 
                    // will be drawn with the same orientation as the page.
                    graphics.Transform = transform;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

                    // Apply any specified rotation to the to the graphics object.
                    graphics.RotateTransform(orientation);

                    // The location to draw the text will be based off the bounds location (but may be 
                    // offset to account for a border.
                    Point textLocation = bounds.Location;

                    // Check to see if a background color needs to be filled in.
                    if (backgroundColor != null)
                    {
                        // Use the highlight's region
                        using (Region region = new Region(bounds))
                        {
                            graphics.FillRegion(ExtractBrushes.GetSolidBrush(backgroundColor.Value),
                                region);
                        }
                    }

                    // Check to see if a border needs to be drawn
                    if (borderColor != null)
                    {
                        graphics.DrawPolygon(ExtractPens.GetPen(borderColor.Value),
                            GetVertices(bounds, null));

                        // The location the text is drawn is offset from the padded bounds so that the text
                        // is centered properly.
                        textLocation.Offset(borderPadding, borderPadding);
                    }

                    // Draw the text
                    graphics.DrawString(text, font, Brushes.Black, textLocation,
                        StringFormat.GenericTypographic);
                }
                finally
                {
                    // Restore the original graphics settings.
                    graphics.Transform = originalTransform;
                    graphics.SmoothingMode = originalSmoothingMode;
                    graphics.TextRenderingHint = originalTextRenderingHint;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27958", ex);
            }
        }

        /// <summary>
        /// Retrieves the vertices of the <see cref="Rectangle"/> in either internal
        /// (possibly rotated) coordinate system or in logical (image) coordinates.
        /// </summary>
        /// <param name="bounds">The bounds to compute the vertices for.</param>
        /// <param name="orientation">If <see langword="null"/> than the coordinates
        /// will be returned without any transformation applied; otherwise the
        /// coordinates will be rotated based upon the orientation value.</param>
        /// <returns>The vertices of the <see cref="Rectangle"/>in the specified coordinate
        /// system.</returns>
        public static PointF[] GetVertices(Rectangle bounds, float? orientation)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28011",
                    _OBJECT_NAME);

                // Construct the vertices using the bounds of the text layer object
                PointF[] vertices =
                {
                    new PointF(bounds.Left, bounds.Top),
                    new PointF(bounds.Right, bounds.Top),
                    new PointF(bounds.Right, bounds.Bottom),
                    new PointF(bounds.Left, bounds.Bottom)
                };

                // Rotate the coordinates into the image coordinate system if specified.
                if (orientation != null)
                {
                    using (Matrix transform = new Matrix())
                    {
                        transform.Rotate(orientation.Value);
                        transform.TransformPoints(vertices);
                    }
                }

                return vertices;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27959", ex);
            }
        }
    }
}
