#include "StdAfx.h"
#include "FileTagCondition.h"
#include "FileTagConditionDlg.h"

#include <cppUtil.h>
#include <UCLIDException.h>
#include <COMUtils.h>
#include <FAMUtilsConstants.h>

//--------------------------------------------------------------------------------------------------
// FileTagCondition 
//--------------------------------------------------------------------------------------------------
FileTagCondition::FileTagCondition (void)
{
}
//--------------------------------------------------------------------------------------------------
FileTagCondition::FileTagCondition(const FileTagCondition &settings)
{
	*this = settings;
}

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
bool FileTagCondition::configure(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
								 const string& strQueryHeader)
{
	FileTagCondition settings(*this);
	FileTagConditionDlg dialog(ipFAMDB, settings);
	if (dialog.DoModal() == IDOK)
	{
		*this = dialog.getSettings();
		return true;
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
SelectFileCondition* FileTagCondition::clone()
{
	FileTagCondition *pClone = new FileTagCondition();
	*pClone = *this;
	return pClone;
}
//--------------------------------------------------------------------------------------------------
string FileTagCondition::getSummaryString(bool bFirstCondition)
{
	string strSummary = bFirstCondition ? "" : "that are ";
				
	strSummary += "associated with ";
	switch(m_eTagType)
	{
	case eAnyTag:
		strSummary += "any";
		break;
	case eAllTag:
		strSummary += "all";
		break;
	case eNoneTag:
		strSummary += "none";
		break;
	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI33812");
	}
	strSummary += " of the following tags: ";
	string strTagString = "";
	for (vector<string>::iterator it = m_vecTags.begin(); it != m_vecTags.end(); it++)
	{
		if (!strTagString.empty())
		{
			strTagString += ", ";
		}
		strTagString += (*it);
	}
	strSummary += strTagString;

	return strSummary;
}
//--------------------------------------------------------------------------------------------------
string FileTagCondition::buildQuery(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
									const string& strSelect, long nWorkflowID)
{
	string strQuery = "SELECT " + strSelect + " FROM ";

	// Get the size and ensure there is at least 1 tag
	size_t nSize = m_vecTags.size();
	if (nSize == 0)
	{
		UCLIDException uex("ELI33813", "No tags specified.");
		throw uex;
	}

	string strMainQueryTemp = gstrQUERY_FILES_WITH_TAGS;

	// Get the conjunction for the where clause (want the "any" behavior for
	// both the "any" and "none" case - to achieve none just negate the any)
	string strConjunction =
		m_eTagType == eAnyTag || m_eTagType == eNoneTag ? "\nUNION\n" : "\nINTERSECT\n";

	// For the "none" case select all files NOT in the "any" list
	if (m_eTagType == eNoneTag)
	{
		strQuery +=
			"(SELECT " + strSelect + " FROM [FAMFile] WITH (NOLOCK) WHERE [FAMFile].[ID] NOT IN ";

		// The strMainQueryTemp will be used to select the file ids so it needs to 
		// just return File ID's
		replaceVariable(strMainQueryTemp, gstrTAG_QUERY_SELECT,
			"FAMFile.[ID]");
	}
	else
	{
		// The strMainQueryTemp is the section so it needs to return FileName's
		replaceVariable(strMainQueryTemp, gstrTAG_QUERY_SELECT,
			strSelect);
	}

	strQuery += "(" + strMainQueryTemp;
	replaceVariable(strQuery, gstrTAG_NAME_VALUE, m_vecTags[0]);

	// Build the rest of the query
	for (size_t i=1; i < nSize; i++)
	{
		string strTemp = strMainQueryTemp;
		replaceVariable(strTemp, gstrTAG_NAME_VALUE, m_vecTags[i]);
		strQuery += strConjunction + strTemp;
	}

	// Need to add an extra paren in the "none" case
	if (m_eTagType == eNoneTag)
	{
		strQuery += ")";
	}
	strQuery += ") AS FAMFile";

	return strQuery;
}