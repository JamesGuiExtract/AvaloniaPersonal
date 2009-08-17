using Extract;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;

namespace Extract.Drawing
{
    /// <summary>
    /// Represents a grouping of methods for performing geometric calculations.
    /// </summary>
    public static class GeometryMethods
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(GeometryMethods).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// License cache for validating the license.
        /// </summary>
        static LicenseStateCache _licenseCache =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

        #endregion Fields

        #region GeometryMethods Methods

        /// <summary>
        /// Gets the angle of incline in radians from the start point to the end point.
        /// </summary>
        /// <param name="start">The start point of reference for the angle.</param>
        /// <param name="end">The end point of reference for the angle.</param>
        /// <returns>The angle of incline in radians from the start point to the end point 
        /// expressed as a value between positive (inclusive) and negative (exclusive) 
        /// <see cref="Math.PI"/>.
        /// </returns>
        /// <remarks>The angle is measured counterclockwise from the horizontal line that contains 
        /// <paramref name="start"/> and the line containing both 
        /// <paramref name="start"/> &amp; <paramref name="end"/>.</remarks>
        public static double GetAngle(Point start, Point end)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI23162");

                return Math.Atan2(end.Y - start.Y, end.X - start.X);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22204", ex);
            }
        }

        /// <summary>
        /// Retrieves the endpoint of a line segment after it has been clipped by the specified 
        /// rectangle.
        /// </summary>
        /// <param name="start">A point within the clipping rectangle.</param>
        /// <param name="end">Another point of the clipping rectangle.</param>
        /// <param name="clip">The area within which the endpoint should be clipped.</param>
        /// <returns>The endpoint of a line segment after it has been clipped by the specified 
        /// rectangle.</returns>
        /// <remarks>If <paramref name="start"/> is not contained by <paramref name="clip"/> the 
        /// result is undefined.</remarks>
        // EndPoint is referring to the "ending point", meant in contrast to the "start point".
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId = "EndPoint")]
        public static Point GetClippedEndPoint(Point start, Point end, Rectangle clip)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI23163");

                // Check if the line is vertical
                float deltaX = start.X - end.X;
                if (deltaX == 0)
                {
                    // Crop the line vertically
                    if (end.Y < clip.Top)
                    {
                        return new Point(end.X, clip.Top);
                    }
                    else if (end.Y > clip.Bottom)
                    {
                        return new Point(end.X, clip.Bottom);
                    }

                    // The endpoint is contained in the clipping rectangle
                    return end;
                }

                // Calculate the slope of the line
                float slope = (start.Y - end.Y) / deltaX;

                // Check if the line formed by the new endpoint
                // intersects with the left or right side
                Point result = end;
                if (end.X < clip.Left)
                {
                    result = new Point(clip.Left, (int)(slope * (clip.Left - end.X) + end.Y));
                }
                else if (end.X > clip.Right)
                {
                    result = new Point(clip.Right, (int)(slope * (clip.Right - end.X) + end.Y));
                }

                // Check if the line formed by the new endpoint
                // intersects with the top or bottom side
                if (end.Y < clip.Top)
                {
                    return new Point(
                        (int)(result.X + (clip.Top - result.Y) / slope),
                        clip.Top);
                }
                else if (end.Y > clip.Bottom)
                {
                    return new Point(
                        (int)(result.X + (clip.Bottom - result.Y) / slope),
                        clip.Bottom);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22205", ex);
            }
        }

        /// <summary>
        /// Retrieves the center point of a line in logical (image) coordinates.
        /// </summary>
        /// <param name="start">The starting <see cref="Point"/> of a line segment.</param>
        /// <param name="end">The ending <see cref="Point"/> of a line segment.</param>
        /// <returns>The center point of a line in logical (image) coordinates.</returns>
        public static Point GetCenterPoint(Point start, Point end)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI23164");

                return new Point(
                    (int)((start.X + end.X) / 2.0 + 0.5),
                    (int)((start.Y + end.Y) / 2.0 + 0.5));
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22206", ex);
            }
        }

        /// <overloads>Retrieves the vertices of a highlight.</overloads>
        /// <summary>
        /// Retrieves the vertices of a rectangle with the specified spatial data.
        /// </summary>
        /// <param name="start">The midpoint of one side of the rectangle in logical (image) 
        /// coordinates.</param>
        /// <param name="end">The midpoint of the opposing side of the rectangle in logical 
        /// (image) coordinates.</param>
        /// <param name="height">The distance between two sides of the rectangle measured 
        /// perpendicular to the line formed by <paramref name="start"/> and 
        /// <paramref name="end"/>.</param>
        /// <returns>An array of the four corners of the rectangle.</returns>
        public static Point[] GetVertices(Point start, Point end, int height)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI23165");

                // Calculate the slope of the line
                double slope = GeometryMethods.GetAngle(start, end);

                // Calculate the vertical and horizontal modifiers. These are the values to add and 
                // subtract from the line endpoints to determine the bounds of the rectangle.
                double xModifier = height / 2.0 * Math.Sin(slope);
                double yModifier = height / 2.0 * Math.Cos(slope);

                // Calculate the vertices
                Point[] vertices = new Point[] {
                    new Point((int)(start.X + xModifier + 0.5), (int)(start.Y - yModifier + 0.5)),
                    new Point((int)(start.X - xModifier + 0.5), (int)(start.Y + yModifier + 0.5)),
                    new Point((int)(end.X - xModifier + 0.5), (int)(end.Y + yModifier + 0.5)),
                    new Point((int)(end.X + xModifier + 0.5), (int)(end.Y - yModifier + 0.5))
                };

                return vertices;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22207", ex);
            }
        }

        /// <summary>
        /// Retrieves the vertices of a rectangle with the specified spatial data.
        /// </summary>
        /// <param name="start">The midpoint of one side of the rectangle in logical (image) 
        /// coordinates.</param>
        /// <param name="end">The midpoint of the opposing side of the rectangle in logical 
        /// (image) coordinates.</param>
        /// <param name="height">The distance between two sides of the rectangle measured 
        /// perpendicular to the line formed by <paramref name="start"/> and 
        /// <paramref name="end"/>.</param>
        /// <returns>An array of the four corners of the rectangle.</returns>
        public static PointF[] GetVertices(PointF start, PointF end, int height)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI23166");

                // Calculate the slope of the line
                double slope = GeometryMethods.GetAngle(Point.Truncate(start),
                    Point.Truncate(end));

                // Calculate the vertical and horizontal modifiers. These are the values to add and 
                // subtract from the line endpoints to determine the bounds of the rectangle.
                float xModifier = (float)(height / 2.0 * Math.Sin(slope));
                float yModifier = (float)(height / 2.0 * Math.Cos(slope));

                // Calculate the vertices
                PointF[] vertices = new PointF[] {
                    new PointF(start.X + xModifier, start.Y - yModifier),
                    new PointF(start.X - xModifier, start.Y + yModifier),
                    new PointF(end.X - xModifier, end.Y + yModifier),
                    new PointF(end.X + xModifier, end.Y - yModifier)
                };

                return vertices;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22208", ex);
            }
        }

        /// <summary>
        /// Gets the smallest rectangle that contains the specified angular rectangle.
        /// </summary>
        /// <param name="start">The midpoint of one side of the angular rectangle in logical (image) 
        /// coordinates.</param>
        /// <param name="end">The midpoint of the opposing side of the angular rectangle in logical 
        /// (image) coordinates.</param>
        /// <param name="height">The distance between two sides of the angular rectangle measured 
        /// perpendicular to the line formed by <paramref name="start"/> and 
        /// <paramref name="end"/>.</param>
        /// <returns>The smallest rectangle that contains the angular rectangle.</returns>
        public static Rectangle GetBoundingRectangle(Point start, Point end, int height)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI23167");

                // Calculate the angle of the line
                double angle = GeometryMethods.GetAngle(start, end);

                // Calculate the vertical and horizontal modifiers. These are the values to add and 
                // subtract from the line endpoints to determine the bounds of the rectangle.
                double xModifier = Math.Abs(height / 2.0 * Math.Sin(angle));
                double yModifier = Math.Abs(height / 2.0 * Math.Cos(angle));

                // Find the minimum and maximum horizontal points of the line segment
                int minX = start.X;
                int maxX = end.X;
                if (minX > maxX)
                {
                    minX = end.X;
                    maxX = start.X;
                }

                // Find the minimum and maximum vertical points of the line segment
                int minY = start.Y;
                int maxY = end.Y;
                if (minY > maxY)
                {
                    minY = end.Y;
                    maxY = start.Y;
                }

                // Return the result
                return Rectangle.FromLTRB(
                    (int)(minX - xModifier + 0.5), (int)(minY - yModifier + 0.5),
                    (int)(maxX + xModifier + 0.5), (int)(maxY + yModifier + 0.5));

            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22209", ex);
            }
        }

        /// <summary>
        /// Returns the smallest <see cref="Rectangle"/> that contains all specified points.
        /// </summary>
        /// <param name="points">The array of <see cref="Point"/>s that the resulting rectangle
        /// will contain.</param>
        /// <returns>The smallest <see cref="Rectangle"/> that contains all specified points.
        /// </returns>
        public static Rectangle GetBoundingRectangle(Point[] points)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI26445");

                ExtractException.Assert("ELI25671", "Invalid parameter!", points.Length > 0);

                Rectangle bounds = new Rectangle(points[0], new Size(0, 0));

                for (int i = 1; i < points.Length; i++)
                {
                    bounds = Rectangle.FromLTRB(
                        Math.Min(bounds.Left, points[i].X),
                        Math.Min(bounds.Top, points[i].Y),
                        Math.Max(bounds.Right, points[i].X),
                        Math.Max(bounds.Bottom, points[i].Y));
                }

                return bounds;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25670", ex);
            }
        }

        /// <summary>
        /// Rotates the specified rectangle the specified number of degress around the specified
        /// point.
        /// <para><b>Note:</b></para>
        /// If the rotation is not by a multiple of 90 degrees, the resulting rectangle will be the
        /// smallest rectangle that completely contains the rotated original (it will be larger
        /// than the original).
        /// </summary>
        /// <param name="rectangle">The <see cref="Rectangle"/> to be rotated.</param>
        /// <param name="angle">The angle (in degrees) the rectangle should be rotated.</param>
        /// <param name="rotationPoint">The point the rotation should be centered at.</param>
        /// <returns>The rotated <see cref="Rectangle"/>.</returns>
        public static Rectangle RotateRectangle(Rectangle rectangle, double angle, PointF rotationPoint)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI26446");

                // Initialize a vector of point representing the corners of the rectangle.
                Point[] cornerPoints = { rectangle.Location, new Point(rectangle.Right, rectangle.Top),
                    new Point(rectangle.Right, rectangle.Bottom), 
                    new Point(rectangle.Left, rectangle.Bottom) };

                // Apply the specified rotation to the points.
                using (Matrix transform = new Matrix())
                {
                    transform.RotateAt((float)angle, rotationPoint);
                    transform.TransformPoints(cornerPoints);
                }

                // Get the smallest rectangle that contains the rotated points.
                Rectangle rotatedRectangle = GeometryMethods.GetBoundingRectangle(cornerPoints);

                return rotatedRectangle;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25674", ex);
            }
        }

        /// <summary>
        /// Calculates the distance between the two points.
        /// </summary>
        /// <param name="pointA">The first <see cref="Point"/>.</param>
        /// <param name="pointB">The second <see cref="Point"/>.</param>
        /// <returns>The distance between the two points.</returns>
        public static double Distance(Point pointA, Point pointB)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI26447");

                // Compute the X and Y distance
                double dX = pointA.X - pointB.X;
                double dY = pointA.Y - pointB.Y;

                // Compute the distance
                double distance = Math.Sqrt(Math.Pow(dX, 2) + Math.Pow(dY, 2));

                return distance;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25675", ex);
            }
        }

        /// <summary>
        /// Computes the area enclosed by the specified angular rectangle.
        /// </summary>
        /// <param name="start">The midpoint of one side of the angular rectangle in logical (image) 
        /// coordinates.</param>
        /// <param name="end">The midpoint of the opposing side of the angular rectangle in logical 
        /// (image) coordinates.</param>
        /// <param name="height">The distance between two sides of the angular rectangle measured 
        /// perpendicular to the line formed by <paramref name="start"/> and 
        /// <paramref name="end"/>.</param>
        /// <returns>The area of the specified angular rectangle.</returns>
        public static double Area(Point start, Point end, int height)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI23168");

                // Compute the area
                double area = Distance(start, end) * (double)height;

                // Return the area
                return area;

            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22210", ex);
            }
        }

        /// <summary>
        /// Rotates the specified line segment by the specified number of degrees.
        /// </summary>
        /// <param name="start">The start point of reference for the angle.</param>
        /// <param name="end">The end point of reference for the angle.</param>
        /// <param name="angleInDegrees">The angle (in degrees) to rotate the
        /// line segment by.</param>
        /// <returns>An array of <see cref="Point"/> objects containing the
        /// rotated start and end points.</returns>
        // FxCop does not like passing parameters by reference.  The alternative is to return
        // an order dependent array, or to create a new structure that has a start and end point
        // that can be returned.  It seems less confusing to pass the parameters as a reference
        // and to just update their value.
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#")]
        public static void RotateLineSegmentByAngle(ref Point start, ref Point end,
            double angleInDegrees)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI23169");

                // Check for coordinates validity
                if (start == end)
                {
                    throw new ExtractException("ELI22193", "Invalid line coordinates!");
                }

                // Calculate the slope in radians
                double slope = (angleInDegrees * Math.PI) / 180.0;
                double sinSlope = Math.Sin(slope);
                double cosSlope = Math.Cos(slope);

                // Calculate mid point of the line joining start and end points
                Point midpoint = GeometryMethods.GetCenterPoint(start, end);

                // Rotate the zone coordinates using Rotation and Translation matrix
                Point newStart = new Point(
                    (int)((start.X * cosSlope) - (start.Y * sinSlope) - (midpoint.X * cosSlope)
                    + (midpoint.Y * sinSlope) + midpoint.X),
                    (int)((start.Y * cosSlope) + (start.X * sinSlope) - (midpoint.X * sinSlope)
                    - (midpoint.Y * cosSlope) + midpoint.Y));
                Point newEnd = new Point(
                    (int)((end.X * cosSlope) - (end.Y * sinSlope) - (midpoint.X * cosSlope)
                    + (midpoint.Y * sinSlope) + midpoint.X),
                    (int)((end.Y * cosSlope) + (end.X * sinSlope) - (midpoint.X * sinSlope)
                    - (midpoint.Y * cosSlope) + midpoint.Y));

                // Set the rotated start and end points
                start = newStart;
                end = newEnd;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22211", ex);
            }
        }

        /// <overloads>Calculates the angle from <see paramref="startAngle"/> to 
        /// <see paramref="endAngle"/>.</overloads>
        /// <summary>
        /// Calculates the angle from <see paramref="startAngle"/> to <see paramref="endAngle"/>.
        /// The calculated angle will always be in the range 180 to -180 degrees or PI to -PI
        /// radians.</summary>
        /// <param name="startAngle">The starting angle.</param>
        /// <param name="endAngle">The ending angle.</param>
        /// <param name="degrees"><see langword="true"/> if the calculation should be performed using
        /// degrees, <see langword="false"/> if the calculation should be performed using radians.
        /// </param>
        /// <returns>The angle from <see paramref="startAngle"/> to <see paramref="endAngle"/>.
        /// </returns>
        public static double GetAngleDelta(double startAngle, double endAngle, bool degrees)
        {
            try
            {
                return GetAngleDelta(startAngle, endAngle, degrees ? 360 : Math.PI * 2);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26227", ex);
            }
        }

        /// <summary>
        /// Calculates the angle from <see paramref="startAngle"/> to <see paramref="endAngle"/>.
        /// The calculated angle will always be in the range roundOff/2 to -roundOff/2 degrees.
        /// </summary>
        /// <param name="startAngle">The starting angle.</param>
        /// <param name="endAngle">The ending angle.</param>
        /// <param name="roundOff">The result will be rounded to the nearest multiple of this angle.
        /// </param>
        /// <returns>The angle from <see paramref="startAngle"/> to <see paramref="endAngle"/>
        /// rounded to the nearest <see paramref="roundOff"/></returns>
        public static double GetAngleDelta(double startAngle, double endAngle, double roundOff)
        {
            try
            {
                // Validate the license
                _licenseCache.Validate("ELI25692");

                // Get the difference between the two angles.
                double delta = endAngle - startAngle;

                double halfRoundoff = (roundOff / 2);

                // If the angle is > halfRoundoff degrees, rotate it into the range -halfRoundoff
                // to halfRoundoff
                while (delta > halfRoundoff)
                {
                    delta -= roundOff;
                }

                // If the angle is <= halfRoundoff degrees, rotate it into the range -halfRoundoff
                // to halfRoundoff
                while (delta <= -halfRoundoff)
                {
                    delta += roundOff;
                }

                return delta;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25691", ex);
            }
        }

        #endregion GeometryMethods Methods
    }
}
