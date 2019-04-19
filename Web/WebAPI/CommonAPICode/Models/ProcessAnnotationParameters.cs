namespace WebAPI.Models
{
    /// <summary>
    /// Defines an operation to be performed on an annotation from a web application.
    /// </summary>
    public class ProcessAnnotationParameters
    {
        /// <summary>
        /// "MODIFY" is currently the only type of operation supported
        /// </summary>
        public string OperationType { get; set; }

        /// <summary>
        /// A processor's JSON representation: implementation class + settings
        /// </summary>
        public string Definition { get; set; }

        /// <summary>
        /// Gets or sets the annotation to process
        /// </summary>
        public DocumentAttribute Annotation { get; set; }
    }
}