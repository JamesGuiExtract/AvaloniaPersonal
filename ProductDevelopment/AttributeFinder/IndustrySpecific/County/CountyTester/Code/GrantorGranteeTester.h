// GrantorGranteeTester.h : Declaration of the CGrantorGranteeTester

#pragma once

#include "resource.h"       // main symbols

#include <string>
#include <map>
#include <set>
#include <vector>
#include <fstream>

/////////////////////////////////////////////////////////////////////////////
// CGrantorGranteeTester
class ATL_NO_VTABLE CGrantorGranteeTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CGrantorGranteeTester, &CLSID_GrantorGranteeTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CGrantorGranteeTester();
	~CGrantorGranteeTester();

DECLARE_REGISTRY_RESOURCEID(IDR_GRANTORGRANTEETESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CGrantorGranteeTester)
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

	enum ETestCaseResult
	{
		kNoMatch,
		kPartialMatch,
		kFullMatch
	};
	
	struct TestCaseData
	{
		TestCaseData(const std::string& strRSDFile, const std::string& strUSSFile, const std::string& strEAVFile, const std::string &strRuleID, const ETestCaseResult eResult);
		TestCaseData();
		
		std::string			m_strRSDFilename;
		std::string			m_strUSSFilename;
		std::string			m_strEAVFilename;
		std::string			m_strRuleID;
		ETestCaseResult		m_eMatchResult;
	};

	// So Code in TestCaseData has access to ETestCaseResult enums
	friend TestCaseData;

	// Structure to hold the rule capture rate information at
	// document and attribute levels
	struct RuleCaptureData
	{
		RuleCaptureData();

		// matching file list for a rule
		std::vector<TestCaseData> m_vecTestCasesInfo;	
		// passed attributes for a rule
		long m_lNumOfPassedAttributes;				
		// failed attributes for a rule
		long m_lNumOfFailedAttributes;				
		// duplicate attributes for a rule
		long m_lNumOfDuplicateAttributes;			
	};

	class NamePlusTypeStats
	{
	public:
		NamePlusTypeStats();
		long m_nExpected;
		long m_nPassed;
		long m_nExtra;
	};

	ITestResultLoggerPtr		m_ipResultLogger;
	
	IAttributeFinderEnginePtr	m_ipAttrFinderEngine;
	
	// store current found attributes in vector
	IIUnknownVectorPtr			m_ipCurrentAttributes;

	IAFUtilityPtr m_ipAFUtility;

	// Total number of test attributes for current batch
	long m_nTotalTestAttributes;

	// Number of passed attributees
	long m_nNumOfPassedAttributes;

	// Number of other, unmatched attributees
	long m_nNumOfOtherAttributes;

	// this final note lists any test case that passes and has
	// related note file in the same directory
	std::string m_strFileSucceedWithNote;

	// this final note lists any test case that is failed and
	// doesn't have any related note
	std::string m_strFileFailedWithoutNote;

	// map to hold capture information for a particular rule
	std::map<std::string, RuleCaptureData > m_mapRuleCaptureInfo;

	std::map<std::string, NamePlusTypeStats> m_mapNamePlusType;

	// output filenames
	std::string m_strPartialMatchFile;
	std::string m_strFullMatchFile;

	// output files for full and partial matches
	std::ofstream m_FullMatchFile;
	std::ofstream m_PartialMatchFile;


	// Set used to keep the same file from being ran multiply times in test run for <READ_FROM_FILE_TAG> case
	std::set<std::string> m_setProcessedFiles;

	////////////
	// Methods
	////////////

	//========================================================================================
	// get attributes' name/value/type into a string
	std::string attributesAsString(IIUnknownVectorPtr ipAttributes);

	//========================================================================================
	// Compare the current found attributes with expected attributes.
	// Also updates the Total Attribute count and Passed Attribute count
	bool compareAttributes(IIUnknownVectorPtr ipFoundAttributes, 
						   IIUnknownVectorPtr ipExpectedAttributes,
						   const std::string& strRuleID);
	
	//========================================================================================
	// Read input file, which is a text file for testing a rule set.
	// Each rule set file will have its own input text file.
	// This method will read in all line and concatenate them into a string
	std::string fileAsString(const std::string& strInputFileName);

	//========================================================================================
	// Get current document classification info, such as the type of the
	// document and the confidence level (Sure, Probable, Maybe)
	std::string getDocumentClassificationInfo(IAFDocumentPtr ipAFDoc);

	//========================================================================================
	// Get document probability using Sure, Probable, Maybe or Zero
	std::string getDocumentProbabilityString(const std::string& strProbability);
	//========================================================================================
	// parse the file that contains all expected attributes and return them in
	// an IIUnknownVector of IAttributes
	IIUnknownVectorPtr getExpectedAttributes(const std::string& strExpectedAttrFileName);

	//========================================================================================
	// Take the ipAttribute and look at its sub attributes, extract out the second
	// level sub attributes with the name of "Person" or "Company" and make them into
	// individual attributes of the same type and with the Name as "Grantor-Grantee".
	IIUnknownVectorPtr getGrantorGranteeAttributes(IAttributePtr ipAttribute);

	//========================================================================================
	// Get the rule id for the rule that actually extracts the attributes
	std::string getRuleID(IAFDocumentPtr ipAFDoc);

	//========================================================================================
	// interpret the line of text to see if it contains another dat file
	// for further process, or it contains simply a test case
	// strCurrentDatFileName -- current .dat file that's been processed
	// nItemNumber -- each actual line number (exclude any empty lines) in the 
	//				  current .dat file
	void interpretLine(const std::string& strLineText,
		const std::string& strCurrentDatFileName, const std::string& strCaseNum);

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
	void processDatFile(const std::string& strDatFileName, const std::string& strCaseNumPrefix = "");

	//========================================================================================
	// process the line that has three columns of info. They are 
	// input text file and expected output attribute value file (.eav)
	// As a type of <TESTCASE>, each line can have 2-3 tokens (excluding the tag)
	// for rsd file, input text file, eav file and optionally the attached note for
	// this test case.
	void processTestCase(const std::string& strRSDFile,
		const std::string& strInputTextFile,
		const std::string& strEAVFile,
		const std::string& strTestCaseTitle,
		const std::string& strTestCaseNo,
		const std::string& strExpectedRule = "" );

	//========================================================================================
	// process the line that has three columns of info. They are .rsd file, 
	// input text files' folder
	// As a type of <TESTFOLDER>, each line can have 1 token (excluding the tag)
	void processTestFolder(const std::string& strRSDFile,
		const std::string& strTestFolder,
		const std::string& strEAVFilesFolder,
		const std::string& strTestCaseTitle,
		const std::string& strTestCaseNo);

	//========================================================================================
	// Dump the rule capture information to two files: one for partial matches, and one
	// for full matches
	void dumpRuleCaptureInfo(std::ostream& partialMatches, std::ostream& fullMatches);

	//========================================================================================
	// Creates a comment line from a given string and outputs it to the given stream
	void createCommentLine(std::ostream& output, const std::string& strComment);
	
	//========================================================================================
	// Creates a <TESTCASE> line from the provided RSD, USS, EAV files and outputs it 
	// to the given stream
	void createTestCaseLine(std::ostream& output, const std::string& strRSDFile, const std::string& strUSSFile, const std::string& strEAVFile);

	//========================================================================================
	// Creates a <FILE> line from the provided file name and outputs it to the given stream
	void createTestFileLine(std::ostream& output, const std::string& strFilename, const bool bCommented);

	//========================================================================================
	// Returns a vector of TestCaseData that will contain the uss files obtained from the file lines
	// containting the tag.  If that line also contains a eav file name that file will be in the 
	// TestCaseData structure
	std::vector<TestCaseData> getTestCaseFilesUsingTag( const std::string& strFileName, const std::string& strTag );
	
	//========================================================================================
	// Fills in the RSDFile and EAVFile of the given TestCaseFiles struct from the given mapfile
	//		RSDFile and EAVfile path is found by comparing a path to the path of the uss file
	//		in the structure, if found the line contains the RSDFile to use and a path relative
	//		to the matched path to use to find the EAVFile to  use if it was not given already
	void getRSDAndEAVFileName ( const std::string& strMapFile, TestCaseData& testCaseData );
	
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To throw an exception if this object is not licensed to run
	void validateLicense();
	//---------------------------------------------------------------------------------------------
};

