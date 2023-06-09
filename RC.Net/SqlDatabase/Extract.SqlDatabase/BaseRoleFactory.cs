﻿using System.Data.Common;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Security;
using System.Security.Permissions;

namespace Extract.SqlDatabase
{
    abstract internal class BaseRoleFactory : DbProviderFactory
    {
        protected internal BaseRoleFactory()
        {
        }

        public override bool CanCreateDataSourceEnumerator => true;

        public override DbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new SqlCommandBuilder();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new SqlConnectionStringBuilder();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return new SqlDataAdapter();
        }

        public override DbParameter CreateParameter()
        {
            return new SqlParameter();
        }

        public override CodeAccessPermission CreatePermission(PermissionState state)
        {
            return new SqlClientPermission(state);
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return SqlDataSourceEnumerator.Instance;
        }
    }
}
