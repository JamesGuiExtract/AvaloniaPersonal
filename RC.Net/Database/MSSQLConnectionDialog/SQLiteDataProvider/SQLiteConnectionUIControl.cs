using System;
using System.Windows.Forms;

namespace Microsoft.Data.ConnectionUI
{
	/// <summary>
	/// Represents the connection UI control for the SQLite provider.
	/// </summary>
	internal partial class SQLiteConnectionUIControl : UserControl, IDataConnectionUIControl
	{
		private bool _loading;

		private SQLiteConnectionProperties _properties;

		public SQLiteConnectionUIControl()
		{
			InitializeComponent();
			RightToLeft = RightToLeft.Inherit;
			this.createButton.Enabled = false;
		}

		private string DataSourceProperty
		{
			get
			{
				return "Data Source";
			}
		}

		private string PasswordProperty
		{
			get
			{
				return "Password";
			}
		}

		public string PersistSecurityInfoProperty
		{
			get
			{
				return "Persist Security Info";
			}
		}

		public void Initialize(IDataConnectionProperties connectionProperties)
		{
			if (connectionProperties == null)
			{
				throw new ArgumentNullException("connectionProperties");
			}
			SQLiteConnectionProperties properties = connectionProperties as SQLiteConnectionProperties;
			if (properties == null)
			{
				throw new ArgumentException(Resources.SQLiteConnectionUIControl_InvalidConnectionProperties);
			}
			_properties = properties;
		}

		public void LoadProperties()
		{
			_loading = true;

			string dataSource = _properties[DataSourceProperty] as string;
			databaseTextBox.Text = dataSource;

			_loading = false;
		}

		private void databaseTextBox_TextChanged(object sender, EventArgs e)
		{
			if (!_loading)
			{
				string dataSource = databaseTextBox.Text.Trim();
				if (dataSource.Length == 0)
				{
					dataSource = null;
				}
				_properties[DataSourceProperty] = dataSource;
			}
		}

		private void browseButton_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog fileDialog = new OpenFileDialog())
			{
				fileDialog.Title = Resources.SQLiteConnectionUIControl_BrowseFileTitle;
				fileDialog.Multiselect = false;
				if (String.IsNullOrEmpty(_properties[DataSourceProperty] as string))
				{
					fileDialog.InitialDirectory = InitialDirectory;
				}
				fileDialog.RestoreDirectory = true;
				fileDialog.Filter = Resources.SQLiteConnectionUIControl_BrowseFileFilter;
				fileDialog.DefaultExt = Resources.SQLiteConnectionUIControl_BrowseFileDefaultExt;
				if (fileDialog.ShowDialog() == DialogResult.OK)
				{
					_properties[DataSourceProperty] = fileDialog.FileName.Trim();
					LoadProperties();
				}
			}
		}

		private void TrimControlText(object sender, EventArgs e)
		{
			Control c = sender as Control;
			c.Text = c.Text.Trim();
		}

		private static string InitialDirectory
		{
			get
			{
                return "";
			}
		}

	}
}
