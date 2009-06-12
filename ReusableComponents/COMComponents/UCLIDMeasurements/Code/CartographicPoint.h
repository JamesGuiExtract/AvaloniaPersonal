// CartographicPoint.h : Declaration of the CCartographicPoint

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CCartographicPoint
class ATL_NO_VTABLE CCartographicPoint : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCartographicPoint, &CLSID_CartographicPoint>,
	public ISupportErrorInfo,
	public IDispatchImpl<ICartographicPoint, &IID_ICartographicPoint, &LIBID_UCLID_MEASUREMENTSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CCartographicPoint()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_CARTOGRAPHICPOINT)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CCartographicPoint)
	COM_INTERFACE_ENTRY(ICartographicPoint)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ICartographicPoint)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICartographicPoint
	STDMETHOD(IsValid)(/*[out, retval]*/ VARIANT_BOOL *pbValid);
	STDMETHOD(IsEqual)(/*[in]*/ ICartographicPoint *pPointToCompare, /*[out, retval]*/ VARIANT_BOOL *pbVal);
	STDMETHOD(GetPointInXY)(/*[in, out]*/ double *pdX, /*[in, out]*/ double *pdY);
	STDMETHOD(InitPointInXY)(/*[in]*/ double dX, /*[in]*/ double dY);
	STDMETHOD(InitPointInString)(/*[in]*/ BSTR strInput);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	//***************************************
	// Helper functions
	// test the object
	void executeTest1();
	void executeTest2();
	void executeTest3();

	// check license
	void validateLicense();
	// parse the string into x, y
	void parseInput(const CString& cstrInput);


	//***************************************
	// Member variables

	// x, y for the point
	double m_dX, m_dY;
	// whether the input string for the point is valid or not
	bool m_bValid;

	// result logger for recording test result
	ITestResultLoggerPtr m_ipResultLogger;
};

