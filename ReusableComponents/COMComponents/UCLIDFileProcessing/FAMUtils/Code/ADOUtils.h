// ADOUtils.h - Header file for ADO related functions

#pragma once

#include "FAMUtils.h"

#include <string>
#include <vector>


// Query to turn on Identity insert so Identity fields can be copied
static const std::string gstrSET_IDENTITY_INSERT_ON = "SET IDENTITY_INSERT FAMFile ON";

// Query to turn off Identity insert so Identity fields cannot be copied
static const std::string gstrSET_IDENTITY_INSERT_OFF = "SET IDENTITY_INSERT FAMFile OFF";

// PROMISE: To return the long value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API long getLongField( ADODB::FieldsPtr ipFields, const std::string& strFieldName );

// PROMISE: To return the long long value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API long long getLongLongField( ADODB::FieldsPtr ipFields, const std::string& strFieldName );

// PROMISE: To return the string value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API std::string getStringField( ADODB::FieldsPtr ipFields, const std::string& strFieldName );

// PROMISE: To return the CTime value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API CTime getTimeDateField(ADODB::FieldsPtr ipFields, const std::string& strFieldName );

// PROMISE: To return the double value of the field named strFieldName in the ipFields collection
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API double getDoubleField( ADODB::FieldsPtr ipFields, const std::string& strFieldName );

// PROMISE: To set the field named strFieldName in the ipFields collection to the long value nValue
//			if the field does not exist an exception will be thrown
FAMUTILS_API void setLongField( ADODB::FieldsPtr ipFields, const std::string& strFieldName, const long nValue );

// PROMISE: To set the field named strFieldName in the ipFields collection to the long long value llValue
//			if the field does not exist an exception will be thrown
FAMUTILS_API void setLongLongField( ADODB::FieldsPtr ipFields, const std::string& strFieldName, const long long llValue );

// PROMISE: To set the field named strFieldName in the ipFields collection to the string nValue
//			if the field does not exist an exception will be thrown
//		
FAMUTILS_API void setStringField( ADODB::FieldsPtr ipFields, const std::string& strFieldName, const std::string& strValue, bool bEmptyStrAsNull = false );

// PROMISE: To set the field named strFieldName in the ipFields collection to the time date in timeDate
FAMUTILS_API void setTimeDateField( ADODB::FieldsPtr ipFields, const std::string& strFieldName, const CTime timeDate);

// PROMISE: To set the field named strFieldName in the ipFields collection to the double value dValue
//			if the field does not exist an exception will be thrown
//			if the field is of the wrong type an exception will be thrown
FAMUTILS_API void setDoubleField( ADODB::FieldsPtr ipFields, const std::string& strFieldName, const double& dValue );

// PROMISE:	To return the last Identity value for the last record added to the given table using the given connection
FAMUTILS_API long getLastTableID ( ADODB::_ConnectionPtr ipDBConnection, std::string strTableName );

// PROMISE:	To return datetime as string from the SQL server using the GETDATE() SQL function 
FAMUTILS_API std::string getSQLServerDateTime( ADODB::_ConnectionPtr ipDBConnection );

// Returns the connection string using the given server and database.
FAMUTILS_API std::string createConnectionString(const std::string& strServer, const std::string& strDatabase);

// PROMISE:	To execute the SQL Query and return the number of records affected
// NOTES:	strSQLQuery must be a query that returns no records
//			if bDisplayExceptions == false any exceptions will be thrown to caller
//			if bDisplayExceptions == true any exceptions will be displayed and 0 will be returned
FAMUTILS_API long executeCmdQuery( ADODB::_ConnectionPtr ipDBConnection, const std::string& strSQLQuery, bool bDisplayExceptions = false );

// Returns ID from the given table by looking up the strKey in the key column and if
// the key is not found it is added to the table if bAddKey is true and the new ID is returned
// otherwise an exception will be thrown that indicates the key was not found.
// If the key was found the rstrKey value will be modified to match value of strKeyCol stored in the database.
// this is so the case of the value will be the same since the database value is not case sensitive
FAMUTILS_API long getKeyID(ADODB::_ConnectionPtr ipDBConnection, const std::string& strTable, const std::string& strKeyCol,
						   std::string& rstrKey, bool bAddKey = true);
 
// PROMISE: To drop all of the contraints that have any of the tables in vecTables as Foreign key table.
FAMUTILS_API void dropFKContraintsOnTables(ADODB::_ConnectionPtr ipDBConnection, const std::vector<std::string>& vecTables);

// PROMISE: To drop the constraint named strConstraint on the table strTableName
FAMUTILS_API void dropConstraint(ADODB::_ConnectionPtr ipDBConnection, const std::string& strTableName, const std::string& strConstraint);

// PROMISE: To drop all of the tables in vecTables from the database that is connected 
FAMUTILS_API void dropTablesInVector(ADODB::_ConnectionPtr ipDBConnection, const std::vector<std::string>& vecTables);

// PROMISE: To return true if strTable exists in the database, using the given connection to the database
FAMUTILS_API bool doesTableExist(ADODB::_ConnectionPtr ipDBConnection, std::string strTable);

// PROMISE: To call executeCmdQuery on all of the sql Queries in vecQueries
FAMUTILS_API void executeVectorOfSQL(ADODB::_ConnectionPtr ipDBConnection, const std::vector<std::string>& vecQueries ); 

// PROMISE: To copy the values for fields that are in both ipSource and ipDest.
//			If bCopyID is false then a field named "ID" will not be copied.
FAMUTILS_API void copyExistingFields(ADODB::FieldsPtr ipSource, ADODB::FieldsPtr ipDest, bool bCopyID);

// PROMISE: To search the ipFields for the field named strFieldName if found return FieldPtr
//			if not found return NULL
FAMUTILS_API ADODB::FieldPtr getNamedField(ADODB::FieldsPtr ipFields, const std::string& strFieldName);

// PROMISE: To set the ID column in the ipDestFields if it exists the column name that is used
//			is the name of the strKeyTable with "ID" appended.  If the column is not found 
//			nothing is done. If the key is not found in the key table it will be added if bAddKey
//			is true otherwise an exception will be thrown
FAMUTILS_API void copyIDValue(ADODB::_ConnectionPtr ipDestDB, ADODB::FieldsPtr ipDestFields, 
							  const std::string& strKeyTable, const std::string& strKeyCol,
							  std::string strKeyValue, bool bAddKey);