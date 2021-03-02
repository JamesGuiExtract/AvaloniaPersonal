#include "stdafx.h"
#include "SpecifiedFilesCondition.h"
#include "SpecifiedFilesConditionDlg.h"

#include <cpputil.h>
#include <Misc.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// SpecifiedFilesCondition
//--------------------------------------------------------------------------------------------------
SpecifiedFilesCondition::SpecifiedFilesCondition(void)
: m_eFileListSource(eSpecifiedFiles)
{
}
//--------------------------------------------------------------------------------------------------
SpecifiedFilesCondition::SpecifiedFilesCondition(const SpecifiedFilesCondition &settings)
{
	*this = settings;
}
//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
bool SpecifiedFilesCondition::configure(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
								  const string& strQueryHeader)
{
	SpecifiedFilesCondition settings(*this);
	SpecifiedFilesConditionDlg dialog(ipFAMDB, settings);
	if (dialog.DoModal() == IDOK)
	{
		*this = dialog.getSettings();
		return true;
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
SelectFileCondition* SpecifiedFilesCondition::clone()
{
	SpecifiedFilesCondition *pClone = new SpecifiedFilesCondition();
	*pClone = *this;
	return pClone;
}
//--------------------------------------------------------------------------------------------------
string SpecifiedFilesCondition::getSummaryString(bool bFirstCondition)
{
	string strSummary;
	switch (m_eFileListSource)
	{
		case eSpecifiedFiles:	strSummary = "that are in the specified set of files";
								break;
		case eListFile:			strSummary = "that are listed in the file \"" + 
								getFileNameFromFullPath(m_strListFileName) + "\"";
								break;
	}

	return strSummary;
}
//--------------------------------------------------------------------------------------------------
string SpecifiedFilesCondition::buildQuery(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB, 
									 const string& strSelect, long nWorkflowID)
{
	string strQuery;
	switch (m_eFileListSource)
	{
		case eSpecifiedFiles:
		{
			vector<string> vecWhereExpressions;
			for each (string strFileName in m_vecSpecifiedFiles)
			{
				// Escape each file name for use in a SQL LIKE expression.
				// ">" will be the escape char, while * and ? are the supported wildcards that
				// be translated to SQL wildcards.
				replaceVariable(strFileName, "%", ">%");
				replaceVariable(strFileName, "_", ">_");
				replaceVariable(strFileName, "[", ">[");
				replaceVariable(strFileName, "]", ">]");
				replaceVariable(strFileName, "'", ">'");
				replaceVariable(strFileName, "*", "%");
				replaceVariable(strFileName, "?", "_");

				vecWhereExpressions.push_back(
					"[FileName] LIKE '" + strFileName + "' ESCAPE '>'");
			}
			string strWhereClause = asString(vecWhereExpressions, false, " OR ");

			strQuery = "SELECT " + strSelect + " FROM FAMFile WHERE " + strWhereClause;
		}
		break;

		case eListFile:
		{
			vector<string> vecFileList = convertFileToLines(m_strListFileName);

			strQuery = "SELECT " + strSelect + " FROM FAMFile WHERE [FileName] IN ('"
				+ asString(vecFileList, false, "','") + "')";
		}
		break;
	}

	return strQuery;
}

