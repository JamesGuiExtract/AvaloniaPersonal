﻿using System.Collections.Generic;
using UCLID_RASTERANDOCRMGMTLib;

namespace WebAPI.Models
{
    /// <summary>
    /// Data model for information on all pages in a document.
    /// </summary>
    public class PagesInfo : IResultData
    {
        /// <summary>
        /// Gets or sets the page count.
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PageInfo"/>s.
        /// </summary>
        public List<PageInfo> PageInfos { get; set; }

        /// <summary>
        /// error information, if Error.ErrorOccurred = true
        /// </summary>
        public ErrorInfo Error { get; set; }
    }

    /// <summary>
    /// Data for a specific document page.
    /// </summary>
    public class PageInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PageInfo"/> class.
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
        /// Gets or sets the page.
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the orientation (in degrees) the page should display in by default.
        /// </summary>
        public int DisplayOrientation { get; set; }

        /// <summary>
        /// Gets or sets the width of the page (in pixels).
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the page (in pixels).
        /// </summary>
        public int Height { get; set; }
    }
}
