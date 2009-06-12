// DistanceFilter.h : Declaration of the CDistanceFilter

#pragma once

#include "resource.h"       // main symbols

#include <DistanceCore.h>

#include <string>
#include <vector>
#include<map>

/////////////////////////////////////////////////////////////////////////////
// CDistanceFilter
class ATL_NO_VTABLE CDistanceFilter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDistanceFilter, &CLSID_DistanceFilter>,
	public ISupportErrorInfo,
	public IDispatchImpl<IDistanceFilter, &IID_IDistanceFilter, &LIBID_UCLID_FILTERSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CDistanceFilter();
	~CDistanceFilter();

DECLARE_REGISTRY_RESOURCEID(IDR_DISTANCEFILTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDistanceFilter)
	COM_INTERFACE_ENTRY(IDistanceFilter)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IDistanceFilter)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IDistanceFilter
	// Gets the evaluated distance in string in the unit as specified
	STDMETHOD(AsStringInUnit)(/*[out, retval]*/ BSTR* pbstrOut, /*[in]*/ EDistanceUnitType eUnitType);
	// Evaluates the input string
	STDMETHOD(Evaluate)(/*[in]*/ BSTR bstrInput);
	// Returns the original input string
	STDMETHOD(GetOriginalInputString)(/*[out, retval]*/ BSTR* pbstrOrignInput);
	// Gets the distance value in double in the unit as specified
	STDMETHOD(GetDistanceInUnit)(/*[out, retval]*/ double* pdOutValue, /*[in]*/ EDistanceUnitType eOutUnit);
	// Whether or not the original input string for distance is valid
	STDMETHOD(IsValid)(/*[out, retval]*/ VARIANT_BOOL *pbValid);
	// Resets certain member variables
	STDMETHOD(Reset)();
	// Set current default unit type in case the input string doesn't 
	// have any unit specified in the string. For instance, "1234" doesn't
	// have any unit specified, whereas "1234 meters" has.
	STDMETHOD(SetDefaultUnitType)(/*[in]*/ EDistanceUnitType eDefaultUnit);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:

	////////////////////////////////////////////////////////////////////////////////
	//***				Member Variables	******
	///////////////////////////////////////////////////////////////////////////////
	// result logger for recording test result
	ITestResultLoggerPtr m_ipResultLogger;

	// DistanceCore
	DistanceCore m_DistanceCore;

	////////////////////////////////////////////////////////////////////////////////
	//***				Helper Functions		******
	///////////////////////////////////////////////////////////////////////////////
	// test distance filter
	void testDistanceFilter();

	// validate the license
	void validateLicense();

};
