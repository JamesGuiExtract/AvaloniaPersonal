// MCRTextInputFinder.h : Declaration of the CMCRTextInputFinder

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CMCRTextInputFinder
class ATL_NO_VTABLE CMCRTextInputFinder : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMCRTextInputFinder, &CLSID_MCRTextInputFinder>,
	public ISupportErrorInfo,
	public IDispatchImpl<IInputFinder, &IID_IInputFinder, &LIBID_UCLID_INPUTFINDERSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CMCRTextInputFinder()
	{
	}
	~CMCRTextInputFinder()
	{
	}


DECLARE_REGISTRY_RESOURCEID(IDR_MCRTEXTINPUTFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CMCRTextInputFinder)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IInputFinder)
	COM_INTERFACE_ENTRY2(IDispatch, IInputFinder)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IInputFinder
	STDMETHOD(ParseString)(BSTR strInput, IIUnknownVector **ippTokenPositions);

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

	void validateLicense();

	void automatedTest1();
};

