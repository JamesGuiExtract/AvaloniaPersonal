using System.Collections.Generic;
using UCLID_RASTERANDOCRMGMTLib;

namespace WebAPI.Models
{
    /// <summary>
    /// Data model for information on all pages in a document
    /// </summary>
    public class PagesInfoResult
    {
        /// <summary>
        /// The page count
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// The <see cref="PageInfo"/>s
        /// </summary>
        public List<PageInfo> PageInfos { get; set; }
    }

    /// <summary>
    /// Data for a specific document page
    /// </summary>
    public class PageInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageInfo"/> class
        /// </summary>
        /// <param name="page"></param>
        /// <param name="spatialPageInfo"></param>
        public PageInfo(int page, SpatialPageInfo spatialPageInfo)
        {
            Page = page;
            switch (spatialPageInfo.Orientation)
            {
                case EOrientation.kRotNone: DisplayOrientation = 0; break;
                case EOrientation.kRotRight: DisplayOrientation = 90; break;
                case EOrientation.kRotDown: DisplayOrientation = 180; break;
                case EOrientation.kRotLeft: DisplayOrientation = 270; break;
            }
            Width = spatialPageInfo.Width;
            Height = spatialPageInfo.Height;
        }

        /// <summary>
        /// The page
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// The orientation (in degrees) the page should display in by default
        /// </summary>
        public int DisplayOrientation { get; set; }

        /// <summary>
        /// The width of the page (in pixels)
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the page (in pixels)
        /// </summary>
        public int Height { get; set; }
    }
}
