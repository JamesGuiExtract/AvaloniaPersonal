// FAMFileSelector.cpp : Implementation of CFAMFileSelector

#include "stdafx.h"
#include "FAMFileSelector.h"
#include "SelectFilesDlg.h"
#include "ActionStatusCondition.h"
#include "QueryCondition.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// CFAMFileSelector
//--------------------------------------------------------------------------------------------------
CFAMFileSelector::CFAMFileSelector()
{
}
//--------------------------------------------------------------------------------------------------
CFAMFileSelector::~CFAMFileSelector()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI35692");
}
//-------------------------------------------------------------------------------------------------
HRESULT CFAMFileSelector::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CFAMFileSelector::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// IFAMTagManager
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMFileSelector::Configure(IFileProcessingDB *pFAMDB,
	BSTR bstrSectionHeader, BSTR bstrQueryLabel, VARIANT_BOOL* pbNewSettingsApplied)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB(pFAMDB);
		ASSERT_ARGUMENT("ELI35693", ipFAMDB != __nullptr);
		ASSERT_ARGUMENT("ELI35694", pbNewSettingsApplied != __nullptr);

		// Create the file select dialog
		CSelectFilesDlg dlg(
			ipFAMDB, asString(bstrSectionHeader), asString(bstrQueryLabel), m_settings);

		// Display the dialog and save changes if user clicked OK
		if (dlg.DoModal() == IDOK)
		{
			// Get the settings from the dialog
			m_settings = dlg.getSettings();

			*pbNewSettingsApplied = VARIANT_TRUE;
		}
		else
		{
			*pbNewSettingsApplied = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35695");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMFileSelector::AddActionStatusCondition(IFileProcessingDB *pFAMDB,
	long nActionID, EActionStatus eStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB(pFAMDB);
		ASSERT_ARGUMENT("ELI35696", ipFAMDB != __nullptr);

		ActionStatusCondition* pCondition = new ActionStatusCondition();
		pCondition->setActionID(nActionID);		
		pCondition->setAction(asString(ipFAMDB->GetActionName(nActionID)));
		pCondition->setStatus(eStatus);
		string strStatusString =
			asString(ipFAMDB->AsStatusName((UCLID_FILEPROCESSINGLib::EActionStatus)eStatus));
		pCondition->setStatusString(strStatusString);
		pCondition->setUser(gstrANY_USER);
				
		m_settings.addCondition(pCondition);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35697");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMFileSelector::AddQueryCondition(IFileProcessingDB *pFAMDB, BSTR bstrQuery)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB(pFAMDB);
		ASSERT_ARGUMENT("ELI36095", ipFAMDB != __nullptr);

		string strQuery = asString(bstrQuery);

		if (!trimLeadingWord(strQuery, "SELECT FAMFile.ID FROM FAMFile"))
		{
			if (!trimLeadingWord(strQuery, "SELECT [FAMFile].[ID] FROM [FAMFile]"))
			{
				UCLIDException ue("ELI36096", "Query for query condition must begin with \""
					"SELECT FAMFile.ID FROM FAMFile\"");
				ue.addDebugInfo("Query", strQuery);
				throw ue;
			}
		}

		QueryCondition* pCondition = new QueryCondition();
		pCondition->setSQLString(strQuery);
		m_settings.addCondition(pCondition);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36097");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMFileSelector::LimitToSubset(VARIANT_BOOL bRandomSubset,
											 VARIANT_BOOL bUsePercentage, LONG nSubsetSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_settings.setLimitToSubset(true);
		m_settings.setSubsetIsRandom(asCppBool(bRandomSubset));
		m_settings.setSubsetUsePercentage(asCppBool(bUsePercentage));
		m_settings.setSubsetSize(nSubsetSize);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35769");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMFileSelector::GetSummaryString(BSTR *pbstrSummaryString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI35698", pbstrSummaryString != __nullptr);

		string strSummaryString = m_settings.getSummaryString();
		*pbstrSummaryString = get_bstr_t(strSummaryString).Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35699");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMFileSelector::get_SelectingAllFiles(VARIANT_BOOL* pbSelectingAllFiles)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI35700", pbSelectingAllFiles != __nullptr);
		
		*pbSelectingAllFiles = asVariantBool(m_settings.selectingAllFiles());

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35701")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMFileSelector::BuildQuery(IFileProcessingDB *pFAMDB,
	BSTR bstrSelect, BSTR bstrOrderByClause, BSTR* pbstrQuery)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB(pFAMDB);
		ASSERT_ARGUMENT("ELI35702", ipFAMDB != __nullptr);
		ASSERT_ARGUMENT("ELI35703", pbstrQuery != __nullptr);

		string strQuery = m_settings.buildQuery(ipFAMDB, asString(bstrSelect),
			asString(bstrOrderByClause));
		*pbstrQuery = get_bstr_t(strQuery).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35704");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMFileSelector::Reset()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_settings = SelectFileSettings();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35705");
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMFileSelector::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFAMFileSelector,
		&IID_ILicensedComponent
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMFileSelector::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
		return E_POINTER;

	try
	{
		// validate license
		validateLicense();
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CFAMFileSelector::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI35706", "FAM File Selector");
}
//-------------------------------------------------------------------------------------------------