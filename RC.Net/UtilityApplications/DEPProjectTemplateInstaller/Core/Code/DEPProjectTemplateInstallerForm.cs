using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Threading;

namespace DEPProjectTemplateInstaller
{
    /// <summary>
    /// 
    /// </summary>
    public partial class DEPProjectTemplateInstallerForm : Form
    {
        #region Consts

        /// <summary>
        /// The name used for the default DEP template.
        /// </summary>
        static readonly string _DEP_TEMPLATE = "DEP_Template";

        #endregion Consts

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DEPProjectTemplateInstallerForm"/> class.
        /// </summary>
        public DEPProjectTemplateInstallerForm()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_createButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButton_Click(object sender, EventArgs e)
        {
            string sourceDir = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                _DEP_TEMPLATE);

            string templateName = _templateNameTextBox.Text;
            string templateFileName = Regex.Replace(templateName, @"[^a-zA-Z0-9_\-\x20]+", "_");

            string installDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                @"Visual Studio 2010\Templates\ProjectTemplates",
                templateFileName);
            string installZip = installDir + ".zip";

            if (File.Exists(installZip))
            {
                if (MessageBox.Show("The specified template already exists. Replace?", "Replace template?",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                {
                    return;
                }
            }

            InstallTemplateFiles(sourceDir, installDir);

            ApplyTemplateName(templateName, installDir);

            if (_sourceRadioButton.Checked && File.Exists(_sourceProjectTextBox.Text))
            {
                CopyTargetProjectFiles(installDir);
            }

            if (File.Exists(installZip))
            {
                File.Delete(installZip);
            }
            CreateZip(installDir, installZip);

            Directory.Delete(installDir, true);

            MessageBox.Show("Template created.", "Success", MessageBoxButtons.OK);
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_sourceProjectBrowseButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSourceProjectBrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFile = new OpenFileDialog())
            {
                if (File.Exists(_sourceProjectTextBox.Text))
                {
                    openFile.InitialDirectory = Path.GetDirectoryName(_sourceProjectTextBox.Text);
                }

                openFile.Filter = "C# Project Files (*.csproj)|*.csproj";
                openFile.FilterIndex = 0;
                openFile.AddExtension = true;
                openFile.Multiselect = false;
                openFile.CheckFileExists = true;
                openFile.CheckPathExists = true;

                if (openFile.ShowDialog() == DialogResult.OK)
                {
                    _sourceProjectTextBox.Text = openFile.FileName;
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="RadioButton.CheckedChanged"/> event of the
        /// <see cref="_sourceRadioButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSourceRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            _sourceProjectTextBox.Enabled = _sourceRadioButton.Checked;
            _sourceProjectBrowseButton.Enabled = _sourceRadioButton.Checked;
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Installs appropriate product-specific files.
        /// </summary>
        /// <param name="sourceDir">The un-configured source template files.</param>
        /// <param name="installDir">The template install directory.</param>
        void InstallTemplateFiles(string sourceDir, string installDir)
        {
            CopyDirectory(sourceDir, installDir, true);

            string labDEProj = Path.Combine(installDir, "LabDE.csproj");
            string labDEConfig = Path.Combine(installDir, "LabDE.config");
            string flexIndexProj = Path.Combine(installDir, "FlexIndex.csproj");
            string flexIndexConfig = Path.Combine(installDir, "FlexIndex.config");
            string targetProj = Path.Combine(installDir, _DEP_TEMPLATE + ".csproj");
            string targetConfig = Path.Combine(installDir, _DEP_TEMPLATE + ".config");
            if (_labdeRadioButton.Checked)
            {
                File.Move(labDEProj, targetProj);
                File.Move(labDEConfig, targetConfig);
                File.Delete(flexIndexProj);
                File.Delete(flexIndexConfig);
            }
            else
            {
                File.Move(flexIndexProj, targetProj);
                File.Move(flexIndexConfig, targetConfig);
                File.Delete(labDEProj);
                File.Delete(labDEConfig);
            }
        }

        /// <summary>
        /// Applies the name of the template to the template files.
        /// </summary>
        /// <param name="templateName">Th name to apply to the template.</param>
        /// <param name="installDir">The directory the template is installed into.</param>
        static void ApplyTemplateName(string templateName, string installDir)
        {
            string templateFile = Path.Combine(installDir, _DEP_TEMPLATE) + ".vstemplate";
            if (templateName != _DEP_TEMPLATE)
            {
                ReplaceText(templateFile,
                    "<Name>" + _DEP_TEMPLATE + "</Name>",
                    "<Name>" + XmlEscape(templateName) + "</Name>");

                if (!templateName.Equals(_DEP_TEMPLATE, StringComparison.OrdinalIgnoreCase))
                {
                    string newTemplateFile =
                        Path.Combine(installDir, new DirectoryInfo(installDir).Name) + ".vstemplate";
                    File.Move(templateFile, newTemplateFile);

                    string formFile =
                        Path.Combine(installDir, _DEP_TEMPLATE) + "Panel.cs";
                    ReplaceText(formFile, _DEP_TEMPLATE, "$safeprojectname$");
                }
            }
        }

        /// <summary>
        /// Copies the target project files into the new template and renames members to match the
        /// template name.
        /// </summary>
        /// <param name="templateDir">The template directory.</param>
        void CopyTargetProjectFiles(string templateDir)
        {
            string sourceProjectDir = Path.GetDirectoryName(_sourceProjectTextBox.Text);
            string oldProjectName =
                Regex.Match(_sourceProjectTextBox.Text, @"(?<=Extract\.DataEntry\.DEP\.)[\w]+(?=\.csproj)")
                .Value;

            string designerFile = Directory.GetFiles(sourceProjectDir, "*.Designer.cs").Single();
            string newDesignerFile =
                Path.Combine(templateDir, _DEP_TEMPLATE) + "Panel.Designer.cs";
            File.Copy(designerFile, newDesignerFile, true);
            ReplaceText(newDesignerFile, oldProjectName, "$safeprojectname$");

            string resxFile = Directory.GetFiles(sourceProjectDir, "*.resx").Single();
            string newResxFile =
                Path.Combine(templateDir, _DEP_TEMPLATE) + "Panel.resx";
            File.Copy(resxFile, newResxFile, true);
        }

        static void CopyDirectory(string source, string destination, bool recursive)
        {
            // Ensure the destination path ends in the directory separator character
            if (destination[destination.Length - 1] != Path.DirectorySeparatorChar)
            {
                destination += Path.DirectorySeparatorChar;
            }

            // If the destination doesn't exist, create it
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }

            // For each element in the source directory copy it
            foreach (string element in Directory.GetFileSystemEntries(source))
            {
                // Check if this is a directory
                if (Directory.Exists(element))
                {
                    // If it is a directory and recursion was specified,
                    // copy the sub-directory files
                    if (recursive)
                    {
                        CopyDirectory(element, destination + Path.GetFileName(element), true);
                    }
                }
                else
                {
                    // Copy the file
                    File.Copy(element, destination + Path.GetFileName(element), true);
                }
            }
        }

        /// <summary>
        /// Escapes the specified tring for use in XML.
        /// </summary>
        /// <param name="unescaped">The unescaped string.</param>
        /// <returns>The escaped string.</returns>
        static string XmlEscape(string unescaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }

        /// <summary>
        /// Replaces the specified originalText in the specified file with the newText.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="originalText">The original text.</param>
        /// <param name="newText">The new text.</param>
        static void ReplaceText(string fileName, string originalText, string newText)
        {
            string fileText = File.ReadAllText(fileName);
            fileText = fileText.Replace(originalText, newText);
            File.Delete(fileName);
            File.WriteAllText(fileName, fileText);
        }

        /// <summary>
        /// Zips the specified directory into the specified zip archive.
        /// <para><b>Note</b></para>
        /// This code is from Gerald Gibson Jr's article here:
        /// http://www.codeproject.com/Articles/12064/Compress-Zip-files-with-Windows-Shell-API-and-C
        /// </summary>
        /// <param name="sourceDirectory">The directory to zip.</param>
        /// <param name="destFileName">The target zip file archive. Must end in .zip</param>
        static void CreateZip(string sourceDirectory, string destFileName)
        {
            byte[] emptyZip = new byte[]
                {80, 75, 5, 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};

            FileStream fs = File.Create(destFileName);
            fs.Write(emptyZip, 0, emptyZip.Length);
            fs.Flush();
            fs.Close();
            fs = null;

            //Copy a folder and its contents into the newly created zip file
            Shell32.ShellClass sc = new Shell32.ShellClass();
            Shell32.Folder sourceFolder = sc.NameSpace(sourceDirectory);
            Shell32.Folder destFolder = sc.NameSpace(destFileName);
            Shell32.FolderItems items = sourceFolder.Items();
            destFolder.CopyHere(items, 20);

            // Ziping a file using the Windows Shell API creates another thread where the zipping is
            // executed. This means that it is possible that this thread or event handler would end
            // before the zipping thread  starts to execute which would cause the zip to never occur
            // and you will end up with just an empty zip file. So wait a second and give the
            // zipping thread time to get started.
            Thread.Sleep(1000);
        }
    }

    #endregion Private Members
}
