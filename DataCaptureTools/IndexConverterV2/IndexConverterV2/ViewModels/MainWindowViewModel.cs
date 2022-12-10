
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using IndexConverterV2.Views;
using NUnit.Framework;
using IndexConverterV2.Models.AllDataClasses;

namespace IndexConverterV2.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        [Reactive]
        public string? InputFile { get; set; }
        [Reactive]
        public char? Delimiter { get; set; }
        [Reactive]
        public char? Qualifier { get; set; }
        [Reactive]
        public string? OutputFile { get; set; }
        [Reactive]
        public string? AttributeName { get; set; }
        [Reactive]
        public string? AttributeValue { get; set; }
        [Reactive]
        public string? AttributeType { get; set; }
        [Reactive]
        public string? Condition1 { get; set; }
        [Reactive]
        public string? Condition2 { get; set; }
        [Reactive]
        public int FileListIndex { get; set; }
        [Reactive]
        public int AttributeListIndex { get; set; }
        [Reactive]
        public int AttributeFile { get; set; }

        public List<FileListItem> InputFiles = new List<FileListItem>();
        public List<AttributeListItem> Attributes = new List<AttributeListItem>();

        /// <summary>
        /// Used to browse for a file, and set the path to InputFile
        /// </summary>
        public void BrowseFileButtonClick() { InputFile = "browse"; }

        /// <summary>
        /// Used to add a file to InputFiles, using the current data of the view
        /// </summary>
        public void AddFileButtonClick() 
        {
            if (InputFile == null || Delimiter == null || Qualifier == null)
                return;
            InputFiles.Add(new FileListItem(InputFile, Delimiter.GetValueOrDefault(), Qualifier.GetValueOrDefault()));
        }

        /// <summary>
        /// Used to remove the file selected in the view from InputFiles
        /// </summary>
        public void RemoveFileButtonClick() 
        { 
            InputFile = FileListIndex.ToString(); 
        }

        /// <summary>
        /// Used to add an attribute to Attributes, using the current data of the view
        /// </summary>
        public void AddAttributeButtonClick() 
        {
            if (AttributeName == null || AttributeValue == null || AttributeType == null)
                return;
            Attributes.Add(new AttributeListItem(AttributeName, AttributeValue, AttributeType, AttributeFile));
        }

        /// <summary>
        /// Used to remove the attribute selected in the view from Attributes
        /// </summary>
        public void RemoveAttributeButtonClick() 
        {
            Attributes.RemoveAt(AttributeListIndex);
        }

        /// <summary>
        /// Used to move the attribute selected in the view up in the list
        /// </summary>
        public void MoveAttributeUpButtonClick() { AttributeName = AttributeListIndex.ToString(); }

        /// <summary>
        /// Used to move the attribute selected in the view down in the list
        /// </summary>
        public void MoveAttributeDownButtonClick() { AttributeName = AttributeListIndex.ToString(); }

        /// <summary>
        /// Used to save the configuration of the program to a text file
        /// </summary>
        public void SaveConfigButtonClick() { OutputFile = "save"; }

        /// <summary>
        /// Used to load the configuration of the progra from a text file
        /// </summary>
        public void LoadConfigButtonClick() { OutputFile = "load"; }

        /// <summary>
        /// Used to process the next EAV
        /// </summary>
        public void ProcessOneButtonClick() { OutputFile = "proc one"; }

        /// <summary>
        /// Used to process all remaining EAVs
        /// </summary>
        public void ProcessAllButtonClick() { OutputFile = "proc all"; }

        /// <summary>
        /// Used to reset the process
        /// </summary>
        public void ResetProcessButtonClick() { OutputFile = "reset proc"; }
    }
}