using System.Drawing;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents the start point, end point, and angles used for the autofitting algorithm.
    /// </summary>
    class FittingData
    {
        public PointF LeftTop;
        public PointF RightBottom;
        public PointF Theta;
        public int PageNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="FittingData"/> class.
        /// </summary>
        public FittingData(PointF leftTop, PointF rightBottom, PointF theta, int pageNumber)
        {
            LeftTop = leftTop;
            RightBottom = rightBottom;
            Theta = theta;
            PageNumber = pageNumber;
        }
    }
}