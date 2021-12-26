#include "stdafx.h"

#include <FAMUtils.h>
#include "CppSqlApplicationRoleTest.h"
#include <SqlApplicationRole.h>
#include <COMUtils.h>
#include <UCLIDException.h>
#include <ADOUtils.h>

#include <string>

#include <msclr\marshal_cppstd.h>
#include "DatabaseTestingUtils.h"


using namespace ADODB;
using namespace msclr::interop;

namespace FAMUtils {
	namespace Test {

		using namespace DatabaseTestingUtils;

		_ConnectionPtr CreateDBWithAppRole(
			std::string dbName
			, std::string approle
			, std::string testValue
			, CppSqlApplicationRole::AppRoleAccess access);

		void checkAccess(CppSqlApplicationRole::AppRoleAccess access, _ConnectionPtr connection, System::String^ description);

		void CppSqlApplicationRoleTest::CreateApplicationRoleTest()
		{
			std::string testDBName = "TestCppCreateApplicationRoleTest";
			try
			{
				_ConnectionPtr connection =
					CreateDBWithAppRole(testDBName, "TestAppRole", "123Test@333", CppSqlApplicationRole::NoAccess);

				string sql = "SELECT Count(name) Roles FROM sys.database_principals p where type_desc = @Description AND name = @RoleName";
				auto cmd = buildCmd(connection, sql, { {"@Description", "APPLICATION_ROLE"}, {"@RoleName", "TestAppRole"} });

				// check that the Application Role exists
				auto result = cmd->Execute(NULL, NULL, adCmdText);
				Assert::IsFalse(result->adoEOF, "Recordset should have records.");
				Assert::AreEqual(1, (int)result->Fields->Item["Roles"]->Value, "TestAppRole should exist");
			}
			finally
			{
				RemoveDB(testDBName);
			}
		}

		void CppSqlApplicationRoleTest::UseApplicationRoleTest(int access, System::String^ testDBName)
		{
			std::string cppTestDBName = marshal_as<std::string>(testDBName);
			try
			{
				std::string appRole = "testAppRole";
				std::string password = "123Test@333";

				_ConnectionPtr connection =
					CreateDBWithAppRole(cppTestDBName, appRole, password, (CppSqlApplicationRole::AppRoleAccess)access);

				// enable the approle
				{
					CppSqlApplicationRole role(connection, appRole, password);
					checkAccess((CppSqlApplicationRole::AppRoleAccess)access, connection, "Access Set");
				}
				checkAccess(CppSqlApplicationRole::AppRoleAccess::ReadWriteAccess, connection, "Restored access");
			}
			finally
			{
				RemoveDB(cppTestDBName);
			}

		}

		bool throwsException(_CommandPtr cmd)
		{
			try
			{
				cmd->Execute(NULL, NULL, adCmdText);
				return false;
			}
			catch (...)
			{
				return true;
			}
		}

		void checkAccess(CppSqlApplicationRole::AppRoleAccess access, _ConnectionPtr connection, System::String^ description)
		{
			_CommandPtr selectDBInfoCmd, selectCmd, insertCmd, updateCmd, deleteCmd;

			map<std::string, variant_t> mapEmpty;
			selectDBInfoCmd = buildCmd(connection, "SELECT Count(ID) c FROM DBINFO", mapEmpty);

			auto selectResults = selectDBInfoCmd->Execute(NULL, NULL, adCmdText);
			Assert::Greater(selectResults->Fields->Item["c"]->Value, 0, description + ": Should always be able to select from DBInfo");

			selectCmd = buildCmd(connection, "SELECT Count(*) c FROM TaskClass", mapEmpty);

			if ((access & CppSqlApplicationRole::SelectExecuteAccess) > 0)
			{
				auto selectResults = selectCmd->Execute(NULL, NULL, adCmdText);
				Assert::Greater(selectResults->Fields->Item["c"]->Value, 0, description + ": Number of records in DBInfo should be > 0");
			}
			else
			{
				Assert::IsTrue(throwsException(selectCmd), description + ": Select statement should throw exception");
			}

			insertCmd = buildCmd(connection, "INSERT INTO DBInfo(Name, Value) VALUES(@Name, '1'); ",
				{ {"@Name", (marshal_as<std::string>(description) + "_TestName_Delete").c_str()} });

			if ((access & CppSqlApplicationRole::InsertAccess & ~CppSqlApplicationRole::SelectExecuteAccess) > 0)
			{
				variant_t records;
				insertCmd->Execute(&records, NULL, adCmdText);
				Assert::AreEqual(1, (int)records), description + ": Insert should add one record.";
				// verify that the data was added to the table


				auto cmd = buildCmd(connection, "SELECT COUNT (Name) c FROM DBInfo WHERE Name = @Name and Value = '1'",
					{ {"@Name", (marshal_as<std::string>(description) + "_TestName_Delete").c_str()} });

				_RecordsetPtr result = cmd->Execute(NULL, NULL, adCmdText);
				Assert::AreEqual(1, (int)result->Fields->Item["c"]->Value, description + ": Should be one record");
			}
			else
			{
				Assert::IsTrue(throwsException(insertCmd), description + ": Insert command should throw Exception");
			}

			updateCmd = buildCmd(connection, "UPDATE DBInfo Set Value = '200' WHERE Name = 'CommandTimeout'", mapEmpty);

			if ((access & CppSqlApplicationRole::UpdateAccess & ~CppSqlApplicationRole::SelectExecuteAccess) > 0)
			{
				variant_t records;
				updateCmd->Execute(&records, NULL, adCmdText);
				Assert::AreEqual(1, (int)records, description + ": Should update 1 record");
			}
			else
			{
				Assert::IsTrue(throwsException(updateCmd), description + ": Update command should throw exception");
			}

			deleteCmd = buildCmd(connection, "DELETE TOP(1) FROM DBInfo", mapEmpty);

			if ((access & CppSqlApplicationRole::DeleteAccess & ~CppSqlApplicationRole::SelectExecuteAccess) > 0)
			{
				variant_t records;
				deleteCmd->Execute(&records, NULL, adCmdText);
				Assert::AreEqual(1, (int)records, description + ": Delete command should delete 1 record.");
			}
			else
			{
				Assert::IsTrue(throwsException(deleteCmd), description + ": Delete command should throw exception");
			}
		}

		_ConnectionPtr CreateDBWithAppRole(
			std::string dbName
			, std::string approle
			, std::string testValue
			, CppSqlApplicationRole::AppRoleAccess access)
		{
			auto famDb = CreateDB(dbName);

			_ConnectionPtr connection(__uuidof(Connection));
			connection->Open(famDb->ConnectionString, "", "", adConnectUnspecified);

			CppSqlApplicationRole::CreateTestApplicationRole(connection, approle, access, {}, testValue);

			return connection;
		}
	}
}
