#pragma once

#include "FAMUtils.h"
#include "SqlApplicationRole.h"
#include "UCLIDException.h"
#include <string>
#include <memory>

namespace FAMUtils
{
	enum class AppRole
	{
		kNoRole,
		kExtractRole,
		kReportingRole
	};
}

class FAMUTILS_API CppBaseApplicationRoleConnection
{
public:

	CppBaseApplicationRoleConnection(ADODB::_ConnectionPtr ipConnection, long nDBHash = 0);
	CppBaseApplicationRoleConnection(std::string server, std::string database, long nDBHash = 0, bool enlist = true);
	CppBaseApplicationRoleConnection(std::string connectionString, long nDBHash = 0);
	CppBaseApplicationRoleConnection() {}

	virtual void AssignRole(long nDBHash = 0) = 0;
	void AssignRoleToConnection(ADODB::_ConnectionPtr ipConnection, long nDBHash = 0);

	virtual FAMUtils::AppRole ActiveRole() const = 0;

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

	ADODB::_ConnectionPtr ADOConnection() const { return m_ipConnection; };

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
	virtual FAMUtils::AppRole ActiveRole() const;
};

class FAMUTILS_API ExtractRoleConnection final : public CppBaseApplicationRoleConnection 
{
public:
	ExtractRoleConnection(ADODB::_ConnectionPtr ipConnection, long nDBHash);
	ExtractRoleConnection(std::string server, std::string database, long nDBHash, bool enlist = true);
	ExtractRoleConnection(std::string connectionString, long nDBHash);
	ExtractRoleConnection() : CppBaseApplicationRoleConnection() {};

	virtual void AssignRole(long nDBHash);
	virtual FAMUtils::AppRole ActiveRole() const;
};

class FAMUTILS_API ReportingRoleConnection final : public CppBaseApplicationRoleConnection
{
public:
	ReportingRoleConnection(ADODB::_ConnectionPtr ipConnection, long nDBHash);
	ReportingRoleConnection(std::string server, std::string database, long nDBHash, bool enlist = true);
	ReportingRoleConnection(std::string connectionString, long nDBHash);
	ReportingRoleConnection() : CppBaseApplicationRoleConnection() {};

	virtual void AssignRole(long nDBHash);
	virtual FAMUtils::AppRole ActiveRole() const;
};
