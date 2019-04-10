namespace WebAPI.Models
{
    public class ProcessAnnotationParameters
    {
        public string OperationType { get; set; }
        public string Definition { get; set; }
        public DocumentAttribute Annotation { get; set; }
    }
}