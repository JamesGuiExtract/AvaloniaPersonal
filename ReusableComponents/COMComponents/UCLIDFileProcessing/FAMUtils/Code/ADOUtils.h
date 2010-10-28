// ADOUtils.h - Header file for ADO related functions

#pragma once

#include "FAMUtils.h"

#include <StopWatch.h>

#include <string>
#include <vector>

using namespace ADODB;
using namespace std;

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
#define END_ADO_CONNECTION_RETRY(ipRetryConnection, getDBConnection, nMaxRetryCount, dRetryTimeout, strELICode) \
					bRetrySuccess = true; \
				}\
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION(strELICode)\
			} \
			catch(UCLIDException ue) \
			{ \
				bool bConnectionIsAlive = false; \
				\
				try \
				{ \
					if (ipRetryConnection != NULL) \
					{ \
						getSQLServerDateTime(ipRetryConnection); \
						bConnectionIsAlive = true; \
					} \
				} \
				catch(...){}; \
				if (bConnectionIsAlive || nRetryCount >= nMaxRetryCount) \
				{ \
					throw ue; \
				}\
				if (!bRetryExceptionLogged) \
				{ \
					UCLIDException uex("ELI29853", "Database connection failed. Attempting to reconnect.", ue); \
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
							/* Do the close within a try catch because an exception on the close */ \
							/* could just mean the connection is in a bad state and recreating and */ \
							/* opening will put it in a good state */ \
							try \
							{ \
								if (ipRetryConnection != NULL && ipRetryConnection->State != adStateClosed) \
								{ \
									ipRetryConnection->Close(); \
								} \
							} \
							CATCH_AND_LOG_ALL_EXCEPTIONS("ELI29857") \
							\
							/* This will create a new connection for this thread and initialize */ \
							/* the schema */ \
							ipRetryConnection = getDBConnection(); \
							\
							UCLIDException ueConnected("ELI29854", "Connection retry successful."); \
							ueConnected.log(); \
							\
							break; \
						} \
						CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29855"); \
					} \
					catch(UCLIDException ue) \
					{ \
						if (sw.getElapsedTime() > dRetryTimeout) \
						{ \
							/* Create exception to indicate retry timed out*/ \
							UCLIDException uex("ELI29856", "Database connection retry timed out!", ue);  \
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
FAMUTILS_API long getLongField( const FieldsPtr& ipFields, const string& strFieldName );

// PROMISE: To return the long long value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API long long getLongLongField( const FieldsPtr& ipFields, const string& strFieldName );

// PROMISE: To return the string value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API string getStringField( const FieldsPtr& ipFields, const string& strFieldName );

// PROMISE: To return the CTime value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API CTime getTimeDateField(const FieldsPtr& ipFields, const string& strFieldName );

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
FAMUTILS_API void setTimeDateField( const FieldsPtr& ipFields, const string& strFieldName, const CTime timeDate);

// PROMISE: To set the field named strFieldName in the ipFields collection to the double value dValue
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API void setDoubleField( const FieldsPtr& ipFields, const string& strFieldName, const double& dValue );

// PROMISE:	To return the last Identity value for the last record added to the given table using the given connection
FAMUTILS_API long getLastTableID ( const _ConnectionPtr& ipDBConnection, string strTableName );

// PROMISE:	To return datetime as string from the SQL server using the GETDATE() SQL function 
FAMUTILS_API string getSQLServerDateTime( const _ConnectionPtr& ipDBConnection );

// PROMISE: To return datetime as CTime from the SQL server using the GETDATE() SQL function
FAMUTILS_API CTime getSQLServerDateTimeAsCTime(const _ConnectionPtr& ipDBConnection);

// Returns the connection string using the given server and database.
FAMUTILS_API string createConnectionString(const string& strServer, const string& strDatabase);

// PROMISE:	To execute the SQL Query and return the number of records affected
// NOTES:	strSQLQuery must be a query that returns no records
//			if bDisplayExceptions == false any exceptions will be thrown to caller
//			if bDisplayExceptions == true any exceptions will be displayed and 0 will be returned
FAMUTILS_API long executeCmdQuery( const _ConnectionPtr& ipDBConnection, const string& strSQLQuery, bool bDisplayExceptions = false );

// Returns ID from the given table by looking up the strKey in the key column and if
// the key is not found it is added to the table if bAddKey is true and the new ID is returned
// otherwise an exception will be thrown that indicates the key was not found.
// If the key was found the rstrKey value will be modified to match value of strKeyCol stored in the database.
// this is so the case of the value will be the same since the database value is not case sensitive
FAMUTILS_API long getKeyID(const _ConnectionPtr& ipDBConnection, const string& strTable, const string& strKeyCol,
						   string& rstrKey, bool bAddKey = true);
 
// PROMISE: To drop all of the contraints that have any of the tables in vecTables as Foreign key table.
FAMUTILS_API void dropFKContraintsOnTables(const _ConnectionPtr& ipDBConnection, const vector<string>& vecTables);

// PROMISE: To drop the constraint named strConstraint on the table strTableName
FAMUTILS_API void dropConstraint(const _ConnectionPtr& ipDBConnection, const string& strTableName, const string& strConstraint);

// PROMISE: To drop all of the tables in vecTables from the database that is connected 
FAMUTILS_API void dropTablesInVector(const _ConnectionPtr& ipDBConnection, const vector<string>& vecTables);

// PROMISE: To return true if strTable exists in the database, using the given connection to the database
FAMUTILS_API bool doesTableExist(const _ConnectionPtr& ipDBConnection, string strTable);

// PROMISE: To call executeCmdQuery on all of the sql Queries in vecQueries
FAMUTILS_API void executeVectorOfSQL(const _ConnectionPtr& ipDBConnection, const vector<string>& vecQueries ); 

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
