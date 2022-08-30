using System;
using System.Reactive.Linq;
using ReactiveUI;
using ExtractVMManager.Models;
using ExtractVMManager.Services;


namespace ExtractVMManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        ViewModelBase? content;
        Database localDb;
        public MainWindowViewModel(Database db)
        {
            Content = List = new VMListViewModel(db.GetItems(), db);
            localDb = db;
        }

        public ViewModelBase? Content
        {
            get => content;
            private set => this.RaiseAndSetIfChanged(ref content, value);
        }

        public VMListViewModel List { get; set; }

        public void AddItem()
        {
            var vm = new CreateVMViewModel();

            Observable.Merge(
                vm.Ok,
                vm.Cancel.Select(_ => (VMCreationRequest?)null))
                .Take(1)
                .Subscribe(model =>
                {
                    if (model != null)
                    {
                        localDb.CreateNewVM(model.Name, model.TemplateIndex);
                        Content = List = new VMListViewModel(localDb.GetItems(), localDb);
                    }
                    else
                    {
                        Content = List;
                    }
                });

            Content = vm;
        }
    }
}
