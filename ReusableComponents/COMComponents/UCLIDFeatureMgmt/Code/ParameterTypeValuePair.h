// ParameterTypeValuePair.h : Declaration of the CParameterTypeValuePair

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CParameterTypeValuePair
class ATL_NO_VTABLE CParameterTypeValuePair : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CParameterTypeValuePair, &CLSID_ParameterTypeValuePair>,
	public ISupportErrorInfo,
	public IDispatchImpl<IParameterTypeValuePair, &IID_IParameterTypeValuePair, &LIBID_UCLID_FEATUREMGMTLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{

public:
	CParameterTypeValuePair();
	~CParameterTypeValuePair();

DECLARE_REGISTRY_RESOURCEID(IDR_PARAMETERTYPEVALUEPAIR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CParameterTypeValuePair)
	COM_INTERFACE_ENTRY(IParameterTypeValuePair)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IParameterTypeValuePair)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IParameterTypeValuePair
public:
	STDMETHOD(valueIsEqualTo)(/*[in]*/ IParameterTypeValuePair *pParamValueTypePair, /*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(get_strValue)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_strValue)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_eParamType)(/*[out, retval]*/ ECurveParameterType *pVal);
	STDMETHOD(put_eParamType)(/*[in]*/ ECurveParameterType newVal);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// check license
	void validateLicense();

	/////////////
	// Variables
	/////////////
	ECurveParameterType m_eParamType;
	std::string m_strValue;
};
