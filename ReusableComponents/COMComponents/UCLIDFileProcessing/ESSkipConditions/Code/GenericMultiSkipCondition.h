// GenericMultiSkipCondition.h : Declaration of the CGenericMultiFAMCondition

#pragma once
#include "resource.h"       // main symbols

#include "..\..\Code\FPCategories.h"
#include "ESSkipConditions.h"

/////////////////////////////////////////////////////////////////////////////
// CGenericMultiFAMCondition
class ATL_NO_VTABLE CGenericMultiFAMCondition :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CGenericMultiFAMCondition, &CLSID_GenericMultiFAMCondition>,
	public ISupportErrorInfo,
	public IDispatchImpl<IGenericMultiFAMCondition, &IID_IGenericMultiFAMCondition, &LIBID_EXTRACT_FAMCONDITIONSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CGenericMultiFAMCondition();
	~CGenericMultiFAMCondition();

DECLARE_REGISTRY_RESOURCEID(IDR_GENERICMULTIFAMCONDITION)

BEGIN_COM_MAP(CGenericMultiFAMCondition)
	COM_INTERFACE_ENTRY(IGenericMultiFAMCondition)
	COM_INTERFACE_ENTRY2(IDispatch, IGenericMultiFAMCondition)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL* pbValue);

// IGenericMultiFAMCondition
	STDMETHOD(FileMatchesFAMCondition)(IIUnknownVector* pFAMConditions, ELogicalOperator eLogicalOperator, 
		BSTR bstrFile, IFileProcessingDB* pFPDB, long lFileID, long lActionID, 
		IFAMTagManager* pFAMTM, VARIANT_BOOL* pRetVal);

private:
	/////////////
	// Methods
	/////////////
	void validateLicense();
};