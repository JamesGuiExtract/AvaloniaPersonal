using System;
using System.IO;
using System.Data.SQLite;

namespace Microsoft.Data.ConnectionUI
{

	public class SQLiteConnectionProperties : AdoDotNetConnectionProperties
	{
		public SQLiteConnectionProperties()
			: base("System.Data.SqlClient")
		{
		}

		public override void Reset()
		{
			base.Reset();
		}

		public override bool IsComplete
		{
			get
			{

				string dataSource = this["Data Source"] as string;

				if (String.IsNullOrEmpty(dataSource))
				{
					return false;
				}

				return true;
			}
		}

		protected override string ToTestString()
		{
			bool savedPooling = (bool)ConnectionStringBuilder["Pooling"];
			bool wasDefault = !ConnectionStringBuilder.ShouldSerialize("Pooling");
			ConnectionStringBuilder["Pooling"] = false;
			string testString = ConnectionStringBuilder.ConnectionString;
			ConnectionStringBuilder["Pooling"] = savedPooling;
			if (wasDefault)
			{
				ConnectionStringBuilder.Remove("Pooling");
			}
			return testString;
		}

		public override void Test()
		{
			using var connection = new SQLiteConnection();

			// Without "FailIfMissing=True", if data source is set to a non-existing file it will
			// create that file. This dialog is intented to validate a connection to an existing database.
			connection.ConnectionString = ToFullString() + ";FailIfMissing=True";
			connection.Open();

			// SQLiteConnection seems to allow connections to be openend on any type of file.
			// Errors are only thrown once a query is attempted against the specified DB.

			try
			{
				using var command = connection.CreateCommand();
				command.CommandText = "pragma schema_version";

				using var reader = command.ExecuteReader();
				
				// Value of 0 can represent an empty file. For purposes of test, look for a DB
				// that has been populated with at least some kind of schema element.
				if (!reader.Read() || reader.GetInt32(0) <= 0)
				{
					throw new Exception("No schema version found");
				}
			}
			catch (Exception ex)
			{
				// Exception messages caught from SQLite here from can be poorly formatted;
				// ensure tidy message.
				throw new Exception("Not a valid SQLite database", ex);
			}
		}
	}
}

