// TestResultLogger.h : Declaration of the CTestResultLogger

#pragma once

#include "resource.h"       // main symbols

#include "TestCaseStats.h"

#include <StopWatch.h>

#include <fstream>
#include <string>
#include <vector>

using namespace std;

// forward declarations
class TestResultLoggerDlg;

// uncomment this line to include logging to databases, which was initially supported
// #define INCLUDE_DB_SUPPORT

/////////////////////////////////////////////////////////////////////////////
// CTestResultLogger
class ATL_NO_VTABLE CTestResultLogger : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTestResultLogger, &CLSID_TestResultLogger>,
	public ISupportErrorInfo,
	public IDispatchImpl<ITestResultLogger, &IID_ITestResultLogger, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CTestResultLogger();
	~CTestResultLogger();

DECLARE_REGISTRY_RESOURCEID(IDR_TESTRESULTLOGGER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CTestResultLogger)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ITestResultLogger)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ITestResultLogger2
public:
// ITestResultLogger
	STDMETHOD(raw_StartTestHarness)(BSTR strHarnessDescription);
	STDMETHOD(raw_StartComponentTest)(BSTR strComponentDescription, BSTR strOutputFileName);
	STDMETHOD(raw_StartTestCase)(BSTR strTestCaseID, BSTR strTestCaseDescription,
		ETestCaseType eTestCaseType);
	STDMETHOD(raw_AddTestCaseNote)(BSTR strTestCaseNote);
	STDMETHOD(raw_AddTestCaseException)(BSTR strTestCaseException, VARIANT_BOOL vbFailTestCase);
	STDMETHOD(raw_EndTestCase)(VARIANT_BOOL bResult);
	STDMETHOD(raw_EndComponentTest)();
	STDMETHOD(raw_EndTestHarness)();
	STDMETHOD(raw_AddTestCaseDetailNote)(BSTR strTitle, BSTR strTestCaseDetailNote);
	STDMETHOD(raw_AddTestCaseMemo)(BSTR strTitle, BSTR strTestCaseMemo);
	STDMETHOD(raw_AddTestCaseFile)(BSTR strFileName);
	STDMETHOD(raw_AddComponentTestException)(BSTR strComponentTestException);
	STDMETHOD(raw_AddTestCaseCompareData) (BSTR strTitle, BSTR strLabel1, BSTR strInput1, BSTR strLabel2, BSTR strInput2);
	STDMETHOD(get_AddEntriesToTestLogger)(VARIANT_BOOL* pvbAddEntries);
	STDMETHOD(put_AddEntriesToTestLogger)(VARIANT_BOOL vbAddEntries);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// this method is called by the thread function, to show the
	// test result logger window as a modeless dialog box.
	void showTestResultLoggerWindow();

private:
	/////////////
	// Variables
	////////////
	// the dialog object and its associated theread
	TestResultLoggerDlg *m_pDlg;

	bool m_bWriteToDB;

	// stop watch to keep track of how long a test case has been running for
	StopWatch m_testCaseStopWatch;

	bool m_bAddEntriesInLogWindow;

	// Keeps track of whether logging should occur for the current test case (may be true even if
	// m_bAddEntriesInLogWindow is false overall for summary stats).
	bool m_bAddEntriesForCurrentTestCase;

	// the database objects and the various tables
#ifdef INCLUDE_DB_SUPPORT
	CDaoDatabase m_db;
	CDaoTableDef m_tblTestHarness;
	CDaoTableDef m_tblComponentTest;
	CDaoTableDef m_tblTestCase;
	CDaoTableDef m_tblTestCaseNote;
	CDaoTableDef m_tblTestCaseException;
#endif

	// database record fields associated with the current
	// harness, component test, and test case
	unsigned long m_ulTestHarnessId, m_ulComponentTestId, m_ulTestCaseId;

	// objects to keep track of stats at the current component test 
	TestCaseStats m_componentLevelStats;

	// output file where test case summary statistics are written to
	ofstream m_outputFile;

	// what is the current active test case type
	ETestCaseType m_eCurrentTestCaseType;

	vector<string> m_vecComponentTestExceptions;

	///////////
	// Methods
	///////////

	// following methods are used to update recordset fields
#ifdef INCLUDE_DB_SUPPORT
	void setStringFieldValue(CDaoRecordset& rs, const char *pszFieldName, const char *pszValue);
	void setNumberFieldValue(CDaoRecordset& rs, const char *pszFieldName, long lValue);
#endif

	// get current time stamp in a formated way ((mm-dd-yyyy)-hhmmss). 
	// For example, (12-28-2004)-143952
	string getTimeStamp();

	// following method returns "<HTML>" if strTagName is "HTML"
	string getStartTag(const string& strTagName);

	// following method returns "</HTML>" if strTagName is "HTML"
	string getEndTag(const string& strTagName);

	// converts text to HTML text syntax such as converting "&" to "&amp;" etc
	void convertToHTMLText(string& rstrText);

	// if strText is all on one line, it's written out as it is.
	// if strText is on multiple lines, each line is written out as a line tag
	// also, rstr is converted to HTML text using convertToHTMLText()
	void writeHTMLText(string strText);

	// method to add a test case and display summary underneath it
	void addSummaryTestCase(const TestCaseStats& stats);

	void startTestCase(const string& strTestCaseID, const string& strTestCaseDescription,
		ETestCaseType eTestCaseType);

	void addTestCaseException(const string& strTestCaseException, bool bFailTestCase);

	void endTestCase(bool bResult);

	// Check license state
	void validateLicense();
};