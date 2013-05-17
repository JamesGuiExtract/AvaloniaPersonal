using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extract.SharePoint.Redaction.Utilities
{
    /// <summary>
    /// Helper form that displays save options to the user.
    /// </summary>
    internal partial class IDShieldForSPSaveFileForm : System.Windows.Forms.Form
    {
        #region Helper Class

        /// <summary>
        /// Class for comparing strings by their length
        /// </summary>
        class InverseLengthCompare : IComparer<string>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="InverseLengthCompare"/> class.
            /// </summary>
            public InverseLengthCompare()
            {
            }

            /// <summary>
            /// Compares the specified strings.
            /// </summary>
            /// <param name="x">The x.</param>
            /// <param name="y">The y.</param>
            /// <returns></returns>
            public int Compare(string x, string y)
            {
                if (x == null)
                {
                    if (y == null)
                    {
                        // If x is null and y is null, they're
                        // equal. 
                        return 0;
                    }
                    else
                    {
                        // If x is null and y is not null, x
                        // is greater. 
                        return 1;
                    }
                }
                else
                {
                    // If x is not null...
                    //
                    if (y == null)
                    // ...and y is null, y is greater.
                    {
                        return -1;
                    }
                    else
                    {
                        // ...and y is not null, compare the 
                        // lengths of the two strings.
                        //
                        int retval = x.Length.CompareTo(y.Length);

                        if (retval != 0)
                        {
                            // If the strings are not of equal length,
                            // the shorter string is greater.
                            //
                            return -retval;
                        }
                        else
                        {
                            // If the strings are of equal length,
                            // sort them with inverse ordinary string comparison.
                            //
                            return string.Compare(y, x, StringComparison.Ordinal);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper class to maintain list information.
        /// </summary>
        class ListData
        {
            /// <summary>
            /// Gets or sets the list id.
            /// </summary>
            /// <value>The list id.</value>
            public Guid ListId { get; set; }

            /// <summary>
            /// Gets or sets the list title.
            /// </summary>
            /// <value>The list title.</value>
            public string ListTitle { get; set; }

            /// <summary>
            /// Gets or sets the list internal name
            /// </summary>
            public string ListInternalName { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ListData"/> class.
            /// </summary>
            /// <param name="listId">The list id.</param>
            /// <param name="title">The title.</param>
            /// <param name="listInternalName">The internal name for the list</param>
            public ListData(Guid listId, string title, string listInternalName)
            {
                ListId = listId;
                ListTitle = title;
                ListInternalName = listInternalName;
            }
        }

        #endregion Helper Class

        #region Constants

        /// <summary>
        /// Places holder node displayed when expanding.
        /// </summary>
        const string _NODE_PLACEHOLDER = "Retrieving folder list...";

        /// <summary>
        /// String compare object used to compare strings based on their length.
        /// </summary>
        static readonly InverseLengthCompare _LENGTH_COMPARE = new InverseLengthCompare();

        /// <summary>
        /// Constants for the tree view image index
        /// </summary>
        const int _DISK_IMAGE = 0;
        const int _FOLDER_IMAGE = 1;
        const int _REFRESH_IMAGE = 2;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name of the redacted file
        /// </summary>
        readonly string _redactedFile;

        /// <summary>
        /// The url for the site
        /// </summary>
        readonly string _siteUrl;

        /// <summary>
        /// The name of the original file
        /// </summary>
        readonly string _originalFile;

        /// <summary>
        /// Tree node for the save to disk option.
        /// </summary>
        readonly TreeNode _saveToDisk = new TreeNode("Save To Local Disk",
            _DISK_IMAGE, _DISK_IMAGE);

        /// <summary>
        /// Gets or sets a value indicating whether the user selected a SharePoint
        /// folder or save to local disk folder.
        /// </summary>
        /// <value>
        /// if <see langword="true"/> user selected a SharePoint destination;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool SaveToSharePoint { get; set; }

        /// <summary>
        /// Gets or sets the destination file path.
        /// </summary>
        /// <value>The destination file path.</value>
        public string DestinationFilePath { get; set; }

        /// <summary>
        /// Gets or sets the list id.
        /// </summary>
        /// <value>The list id.</value>
        public Guid ListId { get; set; }

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="IDShieldForSPSaveFileForm"/> class.
        /// </summary>
        /// <param name="redactedFile">The redacted file.</param>
        /// <param name="originalFile">The original file.</param>
        /// <param name="siteUrl">The site URL.</param>
        public IDShieldForSPSaveFileForm(string redactedFile, string originalFile,
            string siteUrl)
            : this()
        {
            _redactedFile = redactedFile;
            _originalFile = originalFile;
            _siteUrl = siteUrl;
            Icon = Properties.Resources.IDShieldLogo;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IDShieldForSPSaveFileForm"/> class.
        /// </summary>
        public IDShieldForSPSaveFileForm()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(System.EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                if (string.IsNullOrEmpty(_redactedFile))
                {
                    return;
                }

                if (!string.IsNullOrEmpty(_originalFile))
                {
                    _textSourceFile.Text = _originalFile;
                }

                _textRedactedFile.Text = Path.GetFileName(_redactedFile);

                // Add the save to local disk node at the top
                _treeSaveFile.Nodes.Add(_saveToDisk);

                using (var context = new ClientContext(_siteUrl))
                {
                    // Load the document libraries from the web context
                    var query = from list in context.Web.Lists
                                where list.Hidden == false
                                && list.BaseTemplate == 101
                                && list.Title != "Site Assets"
                                select list;
                    var result = context.LoadQuery(query);
                    context.ExecuteQuery();

                    // Add the root nodes to the tree view
                    foreach (var item in result)
                    {
                        // Determine Internal name
                        string internalName = item.Title;

                        // Get the internal name of the library from the default view URL
                        // NOTE: This internal name is needed to upload files properly if 
                        //       the internal name is different from the Title name
                        if (item.DefaultViewUrl.Count( c => c == '/') >= 2)
                        {
                            internalName = item.DefaultViewUrl.Split('/')[1];
                        }
                        var data = new ListData(item.Id, item.Title, internalName);
                        var node = new TreeNode(data.ListTitle, _FOLDER_IMAGE,
                            _FOLDER_IMAGE, new TreeNode[] {
                                new TreeNode(_NODE_PLACEHOLDER, _REFRESH_IMAGE, _REFRESH_IMAGE)
                            });
                        node.Name = data.ListTitle;
                        node.Tag = data;

                        _treeSaveFile.Nodes.Add(node);
                    }
                }

                UpdateSaveButtonEnabledState();

                BeginInvoke((MethodInvoker)(() =>
                    {
                        // Cycle top most property to bring window to the front
                        TopMost = true;
                        TopMost = false;
                    }));
            }
            catch (Exception ex)
            {
                ex.LogExceptionWithHelperApp("ELI31459");
                ex.DisplayInMessageBox();
            }
        }

        /// <summary>
        /// Handles the tree node selection changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.TreeViewEventArgs"/> instance containing the event data.</param>
        void HandleTreeNodeSelectionChanged(object sender, TreeViewEventArgs e)
        {
            try
            {
                UpdateSaveButtonEnabledState();
            }
            catch (Exception ex)
            {
                ex.LogExceptionWithHelperApp("ELI31494");
                ex.DisplayInMessageBox();
            }
        }

        /// <summary>
        /// Handles the tree node exanded.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.TreeViewCancelEventArgs"/> instance containing the event data.</param>
        void HandleTreeNodeExanded(object sender, TreeViewEventArgs e)
        {
            try
            {
                var nodes = e.Node.Nodes;

                // Check if this is a root node OR if the node already has its
                // children computed
                if (e.Node.Parent != null ||
                    !(nodes.Count == 1
                    && nodes[0].Text.Equals(_NODE_PLACEHOLDER, StringComparison.Ordinal)))
                {
                    return;
                }

                // Refresh the tree view
                _treeSaveFile.Refresh();

                // Suspend updates
                _treeSaveFile.BeginUpdate();
                try
                {
                    // Remove the place holder node
                    nodes.RemoveAt(0);

                    // Get the data associated with the root node
                    var data = e.Node.Tag as ListData;
                    var rootText = e.Node.Text;
                    var rootTextLength = rootText.Length;
                    var nodeStrings = BuildListOfNodeStrings(data.ListId);
                    foreach (var nodeString in nodeStrings)
                    {
                        // Get the list of nodes
                        var roots = nodeString.Split(new char[] { '/' },
                            StringSplitOptions.RemoveEmptyEntries);

                        // The initial parent is the current root node
                        var parentNode = e.Node;
                        var sb = new StringBuilder(rootText, nodeString.Length + rootTextLength);
                        for (int rootIndex = 0; rootIndex < roots.Length; rootIndex++)
                        {
                            var parentName = roots[rootIndex];
                            sb.Append("/" + parentName);
                            var nodeName = sb.ToString();
                            var index = parentNode.Nodes.IndexOfKey(nodeName);
                            if (index == -1)
                            {
                                var temp = new TreeNode(parentName, 1, 1);
                                temp.Name = nodeName;
                                parentNode.Nodes.Add(temp);
                                parentNode = temp;
                            }
                            else
                            {
                                parentNode = parentNode.Nodes[index];
                            }
                        }
                    }
                }
                finally
                {
                    _treeSaveFile.EndUpdate();
                    _treeSaveFile.Invalidate();
                }
            }
            catch (Exception ex)
            {
                ex.LogExceptionWithHelperApp("ELI31460");
                ex.DisplayInMessageBox();
            }
        }

        /// <summary>
        /// Builds the list of node strings.
        /// </summary>
        /// <param name="listId">The list id.</param>
        /// <returns></returns>
        List<string> BuildListOfNodeStrings(Guid listId)
        {
            using (var context = new ClientContext(_siteUrl))
            {
                // Get the list
                var list = context.Web.Lists.GetById(listId);

                // Get all folders from the list
                var query = CamlQuery.CreateAllFoldersQuery();
                var folders = list.GetItems(query);
                context.Load(list);
                context.Load(folders,
                    folder => folder.Include(fi => fi["FileDirRef"],
                    fi => fi["FileLeafRef"]));
                context.ExecuteQuery();

                var listUrl = list.DefaultViewUrl;
                listUrl = listUrl.Substring(0, listUrl.IndexOf("/Forms", 0, StringComparison.Ordinal));
                var urlLength = listUrl.Length;

                // Build the path string for each folder
                // maintain a sorted order based on the length
                var paths = new List<string>();
                foreach (var folder in folders)
                {
                    var leaf = (string)folder["FileLeafRef"];
                    var root = (string)folder["FileDirRef"];
                    var index = root.IndexOf(listUrl, 0, StringComparison.Ordinal);
                    if (index != -1)
                    {
                        root = root.Remove(index, urlLength);
                    }
                    var path = string.Concat(root, "/", leaf);
                    paths.Add(path);
                }
                paths.Sort(_LENGTH_COMPARE);

                // Build the final list of all nodes that need to be added
                // Remove any paths that are contained within a longer path
                var finalList = new List<string>();
                foreach (var path in paths)
                {
                    if (!finalList.Any(item => item.StartsWith(path, StringComparison.Ordinal)))
                    {
                        finalList.Add(path);
                    }
                }

                return finalList;
            }
        }

        /// <summary>
        /// Handles the save click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleSaveClick(object sender, EventArgs e)
        {
            try
            {
                var node = _treeSaveFile.SelectedNode;
                if (node == null || string.IsNullOrEmpty(_textRedactedFile.Text))
                {
                    return;
                }

                if (node == _saveToDisk)
                {
                    using (var saveFile = new SaveFileDialog())
                    {
                        saveFile.FileName = _textRedactedFile.Text;
                        saveFile.Filter = "All Files (*.*)|*.*||";
                        if (saveFile.ShowDialog() == DialogResult.OK)
                        {
                            DestinationFilePath = saveFile.FileName;
                            SaveToSharePoint = false;
                            DialogResult = DialogResult.OK;
                        }
                    }
                }
                else
                {
                    string fileName = _textRedactedFile.Text;
                    if (Regex.IsMatch(fileName, "\\.{2,}|[#%&*:<>?{}|~!@$^+=,'`\\[\\]\"\\/\\\\]"))
                    {
                        MessageBox.Show("The specified file name contains some invalid characters.",
                            "Invalid File Name", MessageBoxButtons.OK, MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1, 0);
                        _textRedactedFile.Focus();
                        return;
                    }

                    // Get the root node
                    var topNode = node;
                    while (topNode.Parent != null)
                    {
                        topNode = topNode.Parent;
                    }

                    ListData topLD = ((ListData)topNode.Tag);

                    // Replace the library name in the current node with the Internal Name
                    // FlexIDSIntegrations #313
                    string path = node.Name.Replace(topLD.ListTitle, topLD.ListInternalName);
                    
                    // Get the list id from the root node
                    ListId = topLD.ListId;

                    // Create the destination 
                    DestinationFilePath = string.Concat("/", path, "/", fileName);
                    SaveToSharePoint = true;
                    DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                ex.LogExceptionWithHelperApp("ELI31490");
                ex.DisplayInMessageBox();
            }
        }

        /// <summary>
        /// Handles the redacted file text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleRedactedFileTextChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateSaveButtonEnabledState();
            }
            catch (Exception ex)
            {
                ex.LogExceptionWithHelperApp("ELI31518");
                ex.DisplayInMessageBox();
            }
        }

        /// <summary>
        /// Updates the state of the save button enabled.
        /// </summary>
        void UpdateSaveButtonEnabledState()
        {
            _buttonSave.Enabled = _treeSaveFile.SelectedNode != null && !string.IsNullOrEmpty(_textRedactedFile.Text);
        }

        #endregion Methods
    }
}
