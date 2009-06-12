// TextInput.h : Declaration of the CTextInput

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CTextInput
class ATL_NO_VTABLE CTextInput : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTextInput, &CLSID_TextInput>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITextInput, &IID_ITextInput, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CTextInput();
	~CTextInput();

DECLARE_REGISTRY_RESOURCEID(IDR_TEXTINPUT)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CTextInput)
	COM_INTERFACE_ENTRY(ITextInput)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ITextInput)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITextInput
	STDMETHOD(GetInputEntity)(/*[out, retval]*/ IInputEntity **pEntity);
	STDMETHOD(SetValidatedInput)(/*[in]*/ IUnknown *pObj);
	STDMETHOD(GetValidatedInput)(/*[out, retval]*/ IUnknown **pObj);
	STDMETHOD(GetText)(/*[out, retval]*/ BSTR *pstrText);
	STDMETHOD(SetText)(/*[in]*/ BSTR strText);
	STDMETHOD(InitTextInput)(/*[in]*/ IInputEntity *pEntity, /*[in]*/ BSTR strText);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// text for this text input
	CComBSTR m_cbstrText;
	// validated input object
	CComPtr<IUnknown> m_cipValidatedInput;
	// keep the input entity object
	UCLID_INPUTFUNNELLib::IInputEntityPtr m_ipInputEntity;

	// check license
	void validateLicense();
};

