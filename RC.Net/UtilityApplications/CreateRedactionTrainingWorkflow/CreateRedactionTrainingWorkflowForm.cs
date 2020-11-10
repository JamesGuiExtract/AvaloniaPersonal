using Extract;
using System;
using System.IO;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace VerifierWorkflowConfig
{
    public partial class CreateRedactionTrainingWorkflowForm : Form
    {
        static readonly string _DEFAULT_WORKFLOW_LOCATION = @"\\engsvr\ps\Services\Verifier_Training";
        static readonly string _DEFAULT_DB_SERVER = @"pssvr";
        static readonly string _DEFAULT_DB_NAME = @"Verifier_Training";
        static readonly string _DEFAULT_SET_NAME = @"SetA";

        public CreateRedactionTrainingWorkflowForm()
        {
            InitializeComponent();
        }

        public string WorkflowLocation { get; set; } = _DEFAULT_WORKFLOW_LOCATION;
        public string DbServer { get; set; } = _DEFAULT_DB_SERVER;
        public string DbName { get; set; } = _DEFAULT_DB_NAME;
        public string UserLogin { get; set; }
        public string SetName { get; set; } = _DEFAULT_SET_NAME;
        public string WorkflowName { get; set; }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _workflowLocationTextBox.Text = WorkflowLocation;
            _loginTextBox.Text = UserLogin;
            _dbServerTextBox.Text = DbServer;
            _dbNameTextBox.Text = DbName;
            _setNameTextBox.Text = SetName;
        }

        void HandleCreateButton_Click(object sender, EventArgs e)
        {
            try
            {
                WorkflowLocation = _workflowLocationTextBox.Text;
                UserLogin = _loginTextBox.Text;
                DbServer = _dbServerTextBox.Text;
                DbName = _dbNameTextBox.Text;
                SetName = _setNameTextBox.Text;

                WorkflowName = UserLogin + ":" + SetName;

                try
                {
                    Cursor.Current = Cursors.WaitCursor;

                    _statusLabel.Text = "Copying master files to user input directory...";
                    _statusLabel.Refresh();

                    CopyFilesToWorkflowDirectory();

                    _statusLabel.Text = "Configuring database workflow...";
                    _statusLabel.Refresh();

                    AddWorkflowToDatabase();
                    var setupFpsFilename = SaveWorkflowFps("Setup");
                    SaveWorkflowFps("Verify");

                    _statusLabel.Text = "Finished installing workflow for " + UserLogin;
                    _statusLabel.Refresh();
                    _createButton.Enabled = false;

                    MessageBox.Show("Run FPS\\\"" + Path.GetFileName(setupFpsFilename) + "\" to initialize the files for " + UserLogin);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51444");
            }
        }

        void CopyFilesToWorkflowDirectory()
        {
            string masterFilesFolder = Path.Combine(WorkflowLocation, "MasterFiles", SetName).ToLowerInvariant();
            string userFolder = Path.Combine(WorkflowLocation, "Input_" + UserLogin);
            if (!Directory.Exists(userFolder))
            {
                Directory.CreateDirectory(userFolder);
            }
            string workflowFolder = Path.Combine(userFolder, SetName);

            if (!Directory.Exists(workflowFolder))
            {
                Directory.CreateDirectory(workflowFolder);
            }

            foreach (var sourceFile in Directory.EnumerateFiles(masterFilesFolder, "*.tif", SearchOption.AllDirectories))
            {
                var destPath = sourceFile.ToLowerInvariant().Replace(masterFilesFolder, workflowFolder);
                var destDirectory = Path.GetDirectoryName(destPath);

                if (!Directory.Exists(destDirectory))
                {
                    Directory.CreateDirectory(destDirectory);
                }

                File.Copy(sourceFile, destPath, false);
            }
        }

        void AddWorkflowToDatabase()
        {
            var famDB = new FileProcessingDB();
            famDB.DatabaseServer = DbServer;
            famDB.DatabaseName = DbName;
            int workflowID = famDB.AddWorkflow(WorkflowName, EWorkflowType.kRedaction);

            var actionList = new IUnknownVector();
            var variantVector = famDB.GetActions().GetKeys();
            var count = variantVector.Size;
            for (int i = 0; i < count; i++)
            {
                var vecActionInfo = new VariantVector();
                vecActionInfo.PushBack(variantVector[i]);
                vecActionInfo.PushBack(true); // main sequence

                actionList.PushBack(vecActionInfo);
            }

            famDB.SetWorkflowActions(workflowID, actionList);
        }

        string SaveWorkflowFps(string baseFpsName)
        {
            string masterFpsFile = Path.Combine(WorkflowLocation, "FPS", baseFpsName + ".fps");
            string workflowFpsFile = Path.Combine(WorkflowLocation, "FPS", baseFpsName + " " + UserLogin + "_" + SetName + ".fps");

            var famConfig = new FileProcessingManager();
            famConfig.LoadFrom(masterFpsFile, false);
            famConfig.ActiveWorkflow = WorkflowName;
            famConfig.SaveTo(workflowFpsFile, true);

            return workflowFpsFile;
        }

        void HandleTextChanged(object sender, EventArgs e)
        {
            _createButton.Enabled = true;
        }
    }
}
