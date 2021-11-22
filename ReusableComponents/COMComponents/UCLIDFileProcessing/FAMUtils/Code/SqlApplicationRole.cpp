#include "stdafx.h"
#include "SqlApplicationRole.h"
#include "ADOUtils.h"
#include <UCLIDException.h>


void CppSqlApplicationRole::SetApplicationRole(std::string applicationRoleName, std::string password)
{
	try
	{
		ASSERT_ARGUMENT("ELI51761", m_ipConnection->State != adStateClosed);

		_CommandPtr cmd;
		cmd.CreateInstance(__uuidof(Command));
		ASSERT_RESOURCE_ALLOCATION("ELI51762", cmd != __nullptr);

		cmd->ActiveConnection = m_ipConnection;
		cmd->CommandText = _bstr_t("sys.sp_setapprole");
		cmd->CommandType = adCmdStoredProc;
		cmd->Parameters->Refresh();
		if (cmd->Parameters->Count <= 4 )
		{
			UCLIDException ue("ELI51799", "Unable to get paramters to set app role.");
			ue.addDebugInfo("Count", cmd->Parameters->Count);
			throw ue;
		}
		cmd->Parameters->Item["@rolename"]->Value = applicationRoleName.c_str();
		cmd->Parameters->Item["@password"]->Value = password.c_str();
		cmd->Parameters->Item["@encrypt"]->Value = "none";
		cmd->Parameters->Item["@fCreateCookie"]->Value = VARIANT_TRUE;

		cmd->Execute(NULL, NULL, adCmdStoredProc);
		auto cookie = cmd->Parameters->Item["@cookie"];
		if (cookie == __nullptr)
		{
			UCLIDException ue("ELI51790", "Unable to set Application Role.");
			ue.addDebugInfo("Role", applicationRoleName);
			throw ue;
		}
		_cookie = cmd->Parameters->Item["@cookie"]->Value;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51759");
}
//-------------------------------------------------------------------------------------------------
void CppSqlApplicationRole::UnsetApplicationRole()
{
	try
	{
		if (_cookie.vt == VT_EMPTY
			|| m_ipConnection == __nullptr
			|| m_ipConnection->State == adStateClosed)
		{
			return;
		}

		_CommandPtr cmd;
		cmd.CreateInstance(__uuidof(Command));
		ASSERT_RESOURCE_ALLOCATION("ELI51764", cmd != __nullptr);

		cmd->ActiveConnection = m_ipConnection;
		cmd->CommandText = _bstr_t("sys.sp_unsetapprole");
		cmd->CommandType = adCmdStoredProc;

		cmd->Parameters->Refresh();

		// If the command doesn't have a cookie parameter then
		// it won't work. This happens, e.g., during unit test teardown,
		// when the call to close all connections is run (FAMTestDBManager.RemoveDatabase)
		if (cmd->Parameters->Count == 0)
		{
			return;
		}
		cmd->Parameters->Item["@cookie"]->Value = _cookie;
		cmd->Execute(NULL, NULL, adCmdStoredProc);
		_cookie.Clear();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51760");
}
//-------------------------------------------------------------------------------------------------
void CppSqlApplicationRole::CreateApplicationRole(
	ADODB::_ConnectionPtr ipConnection
	, std::string applicationRoleName
	, std::string password
	, AppRoleAccess access)
{
	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI51766", ipConnection->State != adStateClosed);

			_CommandPtr cmd;
			cmd.CreateInstance(__uuidof(Command));
			ASSERT_RESOURCE_ALLOCATION("ELI51767", cmd != __nullptr);

			cmd->ActiveConnection = ipConnection;
			
			// Parameters are not being used here because the "CREATE APPLICATION ROLE" sql would not accept them.
			string sql = " IF DATABASE_PRINCIPAL_ID('" + applicationRoleName + "') IS NULL \r\n";
			sql += "BEGIN \r\n";
			sql += "CREATE APPLICATION ROLE " + applicationRoleName + " WITH PASSWORD = '" + password + "', DEFAULT_SCHEMA = dbo; ";
			if (access > 0)
			{
				sql += "\r\nGRANT VIEW DEFINITION TO " + applicationRoleName + "; ";
				sql += "\r\nGRANT EXECUTE TO " + applicationRoleName + "; ";
				sql += "\r\nGRANT SELECT TO " + applicationRoleName + "; ";
			}
			if ((access & AppRoleAccess::InsertAccess & ~CppSqlApplicationRole::SelectExecuteAccess) > 0)
				sql += "\r\nGRANT INSERT TO " + applicationRoleName + "; ";
			if ((access & AppRoleAccess::UpdateAccess & ~CppSqlApplicationRole::SelectExecuteAccess) > 0)
				sql += "\r\nGRANT UPDATE TO " + applicationRoleName + "; ";
			if ((access & AppRoleAccess::DeleteAccess & ~CppSqlApplicationRole::SelectExecuteAccess) > 0)
				sql += "\r\nGRANT DELETE TO " + applicationRoleName + "; ";
			if ((access & AppRoleAccess::AlterAccess & ~CppSqlApplicationRole::SelectExecuteAccess) > 0)
			{
				sql += "\r\nGRANT ALTER TO " + applicationRoleName + "; ";
				sql += "\r\nGRANT REFERENCES TO " + applicationRoleName + "; ";
				sql += "\r\nALTER ROLE db_owner ADD MEMBER " + applicationRoleName + "; ";
			}
			sql += " END\r\n";

			cmd->CommandText = sql.c_str();

			executeCmd(cmd);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51765")
	}
	catch (UCLIDException& ue)
	{
		ue.addDebugInfo("ApplicationRole", applicationRoleName);
		ue.log();
		throw;
	}
}
void CppSqlApplicationRole::CreateAllRoles(ADODB::_ConnectionPtr ipConnection)
{
	CppSqlApplicationRole::CreateApplicationRole(ipConnection, "ExtractSecurityRole", "Change2This3Password", CppSqlApplicationRole::SelectExecuteAccess);
	CppSqlApplicationRole::CreateApplicationRole(ipConnection, "ExtractRole", "Change2This3Password", CppSqlApplicationRole::AllAccess);
}
