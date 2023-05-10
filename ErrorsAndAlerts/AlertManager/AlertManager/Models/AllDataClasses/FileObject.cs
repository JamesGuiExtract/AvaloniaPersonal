using Extract.ErrorHandling;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UCLID_FILEPROCESSINGLib;

namespace AlertManager.Models.AllDataClasses
{
    public class FileObject
    {
        public FileObject(string fileName, EActionStatus fileStatus, int fileId)
        {
            FileName = fileName;
            FileStatus = fileStatus;
            FileId = fileId;
        }

        public string FileName { get; set; } = "";
        public EActionStatus FileStatus { get; set; } = EActionStatus.kActionUnattempted;
        public int FileId { get; set; }

    }
}