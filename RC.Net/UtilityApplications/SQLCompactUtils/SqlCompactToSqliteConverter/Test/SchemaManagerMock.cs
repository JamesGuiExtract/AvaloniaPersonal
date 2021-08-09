using System.Collections.Generic;
using System.Threading.Tasks;
using Extract.Database;
using System.Threading;
using System.Data.Common;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.SqlCompactToSqliteConverter.Test
{
    // Simple IDatabaseSchemaManager to be used to test the code that loads the manager from a database.
    // (The code that loads schema managers from a database needs types that match the containing assembly name so Moq won't work)
    public class SchemaManagerMock : IDatabaseSchemaManager
    {
        public bool IsUpdateRequired { get; }

        public bool IsNewerVersion { get; }

        public IEnumerable<object> UIReplacementPlugins { get; }

        public Task<string> BeginUpdateToLatestSchema(IProgressStatus progressStatus, CancellationTokenSource cancelTokenSource)
        {
            throw new System.NotImplementedException();
        }

        public void SetDatabaseConnection(DbConnection connection)
        {
            throw new System.NotImplementedException();
        }
    }
}