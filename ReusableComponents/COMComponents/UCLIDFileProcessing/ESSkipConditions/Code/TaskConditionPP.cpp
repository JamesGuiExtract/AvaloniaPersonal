// TaskConditionPP.cpp : Implementation of CTaskConditionPP

#include "stdafx.h"
#include "TaskConditionPP.h"
#include "..\..\Code\FPCategories.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CTaskConditionPP
//-------------------------------------------------------------------------------------------------
CTaskConditionPP::CTaskConditionPP() :
	m_ipTaskMap(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CTaskConditionPP::~CTaskConditionPP()
{
	try
	{
		m_ipTaskMap = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20106");
}
//-------------------------------------------------------------------------------------------------
HRESULT CTaskConditionPP::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CTaskConditionPP::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CTaskConditionPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Obtain interface pointer to the ITaskCondition class
		EXTRACT_FAMCONDITIONSLib::ITaskConditionPtr ipTaskCondition = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI20107", ipTaskCondition);

		// Map controls to member variables
		m_cmbTasks			= GetDlgItem(IDC_COMBO_TASK_SELECT);
		m_btnConfigure		= GetDlgItem(IDC_BTN_CONFIGURE);
		m_txtMustConfigure	= GetDlgItem(IDC_STATIC_CONFIGURE);
		m_chkLogExceptions	= GetDlgItem(IDC_LOG_EXCEPTIONS);

		// Retrieve a count of the number of registered IFileProcessingTask objects
		long nCount = getTaskMap()->Size;

		// If no objects are found that meet the specified criteria, throw an
		// exception
		if (nCount == 0)
		{
			throw UCLIDException("ELI20125", "No qualifying file processing tasks found!");
		}

		// Populate the combo box
		for (long i = 0; i < nCount; i++)
		{	
			_bstr_t bstrName, bstrProgID;
			getTaskMap()->GetKeyValue(i, bstrName.GetAddress(), bstrProgID.GetAddress());

			m_cmbTasks.AddString(asString(bstrName).c_str());
		}

		if(ipTaskCondition->Task != NULL)
		{
			// Select the currently configured task
			ICategorizedComponentPtr ipObject(ipTaskCondition->Task);
			ASSERT_RESOURCE_ALLOCATION("ELI20122", ipObject != NULL);

			_bstr_t bstrName = ipObject->GetComponentDescription();
			m_cmbTasks.SelectString(-1, bstrName);

			updateRequiresConfig(ipTaskCondition->Task);
		}
		else
		{
			// Select first task by default
			m_cmbTasks.SetCurSel(0);

			// Get a pointer to the selected rule object
			IFileProcessingTaskPtr ipNewTask = getSelectedTask();
			ASSERT_RESOURCE_ALLOCATION("ELI20134", ipNewTask != NULL);

			// Update configuration message/button appropriately
			updateRequiresConfig(ipNewTask);
		}

		// Set log exceptions checkbox appropriately
		m_chkLogExceptions.SetCheck(
			asCppBool(ipTaskCondition->LogExceptions) ? BST_CHECKED : BST_UNCHECKED);

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20108");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTaskConditionPP::OnSelChangeTask(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get a pointer to the selected rule object
		IFileProcessingTaskPtr ipNewTask = getSelectedTask();
		ASSERT_RESOURCE_ALLOCATION("ELI20132", ipNewTask != NULL);

		// Update configuration message/button appropriately
		updateRequiresConfig(ipNewTask);

		// Don't assign the object for now... as soon as we do, the user loses configuration settings
		// they had configured for a previously selected object.  Wait until it is absolutely
		// necessary before assigning the selected object (Apply or Configure)
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20135");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CTaskConditionPP::OnBnClickedBtnConfigure(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Obtain interface pointer to the ITaskCondition class
		EXTRACT_FAMCONDITIONSLib::ITaskConditionPtr ipTaskCondition = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI20136", ipTaskCondition);

		// Create the ObjectPropertiesUI object
		IObjectPropertiesUIPtr ipProperties(CLSID_ObjectPropertiesUI);
		ASSERT_RESOURCE_ALLOCATION("ELI20137", ipProperties != NULL);

		// Create a copy of the object for configuration
		ICopyableObjectPtr ipCopyObj(getSelectedTask());
		ASSERT_RESOURCE_ALLOCATION("ELI20138", ipCopyObj != NULL);
		ICategorizedComponentPtr ipCopy = ipCopyObj->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI20139", ipCopy);

		// Create the title for the task configuration box
		string strComponentDesc = asString(ipCopy->GetComponentDescription());
		string strTitle = string( "Configure " ) + strComponentDesc;

		if(asCppBool(ipProperties->DisplayProperties1(ipCopy, strTitle.c_str())))
		{
			// Store the object now the user has applied configuration settings
			IFileProcessingTaskPtr ipConfiguredTask(ipCopy);
			ASSERT_RESOURCE_ALLOCATION("ELI20140", ipConfiguredTask != NULL);

			ipTaskCondition->Task = ipConfiguredTask;

			// Check configuration state of the component
			updateRequiresConfig(ipConfiguredTask);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20123");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskConditionPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ATLTRACE(_T("CTaskConditionPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Obtain interface pointer to the IImageRegionWithLines class
			EXTRACT_FAMCONDITIONSLib::ITaskConditionPtr ipTaskCondition = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI20109", ipTaskCondition != NULL);
		
			// Obtain a pointer to the selected task
			IFileProcessingTaskPtr ipNewTask = getSelectedTask();
			ASSERT_RESOURCE_ALLOCATION("ELI20141", ipNewTask != NULL);

			// Check configuration state of the component
			if (updateRequiresConfig(ipNewTask) == true)
			{
				MessageBox("The selected task has not been configured completely.  "
					       "Please specify all required properties.", "Configuration");

				// Return S_FALSE to prevent apply being committed if the selected object 
				// has not been configured
				return S_FALSE;
			}

			// Assign the selected rule
			ipTaskCondition->Task = ipNewTask;

			// Store whether to log exceptions from the conditional task
			ipTaskCondition->LogExceptions = asVariantBool(m_chkLogExceptions.GetCheck() == BST_CHECKED);
		}

		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20110");

	// If we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTaskConditionPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20111", pbValue != NULL);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20112");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
IStrToStrMapPtr CTaskConditionPP::getTaskMap()
{
	if (!m_ipTaskMap)
	{
		// Create a CategoryManager instance with which to build the task map
		ICategoryManagerPtr ipCategoryManager(CLSID_CategoryManager);
		ASSERT_RESOURCE_ALLOCATION("ELI20177", ipCategoryManager != NULL);

		// Create array of required interfaces
		static const long nIIDCount = 3;
		IID pIIDs[nIIDCount];

		// Specify the interfaces that any selectable objects must implement
		pIIDs[0] = IID_IFileProcessingTask;
		pIIDs[1] = IID_ICopyableObject;
		pIIDs[2] = IID_IPersistStream;

		// Query the category manager for registered tasks which implement IFileProcessingTask
		m_ipTaskMap = ipCategoryManager->GetDescriptionToProgIDMap2(FP_FILE_PROC_CATEGORYNAME.c_str(),
			nIIDCount, pIIDs);

		ASSERT_RESOURCE_ALLOCATION("ELI20124", m_ipTaskMap != NULL);
	}

	return m_ipTaskMap;
}
//-------------------------------------------------------------------------------------------------
IFileProcessingTaskPtr CTaskConditionPP::getSelectedTask()
{
	EXTRACT_FAMCONDITIONSLib::ITaskConditionPtr ipTaskCondition = m_ppUnk[0];
	ASSERT_RESOURCE_ALLOCATION("ELI20126", ipTaskCondition != NULL);

	// Get the currently selected rule
	int nIndex = m_cmbTasks.GetCurSel();
	if (nIndex < 0)
	{
		UCLIDException ue("ELI20129", "No file processing task is selected!");
		throw ue;
	}

	// Get the selected rules's description
	_bstr_t bstrTaskName;
	m_cmbTasks.GetLBTextBSTR(nIndex, *bstrTaskName.GetAddress());

	// Check to see if the existing task matches the requested description
	// If so, just return the object we already have so that we don't throw
	// away any configuration settings the user may have already applied.
	ICategorizedComponentPtr ipCurrentTask = ipTaskCondition->Task;
	if (ipCurrentTask != NULL && 
		bstrTaskName == ipCurrentTask->GetComponentDescription())
	{
		return ipCurrentTask;
	}

	// Retrieve the Prog ID string
	_bstr_t bstrProgID = m_ipTaskMap->GetValue(bstrTaskName);

	// Create the object
	IFileProcessingTaskPtr ipTask((const char *)bstrProgID);
	ASSERT_RESOURCE_ALLOCATION("ELI20131", ipTask != NULL);

	return ipTask;
}
//-------------------------------------------------------------------------------------------------
bool CTaskConditionPP::updateRequiresConfig(IFileProcessingTaskPtr ipTask)
{
	ASSERT_ARGUMENT("ELI20133", ipTask != NULL);

	// Enable/Disable the configure button as necessary
	ISpecifyPropertyPagesPtr ipPP(ipTask);
	IConfigurableObjectPtr ipConfigurable(ipTask);
	m_btnConfigure.EnableWindow((ipPP != NULL || ipConfigurable != NULL) ? TRUE : FALSE);

	// Check configuration status
	IMustBeConfiguredObjectPtr ipRuleConfig(ipTask);
	if (ipRuleConfig != NULL && ipRuleConfig->IsConfigured() == VARIANT_FALSE)
	{
		// Show message to indicate the object needs configuration
		m_txtMustConfigure.ShowWindow(SW_SHOW);	
		return true;
	}
	else
	{
		// Object doesn't need configuration or is already configured
		m_txtMustConfigure.ShowWindow(SW_HIDE);
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
void CTaskConditionPP::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI20113", "Task Condition PP");
}
//-------------------------------------------------------------------------------------------------
