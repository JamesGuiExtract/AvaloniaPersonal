using Extract.Utilities.WPF;
using Extract.Utilities.ReactiveUI;
using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Services;
using Extract.Web.ApiConfiguration.ViewModels;
using Extract.Web.ApiConfiguration.Views;
using ReactiveUI;
using Splat;
using System;
using System.Reactive.Disposables;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Web.ApiConfiguration
{
    public class ApiConfigMgmtForm : Form
    {
        readonly CompositeDisposable _disposables = new();
        readonly ElementHost _elementHost;
        readonly string _databaseServer;
        readonly string _databaseName;
        IMessageDialogService _messageDialogService;
        ApiConfigMgmtViewModel _configMgmtViewModel;

        /// <summary>
        /// A form that contains WPF controls to manage web API configurations
        /// </summary>
        public ApiConfigMgmtForm(string databaseServer, string databaseName)
        {
            try
            {
                _databaseServer = databaseServer ?? throw new ArgumentNullException(nameof(databaseServer));
                _databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));

                Height = 690;
                Width = 950;
                MinimumSize = new System.Drawing.Size(400, 400);

                ShowInTaskbar = false;
                MaximizeBox = true;
                MinimizeBox = false;
                Text = "Web API Configurations";
                ShowIcon = false;
                StartPosition = FormStartPosition.CenterParent;

                _elementHost = new()
                {
                    Dock = DockStyle.Fill
                };
                Controls.Add(_elementHost);
                _disposables.Add(_elementHost);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53695");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // BEGIN Composition Root

                // Handle exceptions thrown from observable subscriptions, e.g.
                RxApp.DefaultExceptionHandler = new GlobalErrorHandler("ELI53778");

                _messageDialogService = new MessageDialogService(window => new WindowInteropHelper(window).Owner = Handle);

                // Register dependencies to simplify creating the object graph
                SplatRegistrations.Register<ApiConfigMgmtViewModel>();
                SplatRegistrations.RegisterConstant(_messageDialogService);
                SplatRegistrations.RegisterConstant<IFileProcessingDB>(new FileProcessingDBClass { DatabaseServer = _databaseServer, DatabaseName = _databaseName });
                SplatRegistrations.RegisterLazySingleton<IFileBrowserDialogService, FileBrowserDialogService>();
                SplatRegistrations.RegisterLazySingleton<IApiConfigViewModelFactory, ApiConfigViewModelFactory>();
                SplatRegistrations.RegisterLazySingleton<IObservableConfigurationDatabaseService, ConfigurationDatabaseService>();
                SplatRegistrations.SetupIOC();
                
                _configMgmtViewModel = Locator.Current.GetService<ApiConfigMgmtViewModel>();
                _disposables.Add(_configMgmtViewModel);

                _elementHost.Child = new ApiConfigMgmtView()
                {
                    DataContext = _configMgmtViewModel
                };

                // END Composition Root

                // Allow view models to close the window
                MessageBus.Current.Listen<CloseWindowMessage>()
                    .Subscribe(_ => Close())
                    .DisposeWith(_disposables);

            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI53696");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (_configMgmtViewModel.HasUnsavedChanges())
                {
                    var dialogResult = _messageDialogService.ShowYesNoCancelDialog("Save Changes", "There are uncommitted changes. Save now?");
                    if (dialogResult == MessageDialogResult.Yes)
                    {
                        _configMgmtViewModel.SaveAllChanges();
                    }
                    else if (dialogResult == MessageDialogResult.Cancel && e is not null)
                    {
                        e.Cancel = true;
                    }
                }

                base.OnFormClosing(e);
            }
            catch (Exception ex)
            {
                if (e is not null)
                {
                    e.Cancel = true;
                }
                ex.ExtractDisplay("ELI53766");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposables.Dispose();
            }

            base.Dispose(disposing);

            GC.Collect();
        }
    }
}
