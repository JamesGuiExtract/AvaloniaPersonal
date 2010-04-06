using System.Drawing;
using Extract.Testing.Utilities;
using NUnit.Framework;

namespace Extract.Geometry.Test
{
    /// <summary>
    /// Represents a grouping of tests for the <see cref="TPPolygon"/> class.
    /// </summary>
    [Category("Geometry")]
    [TestFixture]
    public class TestConvexPolygon
    {
        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Tests whether polygon can detect contained point.
        /// </summary>
        [Test]
        public static void ContainsPoint()
        {
            // Given
            TPPolygon square = CreateSquare(2);

            // When
            bool contains = square.encloses(new PointF(1,1), false);

            // Then
            Assert.That(contains);
        }

        /// <summary>
        /// Tests whether polygon can detect uncontained point.
        /// </summary>
        [Test]
        public static void DoesNotContainPoint()
        {
            // Given
            TPPolygon square = CreateSquare(2);

            // When
            bool contains = square.encloses(new PointF(3, 3), false);

            // Then
            Assert.That(!contains);
        }

        /// <summary>
        /// Tests whether polygon can detect a point contained on the border.
        /// </summary>
        [Test]
        public static void ContainsPointOnBorder()
        {
            // Given
            TPPolygon square = CreateSquare(2);

            // When
            bool contains = square.encloses(new PointF(1, 2), true);

            // Then
            Assert.That(contains);
        }

        /// <summary>
        /// Tests whether polygon can detect point not contained on border.
        /// </summary>
        [Test]
        public static void DoesNotContainPointOnBorder()
        {
            // Given
            TPPolygon square = CreateSquare(2);

            // When
            bool contains = square.encloses(new PointF(1, 2), false);

            // Then
            Assert.That(!contains);
        }

        /// <summary>
        /// Tests whether a polygon can calculate its area.
        /// </summary>
        [Test]
        public static void CalculatesArea()
        {
            // Given
            TPPolygon square = CreateSquare(3);

            // When
            double area = square.getArea();

            // Then
            Assert.That(area == 9.0);
        }

        /// <summary>
        /// Tests whether the intersection area of overlapping polygons can be computed.
        /// </summary>
        [Test]
        public static void CalculatesIntersectionArea()
        {
            // Given
            TPPolygon square1 = CreateSquare(3);
            TPPolygon square2 = CreateSquare(3, 1);

            // When
            double area = square1.getIntersectionArea(square2);

            // Then
            Assert.That(area == 4.0);
        }

        /// <summary>
        /// Tests whether the intersection area of a contained polygon can be computed.
        /// </summary>
        [Test]
        public static void CalculatesIntersectionAreaOfContainedPolygon()
        {
            // Given
            TPPolygon square1 = CreateSquare(3);
            TPPolygon square2 = CreateSquare(5);

            // When
            double area = square1.getIntersectionArea(square2);

            // Then
            Assert.That(area == 9.0);
        }

        /// <summary>
        /// Tests whether the intersection area of a non-overlapping polygons can be computed.
        /// </summary>
        [Test]
        public static void CalculatesIntersectionAreaOfNonOverlappingPolygon()
        {
            // Given
            TPPolygon square1 = CreateSquare(3);
            TPPolygon square2 = CreateSquare(4, 4);

            // When
            double area = square1.getIntersectionArea(square2);

            // Then
            Assert.That(area == 0.0);
        }

        /// <summary>
        /// Creates a square of the specified size whose lower left coordinate is the origin.
        /// </summary>
        /// <param name="size">The size of the square to create.</param>
        static TPPolygon CreateSquare(float size)
        {
            return CreateSquare(size, 0F);
        }

        /// <summary>
        /// Creates a square of the specified size whose lower left coordinate is {offset, offset}.
        /// </summary>
        /// <param name="size">The size of the square to create.</param>
        /// <param name="offset">The amount the lower left of the square is offset in both x and y.
        /// </param>
        static TPPolygon CreateSquare(float size, float offset)
        {
            float sizeOffset = size + offset;

            TPPolygon square = new TPPolygon();
            square.addPoint(new PointF(offset, offset));
            square.addPoint(new PointF(sizeOffset, offset));
            square.addPoint(new PointF(sizeOffset, sizeOffset));
            square.addPoint(new PointF(offset, sizeOffset));

            return square;
        }

        // TODO: Test comparison
        // TODO: Test GetHashCode
    }
}