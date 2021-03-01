// FAMFileSelector.h : Declaration of the CFAMFileSelector

#pragma once

#include "resource.h"       // main symbols
#include "UCLIDFileProcessing.h"
#include "SelectFileSettings.h"

/////////////////////////////////////////////////////////////////////////////
// CFAMFileSelector
/////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CFAMFileSelector :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFAMFileSelector, &CLSID_FAMFileSelector>,
	public ISupportErrorInfo,
	public IDispatchImpl<IFAMFileSelector, &IID_IFAMFileSelector, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CFAMFileSelector();
	~CFAMFileSelector();

DECLARE_REGISTRY_RESOURCEID(IDR_FAMFILESELECTOR)

BEGIN_COM_MAP(CFAMFileSelector)
	COM_INTERFACE_ENTRY(IFAMFileSelector)
	COM_INTERFACE_ENTRY2(IDispatch,IFAMFileSelector)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

// IFAMFileSelector
	STDMETHOD(Configure)(IFileProcessingDB *pFAMDB, BSTR bstrSectionHeader,
		BSTR bstrQueryLabel, VARIANT_BOOL bIgnoreWorkflows, VARIANT_BOOL* pbNewSettingsApplied);
	STDMETHOD(AddActionStatusCondition)(IFileProcessingDB *pFAMDB, BSTR bstrAction,
		EActionStatus eStatus);
	STDMETHOD(AddQueryCondition)(BSTR bstrQuery);
	STDMETHOD(AddFileTagCondition)(BSTR tag, TagMatchType tagType);
	STDMETHOD(AddFileSetCondition)(BSTR bstrFileSet);
	STDMETHOD(LimitToSubset)(VARIANT_BOOL bRandomSubset, VARIANT_BOOL bTopSubset,
		VARIANT_BOOL bUsePercentage, LONG nSubsetSize, LONG nOffset);
	STDMETHOD(GetSummaryString)(IFileProcessingDB *pFAMDB, VARIANT_BOOL bIgnoreWorkflows,
		BSTR* pbstrSummaryString);
	STDMETHOD(get_SelectingAllFiles)(VARIANT_BOOL* pbSelectingAllFiles);
	STDMETHOD(BuildQuery)(IFileProcessingDB *pFAMDB, BSTR bstrSelect,
			BSTR bstrOrderByClause, VARIANT_BOOL bIgnoreWorkflows, BSTR* pbstrQuery);
	STDMETHOD(Reset)();

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:

	////////////
	//Variables
	////////////

	// The current settings
	SelectFileSettings m_settings;

	///////////
	//Methods
	///////////

	void validateLicense();
};
