using System;
using System.Collections.Generic;
using System.Drawing;

namespace Extract.Geometry
{
    /// <summary>
    /// Represents a simple polygon where every interior angle is less than 180 degrees.
    /// </summary>
    public class TPPolygon
    {
        #region Fields

        float m_dMinX;
        float m_dMinY;
        float m_dMaxX;
        float m_dMaxY;

        List<PointF> m_vecPoints = new List<PointF>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TPPolygon"/> class.
        /// </summary>
        public TPPolygon()
        {
            // initialize the max to 0.0 and the min to INFINITY
            m_dMinX = float.MaxValue;
            m_dMinY = float.MaxValue;
            m_dMaxX = 0F;
            m_dMaxY = 0F;
        }

        #endregion Constructors

        /// <summary>
        /// Adds the specified point to <see cref="TPPolygon"/> sequentially.
        /// </summary>
        /// <param name="point">The point to add to <see cref="TPPolygon"/>.</param>
        public void addPoint(PointF point)
        {
            // add the point to the underlying vector
            m_vecPoints.Add(point);
    	
            // recalculate the extents
            calculateExtents(point);
        }
	    
        /// <summary>
        /// Determines whether the specified polygon is fully contained in the 
        /// <see cref="TPPolygon"/>.
        /// </summary>
        /// <param name="poly">The polygon to test for containment.</param>
        /// <returns><see langword="true"/> if <paramref name="poly"/> is fully contained in the 
        /// <see cref="TPPolygon"/>; <see langword="false"/> if any part of 
        /// <paramref name="poly"/> is outside this polygon.</returns>
        public bool contains(TPPolygon poly)
        {
            foreach (PointF inputTPPolygonIter in poly.m_vecPoints)
            {
                if (!encloses(inputTPPolygonIter, true))
                {
                    return false;
                }
            }
    	
            return true;
        }
	    
        /// <summary>
        /// Determines whether the specified point is inside the polygon.
        /// </summary>
        /// <param name="targetPoint">The points to test for enclosure.</param>
        /// <param name="bValueToReturnIfPointOnBorder"><see langword="true"/> if the border 
        /// should be considered inside the polygon; <see langword="false"/> if the border should 
        /// be considered outside.</param>
        /// <returns><see langword="true"/> if <paramref name="targetPoint"/> is inside the 
        /// polygon; <see langword="false"/> if it is outside the polygon.</returns>
        public bool encloses(PointF targetPoint, bool bValueToReturnIfPointOnBorder)
        {
            // first see if the target point is on the boundary itself
            {
                // get the vector of line segments
                List<TPLineSegment> vecSegments = getSegments();
    	
                // for all line segments, check if the segment includes the
                // targetPoint
                for (int i = 0; i < vecSegments.Count; i++)
                {
                    TPLineSegment currSegment = vecSegments[i];
    	
                    // if the point is on the current line segment, then return
                    // what ever value the user wanted returned.
                    if (currSegment.contains(targetPoint))
                    {
                        return bValueToReturnIfPointOnBorder;
                    }
                }
            }
    	
            // create a horizontal line segment that spans horizontally to the right of the specified point
            TPLineSegment horizontalSegment = new TPLineSegment(new PointF(targetPoint.X, targetPoint.Y), 
                new PointF(m_dMaxX, targetPoint.Y));
    	
            // create a vertical line segment that spans vertically top the top of the specified point
            TPLineSegment verticalSegment = new TPLineSegment(new PointF(targetPoint.X, targetPoint.Y), 
                new PointF(targetPoint.X, m_dMaxY));
    	
            // look at the number of times that the horizontal and vertical segments cross the boundary
            List<PointF> horizontalIntersectionPoints = new List<PointF>();
            List<PointF> verticalIntersectionPoints = new List<PointF>();
            int iHorz = 0;
            int iVert = 0;
            {
                for (int i = 0; i < getSegments().Count; i++)
                {
                    TPLineSegment currSegment = getSegments()[i];
    	
                    PointF intersectionPoint = new PointF();
    	
                    if (currSegment.intersects(horizontalSegment, out intersectionPoint))
                    {
                        // if the intersection point has not yet been found, then add it to the list of
                        // points that were found and increment the counter
                        if (!horizontalIntersectionPoints.Contains(intersectionPoint))
                        {
                            horizontalIntersectionPoints.Add(intersectionPoint);
                            iHorz++;
                        }
                    }

                    if (currSegment.intersects(verticalSegment, out intersectionPoint))
                    {
                        // if the intersection point has not yet been found, then add it to the list of
                        // points that were found and increment the counter
                        if (!verticalIntersectionPoints.Contains(intersectionPoint))
                        {
                            verticalIntersectionPoints.Add(intersectionPoint);
                            iVert++;
                        }
                    }
                }
            }
    	
            // the point lies within the boundary if the number of horizontal and vertical intersections
            // are both odd numbers
            return (iHorz > 0) && (iVert > 0) && (iHorz % 2 == 1) && (iVert % 2 == 1);
        }

        /// <summary>
        /// Determines whether the specified polygon intersects this polygon.
        /// </summary>
        /// <param name="poly">The polygon to test for intersection.</param>
        /// <returns><see langword="true"/> if the polygon intersects; <see langword="false"/> if 
        /// the polygon does not intersect.</returns>
        public bool overlaps(TPPolygon poly)
        {
            PointF dummyIntersectionPoint = new PointF();

            // TPPolygon A overlaps TPPolygon B if any of their lines intersect.
            for (int i = 0; i < poly.getSegments().Count; i++)
            {
                for (int j = 0; j < getSegments().Count; j++)
                {
                    if (getSegments()[j].intersects(poly.getSegments()[i],
                        out dummyIntersectionPoint))
                    {
                        return true;
                    }
                }
            }

            // TPPolygon A overlaps TPPolygon B if either one is contained in the other
            if (contains(poly) || poly.contains(this))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to the current 
        /// <see cref="TPPolygon"/>.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with the current 
        /// <see cref="TPPolygon"/>.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="Object"/> is equal to the current 
        /// <see cref="TPPolygon"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return this == (obj as TPPolygon);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. <see cref="GetHashCode"/> is suitable 
        /// for use in hashing algorithms and data structures like a hash table.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="TPPolygon"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified polygons are equivalent.
        /// </summary>
        /// <param name="a">A polygon to compare.</param>
        /// <param name="b">Another polygon to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> is equivalent to 
        /// <paramref name="b"/>; <see langword="false"/> if they differ.</returns>
        public static bool operator ==(TPPolygon a, TPPolygon b)
        {
            // TODO: make more efficient

            if (a.m_vecPoints.Count != b.m_vecPoints.Count)
            {
                return false;
            }

            foreach (PointF itera in a.m_vecPoints)
            {
                bool iterAValueFoundInVectorB = false;
                foreach (PointF iterb in b.m_vecPoints)
                {
                    if (IsEqual(itera, iterb))
                    {
                        iterAValueFoundInVectorB = true;
                    }
                }

                if (!iterAValueFoundInVectorB)
                {
                    return false;
                }
            }

            foreach (PointF iterb in b.m_vecPoints)
            {
                bool iterBValueFoundInVectorA = false;
                foreach (PointF itera in a.m_vecPoints)
                {
                    if (IsEqual(itera, iterb))
                    {
                        iterBValueFoundInVectorA = true;
                    }
                }

                if (!iterBValueFoundInVectorA)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified polygons are differ.
        /// </summary>
        /// <param name="a">A polygon to compare.</param>
        /// <param name="b">Another polygon to compare.</param>
        /// <returns><see langword="true"/> if <paramref name="a"/> differs from 
        /// <paramref name="b"/>; <see langword="false"/> if they are equivalent.</returns>
        public static bool operator !=(TPPolygon a, TPPolygon b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Determines whether the specified points are equal within a small margin of error.
        /// </summary>
        /// <param name="a">A point to compare.</param>
        /// <param name="b">Another point to compare.</param>
        /// <returns><see langword="true"/> if the points are equivalent within a small margin of 
        /// error; <see langword="false"/> otherwise.</returns>
        static bool IsEqual(PointF a, PointF b)
        {
            return Math.Abs(a.X - b.X) < 1e-8 && Math.Abs(a.Y - b.Y) < 1e-8;
        }

        /// <summary>
        /// Determines whether a particular polygon comes before another polygon in left to right, 
        /// top to bottom order.
        /// </summary>
        /// <param name="TPPolygon1">A polygon to test.</param>
        /// <param name="TPPolygon2">Another polygon to test.</param>
        /// <returns><see langword="true"/> if <paramref name="TPPolygon1"/> comes before 
        /// <paramref name="TPPolygon2"/>; <see langword="false"/> otherwise.</returns>
        public static bool operator <(TPPolygon TPPolygon1, TPPolygon TPPolygon2)
        {
            // sort TPPolygons based upon the extents as follows: from left to right, top to bottom.
            if (TPPolygon1.m_dMinX != TPPolygon2.m_dMinX)
            {
                // the x's are not equal - sort left to right
                return TPPolygon1.m_dMinX < TPPolygon2.m_dMinX;
            }
            else
            {
                // the x min's are equal...
                if (TPPolygon1.m_dMaxY != TPPolygon2.m_dMaxY)
                {
                    // the maxy's are not equal...sort top to bottom
                    return TPPolygon1.m_dMaxY > TPPolygon2.m_dMaxY;
                }
                else
                {
                    // the x'mins and ymax's are equal 
                    if (TPPolygon1.m_dMaxX != TPPolygon2.m_dMaxX)
                    {
                        // sort by horizontal width
                        return TPPolygon1.m_dMaxX < TPPolygon2.m_dMaxX;
                    }
                    else
                    {
                        // the horizontal widths are the same - sort by vertical height
                        return TPPolygon1.m_dMinY > TPPolygon2.m_dMinY;
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether a particular polygon comes after another polygon in left to right, 
        /// top to bottom order.
        /// </summary>
        /// <param name="TPPolygon1">A polygon to test.</param>
        /// <param name="TPPolygon2">Another polygon to test.</param>
        /// <returns><see langword="true"/> if <paramref name="TPPolygon1"/> comes after 
        /// <paramref name="TPPolygon2"/>; <see langword="false"/> otherwise.</returns>
        public static bool operator >(TPPolygon TPPolygon1, TPPolygon TPPolygon2)
        {
            return !(TPPolygon1 < TPPolygon2) && TPPolygon1 != TPPolygon2;
        }


        /// <summary>
        /// Determines the area of intersection between this polygon and the specified polygon.
        /// </summary>
        /// <param name="polySecond">The polygon from which to compute intersection area.</param>
        /// <returns>The area of intersection between this polygon and the specified 
        /// <paramref name="polySecond"/>.</returns>
        public double getIntersectionArea(TPPolygon polySecond)
        {
            try
            {
                // Get the points and segments vector of polySecond
                List<PointF> vecPointsSecond = polySecond.m_vecPoints;
                List<TPLineSegment> vecSegmentsSecond = polySecond.getSegments();

                // Intersection polygon
                TPPolygon polyIntersection = new TPPolygon();

                // Get the points of the second polygon that is inside the first polygon
                for (int i = 0; i < vecPointsSecond.Count; i++)
                {
                    // Initialize bIfPointOnBorder to true
                    // so that the point on border will be treated as inside polygon
                    bool bIfPointOnBorder = true;

                    // Check if one point of the second polygon is inside the first polygon
                    bool bInside = encloses(vecPointsSecond[i], bIfPointOnBorder);

                    if (bInside)
                    {
                        // Add this point to collection of intersection polygon vertices
                        polyIntersection.addPoint(vecPointsSecond[i]);
                    }
                }

                // Get the points of the first polygon that is inside the second polygon
                for (int i = 0; i < m_vecPoints.Count; i++)
                {
                    // Initialize bIfPointOnBorder to true
                    // so that the point on border will be treated as inside polygon
                    bool bIfPointOnBorder = true;

                    // Check if one point of the first polygon is inside the second polygon
                    bool bInside = polySecond.encloses(m_vecPoints[i], bIfPointOnBorder);

                    if (bInside)
                    {
                        // Add this point to collection of intersection polygon vertices
                        polyIntersection.addPoint(m_vecPoints[i]);
                    }
                }

                // Go through each line segment from one polygon and find all the intersection
                // points with line segments from the other polygon
                for (int i = 0; i < getSegments().Count; i++)
                {
                    for (int j = 0; j < vecSegmentsSecond.Count; j++)
                    {
                        // Get intersect point
                        PointF interPoint;
                        bool bResult = getSegments()[i].intersects(vecSegmentsSecond[j], out interPoint);

                        if (bResult)
                        {
                            // If the two lines intersect, add the intersection point to
                            // collection of intersection polygon vertices
                            polyIntersection.addPoint(interPoint);
                        }
                    }
                }

                // Add the closing segment
                // polyIntersection.close();

                // Define the area of the intersection polygon
                double dAreaOfIntersection = 0;

                // The intersection should have at least three lines to
                // form a polygon
                if (polyIntersection.VertexCount > 2)
                {
                    // Call to make all the points form a convex polygon
                    // and the points are set to counterclock-wise order
                    polyIntersection.reOrderPointsInPolygon();

                    // Get the area of the intersection polygon
                    dAreaOfIntersection = polyIntersection.getArea();
                }

                return dAreaOfIntersection;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29976", "Error calculating the intersection area of two polygons.", ex);
            }
        }

        /// <summary>
        /// Gets the number of points in the polygon.
        /// </summary>
        /// <value>The number of points in the polygon.</value>
        public int VertexCount
        {
            get
            {
                return m_vecPoints.Count;
            }
        }

        /// <summary>
        /// Gets the outer bounds of the <see cref="TPPolygon"/>.
        /// </summary>
        /// <param name="rdMinX">The smallest x coordinate of the polygon.</param>
        /// <param name="rdMinY">The smallest y coordinate of the polygon.</param>
        /// <param name="rdMaxX">The largest x coordinate of the polygon.</param>
        /// <param name="rdMaxY">The largest y coordinate of the polygon.</param>
        public void getExtents(out double rdMinX, out double rdMinY, out double rdMaxX, out double rdMaxY)
        {
            rdMinX = m_dMinX;
            rdMinY = m_dMinY;
            rdMaxX = m_dMaxX;
            rdMaxY = m_dMaxY;
        }

        /// <summary>
        /// Gets the line segments of the polygon.
        /// </summary>
        /// <returns>The line segments of the polygon.</returns>
        public List<TPLineSegment> getSegments()
        {
            List<TPLineSegment> vecSegments = new List<TPLineSegment>();
            if (m_vecPoints.Count >= 3)
            {
                for (int i = 0; i < m_vecPoints.Count; i++)
                {
                    // The second point
                    int j = i + 1;
    	
                    // if i is the last point, then the second
                    // point should be the first point in m_vecPoints;
                    if (i == m_vecPoints.Count - 1)
                    {
                        j = 0;
                    }
    	
                    // create a new line segment
                    vecSegments.Add(new TPLineSegment(m_vecPoints[i], m_vecPoints[j]));
                }
            }
            else
            {
                throw new ExtractException("ELI29977", "A polygon should have at least 3 vertices.");
            }
    	
            return vecSegments;
        }

        /// <summary>
        /// Gets the area of the polygon.
        /// </summary>
        /// <returns>The area of the polygon.</returns>
        public double getArea()
        {
            double dArea = 0.0;
    	
            // Get the number of lines inside the polygon
            int nLines = VertexCount;
    	
            // Project lines from each vertex to some horizontal line below the lowest part of the polygon.
            // The enclosed region from each line segment is made up of a triangle and rectangle.
            // Sum these areas together noting that the areas outside the polygon eventually
            // cancel as the polygon loops around to the beginning.
    	
            // Note: The only restriction that will be placed on the polygon for this technique to work is
            // that the polygon must not be self intersecting
    	
            // Area = 1/2 * sum(x[i]*y[i+1] - x[i+1]*y[i]) (i = 0...nLines - 2)
    	
            // for detailed description, please refer to:
            // http://local.wasp.uwa.edu.au/~pbourke/geometry/polyarea/
            for (int i = 0; i < nLines; i++)
            {
                int j = (i + 1) % nLines;
                dArea += m_vecPoints[i].X * m_vecPoints[j].Y;
                dArea -= m_vecPoints[i].Y * m_vecPoints[j].X;
            }
            dArea /= 2.0;
    	
            return (dArea < 0.0 ? -dArea : dArea);
        }

        void calculateExtents(PointF p)
        {
            if (m_vecPoints.Count == 0)
            {
                m_dMinX = float.MaxValue;
                m_dMinY = float.MaxValue;
                m_dMaxX = -float.MaxValue;
                m_dMaxY = -float.MaxValue;
            }
    	
            if (p.X < m_dMinX)
            {
                m_dMinX = p.X;
            }
    	
            if (p.Y < m_dMinY)
            {
                m_dMinY = p.Y;
            }
    	
            if (p.X > m_dMaxX)
            {
                m_dMaxX = p.X;
            }
    	
            if (p.Y > m_dMaxY)
            {
                m_dMaxY = p.Y;
            }
        }

        /// <summary>
        /// Reorders the points in the polygon so it does not intersect itself.
        /// </summary>
        void reOrderPointsInPolygon()
        {
            // Get the point whose Y coordinate is the smallest
            PointF minYPoint = m_vecPoints[0];
            for (int i = 1; i < m_vecPoints.Count; i++)
            {
                PointF current = m_vecPoints[i];
                if (current.Y < minYPoint.Y)
                {
                    minYPoint = current;
                }
            }

            CosValueIsSmaller sorter2 = new CosValueIsSmaller(minYPoint);
    	
            // reorder the pointers according to the cos value with m_MinYInPolygon
            m_vecPoints.Sort(sorter2);
        }
    }
}