
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System;
using System.Reactive;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using DynamicData;
using System.Reactive.Linq;
using IndexConverterV2.Models;

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

            IObservable<bool> processCommandsCanExecute =
                Observable.CombineLatest(
                    this.WhenAnyValue(x => x.OutputFile),
                    Observable.FromEventPattern(Attributes, "CollectionChanged"),
                    (output, collectionChanged) =>
                        !string.IsNullOrEmpty(output)
                        && (collectionChanged.Sender is IList<AttributeListItem> attributes)
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
        public ObservableCollection<FileListItem> InputFiles { get; set; } = new();
        [Reactive]
        public ObservableCollection<AttributeListItem> Attributes { get; set; } = new();

        [Reactive]
        public int FileListSelectedIndex { get; set; } = -1;

        [Reactive]
        public int AttributeListSelectedIndex { get; set; } = -1;

        [Reactive]
        public string OutputFile { get; set; } = "";


        /// <summary>
        /// Used to browse for a file, and set the path to InputFile
        /// </summary>
        public void BrowseFileButtonClick() { InputFile = "browse"; }

        /// <summary>
        /// Used to add a file to InputFiles, using the current data of the view
        /// </summary>
        internal void AddFile() 
        {
            InputFiles.Add(new FileListItem(StripQuotes(InputFile), Delimiter[0], Qualifier[0]));
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
        /// Used to add an attribute to Attributes, using the current data of the view
        /// </summary>
        internal void AddAttribute() 
        {
            bool? condType = WriteIf ? ConditionTypeIndex == 1 : null;
            string? cond1 = WriteIf ? Condition1 : null;
            string? cond2 = WriteIf ? Condition2 : null;
            Attributes.Add(new AttributeListItem(AttributeName, AttributeValue, AttributeType, InputFiles[AttributeFile],
                WriteIf, condType, cond1, cond2));
        }

        /// <summary>
        /// Used to remove the attribute selected in the view from Attributes
        /// </summary>
        internal void RemoveAttribute() 
        {
            if (AttributeListSelectedIndex >= Attributes.Count || AttributeListSelectedIndex < 0)
                return;
            Attributes.RemoveAt(AttributeListSelectedIndex);
        }

        /// <summary>
        /// Used to move the attribute selected in the view up (decreasing index) in the Attributes list
        /// </summary>
        internal void MoveAttributeUp() 
        {
            if (AttributeListSelectedIndex <= 0 || AttributeListSelectedIndex >= Attributes.Count)
                return;

            SwapAttributes(AttributeListSelectedIndex - 1, AttributeListSelectedIndex);
        }

        /// <summary>
        /// Used to move the attribute selected in the view down (increasing index) in the Attributes list
        /// </summary>
        internal void MoveAttributeDown() 
        {
            if (AttributeListSelectedIndex >= Attributes.Count - 1 || AttributeListSelectedIndex < 0)
                return;

            SwapAttributes(AttributeListSelectedIndex + 1, AttributeListSelectedIndex);
        }

        internal void SwapAttributes(int index1, int index2) 
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
        internal void ProcessOne() { OutputFile = "proc one"; }

        /// <summary>
        /// Used to process all remaining EAVs
        /// </summary>
        internal void ProcessAll() { OutputFile = "proc all"; }

        /// <summary>
        /// Used to reset the process
        /// </summary>
        internal void ResetProcess() { OutputFile = "reset proc"; }

        /// <summary>
        /// Used to save the configuration of the program to a text file
        /// </summary>
        internal void SaveConfig()
        {
            string path = StripQuotes(OutputFile);
            FileStream stream = new(path, FileMode.Create);
            MainWindowModel model = new(InputFiles, Attributes);


            using (StreamWriter writer = new(stream))
            {
                string jsonString = JsonSerializer.Serialize(model);
                writer.Write(jsonString);
            }
        }

        /// <summary>
        /// Used to load the configuration of the program from a text file. Clears the current configuration before loading.
        /// </summary>
        internal void LoadConfig() 
        {
            string path = StripQuotes(OutputFile);
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
                }
            } 
            catch (Exception e)
            { Console.WriteLine(e.Message); }
            
        }

        /// <summary>
        /// Replaces any occurrences of a % followed by a number with the relevant index in an array of strings. Not 0 based.
        /// </summary>
        /// <param name="replaceIn">String to search and replace %s in</param>
        /// <param name="values">Array of strings to pull values from when a % that matches its index is found. %3 would match values[2]</param>
        /// <returns>The string replaceIn but with all %s replaced with relevant values.</returns>
        internal string ReplacePercents(string replaceIn, string[] values)
        {
            Regex rgx = new(@"%(?'num'\d+)");
            return rgx.Replace(replaceIn, new MatchEvaluator(GetPercentIndexValue));

            string GetPercentIndexValue(Match m)
            {
                string matchText = m.Groups["num"].Value;
                int index = int.Parse(matchText);
                return values[index - 1];
            }
        }

        /// <summary>
        /// Removes the quotation marks wrapping a string.
        /// </summary>
        /// <param name="stripFrom">The string to be stripped of quotes.</param>
        /// <returns>stripFrom without wrapping quotes. If stripFrom was not wrapped in quotes, then it is returned unmodified.</returns>
        internal string StripQuotes(string stripFrom) 
        {
            if (stripFrom[0] == '"' && stripFrom[stripFrom.Length - 1] == '"')
                return stripFrom.Substring(1, stripFrom.Length - 2);
            else
                return stripFrom;
        }
    }
}