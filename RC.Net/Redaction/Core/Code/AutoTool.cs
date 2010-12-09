namespace Extract.Redaction
{
    /// <summary>
    /// Specifies a set of cursor tools that can be automatically selected after a highlight is 
    /// created.
    /// </summary>
    public enum AutoTool
    {
        /// <summary>
        /// No cursor tool is specified
        /// </summary>
        None = 0,

        /// <summary>
        /// The pan tool
        /// </summary>
        Pan = 1,

        /// <summary>
        /// The select layer object tool
        /// </summary>
        Selection = 2,

        /// <summary>
        /// The zoom layer object tool
        /// </summary>
        Zoom = 3
    }
}