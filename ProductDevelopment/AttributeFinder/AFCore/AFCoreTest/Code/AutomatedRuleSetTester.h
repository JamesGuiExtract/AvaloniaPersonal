// AutomatedRuleSetTester.h : Declaration of the CAutomatedRuleSetTester

#pragma once

#include "resource.h"       // main symbols

#include <string>
#include <vector>
#include <map>
#include <utility>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CAutomatedRuleSetTester
class ATL_NO_VTABLE CAutomatedRuleSetTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAutomatedRuleSetTester, &CLSID_AutomatedRuleSetTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CAutomatedRuleSetTester();
	~CAutomatedRuleSetTester();

DECLARE_REGISTRY_RESOURCEID(IDR_AUTOMATEDRULESETTESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAutomatedRuleSetTester)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY2(IDispatch, ITestableComponent)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
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
	ITestResultLoggerPtr		m_ipResultLogger;

	IAttributeFinderEnginePtr	m_ipAttrFinderEngine;

	// store current found attributes in vector
	IIUnknownVectorPtr			m_ipCurrentAttributes;

	// used to expand the tags for the voa and eav files
	IFAMTagManagerPtr			m_ipFAMTagManager;

	IAFUtilityPtr				m_ipAFUtility;

	// Map objects to track the total of individual values found
	map<string, long> m_mapTotalExpected;
	map<string, long> m_mapTotalCorrectFound;
	map<string, long> m_mapTotalIncorrectlyFound;

	// When true attribute comparisons will be case sensitive
	bool m_bCaseSensitive;

	// When true only the final stats will be output
	bool m_bOutputFinalStats;

	// To Allow only processing files with existing eav file
	bool m_bEAVMustExist;

	bool m_bIgnoreTextFiles;

	//////////
	// Methods
	//////////
	//========================================================================================
	// get attribute name/value/type and all its sub attributes in to a string
	// nLevel -- top level attribute is at level 0. its sub attribute is level 1,
	//			 and so on.
	string attributeAsString(IAttributePtr ipAttribute, int nLevel);

	//========================================================================================
	// get a compare string containing all of the attributes in the specified vector
	string getAttributesCompareString(IIUnknownVectorPtr ipAttributes);

	//========================================================================================
	// gets the qualified attribute name for the specified attribute
	string getQualifiedName(IAttributePtr ipAttribute, const string& strQualifiedAttrName,
		const string& strSeparator);
	
	//========================================================================================
	// get the attribute as a string with the following format 'Name|Value|Type'
	string getTopLevelAttributeString(IAttributePtr ipAttribute);

	//========================================================================================
	// returns the total number of 'lines' (attribute/subattribute/subsubattribute/etc)
	// within an attribute (an attribute with no sub attributes will have a size of 1)
	long getAttributeSize(IAttributePtr ipAttribute);

	//========================================================================================
	// Computes the match score for the expected and found attribute and returns a pair
	// containing the score and whether it matched or not - (score, matched)
	// Score computed by the following algorithm: 1 point is given for an attribute or
	// sub-attribute match, 0 for no match, and -1 for a false positive (attribute matched
	// but was not the best match or was incorrectly found)
	// Example score computation:
	// Expected						Found				Score
	// Test|N/A						Test|N/A			3
	// .Component|PSA				.Component|PSA		1
	// ..Flag|H						..Flag|H			1
	// ..Range|0.0-4.0				..Range|0.0-4.0		-1 (false positive since other range is better match)
	// ...Min|0.0					...Min|0.0			-1
	// ...Max|4.0					...SMax|4.0			-1
	// ..Units|ng/ML				..Range|0.0-4.0		1
	// ..Value|5.2					...Min|0.0			1
	// ..Status|Bad					...Max|4.0			1
	//								..Status|Good		-1 (false positive since did not match anything)
	//								..Units|ng/ML		1
	//								..Value|5.2			1
	pair<long, bool> computeScore(IAttributePtr ipExpected, IAttributePtr ipFound);

	//========================================================================================
	// Compare the current found attributes with expected attributes
	bool compareAttributes(IIUnknownVectorPtr ipFoundAttributes, 
						   IIUnknownVectorPtr ipExpectedAttributes);
	
	//========================================================================================
	// Read input file, which is a text file for testing a rule set.
	// Each rule set file will have its own input text file.
	// This method will read in all line and concatenate them into a string
	string fileAsString(const string& strInputFileName);

	//========================================================================================
	// Get current document classification info, such as the type of the
	// document and the confidence level (Sure, Probable, Maybe)
	string getDocumentClassificationInfo(IAFDocumentPtr ipAFDoc);

	//========================================================================================
	// Get document probability using Sure, Probable, Maybe or Zero
	string getDocumentProbabilityString(const string& strProbability);

	//========================================================================================
	// Parse the specified file and return all non-metadata attributes in an IUnknownVector
	IIUnknownVectorPtr getAttributesFromFile(const string& strAttrFileName);

	//========================================================================================
	// Get the rule id for the rule that actually extracts the attributes
	string getRuleID(IAFDocumentPtr ipAFDoc);

	//========================================================================================
	// interpret the line of text to see if it contains another dat file
	// for further process, or it contains simply a test case
	// strCurrentDatFileName -- current .dat file that's been processed
	// nItemNumber -- each actual line number (exclude any empty lines) in the 
	//				  current .dat file
	void interpretLine(const string& strLineText,
		const string& strCurrentDatFileName, const string& strCaseNum);

	//========================================================================================
	// process the line that has another .dat file that requires further parsing
	// strCaseNumPrefix -- by default, it's "", which means this strDatFileName
	//			  is the master .dat file.
	//			  For any subsequent dat files, this strCaseNoPrefix must be used with
	//			  the case number.
	//			  It will have the format as 1-1-3 : first '1' indicates where in
	//			  the master file the dat file (1) is, i.e. which line it is located (not counting
	//			  any empty or commented lines). Second '1' indicates where in its parent dat 
	//			  file this dat file(2) is located. '3' indicates the actual test case's location
	//			  in the aforementioned dat file(2).
	void processDatFile(const string& strDatFileName, const string& strCaseNumPrefix = "");

	//========================================================================================
	// process the line that has three columns of info. They are .rsd file, 
	// input text file (.txt, .uss), and expected output attribute value file (.eav)
	// As a type of <TESTCASE>, each line can have 3-4 tokens 
	// for rsd file, input text file,  and eav file 
	void processTestCase(const string& strRSDFile,
		const string& strInputImageFile,
		const string& strInputTextFile,
		const string& strVOAFile,
		const string& strEAVFile,
		const string& strTestCaseTitle,
		const string& strTestCaseNo);

	//========================================================================================
	// process the line that has three columns of info. They are .rsd file, 
	// input text files' folder
	// As a type of <TESTFOLDER>, each line can have 2 tokens (excluding the tag)
	void processTestFolder(const string& strRSDFile,
		const string& strTestFolder,
		const string& strVOAFilesExpression,
		const string& strEAVFilesExpression,
		const string& strDatFileName,
		const string& strTestCaseNo);

	//---------------------------------------------------------------------------------------------
	// PROMISE:	To throw an exception if this object is not licensed to run
	void validateLicense();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return the full path to the master test file.  The path is 
	//			computed differently in debug and release builds.
	const string getMasterTestFileName(IVariantVectorPtr ipParams, const string &strTCLFile) const;
	//---------------------------------------------------------------------------------------------
	// PROMISE: To compute and return the absolute path from the specified parent
	//			file and relative file names.  If the computed absolute path is
	//			does not exist, an exception will be thrown
	const string getAndValidateAbsolutePath(
		const string& strParentFile, const string& strRelativeFile);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To compare the values of the found and expected attributes and return 
	//			true if an exact match and false otherwise.  Also compares each individual 
	//			attribute and sub-attribute to determine if the found value matches the expected
	//			and increment the map count using a Qualified name:
	//				<attributeName>[\attributeType].<sub-attribute Name>[\sub-attribute Type]
	//				with the .<sub-attribute Name>[\sub-attribute Type] repeated as many times as
	//				needed.
	//			This function is called recursively
	// ARGS:	ipFound is the vector of found attributes can be NULL
	//			ipExpected is the vector of expected attributes can be NULL
	//			strQualifiedAttrName is the Qualified name of the previously handled
	//				attribute/sub attribute
	bool compareResultVectors(IIUnknownVectorPtr ipFound, IIUnknownVectorPtr ipExpected,
		const string& strQualifiedAttrName = "");
	//---------------------------------------------------------------------------------------------
	// Adds a Test case containing the results of the attribute/sub attributes totals generated by 
	// compareResultVectors
	void addAttributeResultCase();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return a vector of attributes from an "*" delimited string
	//			Will replace all \n references in the string with an actual newline
	// ARGS:	strInlineEAV - string containing a "*" delimited EAV style attribute list
	IIUnknownVectorPtr processInlineEAVString(const string &strInlineEAV);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return a string with the tags expanded as specified in strInput using
	//			the strSourcDocName as the source
	const string expandTagsAndTFE(const string &strInput, string &strSourceDocName);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To count each of the expected attributes as well as its sub attributes
	void countExpectedAttributes(IIUnknownVectorPtr ipExpected,
		const string& strQualifiedAttrName = "");
	//---------------------------------------------------------------------------------------------
};
