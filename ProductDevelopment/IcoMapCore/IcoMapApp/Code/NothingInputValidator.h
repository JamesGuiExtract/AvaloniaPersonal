// NothingInputValidator.h : Declaration of the CNothingInputValidator

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CNothingInputValidator
class ATL_NO_VTABLE CNothingInputValidator : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CNothingInputValidator, &CLSID_NothingInputValidator>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<INothingInputValidator, &IID_INothingInputValidator, &LIBID_ICOMAPAPPLib>,
	public IDispatchImpl<IInputValidator, &IID_IInputValidator, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>
{
public:
	CNothingInputValidator();
	~CNothingInputValidator();

DECLARE_REGISTRY_RESOURCEID(IDR_NOTHINGINPUTVALIDATOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CNothingInputValidator)
	COM_INTERFACE_ENTRY(INothingInputValidator)
	COM_INTERFACE_ENTRY2(IDispatch, INothingInputValidator)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(IInputValidator)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IInputValidator
	STDMETHOD(raw_GetInputType)(/*[out, retval]*/ BSTR *pstrInputType);
	STDMETHOD(raw_ValidateInput)(/*[in]*/ ITextInput *pTextInput, /*[out, retval]*/ VARIANT_BOOL *pbSuccessful);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pbstrComponentDescription);

// INothingInputValidator

private:
	bool m_bDirty;
};

