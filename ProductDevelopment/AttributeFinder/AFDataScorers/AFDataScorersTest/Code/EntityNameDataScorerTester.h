// EntityNameDataScorerTester.h : Declaration of the CEntityNameDataScorerTester

#pragma once

#include "resource.h"       // main symbols

#include <string>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CEntityNameDataScorerTester
class ATL_NO_VTABLE CEntityNameDataScorerTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEntityNameDataScorerTester, &CLSID_EntityNameDataScorerTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CEntityNameDataScorerTester();

DECLARE_REGISTRY_RESOURCEID(IDR_ENTITYNAMEDATASCORERTESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEntityNameDataScorerTester)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ITestableComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IEntityNameDataScorerTester
public:
// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

private:
	////////////
	// Variables
	////////////
	ITestResultLoggerPtr		m_ipResultLogger;
	IEntityNameDataScorerPtr	m_ipENDScorer;

	//////////
	// Methods
	//////////

	//---------------------------------------------------------------------------------------------
	// PROMISE: To return the full path to the master test file.  The path is 
	//			computed differently in debug and release builds.
	const std::string getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const;

	//---------------------------------------------------------------------------------------------
	// Processes lines in specified file
	void	processFile( std::string strFile );

	//---------------------------------------------------------------------------------------------
	// Parses specified line and executes the testcase or processes items in the provided file
	void	processLine( std::string strLine, const std::string& strFileWithLine );

	//---------------------------------------------------------------------------------------------
	// Processes ENDSlog file as input
	void	processENDSLog( std::string strFile, bool bGroupOnly = false );

	//---------------------------------------------------------------------------------------------
	// Runs test of scorer using the value provided and compares against the score
	// This test uses the GetDataScore1 method
	void doSingleTest( long nExpectedScore, std::string& strAttributeValue );

	//---------------------------------------------------------------------------------------------
	// Runs test of scorer using the value provided and compares against the score
	// This test uses the GetDataScore2 method
	void doGroupTest( long nExpectedGroupScore, std::vector<std::string>& vecAttrValues );

	//---------------------------------------------------------------------------------------------
	// PROMISE:	To throw an exception if this object is not licensed to run
	void validateLicense();
};

