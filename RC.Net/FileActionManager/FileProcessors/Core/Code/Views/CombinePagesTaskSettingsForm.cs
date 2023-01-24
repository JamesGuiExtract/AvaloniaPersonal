using Extract.FileActionManager.FileProcessors.Models;
using Extract.FileActionManager.FileProcessors.ViewModels;
using Extract.Utilities;
using Extract.Utilities.ReactiveUI;
using Extract.Utilities.WPF;
using ReactiveUI;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using UCLID_FILEPROCESSINGLib;

using SettingsModel = Extract.FileActionManager.FileProcessors.Models.CombinePagesTaskSettingsModelV1;

namespace Extract.FileActionManager.FileProcessors.Views
{
    [CLSCompliant(false)]
    public class CombinePagesTaskSettingsForm : Form
    {
        readonly CompositeDisposable _disposables = new();
        private ElementHost _elementHost;
        CombinePagesTaskSettingsViewModel _viewModel;

        public CombinePagesTaskSettingsForm(SettingsModel settingsModel, IFileProcessingDB fileProcessingDB)
        {
            try
            {
                InitializeComponent();
                
                _viewModel = new CombinePagesTaskSettingsViewModel(
                    settingsModel,
                    fileProcessingDB,
                    new FileBrowserDialogService(),
                    new MessageDialogService(window =>
                        new WindowInteropHelper(window).Owner = this.Handle));

                _disposables.Add(_elementHost);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53910");
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public SettingsModel GetSettings()
        {
            try
            {
                return _viewModel?.GetSettings() ?? new();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53938");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Handle exceptions thrown from observable subscriptions, e.g.
                RxApp.DefaultExceptionHandler = new GlobalErrorHandler("ELI53927");

                _elementHost.Child = new SpecifiedPaginationTaskSettingsView()
                {
                    DataContext = _viewModel
                };

                // Allow view models to close the window
                MessageBus.Current.Listen<OkWindowMessage>()
                    .Subscribe(_ => HandleOkWindowMessage())
                    .DisposeWith(_disposables);

                // Allow view models to close the window
                MessageBus.Current.Listen<CloseWindowMessage>()
                    .Subscribe(_ => Close())
                    .DisposeWith(_disposables);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI53911");
            }
        }

        void HandleOkWindowMessage()
        {
            try
            {
                string validationErrors = string.Join(
                    "\r\n\r\n",
                    _viewModel.ValidationErrors);
                if (string.IsNullOrEmpty(validationErrors))
                {
                    if (_viewModel.VerifyOutputPath())
                    {
                        DialogResult = DialogResult.OK;
                        Close();
                    }
                }
                else
                {
                    UtilityMethods.ShowMessageBox(
                        validationErrors,
                        "Invalid Settings",
                        true);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI53928");
            }
        }

        /// <summary>
        /// HACK: There is obviously a better way to do this, but after much fuss, this is at least
        /// a way I found the form to be sizeable while respecting the required size of _elementHost.
        /// </summary>
        public override Size MinimumSize 
        { 
            get
            {
                var borderSize = new Size(
                    Width - ClientSize.Width,
                    Height - ClientSize.Height);

                var requiredSize = _elementHost.MinimumSize + borderSize;

                return new Size(
                    Math.Max(requiredSize.Width, base.MinimumSize.Width),
                    Math.Max(requiredSize.Height, base.MinimumSize.Height));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposables.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this._elementHost = new System.Windows.Forms.Integration.ElementHost();
            this.SuspendLayout();
            // 
            // _elementHost
            // 
            this._elementHost.AutoSize = true;
            this._elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this._elementHost.Location = new System.Drawing.Point(0, 0);
            this._elementHost.MinimumSize = new System.Drawing.Size(400, 250);
            this._elementHost.Name = "_elementHost";
            this._elementHost.Size = new System.Drawing.Size(584, 361);
            this._elementHost.TabIndex = 0;
            this._elementHost.Text = "_elementHost";
            this._elementHost.Child = null;
            // 
            // SpecifiedPaginationTaskSettingsForm
            // 
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this._elementHost);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SpecifiedPaginationTaskSettingsForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Specified Pagination Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
