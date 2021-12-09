#pragma once

#include "FAMUtils.h"
#include "SqlApplicationRole.h"
#include "UCLIDException.h"
#include <string>
#include <memory>


class FAMUTILS_API CppBaseApplicationRoleConnection
{
public:
	typedef  enum {
		kNoRole,
		kExtractRole,
		kSecurityRole
	} AppRoles;

	CppBaseApplicationRoleConnection(ADODB::_ConnectionPtr ipConnection, long nDBHash = 0);
	CppBaseApplicationRoleConnection(std::string server, std::string database, long nDBHash = 0, bool enlist = true);
	CppBaseApplicationRoleConnection(std::string connectionString, long nDBHash = 0);
	CppBaseApplicationRoleConnection() {}

	virtual void AssignRole(long nDBHash = 0) = 0;
	void AssignRoleToConnection(ADODB::_ConnectionPtr ipConnection, long nDBHash = 0);

	virtual AppRoles ActiveRole() = 0;

	~CppBaseApplicationRoleConnection()
	{
		try
		{
			if (m_ipConnection != __nullptr && (m_ipConnection->State != ADODB::adStateClosed)) 
			{
				m_ApplicationRole.reset(__nullptr);
				m_ipConnection = __nullptr;
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI51770")
	}

	ADODB::_ConnectionPtr ADOConnection() { return m_ipConnection; };

protected:
	ADODB::_ConnectionPtr m_ipConnection;
	std::unique_ptr<CppSqlApplicationRole> m_ApplicationRole;
};


class FAMUTILS_API NoRoleConnection final : public CppBaseApplicationRoleConnection
{
public:
	NoRoleConnection(ADODB::_ConnectionPtr ipConnection);
	NoRoleConnection(std::string server, std::string database, bool enlist = true);
	NoRoleConnection(std::string connectionString);
	NoRoleConnection() : CppBaseApplicationRoleConnection() {};

	virtual void AssignRole(long nDBHash = 0);
	virtual AppRoles ActiveRole();
};


class FAMUTILS_API SecurityRoleConnection final : public CppBaseApplicationRoleConnection
{
public:
	SecurityRoleConnection(ADODB::_ConnectionPtr ipConnection, long nDBHash);
	SecurityRoleConnection(std::string server, std::string database, long nDBHash, bool enlist = true);
	SecurityRoleConnection(std::string connectionString, long nDBHash);
	SecurityRoleConnection() : CppBaseApplicationRoleConnection() {};

	virtual void AssignRole(long nDBHash);
	virtual AppRoles ActiveRole();
};


class FAMUTILS_API ExtractRoleConnection final : public CppBaseApplicationRoleConnection 
{
public:
	ExtractRoleConnection(ADODB::_ConnectionPtr ipConnection, long nDBHash);
	ExtractRoleConnection(std::string server, std::string database, long nDBHash, bool enlist = true);
	ExtractRoleConnection(std::string connectionString, long nDBHash);
	ExtractRoleConnection() : CppBaseApplicationRoleConnection() {};

	virtual void AssignRole(long nDBHash);
	virtual AppRoles ActiveRole();
};
