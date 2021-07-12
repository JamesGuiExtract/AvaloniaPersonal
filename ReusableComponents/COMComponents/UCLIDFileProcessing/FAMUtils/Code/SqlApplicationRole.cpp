#include "stdafx.h"
#include "SqlApplicationRole.h"
#include "ADOUtils.h"
#include <UCLIDException.h>


void CppSqlApplicationRole::SetApplicationRole(std::string applictionRoleName, std::string password)
{
	try
	{
		ASSERT_ARGUMENT("ELI51761", ipConnection->State != adStateClosed);
		
		_CommandPtr cmd;
		cmd.CreateInstance(__uuidof(Command));
		ASSERT_RESOURCE_ALLOCATION("ELI51762", cmd != __nullptr);

		cmd->ActiveConnection = ipConnection;
		cmd->CommandText = _bstr_t("sys.sp_setapprole");
		cmd->CommandType = adCmdStoredProc;
		cmd->Parameters->Refresh();
		cmd->Parameters->Item["@rolename"]->Value = applictionRoleName.c_str();
		cmd->Parameters->Item["@password"]->Value = password.c_str();
		cmd->Parameters->Item["@encrypt"]->Value = "none";
		cmd->Parameters->Item["@fCreateCookie"]->Value = VARIANT_TRUE;
		
		cmd->Execute(NULL, NULL, adCmdStoredProc);
		_cookie = cmd->Parameters->Item["@cookie"]->Value;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51759");
}
//-------------------------------------------------------------------------------------------------
void CppSqlApplicationRole::UnsetApplicationRole()
{
	try
	{
		if (_cookie.vt == VT_EMPTY) return;
		if (ipConnection == __nullptr || ipConnection->State != adStateOpen) return;

		_CommandPtr cmd;
		cmd.CreateInstance(__uuidof(Command));
		ASSERT_RESOURCE_ALLOCATION("ELI51764", cmd != __nullptr);

		cmd->ActiveConnection = ipConnection;
		cmd->CommandText = _bstr_t("sys.sp_unsetapprole");
		cmd->CommandType = adCmdStoredProc;
		cmd->Parameters->Refresh();

		cmd->Parameters->Item["@cookie"]->Value = _cookie;
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
			string sql = "CREATE APPLICATION ROLE " + applicationRoleName + " WITH PASSWORD = '" + password + "', DEFAULT_SCHEMA = dbo; ";
			if (access > 0)
				sql += "\r\nGRANT SELECT TO " + applicationRoleName +"; ";
			if ((access & AppRoleAccess::InsertAccess) > 0)
				sql += "\r\nGRANT INSERT TO " + applicationRoleName + "; ";
			if ((access & AppRoleAccess::UpdateAccess) > 0)
				sql += "\r\nGRANT UPDATE TO " + applicationRoleName + "; ";
			if ((access & AppRoleAccess::DeleteAccess) > 0)
				sql += "\r\nGRANT DELETE TO " + applicationRoleName + "; ";

			cmd->CommandText = sql.c_str();
			cmd->Execute(NULL, NULL, adCmdText);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51765")
	}
	catch (UCLIDException& ue)
	{
		ue.addDebugInfo("ApplicationRole", applicationRoleName);
		throw;
	}
}
