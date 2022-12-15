// FAMFileSelector.cpp : Implementation of CFAMFileSelector

#include "stdafx.h"
#include "FAMFileSelector.h"
#include "SelectFilesDlg.h"
#include "ActionStatusCondition.h"
#include "QueryCondition.h"
#include "FileSetCondition.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include "FileTagCondition.h"

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
	BSTR bstrSectionHeader, BSTR bstrQueryLabel, VARIANT_BOOL bIgnoreWorkflows, 
	VARIANT_BOOL* pbNewSettingsApplied)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI35693", pFAMDB != __nullptr);
		ASSERT_ARGUMENT("ELI35694", pbNewSettingsApplied != __nullptr);
		
		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB;
		if (asCppBool(bIgnoreWorkflows))
		{
			ipFAMDB.CreateInstance(CLSID_FileProcessingDB);
			ASSERT_RESOURCE_ALLOCATION("ELI43541", ipFAMDB != __nullptr);
			
			ipFAMDB->DuplicateConnection((UCLID_FILEPROCESSINGLib::IFileProcessingDB*)pFAMDB);
			ipFAMDB->ActiveWorkflow = "";
		}
		else
		{
			ipFAMDB = pFAMDB;
			ASSERT_RESOURCE_ALLOCATION("ELI43542", ipFAMDB != __nullptr);
		}

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
	BSTR bstrAction, EActionStatus eStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB(pFAMDB);
		ASSERT_ARGUMENT("ELI35696", ipFAMDB != __nullptr);

		ActionStatusCondition* pCondition = new ActionStatusCondition();
		pCondition->setAction(asString(bstrAction));
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
STDMETHODIMP CFAMFileSelector::AddFileTagCondition(BSTR tag, TagMatchType tagType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		FileTagCondition* pCondition = new FileTagCondition();

		std::vector<string> tags;
		tags.push_back(asString(tag));
		pCondition->setTags(tags);

		pCondition->setTagType(tagType);

		m_settings.addCondition(pCondition);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI53823");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMFileSelector::AddQueryCondition(BSTR bstrQuery)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

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
STDMETHODIMP CFAMFileSelector::AddFileSetCondition(BSTR bstrFileSet)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		FileSetCondition* pCondition = new FileSetCondition();
		pCondition->setFileSetName(asString(bstrFileSet));
		m_settings.addCondition(pCondition);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37350");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMFileSelector::LimitToSubset(VARIANT_BOOL bRandomSubset, VARIANT_BOOL bTopSubset,
											 VARIANT_BOOL bUsePercentage, LONG nSubsetSize, LONG nOffset)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// I don't want to mess with the giant query to support offsets for all these conditions so assert a simple subset if offset >= 0
		ASSERT_RUNTIME_CONDITION("ELI47046", nOffset < 0 || !bRandomSubset && bTopSubset && !bUsePercentage,
			"Offset not supported with random, reverse or percentage subsets");

		m_settings.setLimitToSubset(true);
		m_settings.setSubsetIsRandom(asCppBool(bRandomSubset));
		m_settings.setSubsetIsTop(asCppBool(bTopSubset));
		m_settings.setSubsetUsePercentage(asCppBool(bUsePercentage));
		m_settings.setSubsetSize(nSubsetSize);
		m_settings.setOffset(nOffset);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35769");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFAMFileSelector::GetSummaryString(IFileProcessingDB *pFAMDB, VARIANT_BOOL bIgnoreWorkflows,
												BSTR *pbstrSummaryString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI35698", pbstrSummaryString != __nullptr);
		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB(pFAMDB);
		ASSERT_ARGUMENT("ELI43538", ipFAMDB != __nullptr);

		string strSummaryString = m_settings.getSummaryString(ipFAMDB, asCppBool(bIgnoreWorkflows));
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
	BSTR bstrSelect, BSTR bstrOrderByClause, VARIANT_BOOL bIgnoreWorkflows,
	BSTR* pbstrQuery)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipFAMDB(pFAMDB);
		ASSERT_ARGUMENT("ELI35702", ipFAMDB != __nullptr);
		ASSERT_ARGUMENT("ELI35703", pbstrQuery != __nullptr);

		string strQuery = m_settings.buildQuery(ipFAMDB, asString(bstrSelect),
			asString(bstrOrderByClause), asCppBool(bIgnoreWorkflows));
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