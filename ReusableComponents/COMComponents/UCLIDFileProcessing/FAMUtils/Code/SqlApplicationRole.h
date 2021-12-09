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

	// Constructor to enable the given application role of the given connection
	// connection:			Connection to enable the given "applicationRoleName" on.
	// applicationRoleName: The Application role that is to be enabled on "sqlConnection".
	// hash:				The hash component of the encrypted DatabaseID used to generate the
	//						password for the given application role.
	// password:			For testing of this class, a password may be directly specified instead of using
	//						a password based on hash.
	CppSqlApplicationRole(ADODB::_ConnectionPtr connection, std::string applicationRoleName, long hash);
	CppSqlApplicationRole(ADODB::_ConnectionPtr connection, std::string applicationRoleName, std::string password);
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
	static void CreateApplicationRole(ADODB::_ConnectionPtr ipConnection, std::string applicationRoleName
		, long hash, AppRoleAccess access, std::string password = std::string());
	static void CreateAllRoles(ADODB::_ConnectionPtr ipConnection, long hash);

	static void UpdateRole(ADODB::_ConnectionPtr ipConnection, std::string applicationRoleName, long hash);
	static void UpdateAllRoles(ADODB::_ConnectionPtr ipConnection, long hash);
private:

	ADODB::_ConnectionPtr m_ipConnection;
	variant_t _cookie;
};

