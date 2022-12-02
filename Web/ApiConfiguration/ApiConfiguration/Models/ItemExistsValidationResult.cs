namespace Extract.Web.ApiConfiguration.Models
{
    public class ItemExistsValidationResult
    {
        public bool ItemIsEmpty { get; set; }
        public bool ItemExists { get; set; }
        public bool IsValid => !ItemIsEmpty && ItemExists;
    }
}