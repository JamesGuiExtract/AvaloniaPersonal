#include "StdAfx.h"
#include "ActionStatusCondition.h"
#include "ActionStatusConditionDlg.h"

#include <UCLIDException.h>
#include <COMUtils.h>

//--------------------------------------------------------------------------------------------------
// ActionStatusCondition
//--------------------------------------------------------------------------------------------------
ActionStatusCondition::ActionStatusCondition(void)
{
}
//--------------------------------------------------------------------------------------------------
ActionStatusCondition::ActionStatusCondition(const ActionStatusCondition &settings)
{
	*this = settings;
}

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
bool ActionStatusCondition::configure(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
									  const string& strQueryHeader)
{
	ActionStatusCondition settings(*this);
	ActionStatusConditionDlg dialog(ipFAMDB, settings);
	if (dialog.DoModal() == IDOK)
	{
		*this = dialog.getSettings();
		return true;
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
SelectFileCondition* ActionStatusCondition::clone()
{
	ActionStatusCondition *pClone = new ActionStatusCondition();
	*pClone = *this;
	return pClone;
}
//--------------------------------------------------------------------------------------------------
string ActionStatusCondition::getSummaryString(bool bFirstCondition)
{
	string strSummary = "for which the \"" + m_strAction + "\" action's status is \""
		+ m_strStatus + "\"";

	strSummary += " by " + m_strUser;

	return strSummary;
}
//--------------------------------------------------------------------------------------------------
string ActionStatusCondition::buildQuery(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
										 const string& strSelect, long nWorkflowID)
{
	ASSERT_ARGUMENT("ELI33785", ipFAMDB != __nullptr);
	
	string strQuery = "SELECT " + strSelect + " FROM FAMFile WITH (NOLOCK) ";

	long nActionID = ipFAMDB->GetActionIDForWorkflow(m_strAction.c_str(), nWorkflowID);

	// Get the status as a string
	string strStatus = ipFAMDB->AsStatusString((UCLID_FILEPROCESSINGLib::EActionStatus)m_nStatus);

	strQuery += " LEFT JOIN FileActionStatus WITH (NOLOCK) ON FAMFile.ID = FileActionStatus.FileID "
		" AND FileActionStatus.ActionID = " + asString(nActionID);
	if (!m_strUser.empty() && m_strUser != gstrANY_USER)
	{
		if (m_strUser == gstrNO_USER)
		{
			strQuery += " AND UserID IS NULL";
		}
		else
		{
			// Get all of the users 
			IStrToStrMapPtr ipUsers = ipFAMDB->GetFamUsers();
			if (ipUsers->Contains(m_strUser.c_str()))
			{
				strQuery += " AND UserID = " + asString(ipUsers->GetValue(m_strUser.c_str()));
			}
			else
			{
				UCLIDException ue("ELI53356", "FAM user not found in database!");
				ue.addDebugInfo("FAMUser", m_strUser);
				throw ue;
			}
		}
	}
	strQuery += " WHERE (";

	// [LRCAU #5942] - Files are no longer marked as unattempted due to the
	// database normalization changes. A file is unattempted for a particular
	// action if it does not contain an entry in the FileActionStatus table.
	if (m_nStatus == UCLID_FILEPROCESSINGLib::kActionUnattempted)
	{
		strQuery += "FileActionStatus.FileID IS NULL)";
	}
	else
	{
		strQuery += "FileActionStatus.ActionStatus = '"
			+ strStatus + "')";
	}

	return strQuery;
}
//--------------------------------------------------------------------------------------------------
