// MERSTester.h : Declaration of the CMERSTester

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CMERSTester
class ATL_NO_VTABLE CMERSTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMERSTester, &CLSID_MERSTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CMERSTester();
	~CMERSTester();

DECLARE_REGISTRY_RESOURCEID(IDR_MERSTESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CMERSTester)
	COM_INTERFACE_ENTRY(ITestableComponent)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ITestableComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	////////////
	// Variables
	////////////
	// For automated testing
	ITestResultLoggerPtr m_ipResultLogger;

	IMERSHandlerPtr m_ipMERSHandler;

	IAFUtilityPtr m_ipAFUtility;

	////////////
	// Methods
	////////////
	// return true if these attributes are same
	bool compareAttributes(IIUnknownVectorPtr ipFoundAttributes, IIUnknownVectorPtr ipExpectedAttributes);

	//---------------------------------------------------------------------------------------------
	// PROMISE:	To throw an exception if this object is not licensed to run
	void validateLicense();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return the full path to the master test file.  The path is 
	//			computed differently in debug and release builds.
	const std::string getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const;
	//---------------------------------------------------------------------------------------------
};

