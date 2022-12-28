using ReactiveUI.Fody.Helpers;

namespace ExtractDataExplorer.ViewModels
{
    /// <summary>
    /// An 'attribute' view models that does <c>not</c> include any information about sub-attributes
    /// </summary>
    public class AttributeViewModel : ViewModelBase
    {
        [Reactive] public string? Name { get; set; }
        [Reactive] public string? Value { get; set; }
        [Reactive] public string? Type { get; set; }
        [Reactive] public int Page { get; set; }

        public AttributeViewModel(string? name, string? value, string? type, int page)
        {
            Name = name;
            Value = value;
            Type = type;
            Page = page;
        }

        public override string ToString()
        {
            return $"{Name}|{Value?.Replace("\r", "\\r").Replace("\n", "\\n")}|{Type}|{Page}";
        }
    }
}
