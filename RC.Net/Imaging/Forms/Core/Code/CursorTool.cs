namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Specifies tools that involve user interaction with the mouse cursor.
    /// </summary>
    public enum CursorTool
    {
        /// <summary>
        /// No cursor tool selected.
        /// </summary>
        None,

        /// <summary>
        /// Allows the user to zoom in to a particular rectangular region.
        /// </summary>
        ZoomWindow,

        /// <summary>
        /// Allows the user to pan the image in all directions.
        /// </summary>
        Pan,

        /// <summary>
        /// Allows the user to draw an angled rectangular highlight.
        /// </summary>
        AngularHighlight,

        /// <summary>
        /// Allows the user to draw a rectangular highlight with sides perpendicular and parallel 
        /// to the sides of the image.
        /// </summary>
        RectangularHighlight,

        /// <summary>
        /// Allows the user to draw an angled rectangular redaction.
        /// </summary>
        AngularRedaction,

        /// <summary>
        /// Allows the user to draw a rectangular redaction with sides perpendicular and parallel 
        /// to the sides of the image.
        /// </summary>
        RectangularRedaction,

        /// <summary>
        /// Allows the user to specify the default highlight height of angular highlights.
        /// </summary>
        SetHighlightHeight,

        /// <summary>
        /// Allows the user to edit the highlight text of a particular highlight.
        /// </summary>
        EditHighlightText,

        /// <summary>
        /// Allows the user to delete one or more layer objects.
        /// </summary>
        DeleteLayerObjects,

        /// <summary>
        /// Allows the user to select a layer object.
        /// </summary>
        SelectLayerObject,

        /// <summary>
        /// Allows the user to extract a rectangular region of the image.
        /// </summary>
        ExtractImage
    }
}