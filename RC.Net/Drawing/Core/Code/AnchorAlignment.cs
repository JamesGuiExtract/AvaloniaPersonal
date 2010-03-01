using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Drawing
{
    /// <summary>
    /// Specifies a set of anchor points.
    /// </summary>
    public enum AnchorAlignment
    {
        /// <summary>
        /// The left-bottom point.
        /// </summary>
        LeftBottom,

        /// <summary>
        /// The center-bottom point.
        /// </summary>
        Bottom,

        /// <summary>
        /// The bottom-right point.
        /// </summary>
        RightBottom,

        /// <summary>
        /// The left-center point.
        /// </summary>
        Left,

        /// <summary>
        /// The center point.
        /// </summary>
        Center,

        /// <summary>
        /// The right-center point.
        /// </summary>
        Right,

        /// <summary>
        /// The left-top point.
        /// </summary>
        LeftTop,

        /// <summary>
        /// The center-top point.
        /// </summary>
        Top,

        /// <summary>
        /// The right top point.
        /// </summary>
        RightTop
    }

    /// <summary>
    /// Class of helper methods for working with the <see cref="AnchorAlignment"/> class.
    /// </summary>
    public static class AnchorAlignmentHelper
    {
        /// <summary>
        /// Gets a human readable string representation of the specified alignment."
        /// </summary>
        /// <param name="alignment">The alignment to get a string for.</param>
        /// <returns>A human readble string representation of the specified aligment.</returns>
        public static string GetAlignmentAsString(AnchorAlignment alignment)
        {
            try
            {
                switch (alignment)
                {
                    case AnchorAlignment.LeftBottom:
                        return "Left bottom";

                    case AnchorAlignment.Bottom:
                        return "Center bottom";

                    case AnchorAlignment.RightBottom:
                        return "Right bottom";

                    case AnchorAlignment.Left:
                        return "Left center";

                    case AnchorAlignment.Center:
                        return "Center";

                    case AnchorAlignment.Right:
                        return "Right center";

                    case AnchorAlignment.LeftTop:
                        return "Left top";

                    case AnchorAlignment.Top:
                        return "Center top";

                    case AnchorAlignment.RightTop:
                        return "Right top";

                    default:
                        ExtractException.ThrowLogicException("ELI29865");
                        return ""; // Added so code will compile
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29867", ex);
            }
        }
    }
}
