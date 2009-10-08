using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Runtime.Serialization;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents a group of methods for working with <see cref="Font"/>s.
    /// </summary>
    public static class FontMethods
    {
        #region Constants

        /// <summary>
        /// The format string for font description label.
        /// </summary>
        readonly static string _FONT_TEXT_FORMAT = "{0} {1}pt; {2}";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(FontMethods).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

        #endregion Fields

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> containg the <see cref="Font"/>
        /// information in a user friendly readable format (i.e. 'Arial 30pt; Bold')
        /// </summary>
        /// <param name="font">The font to build the string from.</param>
        /// <returns>A user friendly representation of the specified <see cref="Font"/>.</returns>
        public static string GetUserFriendlyFontString(Font font)
        {
            try
            {
                // Check the license
                _licenseCache.Validate("ELI27860");

                // Return the user friendly font string
                return string.Format(CultureInfo.CurrentCulture, _FONT_TEXT_FORMAT,
                    font.Name, font.SizeInPoints, font.Style.ToString());
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27861", ex);
            }
        }

        /// <summary>
        /// Creates a new font in the specified units from another font.
        /// </summary>
        /// <param name="font">The font from which to create a new font.</param>
        /// <param name="verticalResolution">The Y-resolution to compute the font size with.</param>
        /// <param name="unit">The unit of measure for the new font.</param>
        /// <returns>A new font measured by <paramref name="unit"/> from <paramref name="font"/>.
        /// </returns>
        /// <remarks>This method does not dispose of the input font or the output font.</remarks>
        public static Font ConvertFontToUnits(Font font, int verticalResolution, GraphicsUnit unit)
        {
            try
            {
                // Check the license
                _licenseCache.Validate("ELI27979");


                // Check for conversion from one unit to another
                Font newFont;
                if (font.Unit != unit)
                {
                    // Get the input font size in pixels
                    float fontSize = GetFontSizeInPixels(font, verticalResolution);

                    // Get the input font size in the specified units
                    fontSize = ConvertPixelsToEmUnits(fontSize, verticalResolution, unit);

                    // Create the new font with the specified units
                    newFont = new Font(font.FontFamily, fontSize, font.Style, unit, font.GdiCharSet,
                        font.GdiVerticalFont);
                }
                else
                {
                    newFont = (Font) font.Clone();
                }

                return newFont;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27980", ex);
            }
        }

        /// <overloads>Gets a font the fits exactly within the specified size.</overloads>
        /// <summary>
        /// Gets a font the fits exactly within the specified size.
        /// <para><b>Note:</b></para>
        /// This method overload will create a font having a <see cref="FontFamily"/>
        /// value of GenericSerif and a <see cref="FontStyle"/> of Bold.
        /// </summary>
        /// <param name="text">The text to fit.</param>
        /// <param name="graphics">The graphics object that will draw the font.</param>
        /// <param name="size">The size into which the font should fit.</param>
        /// <returns>A font that fits exactly within the specified size.</returns>
        public static Font GetFontThatFits(string text, Graphics graphics, SizeF size)
        {
            return GetFontThatFits(text, graphics, size, FontFamily.GenericSerif,
                FontStyle.Bold);
        }

        /// <summary>
        /// Gets a font the fits exactly within the specified size, with the specified
        /// <see cref="FontFamily"/> and <see cref="FontStyle"/>
        /// </summary>
        /// <param name="text">The text to fit.</param>
        /// <param name="graphics">The graphics object that will draw the font.</param>
        /// <param name="size">The size into which the font should fit.</param>
        /// <param name="family">The family of the font to use.</param>
        /// <param name="style">The style of the font to use.</param>
        /// <returns>A font that fits exactly within the specified size.</returns>
        public static Font GetFontThatFits(string text, Graphics graphics, SizeF size,
            FontFamily family, FontStyle style)
        {
            try
            {
                // Check the license
                _licenseCache.Validate("ELI27981");

                // Guess the size needed
                float guess = Math.Min(size.Width, size.Height) * 1.2F / text.Length;

                // Create a font to use to test the guess
                using (Font font = new Font(family, guess, style, GraphicsUnit.Pixel))
                {
                    // Measure the size of the font
                    SizeF guessSize = graphics.MeasureString(text, font);

                    // Calculate how far from the desired size the guess was
                    SizeF scale = new SizeF(size.Width / guessSize.Width,
                        size.Height / guessSize.Height);

                    // Calculate the actual font size
                    float actual = (scale.Height < scale.Width) ?
                        scale.Height * guess : scale.Width * guess;

                    // Return the actual font
                    return new Font(family, actual, style, GraphicsUnit.Pixel);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27982", ex);
            }
        }

        #endregion Methods

        #region Private Helper Methods

        /// <summary>
        /// Calculates the font size in logical (image) pixels.
        /// </summary>
        /// <param name="font">The font from which to retrieve the font size.</param>
        /// <param name="yResolution">The Y-resolution to compute the font size with.</param>
        /// <returns>The font size in logical (image) pixels.</returns>
        static float GetFontSizeInPixels(Font font, int yResolution)
        {
            switch (font.Unit)
            {
                case GraphicsUnit.Document:
                    // (1 inch = 300 document units)
                    return font.Size * yResolution / 300F;

                case GraphicsUnit.Inch:
                    return font.Size * yResolution;

                case GraphicsUnit.Millimeter:
                    // (1 inch = 25.4 millimeters)
                    return font.Size * yResolution / 25.4F;

                case GraphicsUnit.Pixel:
                    return font.Size;

                case GraphicsUnit.Point:
                    // (1 inch = 72 points)
                    return font.Size * yResolution / 72F;

                case GraphicsUnit.Display:
                case GraphicsUnit.World:
                    throw new NotImplementedException();

                default:
                    ExtractException ee =
                        new ExtractException("ELI27983", "Unexpected graphics unit.");
                    ee.AddDebugData("Unit", font.Unit, false);
                    throw ee;
            }
        }

        /// <summary>
        /// Calculates font size in the units specified.
        /// </summary>
        /// <param name="pixels">The font size in logical (image) pixels.</param>
        /// <param name="yResolution">The Y-resolution to compute the font size with.</param>
        /// <param name="unit">The unit of measure for the new font.</param>
        /// <returns>Converts </returns>
        static float ConvertPixelsToEmUnits(float pixels, int yResolution, GraphicsUnit unit)
        {
            switch (unit)
            {
                case GraphicsUnit.Document:
                    // (1 inch = 300 document units)
                    return pixels * 300F / yResolution;

                case GraphicsUnit.Inch:
                    return pixels / yResolution;

                case GraphicsUnit.Millimeter:
                    // (1 inch = 25.4 millimeters)
                    return pixels * 25.4F / yResolution;

                case GraphicsUnit.Pixel:
                    return pixels;

                case GraphicsUnit.Point:
                    // (1 inch = 72 points)
                    return pixels * 72F / yResolution;

                case GraphicsUnit.Display:
                case GraphicsUnit.World:
                    throw new NotImplementedException();

                default:
                    ExtractException ee = 
                        new ExtractException("ELI27984", "Unexpected graphics unit.");
                    ee.AddDebugData("Unit", unit, false);
                    throw ee;
            }
        }

        #endregion Private Helper Methods
    }
}
