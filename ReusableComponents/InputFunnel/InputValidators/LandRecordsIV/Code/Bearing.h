// Bearing.h : Declaration of the CBearing

#pragma once

#include "resource.h"       // main symbols

#include <Bearing.hpp>

/////////////////////////////////////////////////////////////////////////////
// CBearing
class ATL_NO_VTABLE CBearing : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CBearing, &CLSID_Bearing>,
	public ISupportErrorInfo,
	public IDispatchImpl<IBearing, &IID_IBearing, &LIBID_UCLID_LANDRECORDSIVLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CBearing();
	~CBearing();

DECLARE_REGISTRY_RESOURCEID(IDR_BEARING)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CBearing)
	COM_INTERFACE_ENTRY(IBearing)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IBearing)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IBearing
	STDMETHOD(IsValid)(/*[out, retval]*/ VARIANT_BOOL *bValid);
	STDMETHOD(InitBearing)(/*[in]*/ BSTR strInput);
	STDMETHOD(GetBearingInDegrees)(/*[out, retval]*/ double *dValue);
	STDMETHOD(GetBearingInRadians)(/*[out, retval]*/ double *dValue);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	Bearing m_Bearing;

	ITestResultLoggerPtr m_ipResultLogger;

	//helper functions
	void executeTest1();
	void executeTest2();
	void executeTest3();

	// validate license, throw exception if the component is not licensed
	void validateLicense();
};
