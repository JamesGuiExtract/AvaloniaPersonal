#include "stdafx.h"
#include "CppApplicationRoleConnection.h"
#include "ADOUtils.h"

CppBaseApplicationRoleConnection::CppBaseApplicationRoleConnection(ADODB::_ConnectionPtr ipConnection, long nDBHash)
{
	m_ipConnection = ipConnection;

}

CppBaseApplicationRoleConnection::CppBaseApplicationRoleConnection(std::string server, std::string database, long nDBHash, bool enlist)
{
	m_ipConnection.CreateInstance(__uuidof(ADODB::Connection));

	m_ipConnection->Open(createConnectionString(server, database).c_str(), "", "", adConnectUnspecified);
}

CppBaseApplicationRoleConnection::CppBaseApplicationRoleConnection(std::string connectionString, long nDBHash)
{
	m_ipConnection.CreateInstance(__uuidof(ADODB::Connection));
	m_ipConnection->Open(connectionString.c_str(), "", "", adConnectUnspecified);
}

void CppBaseApplicationRoleConnection::AssignRoleToConnection(ADODB::_ConnectionPtr ipConnection, long nDBHash)
{
	m_ipConnection = ipConnection;
	AssignRole();
}

NoRoleConnection::NoRoleConnection(ADODB::_ConnectionPtr ipConnection)
	: CppBaseApplicationRoleConnection(ipConnection)
{
	AssignRole();
}

NoRoleConnection::NoRoleConnection(std::string server, std::string database, bool enlist)
	: CppBaseApplicationRoleConnection(server, database, enlist)
{
	AssignRole();
}

NoRoleConnection::NoRoleConnection(std::string connectionString)
	: CppBaseApplicationRoleConnection(connectionString)
{
	AssignRole();
}

void NoRoleConnection::AssignRole(long nDBHash)
{
	const std::string Role = "";
	m_ApplicationRole.reset(new CppSqlApplicationRole(m_ipConnection, Role, nDBHash));
}

CppBaseApplicationRoleConnection::AppRoles NoRoleConnection::ActiveRole()
{
	return AppRoles::kNoRole;
}

SecurityRoleConnection::SecurityRoleConnection(ADODB::_ConnectionPtr ipConnection, long nDBHash)
	: CppBaseApplicationRoleConnection(ipConnection)
{
	AssignRole(nDBHash);
}

SecurityRoleConnection::SecurityRoleConnection(std::string server, std::string database, long nDBHash, bool enlist)
	: CppBaseApplicationRoleConnection(server, database, enlist)
{
	AssignRole(nDBHash);
}

SecurityRoleConnection::SecurityRoleConnection(std::string connectionString, long nDBHash)
	: CppBaseApplicationRoleConnection(connectionString)
{
	AssignRole(nDBHash);

}

void SecurityRoleConnection::SecurityRoleConnection::AssignRole(long nDBHash)
{
	const std::string Role = "ExtractSecurityRole";
	m_ApplicationRole.reset(new CppSqlApplicationRole(m_ipConnection, Role, nDBHash));
}

CppBaseApplicationRoleConnection::AppRoles SecurityRoleConnection::ActiveRole()
{
	return AppRoles::kSecurityRole;
}

ExtractRoleConnection::ExtractRoleConnection(ADODB::_ConnectionPtr ipConnection, long nDBHash)
	: CppBaseApplicationRoleConnection(ipConnection)
{
	AssignRole(nDBHash);
}

ExtractRoleConnection::ExtractRoleConnection(std::string server, std::string database, long nDBHash, bool enlist)
	: CppBaseApplicationRoleConnection(server, database, enlist)
{
	AssignRole(nDBHash);
}

ExtractRoleConnection::ExtractRoleConnection(std::string connectionString, long nDBHash)
	: CppBaseApplicationRoleConnection(connectionString)
{
	AssignRole(nDBHash);
}

void ExtractRoleConnection::ExtractRoleConnection::AssignRole(long nDBHash)
{
	const std::string Role = "ExtractRole";
	m_ApplicationRole.reset(new CppSqlApplicationRole(m_ipConnection, Role, nDBHash));
}

CppBaseApplicationRoleConnection::AppRoles ExtractRoleConnection::ActiveRole()
{
	return AppRoles::kExtractRole;
}




