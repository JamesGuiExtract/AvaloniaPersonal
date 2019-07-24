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
	public IDispatchImpl<IFAMCancelable, &IID_IFAMCancelable, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<IFAMProcessingResult, &IID_IFAMProcessingResult, &LIBID_UCLID_FILEPROCESSINGLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CGenericMultiFAMCondition();
	~CGenericMultiFAMCondition();

DECLARE_REGISTRY_RESOURCEID(IDR_GENERICMULTIFAMCONDITION)

BEGIN_COM_MAP(CGenericMultiFAMCondition)
	COM_INTERFACE_ENTRY(IGenericMultiFAMCondition)
	COM_INTERFACE_ENTRY2(IDispatch, IGenericMultiFAMCondition)
	COM_INTERFACE_ENTRY(IFAMCancelable)
	COM_INTERFACE_ENTRY(IFAMProcessingResult)
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
		VARIANT_BOOL bPaginationCondition, IFileRecord* pFileRecord, BSTR bstrProposedFileName,
		BSTR bstrDocumentStatus, BSTR bstrSerializedDocumentAttributes,
		IFileProcessingDB* pFPDB, long lActionID,
		IFAMTagManager* pFAMTM, VARIANT_BOOL* pRetVal);
	STDMETHOD(Parallelize)(IIUnknownVector* pFAMConditions, VARIANT_BOOL *pvbParallelize);
	STDMETHOD(Init)(IIUnknownVector* pFAMConditions, long nActionID, IFAMTagManager* pFAMTM, IFileProcessingDB* pDB,
			IFileRequestHandler* pFileRequestHandler);
	STDMETHOD(Close)(IIUnknownVector* pFAMConditions);

	// IFAMCancelable
	STDMETHOD(raw_Cancel)();
	STDMETHOD(raw_IsCanceled)(VARIANT_BOOL *pvbCanceled);
		
	// IFAMProcessingResult
	STDMETHOD(raw_GetResult)(EFileProcessingResult* pResult);

private:
	/////////////
	// Methods
	/////////////
	void validateLicense();

	/////////////
	// Variables
	/////////////

	// Flag to indicate if the last call to FileMatchesFAMCondition had any conditions that were 
	// canceled
	bool m_bCanceled;

	// Flag to indicate that a cancel has been requested
	bool m_bCancelRequested;

	// Used to hold the currently processing condition
	IUnknownPtr m_ipCurrentCondition;

	// Result of the processFile call for the on the task
	EFileProcessingResult m_eTaskResult;
};
