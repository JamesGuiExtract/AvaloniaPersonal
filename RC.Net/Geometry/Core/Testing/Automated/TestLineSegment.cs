using System;
using System.Drawing;
using Extract.Testing.Utilities;
using NUnit.Framework;

namespace Extract.Geometry.Test
{
    /// <summary>
    /// Represents a grouping of tests for the <see cref="TPLineSegment"/> class.
    /// </summary>
    [Category("Geometry")]
    [TestFixture]
    public class TestLineSegment
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
        /// Tests whether a line segment can be created from one point to another.
        /// </summary>
        [Test]
        public static void CreateFromPointToPoint()
        {
            // Given
            PointF start = new PointF(1, 2);
            PointF end = new PointF(3, 4);

            // When
            TPLineSegment segment = new TPLineSegment(start, end);

            // Then
            Assert.That(segment.m_p1 == start && segment.m_p2 == end);
        }

        /// <summary>
        /// Tests whether a line segment can be created from coordinates.
        /// </summary>
        [Test]
        public static void CreateFromCoordinates()
        {
            // Given
            float startX = 1;
            float startY = 2;
            float endX = 3;
            float endY = 4;

            // When
            TPLineSegment segment = new TPLineSegment(startX, startY, endX, endY);

            // Then
            Assert.That(segment.m_p1.X == startX);
            Assert.That(segment.m_p1.Y == startY);
            Assert.That(segment.m_p2.X == endX);
            Assert.That(segment.m_p2.Y == endY);
        }

        /// <summary>
        /// Tests whether a line segment can be created from another line segment.
        /// </summary>
        [Test]
        public static void CreateFromLineSegment()
        {
            // Given
            PointF start = new PointF(1, 2);
            PointF end = new PointF(3, 4);
            TPLineSegment oldSegment = new TPLineSegment(start, end);

            // When
            TPLineSegment newSegment = new TPLineSegment(oldSegment);

            // Then
            Assert.That(newSegment.m_p1 == start && newSegment.m_p2 == end);
        }

        /// <summary>
        /// Tests the slope is accurate.
        /// </summary>
        [Test]
        public static void CalculateSlope()
        {
            // Given
            TPLineSegment segment = new TPLineSegment(0, 0, 1, 2);

            // When
            double slope = segment.getSlope();

            // Then
            Assert.That(Math.Abs(slope - 2.0) < 1e-8);
        }

        /// <summary>
        /// Tests vertical slope can be calculated.
        /// </summary>
        [Test]
        public static void CalculateVerticalSlope()
        {
            // Given
            TPLineSegment segment = new TPLineSegment(0, 0, 0, 1);

            // When
            double slope = segment.getSlope();

            // Then
            Assert.That(Math.Abs(slope - double.MaxValue) < 1e-8);
        }

        /// <summary>
        /// Tests whether a line segment can detect a contained point.
        /// </summary>
        [Test]
        public static void ContainsPoints()
        {
            // Given
            TPLineSegment segment = new TPLineSegment(0,0,2,2);
            PointF point = new PointF(1,1);

            // When
            bool contains = segment.contains(point);

            // Then
            Assert.That(contains);
        }

        /// <summary>
        /// Tests whether a vertical line segment can detect a contained point.
        /// </summary>
        [Test]
        public static void ContainsPointOnVertical()
        {
            // Given
            TPLineSegment segment = new TPLineSegment(0,0,0,2);
            PointF point = new PointF(0, 1);

            // When
            bool contains = segment.contains(point);

            // Then
            Assert.That(contains);
        }

        /// <summary>
        /// Tests whether a line segment can detect an uncontained point.
        /// </summary>
        [Test]
        public static void DoesNotContainPoints()
        {
            // Given
            TPLineSegment segment = new TPLineSegment(0, 0, 2, 2);
            PointF point = new PointF(1, 2);

            // When
            bool contains = segment.contains(point);

            // Then
            Assert.That(!contains);
        }

        /// <summary>
        /// Tests whether a vertical line segment can detect an uncontained point.
        /// </summary>
        [Test]
        public static void DoesNotContainPointOnVertical()
        {
            // Given
            TPLineSegment segment = new TPLineSegment(0, 0, 0, 2);
            PointF point = new PointF(1, 2);

            // When
            bool contains = segment.contains(point);

            // Then
            Assert.That(!contains);
        }

        /// <summary>
        /// Tests whether a line segment can detect an uncontained point along the same line.
        /// </summary>
        [Test]
        public static void DoesNotContainPointsAlongSameLine()
        {
            // Given
            TPLineSegment segment = new TPLineSegment(0, 0, 2, 2);
            PointF point = new PointF(3, 3);

            // When
            bool contains = segment.contains(point);

            // Then
            Assert.That(!contains);
        }

        /// <summary>
        /// Tests whether a vertical line segment can detect an uncontained point.
        /// </summary>
        [Test]
        public static void DoesNotContainPointOnVerticalAlongSameLine()
        {
            // Given
            TPLineSegment segment = new TPLineSegment(0, 0, 0, 2);
            PointF point = new PointF(0, 3);

            // When
            bool contains = segment.contains(point);

            // Then
            Assert.That(!contains);
        }

        /// <summary>
        /// Tests whether line segment can detect intersection with another line segment.
        /// </summary>
        [Test]
        public static void IntersectsLineSegment()
        {
            // Given
            TPLineSegment segment1 = new TPLineSegment(0, 0, 2, 2);
            TPLineSegment segment2 = new TPLineSegment(2, 0, 0, 2);

            // When
            PointF intersection;
            bool intersects = segment1.intersects(segment2, out intersection);

            // Then
            Assert.That(intersects);
            Assert.That(intersection.X == 1 && intersection.Y == 1);
        }

        /// <summary>
        /// Tests whether a vertical line segment can detect intersection with another line 
        /// segment.
        /// </summary>
        [Test]
        public static void IntersectsVerticalLineSegment()
        {
            // Given
            TPLineSegment segment1 = new TPLineSegment(0, 0, 0, 2);
            TPLineSegment segment2 = new TPLineSegment(0, 1, 2, 3);

            // When
            PointF intersection;
            bool intersects = segment1.intersects(segment2, out intersection);

            // Then
            Assert.That(intersects);
            Assert.That(intersection.X == 0 && intersection.Y == 1);
        }

        /// <summary>
        /// Tests whether line segment can detect absence of intersection with another line 
        /// segment.
        /// </summary>
        [Test]
        public static void DoesNotIntersectLineSegment()
        {
            // Given
            TPLineSegment segment1 = new TPLineSegment(0, 0, 2, 2);
            TPLineSegment segment2 = new TPLineSegment(4, 2, 2, 4);

            // When
            PointF intersection;
            bool intersects = segment1.intersects(segment2, out intersection);

            // Then
            Assert.That(!intersects);
        }

        /// <summary>
        /// Tests whether a vertical line segment can detect the absence of intersection with 
        /// another line segment.
        /// </summary>
        [Test]
        public static void DoesNotIntersectVerticalLineSegment()
        {
            // Given
            TPLineSegment segment1 = new TPLineSegment(0, 0, 0, 2);
            TPLineSegment segment2 = new TPLineSegment(1, 1, 2, 0);

            // When
            PointF intersection;
            bool intersects = segment1.intersects(segment2, out intersection);

            // Then
            Assert.That(!intersects);
        }

        /// <summary>
        /// Tests whether line segment can detect absence of intersection with another line 
        /// segment.
        /// </summary>
        [Test]
        public static void DoesNotIntersectParallelLineSegment()
        {
            // Given
            TPLineSegment segment1 = new TPLineSegment(0, 0, 2, 2);
            TPLineSegment segment2 = new TPLineSegment(1, 1, 3, 3);

            // When
            PointF intersection;
            bool intersects = segment1.intersects(segment2, out intersection);

            // Then
            Assert.That(!intersects);
        }

        /// <summary>
        /// Tests whether a vertical line segment can detect the absence of intersection with 
        /// another line segment.
        /// </summary>
        [Test]
        public static void DoesNotIntersectParallelVerticalLineSegment()
        {
            // Given
            TPLineSegment segment1 = new TPLineSegment(0, 0, 0, 2);
            TPLineSegment segment2 = new TPLineSegment(1, 0, 1, 2);

            // When
            PointF intersection;
            bool intersects = segment1.intersects(segment2, out intersection);

            // Then
            Assert.That(!intersects);
        }

        // TODO: Test comparison
        // TODO: Test GetHashCode
    }
}
