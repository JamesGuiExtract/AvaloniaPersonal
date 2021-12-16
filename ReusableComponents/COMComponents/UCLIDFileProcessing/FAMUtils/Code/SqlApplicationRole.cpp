#include "stdafx.h"
#include "SqlApplicationRole.h"
#include "ADOUtils.h"
#include <UCLIDException.h>
#include <ByteStreamManipulator.h>
#include <EncryptionEngine.h>

#include <string>

using namespace std;

unsigned long ENCRYPTED_PASSWORD_LEN = 16;

//--------------------------------------------------------------------------------------------------
// FILE-SCOPE FUNCTIONS
//--------------------------------------------------------------------------------------------------
// NOTE: This function is purposely not exposed at header file level as a class
//		 method, as no user of this class needs to know that such a function
//		 exists.
// Calculates the password for a FAMDB application role via the following algorithm that matches the algorithm
// used in SqlAppRoleConnection.cs SetRolePassword method:
// 1) Obtain the hash component of a FAMDB's DatabaseID field (part of the encrypted DBInfo table value)
// 2) Create hash that combines the role name with the DB hash by interpreting each successive 4-byte
//    chuck of the name as an int value and summing these together along with the DB hash
// 3) Encrypt the resulting hash using the encryption algorithm and password from UCLIDException.
// 4) Add a fixed suffix with a special char, digit, lowercase letter, uppercase letter to prevent it
//    from being rejected as not sufficiently complex.
const std::string getRolePassword(string strRoleName, long hash)
{
	long nRoleHash = hash;
	for (size_t i = 0; i < strRoleName.length(); i += 4)
	{
		string strSegment = strRoleName.substr(i, 4);
		char pszTemp[4] = { 0 };
		memcpy(pszTemp, strSegment.c_str(), strSegment.length());
		nRoleHash += *(long *)pszTemp;
	}

	char pszHashAsString[9] = { 0 };
	sprintf_s(pszHashAsString, 9, "%08lX", nRoleHash);
	unsigned long nLength = 0;
	
	unsigned char* pszEncrypted = externManipulator(pszHashAsString, &nLength);

	// Free memory when this goes out of scope
	std::shared_ptr<void> deleteAllocatedMemory(__nullptr, [&](void*) {
		ZeroMemory(pszEncrypted, nLength); // Clear the password bytes when done to prevent it from living in process memory.
		CoTaskMemFree(pszEncrypted);
		});

	ByteStream encryptedBS(pszEncrypted, nLength);

	// Allocate 4 bytes for password suffix
	unsigned long nBufferSize = ENCRYPTED_PASSWORD_LEN + 4;
	std::vector<char> encryptedHex(nBufferSize, 0);
	encryptedBS.copyToCharVector(encryptedHex);

	// Suffix for password complexity
	encryptedHex[ENCRYPTED_PASSWORD_LEN] = '.';
	encryptedHex[ENCRYPTED_PASSWORD_LEN + 1] = '9';
	encryptedHex[ENCRYPTED_PASSWORD_LEN + 2] = 'f';
	encryptedHex[ENCRYPTED_PASSWORD_LEN + 3] = 'F';

	std::string encryptedHexString(encryptedHex.begin(), encryptedHex.end());

	// Clear the vector to prevent it from living in process memory.
	std::fill(encryptedHex.begin(), encryptedHex.end(), 0);

	return encryptedHexString;
}

variant_t SetApplicationRole(_ConnectionPtr ipConnection, std::string applicationRoleName, long hash, string testPassword = string())
{
	std::string password;
	_CommandPtr cmd = __nullptr;

	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI51761", ipConnection->State != adStateClosed);

			cmd.CreateInstance(__uuidof(Command));
			ASSERT_RESOURCE_ALLOCATION("ELI51762", cmd != __nullptr);

			cmd->ActiveConnection = ipConnection;
			cmd->CommandText = _bstr_t("sys.sp_setapprole");
			cmd->CommandType = adCmdStoredProc;
			cmd->Parameters->Refresh();
			if (cmd->Parameters->Count <= 4)
			{
				UCLIDException ue("ELI51799", "Unable to get paramters to set app role.");
				ue.addDebugInfo("Count", cmd->Parameters->Count);
				throw ue;
			}
			cmd->Parameters->Item["@rolename"]->Value = applicationRoleName.c_str();
			cmd->Parameters->Item["@encrypt"]->Value = "none";
			cmd->Parameters->Item["@fCreateCookie"]->Value = VARIANT_TRUE;

			password = testPassword.empty()
				? getRolePassword(applicationRoleName, hash)
				: testPassword;

			cmd->Parameters->Item["@password"]->Value = password.data();

			cmd->Execute(NULL, NULL, adCmdStoredProc);
			
			// Clear the password bytes when done to prevent it from living in process memory.
			std::fill(password.begin(), password.end(), 0);
			cmd->Parameters->Item["@password"]->Value.Clear();

			auto cookie = cmd->Parameters->Item["@cookie"];
			if (cookie == __nullptr)
			{
				UCLIDException ue("ELI51790", "Unable to set Application Role.");
				ue.addDebugInfo("Role", applicationRoleName);
				throw ue;
			}
			return cmd->Parameters->Item["@cookie"]->Value;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51759");
	}
	catch (UCLIDException& ue)
	{
		try
		{
			// Clear the password bytes when done to prevent it from living in process memory.
			if (!password.empty())
			{
				std::fill(password.begin(), password.end(), 0);
			}
			if (cmd != __nullptr)
			{
				cmd->Parameters->Item["@password"]->Value.Clear();
			}
		}
		catch (...) {}

		ue.log();
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void UnsetApplicationRole(ADODB::_ConnectionPtr connection, variant_t& cookie)
{
	try
	{
		if (cookie.vt == VT_EMPTY
			|| connection == __nullptr
			|| connection->State == adStateClosed)
		{
			return;
		}

		_CommandPtr cmd;
		cmd.CreateInstance(__uuidof(Command));
		ASSERT_RESOURCE_ALLOCATION("ELI51764", cmd != __nullptr);

		cmd->ActiveConnection = connection;
		cmd->CommandText = _bstr_t("sys.sp_unsetapprole");
		cmd->CommandType = adCmdStoredProc;

		cmd->Parameters->Refresh();

		// If the parameters can't be loaded then the connection is no good.
		// This can happen during unit tests when the DB is dropped before all FileProcessingDB
		// instances are garbage collected
		if (cmd->Parameters->Count == 0)
		{
			return;
		}
		cmd->Parameters->Item["@cookie"]->Value = cookie;
		cmd->Execute(NULL, NULL, adCmdStoredProc);
		cookie.Clear();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51760");
}

CppSqlApplicationRole::CppSqlApplicationRole(ADODB::_ConnectionPtr connection, std::string applicationRoleName, long hash)
{
	m_ipConnection = connection;
	_cookie.Clear();
	if (!applicationRoleName.empty())
		_cookie = SetApplicationRole(m_ipConnection, applicationRoleName, hash);
}
//-------------------------------------------------------------------------------------------------
CppSqlApplicationRole::CppSqlApplicationRole(ADODB::_ConnectionPtr connection, std::string applicationRoleName, string password)
{
	m_ipConnection = connection;
	_cookie.Clear();
	if (!applicationRoleName.empty())
		_cookie = SetApplicationRole(m_ipConnection, applicationRoleName, 0, password);
}
//-------------------------------------------------------------------------------------------------
CppSqlApplicationRole::~CppSqlApplicationRole()
{
	try
	{
		UnsetApplicationRole(m_ipConnection, _cookie);
	}
	catch (...) {}
	m_ipConnection = __nullptr;
}
//-------------------------------------------------------------------------------------------------
void CppSqlApplicationRole::CreateApplicationRole(
	ADODB::_ConnectionPtr ipConnection
	, std::string applicationRoleName
	, long hash
	, AppRoleAccess access
	, string password /*= string()*/)
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
			sql += "CREATE APPLICATION ROLE " + applicationRoleName + " WITH PASSWORD = '";
			sql += password.empty()
				? getRolePassword(applicationRoleName, hash)
				: password;
			sql += "', DEFAULT_SCHEMA = dbo; ";
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
//-------------------------------------------------------------------------------------------------
void CppSqlApplicationRole::UpdateRole(ADODB::_ConnectionPtr ipConnection, std::string applicationRoleName, long hash)
{
	try
	{
		try
		{
			// Parameters are not being used here because the "ALTER APPLICATION ROLE" sql would not accept them.
			executeCmdQuery(ipConnection, 
				"ALTER APPLICATION ROLE " + applicationRoleName + " WITH PASSWORD = '" 
				+ getRolePassword(applicationRoleName, hash) + "'");
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI53017")
	}
	catch (UCLIDException& ue)
	{
		ue.addDebugInfo("ApplicationRole", applicationRoleName);
		ue.log();
		throw;
	}
}
void CppSqlApplicationRole::CreateAllRoles(ADODB::_ConnectionPtr ipConnection, long hash)
{
	CppSqlApplicationRole::CreateApplicationRole(ipConnection, "ExtractRole", hash, CppSqlApplicationRole::AllAccess);
	CppSqlApplicationRole::CreateApplicationRole(ipConnection, "ExtractSecurityRole", hash, CppSqlApplicationRole::SelectExecuteAccess);
}
void CppSqlApplicationRole::UpdateAllRoles(ADODB::_ConnectionPtr ipConnection, long hash)
{
	CppSqlApplicationRole::UpdateRole(ipConnection, "ExtractRole", hash);
	CppSqlApplicationRole::UpdateRole(ipConnection, "ExtractSecurityRole", hash);
}
