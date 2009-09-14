using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Represents a COM attribute comparison operation.
    /// </summary>
    [CLSCompliant(false)]
    public class AttributeComparer : IComparer, IComparer<ComAttribute>
    {
        #region AttributeComparer Methods

        /// <summary>
        /// Compares two spatial strings.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>Less than zero if <paramref name="x"/> is less than <paramref name="y"/>;
        /// zero if <paramref name="x"/> equals <paramref name="y"/>; greater than zero if 
        /// <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
        static int CompareSpatialStrings(SpatialString x, SpatialString y)
        {
            // null values come before non-null values
            if (x == null)
            {
                return y == null ? 0 : -1;
            }
            else if (y == null)
            {
                return 1;
            }

            // non-spatial comes before spatial
            bool xHasSpatialInfo = x.HasSpatialInfo();
            bool yHasSpatialInfo = y.HasSpatialInfo();
            if (!xHasSpatialInfo)
            {
                return yHasSpatialInfo ? -1 : 0;
            }
            else if (!yHasSpatialInfo)
            {
                return 1;
            }

            // Earlier pages come first
            int xFirstPageNumber = x.GetFirstPageNumber();
            int yFirstPageNumber = y.GetFirstPageNumber();
            if (xFirstPageNumber < yFirstPageNumber)
            {
                return -1;
            }
            else if (xFirstPageNumber > yFirstPageNumber)
            {
                return 1;
            }

            // Get the bounds of the first page
            SpatialString xFirstPage = x.GetSpecifiedPages(xFirstPageNumber, xFirstPageNumber);
            SpatialString yFirstPage = y.GetSpecifiedPages(yFirstPageNumber, yFirstPageNumber);
            LongRectangle xBounds = xFirstPage.GetOriginalImageBounds();
            LongRectangle yBounds = yFirstPage.GetOriginalImageBounds();
            int xLeft = 0, xTop = 0, xRight = 0, xBottom = 0;
            int yLeft = 0, yTop = 0, yRight = 0, yBottom = 0;
            xBounds.GetBounds(ref xLeft, ref xTop, ref xRight, ref xBottom);
            yBounds.GetBounds(ref yLeft, ref yTop, ref yRight, ref yBottom);

            // If the bounds don't overlap horizontally, the higher attribute comes first
            if (xBottom < yTop)
            {
                return -1;
            }
            else if (yBottom < xTop)
            {
                return 1;
            }

            // Calculate the horizontal overlap
            // Note that this overlap can be greater than the minHeight in the case where one 
            // zone is horizontally contained within the other. We will cap it at minHeight.
            int minHeight = Math.Min(xBottom - xTop, yBottom - yTop);
            int minOverlap = Math.Min(xBottom - yTop, yBottom - xTop);
            if (minHeight < minOverlap)
            {
                minOverlap = minHeight;
            }

            // If the overlap is greater than 20% of smaller zone 
            // then the leftmost attribute comes first
            double percentOverlap = minOverlap * 100.0 / minHeight;
            if (percentOverlap >= 20)
            {
                if (xLeft < yLeft)
                {
                    return -1;
                }
                else if (xLeft > yLeft)
                {
                    return 1;
                }
            }

            // Lastly, the higher attribute comes first
            return xTop - yTop;
        }

        #endregion AttributeComparer Methods

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
            return Compare(x as ComAttribute, y as ComAttribute);
        }

        #endregion IComparer Members

        #region IComparer<ComAttribute> Members

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, 
        /// or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>Less than zero if <paramref name="x"/> is less than <paramref name="y"/>;
        /// zero if <paramref name="x"/> equals <paramref name="y"/>; greater than zero if 
        /// <paramref name="x"/> is greater than <paramref name="y"/>.</returns>
        public int Compare(ComAttribute x, ComAttribute y)
        {
            // If these are the same object they are equal
            if (ComAttribute.ReferenceEquals(x, y))
            {
                return 0;
            }

            // null comes before non-null
            if (x == null)
            {
                return -1;
            }
            else if (y == null)
            {
                return 1;
            }

            return CompareSpatialStrings(x.Value, y.Value);
        }

        #endregion IComparer<ComAttribute> Members

        
    }
}
