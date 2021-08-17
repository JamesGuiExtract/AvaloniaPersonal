#pragma once

#include "FAMUtils.h"
#include <string>
#include <map>

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
		AlterAccess  = 17,
		AllAccess = SelectExecuteAccess | InsertAccess | UpdateAccess | DeleteAccess | AlterAccess
	} ;	

	/// <summary>
	/// Constructor to enable the given application role of the given connection
	/// </summary>
	/// <param name="connection">Connection to enable the given "applicationRoleName" on</param>
	/// <param name="applicationRoleName">The Application role that is to be enabled on "sqlConnection".</param>
	/// <param name="password">Password for the given application role</param>
	CppSqlApplicationRole(ADODB::_ConnectionPtr connection, std::string applicationRoleName, std::string password)
	{
		ipConnection = connection;
		_cookie.Clear();
		if (!applicationRoleName.empty())
			SetApplicationRole(applicationRoleName, password);
	}
	~CppSqlApplicationRole()
	{
		UnsetApplicationRole();
	}

	/// <summary>
	/// Static method to create and Application role
	/// NOTE: This method will need to be called when running as a user that has the ability to create application roles
	/// </summary>
	/// <param name="ipConnection">Connection to create the <paramref name="applicationRoleName"/> on</param>
	/// <param name="applicationRoleName">The name of the Application role to create</param>
	/// <param name="password">Password that will be used to enable the application role</param>
	/// <param name="access">Access that should be granted for the application role</param>
	static void CreateApplicationRole(ADODB::_ConnectionPtr ipConnection, std::string applicationRoleName, std::string password, AppRoleAccess access);
	static void CreateAllRoles(ADODB::_ConnectionPtr ipConnection);
private:

	ADODB::_ConnectionPtr ipConnection;
	variant_t _cookie;

	void SetApplicationRole(std::string applictionRoleName, std::string password);
	void UnsetApplicationRole();
};

