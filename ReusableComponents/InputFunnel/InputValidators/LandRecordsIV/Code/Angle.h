// Angle.h : Declaration of the CAngle

#pragma once

#include "resource.h"       // main symbols

#include <Angle.hpp>
/////////////////////////////////////////////////////////////////////////////
// CAngle
class ATL_NO_VTABLE CAngle : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAngle, &CLSID_Angle>,
	public ISupportErrorInfo,
	public IDispatchImpl<IAngle, &IID_IAngle, &LIBID_UCLID_LANDRECORDSIVLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CAngle();
	~CAngle();

DECLARE_REGISTRY_RESOURCEID(IDR_ANGLE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAngle)
	COM_INTERFACE_ENTRY(IAngle)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IAngle)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAngle
	STDMETHOD(IsValid)(/*[out, retval]*/ VARIANT_BOOL *bValid);
	STDMETHOD(InitAngle)(/*[in]*/ BSTR strInput);
	STDMETHOD(GetAngleInDegrees)(/*[out, retval]*/ double *dValue);
	STDMETHOD(GetAngleInRadians)(/*[out, retval]*/ double *dValue);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	// Angle class from filters
	Angle m_Angle;

	ITestResultLoggerPtr m_ipResultLogger;

	// Helper functions
	void executeTest1();
	void executeTest2();
	void executeTest3();

	// validate license, throw exception if the component is not licensed
	void validateLicense();
};

