namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Specifies the mode to scale the image within the visible area.
    /// </summary>
    public enum FitMode
    {
        /// <summary>
        /// Image is not scaled in any particular way.
        /// </summary>
        None,

        /// <summary>
        /// Image is scaled so that the whole page fills the visible area and the original 
        /// proportions of the image are maintained.
        /// </summary>
        FitToPage,

        /// <summary>
        /// Image is scaled so the width of the image fills the visible area and the original 
        /// proportions of the image are maintained.
        /// </summary>
        FitToWidth
    }
}