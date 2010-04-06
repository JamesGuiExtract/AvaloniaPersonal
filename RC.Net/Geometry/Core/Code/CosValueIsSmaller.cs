using System.Collections.Generic;
using System.Drawing;
using Extract.Drawing;

namespace Extract.Geometry
{
    /// <summary>
    /// Represents a comparison of points based on which has a smaller cosine relative to a 
    /// particular point.
    /// </summary>
    class CosValueIsSmaller : IComparer<PointF>
    {
        PointF m_MinYPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosValueIsSmaller"/> class.
        /// </summary>
        public CosValueIsSmaller(PointF minYPoint)
        {
            m_MinYPoint = minYPoint;
        }

        double calCosine(PointF point)
        {
	        // Get the length between one point with the point who has minimum y value
	        double dLength = GeometryMethods.Distance(point, m_MinYPoint);

	        // Initialize the cos value to some impossibly large value
	        // so that if the point that needs to be compared is m_MinYPoint itself
	        // its cos value will be the largest and it will be the last point in the 
	        // polygon of intersection
	        double dCosValue = 2;

	        // If it is not the same point, get the cos value of the angle of the line
	        // whose starting point is m_MinYPoint and end point is p2
	        if (dLength > 0.00001)
	        {
		        dCosValue = (point.X - m_MinYPoint.X)/dLength;
		        if (dCosValue > 1 || dCosValue < -1 )
		        {
			        ExtractException ue = new ExtractException("ELI29978", 
                        "Cosine value of an angle between two points must be within [-1, 1].");
			        ue.AddDebugData("CosValue", dCosValue, false);
			        ue.AddDebugData("point1.m_dX", point.X, false);
			        ue.AddDebugData("point1.m_dY", point.Y, false);
			        ue.AddDebugData("m_MinYPoint.m_dX", m_MinYPoint.X, false);
			        ue.AddDebugData("m_MinYPoint.m_dX", m_MinYPoint.Y, false);
			        throw ue;
		        }
	        }

	        return dCosValue;
        }

        #region IComparer Members

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, 
        /// or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>Less than zero if <paramref name="x"/> is less than <paramref name="y"/>;
        /// zero if <paramref name="x"/> equals <paramref name="y"/>; greater than zero if 
        /// <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
        public int Compare(object x, object y)
        {
            PointF? left = x as PointF?;
            PointF? right = y as PointF?;
            if (left == null)
            {
                return right == null ? 0 : -1;
            }
            else if (right == null)
            {
                return 1;
            }

            return Compare(left.Value, right.Value);
        }

        #endregion IComparer Members

        #region IComparer<PointF> Members

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, 
        /// or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>Less than zero if <paramref name="x"/> is less than <paramref name="y"/>;
        /// zero if <paramref name="x"/> equals <paramref name="y"/>; greater than zero if 
        /// <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
        public int Compare(PointF x, PointF y)
        {
            // the cosine value of the first point with m_MinYPoint
	        double dCosValue1 = calCosine(x);

	        // the cosine value of the first point with m_MinYPoint
	        double dCosValue2 = calCosine(y);

            return dCosValue1.CompareTo(dCosValue2);
        }

        #endregion IComparer<PointF> Members
    }
}