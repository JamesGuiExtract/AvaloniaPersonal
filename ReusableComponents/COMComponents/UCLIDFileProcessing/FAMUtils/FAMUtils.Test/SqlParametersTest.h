#pragma once

#include "stdafx.h"
#include "DatabaseTestingUtils.h"

#include <msclr\marshal_cppstd.h>
#include <string>
#include <cpputil.h>
#include <fstream>

using namespace Extract::Utilities;
using namespace NUnit::Framework;
using namespace msclr::interop;

namespace FAMUtils
{
	namespace Test
	{
		namespace
		{
			// Map a vector of bools into a vector of strings
			std::vector<std::string> getParamValues(const std::vector<bool>& instances, const std::string& param1, const std::string& param2)
			{
				std::vector<std::string> values;
				std::transform(instances.begin(), instances.end(), std::back_inserter(values),
					[param1, param2](bool isParam1) -> std::string { return isParam1 ? param1 : param2; });
				return values;
			}
		}

		const std::string databaseName = "Test_FAMUtils_SqlParameters";

		[TestFixture, Category("SqlParameters")]
		ref class SqlParametersTest
		{

		private:

			// Use a single database instance for all the tests in this class so that they run quickly
			static IFileProcessingDB* _famDB;
			static ADODB::_Connection* _connection;

			// Open a connection to the _famDB
			static ADODB::_ConnectionPtr getConnection()
			{
				ADODB::_ConnectionPtr connection;
				connection.CreateInstance(__uuidof(Connection));

				std::string connectionString = createConnectionString(std::string(_famDB->DatabaseServer), std::string(_famDB->DatabaseName));
				connection->Open(connectionString.data(), "", "", ADODB::adConnectUnspecified);

				return connection;
			}

			// (re)create the specified table
			static void createTable(ADODB::_ConnectionPtr connection, const std::string& tableName, const std::string& columnDefinition)
			{
				std::string initQuery =
					"IF OBJECT_ID('dbo." + tableName + "', 'U') IS NOT NULL DROP TABLE dbo." + tableName + "; "
					"CREATE TABLE dbo." + tableName + " (ID int IDENTITY(1,1) PRIMARY KEY, " + columnDefinition + ") ";
				executeCmdQuery(connection, initQuery, false);
			}


		public:

			// ---------------------------------------------------------------------------------------------------------
			// Create the environment for all the tests in this class to use
			[OneTimeSetUp]
			static void Setup()
			{
				Extract::Testing::Utilities::GeneralMethods::TestSetup();
				_famDB = DatabaseTestingUtils::CreateDB(databaseName).Detach();
				_connection = getConnection().Detach();
			}

			// ---------------------------------------------------------------------------------------------------------
			// Free unmanaged memory and remove the database after all tests have run
			[OneTimeTearDown]
			static void FinalCleanup()
			{
				if (_connection)
				{
					_connection->Release();
				}

				if (_famDB)
				{
					_famDB->Release();
				}

				DatabaseTestingUtils::RemoveDB(databaseName);
			}

			// ---------------------------------------------------------------------------------------------------------
			// Confirm that parameters can be used multiple times in a query
			[Test, Category("Automated"), Category("Cpp")]
			static void SqlParameters_MultipleInstances(
				[Values] bool instance1,
				[Values] bool instance2,
				[Values] bool instance3,
				[Values] bool instance4,
				[Values] bool instance5)
			{
				// Arrange
				ADODB::_ConnectionPtr connection(_connection);

				std::string tableName = "MultipleInstancesTest";
				std::string columnType = "varchar(50)";
				createTable(connection, tableName, "Col1 " + columnType + ", Col2 " + columnType);

				// Setup a query to calculate a result that depends on the permutation of parameters
				std::string insertQuery = "INSERT INTO dbo." + tableName + " VALUES ('V1', 'O1'), ('V2', 'O2')";
				executeCmd(buildCmd(connection, insertQuery, {}));

				std::vector<bool> isParam1 = { instance1, instance2, instance3, instance4 };
				std::vector<std::string> instances = getParamValues(isParam1, "@P", "@PP");
				std::string resultClause = "CAST(ID AS varchar(50)) + Col2 + " + asString(instances, false, " + ");
				std::string selectQuery = "SELECT (" + resultClause + ") AS Result FROM dbo." + tableName + " WHERE Col1 = " + (instance5 ? "@P" : "@PP");

				// Act
				_variant_t result;
				executeCmd(buildCmd(connection, selectQuery, { { "@P", "V1"}, {"@PP", "V2"}}), false, false, "Result", &result);

				// Assert

				// Do the same calculation as the resultClause
				std::vector<std::string> paramValues = getParamValues(isParam1, "V1", "V2");
				int id = instance5 ? 1 : 2;
				string col2 = instance5 ? "O1" : "O2";
				std::string expectedResult = asString(id) + col2 + paramValues[0] + paramValues[1] + paramValues[2] + paramValues[3];

				Assert::AreEqual(gcnew System::String(expectedResult.data()), gcnew System::String(result.bstrVal));
			}

			// ---------------------------------------------------------------------------------------------------------
			// Confirm that the value selected from a database matches the value of a string parameter used to insert the value
			// No special handling of "NULL" is implemented
			[Test, Category("Automated"), Category("Cpp")]
			static void SqlParameters_String([Values("", "123", "NULL", "\"NULL\"")] System::String^ value)
			{
				// Arrange
				ADODB::_ConnectionPtr connection(_connection);

				std::string tableName = "StringTest";
				createTable(connection, tableName, "Value nvarchar(50)");

				// Act
				std::string insertQuery = "INSERT INTO dbo." + tableName + " VALUES (@Value)";
				executeCmd(buildCmd(connection, insertQuery, { { "@Value", _bstr_t(marshal_as<std::string>(value).data()) } }));

				// Assert
				std::string selectQuery = "SELECT Value FROM dbo." + tableName;
				_variant_t result;
				executeCmd(buildCmd(connection, selectQuery, {}), false, false, "Value", &result);

				Assert::AreEqual((VARTYPE)VT_BSTR, result.vt);
				Assert::AreEqual(value, gcnew System::String(result.bstrVal));
			}

			// ---------------------------------------------------------------------------------------------------------
			// Confirm that the value selected from a database matches the value of an int parameter used to insert the value
			[Test, Category("Automated"), Category("Cpp")]
			static void SqlParameters_Int([Values(2147483647, 15, -10)] int value)
			{
				// Arrange
				ADODB::_ConnectionPtr connection(_connection);

				std::string tableName = "IntTest";
				createTable(connection, tableName, "Value int");

				// Act
				std::string insertQuery = "INSERT INTO dbo." + tableName + " VALUES (@Nums)";
				executeCmd(buildCmd(connection, insertQuery, { { "@Nums", _variant_t(value) } }));

				// Assert
				std::string selectQuery = "SELECT Value FROM dbo." + tableName;
				_variant_t result;
				executeCmd(buildCmd(connection, selectQuery, {}), false, false, "Value", &result);

				Assert::AreEqual((VARTYPE)VT_I4, result.vt);
				Assert::AreEqual(value, result.intVal);
			}

			// ---------------------------------------------------------------------------------------------------------
			// Confirm that the value selected from a database matches the value of a 64-bit int parameter used to insert the value
			[Test, Category("Automated"), Category("Cpp")]
			static void SqlParameters_BigInt([Values(9223372036854775807, 2, -9223372036854775807)] long long value)
			{
				// Arrange
				ADODB::_ConnectionPtr connection(_connection);

				std::string tableName = "BigIntTest";
				createTable(connection, tableName, "Value bigint");

				// Act
				std::string insertQuery = "INSERT INTO dbo." + tableName + " VALUES (@Huge)";
				executeCmd(buildCmd(connection, insertQuery, { { "@Huge", _variant_t(value) } }));

				// Assert
				std::string selectQuery = "SELECT Value FROM dbo." + tableName;
				_variant_t result;
				executeCmd(buildCmd(connection, selectQuery, {}), false, false, "Value", &result);

				// idk why the vt is VT_DECIMAL instead of VT_I8 but it doesn't seem to be a problem
				Assert::AreEqual((VARTYPE)VT_DECIMAL, result.vt);
				Assert::AreEqual(value, result.operator long long());
			}

			// ---------------------------------------------------------------------------------------------------------
			// Confirm that the value selected from a database matches the value of a double parameter used to insert the value
			[Test, Category("Automated"), Category("Cpp")]
			static void SqlParameters_Double([Values(1.3e+10, 7.0e-2, -1.0e-5, -1.0e+5)] double value)
			{
				// Arrange
				ADODB::_ConnectionPtr connection(_connection);

				std::string tableName = "DoubleTest";
				createTable(connection, tableName, "Value float");

				// Act
				std::string insertQuery = "INSERT INTO dbo." + tableName + " VALUES (@Value)";
				executeCmd(buildCmd(connection, insertQuery, { { "@Value", _variant_t(value) } }));

				// Assert
				std::string selectQuery = "SELECT Value FROM dbo." + tableName;
				_variant_t result;
				executeCmd(buildCmd(connection, selectQuery, {}), false, false, "Value", &result);

				Assert::AreEqual((VARTYPE)VT_R8, result.vt);
				Assert::AreEqual(value, result.operator double());
			}

			// ---------------------------------------------------------------------------------------------------------
			// Confirm that the value selected from a database matches the value of a bool parameter used to insert the value
			// Confirm that null values can be passed as vtMissing and retrieved as VT_NULL
			[Test, Category("Automated"), Category("Cpp")]
			static void SqlParameters_Bool([Values] System::Nullable<bool> value)
			{
				// Arrange
				ADODB::_ConnectionPtr connection(_connection);

				std::string tableName = "BoolTest";
				createTable(connection, tableName, "Value bit");

				// Act
				std::string insertQuery = "INSERT INTO dbo." + tableName + " VALUES (@IsTrue)";
				executeCmd(buildCmd(connection, insertQuery, { { "@IsTrue", value.HasValue ? _variant_t(value.Value) : vtMissing }}));

				// Assert
				std::string selectQuery = "SELECT Value FROM dbo." + tableName;
				_variant_t result;
				executeCmd(buildCmd(connection, selectQuery, {}), false, false, "Value", &result);

				System::Nullable<bool> actualValue;
				if (result.vt == VT_BOOL)
				{
					actualValue = result.boolVal;
				}
				else if (result.vt != VT_NULL)
				{
					Assert::Fail("Unexpected result (expecting VT_BOOL or VT_NULL)");
				}

				Assert::AreEqual(value, actualValue);
			}

			// ---------------------------------------------------------------------------------------------------------
			// Confirm that the values selected from a database match the values of a string list parameter used to insert the values
			// There is special handling of "NULL" (with/without quotes)
			[Test, Category("Automated"), Category("Cpp")]
			static void SqlParameters_StringList(
				[Values("", "12345", "Blah Blah", "NULL", "\"NULL\"")] System::String^ value1,
				[Values("", "12345", "Blah Blah", "NULL", "\"NULL\"")] System::String^ value2)
			{
				// Arrange
				ADODB::_ConnectionPtr connection(_connection);

				std::string tableName = "StringListTest";
				createTable(connection, tableName, "SmallValue nvarchar(50), LargeValue nvarchar(max)");

				std::string parameterSpec = "@|<VT_BSTR>SomeRandomThings|";
				std::string insertQuery = "INSERT INTO dbo." + tableName + " (SmallValue, LargeValue) VALUES(" + parameterSpec + ")";
				std::string valuesToInsert = marshal_as<std::string>(value1 + ", " + value2);

				// Act
				executeCmd(buildCmd(connection, insertQuery, { { parameterSpec, _bstr_t(valuesToInsert.data()) } }));

				// Assert
				value1 = value1 == "\"NULL\"" ? "NULL" : value1 == "NULL" ? "Actual_NULL" : value1;
				value2 = value2 == "\"NULL\"" ? "NULL" : value2 == "NULL" ? "Actual_NULL" : value2;
				System::String^ expectedResult = value1 + ", " + value2;

				std::string selectQuery =
					"SELECT COALESCE(SmallValue, 'Actual_NULL') + "
					"', ' + COALESCE(LargeValue, 'Actual_NULL') AS Result FROM dbo." + tableName;
				_variant_t result;
				executeCmd(buildCmd(connection, selectQuery, {}), false, false, "Result", &result);

				Assert::AreEqual(expectedResult, gcnew System::String(result.bstrVal));
			}

			// ---------------------------------------------------------------------------------------------------------
			// Confirm that a list of size one works
			[Test, Category("Automated"), Category("Cpp")]
			static void SqlParameters_SingleItemList()
			{
				// Arrange
				ADODB::_ConnectionPtr connection(_connection);

				std::string tableName = "SingleItemListTest";
				createTable(connection, tableName, "Value nvarchar(50)");

				std::string parameterSpec = "@|<VT_BSTR>Values|";
				std::string insertQuery = "INSERT INTO dbo." + tableName + " VALUES(" + parameterSpec + ")";

				// Act
				executeCmd(buildCmd(connection, insertQuery, { { parameterSpec, "Expected Value" }}));

				// Assert
				std::string selectQuery = "SELECT Value FROM dbo." + tableName;
				_variant_t result;
				executeCmd(buildCmd(connection, selectQuery, {}), false, false, "Value", &result);

				Assert::AreEqual("Expected Value", gcnew System::String(result.bstrVal));
			}

			// ---------------------------------------------------------------------------------------------------------
			// Confirm that the values selected from a database match the values of an int list parameter used to insert the values
			// There is special handling of "NULL" (without quotes)
			[Test, Category("Automated"), Category("Cpp"), Pairwise]
			static void SqlParameters_IntList(
				[Values("VT_INT", "VT_I4")] System::String^ typeName,
				[Values("12345", "-45", "NULL")] System::String^ value1,
				[Values("67890", "-3", "NULL")] System::String^ value2)
			{
				// Arrange
				ADODB::_ConnectionPtr connection(_connection);

				std::string tableName = "IntListTest";
				std::string columnType = "int";
				createTable(connection, tableName, "Value1 " + columnType + ", Value2 " + columnType);

				std::string parameterSpec = marshal_as<std::string>("@|<" + typeName + ">Values|");
				std::string insertQuery = "INSERT INTO dbo." + tableName + "(Value1, Value2) VALUES(" + parameterSpec + ")";
				std::string valuesToInsert = marshal_as<std::string>(value1 + ", " + value2);

				// Act
				executeCmd(buildCmd(connection, insertQuery, { { parameterSpec, _bstr_t(valuesToInsert.data()) } }));

				// Assert
				System::String^ expectedResult = value1 + ", " + value2;

				std::string selectQuery =
					"SELECT COALESCE(CAST(Value1 AS varchar(50)), 'NULL') + "
					"', ' + COALESCE(CAST(Value2 AS varchar(50)), 'NULL') AS Result FROM dbo." + tableName;
				_variant_t result;
				executeCmd(buildCmd(connection, selectQuery, {}), false, false, "Result", &result);

				Assert::AreEqual(expectedResult, gcnew System::String(result.bstrVal));
			}

			// ---------------------------------------------------------------------------------------------------------
			// Confirm that the values selected from a database match the values of a 64-bit int list parameter used to insert the values
			// There is special handling of "NULL" (without quotes)
			[Test, Category("Automated"), Category("Cpp")]
			static void SqlParameters_BigIntList(
				[Values("9223372036854775807", "0", "NULL")] System::String^ value1,
				[Values("58", "-9223372036854775807", "NULL")] System::String^ value2)
			{
				// Arrange
				ADODB::_ConnectionPtr connection(_connection);

				std::string tableName = "BigIntListTest";
				std::string columnType = "bigint";
				createTable(connection, tableName, "Value1 " + columnType + ", Value2 " + columnType);

				std::string parameterSpec = "@|<VT_I8>Values|";
				std::string insertQuery = "INSERT INTO dbo." + tableName + "(Value1, Value2) VALUES(" + parameterSpec + ")";
				std::string valuesToInsert = marshal_as<std::string>(value1 + ", " + value2);

				// Act
				executeCmd(buildCmd(connection, insertQuery, { { parameterSpec, _bstr_t(valuesToInsert.data()) } }));

				// Assert
				System::String^ expectedResult = value1 + ", " + value2;

				std::string selectQuery =
					"SELECT COALESCE(CAST(Value1 AS varchar(50)), 'NULL') + "
					"', ' + COALESCE(CAST(Value2 AS varchar(50)), 'NULL') AS Result FROM dbo." + tableName;
				_variant_t result;
				executeCmd(buildCmd(connection, selectQuery, {}), false, false, "Result", &result);

				Assert::AreEqual(expectedResult, gcnew System::String(result.bstrVal));
			}

			// ---------------------------------------------------------------------------------------------------------
			// Confirm that the values selected from a database match the values of a double list parameter used to insert the values
			// There is special handling of "NULL" (without quotes)
			[Test, Category("Automated"), Category("Cpp")]
			static void SqlParameters_DoubleList(
				[Values("1.0000000e+005", "0.0000000e+000", "NULL")] System::String^ value1,
				[Values("-1.0000000e-005", "-1.0000000e+005", "NULL")] System::String^ value2)
			{
				// Arrange
				ADODB::_ConnectionPtr connection(_connection);

				std::string tableName = "DoubleListTest";
				std::string columnType = "float";
				createTable(connection, tableName, "Value1 " + columnType + ", Value2 " + columnType);

				std::string parameterSpec = "@|<VT_R8>Values|";
				std::string insertQuery = "INSERT INTO dbo." + tableName + "(Value1, Value2) VALUES(" + parameterSpec + ")";
				std::string valuesToInsert = marshal_as<std::string>(value1 + ", " + value2);

				// Act
				executeCmd(buildCmd(connection, insertQuery, { { parameterSpec, _bstr_t(valuesToInsert.data()) } }));

				// Assert
				System::String^ expectedResult = value1 + ", " + value2;

				std::string selectQuery =
					"SELECT COALESCE(CONVERT(varchar(50), Value1, 1), 'NULL') + "
					"', ' + COALESCE(CONVERT(varchar(50), Value2, 1), 'NULL') AS Result FROM dbo." + tableName;
				_variant_t result;
				executeCmd(buildCmd(connection, selectQuery, {}), false, false, "Result", &result);

				Assert::AreEqual(expectedResult, gcnew System::String(result.bstrVal));
			}

			// ---------------------------------------------------------------------------------------------------------
			// Confirm that the values selected from a database match the values of a bool list parameter used to insert the values
			// There is special handling of "NULL" (without quotes)
			[Test, Category("Automated"), Category("Cpp"), Pairwise]
			static void SqlParameters_BoolList(
				[Values(",", ", ")] System::String^ separator, // Test that the values are parsed correctly with/without a space after the comma
				[Values("true", "false", "NULL")] System::String^ value1,
				[Values("true", "false", "NULL")] System::String^ value2)
			{
				// Arrange
				ADODB::_ConnectionPtr connection(_connection);
				std::string tableName = "BoolListTest";
				std::string columnType = "bit";
				createTable(connection, tableName, "Value1 " + columnType + ", Value2 " + columnType);

				std::string parameterSpec = "@|<VT_BOOL>MyBools|";
				std::string insertQuery = "INSERT INTO dbo." + tableName + "(Value1, Value2) VALUES(" + parameterSpec + ")";
				std::string valuesToInsert = marshal_as<std::string>(value1 + separator + value2);

				// Act
				executeCmd(buildCmd(connection, insertQuery, { { parameterSpec, _bstr_t(valuesToInsert.data()) } }));

				// Assert
				System::String^ expectedResult = value1 + ", " + value2;

				std::string selectQuery =
					"SELECT (CASE WHEN Value1 = 0 THEN 'false' WHEN Value1 = 1 THEN 'true' ELSE 'NULL' END) + "
					"', ' + (CASE WHEN Value2 = 0 THEN 'false' WHEN Value2 = 1 THEN 'true' ELSE 'NULL' END) AS Result FROM dbo." + tableName;
				_variant_t result;
				executeCmd(buildCmd(connection, selectQuery, {}), false, false, "Result", &result);

				Assert::AreEqual(expectedResult, gcnew System::String(result.bstrVal));
			}
		};
	}
}
