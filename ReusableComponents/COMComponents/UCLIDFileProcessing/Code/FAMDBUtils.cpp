// FAMDBUtils.cpp : Implementation of CFAMDBUtils

#include "stdafx.h"
#include "FAMDBUtils.h"
#include "SelectActionDlg.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <ComUtils.h>

//--------------------------------------------------------------------------------------------------
// CFAMDBUtils
//--------------------------------------------------------------------------------------------------
CFAMDBUtils::CFAMDBUtils()
{
}
//--------------------------------------------------------------------------------------------------
CFAMDBUtils::~CFAMDBUtils()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16522");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMDBUtils::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFAMDBUtils
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// IFAMDBUtils
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMDBUtils::PromptForActionSelection(IFileProcessingDB* pDB, BSTR strTitle, BSTR strPrompt, BSTR *pActionName)
{
	// override the current resource project instance
	TemporaryResourceOverride rcOverride(_Module.m_hInst);

	try
	{
		// Create select action dialog
		SelectActionDlg dlgSelectAction(pDB, asString(strTitle), asString(strPrompt));

		// Define the selected action name and ID
		string strActionName = "";
		DWORD dwActionID;

		// if the user clicked on OK, then delete the selected action
		if (dlgSelectAction.DoModal() == IDOK)
		{
			// Call GetSelectedAction() to get the selected action name and ID
			dlgSelectAction.GetSelectedAction(strActionName, dwActionID);
		}

		// Return the action name
		*pActionName = get_bstr_t(strActionName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14693");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------