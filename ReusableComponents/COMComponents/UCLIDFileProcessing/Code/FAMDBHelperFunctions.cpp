
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
		vector<string> vecTokens;
		StringTokenizer::sGetTokens(vecCreationQueries[i], " )(", vecTokens, true);
		bool foundCreateStatement = false;
		for (int j = 0, vecTokensSize = vecTokens.size(); j < vecTokensSize; j++)
		{
			if (vecTokensSize - j > 3 && vecTokens[j] == "CREATE" && vecTokens[j+1] == "TABLE")
			{
				// Trim enclosing braces as well as any "dbo." prefix.
				string strTableName = trim(vecTokens[j + 2], "[", "]");
				if (strTableName.length() > 3 && _stricmp(strTableName.substr(0, 3).c_str(), "dbo") == 0)
				{
					strTableName = trim(strTableName.substr(3), "][.", "]");
				}

				vecTableNames[i] = strTableName;

				foundCreateStatement = true;
				break;
			}
		}

		if (!foundCreateStatement)
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