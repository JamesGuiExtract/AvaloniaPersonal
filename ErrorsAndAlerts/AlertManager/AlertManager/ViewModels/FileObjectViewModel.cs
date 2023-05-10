using AlertManager.Models.AllDataClasses;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace AlertManager.ViewModels
{
    public class FileObjectViewModel
    {
        [Reactive]
        //this is the shorthand name of the EActionStatus FileStatus, ex. kActionPending = Pending
        public string FileStatusShorthand { get; set; } = "";
        [Reactive]
        public KeyValuePair<EActionStatus, string> SelectedFileStatus { get; set; }
        public IEnumerable<KeyValuePair<EActionStatus, string>> ComboBoxFileStatuses => Constants.ActionStatusToDescription;
        public FileObjectViewModel(FileObject fileObject)
        {
            FileStatusShorthand = ConvertValue(fileObject.FileStatus);
            this.FileObject = fileObject;
        }

        public static string ConvertValue(EActionStatus actionStatus)
        {
            string returnString = "";

            if (!Constants.ActionStatusToDescription.TryGetValue(actionStatus, out var description))
            {
                throw new Exception("Issue with retrieving enum name");
            }

            returnString = description;

            return returnString;
        }


        public FileObject? FileObject { get; set; }
        public ReactiveCommand<FileObject, Unit>? SetIndividualStatuses { get; set; }
    }
}
