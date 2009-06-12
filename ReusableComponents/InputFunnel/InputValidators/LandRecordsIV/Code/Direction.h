// Direction.h : Declaration of the CDirection

#pragma once

#include "resource.h"       // main symbols
#include <DirectionHelper.h>

/////////////////////////////////////////////////////////////////////////////
// CDirection
class ATL_NO_VTABLE CDirection : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDirection, &CLSID_Direction>,
	public ISupportErrorInfo,
	public IDispatchImpl<IDirection, &IID_IDirection, &LIBID_UCLID_LANDRECORDSIVLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CDirection();
	~CDirection();

DECLARE_REGISTRY_RESOURCEID(IDR_DIRECTION)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDirection)
	COM_INTERFACE_ENTRY(IDirection)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IDirection)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IDirection
	STDMETHOD(get_GlobalDirectionType)(/*[out, retval]*/ ECartographicDirection *pVal);
	STDMETHOD(put_GlobalDirectionType)(/*[in]*/ ECartographicDirection newVal);
	STDMETHOD(IsValid)(/*[out, retval]*/ VARIANT_BOOL *pbValid);
	STDMETHOD(InitDirection)(/*[in]*/ BSTR bstrInput);
	STDMETHOD(GetDirectionAsPolarAngleInRadians)(/*[out, retval]*/ double *pdPolarAngleRadians);
	STDMETHOD(GetDirectionAsPolarAngleInDegrees)(/*[out, retval]*/ double *pdPolarAngleDegrees);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	ITestResultLoggerPtr m_ipResultLogger;
	DirectionHelper m_DirectionHelper;

	// validate license, throw exception if the component is not licensed
	void validateLicense();
};

