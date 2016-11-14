using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents a container for <see cref="Brush"/> objects. 
    /// </summary>
    public static class ExtractBrushes
    {
        #region Fields

        /// <summary>
        /// The cached collection of <see cref="SolidBrush"/> objects.
        /// </summary>
        static ThreadLocal<Dictionary<Color, SolidBrush>> _solidBrushes =
            new ThreadLocal<Dictionary<Color, SolidBrush>>(() =>
                new Dictionary<Color, SolidBrush>());

        #endregion Fields

        #region Methods

        /// <summary>
        /// Gets a <see cref="SolidBrush"/> of the specified <see cref="Color"/>.
        /// </summary>
        /// <param name="color">The <see cref="Color"/> of the <see cref="SolidBrush"/>
        /// to return.</param>
        /// <returns>A <see cref="SolidBrush"/> of the specified <see cref="Color"/>.</returns>
        public static SolidBrush GetSolidBrush(Color color)
        {
            try
            {
                SolidBrush brush;
                if (!_solidBrushes.Value.TryGetValue(color, out brush))
                {
                    brush = new SolidBrush(color);
                    _solidBrushes.Value.Add(color, brush);
                }

                return brush;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27977", ex);
            }
        }

        #endregion Methods
    }
}
