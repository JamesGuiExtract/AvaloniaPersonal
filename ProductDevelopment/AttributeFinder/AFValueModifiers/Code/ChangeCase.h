// ChangeCase.h : Declaration of the CChangeCase

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

/////////////////////////////////////////////////////////////////////////////
// CChangeCase
class ATL_NO_VTABLE CChangeCase : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CChangeCase, &CLSID_ChangeCase>,
	public ISupportErrorInfo,
	public IDispatchImpl<IChangeCase, &IID_IChangeCase, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CChangeCase>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>
{
public:
	CChangeCase();

DECLARE_REGISTRY_RESOURCEID(IDR_CHANGECASE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CChangeCase)
	COM_INTERFACE_ENTRY(IChangeCase)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IChangeCase)
	COM_INTERFACE_ENTRY(IAttributeModifyingRule)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	COM_INTERFACE_ENTRY(IOutputHandler)
END_COM_MAP()

BEGIN_PROP_MAP(CChangeCase)
	PROP_PAGE(CLSID_ChangeCasePP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CChangeCase)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IChangeCase
	STDMETHOD(get_CaseType)(/*[out, retval]*/ EChangeCaseType *pVal);
	STDMETHOD(put_CaseType)(/*[in]*/ EChangeCaseType newVal);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(IAttribute * pAttribute, IAFDocument* pOriginInput, 
		IProgressStatus *pProgressStatus);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector * pAttributes, IAFDocument * pDoc, 
		IProgressStatus *pProgressStatus);

private:	
	////////////
	// Variables
	////////////
	// flag to keep track of whether this object has been modified
	// since the last save-to-stream operation
	bool m_bDirty;

	// Current setting for resulting case
	EChangeCaseType m_eCaseType;

	///////////
	// Methods
	///////////
	void validateLicense();
};
