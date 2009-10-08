using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

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
        static Dictionary<Color, SolidBrush> _solidBrushes = new Dictionary<Color, SolidBrush>();

        /// <summary>
        /// Mutex used to protect read/write access to the dictionary.
        /// </summary>
        static object _lock = new object();

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
                lock (_lock)
                {
                    if (!_solidBrushes.TryGetValue(color, out brush))
                    {
                        brush = new SolidBrush(color);
                        _solidBrushes.Add(color, brush);
                    }
                }

                return brush;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27977", ex);
            }
        }

        /// <summary>
        /// Disposes of all cached <see cref="Brush"/> objects and clears out the collection.
        /// </summary>
        public static void ClearAndDisposeAllBrushes()
        {
            try
            {
                lock (_lock)
                {
                    CollectionMethods.ClearAndDispose(_solidBrushes);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27978", ex);
            }
        }

        #endregion Methods
    }
}
