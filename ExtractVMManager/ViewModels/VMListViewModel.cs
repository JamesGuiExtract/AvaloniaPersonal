using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ExtractVMManager.Services;
using VMService;

namespace ExtractVMManager.ViewModels
{
    public class VMListViewModel : ViewModelBase
    {
        VirtualMachineModel? selectedItem;
        Database localDb;
        ObservableCollection<VirtualMachineModel>? items;
        private bool canStart = false;
        private bool canStopAndReset = false;


        public VMListViewModel(IEnumerable<VirtualMachineModel> items, Database db)
        {
            Items = new ObservableCollection<VirtualMachineModel>(items);
            localDb = db;
        }


        public ObservableCollection<VirtualMachineModel>? Items 
        {
            get => items;
            set => this.RaiseAndSetIfChanged(ref items, value);
        }

        public VirtualMachineModel? SelectedItem
        {
            get => selectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedItem, value);
                if(selectedItem != null && selectedItem.Status == VirtualMachineStatus.Off)
                {
                    CanStart = true;
                    CanStopAndReset = false;
                }
                else if (selectedItem != null && selectedItem.Status == VirtualMachineStatus.Running)
                {
                    CanStart = false;
                    CanStopAndReset = true;
                }
                else
                {
                    CanStart = false;
                    CanStopAndReset = false;
                }
            }
        }

        public bool CanStart
        {
            get => canStart;
            set => this.RaiseAndSetIfChanged(ref canStart, value);
        }

        public bool CanStopAndReset
        {
            get => canStopAndReset;
            set => this.RaiseAndSetIfChanged(ref canStopAndReset, value);
        }

        public void StartVM()
        {
            if (SelectedItem != null)
            {
                localDb.StartVM(SelectedItem.Name);
                Items = new ObservableCollection<VirtualMachineModel>(localDb.GetVirtualMachineModels());
            }
        }

        public void StopVM()
        {
            if (SelectedItem != null)
            {
                localDb.StopVM(SelectedItem.Name);
                Items = new ObservableCollection<VirtualMachineModel>(localDb.GetVirtualMachineModels());
            }
        }

        public void RestartVM()
        {
            if (SelectedItem != null)
            {
                localDb.RestartVM(SelectedItem.Name);
                Items = new ObservableCollection<VirtualMachineModel>(localDb.GetVirtualMachineModels());
            }
        }
    }
}
