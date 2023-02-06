// FAMDBUtils.cpp : Implementation of CFAMDBUtils

#include "stdafx.h"
#include "FAMDBUtils.h"
#include "SelectActionDlg.h"
#include "FileProcessingConfigMgr.h"

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
STDMETHODIMP CFAMDBUtils::PromptForActionSelection(IFileProcessingDB* pDB, BSTR strTitle, 
	BSTR strPrompt, VARIANT_BOOL vbAllowTags, BSTR *pActionName)
{
	// override the current resource project instance
	TemporaryResourceOverride rcOverride(_Module.m_hInst);

	try
	{
		// Create select action dialog
		SelectActionDlg dlgSelectAction(pDB, asString(strTitle), asString(strPrompt), 
			asCppBool(vbAllowTags));

		// Define the selected action name
		string strActionName;

		// if the user clicked on OK, then get the selected action
		if (dlgSelectAction.DoModal() == IDOK)
		{
			strActionName = dlgSelectAction.GetSelectedAction();
		}

		// Return the action name
		*pActionName = get_bstr_t(strActionName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14693");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMDBUtils::GetFAMDBProgId(BSTR *pProgID)
{
	try
	{
		FileProcessingConfigMgr configMgr;
		string strMgrType = configMgr.getDBManagerType();
		makeLowerCase(strMgrType);
		string strProgID = "UCLIDFileProcessing.FileProcessingDB.1";

		if (strMgrType == ".net")
		{
			strProgID = "Extract.FileActionManager.Database.FAMDatabaseManager";
		}

		*pProgID = get_bstr_t(strProgID.c_str()).Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34519");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMDBUtils::GetIDShieldDBProgId(BSTR *pProgID)
{
	try
	{
		FileProcessingConfigMgr configMgr;
		string strMgrType = configMgr.getDBManagerType();
		makeLowerCase(strMgrType);
		string strProgID = "RedactionCustomComponents.IDShieldProductDBMgr.1";

		if (strMgrType == ".net")
		{
			strProgID = "Extract.Redaction.Database.IDShieldDatabaseManager";
		}
		
		*pProgID = get_bstr_t(strProgID.c_str()).Detach();
				return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34520");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMDBUtils::GetDataEntryDBProgId(BSTR *pProgID)
{
	try
	{
		FileProcessingConfigMgr configMgr;
		string strMgrType = configMgr.getDBManagerType();
		makeLowerCase(strMgrType);
		string strProgID = "DataEntryCustomComponents.DataEntryProductDBMgr.1";

		if (strMgrType == ".net")
		{
			strProgID = "Extract.DataEntry.Database.DataEntryDatabaseManager";
		}

		*pProgID = get_bstr_t(strProgID.c_str()).Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI34521");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------