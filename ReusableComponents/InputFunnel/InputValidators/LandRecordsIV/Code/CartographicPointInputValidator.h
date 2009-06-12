// CartographicPointInputValidator.h : Declaration of the CCartographicPointInputValidator

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CCartographicPointInputValidator
class ATL_NO_VTABLE CCartographicPointInputValidator : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCartographicPointInputValidator, &CLSID_CartographicPointInputValidator>,
	public ISupportErrorInfo,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IInputValidator, &IID_IInputValidator, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CCartographicPointInputValidator();
	~CCartographicPointInputValidator();

DECLARE_REGISTRY_RESOURCEID(IDR_CARTOGRAPHICPOINTINPUTVALIDATOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CCartographicPointInputValidator)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY2(IDispatch, ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IInputValidator)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICartographicPointInputValidator
public:
// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pbstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IInputValidator
	STDMETHOD(raw_ValidateInput)(ITextInput * pTextInput, VARIANT_BOOL * pbSuccessful);
	STDMETHOD(raw_GetInputType)(BSTR * pstrInputType);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	// validate license, throw exception if the component is not licensed
	void validateLicense();

	IInteractiveTestExecuterPtr m_ipExecuter;
};

