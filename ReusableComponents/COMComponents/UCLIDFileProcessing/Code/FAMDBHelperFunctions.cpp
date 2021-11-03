
#include "stdafx.h"
#include "FAMDBHelperFunctions.h"
#include "FAMUtilsConstants.h"

#include <ADOUtils.h>
#include <UCLIDException.h>
#include <StringTokenizer.h>
#include <regex>

//--------------------------------------------------------------------------------------------------
vector<string> getTableNamesFromCreationQueries(vector<string> vecCreationQueries)
{
	int count = vecCreationQueries.size();
	vector<string> vecTableNames(count);
	// Pattern to match CREATE TABLE [dbo].[TableName], with or without quoting and namespace, or other create/alter statement
	// Sub-match #1 will have the table name if the match is a create table statement, else it will be empty
	string strCreateTablePattern = "\\b(?:CREATE\\s+TABLE\\s+(?:\\W?\\w+\\W?\\.)?\\W?(\\w+)|CREATE\\s+SCHEMA|ALTER\\s+TABLE)\\b";
	regex rgxFindCreateTableName(strCreateTablePattern, std::regex_constants::icase);
	for (int i = 0; i < count; i++)
	{
		smatch subMatches;
		if (regex_search(vecCreationQueries[i], subMatches, rgxFindCreateTableName))
		{
			vecTableNames[i] = subMatches[1];
		}
		else
		{
			UCLIDException ue("ELI31406", "Expected table create query.");
			ue.addDebugInfo("Query", vecCreationQueries[i], true);
			throw ue;
		}
	}
	return vecTableNames;
}
//-------------------------------------------------------------------------------------------------
vector<string> getFeatureDefinitionQueries(int nSchemaVersion/* = -1*/)
{
	vector<string> vecFeatureDefinitions;

	CString zSQL;
	CString zSQLTemplate = "INSERT INTO [Feature] "
		"([Enabled], [FeatureName], [FeatureDescription], [AdminOnly]) VALUES(%u, '%s', '%s', '%u')";

	if (nSchemaVersion == -1 || nSchemaVersion >= 116)
	{
		zSQL.Format(zSQLTemplate, 1, gstrFEATURE_FILE_HANDLER_COPY_NAMES.c_str(),
			"Allows filenames to be copied as text from a file list.", 1);
		vecFeatureDefinitions.push_back((LPCTSTR)zSQL);

		zSQL.Format(zSQLTemplate, 1, gstrFEATURE_FILE_HANDLER_COPY_FILES.c_str(),
			"Allows documents to be copied or dragged as files from a file list.", 1);
		vecFeatureDefinitions.push_back((LPCTSTR)zSQL);

		zSQL.Format(zSQLTemplate, 1, gstrFEATURE_FILE_HANDLER_COPY_FILES_AND_DATA.c_str(),
			"Allows documents and associated data to be copied or dragged as files from a file list.", 1);
		vecFeatureDefinitions.push_back((LPCTSTR)zSQL);

		if (nSchemaVersion == -1 || nSchemaVersion >= 117)
		{
			zSQL.Format(zSQLTemplate, 1, gstrFEATURE_FILE_HANDLER_OPEN_FILE_LOCATION.c_str(),
				"Allows the containing folder of document to be opened in Windows file explorer.", 1);
			vecFeatureDefinitions.push_back((LPCTSTR)zSQL);
		}

		zSQL.Format(zSQLTemplate, 1, gstrFEATURE_FILE_RUN_DOCUMENT_SPECIFIC_REPORTS.c_str(),
			"Allows document specific reports to be run.", 1);
		vecFeatureDefinitions.push_back((LPCTSTR)zSQL);
	}

	return vecFeatureDefinitions;
}
//-------------------------------------------------------------------------------------------------
long getDefaultSessionTimeoutFromWebConfig(_ConnectionPtr ipConnection)
{
	long nDefaultSessionTimeout = 0;

	static regex regexInactivityTimout(
		"(\"InactivityTimeout\":\\s*)(\\d+)"
		, std::regex_constants::icase);

	// Create a pointer to a recordset
	_RecordsetPtr ipWebAppConfigSet(__uuidof(Recordset));
	ASSERT_RESOURCE_ALLOCATION("ELI51969", ipWebAppConfigSet != __nullptr);

	ipWebAppConfigSet->Open("SELECT [Settings] FROM [WebAppConfig]",
		_variant_t((IDispatch*)ipConnection, true), adOpenStatic,
		adLockOptimistic, adCmdText);

	while (!asCppBool(ipWebAppConfigSet->adoEOF))
	{
		string strConfig = getStringField(ipWebAppConfigSet->Fields, "Settings");
		smatch subMatches;
		if (regex_search(strConfig, subMatches, regexInactivityTimout))
		{
			long nTimeout = asLong(subMatches[2]) * 60; // (minutes to seconds)
			if (nTimeout > 0
				&& (nDefaultSessionTimeout == 0 || nTimeout < nDefaultSessionTimeout))
			{
				nDefaultSessionTimeout = nTimeout;
			}
		}

		ipWebAppConfigSet->MoveNext();
	}

	return nDefaultSessionTimeout;
}