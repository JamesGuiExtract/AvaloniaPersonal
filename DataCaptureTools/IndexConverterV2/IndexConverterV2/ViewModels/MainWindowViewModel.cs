
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
using Avalonia.Controls;
using IndexConverterV2.Views;
using System.Linq;
using System.Threading.Tasks;

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

        //Commands for working with attributes
        public ReactiveCommand<Unit, Unit> AddAttributeCommand { get; }
        public ReactiveCommand<Unit, Unit> RemoveAttributeCommand { get; }
        public ReactiveCommand<Unit, Unit> MoveAttributeUpCommand { get; }
        public ReactiveCommand<Unit, Unit> MoveAttributeDownCommand { get; }

        //Commands for processing EAVs
        public ReactiveCommand<Unit, Unit> ProcessNextLineCommand { get; }
        public ReactiveCommand<Unit, Unit> ProcessNextAttributeCommand { get; }
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
        [Reactive]
        public string Qualifier { get; set; } = "";

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
        public bool WriteIf { get; set; }
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

            //To add a file, InputFileName, Delimiter, and Qualifier must be filled
            IObservable<bool> addFileCommandCanExecute =
                this.WhenAnyValue(
                    x => x.InputFileName,
                    x => x.Delimiter,
                    x => x.Qualifier,
                    (file, delimiter, qualifier) =>
                        !string.IsNullOrEmpty(file)
                        && !string.IsNullOrEmpty(delimiter)
                        && !string.IsNullOrEmpty(qualifier));
            AddFileCommand = ReactiveCommand.Create(() => { AddFile(); }, addFileCommandCanExecute);

            //To remove a file, a file must be selected in the list box
            IObservable<bool> removeFileCommandCanExecute = this.WhenAnyValue(
                x => x.FileListSelectedIndex,
                (index) => index > -1);
            RemoveFileCommand = ReactiveCommand.Create(() => { RemoveFile(); }, removeFileCommandCanExecute);

            //To add an attribute, fields must be filled, and the conditional boxes must be filled if the attribute is marked as conditional
            IObservable<bool> addAttributeCommandCanExecute = this.WhenAnyValue(
                x => x.AttributeName,
                x => x.AttributeValue,
                x => x.AttributeOutputFileName,
                x => x.AttributeFileSelectedIndex,
                x => x.WriteIf,
                x => x.Condition1,
                x => x.Condition2,
                (name, val, outputName, fileIndex, writeIf, con1, con2) =>
                    !string.IsNullOrEmpty(name)
                    && !string.IsNullOrEmpty(val)
                    && !string.IsNullOrEmpty(outputName)
                    && (fileIndex > -1)
                    && (!writeIf || (!string.IsNullOrEmpty(con1) && !string.IsNullOrEmpty(con2))));
            AddAttributeCommand = ReactiveCommand.Create(() => { AddAttribute(); }, addAttributeCommandCanExecute);

            //To remove an attribute, an attribute must be selected in the list box
            IObservable<bool> removeAttributeCommandCanExecute = this.WhenAnyValue(
                x => x.AttributeListSelectedIndex,
                (index) => index > -1);
            RemoveAttributeCommand = ReactiveCommand.Create(() => { RemoveAttribute(); }, removeAttributeCommandCanExecute);

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
            ProcessNextAttributeCommand = ReactiveCommand.Create(() => { ProcessNextAttribute(); }, processCommandsCanExecute);
            ProcessAllCommand = ReactiveCommand.Create(() => { ProcessAll(); }, processCommandsCanExecute);

            //To reset processing, eavwriter must be already processing
            IObservable<bool> resetProcessingCommandCanExecute = this.eavWriter.ObservableProcessing;
            ResetProcessingCommand = ReactiveCommand.Create(() => { ResetProcessing(); }, resetProcessingCommandCanExecute);
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
        /// Used to add a file to InputFiles, using the current data of the view
        /// </summary>
        internal void AddFile() 
        {
            InputFiles.Add(new FileListItem((InputFileName), Delimiter[0], Qualifier[0]));
        }

        /// <summary>
        /// Used to remove the file selected in the view from InputFiles. Any attributes that reference the file will also be removed.
        /// </summary>
        internal void RemoveFile() 
        {
            if (FileListSelectedIndex >= InputFiles.Count || FileListSelectedIndex < 0)
                return;

            string deletedPath = InputFiles[FileListSelectedIndex].Path;
            InputFiles.RemoveAt(FileListSelectedIndex);

            //Removing attributes that reference the removed file
            List<AttributeListItem> attributesToRemove = new();

            foreach (AttributeListItem att in Attributes)
            {
                if (att.File.Path == deletedPath)
                    attributesToRemove.Add(att);
            }

            foreach (AttributeListItem att in attributesToRemove)
            { 
                Attributes.Remove(att);
            }
        }

        /// <summary>
        /// Used to add an attribute to _attributes, using the current data of the view
        /// </summary>
        internal void AddAttribute() 
        {
            bool? condType = WriteIf ? ConditionTypeIndex == 1 : null;
            string? cond1 = WriteIf ? Condition1 : null;
            string? cond2 = WriteIf ? Condition2 : null;
            Attributes.Add(new AttributeListItem(AttributeName, AttributeValue, AttributeType, InputFiles[AttributeFileSelectedIndex], AttributeOutputFileName,
                WriteIf, condType, cond1, cond2));

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
        }

        /// <summary>
        /// Used to move the attribute selected in the view up (decreasing index) in the _attributes list
        /// </summary>
        internal void MoveAttributeUp() 
        {
            if (AttributeListSelectedIndex <= 0 || AttributeListSelectedIndex >= Attributes.Count)
                return;

            SwapAttributes(AttributeListSelectedIndex - 1, AttributeListSelectedIndex);
        }

        /// <summary>
        /// Used to move the attribute selected in the view down (increasing index) in the _attributes list
        /// </summary>
        internal void MoveAttributeDown() 
        {
            if (AttributeListSelectedIndex >= Attributes.Count - 1 || AttributeListSelectedIndex < 0)
                return;

            SwapAttributes(AttributeListSelectedIndex + 1, AttributeListSelectedIndex);
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
        internal void ProcessNextAttribute()
        {
            Process(() => eavWriter.ProcessNextAttribute());
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
            string jsonString = JsonSerializer.Serialize(model);
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
    }
}