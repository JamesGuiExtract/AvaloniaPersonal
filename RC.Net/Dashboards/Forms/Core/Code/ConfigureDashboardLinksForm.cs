using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Dashboard.Forms
{
    public partial class ConfigureDashboardLinksForm : Form
    {
        #region Private Fields
        
        HashSet<string> _existingDashboards;

        bool _dirty = false; 
        
        #endregion

        #region Public Properties

        public HashSet<string> DashboardLinks { get; }

        #endregion

        #region Constructors

        public ConfigureDashboardLinksForm()
        {
            InitializeComponent();
            buttonDelete.Enabled = false;
            DashboardLinks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        public ConfigureDashboardLinksForm(HashSet<string> dashboardLinks, HashSet<string> existingDashboards)
        {
            InitializeComponent();
            DashboardLinks = new HashSet<string>(dashboardLinks, StringComparer.OrdinalIgnoreCase);
            _existingDashboards = existingDashboards;
        }

        #endregion

        #region Event Handlers

        void HandleConfigureDashboardLinksForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {

                if (_dirty && DialogResult != DialogResult.Cancel)
                {
                    var result = MessageBox.Show("Keep changes?", "Keep changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 0);
                    switch (result)
                    {
                        case DialogResult.Cancel:
                            e.Cancel = true;
                            break;
                        case DialogResult.Yes:
                            SaveDataToDashboardLinks();
                            DialogResult = DialogResult.OK;
                            break;
                        case DialogResult.No:
                            DialogResult = DialogResult.OK;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47219");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                listBoxDashboardLinks.Items.AddRange(DashboardLinks.ToArray());

            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47179");
            }
        }
        void HandleButtonOk_Click(object sender, EventArgs e)
        {
            try
            {
                SaveDataToDashboardLinks();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47088");
            }
        }

 
        void HandleButtonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                string value = string.Empty;
                var selectDashboardForm = new SelectFromListInputBox();
                selectDashboardForm.SelectionStrings = _existingDashboards
                    .Except(listBoxDashboardLinks.Items.OfType<string>(), StringComparer.OrdinalIgnoreCase).ToList();
                selectDashboardForm.Prompt = "Dashboard Name";
                selectDashboardForm.Title = "Add Dashboard link";
                selectDashboardForm.DropDownStyle = ComboBoxStyle.DropDown;
                if (selectDashboardForm.ShowDialog(this) == DialogResult.OK)
                {
                    value = selectDashboardForm.ReturnValue;
                    if (!string.IsNullOrWhiteSpace(value) && !listBoxDashboardLinks.Items.Contains(value))
                    {
                        listBoxDashboardLinks.Items.Add(value);
                        _dirty = true; ;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47089");
            }
        }

        void HandleButtonDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (listBoxDashboardLinks.SelectedIndex >= 0)
                {
                    listBoxDashboardLinks.Items.RemoveAt(listBoxDashboardLinks.SelectedIndex);
                    _dirty = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47090");
            }
        }

        void HandleListBoxDashboardLinks_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                buttonDelete.Enabled = listBoxDashboardLinks.SelectedIndex >= 0;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47091");
            }
        }

        #endregion

        #region Private Methods
        
        void SaveDataToDashboardLinks()
        {
            DashboardLinks.Clear();
            foreach (var value in listBoxDashboardLinks.Items)
            {
                DashboardLinks.Add(value as string);
            }
            _dirty = false;
        }
        #endregion

    }
}
