#pragma once

#include "FAMUtils.h"
#include "SqlApplicationRole.h"
#include "UCLIDException.h"
#include <string>
#include <memory>


class FAMUTILS_API CppBaseApplicationRoleConnection 
{
public:
	CppBaseApplicationRoleConnection(ADODB::_ConnectionPtr ipConnection);
	CppBaseApplicationRoleConnection(std::string server, std::string database, bool enlist = true);
	CppBaseApplicationRoleConnection(std::string connectionString);
	CppBaseApplicationRoleConnection() {}

	virtual void AssignRole() = 0;
	void AssignRoleToConnection(ADODB::_ConnectionPtr ipConnection);

	~CppBaseApplicationRoleConnection()
	{
		try
		{
			if (m_ipConnection != __nullptr && m_ipConnection->State != ADODB::adStateClosed)
			{
				m_ApplicationRole.reset(__nullptr);
				m_ipConnection->Close();
				m_ipConnection = __nullptr;
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI51770")
	}

	operator ADODB::_ConnectionPtr() const { return m_ipConnection; };
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

	virtual void AssignRole();
};


class FAMUTILS_API SecurityRoleConnection final : public CppBaseApplicationRoleConnection
{
public:
	SecurityRoleConnection(ADODB::_ConnectionPtr ipConnection);
	SecurityRoleConnection(std::string server, std::string database, bool enlist = true);
	SecurityRoleConnection(std::string connectionString);
	SecurityRoleConnection() : CppBaseApplicationRoleConnection() {};

	virtual void AssignRole();
};


class FAMUTILS_API ExtractRoleConnection final : public CppBaseApplicationRoleConnection 
{
public:
	ExtractRoleConnection(ADODB::_ConnectionPtr ipConnection);
	ExtractRoleConnection(std::string server, std::string database, bool enlist = true);
	ExtractRoleConnection(std::string connectionString);
	ExtractRoleConnection() : CppBaseApplicationRoleConnection() {};

	virtual void AssignRole();
};
