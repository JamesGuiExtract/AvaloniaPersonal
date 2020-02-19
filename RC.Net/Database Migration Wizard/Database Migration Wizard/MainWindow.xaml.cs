using DatabaseMigrationWizard.Database;
using DatabaseMigrationWizard.Pages.Utility;
using Extract.Licensing;
using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace DatabaseMigrationWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        public ConnectionInformation ConnectionInformation { get; set; }

        public MainWindow(ConnectionInformation connectionInformation)
        {
            this.ConnectionInformation = connectionInformation;
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            InitializeComponent();

            //var fileProcessingDb = new FileProcessingDB()
            //{
            //    DatabaseServer = connectionInformation.DatabaseServer,
            //    DatabaseName = connectionInformation.DatabaseName
            //};
            //
            //bool invalidResult = true;
            //while(invalidResult)
            //{
            //    try
            //    {
            //        var results = BuildDialog("Enter Admin Password:", "DBWizard");
            //        var dialogResult = results.Item1.ShowDialog();
            //
            //        switch(dialogResult)
            //        {
            //            case System.Windows.Forms.DialogResult.OK:
            //                fileProcessingDb.LoginUser("admin", results.Item2.Text);
            //                invalidResult = false;
            //                break;
            //            case System.Windows.Forms.DialogResult.Cancel:
            //                System.Windows.Forms.Application.Exit();
            //                break;
            //        }
            //    }
            //    catch(Exception){ }
            //}
        }

        private static (Form, TextBox) BuildDialog(string text, string caption)
        {
            Form prompt = new Form()
            {
                Width = 250,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen,
            };
            Label textLabel = new Label() { Left = 10, Top = 10, Width = 200, Text = text };
            TextBox textBox = new TextBox() { Left = 10, Top = 40, Width = 200, PasswordChar = '*' };
            Button confirmation = new Button() { Text = "Ok", Left = 110, Width = 100, Top = 60, DialogResult = System.Windows.Forms.DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return (prompt, textBox);
        }
    }
}
