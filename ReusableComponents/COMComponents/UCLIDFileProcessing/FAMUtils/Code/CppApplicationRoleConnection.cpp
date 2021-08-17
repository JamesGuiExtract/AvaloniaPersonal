#include "stdafx.h"
#include "CppApplicationRoleConnection.h"
#include "ADOUtils.h"


CppBaseApplicationRoleConnection::CppBaseApplicationRoleConnection(ADODB::_ConnectionPtr ipConnection)
{
	m_ipConnection = ipConnection;

}

CppBaseApplicationRoleConnection::CppBaseApplicationRoleConnection(std::string server, std::string database, bool enlist)
{
	m_ipConnection.CreateInstance(__uuidof(ADODB::Connection));

	m_ipConnection->Open(createConnectionString(server, database).c_str(), "", "", adConnectUnspecified);
}

CppBaseApplicationRoleConnection::CppBaseApplicationRoleConnection(std::string connectionString)
{
	m_ipConnection.CreateInstance(__uuidof(ADODB::Connection));
	m_ipConnection->Open(connectionString.c_str(), "", "", adConnectUnspecified);
}

void CppBaseApplicationRoleConnection::AssignRoleToConnection(ADODB::_ConnectionPtr ipConnection)
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

void NoRoleConnection::AssignRole()
{
	const std::string Role = "";
	const std::string Password = "";
	m_ApplicationRole.reset(new CppSqlApplicationRole(m_ipConnection, Role, Password));
}


SecurityRoleConnection::SecurityRoleConnection(ADODB::_ConnectionPtr ipConnection) 
	: CppBaseApplicationRoleConnection(ipConnection)
{
	AssignRole();
}

SecurityRoleConnection::SecurityRoleConnection(std::string server, std::string database, bool enlist )
	: CppBaseApplicationRoleConnection(server, database, enlist)
{
	AssignRole();
}

SecurityRoleConnection::SecurityRoleConnection(std::string connectionString)
	: CppBaseApplicationRoleConnection(connectionString)
{
	AssignRole();

}

void SecurityRoleConnection::SecurityRoleConnection::AssignRole()
{
	const std::string Role = "ExtractSecurityRole";
	const std::string Password = "Change2This3Password";
	m_ApplicationRole.reset(new CppSqlApplicationRole(m_ipConnection, Role, Password));
}


ExtractRoleConnection::ExtractRoleConnection(ADODB::_ConnectionPtr ipConnection)
	: CppBaseApplicationRoleConnection(ipConnection)
{
	AssignRole();
}

ExtractRoleConnection::ExtractRoleConnection(std::string server, std::string database, bool enlist )
	: CppBaseApplicationRoleConnection(server, database, enlist)
{
	AssignRole();
}

ExtractRoleConnection::ExtractRoleConnection(std::string connectionString)
	: CppBaseApplicationRoleConnection(connectionString)
{
	AssignRole();

}

void ExtractRoleConnection::ExtractRoleConnection::AssignRole()
{
	// TODO: this needs to get the password from the database
	const std::string Role = "ExtractRole";
	const std::string Password = "Change2This3Password";
	m_ApplicationRole.reset(new CppSqlApplicationRole(m_ipConnection, Role, Password));
}






