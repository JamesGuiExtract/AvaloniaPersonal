// TestAbstractMeasurement.h : Declaration of the CTestAbstractMeasurement

#pragma once

#include "resource.h"       // main symbols

#include <string>
#include <vector>
/////////////////////////////////////////////////////////////////////////////
// CTestAbstractMeasurement
class ATL_NO_VTABLE CTestAbstractMeasurement : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTestAbstractMeasurement, &CLSID_TestAbstractMeasurement>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CTestAbstractMeasurement();

DECLARE_REGISTRY_RESOURCEID(IDR_TESTABSTRACTMEASUREMENT)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CTestAbstractMeasurement)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	enum EMeasurementType{kNoType, kAngle, kBearing, kDistance};

	// result logger for recording test result
	ITestResultLoggerPtr m_ipResultLogger;

	std::string m_strTestFilesFolder;

	// read in the .dat file, for each line of text, there is a test case
	void testAngle();
	void testBearing();
	void testDistance();

	void executeAngleTest(const std::string& strForTest);
	void executeBearingTest(const std::string& strForTest);
	void executeDistanceTest(const std::string& strForTest);

	std::vector<CString> parseInput(const std::string& strForTest, char cDelimiter = ';');
	void setTestFileFolder(IVariantVectorPtr ipParams, const std::string &strTCLFile);
};

