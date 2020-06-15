
#include "stdafx.h"
#include "FAMUtils.h"
#include "ADOUtils.h"
#include "FAMUtilsConstants.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <VectorOperations.h>
#include <StringTokenizer.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <DateUtil.h>

using namespace ADODB;
using namespace std;

// Settings that don't change if included in the connection string
static const string& gstrPROVIDER = "Provider=SQLNCLI11";
static const string& gstrINTEGRATED_SECURITY = "Integrated Security=SSPI";
static const string& gstrDATA_TYPE_COMPATIBILITY = "DataTypeCompatibility=80";
static const string& gstrMARS_CONNECTION = "MARS Connection=True";

// Misc queries
static const string gstrGET_SQL_SERVER_TIME = "SELECT GETDATE() as CurrDateTime";
static const string gstrGET_SQL_SERVER_DATETIMEOFFSET = "SELECT SYSDATETIMEOFFSET() as CurrDateTimeOffset";

//-------------------------------------------------------------------------------------------------
string getClusterName(const  _ConnectionPtr& ipDBConnection)
{
	try
	{
		string strClusterNameQuery = "EXEC ('sp_GetClusterName')";
		_RecordsetPtr clusterResult = ipDBConnection->Execute(strClusterNameQuery.c_str(), NULL, adCmdText);
		if (!clusterResult->adoEOF)
		{
			FieldsPtr ipFields = clusterResult->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI49860", ipFields != __nullptr);

			return getStringField(clusterResult->Fields, "cluster_name");
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI49861");
	return "";
}

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
long getLongField(const FieldsPtr& ipFields, const string& strFieldName, long nDefaultIfNull)
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI43417", ipFields != __nullptr);

		try
		{
			FieldPtr ipItem(__nullptr);

			// Get the Field from the fields list
			try
			{
				ipItem = ipFields->Item[strFieldName.c_str()];
			}
			catch (...)
			{
				return nDefaultIfNull;
			}
			ASSERT_RESOURCE_ALLOCATION("ELI43418", ipItem != __nullptr);

			// get the value
			variant_t vtItem = ipItem->Value;

			if (vtItem.vt == VT_NULL)
			{
				return nDefaultIfNull;
			}

			// The value should be long type
			if (vtItem.vt != VT_I4)
			{
				UCLIDException ue("ELI43419", "Value is not a long type.");
				ue.addDebugInfo("Type", vtItem.vt);
				throw ue;
			}

			// return the long value of the variant
			return vtItem.lVal;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI43420");
	}
	catch (UCLIDException& ue)
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
bool getBoolField(const FieldsPtr& ipFields, const string& strFieldName)
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI36081", ipFields != __nullptr );

		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI36082", ipItem != __nullptr );

			// get the value
			variant_t vtItem = ipItem->Value;
			
			// The value should be bool type
			if ( vtItem.vt != VT_BOOL )
			{
				UCLIDException ue("ELI36083", "Value is not a bool type.");
				ue.addDebugInfo("Type", vtItem.vt);
				throw ue;
			}
			
			// return the bool value of the variant
			return asCppBool(vtItem.boolVal);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36084");
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

			// If the value is NULL return an empty string
			if (vtItem.vt == VT_NULL)
			{
				return "";
			}
			else if ( vtItem.vt != VT_BSTR )
			{
				// If the value is not a bstr throw an exception
				UCLIDException ue("ELI15289", "Value is not a string type.");
				ue.addDebugInfo("Type", vtItem.vt);
				throw ue;
			}

			// convert the bstr to a string and return
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
SYSTEMTIME getTimeDateField(const FieldsPtr& ipFields, const string& strFieldName)
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
			if ( vtItem.vt != VT_DATE && vtItem.vt != VT_BSTR)
			{
				UCLIDException ue("ELI15409", "Value is not a date time type.");
				ue.addDebugInfo("Type", vtItem.vt);
				throw ue;
			}
			
			// Get the date time as systemTime
			SYSTEMTIME systemTime;
			VariantTimeToSystemTime(vtItem, &systemTime);

			return systemTime;
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
IPersistStreamPtr getIPersistObjFromField(const FieldsPtr& ipFields, const string& strFieldName)
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make sure ipFields is not NULL
		ASSERT_ARGUMENT("ELI37208", ipFields != __nullptr );

		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI37209", ipItem != __nullptr );

			// Get the value
			variant_t vtValue = ipItem->Value;

			// if the value is null return null
			if (vtValue.vt == VT_NULL)
			{
				return __nullptr;
			}
			// Make sure the data is an array of bytes
			if (vtValue.vt != (VT_ARRAY | VT_UI1))
			{
				UCLIDException ue("ELI37183", "Must be a SAFEARRAY of Bytes.");
				throw ue;
			}
			SAFEARRAY *psaData = vtValue.parray;
	
			return readObjFromSAFEARRAY(psaData);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37211");
	}
	catch(UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void setIPersistObjToField(const FieldsPtr& ipFields, const string& strFieldName, IPersistStreamPtr ipObj)
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI37204", ipFields != __nullptr );

		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI37205", ipItem != __nullptr );

			// set the variant type to the given value
			variant_t vtItem;

            // if the object is null set the field to null
			if (ipObj == __nullptr)
			{
				vtItem.vt = VT_NULL;
			}
			else
			{
				vtItem.parray = writeObjToSAFEARRAY(ipObj); 
				vtItem.vt = VT_ARRAY | VT_UI1;			
			}

			// Set the field to the variant type
			ipItem->Value = vtItem;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37206");
	}
	catch(UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		throw ue;
	}
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
SYSTEMTIME getSQLServerDateTimeAsSystemTime(const _ConnectionPtr& ipDBConnection)
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
string createConnectionString(const string& strServer, const string& strDatabase,
	const string& strAdvancedConnectionStringProperties, bool bAllowEmptyDB/* = false*/)
{
	ASSERT_ARGUMENT("ELI17471", bAllowEmptyDB || !strServer.empty());
	ASSERT_ARGUMENT("ELI17472", bAllowEmptyDB || !strDatabase.empty());

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

	// If anything is specified in strAdvancedConnectionStringProperties, update the
	// strConnectionString with the advanced values.
	if (!strAdvancedConnectionStringProperties.empty())
	{
		updateConnectionStringProperties(strConnectionString, strAdvancedConnectionStringProperties);
	}

	return strConnectionString;
}
//-------------------------------------------------------------------------------------------------
csis_map<string>::type getConnectionStringProperties(const string& strConnectionString)
{
	// A map that provides case-insensitive lookup of property names.
	csis_map<string>::type mapProperties;
	
	// Split out and interate through each property in the connection string.
	vector<string> vecTokens;
	StringTokenizer	s(';');
	s.parse(strConnectionString, vecTokens);

	for (size_t i = 0; i < vecTokens.size(); i++)
	{
		string strVariable = vecTokens[i];
		if (strVariable.empty())
		{
			continue;
		}

		// Extract the name and value of each property.
		int nSplitPos = strVariable.find('=');
		if (nSplitPos == string::npos)
		{
			UCLIDException ue("ELI35142", "Error parsing connection string");
			ue.addDebugInfo("Connection string", strConnectionString, true);
			ue.addDebugInfo("Variable", strVariable, true);
			throw ue;
		}

		string strName = strVariable.substr(0, nSplitPos);
		string strValue = strVariable.substr(nSplitPos + 1);
		mapProperties[strName] = strValue;
	}

	return mapProperties;
}
//-------------------------------------------------------------------------------------------------
bool findConnectionStringProperty(const string& strConnectionString, const string& strName,
																string *pstrValue /*= __nullptr*/)
{
	csis_map<string>::type mapSourceProperties =
		getConnectionStringProperties(strConnectionString);

	auto itProperty = mapSourceProperties.find(strName);
	if (itProperty == mapSourceProperties.end())
	{
		// The property was not found in the connection string.
		return false;
	}
	else
	{
		// The property was not found; assign the value to pstrValue if non-null.
		if (pstrValue != __nullptr)
		{
			*pstrValue = itProperty->second;
		}
		return true;
	}
}
//-------------------------------------------------------------------------------------------------
void updateConnectionStringProperties(string& rstrConnectionString, const string& strNewProperties)
{
	// Retrieve the properties of both the existing connection string and the new properties.
	csis_map<string>::type mapSourceProperties = getConnectionStringProperties(rstrConnectionString);
	csis_map<string>::type mapNewProperties = getConnectionStringProperties(strNewProperties);

	// The server can be specified with either "Server" or "Data source". If strNewProperties
	// specifies one, don't let the final result contain the other as well.
	if (findConnectionStringProperty(strNewProperties, gstrDATA_SOURCE))
	{
		mapSourceProperties.erase(gstrSERVER);
	}
	else if (findConnectionStringProperty(strNewProperties, gstrSERVER))
	{
		mapSourceProperties.erase(gstrDATA_SOURCE);
	}

	// The database can be specified with either "Database" or "Initial Catalog". If strNewProperties
	// specifies one, don't let the final result contain the other as well.
	if (findConnectionStringProperty(strNewProperties, gstrINITIAL_CATALOG))
	{
		mapSourceProperties.erase(gstrDATABASE);
	}
	else if (findConnectionStringProperty(strNewProperties, gstrDATABASE))
	{
		mapSourceProperties.erase(gstrINITIAL_CATALOG);
	}

	// Determine if any properties have been add or modified in mapNewProperties vs
	// mapSourceProperties.
	bool bModifiedProperty = false;
	for (auto iterNew = mapNewProperties.begin();
		 iterNew != mapNewProperties.end(); iterNew++)
	{
		auto iterSource = mapSourceProperties.find(iterNew->first);
		if (iterSource == mapSourceProperties.end() || (iterSource->second != iterNew->second))
		{
			bModifiedProperty = true;
			break;
		}
	}

	// If no property has been modified, don't re-generate the connection string. The properties
	// will likely end up in a different order and make it hard for callers to determine if there
	// has been a meaningful change to the connection string.
	if (!bModifiedProperty)
	{
		return;
	}

	// Use mapNewProperties as the primary values; but fill in any values from mapSourceProperties
	// that don't exist in mapNewProperties.
	mapNewProperties.insert(mapSourceProperties.begin(), mapSourceProperties.end());

	// Use mapNewProperties to create the updated connection string.
	string strNewConnectionString;
	for (csis_map<string>::type::iterator it = mapNewProperties.begin();
		it != mapNewProperties.end(); it++)
	{
		if (!strNewConnectionString.empty())
		{
			strNewConnectionString += ";";
		}

		strNewConnectionString += it->first;
		strNewConnectionString += "=";
		strNewConnectionString += it->second;
	}

	rstrConnectionString = strNewConnectionString;
}
//-------------------------------------------------------------------------------------------------
long executeCmdQuery(const _ConnectionPtr& ipDBConnection, const string& strSQLQuery,
	bool bDisplayExceptions, long *pnOutputID)
{
	return executeCmdQuery(ipDBConnection, strSQLQuery, "ID", bDisplayExceptions, pnOutputID);
}
//-------------------------------------------------------------------------------------------------
long executeCmdQuery( const _ConnectionPtr& ipDBConnection, 
					  const std::string& strSQLQuery,
					  const std::string& resultColumnName,
					  bool bDisplayExceptions, 
					  long *pnOutputID )
{
	ASSERT_ARGUMENT( "ELI46755", ipDBConnection != nullptr );

	_RecordsetPtr ipResult(__nullptr);

	variant_t vtRecordsAffected = 0L;
	try
	{
		try
		{
			if ( pnOutputID == nullptr )
			{
				ipDBConnection->Execute( strSQLQuery.c_str(),
										 &vtRecordsAffected, 
										 adCmdText | adExecuteNoRecords );
			}
			else
			{
				ASSERT_ARGUMENT( "ELI46756", !resultColumnName.empty() );
				ipResult = ipDBConnection->Execute( strSQLQuery.c_str(), 
																  nullptr, 
																  adCmdUnknown );
				ASSERT_RESOURCE_ALLOCATION( "ELI46757", ipResult != nullptr );

				// If pnOutputID is provided, it is assumed the query will return a 
				// single record, with a field name contained in resultColumName.
				ipResult->MoveFirst();
				*pnOutputID = getLongField( ipResult->Fields, resultColumnName );
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION( "ELI46758" );
	}
	catch( UCLIDException& ue )
	{
		ue.addDebugInfo( "SQL", strSQLQuery, true );
		
		if (ipResult != __nullptr)
		{
			UCLIDException uexOuter = UCLIDException("ELI43266", "Record not found", ue);
			ue = uexOuter;
		}

		if ( !bDisplayExceptions )
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
long executeCmdQuery( const _ConnectionPtr& ipDBConnection, 
					  const std::string& strSQLQuery,
					  const std::string& resultColumnName,
					  bool bDisplayExceptions, 
					  long long *pnOutputID )
{
	ASSERT_ARGUMENT( "ELI38681", ipDBConnection != nullptr );

	variant_t vtRecordsAffected = 0L;
	try
	{
		try
		{
			if ( pnOutputID == nullptr )
			{
				ipDBConnection->Execute( strSQLQuery.c_str(),
										 &vtRecordsAffected, 
										 adCmdText | adExecuteNoRecords );
			}
			else
			{
				ASSERT_ARGUMENT( "ELI38682", !resultColumnName.empty() );
				_RecordsetPtr ipResult = ipDBConnection->Execute( strSQLQuery.c_str(), 
																  nullptr, 
																  adCmdUnknown );
				ASSERT_RESOURCE_ALLOCATION( "ELI38683", ipResult != nullptr );

				// If pnOutputID is provided, it is assumed the query will return a 
				// single record, with a field name contained in resultColumName.
				ipResult->MoveFirst();
				*pnOutputID = getLongLongField( ipResult->Fields, resultColumnName );
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION( "ELI38684" );
	}
	catch( UCLIDException& ue )
	{
		ue.addDebugInfo( "SQL", strSQLQuery, true );

		if ( !bDisplayExceptions )
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

			// [LegacyRCAndUtils:6154]
			// Since IDENT_CURRENT can return the wrong ID when multiple processes are updating the
			// same table and there is a known bug with SCOPE_IDENTITY() and @@IDENTITY, re-query to
			// get the ID of the newly added row.
			return getKeyID(ipDBConnection,strTable, strKeyCol, rstrKey, bAddKey);
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
	executeCmdQuery(ipDBConnection, strDropSQL, false);
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
		string strFKTableName = getStringField(ipConstraints->Fields, "FK_TABLE_NAME");
		string strPKTableName = getStringField(ipConstraints->Fields, "PK_TABLE_NAME");
		
		// Check if it is our table
		if (vectorContainsElement(vecTables, strFKTableName) || vectorContainsElement(vecTables, strPKTableName ))
		{
			// Get the name of the Foreign key
			string strConstraintName = getStringField(ipConstraints->Fields, "FK_NAME");

			// Drop the Foreign key
			dropConstraint(ipDBConnection, strFKTableName, strConstraintName);
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
FAMUTILS_API void getDatabaseInfo(const _ConnectionPtr& ipDBConnection, const string &strDBName,
	string &strServerName, string &strCreateDate, string &strLastRestoreDate, bool &isCluster)
{
	try
	{
		string clusterName = getClusterName(ipDBConnection);
        isCluster = !clusterName.empty();

		string strQuery = "select db.name, @@ServerName as ServerName, convert(nvarchar(30), db.create_date,121) as create_date, "
			" convert(nvarchar(30), coalesce( max(rh.restore_date), db.create_date), 121) as restore_date "
			"from master.sys.databases db "
			"LEFT JOIN msdb.dbo.restorehistory rh on db.name = rh.destination_database_name "
			"group by db.name, db.create_date "
			"having name = '"+ strDBName + "'";

		_RecordsetPtr result = ipDBConnection->Execute(strQuery.c_str(), NULL, adCmdText);
		if (!result->adoEOF)
		{
			// Get the fields pointer
			FieldsPtr ipFields = result->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI40288", ipFields != __nullptr );

			strCreateDate = getStringField(ipFields, "create_date");
			strLastRestoreDate = getStringField(ipFields, "restore_date");
			strServerName = (clusterName.empty()) ? getStringField(ipFields, "ServerName"): clusterName;
		}
		else
		{
			UCLIDException ue ("ELI38758", "Unable to get Database creation date.");
			ue.addDebugInfo("Database name", strDBName);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38757");
}

//-------------------------------------------------------------------------------------------------
FAMUTILS_API void getDatabaseInfo(const _ConnectionPtr& ipDBConnection, const string &strDBName,
	string &strServerName, SYSTEMTIME &ctCreateDate, SYSTEMTIME &ctLastRestoreDate, bool &isCluster)
{
	try
	{
		string clusterName = getClusterName(ipDBConnection);
        isCluster = !clusterName.empty();

		string strQuery = "select db.name, @@ServerName as ServerName, create_date, "
			" coalesce( max(rh.restore_date), db.create_date) as restore_date "
			"from master.sys.databases db "
			"LEFT JOIN msdb.dbo.restorehistory rh on db.name = rh.destination_database_name "
			"group by db.name, db.create_date "
			"having name = '"+ strDBName + "'";

		_RecordsetPtr result = ipDBConnection->Execute(strQuery.c_str(), NULL, adCmdText);

		if (!result->adoEOF)
		{
			// Get the fields pointer
			FieldsPtr ipFields = result->Fields;
			ASSERT_RESOURCE_ALLOCATION("ELI38806", ipFields != __nullptr );

			ctCreateDate = getTimeDateField(ipFields, "create_date");
			ctLastRestoreDate = getTimeDateField(ipFields, "restore_date");
			strServerName = (clusterName.empty()) ? getStringField(ipFields, "ServerName") : clusterName;
		}
		else
		{
			UCLIDException ue ("ELI38807", "Unable to get Database creation date.");
			ue.addDebugInfo("Database name", strDBName);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38808");
}
//-------------------------------------------------------------------------------------------------
FAMUTILS_API void createDatabaseID(const _ConnectionPtr& ipConnection, ByteStream &bsDatabaseID)
{
	try
	{
		SYSTEMTIME stDBCreatedDate;
		SYSTEMTIME stDBRestoreDate;
		string strServer;

		string strDBName = ipConnection->DefaultDatabase;

		// Make sure the bytestream is empty
		bsDatabaseID.setSize(0);

		ByteStreamManipulator bsm(ByteStreamManipulator::kWrite, bsDatabaseID);

		bool clustered;
		getDatabaseInfo(ipConnection, strDBName, strServer, stDBCreatedDate, stDBRestoreDate, clustered);
		
		GUID guidDatabaseID;
		CoCreateGuid(&guidDatabaseID);
		SYSTEMTIME stLastUpdateTime = getSQLServerDateTimeAsSystemTime(ipConnection);

		TIME_ZONE_INFORMATION tzi;
		bool daylight = (GetTimeZoneInformation(&tzi) == TIME_ZONE_ID_DAYLIGHT);
		long nTimeZoneBias = daylight
			? tzi.Bias + tzi.DaylightBias
			: tzi.Bias + tzi.StandardBias;

		// Put the values in the ByteStream;
		bsm << guidDatabaseID;
		bsm << strServer;
		bsm << strDBName;
		bsm << stDBCreatedDate;
		bsm << stDBRestoreDate;
		bsm << stLastUpdateTime;
		// Flush in multiple of 8 bytes because it will need to be encrypted
		bsm.flushToByteStream(8);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38900");
}
//-------------------------------------------------------------------------------------------------
FAMUTILS_API bool isNULL(const FieldsPtr& ipFields, const string& strFieldName)
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI38932", ipFields != __nullptr );

		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI38933", ipItem != __nullptr );

			// get the value
			variant_t vtItem = ipItem->Value;
			
			return vtItem.vt == VT_NULL;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38935");
	}
	catch(UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void setFieldToNull(const FieldsPtr& ipFields, const string& strFieldName)
{
	// Use double try catch so that the field name can be added to the debug info
	try
	{
		// Make user ipFields is not NULL
		ASSERT_ARGUMENT("ELI41913", ipFields != __nullptr);

		try
		{
			// Get the Field from the fields list
			FieldPtr ipItem = ipFields->Item[strFieldName.c_str()];
			ASSERT_RESOURCE_ALLOCATION("ELI41914", ipItem != __nullptr);

			variant_t vtItem;
			vtItem.vt = VT_NULL;

			// set the value to the variant
			ipItem->Value = vtItem;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41915");
	}
	catch (UCLIDException& ue)
	{
		// Add FieldName to the debug info
		ue.addDebugInfo("FieldName", strFieldName);
		throw ue;
	}
}
