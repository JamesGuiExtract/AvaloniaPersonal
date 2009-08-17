using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

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

        #region Fields

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

        #endregion Fields

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
                _licenseCache.Validate("ELI23161");

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
    }
}
