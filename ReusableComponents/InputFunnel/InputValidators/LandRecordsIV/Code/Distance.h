// Distance.h : Declaration of the CDistance

#pragma once

#include "resource.h"       // main symbols

#include <DistanceCore.h>

/////////////////////////////////////////////////////////////////////////////
// CDistance
class ATL_NO_VTABLE CDistance : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDistance, &CLSID_Distance>,
	public ISupportErrorInfo,
	public IDispatchImpl<IDistance, &IID_IDistance, &LIBID_UCLID_LANDRECORDSIVLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CDistance();
	~CDistance();

DECLARE_REGISTRY_RESOURCEID(IDR_DISTANCE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDistance)
	COM_INTERFACE_ENTRY(IDistance)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IDistance)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IDistance
	STDMETHOD(get_GlobalDefaultDistanceUnit)(/*[out, retval]*/ EDistanceUnitType *pVal);
	STDMETHOD(put_GlobalDefaultDistanceUnit)(/*[in]*/ EDistanceUnitType newVal);
	STDMETHOD(IsValid)(/*[out, retval]*/ VARIANT_BOOL *bValid);
	STDMETHOD(InitDistance)(/*[in]*/ BSTR strInput);
	STDMETHOD(GetDistanceInUnit)(/*[in]*/ EDistanceUnitType eOutUnit, /*[out, retval]*/ double *dValue);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	// distance class from filters.dll
	DistanceCore m_Distance;

	// distance converter
	IDistanceConverterPtr m_ipDistanceConverter;

	ITestResultLoggerPtr m_ipResultLogger;

	//helper functions
	void executeTest1();
	void executeTest2();
	void executeTest3();

	// validate license, throw exception if the component is not licensed
	void validateLicense();
};
