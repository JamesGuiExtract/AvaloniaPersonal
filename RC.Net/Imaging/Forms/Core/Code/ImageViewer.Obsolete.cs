using Leadtools;
using Leadtools.WinForms;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Extract.Imaging.Forms
{
    sealed partial class ImageViewer
    {
        #region Obsoleted Leadtools Methods and Properties

        /// <summary>
        /// This method has been replaced by the <see cref="CursorTool"/> property.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("This method has been replaced by the CursorTool property.")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public new RasterViewerInteractiveMode InteractiveMode
        {
            // Always throw ExtractException as per Extract Coding Guidelines
            [SuppressMessage("Microsoft.Design",
                "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            get
            {
                throw new ExtractException("ELI21151",
                    "This method has been replaced by the CursorTool property.");
            }
            // Ignore unused parameter
            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                MessageId = "value")]
            set
            {
                throw new ExtractException("ELI21152",
                    "This method has been replaced by the CursorTool property.");
            }
        }

        /// <summary>
        /// This method has been replaced by the <see cref="CursorTool"/> property.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("This method has been replaced by the CursorTool property.")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public new RasterRegionCombineMode InteractiveRegionCombineMode
        {
            // Always throw ExtractException as per Extract Coding Guidelines
            [SuppressMessage("Microsoft.Design",
                "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            get
            {
                throw new ExtractException("ELI21153",
                    "This method has been replaced by the CursorTool property.");
            }
            // Ignore unused parameter
            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                MessageId = "value")]
            set
            {
                throw new ExtractException("ELI21154",
                    "This method has been replaced by the CursorTool property.");
            }
        }

        /// <summary>
        /// This method has been replaced by the <see cref="CursorTool"/> property.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("This method has been replaced by the CursorTool property.")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public new RasterViewerInteractiveRegionType InteractiveRegionType
        {
            // Always throw ExtractException as per Extract Coding Guidelines
            [SuppressMessage("Microsoft.Design",
                "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            get
            {
                throw new ExtractException("ELI21155",
                    "This method has been replaced by the CursorTool property.");
            }
            // Ignore unused parameter
            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                MessageId = "value")]
            set
            {
                throw new ExtractException("ELI21156",
                    "This method has been replaced by the CursorTool property.");
            }
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractiveCenterAt(RasterViewerPointEventArgs e)
        {
            throw new ExtractException("ELI21157",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractiveFloater(RasterViewerPointEventArgs e)
        {
            throw new ExtractException("ELI21158",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractiveMagnifyGlass(RasterViewerLineEventArgs e)
        {
            throw new ExtractException("ELI21159",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractiveModeChanged(EventArgs e)
        {
            throw new ExtractException("ELI21160",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractiveModeEnded(EventArgs e)
        {
            throw new ExtractException("ELI21161",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractivePage(RasterViewerLineEventArgs e)
        {
            throw new ExtractException("ELI21162",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractivePan(RasterViewerLineEventArgs e)
        {
            throw new ExtractException("ELI21163",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractiveRegionCombineModeChanged(EventArgs e)
        {
            throw new ExtractException("ELI21164",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractiveRegionEllipse(RasterViewerRectangleEventArgs e)
        {
            throw new ExtractException("ELI21165",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractiveRegionFreehand(RasterViewerPointsEventArgs e)
        {
            throw new ExtractException("ELI21166",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractiveRegionRectangle(RasterViewerRectangleEventArgs e)
        {
            throw new ExtractException("ELI21167",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractiveRegionTypeChanged(EventArgs e)
        {
            throw new ExtractException("ELI21168",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractiveScale(RasterViewerLineEventArgs e)
        {
            throw new ExtractException("ELI21169",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnCursorToolChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnCursorToolChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnInteractiveZoomTo(RasterViewerRectangleEventArgs e)
        {
            throw new ExtractException("ELI21170",
                "This method has been replaced by the OnCursorToolChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="FitMode"/> property.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("This method has been replaced by the FitMode property.")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public new RasterPaintSizeMode SizeMode
        {
            // Always throw ExtractException as per Extract Coding Guidelines
            [SuppressMessage("Microsoft.Design",
                "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            get
            {
                throw new ExtractException("ELI21171",
                    "This method has been replaced by the FitMode property.");
            }
            // Ignore unused parameter
            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                MessageId = "value")]
            set
            {
                throw new ExtractException("ELI21172",
                    "This method has been replaced by the FitMode property.");
            }
        }

        /// <summary>
        /// This method has been replaced by the <see cref="OnFitModeChanged"/> method.
        /// </summary>
        /// <param name="e">Unused</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method has been replaced by the OnFitModeChanged method.")]
        // Ignore unused parameter
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "e")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        new void OnSizeModeChanged(EventArgs e)
        {
            throw new ExtractException("ELI21173",
                "This method has been replaced by the OnFitModeChanged method.");
        }

        /// <summary>
        /// This method has been replaced by the <see cref="ImageFile"/> property.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Obsolete("This method has been replaced by the ImageFile property.")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        // Don't mark this method as static or else the designer will serialize it.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public new RasterImage Image
        {
            // Always throw ExtractException as per Extract Coding Guidelines
            [SuppressMessage("Microsoft.Design",
                "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
            get
            {
                throw new ExtractException("ELI21174",
                    "This method has been replaced by the ImageFile property.");
            }
            // Ignore unused parameter
            [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters",
                MessageId = "value")]
            set
            {
                throw new ExtractException("ELI21175",
                    "This method has been replaced by the ImageFile property.");
            }
        }

        #endregion
    }
}
