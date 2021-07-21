//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using Extract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Data.ConnectionUI")]
namespace Microsoft.Data.ConnectionUI
{
	/// <summary>
    /// A DataConnection Dialog UI configuration implementation based on the sample project
    /// in the Data Connnection Dialog code made available by Microsoft
    /// (http://archive.msdn.microsoft.com/Connection)
    /// The changes made to the initial implementation are:
    /// - Added XML comments
    /// - Added try/catch blocks
    /// - Added GetDataSourceFromName and GetDataProviderFromName
    /// - Refactored when datasource and dataproviders are initialized so the are available before
    ///   the LoadConfiguration call.
	/// </summary>
    [CLSCompliant(false)]
	public class DataConnectionConfiguration : IDataConnectionConfiguration
	{
		private const string configFileName = @"DataConnection.xml";
		private string fullFilePath = null;
		private XDocument xDoc = null;

		// Available data sources:
		private IDictionary<string, DataSource> dataSources;

		// Available data providers: 
		private IDictionary<string, DataProvider> dataProviders;

        // Object to lock when initializing
        private static object lockInitilization = new object();

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="path">Configuration file path.</param>
		public DataConnectionConfiguration(string path)
		{
			if (!String.IsNullOrEmpty(path))
			{
				fullFilePath = Path.GetFullPath(Path.Combine(path, configFileName));
			}
			else
			{
				fullFilePath = Path.Combine(System.Environment.CurrentDirectory, configFileName);
			}
			if (!String.IsNullOrEmpty(fullFilePath) && File.Exists(fullFilePath))
			{
				xDoc = XDocument.Load(fullFilePath);
			}
			else
			{
				xDoc = new XDocument();
				xDoc.Add(new XElement("ConnectionDialog", new XElement("DataSourceSelection")));
			}

			this.RootElement = xDoc.Root;

            InitializeSourcesAndProviders();
		}

        /// <summary>
        /// Gets or sets the root element.
        /// </summary>
        /// <value>
        /// The root element.
        /// </value>
		public XElement RootElement { get; set; }

        /// <summary>
        /// Loads the configuration.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        public void LoadConfiguration(DataConnectionDialog dialog)
		{
            try
            {
                InitializeSourcesAndProviders();

                dialog.DataSources.Add(DataSource.SqlDataSource);
                dialog.DataSources.Add(DataSource.SqlFileDataSource);
                dialog.DataSources.Add(DataSource.OracleDataSource);
                dialog.DataSources.Add(DataSource.AccessDataSource);
                dialog.DataSources.Add(DataSource.OdbcDataSource);
                dialog.DataSources.Add(SqlCe.SqlCeDataSource);
                dialog.DataSources.Add(SQLite.SQLiteDataSource);

                dialog.UnspecifiedDataSource.Providers.Add(DataProvider.SqlDataProvider);
                dialog.UnspecifiedDataSource.Providers.Add(DataProvider.OracleDataProvider);
                dialog.UnspecifiedDataSource.Providers.Add(DataProvider.OleDBDataProvider);
                dialog.UnspecifiedDataSource.Providers.Add(DataProvider.OdbcDataProvider);
                dialog.DataSources.Add(dialog.UnspecifiedDataSource);

                this.dataSources.Add(dialog.UnspecifiedDataSource.DisplayName, dialog.UnspecifiedDataSource);

                DataSource ds = null;
                string dsName = this.SelectedSource;
                if (!String.IsNullOrEmpty(dsName) && this.dataSources.TryGetValue(dsName, out ds))
                {
                    dialog.SelectedDataSource = ds;
                }

                DataProvider dp = null;
                string dpName = this.SelectedProvider;
                if (!String.IsNullOrEmpty(dpName) && this.dataProviders.TryGetValue(dpName, out dp))
                {
                    dialog.SelectedDataProvider = dp;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34735");
            }
		}

        /// <summary>
        /// Saves the configuration.
        /// </summary>
        /// <param name="dataConnectionDialog">The <see cref="DataConnectionDialog"/>.</param>
		public void SaveConfiguration(DataConnectionDialog dataConnectionDialog)
		{
            try
            {
                if (dataConnectionDialog.SaveSelection)
                {
                    DataSource ds = dataConnectionDialog.SelectedDataSource;
                    if (ds != null)
                    {
                        if (ds == dataConnectionDialog.UnspecifiedDataSource)
                        {
                            this.SaveSelectedSource(ds.DisplayName);
                        }
                        else
                        {
                            this.SaveSelectedSource(ds.Name);
                        }
                    }
                    DataProvider dp = dataConnectionDialog.SelectedDataProvider;
                    if (dp != null)
                    {
                        this.SaveSelectedProvider(dp.Name);
                    }

                    xDoc.Save(fullFilePath);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34736");
            }
		}

        /// <summary>
        /// Resets the data source and provider.
        /// </summary>
        public void ResetConfiguration()
        {
            try
            {
                XElement xElem = this.RootElement.Element("DataSourceSelection");
                XElement sourceElem = xElem.Element("SelectedSource");
                if (sourceElem != null)
                {
                    sourceElem.Value = "";
                }

                xElem = this.RootElement.Element("DataSourceSelection");
                sourceElem = xElem.Element("SelectedProvider");
                if (sourceElem != null)
                {
                    sourceElem.Value = "";
                }

                xDoc.Save(fullFilePath);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34798");
            }
        }

        /// <summary>
        /// Gets the selected source.
        /// </summary>
		public string SelectedSource
		{
            get
            {
                try
                {
                    XElement xElem = this.RootElement.Element("DataSourceSelection");
                    XElement sourceElem = xElem.Element("SelectedSource");
                    if (sourceElem != null)
                    {
                        return sourceElem.Value as string;
                    }
                }
                catch { }

                return null;
            }
		}

        /// <summary>
        /// Gets the selected provider.
        /// </summary>
        /// <returns></returns>
		public string SelectedProvider
		{
            get
            {
                try
                {
                    XElement xElem = this.RootElement.Element("DataSourceSelection");
                    XElement providerElem = xElem.Element("SelectedProvider");
                    if (providerElem != null)
                    {
                        return providerElem.Value as string;
                    }
                }
                catch { }

                return null;
            }
		}

        /// <summary>
        /// Saves the selected source.
        /// </summary>
        /// <param name="source">The source.</param>
		public void SaveSelectedSource(string source)
		{
			if (!String.IsNullOrEmpty(source))
			{
				try
				{
					XElement xElem = this.RootElement.Element("DataSourceSelection");
					XElement sourceElem = xElem.Element("SelectedSource");
					if (sourceElem != null)
					{
						sourceElem.Value = source;
					}
					else
					{
						xElem.Add(new XElement("SelectedSource", source));
					}
				}
				catch
				{
				}
			}

		}

        /// <summary>
        /// Saves the selected provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
		public void SaveSelectedProvider(string provider)
		{
			if (!String.IsNullOrEmpty(provider))
			{
				try
				{
					XElement xElem = this.RootElement.Element("DataSourceSelection");
					XElement sourceElem = xElem.Element("SelectedProvider");
					if (sourceElem != null)
					{
						sourceElem.Value = provider;
					}
					else
					{
						xElem.Add(new XElement("SelectedProvider", provider));
					}
				}
				catch
				{
				}
			}
		}

        /// <summary>
        /// Gets a <see cref="DataSource"/> by name
        /// </summary>
        /// <param name="name">The name or display name of the data source.</param>
        /// <returns>The <see cref="DataSource"/> or <see langword="null"/> if none was found by
        /// the specified <see paramref="name"/></returns>
        public DataSource GetDataSourceFromName(string name)
        {
            try
            {
                return dataSources.Values.Where(dataSource =>
                        !string.IsNullOrEmpty(name) &&
                        string.Equals(name, dataSource.Name, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(name, dataSource.DisplayName, StringComparison.OrdinalIgnoreCase))
                        .SingleOrDefault();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34799");
            }
        }

        /// <summary>
        /// Gets a <see cref="DataProvider"/> by name
        /// </summary>
        /// <param name="name">The name or display name of the data provider.</param>
        /// <returns>The <see cref="DataProvider"/> or <see langword="null"/> if none was found by
        /// the specified <see paramref="name"/></returns>
        public DataProvider GetDataProviderFromName(string name)
        {
            try
            {
                return dataProviders.Values.Where(dataProvider =>
                        !string.IsNullOrEmpty(name) &&
                        string.Equals(name, dataProvider.Name, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(name, dataProvider.DisplayName, StringComparison.OrdinalIgnoreCase))
                        .SingleOrDefault();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34800");
            }
        }

        /// <summary>
        /// Initializes the <see cref="DataSource"/>s and <see cref="DataProvider"/>s.
        /// </summary>
        void InitializeSourcesAndProviders()
        {
            // The first time this is called the DataSource items are uninitialized statics so there
            // needs to be a lock so multiple threads are not trying to initialize them.
            lock (lockInitilization)
            {
                this.dataSources = new Dictionary<string, DataSource>();
                this.dataSources.Add(DataSource.SqlDataSource.Name, DataSource.SqlDataSource);
                this.dataSources.Add(DataSource.SqlFileDataSource.Name, DataSource.SqlFileDataSource);
                this.dataSources.Add(DataSource.OracleDataSource.Name, DataSource.OracleDataSource);
                this.dataSources.Add(DataSource.AccessDataSource.Name, DataSource.AccessDataSource);
                this.dataSources.Add(DataSource.OdbcDataSource.Name, DataSource.OdbcDataSource);
                this.dataSources.Add(SqlCe.SqlCeDataSource.Name, SqlCe.SqlCeDataSource);
                this.dataSources.Add(SQLite.SQLiteDataSource.Name, SQLite.SQLiteDataSource);

                this.dataProviders = new Dictionary<string, DataProvider>();
                this.dataProviders.Add(DataProvider.SqlDataProvider.Name, DataProvider.SqlDataProvider);
                this.dataProviders.Add(DataProvider.OracleDataProvider.Name, DataProvider.OracleDataProvider);
                this.dataProviders.Add(DataProvider.OleDBDataProvider.Name, DataProvider.OleDBDataProvider);
                this.dataProviders.Add(DataProvider.OdbcDataProvider.Name, DataProvider.OdbcDataProvider);
                this.dataProviders.Add(SqlCe.SqlCeDataProvider.Name, SqlCe.SqlCeDataProvider);
                this.dataProviders.Add(SQLite.SQLiteDataProvider.Name, SQLite.SQLiteDataProvider);
            }
        }
	}
}
