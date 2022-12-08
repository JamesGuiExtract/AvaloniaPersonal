
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using IndexConverterV2.Views;
using NUnit.Framework;
using IndexConverterV2.Models.AllDataClasses;
using System.Collections.ObjectModel;
using System.Security.AccessControl;
using System.ComponentModel;
using System.Drawing.Text;
using System;
using System.Reactive;

namespace IndexConverterV2.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ReactiveCommand<Unit, Unit> AddFileCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveFileCommand { get; }
        public ReactiveCommand<Unit, Unit> AddAttributeCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveAttributeCommand { get; }
        public ReactiveCommand<Unit, Unit> MoveAttributeUpCommand { get; }
        public ReactiveCommand<Unit, Unit> MoveAttributeDownCommand { get; }
        public ReactiveCommand<Unit, Unit> ProcessOneCommand { get; }
        public ReactiveCommand<Unit, Unit> ProcessAllCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetProcessCommand { get; }
        public ReactiveCommand<Unit, Unit> SaveConfigCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadConfigCommand { get; }

        public MainWindowViewModel() 
        {
            IObservable<bool> addFileCommandCanExecute = 
                this.WhenAnyValue(
                    x => x.InputFile, 
                    x => x.Delimiter, 
                    x => x.Qualifier, 
                    (file, delimiter, qualifier) => 
                        !string.IsNullOrEmpty(file) 
                        && !string.IsNullOrEmpty(delimiter) 
                        && !string.IsNullOrEmpty(qualifier));
            AddFileCommand = ReactiveCommand.Create(() => { AddFile(); }, addFileCommandCanExecute);

            IObservable<bool> removeFileCommandCanExecute = this.WhenAnyValue(
                x => x.FileListSelectedIndex, 
                (index) => index > -1);
            RemoveFileCommand = ReactiveCommand.Create(() => { RemoveFile(); }, removeFileCommandCanExecute);

            IObservable<bool> addAttributeCommandCanExecute = this.WhenAnyValue(
                x => x.AttributeName, 
                x => x.AttributeValue, 
                x => x.AttributeFile, 
                x => x.WriteIf, 
                x => x.Condition1, 
                x => x.Condition2,
                (name, val, file, writeIf, con1, con2) => 
                    !string.IsNullOrEmpty(name) 
                    && !string.IsNullOrEmpty(val) 
                    && (file > -1) 
                    && (!writeIf || (!string.IsNullOrEmpty(con1) && !string.IsNullOrEmpty(con2))));
            AddAttributeCommand = ReactiveCommand.Create(() => { AddAttribute(); }, addAttributeCommandCanExecute);

            IObservable<bool> removeAttributeCommandCanExecute = this.WhenAnyValue(
                x => x.AttributeListSelectedIndex, 
                (index) => index > -1);
            RemoveAttributeCommand = ReactiveCommand.Create(() => { RemoveAttribute(); }, removeAttributeCommandCanExecute);

            IObservable<bool> moveAttributeUpCommandCanExecute = this.WhenAnyValue(
                x => x.AttributeListSelectedIndex, 
                x => x.Attributes, 
                (index, attributes) => index > 0);
            MoveAttributeUpCommand = ReactiveCommand.Create(() => { MoveAttributeUp(); }, moveAttributeUpCommandCanExecute);

            IObservable<bool> moveAttributeDownCommandCanExecute = this.WhenAnyValue(
                x => x.AttributeListSelectedIndex, 
                x => x.Attributes, 
                (index, attributes) => 
                    index > -1 
                    && index < attributes.Count - 1);
            MoveAttributeDownCommand = ReactiveCommand.Create(() => { MoveAttributeDown(); }, moveAttributeDownCommandCanExecute);

            IObservable<bool> processCommandsCanExecute = this.WhenAnyValue(
                x => x.OutputFile, 
                x => x.Attributes, 
                (output, attributes) => 
                    !string.IsNullOrEmpty(output) 
                    && attributes.Count > 0);
            ProcessOneCommand = ReactiveCommand.Create(() => { ProcessOne(); }, processCommandsCanExecute);
            ProcessAllCommand = ReactiveCommand.Create(() => { ProcessAll(); }, processCommandsCanExecute);
            ResetProcessCommand = ReactiveCommand.Create(() => { ResetProcess(); }, processCommandsCanExecute);

            SaveConfigCommand = ReactiveCommand.Create(() => { SaveConfig(); });
            LoadConfigCommand = ReactiveCommand.Create(() => { LoadConfig(); });
        }
        
        [Reactive]
        public string InputFile { get; set; } = "";
        [Reactive]
        public string Delimiter { get; set; } = "";
        [Reactive]
        public string Qualifier { get; set; } = "";
        
        [Reactive]
        public string AttributeName { get; set; } = "";
        [Reactive]
        public string AttributeValue { get; set; } = "";
        [Reactive]
        public string AttributeType { get; set; } = "";
        [Reactive]
        public int AttributeFile { get; set; } = -1;

        [Reactive]
        public bool WriteIf { get; set; }
        [Reactive]
        public int ConditionTypeIndex { get; set; }
        [Reactive]
        public string Condition1 { get; set; } = "";
        [Reactive]
        public string Condition2 { get; set; } = "";

        [Reactive]
        public ObservableCollection<FileListItem> InputFiles { get; set; } = new ObservableCollection<FileListItem>();
        [Reactive]
        public int FileListSelectedIndex { get; set; } = -1;

        [Reactive]
        public ObservableCollection<AttributeListItem> Attributes { get; set; } = new ObservableCollection<AttributeListItem>();
        [Reactive]
        public int AttributeListSelectedIndex { get; set; } = -1;

        [Reactive]
        public string OutputFile { get; set; } = "";


        /// <summary>
        /// Used to browse for a file, and set the path to InputFile
        /// </summary>
        //TODO Get file browser from Nat's pull request comment
        public void BrowseFileButtonClick() { InputFile = "browse"; }

        /// <summary>
        /// Used to add a file to InputFiles, using the current data of the view
        /// </summary>
        private void AddFile() 
        {
            InputFiles.Add(new FileListItem(InputFile, Delimiter[0], Qualifier[0]));
        }

        /// <summary>
        /// Used to remove the file selected in the view from InputFiles
        /// </summary>
        private void RemoveFile() 
        {
            if (FileListSelectedIndex >= InputFiles.Count || FileListSelectedIndex < 0)
                return;
            InputFiles.RemoveAt(FileListSelectedIndex);

            //Todo: delete attributes from Attributes if they reference a removed file
        }

        /// <summary>
        /// Used to add an attribute to Attributes, using the current data of the view
        /// </summary>
        private void AddAttribute() 
        {
            Attributes.Add(new AttributeListItem(AttributeName, AttributeValue, AttributeType, InputFiles[AttributeFile],
                WriteIf, ConditionTypeIndex == 1, Condition1, Condition2));
        }

        /// <summary>
        /// Used to remove the attribute selected in the view from Attributes
        /// </summary>
        private void RemoveAttribute() 
        {
            if (AttributeListSelectedIndex >= Attributes.Count || AttributeListSelectedIndex < 0)
                return;
            Attributes.RemoveAt(AttributeListSelectedIndex);
        }

        /// <summary>
        /// Used to move the attribute selected in the view up (decreasing index) in the Attributes list
        /// </summary>
        private void MoveAttributeUp() 
        {
            if (AttributeListSelectedIndex <= 0 || AttributeListSelectedIndex >= Attributes.Count)
                return;

            SwapAttributes(AttributeListSelectedIndex - 1, AttributeListSelectedIndex);
        }

        /// <summary>
        /// Used to move the attribute selected in the view down (increasing index) in the Attributes list
        /// </summary>
        private void MoveAttributeDown() 
        {
            if (AttributeListSelectedIndex >= Attributes.Count - 1 || AttributeListSelectedIndex < 0)
                return;

            SwapAttributes(AttributeListSelectedIndex + 1, AttributeListSelectedIndex);
        }

        private void SwapAttributes(int index1, int index2) 
        {
            if (index1 >= Attributes.Count || index1 < 0 || index2 >= Attributes.Count || index2 < 0)
                return;

            AttributeListItem temp = Attributes[index1];
            Attributes[index1] = Attributes[index2];
            Attributes[index2] = temp;
        }

        /// <summary>
        /// Used to process the next EAV
        /// </summary>
        public void ProcessOne() { OutputFile = "proc one"; }

        /// <summary>
        /// Used to process all remaining EAVs
        /// </summary>
        public void ProcessAll() { OutputFile = "proc all"; }

        /// <summary>
        /// Used to reset the process
        /// </summary>
        public void ResetProcess() { OutputFile = "reset proc"; }

        /// <summary>
        /// Used to save the configuration of the program to a text file
        /// </summary>
        //TODO Get file browser from Nat's pull request comment
        public void SaveConfig() { OutputFile = "save"; }

        /// <summary>
        /// Used to load the configuration of the progra from a text file
        /// </summary>
        //TODO Get file browser from Nat's pull request comment
        public void LoadConfig() { OutputFile = "load"; }
    }
}