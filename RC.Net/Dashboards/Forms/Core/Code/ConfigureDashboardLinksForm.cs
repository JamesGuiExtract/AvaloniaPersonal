using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Dashboard.Forms
{
    public partial class ConfigureDashboardLinksForm : Form
    {
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

        public ConfigureDashboardLinksForm(HashSet<string> dashboardLinks)
        {
            InitializeComponent();
            DashboardLinks = new HashSet<string>(dashboardLinks, StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region Event Handlers

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
                DashboardLinks.Clear();
                foreach (var value in listBoxDashboardLinks.Items)
                {
                    DashboardLinks.Add(value as string);
                }
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
                if (InputBox.Show(this, "Dashboard Name", "Add Dashboard link", ref value) == DialogResult.OK)
                {
                    if (!string.IsNullOrWhiteSpace(value) && !listBoxDashboardLinks.Items.Contains(value))
                    {
                        listBoxDashboardLinks.Items.Add(value);
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
    }
}
