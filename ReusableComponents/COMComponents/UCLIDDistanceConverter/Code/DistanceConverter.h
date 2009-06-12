// DistanceConverter.h : Declaration of the CDistanceConverter

#pragma once

#include "resource.h"       // main symbols

#include <map>
#include <string>
#include <vector>

#import "..\..\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

/////////////////////////////////////////////////////////////////////////////
// CDistanceConverter
class ATL_NO_VTABLE CDistanceConverter : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDistanceConverter, &CLSID_DistanceConverter>,
	public ISupportErrorInfo,
	public IDispatchImpl<IDistanceConverter, &IID_IDistanceConverter, &LIBID_UCLID_DISTANCECONVERTERLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CDistanceConverter();
	~CDistanceConverter();

DECLARE_REGISTRY_RESOURCEID(IDR_DISTANCECONVERTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDistanceConverter)
	COM_INTERFACE_ENTRY(IDistanceConverter)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IDistanceConverter)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IDistanceConverter
	STDMETHOD(ConvertDistanceInUnit)(/*[in]*/ double dInValue, 
									 /*[in]*/ EDistanceUnitType eInUnit, 
									 /*[in]*/ EDistanceUnitType eOutUnit, 
									 /*[out, retval]*/ double* dOutValue);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	////////////////////////
	// Helper functions

	// initializes the unit type to value in centimeters pair in the m_mapUnitTypeToValueInCM
	void initTypeToValueMap();
	// read in unit definition file and put them in a map
	void initUnitStrings();
	// parse the string into couple of tokens
	std::vector<std::string> parseInput(const std::string& strForTest, char cDelimiter = ';');
	// initiates the test, like reading the file, set the start/end for each test case, etc.
	void testConverter();
	// execute the test
	void executeDistConverterTest(const std::string& strForTest);
	// is this component licensed
	void validateLicense();
	// determines and sets the value of the test file folder
	void setTestFileFolder(IVariantVectorPtr ipParams, const std::string &strTCLFile);




	////////////////////////
	// Member variables:

	// stores the unit type value in terms of centimeters. 
	// For instance, kFeet and 30.48 pair (1 feet = 30.48 centimers).
	std::map<EDistanceUnitType, double> m_mapUnitTypeToValueInCM;

	// stores the unit definition string
	std::map<std::string, int> m_mapUnitStrToType;

	// result logger for recording test result
	ITestResultLoggerPtr m_ipResultLogger;

	std::string m_strTestFilesFolder;
};
