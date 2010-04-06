using System;
using System.Drawing;

namespace Extract.Geometry
{
    /// <summary>
    /// Represents a part of line that is bounded by two endpoints.
    /// </summary>
    public class TPLineSegment
    {
        #region Fields

        // the following two variables are purposely set to public scope.

        /// <summary>
        /// The start point of the <see cref="TPLineSegment"/>.
        /// </summary>
        public PointF m_p1;

        /// <summary>
        /// The end point of the <see cref="TPLineSegment"/>.
        /// </summary>
        public PointF m_p2;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TPLineSegment"/> class using the 
        /// specified endpoints.
        /// </summary>
        /// <param name="_p1">The start point of the line segment.</param>
        /// <param name="_p2">The end point of the line segment.</param>
        public TPLineSegment(PointF _p1, PointF _p2)
        {
            m_p1 = _p1;
            m_p2 = _p2;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TPLineSegment"/> class using the 
        /// specified coordinates.
        /// </summary>
        /// <param name="startX">The x coordinate of the start point.</param>
        /// <param name="startY">The y coordinate of the start point.</param>
        /// <param name="endX">The x coordinate of the end point.</param>
        /// <param name="endY">The y coordinate of the end point.</param>
        public TPLineSegment(float startX, float startY, float endX, float endY)
        {
            m_p1 = new PointF(startX, startY);
            m_p2 = new PointF(endX, endY);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TPLineSegment"/> class as a copy of 
        /// another line segment.
        /// </summary>
        /// <param name="lineToCopy">The line segment to copy.</param>
        public TPLineSegment(TPLineSegment lineToCopy)
        {
            m_p1 = lineToCopy.m_p1;
            m_p2 = lineToCopy.m_p2;
        }

        #endregion Constructors

        /// <summary>
        /// Determines whether the specified point is on the <see cref="TPLineSegment"/>.
        /// </summary>
        /// <param name="p">The point to test.</param>
        /// <returns><see langword="true"/> if the point is on the <see cref="TPLineSegment"/>; 
        /// <see langword="false"/> if it is not on the <see cref="TPLineSegment"/>.</returns>
        public bool contains(PointF p)
        {
            double dMySlope = getSlope();

            if (Math.Abs(dMySlope) == double.MaxValue)
            {
                // this line is vertical.. the PointF can lie on this line only if the x
                // values match
                if (Math.Abs(m_p1.X - p.X) < 1e-8)
                {
                    // the target PointF is on the imaginary line that extends infinitely,
                    // but we need to check to see if it is in the line segment
                    if (p.Y >= Math.Min(m_p1.Y, m_p2.Y) && p.Y <= Math.Max(m_p1.Y, m_p2.Y))
                    {
                        // the PointF lies on the line segment
                        return true;
                    }
                    else
                    {
                        // the PointF lies in the imaginary line but not on the line segment
                        return false;
                    }
                }
                else
                {
                    // this line is vertical, and the target PointF has a different x value
                    // than this line, so it couldn't be on this line
                    return false;
                }
            }
            else
            {
                // find the PointF on the line segment extended infinitely that
                // is directly above or below the given PointF
                double dY = m_p1.Y + dMySlope * (p.X - m_p1.X);
                PointF PointFOnLine = new PointF(p.X, (float)dY);

                if (PointFOnLine == p)
                {
                    // the specified PointF is somewhere on the imaginary line, but
                    // is it on the line segment?
                    // check the Y coordinates first
                    if (dY >= Math.Min(m_p1.Y, m_p2.Y) && dY <= Math.Max(m_p1.Y, m_p2.Y))
                    {
                        // need to also check if it is within the X boundaries of the segment
                        // [p16 #2691 & #2692]
                        if (p.X >= Math.Min(m_p1.X, m_p2.X) && p.X <= Math.Max(m_p1.X, m_p2.X))
                        {
                            // the PointF lies on the line segment
                            return true;
                        }
                        else
                        {
                            // the PointF lies on the imaginary line, but not on the line segment
                            return false;
                        }
                    }
                    else
                    {
                        // the PointF lies on the imaginary line, but not on the line segment
                        return false;
                    }
                }
                else
                {
                    // the given PointF is not even on the imaginary line
                    return false;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified line segment intersects with this one.
        /// </summary>
        /// <param name="line2">The line segment to test for intersection.</param>
        /// <param name="intersectionPointF">The point at which the line segments intersect. This 
        /// value is meaningless of the line segments do not intersect.</param>
        /// <returns><see langword="true"/> if the line segments intersect; 
        /// <see langword="false"/> the line segments do not intersect.</returns>
        public bool intersects(TPLineSegment line2, out PointF intersectionPointF)
        {
            intersectionPointF = new PointF();

            double dMySlope = getSlope();
            double dLine2Slope = line2.getSlope();

            if (Math.Abs(dMySlope) != double.MaxValue && Math.Abs(dLine2Slope) != double.MaxValue)
            {
                // neither of the two lines are vertical

                // suppose my starting PointF was p1, and s1 was my slope, and
                // the other segment's starting PointF was p2, and its slope was s2,
                // suppose the intersection PointF was (ix, iy)
                // then the intersection PointF Y = p1.y + s1 (ix - p1.x)
                //                       also, Y = p2.y + s2 (ix - p2.x)
                // solving for ix, we get
                // ix = (p2.y - p1.y - s2 * p2.x + s1 * p1.x) / (s1 - s2);

                // before we attempt to calculate the intersection PointF, we should make sure that
                // the two lines are not parallel
                if (Math.Abs(dMySlope - dLine2Slope) < 1e-8)
                {
                    // the two lines are parallel, but they could still be intersecting because
                    // they can have some PointFs in common
                    // TODO: for now return false;
                    return false;
                }

                // calculate the intersection PointF X and Y
                double dIntersectionPointFX = (line2.m_p1.Y - m_p1.Y - dLine2Slope * line2.m_p1.X + dMySlope * m_p1.X) / (dMySlope - dLine2Slope);
                double dIntersectionPointFY = m_p1.Y + dMySlope * (dIntersectionPointFX - m_p1.X);
                intersectionPointF = new PointF((float)dIntersectionPointFX, (float)dIntersectionPointFY);

                // there's intersection only if the intersection PointF's Y
                // is within my Y's and within line2's Y's
                bool b1 = dIntersectionPointFY - Math.Min(m_p1.Y, m_p2.Y) >= -1e-8;
                bool b2 = Math.Max(m_p1.Y, m_p2.Y) - dIntersectionPointFY >= -1e-8;
                bool b3 = dIntersectionPointFY - Math.Min(line2.m_p1.Y, line2.m_p2.Y) >= -1e-8;
                bool b4 = Math.Max(line2.m_p1.Y, line2.m_p2.Y) - dIntersectionPointFY >= -1e-8;

                double d1 = dIntersectionPointFY - Math.Min(m_p1.Y, m_p2.Y);
                double d2 = Math.Max(m_p1.Y, m_p2.Y) - dIntersectionPointFY;
                double d3 = dIntersectionPointFY - Math.Min(line2.m_p1.Y, line2.m_p2.Y);
                double d4 = Math.Max(line2.m_p1.Y, line2.m_p2.Y) - dIntersectionPointFY;

                bool bIntersectionYOK = false;
                if ((dIntersectionPointFY - Math.Min(m_p1.Y, m_p2.Y) >= -1e-8) && 
                    (Math.Max(m_p1.Y, m_p2.Y) - dIntersectionPointFY >= -1e-8) && 
                    (dIntersectionPointFY - Math.Min(line2.m_p1.Y, line2.m_p2.Y) >= -1e-8) && 
                    (Math.Max(line2.m_p1.Y, line2.m_p2.Y) - dIntersectionPointFY >= -1e-8))
                {
                    bIntersectionYOK = true;
                }

                // there's intersection only if the intersection PointF's Y
                // is within my Y's and within line2's Y's
                bool bIntersectionXOK = false;
                if ((dIntersectionPointFX - Math.Min(m_p1.X, m_p2.X) >= -1e-8) && 
                    (Math.Max(m_p1.X, m_p2.X) - dIntersectionPointFX >= -1e-8) && 
                    (dIntersectionPointFX - Math.Min(line2.m_p1.X, line2.m_p2.X) >= -1e-8) && 
                    (Math.Max(line2.m_p1.X, line2.m_p2.X) - dIntersectionPointFX >= -1e-8))
                {
                    bIntersectionXOK = true;
                }

                // there's intersection only if the intersection PointF is already part
                // of the two line segments
                return bIntersectionXOK && bIntersectionYOK;
            }
            else if (Math.Abs(dMySlope) != double.MaxValue)
            {
                // i am not vertical but line 2 is.

                // if line2's x is inbetween my starting and ending x's, there's a chance
                // that we intersesect
                if (line2.m_p1.X >= Math.Min(m_p1.X, m_p2.X) && line2.m_p1.X <= Math.Max(m_p1.X, m_p2.X))
                {
                    // calculate the intersection PointF
                    double dIntersectionPointFY = m_p1.Y + (line2.m_p1.X - m_p1.X) * getSlope();
                    intersectionPointF = new PointF(line2.m_p1.X, (float)dIntersectionPointFY);

                    // line2's x is in the range of my x, but there's intersection only if
                    // the intersection PointF's Y is within my Y's and within line2's Y's
                    if ((dIntersectionPointFY - Math.Min(m_p1.Y, m_p2.Y) >= -1e-8) && 
                        (Math.Max(m_p1.Y, m_p2.Y) - dIntersectionPointFY >= -1e-8) && 
                        (dIntersectionPointFY - Math.Min(line2.m_p1.Y, line2.m_p2.Y) >= -1e-8) && 
                        (Math.Max(line2.m_p1.Y, line2.m_p2.Y) - dIntersectionPointFY >= -1e-8))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    // my x is not in the range of x's of line2, so there could
                    // possibly be no intersection
                    return false;
                }

            }
            else if (Math.Abs(dLine2Slope) != double.MaxValue)
            {
                // i am vertical, but line 2 is not

                // if my x is inbetween the starting and ending x's of line2, there's a chance
                // that we intersesect
                if (m_p1.X >= Math.Min(line2.m_p1.X, line2.m_p2.X) && m_p1.X <= Math.Max(line2.m_p1.X, line2.m_p2.X))
                {
                    // calculate the intersection PointF
                    double dIntersectionPointFY = line2.m_p1.Y + (m_p1.X - line2.m_p1.X) * line2.getSlope();
                    intersectionPointF = new PointF(m_p1.X, (float)dIntersectionPointFY);

                    // my x is in the range of line2's x, but there's intersection only if
                    // the intersection PointF's Y is within my Y's and within line2's Y's
                    if ((dIntersectionPointFY - Math.Min(m_p1.Y, m_p2.Y) >= -1e-8) && 
                        (Math.Max(m_p1.Y, m_p2.Y) - dIntersectionPointFY >= -1e-8) && 
                        (dIntersectionPointFY - Math.Min(line2.m_p1.Y, line2.m_p2.Y) >= -1e-8) && 
                        (Math.Max(line2.m_p1.Y, line2.m_p2.Y) - dIntersectionPointFY >= -1e-8))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }
                else
                {
                    // my x is not in the range of x's of line2, so there could
                    // possibly be no intersection
                    return false;
                }
            }
            else
            {
                // both i and the other line are vertical.
                // Simply return false if the two line overlap with each other
                return false;
            }
        }

        /// <summary>
        /// Calculates the slope from the start point to the end point.
        /// </summary>
        /// <returns>The slope from the start point to the end point.</returns>
        public double getSlope()
        {
            double dY = m_p2.Y - m_p1.Y;
            double dX = m_p2.X - m_p1.X;

            // if the line is vertical, then return a slope of + or - infinity as appropriate.
            if (Math.Abs(dX) < 1e-8)
            {
                return double.MaxValue;
            }
            else
            {
                return dY / dX;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current 
        /// <see cref="TPLineSegment"/>.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Object"/> is equal to the current 
        /// <see cref="TPLineSegment"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <param name="obj">The <see cref="Object"/> to compare with the current 
        /// <see cref="TPLineSegment"/>.</param>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. Suitable for use in hashing 
        /// algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Determines whether the line segments are equivalent.
        /// </summary>
        /// <param name="l1">A line segment to test for equivalence.</param>
        /// <param name="l2">Another line segment to test for equivalence.</param>
        /// <returns><see langword="true"/> if the line segments are equivalent; 
        /// <see langword="false"/> if the line segments differ.</returns>
        public static bool operator ==(TPLineSegment l1, TPLineSegment l2)
        {
            if ((l1.m_p1 == l2.m_p1 && l1.m_p2 == l2.m_p2) || (l1.m_p1 == l2.m_p2 && l1.m_p2 == l2.m_p1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determines whether the line segments differ.
        /// </summary>
        /// <param name="l1">A line segment to test.</param>
        /// <param name="l2">Another line segment to test.</param>
        /// <returns><see langword="true"/> if the line segments differ; <see langword="false"/> 
        /// if the line segments are equivalent.</returns>
        public static bool operator !=(TPLineSegment l1, TPLineSegment l2)
        {
            return !(l1 == l2);
        }

        /// <summary>
        /// Determines whether a line segment comes before another line segment in left to right, 
        /// top to bottom order.
        /// </summary>
        /// <param name="l1">A line segment to compare.</param>
        /// <param name="l2">Another line segment to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="l1"/> comes before 
        /// <paramref name="l2"/>; <see langword="false"/> otherwise.</returns>
        public static bool operator <(TPLineSegment l1, TPLineSegment l2)
        {
            if (IsLessThan(l1.m_p1, l2.m_p1))
            {
                return true;
            }
            else if (l1.m_p1 == l2.m_p1 && IsLessThan(l1.m_p2, l2.m_p2))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether a point comes before another point in left to right, top to bottom 
        /// order.
        /// </summary>
        /// <param name="a">A point to compare.</param>
        /// <param name="b">Another point to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> comes before 
        /// <paramref name="b"/>; <see langword="false"/> otherwise.</returns>
        static bool IsLessThan(PointF a, PointF b)
        {
            // for ordering objects of this type,
            // sort by X first, and then by Y
            if (Math.Abs(a.X - b.X) > 1e-8)
            {
                return (a.X < b.X);
            }
            else
            {
                if (Math.Abs(a.Y - b.Y) > 1e-8)
                {
                    return (a.Y < b.Y);
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether a line segment comes after another line segment in left to right, 
        /// top to bottom order.
        /// </summary>
        /// <param name="l1">A line segment to compare.</param>
        /// <param name="l2">Another line segment to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="l1"/> comes after 
        /// <paramref name="l2"/>; <see langword="false"/> otherwise.</returns>
        public static bool operator >(TPLineSegment l1, TPLineSegment l2)
        {
            return !(l1 < l2) && l1 != l2;
        }
    }
}