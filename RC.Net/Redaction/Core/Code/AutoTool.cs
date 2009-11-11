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
        None,

        /// <summary>
        /// The pan tool
        /// </summary>
        Pan,

        /// <summary>
        /// The select layer object tool
        /// </summary>
        SelectLayerObject
    }
}