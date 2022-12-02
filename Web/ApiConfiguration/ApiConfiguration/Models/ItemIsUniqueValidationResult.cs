namespace Extract.Web.ApiConfiguration.Models
{
    public class ItemIsUniqueValidationResult
    {
        public bool ItemIsEmpty { get; set; }
        public bool ItemIsUnique { get; set; }
        public bool IsValid => !ItemIsEmpty && ItemIsUnique;
    }
}