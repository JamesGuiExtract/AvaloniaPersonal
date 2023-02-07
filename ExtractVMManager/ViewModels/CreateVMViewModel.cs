using System.Reactive;
using ReactiveUI;
using ExtractVMManager.Models;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Generic;

namespace ExtractVMManager.ViewModels
{
    class CreateVMViewModel : ViewModelBase
    {
        string? name = string.Empty;
        string? templateName = string.Empty;
        string? purpose = string.Empty;

        ObservableCollection<string>? templates;
        public ObservableCollection<string>? Templates
        {
            get => templates;
            set => this.RaiseAndSetIfChanged(ref templates, value);
        }

        public CreateVMViewModel(IEnumerable<string> templateList)
        {
            var okEnabled = this.WhenAnyValue(
                model => model.Name,
                model => model.templateName,
                (name,template) => !(string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(template)));

            templates = new ObservableCollection<string>(templateList);

            Ok = ReactiveCommand.Create(
                () => new VMCreationRequest { Name = Name, TemplateName = templateName, Purpose = purpose },
                okEnabled);
            Cancel = ReactiveCommand.Create(() => { });
        }

        public string? Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        public string? TemplateName
        {
            get => templateName;
            set => this.RaiseAndSetIfChanged(ref templateName, value);
        }

        public string? Purpose
        {
            get => purpose;
            set => this.RaiseAndSetIfChanged(ref purpose, value);
        }

        public ReactiveCommand<Unit, VMCreationRequest> Ok { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }
    }
}
