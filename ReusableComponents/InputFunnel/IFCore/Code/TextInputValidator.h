// TextInputValidator.h : Declaration of the CTextInputValidator

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CTextInputValidator
class ATL_NO_VTABLE CTextInputValidator : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTextInputValidator, &CLSID_TextInputValidator>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IInputValidator, &IID_IInputValidator, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ITextInputValidator, &IID_ITextInputValidator, &LIBID_UCLID_INPUTFUNNELLib>
{
public:
	CTextInputValidator();
	~CTextInputValidator();

DECLARE_REGISTRY_RESOURCEID(IDR_TEXTINPUTVALIDATOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CTextInputValidator)
	COM_INTERFACE_ENTRY(IInputValidator)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IInputValidator)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY(ITextInputValidator)
END_COM_MAP()

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITextInputValidator
	STDMETHOD(get_EmptyInputOK)(/*[out, retval]*/ VARIANT_BOOL *pbVal);
	STDMETHOD(put_EmptyInputOK)(/*[in]*/ VARIANT_BOOL bVal);

// IInputValidator
	STDMETHOD(GetInputType)(/*[out, retval]*/ BSTR *pstrInputType);
	STDMETHOD(ValidateInput)(/*[in]*/ ITextInput *pTextInput, /*[out, retval]*/ VARIANT_BOOL *pbSuccessful);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pbstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	ITestResultLoggerPtr m_ipResultLogger;

	void executeTest1();
	void executeTest2();
	void executeTest3();

	// check license
	void validateLicense();

	// flag to keep track of whether empty text is acceptable
	bool m_bEmptyInputOK; 

	// flag to keep track of whether this object has changed
	// since the last save-to-stream operation
	bool m_bDirty;
};

