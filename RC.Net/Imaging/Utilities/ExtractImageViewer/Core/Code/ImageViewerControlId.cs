using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.Imaging.Utilities.ExtractImageViewer
{
    /// <summary>
    /// Class to manage user friendly control ID's for image viewer scripting.
    /// </summary>
    internal static class ImageViewerControlId
    {
        public const int OpenImageButton = 0;
        public const int ZoomWindowButton = 2;
        public const int ZoomInButton = 3;
        public const int ZoomOutButton = 4;
        public const int ZoomPreviousButton = 5;
        public const int ZoomNextButton = 6;
        public const int PanButton = 8;
        public const int HighlightTextSplitButton = 9;
        public const int DeleteLayerObjectsButton = 12;
        public const int OpenSubImageWindowButton = 14;
        public const int RotateCounterClockwiseButton = 15;
        public const int RotateClockwiseButton = 16;
        public const int FirstPageButton = 17;
        public const int LastPageButton = 18;
        public const int PreviousPageButton = 19;
        public const int NextPageButton = 20;
        public const int NavigateToPageEditBox = 21;
        public const int PrintButton = 22;
        public const int FitToPageButton = 23;
        public const int FitToWidthButton = 24;
        public const int PreviousTileButton = 26;
        public const int NextTileButton = 27;
        public const int ThumbnailViewerButton = 28;

        /// <summary>
        /// Collection of descriptions for each button ID.
        /// </summary>
        static Dictionary<int, string> Descriptions = BuildDescriptions();

        /// <summary>
        /// Method to build the collection of button IDs and descriptions.
        /// <para><b>Note:</b></para>
        /// This must be manually updated when control ID's are added/removed.
        /// </summary>
        /// <returns>A collection of control ID's to descriptions.</returns>
        static Dictionary<int, string> BuildDescriptions()
        {
            Dictionary<int, string> descriptions = new Dictionary<int, string>();
            descriptions[OpenImageButton] = "Open image file";
            descriptions[ZoomWindowButton] = "Zoom window";
            descriptions[ZoomInButton] = "Zoom in";
            descriptions[ZoomOutButton] = "Zoom out";
            descriptions[ZoomPreviousButton] = "Zoom previous";
            descriptions[ZoomNextButton] = "Zoom next";
            descriptions[PanButton] = "Pan";
            descriptions[HighlightTextSplitButton] = "Highlight text";
            descriptions[DeleteLayerObjectsButton] = "Delete highlights";
            descriptions[OpenSubImageWindowButton] = "Open portion of the image in another window";
            descriptions[RotateCounterClockwiseButton] = "Rotate 90° left";
            descriptions[RotateClockwiseButton] = "Rotate 90° right";
            descriptions[FirstPageButton] = "Go to the first page";
            descriptions[LastPageButton] = "Go to the last page";
            descriptions[PreviousPageButton] = "Go to the previous page";
            descriptions[NextPageButton] = "Go to the next page";
            descriptions[NavigateToPageEditBox] = "Go to a specific page number";
            descriptions[PrintButton] = "Print document";
            descriptions[FitToPageButton] = "Toggle fit to page mode";
            descriptions[FitToWidthButton] = "Toggle fit to width mode";
            descriptions[PreviousTileButton] = "Go to previous image tile";
            descriptions[NextTileButton] = "Go to next image tile";
            descriptions[ThumbnailViewerButton] = "Toggle display of thumbnails window";

            return descriptions;
        }

        /// <summary>
        /// Builds a usage message for the control ID's used by the image viewer
        /// scripting functionality.
        /// </summary>
        /// <returns>A usage message for the control ID's.</returns>
        public static string BuildControlIdHelp()
        {
            StringBuilder sb = new StringBuilder("Toolbar control ids:");
            sb.AppendLine();
            List<int> controlIds = new List<int>(Descriptions.Keys);
            controlIds.Sort();
            foreach (int id in controlIds)
            {
                sb.Append(id);
                sb.Append(" - ");
                sb.AppendLine(Descriptions[id]);
            }

            return sb.ToString();
        }
    }
}
