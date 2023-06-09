// IDShieldTester.h : Declaration of the CIDShieldTester

#pragma once
#include "stdafx.h"
#include "resource.h"       // main symbols

#include "RedactionTester.h"

#include <AttributeTester.h>
#include <SafeTwoDimensionalArray.h>

#include <string>
#include <vector>
#include <map>
#include <set>
#include <memory>
#include <fstream>

using namespace std;

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

//-------------------------------------------------------------------------------------------------
// MatchInfo struct
//
// PURPOSE: To hold information about a specific expected attribute and a specific found attribute.
//-------------------------------------------------------------------------------------------------
class MatchInfo
{
public:
	// variables to hold the area information
	double m_dAreaOfExpectedRedaction;
	double m_dAreaOfFoundRedaction;
	double m_dAreaOfOverlap;

	// pointer to the attributes
	IAttributePtr m_ipExpectedAttribute;
	IAttributePtr m_ipFoundAttribute;

	// overloaded copy constructor to insure each element gets copied
	// we only copy the smart pointers, no need to make a deep copy
	MatchInfo operator=(MatchInfo miCopy)
	{
		m_dAreaOfExpectedRedaction = miCopy.m_dAreaOfExpectedRedaction;
		m_dAreaOfFoundRedaction = miCopy.m_dAreaOfFoundRedaction;
		m_dAreaOfOverlap = miCopy.m_dAreaOfOverlap;

		m_ipExpectedAttribute = miCopy.m_ipExpectedAttribute;
		m_ipFoundAttribute = miCopy.m_ipFoundAttribute;

		return *this;
	}

	// compute the percent of expected area that was redacted by this found attribute
	double getPercentOfExpectedAreaRedacted()
	{
		double dPercent = (100.0 * (m_dAreaOfOverlap / m_dAreaOfExpectedRedaction));
		
		return dPercent;
	}
};

//-------------------------------------------------------------------------------------------------
// CIDShieldTester
//-------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CIDShieldTester :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CIDShieldTester, &CLSID_IDShieldTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<IIDShieldTester, &IID_IIDShieldTester, &LIBID_EXTRACTREDACTIONTESTERLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CIDShieldTester();
	~CIDShieldTester();

DECLARE_REGISTRY_RESOURCEID(IDR_IDSHIELDTESTER)

BEGIN_COM_MAP(CIDShieldTester)
	COM_INTERFACE_ENTRY(IIDShieldTester)
	COM_INTERFACE_ENTRY2(IDispatch, IIDShieldTester)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ITestableComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// IIDShieldTester
	STDMETHOD(get_OutputFileDirectory)(BSTR *pVal);
	STDMETHOD(GenerateCustomReport)(BSTR bstrReportTemplate);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);
	STDMETHOD(raw_RunInteractiveTests)();
	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

private:

	/////////////////
	// Classes
	/////////////////

	// Encapsulates statistics for a test case.
	class TestCaseStatistics
	{
	public:
		TestCaseStatistics();
		
		// Reset all counters back to zero
		void reset();
		
		// Add one set of statistics to another. In this case m_bTestCaseResult will only be true
		// if both test cases are true
		TestCaseStatistics& operator += (const TestCaseStatistics& otherStatistics);

		// Whether the test case succeeded
		bool m_bTestCaseResult;

		// Statistics counters
		unsigned long m_ulTotalExpectedRedactions;
		unsigned long m_ulExpectedRedactionsInSelectedFiles;
		unsigned long m_ulExpectedRedactionsInSelectedPages;
		unsigned long m_ulFoundRedactions;
		unsigned long m_ulNumCorrectRedactions;
		unsigned long m_ulNumFalsePositives;
		unsigned long m_ulNumOverRedactions;
		unsigned long m_ulNumUnderRedactions;
		unsigned long m_ulNumMisses;
	};


	class IDShieldCounterClass
	{
	// counters
	public:
		IDShieldCounterClass();
		IDShieldCounterClass(const IDShieldCounterClass& c);

		IDShieldCounterClass& operator +=(const IDShieldCounterClass& rhs);

		void clear();

		// Statistics for files selected for automated redaction
		CIDShieldTester::TestCaseStatistics m_automatedStatistics;

		// Statistics for files selected for review	
		CIDShieldTester::TestCaseStatistics m_verificationStatistics;

		unsigned long m_ulTotalExpectedRedactions; 
		unsigned long m_ulNumCorrectRedactionsByDocument; 
		unsigned long m_ulNumCorrectRedactionsByPage; 
		unsigned long m_ulNumOverRedactions; 
		unsigned long m_ulNumUnderRedactions; 
		unsigned long m_ulNumMisses;
		unsigned long m_ulTotalFilesProcessed; 
		unsigned long m_ulNumFilesWithExpectedRedactions; 
		unsigned long m_ulNumFilesSelectedForReview;
		unsigned long m_ulNumPagesInFilesSelectedForReview; 
		unsigned long m_ulNumPagesSelectedForReview;
		unsigned long m_ulNumExpectedRedactionsInReviewedFiles; 
		unsigned long m_ulNumExpectedRedactionsInRedactedFiles;
		unsigned long m_ulNumFilesAutomaticallyRedacted; 
		unsigned long m_ulNumFilesWithOverlappingExpectedRedactions;
		unsigned long m_ulTotalPages; 
		unsigned long m_ulNumPagesWithExpectedRedactions; 
		unsigned long m_ulDocsClassified;
	};

	/////////////////
	// Variables
	/////////////////

	IAFUtilityPtr m_ipAFUtility;

	IMiscUtilsPtr m_ipMiscUtils;
	ITagUtilityPtr m_ipTagUtility;

	// Object to find attributes if necessary
	IAttributeFinderEnginePtr	m_ipAttrFinderEngine;

	// Result Logger Dialog pointer
	ITestResultLoggerPtr m_ipResultLogger;

	// Verification conditions
	string m_strVerificationCondition;
	string m_strVerificationQuantifier;
	set<string> m_setVerificationCondition;
	EAttributeQuantifier m_eaqVerificationQuantifier;
	unique_ptr<AttributeTester> m_apVerificationTester;

	// Redaction conditions
	string m_strRedactionCondition;
	string m_strRedactionQuantifier;
	set<string> m_setRedactionCondition;
	EAttributeQuantifier m_eaqRedactionQuantifier;
	unique_ptr<AttributeTester> m_apRedactionTester;

	// Redaction query
	string m_strRedactionQuery;

	// The attribute types to be included in the test.
	set<string> m_setTypesToBeTested;

	// Flags for statistic output settings
	bool m_bOutputHybridStats;

	// Specifies whether only automated statistics should be calculated.
	bool m_bOutputAutomatedStatsOnly;

	// Specifies to output stats assuming verifiers visited all pages of any document containing any
	// sensitive data.
	bool m_bOutputVerificationByDocumentStats;
	
	// Specifies to output stats assuming visited only pages that contained any sensitive data.
	bool m_bOutputVerificationByPageStats;

	// Keeps the counts for all documents/pages
	IDShieldCounterClass m_iccCounters;

	// Map of field names available in a custom report to their unsigned long values.
	map<string, unsigned long*> m_mapStatisticFields;

	// Map to keep track of counts for document types
	map<string, IDShieldCounterClass> m_mapDocTypeCounts;

	// vector to hold attributes for the test output VOA vector
	IIUnknownVectorPtr m_ipTestOutputVOAVector;

	// Path to which TestOutput files should be written.
	string m_strTestOutputPath;

	// flag to check whether the OuptputAttributeNameFileList setting is on or off
	bool m_bOutputAttributeNameFilesList;

	// Amount that a raster zone overlaps another zone to be a 'matching' raster zone
	double m_dOverlapLeniencyPercent;

	// maximum amount that a raster zone can overlap another zone before being considered
	// an over redaction
	double m_dOverRedactionERAP;

	// Minimum percentage of overlap consider two zones as overlapping. (as a percentage of the
	// area of the smaller zone).
	double m_dOverlapMinimumPercent;

	// Directory for output log files
	string m_strOutputFileDirectory;
	string m_strExplicitOutputFileDirectory;
	bool m_bOutputDirectoryInitialized;

	// Doc types to select for verification
	set<string> m_setDocTypesToBeVerified;

	// Doc types to select for automatic redaction
	set<string> m_setDocTypesToBeAutomaticallyRedacted;

	// Count of the number of files that use an existing VOA file. This is used in the disclaimer
	// note about multiply classified documents.
	unsigned long m_ulNumFilesWithExistingVOA;

	/////////////////
	// Methods
	/////////////////
	void validateLicense();

	// Process the dat file line by line
	void processDatFile(const string& strDatFileName);

	// Interpret each line in the dat file and call the appropriate method to handle each case
	void interpretLine(const string& strLineText, const string& strCurrentDatFileName);

	// Handle SETTINGS tag
	void handleSettings(const string& strSettingsText);
	
	// Handle TESTFOLDER tag
	void handleTestFolder(const string& strRulesFile, const string& strImageDir, 
							string strFoundVOAFile, string strExpectedVOAFile,
							const string& strCurrentDatFileName);

	// Handle TESTCASE tag
	void handleTestCase(const string& strRulesFile, const string& strImageFile,
						const string& strSourceDoc, string& strFoundVOAFile, 
						string& strExpectedVOAFile, int iTestCaseNum,
						const string& strCurrentDatFileName);

	// PROMISE: Display statistics for the test run and display calculated percentages where
	//			appropriate. Also output the document type(s) of the files.
	void displaySummaryStatistics();

	// Creates a custom report using the specified report template.
	void generateCustomReport(string strReportTemplate);

	// Evaluates and expands all field parameters and mathematical expressions in a report line.
	string evaluateCustomReportParameters(const string& strSourceLine);

	// Locates the first instance of one of the specified mathematical expressions in a line of a
	// custom report, evaluates it, and returns the expression's location, length and result.
	bool evaluateCustomReportMathematicalExpression(const string &strLine,
		const vector<char>& vecAllowedOperations, size_t &rnPos, size_t &rnLen, string &strResult);

	// Wraps preliminary expanded custom report arguments to make them easily identifiable for
	// evaluating mathematical expressions.
	string CIDShieldTester::wrapPrelimArg(const string &strArgument);

	// PROMISE: Logs the statistics associated with the specified TestCaseStatistics instance.
	string displayStatisticsSection(const CIDShieldTester::TestCaseStatistics& sectionStatistics,
									bool bVerificationStatistics);

	// PROMISE: This method uses the expected and found attribute vectors to increment the count
	//			of m_ulTotalFilesProcessed, update the m_ulTotalExpectedRedactions,
	//			update m_ulNumFilesWithExpectedRedactions, and then calls the two analyze methods.
	//			If strTestOutputVOAFile is specified, a testoutput voa file will be written to this
	//			path.
	bool updateStatisticsAndDetermineTestCaseResult(IIUnknownVectorPtr ipExpectedAttributes,
													 IIUnknownVectorPtr ipFoundAttributes,
													 const string& strSourceDoc,
													 const string& strTestOutputVOAFile,
													 IDShieldCounterClass &iccCounts);

	// PROMISE: This method counts all expected attributes that are on a page that has at least one
	//			HC/MC/LC/Clue attribute on the same page.
	unsigned long countExpectedAttributesOnSelectedPages(IIUnknownVectorPtr ipExpectedAttributes,
														 IIUnknownVectorPtr ipFoundAttributes,
														 unsigned long &rnSelectedPages);
	
	// PROMISE: This method updates the count for m_ulNumFilesSelectedForReview 
	//			and m_ulNumExpectedRedactionsInReviewedFiles. Returns true if the
	//			file would have been verified.
	//			If strTestOutputVOAFile is specified, a testoutput voa file will be written to this
	//			path.
	bool analyzeDataForVerificationBasedRedaction(IIUnknownVectorPtr ipExpectedAttributes,
												  IIUnknownVectorPtr ipFoundAttributes,
												  bool bSelectedForVerification,
												  const string& strSourceDoc,
												  const string& strTestOutputVOAFile,
												  unsigned long ulNumPagesInDoc,
												  IDShieldCounterClass &iccCounts);

	// PROMISE: This method updates m_ulNumCorrectRedactions and m_ulNumFalsePositives using
	//			the analyzeExpectedAndFoundAttributes() method.
	//			Returns true if there were no false positives OR the ulNumCorrectlyFound is equal to
	//			the number of expected attributes
	//			Returns false otherwise.
	//			If strTestOutputVOAFile is specified, a testoutput voa file will be written to this
	//			path.
	bool analyzeDataForAutomatedRedaction(IIUnknownVectorPtr ipExpectedAttributes,
										  IIUnknownVectorPtr ipFoundAttributes,
										  bool bSelectedForAutomatedProcess,
										  const string& strSourceDoc,
										  const string& strTestOutputVOAFile,
										  IDShieldCounterClass &iccCounts);

	// PROMISE: This method compares two spatial strings using the GetAreaOverlappingWith method from 
	//          IRasterZone. If the two spatial strings overlap within the percentage specified in the 
	//          registry by the RasterZoneOverlapLeniency key, then returns true, otherwise false.
	bool spatiallyMatches(ISpatialStringPtr ipExpectedSS, ISpatialStringPtr ipFoundSS);

	// PROMISE: This method loops through the vectors of expected and found attributes in order
	//			to compare each raster zone and compare them using spatiallyMatches.
	// ARGS:	bDocumentSelected indicates whether the document was selected for automatic
	//			redaction or verification (depending on the test being executed).
	//			ipExpectedAttributesOnSelectedPages indicates that when stats are being generated
	//			based on verifiers visiting only pages containing sensitive data, what expected
	//			attributes exist on those pages. It can be null if ByPage stats are not needed.
	//			If strTestOutputVOAFile is specified, a testoutput voa file will be written to this
	//			path.
	// RETURNS: The statistics calculated from running the analysis.
	CIDShieldTester::TestCaseStatistics analyzeExpectedAndFoundAttributes(
											IIUnknownVectorPtr ipExpectedAttributes, 
											IIUnknownVectorPtr ipFoundAttributes,
											unsigned long ulExpectedAttributesOnSelectedPages,
											bool bDocumentSelected,
											const string& strSourceDoc,
											const string& strTestOutputVOAFile);

	// PROMISE: Determines whether the found redaction at the specified index of the array is an
	//			over-redaction by comparing it to all expected attributes. This method assumes
	//			that the found redaction has already been determined to be a "correct" redaction
	//			which is now required of any redaction to be considered an "over-redaction"
	//			[FlexIDSCore:4036]
	// RETURNS: true if the found redaction is an over-redaction
	bool getIsOverredaction(const SafeTwoDimensionalArray<MatchInfo>& s2dMatchInfos,
							unsigned long ulFoundIndex, unsigned long ulTotalExpected);

	// PROMISE: Outputs the provided attribute to the testoutput file with the specified label and
	//			increments the provided statistic count.
	void RecordStatistic(const string& strLabel, IAttributePtr ipRelatedAttribute,
						 const string &strSourceVOA, unsigned long& rulCount);

	// PROMISE: Takes 2 unsigned longs and if the second value is > 0, divides the first by the second.
	//			If the denominator is <= 0, 0.0 is returned. Otherwise the quotient * 100.0 is returned.
	double getRatioAsPercentOfTwoLongs( unsigned long ulNumerator, 
										unsigned long ulDenominator);

	// PROMISE: Output the document type statistics in their own test case
	void displayDocumentTypeStats();

	// PROMISE: populates m_mapDocTypeCount with the number of each document type in the
	//			found attributes vector if there was a valid Found VOA file supplied. Otherwise
	//			the doc types will be populated from the AFDoc. 
	void countDocTypes( IIUnknownVectorPtr ipFoundAttributes, const string& strSourceDoc,
						IAFDocumentPtr ipAFDoc, bool bCalculatedFoundValues, 
						IDShieldCounterClass iccCounts);

	// PROMISE: to compare each of the Expected attributes with themselves and return the number of
	//			attributes that overlap as well as the number of pages with expected redactions.
	void countExpectedOverlapsAndPages(IIUnknownVectorPtr ipExpectedAttributes,
		unsigned long& rulOverlaps, unsigned long& rulNumPagesWithRedactions);

	// PROMISE: to compare two spatial strings and return true if they overlap spatially
	bool spatialStringsOverlap(ISpatialStringPtr ipSS1, ISpatialStringPtr ipSS2);

	// PROMISE: to compare an expected and found attribute and set the MatchInfo struct.
	void getMatchInfo(MatchInfo& rMatchInfo, IAttributePtr ipExpected, IAttributePtr ipFound);

	// PROMISE: to compute and return the total area covered by a given attribute
	double getTotalArea(IAttributePtr ipAttribute);

	// PROMISE: to compute and return the total area that the found attribute overlaps
	//			of the expected attribute
	double computeTotalAreaOfOverlap(IAttributePtr ipExpected, IAttributePtr ipFound);

	// PROMISE: to add the specified attribute to the output voa vector with the name
	//			field prefixed with strPrefix
	// REQUIRE: m_ipTestOutputVOAVector is not NULL
	void addAttributeToTestOutputVOA(IAttributePtr ipAttribute, const string& strPrefix,
		const string &strSourceVOA);

	// PROMISE: to look at all of the attributes contained in the found attributes vector
	//			and to count up the total number of attributes named HCData, MCData, LCData,
	//			or Clues
	void countAttributeNames(IIUnknownVectorPtr ipFoundAttributes, unsigned long& rulHCData,
		unsigned long& rulMCData, unsigned long& rulLCData, unsigned long& rulClues);

	// PROMISE: to filter the expected and found attributes by m_setTypesToBeTested
	//			and to return a new IIUnknownVector containing only attributes of the
	//			specified type
	IIUnknownVectorPtr filterAttributesByType(IIUnknownVectorPtr ipAttributeVector);

	// PROMISE: To return all attributes from ipAttributeVector except for DocumentType
	//			attributes.
	IIUnknownVectorPtr filterDocumentTypeAttributes(IIUnknownVectorPtr ipAttributeVector);

	// PROMISE: To update the verification AttributeTester so that it is ready to
	//			be used in the testAttributeCondition.
	void updateVerificationAttributeTester();

	// PROMISE: To update the redaction AttributeTester so that it is ready to
	//			be used in the testAttributeCondition.
	void updateRedactionAttributeTester();

	// PROMISE: Sets m_strOutputFileDirectory based on the provided rootDirectory and current time.
	void getOutputDirectory(string rootDirectory);

	// PROMISE: To convert a set of strings into a comma delimited list.
	string getSetAsDelimitedList(const set<string>& setValues);

	string expandTagsAndFunctions(const string& strInput, const string& strSourceDoc);

	// PROMISE: To Output the document statistics for the given document type
	string displayDocumentStats(const string & strDocumentType, const IDShieldCounterClass& stats);
};

OBJECT_ENTRY_AUTO(__uuidof(IDShieldTester), CIDShieldTester)
