#include "StdAfx.h"
#include "UCLIDFileProcessing.h"
#include "FilePriorityCondition.h"
#include "FilePriorityConditionDlg.h"

#include <cppUtil.h>
#include <UCLIDException.h>
#include <COMUtils.h>

//--------------------------------------------------------------------------------------------------
// FilePriorityCondition
//--------------------------------------------------------------------------------------------------
FilePriorityCondition::FilePriorityCondition(void)
{
}
//--------------------------------------------------------------------------------------------------
FilePriorityCondition::FilePriorityCondition(const FilePriorityCondition &settings)
{
	*this = settings;
}

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
bool FilePriorityCondition::configure(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
									  const string& strQueryHeader)
{
	FilePriorityCondition settings(*this);
	FilePriorityConditionDlg dialog(ipFAMDB, settings);
	if (dialog.DoModal() == IDOK)
	{
		*this = dialog.getSettings();
		return true;
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
SelectFileCondition* FilePriorityCondition::clone()
{
	FilePriorityCondition *pClone = new FilePriorityCondition();
	*pClone = *this;
	return pClone;
}
//--------------------------------------------------------------------------------------------------
string FilePriorityCondition::getSummaryString(bool bFirstCondition)
{
	string strSummary = bFirstCondition ? "with a " : "that have a ";
	strSummary += "processing priority of " + m_strPriority;

	return strSummary;
}
//--------------------------------------------------------------------------------------------------
string FilePriorityCondition::buildQuery(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB, 
										 const string& strSelect, long nWorkflowID)
{
	// Currently there is no workflow-specific priorities.

	string strQuery = "SELECT " + strSelect + " FROM FAMFile WHERE FAMFile.Priority = "
		+ asString((long)m_ePriority);

	return strQuery;
}
//--------------------------------------------------------------------------------------------------
void FilePriorityCondition::setPriority(UCLID_FILEPROCESSINGLib::EFilePriority ePriority)
{
	m_ePriority = ePriority;

	// Create a database object
	UCLID_FILEPROCESSINGLib::IFAMDBUtilsPtr ipFAMDBUtils(CLSID_FAMDBUtils);
	ASSERT_RESOURCE_ALLOCATION("ELI34529", ipFAMDBUtils != __nullptr);
	UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipDB(ipFAMDBUtils->GetFAMDBProgId().operator LPCSTR());
	ASSERT_RESOURCE_ALLOCATION("ELI33804", ipDB != __nullptr);
	m_strPriority = asString(ipDB->AsPriorityString(ePriority));
}
