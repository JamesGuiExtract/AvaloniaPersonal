#include "StdAfx.h"
#include "QueryCondition.h"
#include "QueryConditionDlg.h"

#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// QueryCondition
//--------------------------------------------------------------------------------------------------
QueryCondition::QueryCondition(void)
{
}
//--------------------------------------------------------------------------------------------------
QueryCondition::QueryCondition(const QueryCondition &settings)
{
	*this = settings;
}

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
bool QueryCondition::configure(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
							   const string& strQueryHeader)
{
	QueryCondition settings(*this);
	QueryConditionDlg dialog(ipFAMDB, settings, strQueryHeader);
	if (dialog.DoModal() == IDOK)
	{
		*this = dialog.getSettings();
		return true;
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
SelectFileCondition* QueryCondition::clone()
{
	QueryCondition *pClone = new QueryCondition();
	*pClone = *this;
	return pClone;
}
//--------------------------------------------------------------------------------------------------
string QueryCondition::getSummaryString(bool bFirstCondition)
{
	string strSummary = bFirstCondition ? "" : "that are ";
	strSummary += "selected from FAMFile by the query part: \"" + m_strSQL + "\"";

	return strSummary;
}
//--------------------------------------------------------------------------------------------------
string QueryCondition::buildQuery(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
								  const string& strSelect, long nWorkflowID)
{
	string strQuery = "SELECT " + strSelect + " FROM FAMFile WITH (NOLOCK) " += m_strSQL;

	return strQuery;
}