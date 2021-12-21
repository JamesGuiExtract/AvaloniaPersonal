#pragma once

#include "FAMUtils.h"
#include <string>
#include <vector>

using namespace std;

/// <summary>
/// Class to use to enable an application role on a ADO connection
/// NOTE: This class is a C++ version of the c# class SqlApplicationRole
/// Engineering\RC.Net\SqlDatabase\Extract.SqlDatabase\SqlApplicationRole.cs
/// </summary>
class FAMUTILS_API CppSqlApplicationRole
{
public:
	enum AppRoleAccess
	{
		NoAccess = 0,
		SelectExecuteAccess = 1,
		InsertAccess = 3,
		UpdateAccess = 5,
		DeleteAccess = 9,
		ReadWriteAccess = SelectExecuteAccess | InsertAccess | UpdateAccess | DeleteAccess
	};

	// Constructor to enable the given application role of the given connection
	// connection:			Connection to enable the given "applicationRoleName" on.
	// applicationRoleName: The Application role that is to be enabled on "sqlConnection".
	// hash:				The hash component of the encrypted DatabaseID used to generate the
	//						password for the given application role.
	// password:			For testing of this class, a password may be directly specified instead of using
	//						a password based on hash.
	CppSqlApplicationRole(ADODB::_ConnectionPtr connection, string applicationRoleName, long hash);
	CppSqlApplicationRole(ADODB::_ConnectionPtr connection, string applicationRoleName, string password);
	~CppSqlApplicationRole();

	// Static method to create and Application role
	// NOTE: This method will need to be called when running as a user that has the ability to create application roles
	// connection:			Connection to enable the given "applicationRoleName" on.
	// applicationRoleName: The Application role that is to be enabled on "sqlConnection".
	// hash:				The hash component of the encrypted DatabaseID used to generate the
	//						password for the given application role.
	// password:			For testing of this class, a password may be directly specified instead of using
	//						a password based on hash.
	// access:				Access that should be granted for the application role.
	static void CreateExtractApplicationRole(ADODB::_ConnectionPtr ipConnection, string applicationRoleName, long hash);
	static void CreateTestApplicationRole(ADODB::_ConnectionPtr ipConnection, string applicationRoleName
		, AppRoleAccess access, vector<string> excludedTables, string password);
	static void CreateAllRoles(ADODB::_ConnectionPtr ipConnection, long hash);

	static void UpdateExtractRole(ADODB::_ConnectionPtr ipConnection, string applicationRoleName, long hash);
	static void UpdateAllExtractRoles(ADODB::_ConnectionPtr ipConnection, long hash);

	static const string EXTRACT_ROLE;
	static const string EXTRACT_REPORTING_ROLE;
private:

	ADODB::_ConnectionPtr m_ipConnection;
	variant_t _cookie;

	static void CreateApplicationRole(ADODB::_ConnectionPtr ipConnection, string applicationRoleName
		, AppRoleAccess access, vector<string> excludedTables, string& password);
	static vector<string> getAccessTypes(CppSqlApplicationRole::AppRoleAccess access);
	static string createCommands(vector<string> vecAccessTypes, string role, string denyToObject = string());
	static string grantAccessTo(CppSqlApplicationRole::AppRoleAccess access, string role);
	static string grantAccessTo(string access, string role);
	static string denyAccessTo(CppSqlApplicationRole::AppRoleAccess access, string role, string table);
};

