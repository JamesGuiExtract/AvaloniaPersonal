// TestIntersection.h : Declaration of the CTestIntersection

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CTestIntersection
class ATL_NO_VTABLE CTestIntersection : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTestIntersection, &CLSID_TestIntersection>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CTestIntersection();
	~CTestIntersection();

DECLARE_REGISTRY_RESOURCEID(IDR_TESTINTERSECTION)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CTestIntersection)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY2(IDispatch, ITestableComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	
	////////////
	// Variables
	////////////
	// ITestResultLoggerPtr object
	ITestResultLoggerPtr m_ipResultLogger;

	//////////
	// Methods
	//////////
	// Process testing file
	void processFile(const std::string& strFile);

	// Process one test case
	void processTestCase(const std::string& strLine);

	// tokenize the start, end points and height of a rectangle
	IRasterZonePtr asRasterZone(const std::string& strRect);

	// Test the intersection of two rectangles, return true if passed
	bool testIntersection(const std::string & strRect1,
		const std::string & strRect2, const std::string & strExpected);

	// Check lincense
	void validateLicense() const;
};