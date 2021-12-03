// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Naming", "ExtractRolesAnalyzer:Extract roles", Justification = "<Pending>", Scope = "member", Target = "~M:Extract.SqlDatabase.SqlUtil.NewSqlDBConnection(System.String,System.String,System.Boolean)~System.Data.SqlClient.SqlConnection")]
[assembly: SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>", Scope = "member", Target = "~M:Extract.SqlDatabase.AppRoleCommand.#ctor(System.String,Extract.SqlDatabase.SqlAppRoleConnection)")]
[assembly: SuppressMessage("Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "<Pending>", Scope = "member", Target = "~P:Extract.SqlDatabase.AppRoleCommand.CommandText")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Extract exception handling pattern", Scope = "member", Target = "~M:Extract.SqlDatabase.SqlAppRoleConnection.ConnectionPool.Dispose(System.Boolean)")]
