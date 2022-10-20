namespace LabDEOrderMappingInvestigator.ViewModels
{
    public sealed class TextOutputMessageViewModel : OutputMessageViewModelBase
    {
        public string TextMessage { get; set; }

        public TextOutputMessageViewModel(string textMessage)
        {
            TextMessage = textMessage;
        }
    }
}
