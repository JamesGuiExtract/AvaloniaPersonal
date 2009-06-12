// EntityNameSplitterTester.h : Declaration of the CEntityNameSplitterTester

#pragma once

#include "resource.h"       // main symbols

#include <string>
#include <vector>
#include <fstream>

/////////////////////////////////////////////////////////////////////////////
// CEntityNameSplitterTester
class ATL_NO_VTABLE CEntityNameSplitterTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEntityNameSplitterTester, &CLSID_EntityNameSplitterTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CEntityNameSplitterTester();

DECLARE_REGISTRY_RESOURCEID(IDR_ENTITYNAMESPLITTERTESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEntityNameSplitterTester)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY2(IDispatch, ITestableComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	////////////
	// Variables
	////////////
	// Object that splits Entities
	IAttributeSplitterPtr	m_ipNameSplitter;

	// For automated testing
	ITestResultLoggerPtr	m_ipResultLogger;
	ISpatialStringPtr		m_ipTestString;
	ISpatialStringPtr		m_ipExpectedString;

	// Collection of current nodes for storing new Attributes
	std::vector<IIUnknownVectorPtr> m_vecCurrentLevels;

	//////////
	// Methods
	//////////

	//========================================================================================
	// Add specified Attribute to active collection of Attributes or 
	// SubAttributes at the specified level
	void addAttributeLevel(IAttributePtr ipNewAttribute, int iLevel);

	//========================================================================================
	// Get Name / Value / Type into a string - including subattributes
	// Specified Child Level indicates how many leading periods will precede the Attribute name
	std::string attributeAsString(IAttributePtr ipAttribute, int iChildLevel);

	//========================================================================================
	// Compares Attribute Found and Attribute Expected and sets test result
	// Returns: true if Attributes match, false otherwise
	bool	checkTestResults(IAttributePtr ipTest, IAttributePtr ipExpected);

	//========================================================================================
	// Creates a test case with specified label, and specified Found and 
	// Expected Attributes
	void	doTest(std::string strLabel, IAttributePtr ipTest, IAttributePtr ipExpected);

	//========================================================================================
	// Retrieves the next Attribute from the specified stream.  Resets the position 
	// pointer of the stream if the beginning of the next test case is found.
	IAttributePtr	getAttribute(std::ifstream& ifs);

	//========================================================================================
	// Returns number of leading period characters in Name
	int		getAttributeLevel(std::string strName);

	//---------------------------------------------------------------------------------------------
	// PROMISE: To return the full path to the master test file.  The path is 
	//			computed differently in debug and release builds.
	const std::string getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const;

	//========================================================================================
	// Creates the Entity Name Splitter object
	void	prepareTests();

	//========================================================================================
	// Processes lines in specified file
	void	processFile(std::string strFile);

	//========================================================================================
	// Parses specified line and executes the testcase or processes items in the provided file
	void	processLine(std::string strLine, const std::string& strFileWithLine, 
		std::ifstream &ifs);

	//---------------------------------------------------------------------------------------------
	// PROMISE:	To throw an exception if this object is not licensed to run
	void	validateLicense();
};
