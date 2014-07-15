
#include "stdafx.h"
#include "FAMDBHelperFunctions.h"
#include "FAMUtilsConstants.h"

#include <UCLIDException.h>
#include <StringTokenizer.h>

//--------------------------------------------------------------------------------------------------
vector<string> getTableNamesFromCreationQueries(vector<string> vecCreationQueries)
{
	int count = vecCreationQueries.size();
	vector<string> vecTableNames(count);

	for (int i = 0; i < count; i++)
	{
		// Assuming the first two words in the query are "Create" and "Table", the 3rd word will be
		// the table name.
		vector<string> vecTokens;
		StringTokenizer::sGetTokens(vecCreationQueries[i], " )(", vecTokens, true);
		if (vecTokens.size() < 3 ||
			_stricmp(vecTokens[0].c_str(), "CREATE") != 0 ||
			_stricmp(vecTokens[1].c_str(), "TABLE") != 0)
		{
			UCLIDException ue("ELI31406", "Expected table create query.");
			ue.addDebugInfo("Query", vecCreationQueries[i], true);
			throw ue;
		}
		
		// Trim enclosing braces as well as any "dbo." prefix.
		string strTableName = trim(vecTokens[2], "[", "]");
		if (strTableName.length() > 3 && _stricmp(strTableName.substr(0, 3).c_str(), "dbo") == 0)
		{
			strTableName = trim(strTableName.substr(3), "][.", "]");
		}

		vecTableNames[i] = strTableName;
	}

	return vecTableNames;
}
//-------------------------------------------------------------------------------------------------
vector<string> getFeatureDefinitionQueries()
{
	vector<string> vecFeatureDefinitions;

	CString zSQL;
	CString zSQLTemplate = "INSERT INTO [Feature] "
		"([Enabled], [FeatureName], [FeatureDescription], [AdminOnly]) VALUES(%u, '%s', '%s', '%u')";

	zSQL.Format(zSQLTemplate, 1, gstrFEATURE_FILE_HANDLER_COPY_NAMES.c_str(),
		"Allows filenames to be copied as text from a file list.", 1);
	vecFeatureDefinitions.push_back((LPCTSTR)zSQL);

	zSQL.Format(zSQLTemplate, 1, gstrFEATURE_FILE_HANDLER_COPY_FILES.c_str(),
		"Allows documents to be copied or dragged as files from a file list.", 1);
	vecFeatureDefinitions.push_back((LPCTSTR)zSQL);

	zSQL.Format(zSQLTemplate, 1, gstrFEATURE_FILE_HANDLER_COPY_FILES_AND_DATA.c_str(),
		"Allows documents and associated data to be copied or dragged as files from a file list.", 1);
	vecFeatureDefinitions.push_back((LPCTSTR)zSQL);

	zSQL.Format(zSQLTemplate, 1, gstrFEATURE_FILE_HANDLER_OPEN_FILE_LOCATION.c_str(),
		"Allows the containing folder of document to be opened in Windows file explorer.", 1);
	vecFeatureDefinitions.push_back((LPCTSTR)zSQL);

	zSQL.Format(zSQLTemplate, 1, gstrFEATURE_FILE_RUN_DOCUMENT_SPECIFIC_REPORTS.c_str(),
		"Allows document specific reports to be run.", 1);
	vecFeatureDefinitions.push_back((LPCTSTR)zSQL);

	return vecFeatureDefinitions;
}
//-------------------------------------------------------------------------------------------------