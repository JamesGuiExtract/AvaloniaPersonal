using Extract.Licensing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

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
        static readonly string _OBJECT_NAME = typeof(GeometryMethods).ToString();

        #endregion Constants

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
        public static double GetAngle(PointF start, PointF end)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23162",
					_OBJECT_NAME);

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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23163",
					_OBJECT_NAME);

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
        /// Retrieves the center point of a vector of points.
        /// </summary>
        /// <param name="points">The <see cref="PointF"/>s for which a center point should be found.
        /// </param>
        /// <returns>The <see cref="PointF"/> that lines in the center of <see paramref="points"/>.
        /// </returns>
        public static PointF GetCenterPoint(params PointF[] points)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23164",
					_OBJECT_NAME);

                float x = points.Select(point => point.X).Average();
                float y = points.Select(point => point.Y).Average();

                return new PointF(x, y);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22206", ex);
            }
        }

        /// <summary>
        /// Expands the points outward from the center. For a vector of 2 points, the points are
        /// expanded directly away from the center. For a vector of 4 points, the points are assumed
        /// to represent the corners of a rectangle and are therefore expanded by the specified
        /// number of pixels along each axis in order to expand the 
        /// </summary>
        /// <param name="distance">The distance.</param>
        /// <param name="points">The points.</param>
        /// <throws><see cref="ExtractException"/> if <see paramref="points"/> does not contain
        /// either 2 or 4 points.</throws>
        public static void ExpandPoints(float distance, PointF[] points)
        {
            try
            {
                PointF centerPoint = GetCenterPoint(points);

                int count = points.Length;
                if (points.Length == 2)
                {
                    // Expand each point directly away from the center point.
                    for (int i = 0; i < count; i++)
                    {
                        PointF point = points[i];
                        var vector = new System.Windows.Vector(point.X - centerPoint.X, point.Y - centerPoint.Y);
                        vector = System.Windows.Vector.Multiply(distance / vector.Length, vector);
                        points[i] = new PointF(point.X + (float)vector.X, point.Y + (float)vector.Y);
                    }
                }
                else if (points.Length == 4)
                {
                    // Expand each point an equal amount along each axis away from the center point.
                    for (int i = 0; i < count; i++)
                    {
                        PointF point = points[i];
                        float xOffset = point.X - centerPoint.X;
                        if (xOffset != 0)
                        {
                            xOffset = (xOffset * distance) / Math.Abs(xOffset);
                        }
                        float yOffset = point.Y - centerPoint.Y;
                        if (yOffset != 0)
                        {
                            yOffset = (yOffset * distance) / Math.Abs(yOffset);
                        }
                        points[i] = (new PointF(point.X + xOffset, point.Y + yOffset));
                    }
                }
                else
                {
                    ExtractException ee = new ExtractException("ELI32171",
                        "ExpandPoints called with an invalid number of points!");
                    ee.AddDebugData("Point count", points.Length, false);
                    throw ee;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32175");
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
        public static PointF[] GetVertices(PointF start, PointF end, float height)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23166",
					_OBJECT_NAME);

                // Calculate the slope of the line
                double slope = GetAngle(start, end);

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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23167",
                    _OBJECT_NAME);

                // Calculate the angle of the line
                double angle = GetAngle(start, end);

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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI26445",
					_OBJECT_NAME);

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
        /// Returns the smallest <see cref="RectangleF"/> that contains all specified points.
        /// </summary>
        /// <param name="points">The array of <see cref="PointF"/>s that the resulting rectangle
        /// will contain.</param>
        /// <returns>The smallest <see cref="RectangleF"/> that contains all specified points.
        /// </returns>
        public static RectangleF GetBoundingRectangle(PointF[] points)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI31329",
                    _OBJECT_NAME);

                ExtractException.Assert("ELI31330", "Invalid parameter!", points.Length > 0);

                RectangleF bounds = new RectangleF(points[0], new SizeF(0, 0));

                for (int i = 1; i < points.Length; i++)
                {
                    bounds = RectangleF.FromLTRB(
                        Math.Min(bounds.Left, points[i].X),
                        Math.Min(bounds.Top, points[i].Y),
                        Math.Max(bounds.Right, points[i].X),
                        Math.Max(bounds.Bottom, points[i].Y));
                }

                return bounds;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31331", ex);
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI26446",
					_OBJECT_NAME);

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
                Rectangle rotatedRectangle = GetBoundingRectangle(cornerPoints);

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
        public static double Distance(PointF pointA, PointF pointB)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI26447",
					_OBJECT_NAME);

                // Compute the X and Y distance
                double dX = pointA.X - pointB.X;
                double dY = pointA.Y - pointB.Y;

                // Compute the distance
                return Math.Sqrt(dX * dX + dY * dY);
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
        public static double Area(PointF start, PointF end, float height)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23168",
					_OBJECT_NAME);

                // Compute the area
                double area = Distance(start, end) * height;

                // Return the area
                return area;

            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI22210", ex);
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI25692",
					_OBJECT_NAME);

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

        /// <summary>
        /// Gets the specified angle as an angle between -PI/2 (exclusive) and PI/2 (inclusive)
        /// from horizontal.
        /// </summary>
        /// <param name="radians">The radians to express as a new angle.</param>
        /// <returns>The specified angle as an angle between -PI/2 (exclusive) and PI/2 
        /// (inclusive) from horizontal.</returns>
        public static double GetAngleFromHorizontal(double radians)
        {
            try
            {
                if (radians >= Math.PI / 2.0)
                {
                    return radians - Math.PI;
                }
                else if (radians < -Math.PI / 2.0)
                {
                    return radians + Math.PI;
                }

                return radians;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28618",
                    "Unable to calculate angle from horizontal.", ex);
                ee.AddDebugData("Radians", radians, false);
                throw ee;
            }
        }

        /// <summary>
        /// Rotates the specified point by the specified sin and cosine theta.
        /// </summary>
        /// <param name="point">The point to rotate.</param>
        /// <param name="theta">The angles of rotation.</param>
        /// <returns>The <paramref name="point"/> rotated by the specified sin and cosine 
        /// <paramref name="theta"/>.</returns>
        public static PointF Rotate(PointF point, PointF theta)
        {
            return Rotate(point.X, point.Y, theta.X, theta.Y);
        }

        /// <summary>
        /// Rotates the specified point by the specified sin and cosine theta.
        /// </summary>
        /// <param name="x">The x coordinate of the point to rotate.</param>
        /// <param name="y">The y coordinate of the point to rotate.</param>
        /// <param name="theta">The angles of rotation.</param>
        /// <returns>The point rotated by the specified sin and cosine <paramref name="theta"/>.
        /// </returns>
        // X and Y are descriptive names for a coordinate
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y")]
        public static PointF Rotate(float x, float y, PointF theta)
        {
            return Rotate(x, y, theta.X, theta.Y);
        }

        /// <summary>
        /// Rotates the specified point by the specified sin and cosine theta.
        /// </summary>
        /// <param name="x">The x coordinate of the point to rotate.</param>
        /// <param name="y">The y coordinate of the point to rotate.</param>
        /// <param name="sinTheta">The sin theta by which to rotate the point.</param>
        /// <param name="cosTheta">The cos theta by which to rotate the point.</param>
        /// <returns>The point rotated by the specified <paramref name="sinTheta"/> and 
        /// <paramref name="cosTheta"/>.
        /// </returns>
        // X and Y are descriptive names for a coordinate
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "y")]
        public static PointF Rotate(float x, float y, float sinTheta, float cosTheta)
        {
            try
            {
                return new PointF(x * cosTheta - y * sinTheta, x * sinTheta + y * cosTheta);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28619", ex);
            }
        }

        /// <summary>
        /// Converts a measurement in radians into the equivalent measurement in degrees.
        /// </summary>
        /// <param name="radians">The measurement to convert to degrees.</param>
        /// <returns>The radians converted to degrees.</returns>
        public static double ConvertRadiansToDegrees(double radians)
        {
            return radians * (180.0 / Math.PI);
        }

        /// <summary>
        /// Converts a measurement in degrees into the equivalent measurement in radians.
        /// </summary>
        /// <param name="degrees">The measurement to convert to radians.</param>
        /// <returns>The radians converted to radians.</returns>
        public static double ConvertDegreesToRadians(double degrees)
        {
            return (Math.PI * degrees) / 180.0;
        }

        /// <summary>
        /// Inverts the specified point using the specified transform <see cref="Matrix"/>.
        /// </summary>
        /// <param name="transform">The <see cref="Matrix"/> to be used to transform the
        /// <see paramref="point"/>.</param>
        /// <param name="point">The <see cref="Point"/> to be transformed.</param>
        /// <returns>The inverted <see cref="Point"/>.</returns>
        public static Point InvertPoint(Matrix transform, Point point)
        {
            try
            {
                Point[] pointArray = new Point[] { point };
                InvertPoints(transform, pointArray);
                return pointArray[0];
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30177", ex);
            }
        }

        /// <summary>
        /// Inverts the specified point using the specified transform <see cref="Matrix"/>.
        /// </summary>
        /// <param name="transform">The <see cref="Matrix"/> to be used to transform the
        /// <see paramref="point"/>.</param>
        /// <param name="point">The <see cref="PointF"/> to be transformed.</param>
        /// <returns>The inverted <see cref="PointF"/>.</returns>
        public static PointF InvertPoint(Matrix transform, PointF point)
        {
            try
            {
                PointF[] pointArray = new PointF[] { point };
                InvertPoints(transform, pointArray);
                return pointArray[0];
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30178", ex);
            }
        }

        /// <summary>
        /// Inverts the specified points using the specified transform <see cref="Matrix"/>.
        /// </summary>
        /// <param name="transform">The <see cref="Matrix"/> to be used to transform the
        /// <see paramref="points"/>.</param>
        /// <param name="points">The <see cref="Point"/>s to be transformed.</param>
        public static void InvertPoints(Matrix transform, Point[] points)
        {
            try
            {
                if (transform.IsInvertible)
                {
                    using (Matrix inverseMatrix = transform.Clone())
                    {
                        inverseMatrix.Invert();
                        inverseMatrix.TransformPoints(points);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30179", ex);
            }
        }

        /// <summary>
        /// Inverts the specified points using the specified transform <see cref="Matrix"/>.
        /// </summary>
        /// <param name="transform">The <see cref="Matrix"/> to be used to transform the
        /// <see paramref="points"/>.</param>
        /// <param name="points">The <see cref="PointF"/>s to be transformed.</param>
        public static void InvertPoints(Matrix transform, PointF[] points)
        {
            try
            {
                if (transform.IsInvertible)
                {
                    using (Matrix inverseMatrix = transform.Clone())
                    {
                        inverseMatrix.Invert();
                        inverseMatrix.TransformPoints(points);
                    }
                }

            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30180", ex);
            }
        }

        #endregion GeometryMethods Methods
    }
}
