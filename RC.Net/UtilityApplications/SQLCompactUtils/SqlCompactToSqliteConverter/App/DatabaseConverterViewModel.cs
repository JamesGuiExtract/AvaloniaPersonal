using Extract.Utilities.WPF;
using MvvmGen;
using MvvmGen.Events;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Extract.Utilities.SqlCompactToSqliteConverter
{
    [Inject(typeof(IDatabaseConverter))]
    [Inject(typeof(IFileBrowserDialogService))]
    [Inject(typeof(IMessageDialogService))]
    [ViewModel]
    public partial class DatabaseConverterViewModel : IEventSubscriber<DatabaseInputOutputEvent>
    {
        [Property] private string _inputDatabasePath;
        [Property] private string _outputDatabasePath;
        [Property] private bool _isExecuting;
        [Property] private string _statusMessage;

        [Command(CanExecuteMethod = nameof(CanConvert))]
        private async Task Convert()
        {
            IsExecuting = true;
            try
            {
                // Trim the paths to remove quotes, e.g.
                // Update via the view model properties so that the UI shows the trimmed paths
                InputDatabasePath = InputDatabasePath.TrimPath();
                OutputDatabasePath = OutputDatabasePath.TrimPath();

                StatusMessage = "";

                if (File.Exists(OutputDatabasePath))
                {
                    if (MessageDialogService.ShowYesNoDialog(
                        title: "Confirm file overwrite",
                        message: "Overwrite existing database file?") == MessageDialogResult.No)
                    {
                        StatusMessage = "Conversion canceled.";
                        return;
                    }
                }

                await DatabaseConverter.Convert(_inputDatabasePath, _outputDatabasePath, AppendToStatusMessage);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51787");
                AppendToStatusMessage($"\r\nError: {ex.Message}");
            }
            finally
            {
                IsExecuting = false;
            }
        }

        [Command]
        private void SelectInputDatabase()
        {
            if (FileBrowserDialogService.SelectExistingFile(
                "Select input database",
                "SQL Compact database (*.sdf)|*.sdf|All files|*.*")
                is string selectedPath)
            {
                InputDatabasePath = selectedPath;
            }
        }

        [Command]
        private void SelectOutputDatabase()
        {
            if (FileBrowserDialogService.SelectFile(
                "Select output database",
                "SQLite database (*.sqlite)|*.sqlite|All files|*.*")
                is string selectedPath)
            {
                OutputDatabasePath = selectedPath;
            }
        }

        private void AppendToStatusMessage(string message)
        {
            StatusMessage += message;
        }

        [CommandInvalidate(nameof(IsExecuting), nameof(InputDatabasePath), nameof(OutputDatabasePath))]
        private bool CanConvert()
        {
            return !IsExecuting
                && !string.IsNullOrEmpty(InputDatabasePath.TrimPath())
                && !string.IsNullOrEmpty(OutputDatabasePath.TrimPath());
        }

        public void OnEvent(DatabaseInputOutputEvent eventData)
        {
            try
            {
                _ = eventData ?? throw new ArgumentNullException(nameof(eventData));

                InputDatabasePath = eventData.InputDatabasePath;
                OutputDatabasePath = eventData.OutputDatabasePath;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51793");
            }
        }
    }
}
