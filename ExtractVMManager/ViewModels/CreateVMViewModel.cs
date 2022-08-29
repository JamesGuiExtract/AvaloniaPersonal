using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using ReactiveUI;
using ExtractVMManager.Models;

namespace ExtractVMManager.ViewModels
{
    class CreateVMViewModel : ViewModelBase
    {
        string? name;
        int? templateIndex = null;

        public CreateVMViewModel()
        {
            var okEnabled = this.WhenAnyValue(
                x => x.Name,
                x => x.TemplateIndex,
                (n,t) => !(string.IsNullOrWhiteSpace(n) || t==null));

            Ok = ReactiveCommand.Create(
                () => new VMCreationRequest { Name = Name, TemplateIndex = TemplateIndex },
                okEnabled);
            Cancel = ReactiveCommand.Create(() => { });
        }

        public string? Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }

        public int? TemplateIndex
        {
            get => templateIndex;
            set => this.RaiseAndSetIfChanged(ref templateIndex, value);
        }


        public ReactiveCommand<Unit, VMCreationRequest> Ok { get; }
        public ReactiveCommand<Unit, Unit> Cancel { get; }
    }
}
