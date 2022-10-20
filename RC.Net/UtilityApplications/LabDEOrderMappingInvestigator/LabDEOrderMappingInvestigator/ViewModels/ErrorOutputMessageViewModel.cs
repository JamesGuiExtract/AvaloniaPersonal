namespace LabDEOrderMappingInvestigator.ViewModels
{
    public sealed class ErrorOutputMessageViewModel : OutputMessageViewModelBase
    {
        public string ErrorMessage { get; set; }

        public ErrorOutputMessageViewModel(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
