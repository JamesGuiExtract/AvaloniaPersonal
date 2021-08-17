// ADOUtils.h - Header file for ADO related functions

#pragma once

#include "FAMUtils.h"

#include <StopWatch.h>
#include <CsisUtils.h>
#include <ByteStream.h>

#include <string>
#include <vector>

using namespace ADODB;
using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const string& gstrSERVER = "Server";
static const string& gstrDATABASE = "Database";
static const string& gstrDATA_SOURCE = "Data Source";
static const string& gstrINITIAL_CATALOG = "Initial Catalog";

//-------------------------------------------------------------------------------------------------
// PURPOSE:	 (Based on the FileProcessingDB macro, but generalized for use with any ADO::Connection)
//			 The purpose of this macro is to declare and initialize local variables and define the
//			 beginning of a do...while loop that contains a try...catch block to be used to retry
//			 the block of code between the BEGIN_ADO_CONNECTION_RETRY macro and the
//			 END_ADO_CONNECTION_RETRY macro.  If an exception is thrown within the block of code
//			 between the connection retry macros the connection passed to END_ADO_CONNECTION_RETRY
//			 macro will be tested to see if it is a good connection if it is the caught exception
//			 is rethrown, if it is no longer a good connection a check is made to see the retry
//           count is equal to maximum retries, if not, the exception will be logged if this is
//			 the first retry and the connection will be reinitialized.  If the number of retires is
//			 exceeded the exception will be rethrown.
// REQUIRES: An ADODB::ConnectionPtr variable to be declared before the BEGIN_CONNECTION_RETRY macro
//			 is used so it can be passed to the END_CONNECTION_RETRY macro.
//-------------------------------------------------------------------------------------------------
#define BEGIN_ADO_CONNECTION_RETRY() \
		int nRetryCount = 0; \
		bool bRetryExceptionLogged = false; \
		bool bRetrySuccess = false; \
		do \
		{ \
			try \
			{\
				try\
				{\

//-------------------------------------------------------------------------------------------------
// PURPOSE:	 To define the end of the block of code to be retried. (see above)
// NOTE:	 Especially if using IFileProcessingDB::GetConnectionRetrySettings to get the values,
//			 values for nMaxRetryCount and dRetryTimeout should be obtained before
//			 BEGIN_ADO_CONNECTION_RETRY to avoid repeated COM calls to get the same settings.
#define END_ADO_CONNECTION_RETRY(ipRetryConnection, getAppRoleConnection, nMaxRetryCount, dRetryTimeout, strELICode) \
					bRetrySuccess = true; \
				}\
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION(strELICode)\
			} \
			catch(UCLIDException ue1) \
			{ \
				bool bConnectionIsAlive = false; \
				\
				try \
				{ \
					if (ipRetryConnection != __nullptr) \
					{ \
						getSQLServerDateTime(ipRetryConnection); \
						bConnectionIsAlive = true; \
					} \
				} \
				catch(...){}; \
				if (bConnectionIsAlive || nRetryCount >= nMaxRetryCount) \
				{ \
					throw ue1; \
				}\
				if (!bRetryExceptionLogged) \
				{ \
					UCLIDException uex("ELI29853", \
						"Application trace: Database connection failed. Attempting to reconnect.", ue1); \
					uex.log(); \
					bRetryExceptionLogged = true; \
				} \
				\
				StopWatch sw; \
				sw.start(); \
				while(true) \
				{ \
					try \
					{ \
						try \
						{ \
							/* This will create a new connection for this thread and initialize */ \
							/* the schema */ \
							ipRetryConnection = getAppRoleConnection(true)->ADOConnection(); \
							\
							UCLIDException ueConnected("ELI29854", "Application trace: Connection retry successful."); \
							ueConnected.log(); \
							\
							break; \
						} \
						CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29855"); \
					} \
					catch(UCLIDException ue2) \
					{ \
						if (sw.getElapsedTime() > dRetryTimeout) \
						{ \
							/* Create exception to indicate retry timed out*/ \
							UCLIDException uex("ELI29856", "Database connection retry timed out!", ue2);  \
							\
							/* Log the caught exception. */ \
							uex.log(); \
							\
							break; \
						} \
						else \
						{ \
							/* Sleep to reduce the number of retries/second*/ \
							Sleep(100); \
						} \
					} \
				} \
				nRetryCount++; \
			} \
		} \
		while (!bRetrySuccess);


// Query to turn on Identity insert so Identity fields can be copied
static const string gstrSET_IDENTITY_INSERT_ON = "SET IDENTITY_INSERT <TableName> ON";

// Query to turn off Identity insert so Identity fields cannot be copied
static const string gstrSET_IDENTITY_INSERT_OFF = "SET IDENTITY_INSERT <TableName> OFF";

// PROMISE: To return the long value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API long getLongField( const FieldsPtr& ipFields, const string& strFieldName);

// PROMISE: Same as above except nDefaultIfNull will be returned if NULL.
FAMUTILS_API long getLongField(const FieldsPtr& ipFields, const string& strFieldName, long nDefaultIfNull);

// PROMISE: To return the long long value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API long long getLongLongField( const FieldsPtr& ipFields, const string& strFieldName );

// PROMISE: To return the bool value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API bool getBoolField(const FieldsPtr& ipFields, const string& strFieldName);

// PROMISE: To return the string value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API string getStringField( const FieldsPtr& ipFields, const string& strFieldName );

// PROMISE: To return the CTime value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
//			if noTZConversion true then datetime is in the local time zone otherwise there will be no adjustment
//			made to the time for the timezone
FAMUTILS_API SYSTEMTIME getTimeDateField(const FieldsPtr& ipFields, const string& strFieldName);

// PROMISE: To return the double value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API double getDoubleField( const FieldsPtr& ipFields, const string& strFieldName );

// PROMISE: To set the field named strFieldName in the ipFields collection to the long value nValue
//			if the field does not exist an exception will be thrown
FAMUTILS_API void setLongField( const FieldsPtr& ipFields, const string& strFieldName, const long nValue );

// PROMISE: To set the field named strFieldName in the ipFields collection to the long long value llValue
//			if the field does not exist an exception will be thrown
FAMUTILS_API void setLongLongField( const FieldsPtr& ipFields, const string& strFieldName, const long long llValue );

// PROMISE: To set the field named strFieldName in the ipFields collection to the string nValue
//			if the field does not exist an exception will be thrown
//		
FAMUTILS_API void setStringField( const FieldsPtr& ipFields, const string& strFieldName, const string& strValue, bool bEmptyStrAsNull = false );

// PROMISE: To set the field named strFieldName in the ipFields collection to the time date in timeDate
FAMUTILS_API void setTimeDateField(const FieldsPtr& ipFields, const string& strFieldName, const CTime timeDate);

// PROMISE: To set the field named strFieldName in the ipFields collection to the double value dValue
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API void setDoubleField( const FieldsPtr& ipFields, const string& strFieldName, const double& dValue );

// PROMISE:	To return datetime as string from the SQL server using the GETDATE() SQL function 
FAMUTILS_API string getSQLServerDateTime( const _ConnectionPtr& ipDBConnection );

// PROMISE: To return the datetime as SYSTEMTIME from the SQL server using the GETDATE() SQL function
FAMUTILS_API SYSTEMTIME getSQLServerDateTimeAsSystemTime(const _ConnectionPtr& ipDBConnection);

// Returns the connection string using the given server and database.
// If strAdditionalConnectionStringComponents is non=empty it will be added to the end of the
// connection string.
FAMUTILS_API string createConnectionString(const string& strServer, const string& strDatabase,
	const string& strAdvancedConnectionStringProperties = "", bool bAllowEmptyDB = false);

// Gets a case-insensitive map of all connection string property names to the associated values.
FAMUTILS_API csis_map<string>::type getConnectionStringProperties(const string& strConnectionString);

// Attempts to case-insensitively find the specified property in the specified connection string.
// Returns true if the property is found. If pstrValue is non-null and the property is found,
// pstrValue will be assigned the value of the property.
FAMUTILS_API bool findConnectionStringProperty(const string& strConnectionString,
	const string& strName, string *pstrValue = __nullptr);

// Appends or overrides the properties of rstrConnectionString with the properties specified in
// strNewProperties.
FAMUTILS_API void updateConnectionStringProperties(string& rstrConnectionString,
	const string& strNewProperties);

// PROMISE:	To execute the SQL Query and return the number of records affected
// NOTES:	strSQLQuery must be a query that returns no records
//			if bDisplayExceptions == false any exceptions will be thrown to caller
//			if bDisplayExceptions == true any exceptions will be displayed and 0 will be returned
//			If pnOutputID is not NULL, it is assumed that the query will return a single long value
//			with a field name of "ID". (presumably the ID of the record added/affected) Typically
//			this ID will be generated using the "Output" SQL clause.
// REQUIRED: This call must be made from code that has either explicitly locked the database or is
//			testing for the case that optimistic locking failed. Outside of the FileProcessingDB
//			class FileProcessingDB::ExecuteCommandQuery should be used instead.
FAMUTILS_API long executeCmdQuery(const _ConnectionPtr& ipDBConnection, const string& strSQLQuery,
	bool bDisplayExceptions = false, long *pnOutputID = __nullptr);

// Overload with five arguments that returns named long value.
// PROMISE:	To execute the SQL Query and return the number of records affected
// NOTES:	strSQLQuery must be a query that returns no records
//			if bDisplayExceptions == false any exceptions will be thrown to caller
//			if bDisplayExceptions == true any exceptions will be displayed and 0 will be returned
//			If pnOutputID is not NULL, then resultColumnName must be non-empty, and name the
//			column that the value of will be returned in pnOutputID.
//			If pnOutputID is not NULL, it is assumed that the query will return a single long value.
//			Typically this ID will be generated using the "Output" SQL clause.
// REQUIRED: This call must be made from code that has either explicitly locked the database or is
//			testing for the case that optimistic locking failed. Outside of the FileProcessingDB
//			class FileProcessingDB::ExecuteCommandReturnColumnValue() should be used instead.
FAMUTILS_API long executeCmdQuery(const _ConnectionPtr& ipDBConnection,
	const std::string& strSQLQuery,
	const std::string& resultColumnName,
	bool bDisplayExceptions = false,
	long* pnOutputID = nullptr);

// Executes all the command objects in the vector
// vecCmds: vector containing the command objects to execute
FAMUTILS_API long executeVectorOfCmd(const vector<_CommandPtr>& vecCmds);

// Builds a parameterized SQL command
// strQuery: SQL query that should denote parameters prefixed with "@" symbol (e.g. @ActionID)
// params:	 Maps each parameter name (including "@") to a _variant_t with the value to apply.
//			 Supported types: VT_BOOL, VT_INT/VT_I4, VT_I8, VT_BSTR, VT_VOID/VT_NULL
// NULL on its own will be interpreted as a value, unless in quotes, in which it will be
// interpreted as the literal string (without quotes). Quotes will be interpreted literally
// Except if the value is exactly "NULL" (with quotes, case insensitive).
// e.g. { @Comment, "NULL" } passes null value, { @Comment, "\"NULL\"" } passes 'NULL' as string,
// { @Comment, "This is \"NULL\"" } passes string: 'This is "NULL"'
// Values lists can be passed as parameters using the query syntax"@|<[VT_TYPE]>[PARAM_NAME]|".
// e.g.: "@|<VT_INT>FileIDs|"
// In this case, the parameter value must be of type BSTR and must contain a comma-delimited list
// representing all values to be passed in the list.
// Commas in strings need to be escaped using "\,". e.g.: { @|<VT_BSTR>Names|, "Doe\, Jon, Doe\, Jane" }
// NULL as a value will be handled the same as described above.
FAMUTILS_API _CommandPtr buildCmd(const _ConnectionPtr& ipDBConnection,
	const string &strQuery,
	map<string, _variant_t> params);

// Executes the provided _CommandPtr 
// if bDisplayExceptions == false any exceptions will be thrown to caller
// if bDisplayExceptions == true any exceptions will be displayed and 0 will be returned
FAMUTILS_API long executeCmd(const _CommandPtr& ipCommand, bool bDisplayExceptions = false);

// Executes the provided _CommandPtr, returning the first row's column value corresponding to pvtValue
// bAllowBlock: true if call should block if necessary read value; false to return false if record is locked
// bDisplayExceptions: false to throw exceptions to caller, true to display and return 0
// Returns: true if value was read; false if no value exists or record was locked
FAMUTILS_API bool executeCmd(const _CommandPtr& ipCommand,
	bool bDisplayExceptions,
	bool bAllowLock,
	const std::string& strResultColumnName,
	_variant_t* pvtValue);

// Executes the provided _CommandPtr, returning the first row's ID column value  to pnResult
// bAllowBlock: true if call should block if necessary read ID; false to return false if record is locked
// Returns: true if value was read; false if no value exists or record was locked
FAMUTILS_API bool getCmdId(const _CommandPtr& ipCommand, long* pnResult, bool bAllowBlock = true);
FAMUTILS_API bool getCmdId(const _CommandPtr& ipCommand, long long* pllResult, bool bAllowBlock = true);

// Overload with five arguments that returns named long long value.
// PROMISE:	To execute the SQL Query and return the number of records affected
// NOTES:	strSQLQuery must be a query that returns no records
//			if bDisplayExceptions == false any exceptions will be thrown to caller
//			if bDisplayExceptions == true any exceptions will be displayed and 0 will be returned
//			If pnOutputID is not NULL, then resultColumnName must be non-empty, and name the
//			column that the value of will be returned in pnOutputID.
//			If pnOutputID is not NULL, it is assumed that the query will return a single long long value.
//			Typically this ID will be generated using the "Output" SQL clause.
// REQUIRED: This call must be made from code that has either explicitly locked the database or is
//			testing for the case that optimistic locking failed. Outside of the FileProcessingDB
//			class FileProcessingDB::ExecuteCommandReturnColumnValue() should be used instead.
FAMUTILS_API long executeCmdQuery( const _ConnectionPtr& ipDBConnection, 
								   const std::string& strSQLQuery,
								   const std::string& resultColumnName,
								   bool bDisplayExceptions, 
								   long long *pnOutputID );

// Returns ID from the given table by looking up the strKey in the key column and if
// the key is not found it is added to the table if bAddKey is true and the new ID is returned
// otherwise an exception will be thrown that indicates the key was not found.
// If the key was found the rstrKey value will be modified to match value of strKeyCol stored in the database.
// this is so the case of the value will be the same since the database value is not case sensitive
FAMUTILS_API long getKeyID(const _ConnectionPtr& ipDBConnection, const string& strTable, const string& strKeyCol,
						   string& rstrKey, bool bAddKey = true);
 
// PROMISE: To drop all of the constraints that have any of the tables in vecTables as Foreign key table.
FAMUTILS_API void dropFKContraintsOnTables(const _ConnectionPtr& ipDBConnection, const vector<string>& vecTables);

// PROMISE: To drop the constraint named strConstraint on the table strTableName
FAMUTILS_API void dropConstraint(const _ConnectionPtr& ipDBConnection, const string& strTableName, const string& strConstraint);

// PROMISE: To drop all of the tables in vecTables from the database that is connected 
FAMUTILS_API void dropTablesInVector(const _ConnectionPtr& ipDBConnection, const vector<string>& vecTables);

// PROMISE: To return true if strTable exists in the database, using the given connection to the database
FAMUTILS_API bool doesTableExist(const _ConnectionPtr& ipDBConnection, string strTable);

// PROMISE: To call executeCmdQuery on all of the sql Queries in vecQueries
//			Returns the total number of rows updated from all queries
FAMUTILS_API long executeVectorOfSQL(const _ConnectionPtr& ipDBConnection, const vector<string>& vecQueries ); 

// PROMISE: To copy the values for fields that are in both ipSource and ipDest.
//			If bCopyID is false then a field named "ID" will not be copied.
FAMUTILS_API void copyExistingFields(const FieldsPtr& ipSource, const FieldsPtr& ipDest, bool bCopyID);

// PROMISE: To search the ipFields for the field named strFieldName if found return FieldPtr
//			if not found return NULL
FAMUTILS_API FieldPtr getNamedField(const FieldsPtr& ipFields, const string& strFieldName);

// PROMISE: To set the ID column in the ipDestFields if it exists the column name that is used
//			is the name of the strKeyTable with "ID" appended.  If the column is not found 
//			nothing is done. If the key is not found in the key table it will be added if bAddKey
//			is true otherwise an exception will be thrown
FAMUTILS_API void copyIDValue(const _ConnectionPtr& ipDestDB, const FieldsPtr& ipDestFields, 
							  const string& strKeyTable, const string& strKeyCol,
							  string strKeyValue, bool bAddKey);

// PROMISE: To return the IPersistStreamObj saved in the field with name strFieldName
// NOTE:	The named field is expected to be a variant_t with type VT_ARRAY | VT_UI1 (SAFEARRAY of bytes) 
//			that had an object streamed to it.
FAMUTILS_API IPersistStreamPtr getIPersistObjFromField(const FieldsPtr& ipFields, const string& strFieldName);

// PROMISE: To save the ipObj in a SAFEARRAY and stores it in the field with the name strFieldName
// NOTE:	This saves to a varbinary field in the database
FAMUTILS_API void setIPersistObjToField(const FieldsPtr& ipFields, const string& strFieldName, 
	IPersistStreamPtr ipObj );

// PROMISE: To return the Server name, creation date and last restore date of the database as strings
// NOTE:	If the database has never been restored the last restore date will equal the creation date
//			The following arguments will be returned with values from the database
//				strServerName
//				strCreateDate
//				strLastRestoreDate
//              isCluster true if the Database is on a Cluster and false if it is not
FAMUTILS_API void getDatabaseInfo(const _ConnectionPtr& ipDBConnection, const string &strDBName,
	string &strServerName, string &strCreateDate, string &strLastRestoreDate, bool &isCluster);

// PROMISE: To return the Server name, creation date and last restore date of the database as strings
// NOTE:	If the database has never been restored the last restore date will equal the creation date
//			The following arguments will be returned with values from the database
//				strServerName
//				ctCreateDate
//				ctLastRestoreDate
//              isCluster true if the Database is on a Cluster and false if it is not
FAMUTILS_API void getDatabaseInfo(const _ConnectionPtr& ipDBConnection, const string &strDBName,
	string &strServerName, SYSTEMTIME &ctCreateDate, SYSTEMTIME &ctLastRestoreDate, bool &isCluster);

// PROMISE: To return the bsDatabaseID ByteStream that contains the following:
//				<DatabaseGUID>,<DatabaseServer>,<DatabaseName>,<DatabaseCreationDate>,<DatabaseRestoreDate><LastUpdateTime>
// NOTE:	<DatabaseGUID> is a GUID that will be generated by this function
//			<DatabaseServer> is the server the database is on that is creating the string
//			<DatabaseName> is the database name the DatabaseID is being created for
//			<DatabaseCreationDate> is obtained from the database server for the given database
//			<DatabaseRestoreDate> is the last restored date of the database and should be equal
//				to the creation date if the database has not been restored.
//			<LastUpdateTime> - will be 0 to represent never being updated
FAMUTILS_API void createDatabaseID(const _ConnectionPtr& ipConnection, ByteStream &bsDatabaseID);

// PROMISE: To return true if the strFieldName contained in ipFields represents a NULL value in the 
//			record.
FAMUTILS_API bool isNULL(const FieldsPtr& ipFields, const string& strFieldName);

// PROMISE: To set the specified field's value to NULL.
FAMUTILS_API void setFieldToNull(const FieldsPtr& ipFields, const string& strFieldName);