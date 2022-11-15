using System;
using System.Collections.Generic;

namespace Microsoft.Data.ConnectionUI
{
	public class SQLite
	{
		public static DataSource SQLiteDataSource
		{
			get
			{
				if (_SQLiteDataSource == null)
				{
					_SQLiteDataSource = new DataSource("SQLiteClient", "SQLite");
					_SQLiteDataSource.Providers.Add(SQLiteDataProvider);
				}
				return _SQLiteDataSource;
			}
		}

		private static DataSource _SQLiteDataSource;

		public static DataProvider SQLiteDataProvider
		{
			get
			{
				if (_SQLiteDataProvider == null)
				{
					Dictionary<string, string> descriptions = new Dictionary<string, string>();
					descriptions.Add(SQLiteDataSource.Name, Resources.DataProvider_SQLite_Description);

					Dictionary<string, Type> uiControls = new Dictionary<string, Type>();
					uiControls.Add(String.Empty, typeof(SQLiteConnectionUIControl));

					_SQLiteDataProvider = new DataProvider(
						"System.Data.SQLite",
						Resources.DataProvider_SQLite,
						"SQLite",
						Resources.DataProvider_SQLite_Description,
						typeof(System.Data.SQLite.SQLiteConnection),
						descriptions,
						uiControls,
						typeof(SQLiteConnectionProperties));
				}
				return _SQLiteDataProvider;
			}
		}
		private static DataProvider _SQLiteDataProvider;
	}
}
