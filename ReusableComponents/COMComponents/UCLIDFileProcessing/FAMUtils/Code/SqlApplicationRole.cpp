#include "stdafx.h"
#include "SqlApplicationRole.h"
#include "ADOUtils.h"
#include <UCLIDException.h>
#include <ByteStreamManipulator.h>
#include <EncryptionEngine.h>

const string CppSqlApplicationRole::EXTRACT_ROLE = "ExtractRole";
const string CppSqlApplicationRole::EXTRACT_REPORTING_ROLE = "ExtractReportingRole";
const vector<string> CppSqlApplicationRole::ALL_EXTRACT_ROLES =
{
	EXTRACT_ROLE,
	EXTRACT_REPORTING_ROLE
};

unsigned long ENCRYPTED_PASSWORD_LEN = 16;

static const vector<string> TABLES_KNOWN_TO_HAVE_PHI =
{
	"Attribute",
	"LabDEEncounter",
	"LabDEEncounterFile",
	"LabDEOrder",
	"LabDEOrderFile",
	"LabDEOrderStatus",
	"LabDEPatient",
	"LabDEPatientFile",
	"LabDEProvider"
};

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
				UCLIDException ue("ELI51799", "Unable to get parameters to set app role.");
				ue.addDebugInfo("Count", cmd->Parameters->Count);
				throw ue;
			}
			cmd->Parameters->Item["@rolename"]->Value = applicationRoleName.c_str();
			cmd->Parameters->Item["@encrypt"]->Value = "none";
			cmd->Parameters->Item["@fCreateCookie"]->Value = VARIANT_TRUE;

			password = testPassword.empty()
				? getRolePassword(applicationRoleName, hash)
				: testPassword;

			cmd->Parameters->Item["@password"]->Value = password.c_str();

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

			ue.addDebugInfo("TargetRole", applicationRoleName, true);
			ue.addDebugInfo("CurrentRole", CppSqlApplicationRole::GetAssignedRole(ipConnection), true);
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
	{
		_cookie = SetApplicationRole(m_ipConnection, applicationRoleName, hash);
	}
}
//-------------------------------------------------------------------------------------------------
CppSqlApplicationRole::CppSqlApplicationRole(ADODB::_ConnectionPtr connection, std::string applicationRoleName, string password)
{
	m_ipConnection = connection;
	_cookie.Clear();
	if (!applicationRoleName.empty())
	{
		_cookie = SetApplicationRole(m_ipConnection, applicationRoleName, 0, password);
	}
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
void CppSqlApplicationRole::CreateExtractApplicationRole(
	ADODB::_ConnectionPtr ipConnection
	, std::string applicationRoleName
	, long hash)
{
	AppRoleAccess access = AppRoleAccess::NoAccess;
	vector<string> excludedTables;
	if (applicationRoleName == EXTRACT_ROLE)
	{
		access = CppSqlApplicationRole::ReadWriteAccess;
		excludedTables = {};
	}
	else if (applicationRoleName == EXTRACT_REPORTING_ROLE)
	{
		access = CppSqlApplicationRole::SelectExecuteAccess;
		excludedTables = TABLES_KNOWN_TO_HAVE_PHI;

		// Don't exclude the attribute table even though it is known to contain PHI
		// because the FileDetails_DataCapture dashboard uses it
		const auto& attributeTable = std::find(excludedTables.begin(), excludedTables.end(), "Attribute"); 
		if (attributeTable != excludedTables.end())
		{
			excludedTables.erase(attributeTable);
		}
	}
	else
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI53061");
	}

	string password = getRolePassword(applicationRoleName, hash);
	CreateApplicationRole(ipConnection, applicationRoleName, access, excludedTables, password);
}
//-------------------------------------------------------------------------------------------------
void CppSqlApplicationRole::CreateTestApplicationRole(
	ADODB::_ConnectionPtr ipConnection
	, std::string applicationRoleName
	, AppRoleAccess access
	, vector<string> excludedTables
	, string password)
{
	CreateApplicationRole(ipConnection, applicationRoleName, access, excludedTables, password);
}
//-------------------------------------------------------------------------------------------------
void CppSqlApplicationRole::CreateApplicationRole(
	ADODB::_ConnectionPtr ipConnection
	, std::string applicationRoleName
	, AppRoleAccess access
	, vector<string> excludedTables
	, string& password)
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
			sql += "CREATE APPLICATION ROLE " + applicationRoleName + " WITH PASSWORD = '" + password;
			sql += "', DEFAULT_SCHEMA = dbo; \r\n";

			if (access > 0)
			{
				sql += grantAccessTo("VIEW DEFINITION", applicationRoleName);
				sql += grantAccessTo("EXECUTE", applicationRoleName);
				sql += grantAccessTo(access, applicationRoleName);

				for each (string excludedTable in excludedTables)
				{
					sql += denyAccessTo(access, applicationRoleName, excludedTable);
				}
			}
			sql += "END";

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
void CppSqlApplicationRole::UpdateExtractRole(ADODB::_ConnectionPtr ipConnection, std::string applicationRoleName, long hash)
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
	CppSqlApplicationRole::CreateExtractApplicationRole(ipConnection, EXTRACT_ROLE, hash);
	CppSqlApplicationRole::CreateExtractApplicationRole(ipConnection, EXTRACT_REPORTING_ROLE, hash);
}
void CppSqlApplicationRole::UpdateAllExtractRoles(ADODB::_ConnectionPtr ipConnection, long hash)
{
	CppSqlApplicationRole::UpdateExtractRole(ipConnection, EXTRACT_ROLE, hash);
	CppSqlApplicationRole::UpdateExtractRole(ipConnection, EXTRACT_REPORTING_ROLE, hash);
}
//-------------------------------------------------------------------------------------------------
string CppSqlApplicationRole::GetAssignedRole(ADODB::_ConnectionPtr ipConnection)
{
	_RecordsetPtr ipUserResult(__uuidof(Recordset));
	auto cmd = buildCmd(ipConnection, "SELECT USER_NAME() AS [USER_NAME]", {});
	ipUserResult->Open((IDispatch*)cmd, vtMissing, adOpenStatic, adLockReadOnly, adCmdText);

	if (ipUserResult->adoEOF == VARIANT_FALSE)
	{
		string userName = getStringField(ipUserResult->Fields, "USER_NAME");

		if (find(ALL_EXTRACT_ROLES.begin(), ALL_EXTRACT_ROLES.end(), userName) != ALL_EXTRACT_ROLES.end())
		{
			return userName;
		}
	}

	return "";
}
//-------------------------------------------------------------------------------------------------
vector<string> CppSqlApplicationRole::getAccessTypes(CppSqlApplicationRole::AppRoleAccess access)
{
	vector<string> vecAccess;

	if (access > 0)
		vecAccess.push_back("SELECT");
	if ((access & AppRoleAccess::InsertAccess & ~CppSqlApplicationRole::SelectExecuteAccess) > 0)
		vecAccess.push_back("INSERT");
	if ((access & AppRoleAccess::UpdateAccess & ~CppSqlApplicationRole::SelectExecuteAccess) > 0)
		vecAccess.push_back("UPDATE");
	if ((access & AppRoleAccess::DeleteAccess & ~CppSqlApplicationRole::SelectExecuteAccess) > 0)
		vecAccess.push_back("DELETE");

	return vecAccess;
}
string CppSqlApplicationRole::createCommands(vector<string> vecAccessTypes, string role, string denyToObject/* = string()*/)
{
	string sql;
	for each (string accessType in vecAccessTypes)
	{
		if (denyToObject.empty())
		{
			sql += "GRANT " + accessType + " TO " + role + ";\r\n";
		}
		else
		{
			sql += "DENY " + accessType + " ON OBJECT::" + denyToObject + " TO " + role + ";\r\n";
		}
	}

	return sql;
}
string CppSqlApplicationRole::grantAccessTo(CppSqlApplicationRole::AppRoleAccess access, string role)
{
	vector<string> vecAccessTypes = getAccessTypes(access);
	return createCommands(vecAccessTypes, role);
}
string CppSqlApplicationRole::grantAccessTo(string access, string role)
{
	return createCommands({ access }, role);
}
string CppSqlApplicationRole::denyAccessTo(CppSqlApplicationRole::AppRoleAccess access, string role, string table)
{
	vector<string> vecAccessTypes = getAccessTypes(access);
	return createCommands(vecAccessTypes, role, table);
}