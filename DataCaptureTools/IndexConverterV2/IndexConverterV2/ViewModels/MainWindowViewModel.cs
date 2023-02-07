
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Reactive;
using System.IO;
using System.Text.Json;
using DynamicData;
using System.Reactive.Linq;
using IndexConverterV2.Models;
using IndexConverterV2.Views;
using System.Linq;
using IndexConverterV2.Services;

namespace IndexConverterV2.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        //The view that this view model is using
        private readonly IView? _view;
        private readonly IDialogService? _dialog;

        //Commands for working with input files
        public ReactiveCommand<Unit, Unit> AddFileCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveFileCommand { get; }
        public ReactiveCommand<Unit, Unit> EditFileCommand { get; }

        //Commands for working with attributes
        public ReactiveCommand<Unit, Unit> AddAttributeCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveAttributeCommand { get; }
        public ReactiveCommand<Unit, Unit> EditAttributeCommand { get; }
        public ReactiveCommand<Unit, Unit> MoveAttributeUpCommand { get; }
        public ReactiveCommand<Unit, Unit> MoveAttributeDownCommand { get; }

        //Commands for processing EAVs
        public ReactiveCommand<Unit, Unit> ProcessNextLineCommand { get; }
        public ReactiveCommand<Unit, Unit> ProcessNextFileCommand { get; }
        public ReactiveCommand<Unit, Unit> ProcessAllCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetProcessingCommand { get; }

        //Commands for serializing program configuration
        public ReactiveCommand<Unit, Unit>? SaveConfigCommand { get; }
        public ReactiveCommand<Unit, Unit>? LoadConfigCommand { get; }

        public ReactiveCommand<Unit, Unit>? BrowseFileCommand { get; }
        public ReactiveCommand<Unit, Unit>? BrowseOutputCommand { get; }

        //Properties for input files
        [Reactive]
        public string InputFileName { get; set; } = "";
        [Reactive]
        public string Delimiter { get; set; } = "";

        //Properties for attributes
        [Reactive]
        public string AttributeName { get; set; } = "";
        [Reactive]
        public string AttributeValue { get; set; } = "";
        [Reactive]
        public string AttributeType { get; set; } = "";
        [Reactive]
        public string AttributeOutputFileName { get; set; } = "";
        [Reactive]
        public int AttributeFileSelectedIndex { get; set; } = -1;

        //Properties for attribute conditionality
        [Reactive]
        public bool AttributeIsConditional { get; set; }
        [Reactive]
        public int ConditionTypeIndex { get; set; }
        [Reactive]
        public string Condition1 { get; set; } = "";
        [Reactive]
        public string Condition2 { get; set; } = "";

        //List of input files used by attributes
        [Reactive]
        public ObservableCollection<FileListItem> InputFiles { get; set; } = new();
        //List of attributes to generate from input files
        [Reactive]
        public ObservableCollection<AttributeListItem> Attributes { get; set; } = new();

        //Indexes of list boxes in view
        [Reactive]
        public int FileListSelectedIndex { get; set; } = -1;
        [Reactive]
        public int AttributeListSelectedIndex { get; set; } = -1;

        //Name of the folder where output files will go
        [Reactive]
        public string OutputFolder { get; set; } = "";
        [Reactive]
        public string ProcessMessageText { get; set; } = "";

        //Object used for creating EAV files from attributes
        private EAVWriter eavWriter;



        public MainWindowViewModel() 
        {
            eavWriter = new();

            //To add a file, InputFileName and Delimiter must be filled
            IObservable<bool> addFileCommandCanExecute =
                this.WhenAnyValue(
                    x => x.InputFileName,
                    x => x.Delimiter,
                    (file, delimiter) =>
                        !string.IsNullOrEmpty(file)
                        && !string.IsNullOrEmpty(delimiter));
            AddFileCommand = ReactiveCommand.Create(() => { AddFile(); }, addFileCommandCanExecute);

            //To remove a file, a file must be selected in the list box
            IObservable<bool> editFileCommandCanExecute = this.WhenAnyValue(
                x => x.FileListSelectedIndex,
                (index) => index > -1);
            RemoveFileCommand = ReactiveCommand.Create(() => { RemoveFile(); }, editFileCommandCanExecute);
            EditFileCommand = ReactiveCommand.Create(() => { EditFile(); }, editFileCommandCanExecute);

            //To add an attribute, fields must be filled, and the conditional boxes must be filled if the attribute is marked as conditional
            IObservable<bool> addAttributeCommandCanExecute = this.WhenAnyValue(
                x => x.AttributeName,
                x => x.AttributeOutputFileName,
                x => x.AttributeFileSelectedIndex,
                x => x.AttributeIsConditional,
                x => x.Condition1,
                x => x.Condition2,
                (name, outputName, fileIndex, writeIf, con1, con2) =>
                    !string.IsNullOrEmpty(name)
                    && !string.IsNullOrEmpty(outputName)
                    && (fileIndex > -1));
            AddAttributeCommand = ReactiveCommand.Create(() => { AddAttribute(); }, addAttributeCommandCanExecute);

            //To remove an attribute, an attribute must be selected in the list box
            IObservable<bool> editAttributeCommandCanExecute = this.WhenAnyValue(
                x => x.AttributeListSelectedIndex,
                (index) => index > -1);
            RemoveAttributeCommand = ReactiveCommand.Create(() => { RemoveAttribute(); }, editAttributeCommandCanExecute);
            EditAttributeCommand = ReactiveCommand.Create(() => { EditAttribute(); }, editAttributeCommandCanExecute);

            //To move an attribute up, an attribute must be selected in the list box and must not already be at the top
            IObservable<bool> moveAttributeUpCommandCanExecute = this.WhenAnyValue(
                x => x.AttributeListSelectedIndex,
                x => x.Attributes,
                (index, attributes) => index > 0);
            MoveAttributeUpCommand = ReactiveCommand.Create(() => { MoveAttributeUp(); }, moveAttributeUpCommandCanExecute);

            //To move an attribute down, an attribute must be selected in the list box and must not already be at the bottom
            IObservable<bool> moveAttributeDownCommandCanExecute = this.WhenAnyValue(
                x => x.AttributeListSelectedIndex,
                x => x.Attributes,
                (index, attributes) =>
                    index > -1
                    && index < attributes.Count - 1);
            MoveAttributeDownCommand = ReactiveCommand.Create(() => { MoveAttributeDown(); }, moveAttributeDownCommandCanExecute);

            //To process attributes, the attributes list can't be empty, and the output folder must be filled
            IObservable<bool> processCommandsCanExecute =
                Observable.CombineLatest(
                    this.WhenAnyValue(x => x.OutputFolder),
                    Observable.FromEventPattern(Attributes, "CollectionChanged"),
                    (output, collectionChanged) =>
                        !string.IsNullOrEmpty(output)
                        && (collectionChanged.Sender is IList<AttributeListItem> attributes)
                        && attributes.Count > 0);
            ProcessNextLineCommand = ReactiveCommand.Create(() => { ProcessNextLine(); }, processCommandsCanExecute);
            ProcessNextFileCommand = ReactiveCommand.Create(() => { ProcessNextFile(); }, processCommandsCanExecute);
            ProcessAllCommand = ReactiveCommand.Create(() => { ProcessAll(); }, processCommandsCanExecute);

            //To reset processing, eavwriter must be already processing
            IObservable<bool> resetProcessingCommandCanExecute = this.eavWriter.ObservableProcessing;
            ResetProcessingCommand = ReactiveCommand.Create(() => { ResetProcessing(); }, resetProcessingCommandCanExecute);

            IObservable<int> selectedFileIndex = this.WhenAnyValue(x => x.FileListSelectedIndex);
            selectedFileIndex.Subscribe(OnFileSelectionChange);

            IObservable<int> selectedAttributeIndex = this.WhenAnyValue(x => x.AttributeListSelectedIndex);
            selectedAttributeIndex.Subscribe(OnAttributeSelectionChange);
        }

        public MainWindowViewModel(IView view, IDialogService dialog) : this()
        {
            _view = view;
            _dialog = dialog;
            SaveConfigCommand = ReactiveCommand.Create(() => { SaveConfigThroughDialog(); });
            LoadConfigCommand = ReactiveCommand.Create(() => { LoadConfigThroughDialog(); });
            BrowseFileCommand = ReactiveCommand.Create(() => { BrowseFileButtonClick(); });
            BrowseOutputCommand = ReactiveCommand.Create(() => { BrowseOutputButtonClick(); });
        }



        /// <summary>
        /// Used to browse for a file, and set the path to InputFileName
        /// </summary>
        internal async void BrowseFileButtonClick() 
        {
            _ = _view ?? throw new NullReferenceException("_view is null!");
            _ = _dialog ?? throw new NullReferenceException("_dialog is null!");
            if (await _dialog.OpenCSVDialog(_view) is string fileName)
            {
                InputFileName = fileName;
            }
        }

        /// <summary>
        /// Opens a folder browser and sets the selected folder to OutputFolder
        /// </summary>
        internal async void BrowseOutputButtonClick() 
        {
            _ = _view ?? throw new NullReferenceException("_view is null!");
            _ = _dialog ?? throw new NullReferenceException("_dialog is null!");
            if (await _dialog.OpenFolderDialog(_view) is string folderName)
            {
                OutputFolder = folderName;
            }
        }

        /// <summary>
        /// Used to add a file to InputFiles, using the current data of the view.
        /// Does not add a file to the list if an equal file is already present.
        /// </summary>
        internal void AddFile() 
        {
            InputFiles.Add(MakeFile());
        }

        /// <summary>
        /// Used to remove the file selected in the view from InputFiles. Any attributes that reference the file will also be removed.
        /// </summary>
        internal void RemoveFile() 
        {
            if (FileListSelectedIndex >= InputFiles.Count || FileListSelectedIndex < 0)
                return;

            Guid deletedID = InputFiles[FileListSelectedIndex].ID;
            InputFiles.RemoveAt(FileListSelectedIndex);

            //Removing attributes that reference the removed file
            List<AttributeListItem> attributesToRemove = new();

            foreach (AttributeListItem att in Attributes)
            {
                if (att.File.ID == deletedID)
                    attributesToRemove.Add(att);
            }

            foreach (AttributeListItem att in attributesToRemove)
            { 
                Attributes.Remove(att);
            }

            if (attributesToRemove.Count > 0)
                eavWriter.StopProcessing();

            ClearFileFields();
            ClearAttributeFields();
        }

        /// <summary>
        /// Saves the current file settings to the currently selected input file.
        /// Also updates attributes that relied on that file.
        /// </summary>
        internal void EditFile()
        {
            if (FileListSelectedIndex < 0)
                return;

            Guid id = InputFiles[FileListSelectedIndex].ID;

            List<int> attributesToEdit = new();

            for (int attIndex = 0; attIndex < Attributes.Count; attIndex++)
            {
                if (Attributes[attIndex].File.ID == id)
                    attributesToEdit.Add(attIndex);
            }

            //Steps done in this order to prevent file list box from "remembering" selections
            int oldFileListSelectedIndex = this.FileListSelectedIndex;
            Guid oldGuid = InputFiles[oldFileListSelectedIndex].ID;
            FileListItem updatedFile = MakeFile();
            ClearFileFields();
            ClearAttributeFields();
            InputFiles[oldFileListSelectedIndex] = updatedFile with { ID = oldGuid };

            foreach (int attIndex in attributesToEdit)
            {
                AttributeListItem attToEdit = Attributes[attIndex];
                Attributes[attIndex] = attToEdit with { File = InputFiles[oldFileListSelectedIndex] };
            }
        }

        /// <summary>
        /// Used to add an attribute to _attributes, using the current data of the view
        /// </summary>
        internal void AddAttribute() 
        {
            bool? condType = AttributeIsConditional ? ConditionTypeIndex == 1 : null;
            string? cond1 = AttributeIsConditional ? Condition1 : null;
            string? cond2 = AttributeIsConditional ? Condition2 : null;
            Attributes.Add(MakeAttribute());

            eavWriter.StopProcessing();
        }

        /// <summary>
        /// Used to remove the attribute selected in the view from _attributes
        /// </summary>
        internal void RemoveAttribute() 
        {
            if (AttributeListSelectedIndex >= Attributes.Count || AttributeListSelectedIndex < 0)
                return;
            Attributes.RemoveAt(AttributeListSelectedIndex);
            eavWriter.StopProcessing();
            ClearAttributeFields();
        }

        /// <summary>
        /// Saves the current attribute settings to the currently selected attribute.
        /// </summary>
        internal void EditAttribute()
        {
            if (AttributeListSelectedIndex < 0)
                return;
            //Steps done in this order to prevent attribute list box from "remembering" selections
            AttributeListItem updatedAttribute = MakeAttribute();
            int index = AttributeListSelectedIndex;
            ClearAttributeFields();
            Attributes[index] = updatedAttribute;

        }

        /// <summary>
        /// Used to move the attribute selected in the view up (decreasing index) in the _attributes list
        /// </summary>
        internal void MoveAttributeUp() 
        {
            int curIndex = AttributeListSelectedIndex;
            if (curIndex <= 0 || curIndex >= Attributes.Count)
                return;

            SwapAttributes(curIndex - 1, curIndex);
            AttributeListSelectedIndex = curIndex - 1;
        }

        /// <summary>
        /// Used to move the attribute selected in the view down (increasing index) in the _attributes list
        /// </summary>
        internal void MoveAttributeDown() 
        {
            int curIndex = AttributeListSelectedIndex;
            if (curIndex >= Attributes.Count - 1 || curIndex < 0)
                return;

            SwapAttributes(curIndex + 1, curIndex);
            AttributeListSelectedIndex = curIndex + 1; 
        }

        /// <summary>
        /// Used to swap index locations of attributes
        /// </summary>
        /// <param name="index1">Index of first attribute to swap</param>
        /// <param name="index2">Index of second attribute to swap</param>
        internal void SwapAttributes(int index1, int index2) 
        {
            if (index1 >= Attributes.Count || index1 < 0 || index2 >= Attributes.Count || index2 < 0)
                return;

            AttributeListItem temp = Attributes[index1];
            Attributes[index1] = Attributes[index2];
            Attributes[index2] = temp;
            eavWriter.StopProcessing();
        }

        internal void Process(Func<string> processFunction)
        {
            try
            {
                if (eavWriter.Processing)
                    ProcessMessageText = processFunction();
                else
                {
                    eavWriter.StartProcessing(Attributes.ToList(), OutputFolder);
                    ProcessMessageText = processFunction();
                }
            }
            catch (Exception e)
            {
                ProcessMessageText = e.Message;
            }
        }

        /// <summary>
        /// Used to process the next line of input, creating an EAV
        /// </summary>
        internal void ProcessNextLine()
        {
            Process(() => eavWriter.ProcessNextLine());
        }

        /// <summary>
        /// Used to process the next attribute, creating all EAVs for that attribute
        /// </summary>
        internal void ProcessNextFile()
        {
            Process(() => eavWriter.ProcessNextFile());
        }

        /// <summary>
        /// Used to process all remaining EAVs
        /// </summary>
        internal void ProcessAll()
        {
            Process(() => eavWriter.ProcessAll().ToString());
        }

        /// <summary>
        /// Stops and starts eavWriter processing.
        /// </summary>
        internal void ResetProcessing() 
        {
            eavWriter.StopProcessing();
            ProcessMessageText = "reset";
        }

        /// <summary>
        /// Used to save the configuration of the program to a text file.
        /// </summary>
        /// <param name="path">The path to write the text file to.</param>
        internal void SaveConfig(string path)
        {
            FileStream stream = new(path, FileMode.Create);
            MainWindowModel model = new(InputFiles, Attributes, OutputFolder);

            using StreamWriter writer = new(stream);
            JsonSerializerOptions opts = new() { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(model, opts);
            writer.Write(jsonString);
        }

        /// <summary>
        /// Used to save the configuration of the program to a text file specified through a dialog.
        /// </summary>
        internal async void SaveConfigThroughDialog() 
        {
            _ = _view ?? throw new NullReferenceException("_view is null!");
            _ = _dialog ?? throw new NullReferenceException("_dialog is null!");
            if (await _dialog.SaveTxtDialog(_view) is string fileName)
            {
                SaveConfig(fileName);
            }
        }

        /// <summary>
        /// Used to load the configuration of the program from a text file. Clears the current configuration before loading.
        /// </summary>
        /// <param name="path">The path to read the text file from.</param>
        internal void LoadConfig(string path) 
        {
            eavWriter.StopProcessing();

            try 
            {
                string jsonString = File.ReadAllText(path);
                MainWindowModel model = JsonSerializer.Deserialize<MainWindowModel>(jsonString) ?? throw new Exception("Model could not be deserialized.");

                if (model != null)
                {
                    InputFiles.Clear();
                    Attributes.Clear();

                    InputFiles.Add(model.InputFiles);
                    Attributes.Add(model.Attributes);
                    OutputFolder = model.OutputFolder;

                    ClearFileFields();
                    ClearAttributeFields();
                }
            } 
            catch (Exception e)
            { Console.WriteLine(e.Message); }
        }

        /// <summary>
        /// Used to load the configuration of the program from a text file through a dialog.
        /// </summary>
        internal async void LoadConfigThroughDialog() 
        {
            _ = _view ?? throw new NullReferenceException("_view is null!");
            _ = _dialog ?? throw new NullReferenceException("_dialog is null!");
            if (await _dialog.OpenTxtDialog(_view) is string fileName) 
            {
                LoadConfig(fileName);
            }
        }

        /// <summary>
        /// Populates file related fields in the view.
        /// </summary>
        /// <param name="newIndex">The index of the selected file to be populated from.</param>
        protected void OnFileSelectionChange(int newIndex)
        {
            if (newIndex < 0)
                return;

            FileListItem file = InputFiles[newIndex];
            this.InputFileName = file.Path;
            this.Delimiter = file.Delimiter.ToString();
        }

        /// <summary>
        /// Populates attribute related fields in the view.
        /// </summary>
        /// <param name="newIndex">The index of the selected attribute to be populated from.</param>
        protected void OnAttributeSelectionChange(int newIndex)
        {
            if (newIndex < 0)
                return;

            AttributeListItem attribute = Attributes[newIndex];
            this.AttributeName = attribute.Name;
            this.AttributeValue = attribute.Value;
            this.AttributeType = attribute.Type;
            this.AttributeOutputFileName = attribute.OutputFileName;
            this.AttributeFileSelectedIndex = InputFiles.IndexOf(attribute.File);
            this.AttributeIsConditional = attribute.IsConditional;
            if (this.AttributeIsConditional)
            {
                this.Condition1 = attribute.LeftCondition ?? "";
                this.Condition2 = attribute.RightCondition ?? "";
                this.ConditionTypeIndex = Convert.ToInt32(attribute.ConditionType);
            }
        }

        /// <summary>
        /// Makes a file from the view.
        /// </summary>
        /// <returns>File made from current view fields.</returns>
        internal FileListItem MakeFile() 
        {
            return new FileListItem(
                Path: this.InputFileName,
                Delimiter: this.Delimiter[0],
                ID: Guid.NewGuid());
        }

        /// <summary>
        /// Makes an attribute from the view.
        /// </summary>
        /// <returns>Attribute made from current view fields.</returns>
        internal AttributeListItem MakeAttribute() 
        {
            return new AttributeListItem(
                Name: this.AttributeName,
                Value: this.AttributeValue,
                Type: this.AttributeType,
                File: this.InputFiles.ElementAt(this.AttributeFileSelectedIndex),
                OutputFileName: this.AttributeOutputFileName,
                IsConditional: this.AttributeIsConditional,
                ConditionType: this.AttributeIsConditional ? this.ConditionTypeIndex == 1 : null,
                LeftCondition: this.AttributeIsConditional ? this.Condition1 : null,
                RightCondition: this.AttributeIsConditional ? this.Condition2 : null);
        }

        internal void ClearFileFields() 
        {
            this.InputFileName = "";
            this.Delimiter = "";
            this.FileListSelectedIndex = -1;
        }
        internal void ClearAttributeFields() 
        {
            this.AttributeName = "";
            this.AttributeValue = "";
            this.AttributeType = "";
            this.AttributeOutputFileName = "";
            this.AttributeFileSelectedIndex = -1;
            this.AttributeIsConditional = false;
            this.Condition1 = "";
            this.Condition2 = "";
            this.ConditionTypeIndex = 0;
            this.AttributeListSelectedIndex = -1;
        }
    }
}