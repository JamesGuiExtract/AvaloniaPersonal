// WordInputFinder.h : Declaration of the CWordInputFinder

#pragma once

#include "resource.h"       // main symbols

#include <string>
/////////////////////////////////////////////////////////////////////////////
// CWordInputFinder
class ATL_NO_VTABLE CWordInputFinder : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CWordInputFinder, &CLSID_WordInputFinder>,
	public ISupportErrorInfo,
	public IDispatchImpl<IInputFinder, &IID_IInputFinder, &LIBID_UCLID_INPUTFINDERSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CWordInputFinder()
	{
	}
	~CWordInputFinder()
	{
	}


DECLARE_REGISTRY_RESOURCEID(IDR_WORDINPUTFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CWordInputFinder)
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
	typedef struct
	{
		unsigned long ulStart;
		unsigned long ulEnd;
	}WordPos;

	ITestResultLoggerPtr m_ipResultLogger;

	// check for license
	void validateLicense();

	// find words positions and then push them into the vector
	void findWordsPositions(const std::string& strInput, IIUnknownVector **ippTokenPositions);

	void automatedTest1();
};

