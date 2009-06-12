
#include "stdafx.h"
#include "SetActionStatusFileProcessorPP.h"

#include <UCLIDException.h>
#include <COMUtils.h>

// these constants should be moved into UCLIDFileProcessing DLL
static const string gstrUNATTEMPTED = "Unattempted";
static const string gstrPENDING = "Pending";
static const string gstrCOMPLETED = "Completed";
static const string gstrFAILED = "Failed";

// other constants
const int giNUM_VALID_STATUSES = 4;
const EActionStatus gVALID_ACTION_STATUSES[giNUM_VALID_STATUSES] = {kActionUnattempted, kActionPending, kActionCompleted, kActionFailed};

//--------------------------------------------------------------------------------------------------
// CSetActionStatusFileProcessorPP
//--------------------------------------------------------------------------------------------------
CSetActionStatusFileProcessorPP::CSetActionStatusFileProcessorPP()
{
	try
	{
		m_dwTitleID = IDS_TITLESETACTIONSTATUSFILEPROCESSORPP;
		m_dwHelpFileID = IDS_HELPFILESETACTIONSTATUSFILEPROCESSORPP;
		m_dwDocStringID = IDS_DOCSTRINGSETACTIONSTATUSFILEPROCESSORPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI15128")
}
//--------------------------------------------------------------------------------------------------
CSetActionStatusFileProcessorPP::~CSetActionStatusFileProcessorPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI15151")
}
//--------------------------------------------------------------------------------------------------
HRESULT CSetActionStatusFileProcessorPP::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CSetActionStatusFileProcessorPP::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// IPropertyPage
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessorPP::Apply()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CSetActionStatusFileProcessorPP::Apply\n"));

		// update the settings in each of the objects associated with this UI
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_FILEPROCESSORSLib::ISetActionStatusFileProcessorPtr ipFP(m_ppUnk[i]);
			ASSERT_RESOURCE_ALLOCATION("ELI15141", ipFP != NULL);

			// retrieve the action name and update it in the underlying object
			CComBSTR bstrActionName;
			m_cmbActionName.GetWindowText(&bstrActionName);
			ipFP->ActionName = _bstr_t(bstrActionName.m_str);

			// retrieve the action status and update it in the underlying object
			int iActionStatusIndex = m_cmbActionStatus.GetCurSel();
			if (iActionStatusIndex == -1 || iActionStatusIndex >= giNUM_VALID_STATUSES)
			{
				UCLIDException ue("ELI15143", "Internal logic error!");
				ue.addDebugInfo("iActionStatusIndex", iActionStatusIndex);
				throw ue;
			}

			ipFP->ActionStatus = gVALID_ACTION_STATUSES[iActionStatusIndex];
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15142")

	// an exception was caught
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// Message handlers
//--------------------------------------------------------------------------------------------------
LRESULT CSetActionStatusFileProcessorPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	try
	{
		// get the underlying objet
		UCLID_FILEPROCESSORSLib::ISetActionStatusFileProcessorPtr ipSetActionStatusFP = m_ppUnk[0];

		if (ipSetActionStatusFP != NULL)
		{
			// bind the combo box controls to the UI controls
			m_cmbActionName = GetDlgItem(IDC_COMBO_ACTION);
			m_cmbActionStatus = GetDlgItem(IDC_COMBO_STATUS);

			// get the action name and action status from the file processor
			string strActionName = ipSetActionStatusFP->ActionName;
			EActionStatus eActionStatus = (EActionStatus) ipSetActionStatusFP->ActionStatus;

			// add entries the combo box for status
			int iDefaultActionStatusIndex = 0;
			for (long i = 0; i < giNUM_VALID_STATUSES; i++)
			{
				EActionStatus eTempActionStatus = gVALID_ACTION_STATUSES[i];
				m_cmbActionStatus.AddString(getActionStatus(eTempActionStatus).c_str());
				if (eTempActionStatus == eActionStatus)
				{
					iDefaultActionStatusIndex = i;
				}
			}

			// create a database manager object so that we can retrieve
			// the actions stored in the database
			IFileProcessingDBPtr ipDB(CLSID_FileProcessingDB);
			ASSERT_RESOURCE_ALLOCATION("ELI15131", ipDB != NULL);

			// Connect database using last used settings in this instance
			ipDB->ConnectLastUsedDBThisProcess();

			// add entries to the combo box for actions
			IStrToStrMapPtr ipActionIDToNameMap = ipDB->GetActions();
			ASSERT_RESOURCE_ALLOCATION("ELI15132", ipActionIDToNameMap != NULL);
			for (long i = 0; i < ipActionIDToNameMap->Size; i++)
			{
				CComBSTR bstrKey, bstrValue;
				ipActionIDToNameMap->GetKeyValue(i, &bstrKey, &bstrValue);
				m_cmbActionName.AddString(asString(bstrKey).c_str());
			}

			// Ensure at least one action exists
			if(m_cmbActionName.GetCount() > 0)
			{
				// Check if an action has been specified.
				if(strActionName.empty())
				{
					// No action was specified. Select the first action by default.
					m_cmbActionName.SetCurSel(0);
				}
				else
				{
					// An action was specified. Find it by name from the list. [P13 #4967]
					int iIndex = m_cmbActionName.FindStringExact(-1, strActionName.c_str());
					if(iIndex == CB_ERR)
					{
						// If the action is not found, display an error message. [P13 #4966] 
						MessageBox(
							("Action not found: " + strActionName + ". Please specify a new action.").c_str(), 
							"Error", MB_OK | MB_ICONEXCLAMATION);
					}
					else
					{
						// Select the current action.
						m_cmbActionName.SetCurSel(iIndex);
					}
				}
			}

			// select the action status associated with this object, or select "Pending"
			// as the default
			if (m_cmbActionStatus.GetCount() > 0)
			{
				m_cmbActionStatus.SetCurSel(iDefaultActionStatusIndex);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15129")

	return TRUE;
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
EActionStatus CSetActionStatusFileProcessorPP::getActionStatus(const string& strActionStatus) const
{
	// this functionality should really be in the UCLID FileProcessing DLL, but that IDL file
	// is currently checked out
	int todo_move_this_functionality_into_uclid_file_processing_dll;

	// return the action status enum corresponding to the action status string
	if (strActionStatus == gstrUNATTEMPTED)
	{
		return kActionUnattempted;
	}
	else if (strActionStatus == gstrPENDING)
	{
		return kActionPending;
	}
	else if (strActionStatus == gstrCOMPLETED)
	{
		return kActionCompleted;
	}
	else if (strActionStatus == gstrFAILED)
	{
		return kActionFailed;
	}
	else
	{
		UCLIDException ue("ELI15140", "Internal logic error!");
		ue.addDebugInfo("strActionStatus", strActionStatus);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
const std::string& CSetActionStatusFileProcessorPP::getActionStatus(EActionStatus eActionStatus) const
{
	// this functionality should really be in the UCLID FileProcessing DLL, but that IDL file
	// is currently checked out
	int todo_move_this_functionality_into_uclid_file_processing_dll;

	// return the action status string corresponding to the action status enum
	switch (eActionStatus)
	{
	case kActionUnattempted:
		return gstrUNATTEMPTED;

	case kActionPending:
		return gstrPENDING;

	case kActionCompleted:
		return gstrCOMPLETED;

	case kActionFailed:
		return gstrFAILED;

	default:
		{
			// we should never reach here
			UCLIDException ue("ELI15139", "Internal logic error!");
			ue.addDebugInfo("ActionStatus", (long) eActionStatus);
			throw ue;
		}
	}
}
//--------------------------------------------------------------------------------------------------
