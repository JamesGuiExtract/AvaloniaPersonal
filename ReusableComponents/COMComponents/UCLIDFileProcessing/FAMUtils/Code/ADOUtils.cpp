
#include "stdafx.h"
#include "FAMUtils.h"
#include "ADOUtils.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>

using namespace ADODB;
using namespace std;

// Constants for Connection String
static const string& gstrSERVER = "Server";
static const string& gstrDATABASE = "Database";

// Settings that don't change if included in the connection string
static const string& gstrPROVIDER = "Provider=SQLNCLI10";
static const string& gstrINTEGRATED_SECURITY = "Integrated Security=SSPI";
static const string& gstrDATA_TYPE_COMPATIBILITY = "DataTypeCompatibility=80";
static const string& gstrMARS_CONNECTION = "MARS Connection=True";

static const string& gstrDATE_TIME_FORMAT = "%Y-%m-%d %H:%M:%S";

// Misc queries
static const string gstrGET_SQL_SERVER_TIME = "SELECT GETDATE() as CurrDateTime";

//-------------------------------------------------------------------------------------------------
long getLongField( const FieldsPtr& ipFields, const string& strFieldName )
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI15335", ipFields != __nullptr );

		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI15282", ipItem != __nullptr );

			// get the value
			variant_t vtItem = ipItem->Value;
			
			// The value should be long type
			if ( vtItem.vt != VT_I4 )
			{
				UCLIDException ue("ELI15283", "Value is not a long type.");
				ue.addDebugInfo("Type", vtItem.vt);
				throw ue;
			}
			
			// return the long value of the variant
			return vtItem.lVal;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15284");
	}
	catch(UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
long long getLongLongField( const FieldsPtr& ipFields, const string& strFieldName )
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI15336", ipFields != __nullptr );

		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI15285", ipItem != __nullptr );

			// get the value
			variant_t vtItem = ipItem->Value;

			// The value may be of type DECIMAL so we want to convert it to long long
			if ( vtItem.vt == VT_DECIMAL )
			{
				// Change type to long long
				vtItem.ChangeType( VT_I8 );
			}

			// if the type is not long long throw an exception with the type
			if ( vtItem.vt != VT_I8 )
			{
				UCLIDException ue("ELI15286", "Value is not a long long type.");
				ue.addDebugInfo("Type", vtItem.vt);
				throw ue;
			}

			// return the long long value of the variant
			return vtItem.llVal;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15287");
	}
	catch(UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
string getStringField( const FieldsPtr& ipFields, const string& strFieldName )
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI15331", ipFields != __nullptr );

		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI15288", ipItem != __nullptr );

			// get the value
			variant_t vtItem = ipItem->Value;

			// If the value is not a bstr throw an exception
			if ( vtItem.vt != VT_BSTR )
			{
				UCLIDException ue("ELI15289", "Value is not a string type.");
				ue.addDebugInfo("Type", vtItem.vt);
				throw ue;
			}

			// convert the bstr to astring and return
			return asString(vtItem.bstrVal);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15290");
	}
	catch(UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void setLongField( const FieldsPtr& ipFields, const string& strFieldName, const long nValue )
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI15332", ipFields != __nullptr );

		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI15292", ipItem != __nullptr );

			// set the variant type to the given value
			variant_t vtItem;
			vtItem = nValue;

			// Set the field to the variant type
			ipItem->Value = vtItem;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15293");
	}
	catch(UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		ue.addDebugInfo("Value To Set", nValue);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void setLongLongField( const FieldsPtr& ipFields, const string& strFieldName, const long long llValue )
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI15333", ipFields != __nullptr );
		
		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI15294", ipItem != __nullptr );

			// Set a variant to the long long value
			variant_t vtItem;
			vtItem.vt = VT_I8;
			vtItem.llVal = llValue;

			// set the field value to the variant
			ipItem->Value = vtItem;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15295");
	}
	catch(UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		ue.addDebugInfo("Value To Set", llValue);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void setStringField( const FieldsPtr& ipFields, const string& strFieldName, const string& strValue , bool bEmptyStrAsNull )
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI15334", ipFields != __nullptr );
		
		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI15296", ipItem != __nullptr );

			variant_t vtItem;
			
			// if the string is empty and bEmptyStrAsNull is true save as null
			if ( bEmptyStrAsNull & strValue == "" )
			{
				vtItem.vt = VT_NULL;
			}
			// Save the string as the value
			else
			{
				vtItem = strValue.c_str();
			}

			// set the value to the variant
			ipItem->Value = vtItem;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15297");
	}
	catch(UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		ue.addDebugInfo("Value To Set", strValue);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
CTime getTimeDateField(const FieldsPtr& ipFields, const string& strFieldName )
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI15407", ipFields != __nullptr );

		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI15408", ipItem != __nullptr );

			// get the value
			variant_t vtItem = ipItem->Value;
			
			// The value should be Date type
			if ( vtItem.vt != VT_DATE )
			{
				UCLIDException ue("ELI15409", "Value is not a date time type.");
				ue.addDebugInfo("Type", vtItem.vt);
				throw ue;
			}
			
			// Get the date time as systemTime
			SYSTEMTIME systemTime;
			VariantTimeToSystemTime(vtItem, &systemTime);

			// Convert to CTime and return
			return CTime(systemTime);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15410");
	}
	catch(UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void setTimeDateField( const FieldsPtr& ipFields, const string& strFieldName, const CTime timeDate)
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI15411", ipFields != __nullptr );

		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI15412", ipItem != __nullptr );

			// set the variant type to the given value
			variant_t vtItem;
			vtItem = timeDate.Format(gstrDATE_TIME_FORMAT.c_str());

			// Set the field to the variant type
			ipItem->Value = vtItem;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15413");
	}
	catch(UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		ue.addDebugInfo("Value To Set", (LPCTSTR) timeDate.Format("%m/%d/%Y %H:%M:%S"));
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void setDoubleField( const FieldsPtr& ipFields, const string& strFieldName, const double& dValue )
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI19671", ipFields != __nullptr );

		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI19672", ipItem != __nullptr );

			// set the variant type to the given value
			variant_t vtItem;
			vtItem = dValue;

			// Set the field to the variant type
			ipItem->Value = vtItem;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI19673");
	}
	catch(UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		ue.addDebugInfo("Value To Set", dValue);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
double getDoubleField( const FieldsPtr& ipFields, const string& strFieldName )
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI19668", ipFields != __nullptr );

		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI19674", ipItem != __nullptr );

			// get the value
			variant_t vtItem = ipItem->Value;
			
			// The value should be decimal, double or float type
			if ( vtItem.vt != VT_DECIMAL && vtItem.vt != VT_R8 && vtItem.vt != VT_R4 )
			{
				UCLIDException ue("ELI19669", "Value is not a decimal type.");
				ue.addDebugInfo("Type", vtItem.vt);
				throw ue;
			}

			// change value to double type
			vtItem.ChangeType(VT_R8);
			
			// return the long value of the variant
			return vtItem.dblVal;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI19670");
	}
	catch(UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
long getLastTableID( const _ConnectionPtr& ipDBConnection, string strTableName )
{
	ASSERT_ARGUMENT("ELI18816", ipDBConnection != __nullptr);

	_RecordsetPtr ipRSet;
	// Build SQL string to get the last ID for the given table
	string strGetIDSQL = "SELECT IDENT_CURRENT ('" + strTableName + "') AS CurrentID";

	// Execute the command 
	ipRSet = ipDBConnection->Execute( strGetIDSQL.c_str(), NULL, adCmdUnknown );
	ASSERT_RESOURCE_ALLOCATION("ELI13525", ipRSet != __nullptr );

	// The ID field in all of the tables is a 32 bit int so
	// the value returned by this function can be changed to type long
	return (long) getLongLongField( ipRSet->Fields, "CurrentID" );
}
//-------------------------------------------------------------------------------------------------
string getSQLServerDateTime( const _ConnectionPtr& ipDBConnection )
{
	ASSERT_ARGUMENT("ELI18817", ipDBConnection != __nullptr);

	// Get the current date time
	_RecordsetPtr ipRSTime;
	ipRSTime = ipDBConnection->Execute (gstrGET_SQL_SERVER_TIME.c_str(), NULL, adCmdText );
	ASSERT_RESOURCE_ALLOCATION("ELI15326", ipRSTime != __nullptr );
	
	// Get the fields pointer
	FieldsPtr ipFields = ipRSTime->Fields;
	ASSERT_RESOURCE_ALLOCATION("ELI15701", ipFields != __nullptr );

	// Get the CurrDateTime item
	FieldPtr ipItem = ipFields->Item["CurrDateTime"];
	ASSERT_RESOURCE_ALLOCATION("ELI15327", ipItem != __nullptr );

	// get the value
	_variant_t vtTime;
	vtTime = ipItem->Value;

	// Change date to string type
	_variant_t vtTimeStr;
	vtTimeStr.ChangeType( VT_BSTR, &vtTime);

	// return the bstr as a string
	return asString(vtTimeStr.bstrVal);;
}
//-------------------------------------------------------------------------------------------------
CTime getSQLServerDateTimeAsCTime(const _ConnectionPtr& ipDBConnection)
{
	ASSERT_ARGUMENT("ELI30822", ipDBConnection != __nullptr);

	// Get the current date time
	_RecordsetPtr ipRSTime;
	ipRSTime = ipDBConnection->Execute (gstrGET_SQL_SERVER_TIME.c_str(), NULL, adCmdText );
	ASSERT_RESOURCE_ALLOCATION("ELI30823", ipRSTime != __nullptr );
	
	// Get the fields pointer
	FieldsPtr ipFields = ipRSTime->Fields;
	ASSERT_RESOURCE_ALLOCATION("ELI30824", ipFields != __nullptr );

	return getTimeDateField(ipFields, "CurrDateTime");
}
//-------------------------------------------------------------------------------------------------
string createConnectionString(const string& strServer, const string& strDatabase)
{
	ASSERT_ARGUMENT("ELI17471", !strServer.empty());
	ASSERT_ARGUMENT("ELI17472", !strDatabase.empty());

	// Build the connection string
	// Add the default provider
	string strConnectionString = gstrPROVIDER + ";";

	// Add the server
	strConnectionString += gstrSERVER + "=" + strServer+ ";";

	// Add the Database
	strConnectionString += gstrDATABASE + "=" + strDatabase + ";";

	strConnectionString += gstrINTEGRATED_SECURITY + ";";

	// Add the remaining settings that are needed
	strConnectionString += gstrMARS_CONNECTION + ";";
	strConnectionString += gstrDATA_TYPE_COMPATIBILITY;

	return strConnectionString;
}
//-------------------------------------------------------------------------------------------------
long executeCmdQuery(const _ConnectionPtr& ipDBConnection, const string& strSQLQuery, bool bDisplayExceptions)
{
	ASSERT_ARGUMENT("ELI18818", ipDBConnection != __nullptr);

	variant_t vtRecordsAffected = 0L;
	try
	{
		try
		{
			// Execute the SQL drop script
			ipDBConnection->Execute(strSQLQuery.c_str(), &vtRecordsAffected, adCmdText | adExecuteNoRecords );
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI14382");
	}
	catch(UCLIDException& ue)
	{
		ue.addDebugInfo("SQL", strSQLQuery, true);

		if (!bDisplayExceptions)
		{
			// Rethrow the exception
			throw ue;
		}
		// Display exception
		ue.display();
		return 0;
	} 
	return vtRecordsAffected.lVal;
}
//-------------------------------------------------------------------------------------------------
long getKeyID(const _ConnectionPtr& ipDBConnection, const string& strTable, const string& strKeyCol, string& rstrKey, bool bAddKey)
{
	ASSERT_ARGUMENT("ELI18775", ipDBConnection != __nullptr);

	// Set ID to 0
	long lID = 0;
	
	// Convert apostrophe characters to double apostrophes [FlexIDSCore #3362]
	string strFileName = rstrKey;
	replaceVariable(strFileName, "'", "''" );

	// Allocate recordset
	_RecordsetPtr ipKeySet(__uuidof( Recordset ));
	ASSERT_RESOURCE_ALLOCATION("ELI18018", ipKeySet != __nullptr );

	// Create Keys sql
	string strKeySQL = "SELECT ID, " + strKeyCol + " FROM " + strTable + " WHERE " + strKeyCol + " = '" + strFileName + "'";

	CursorTypeEnum eCursorType = (bAddKey) ? adOpenDynamic : adOpenStatic;

	// Open recordset
	ipKeySet->Open( strKeySQL.c_str(), _variant_t((IDispatch *)ipDBConnection, true), eCursorType, 
		adLockOptimistic, adCmdText );

	// If adoEOF is true key value was not found
	if (asCppBool(ipKeySet->adoEOF))
	{
		// Check if add is required
		if (bAddKey)
		{
			ipKeySet->AddNew();
			setStringField(ipKeySet->Fields, strKeyCol, rstrKey, true);
			ipKeySet->Update();
			lID = getLastTableID(ipDBConnection, strTable);
		}
		else
		{
			string strMsg = strKeyCol + " does not exist!";
			UCLIDException ue("ELI18131", strMsg);
			ue.addDebugInfo(strKeyCol, rstrKey);
			throw ue;
		}
	}
	else
	{
		lID = getLongField(ipKeySet->Fields, "ID");
		rstrKey = getStringField(ipKeySet->Fields, strKeyCol );
	}
	return lID;
}
//-------------------------------------------------------------------------------------------------
void dropConstraint(const _ConnectionPtr& ipDBConnection, const string& strTableName, const string& strConstraint)
{
	ASSERT_ARGUMENT("ELI18819", ipDBConnection != __nullptr);

	// Build the drop SQL statement
	string strDropSQL = "ALTER TABLE [" + strTableName + "] DROP CONSTRAINT [" + strConstraint + "]";

	// Drop the contraint
	executeCmdQuery(ipDBConnection, strDropSQL, true);
}
//-------------------------------------------------------------------------------------------------
void dropFKContraintsOnTables(const _ConnectionPtr& ipDBConnection, const vector<string>& vecTables)
{
	ASSERT_ARGUMENT("ELI18820", ipDBConnection != __nullptr);

	// Open recordset with all of the Foreign Key relationships
	_RecordsetPtr ipConstraints = ipDBConnection->OpenSchema(adSchemaForeignKeys);
	ASSERT_RESOURCE_ALLOCATION("ELI18017", ipConstraints != __nullptr );

	// Loop through all constraints
	while (!asCppBool(ipConstraints->adoEOF))
	{
		// Get the Name of the Foreign key table
		string strTableName = getStringField(ipConstraints->Fields, "FK_TABLE_NAME");
		
		// Check if it is our table
		if (vectorContainsElement(vecTables, strTableName))
		{
			// Get the name of the Foreign key
			string strConstraintName = getStringField(ipConstraints->Fields, "FK_NAME");

			// Drop the Foreign key
			dropConstraint(ipDBConnection, strTableName, strConstraintName);
		}

		// Move to next constraint
		ipConstraints->MoveNext();
	}
}
//-------------------------------------------------------------------------------------------------
void dropTablesInVector(const _ConnectionPtr& ipDBConnection, const vector<string>& vecTables)
{
	ASSERT_ARGUMENT("ELI18821", ipDBConnection != __nullptr);
	
	try
	{
		// Get the tables that exist in the database
		_RecordsetPtr ipTables = ipDBConnection->OpenSchema(adSchemaTables);

		// Drop all Foreign key constraints
		dropFKContraintsOnTables(ipDBConnection, vecTables);

		// Drop Each table
		ipTables->MoveFirst();

		// Go thru all of the tables and Remove FK constraints
		while ( ipTables->adoEOF != VARIANT_TRUE)
		{
			// Get the Table Type
			string strType = getStringField( ipTables->Fields, "TABLE_TYPE" );

			// Only want to drop tables that we create
			if ( strType == "TABLE" )
			{
				// Get the table name 
				string strTableName = getStringField( ipTables->Fields, "TABLE_NAME" );

				// Don't drop the Login table if it exists and only drop Extract systems tables
				if (vectorContainsElement(vecTables, strTableName))
				{
					// Build the drop script
					string strDropSQL = "DROP TABLE [" + strTableName + "]";

					// Drop the table
					executeCmdQuery(ipDBConnection, strDropSQL, false);
				}
			}
			ipTables->MoveNext();
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27611")
}
//-------------------------------------------------------------------------------------------------
bool doesTableExist(const _ConnectionPtr& ipDBConnection, string strTable)
{
	// Get the tables that exist in the database
	_RecordsetPtr ipTables = ipDBConnection->OpenSchema(adSchemaTables);

	while ( ipTables->adoEOF != VARIANT_TRUE)
	{
		// Get the Table Type
		string strType = getStringField( ipTables->Fields, "TABLE_TYPE" );

		// Only want to drop tables that we create
		if ( strType == "TABLE" )
		{
			// Get the table name 
			string strTableName = getStringField( ipTables->Fields, "TABLE_NAME" );

			if ( strTableName == strTable )
			{
				return true;
			}
		}
		ipTables->MoveNext();
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
long executeVectorOfSQL(const _ConnectionPtr& ipDBConnection, const vector<string>& vecQueries )
{	
	ASSERT_ARGUMENT("ELI18822", ipDBConnection != __nullptr);

	long nNumRowsUpdated = 0;

	// Execute all of the queries
	for each ( string s in vecQueries )
	{
		try
		{
			nNumRowsUpdated += executeCmdQuery(ipDBConnection, s);
		}
		catch(UCLIDException& ue )
		{
			UCLIDException uexOuter("ELI18811", "Error executing SQL Query", ue);
			uexOuter.addDebugInfo("Query", s, true);
			throw uexOuter;
		}
	}

	return nNumRowsUpdated;
}
//-------------------------------------------------------------------------------------------------
FAMUTILS_API void copyExistingFields(const FieldsPtr& ipSource, const FieldsPtr& ipDest, bool bCopyID)
{
	INIT_EXCEPTION_AND_TRACING("MLI00031");

	try
	{
		ASSERT_ARGUMENT("ELI20024", ipSource != __nullptr);
		ASSERT_ARGUMENT("ELI20025", ipDest != __nullptr);

		// Get the number of fields in the source fields list
		long nSourceCount = ipSource->Count;
		_lastCodePos = "10";

		// Get the number of fields in the dest fields list
		long nDestCount = ipDest->Count;
		_lastCodePos = "20";

		// Step through all of the source fields
		for ( long n = 0; n < nSourceCount; n++ )
		{
			_lastCodePos = "30-" + asString(n);

			// Get field with that index
			FieldPtr ipSourceField = ipSource->Item[variant_t(n)];
			ASSERT_RESOURCE_ALLOCATION("ELI20029", ipSourceField != __nullptr);

			// Get the source field name
			string strSourceFieldName = asString(ipSourceField->Name);
			_lastCodePos = "40";

			// if bCopyID is true all fields should be copied if false
			// then only the fields that are not named "ID"
			if (bCopyID || strSourceFieldName != "ID")
			{
				// Attempt to get the destination field
				FieldPtr ipDestField = __nullptr;
				try
				{
					ipDestField = ipDest->Item[strSourceFieldName.c_str()];
				}
				catch(...)
				{
					// Field was not found, just eat the exception
				}

				// Only copy to the destination field if it exists
				if (ipDestField != __nullptr)
				{
					// Copy the source value into the destination
					ipDestField->Value = ipSourceField->Value;
				}
			}
			_lastCodePos = "80";
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20154");
}
//-------------------------------------------------------------------------------------------------
FieldPtr getNamedField(const FieldsPtr& ipFields, const string& strFieldName)
{
	INIT_EXCEPTION_AND_TRACING("MLI00032");

	try
	{
		ASSERT_ARGUMENT("ELI20052", ipFields != __nullptr);
		ASSERT_ARGUMENT("ELI20053", !strFieldName.empty());

		// Attempt to get the names field, if it does not exist, just return NULL
		try
		{
			FieldPtr ipField = ipFields->Item[strFieldName.c_str()];
			return ipField;
		}
		catch(...)
		{
			return NULL;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20155");
}
//-------------------------------------------------------------------------------------------------
FAMUTILS_API void copyIDValue(const _ConnectionPtr& ipDestDB, const FieldsPtr& ipDestFields, 
							  const string& strKeyTable, const string& strKeyCol,
							  string strKeyValue, bool bAddKey)
{
	INIT_EXCEPTION_AND_TRACING("MLI00033");

	try
	{
		ASSERT_ARGUMENT("ELI20054", ipDestDB != __nullptr);
		ASSERT_ARGUMENT("ELI20055", ipDestFields != __nullptr);

		string strIDColName = strKeyTable + "ID";
		_lastCodePos = "10";

		// Check if there is a column in the dest
		FieldPtr ipField = getNamedField(ipDestFields, strIDColName);
		_lastCodePos = "20";

		// if the field does not exist just return
		if (ipField == __nullptr)
		{
			return;
		}

		// The ID from the dest table the actions should already be transfered
		long nID = getKeyID(ipDestDB, strKeyTable, strKeyCol, strKeyValue, bAddKey);		
		_lastCodePos = "30";

		// Set the ID
		setLongField(ipDestFields, strIDColName, nID);
		_lastCodePos = "40";
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20156");
}
//-------------------------------------------------------------------------------------------------
