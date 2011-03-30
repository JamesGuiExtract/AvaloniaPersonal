// TestResultLogger.cpp : Implementation of CTestResultLogger
#include "stdafx.h"
#include "UCLIDTestingFrameworkCore.h"
#include "TestResultLogger.h"
#include "TestResultLoggerDlg.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#include <Time.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const string gstrTEST_RESULTS = "TestResults";
static const string gstrTEST_HARNESS = "TestHarness";
static const string gstrCOMPONENT_TEST = "ComponentTest";
static const string gstrTEST_CASE = "TestCase";
static const string gstrID = "ID";
static const string gstrTEST_CASE_FILE = "TestCaseFile";
static const string gstrTEST_CASE_EXCEPTION = "TestCaseException";
static const string gstrTEST_CASE_MEMO = "TestCaseMemo";
static const string gstrTEST_CASE_NOTE = "TestCaseNote";
static const string gstrTEST_CASE_DETAIL_NOTE = "TestCaseDetailNote";
static const string gstrDESCRIPTION = "Description";
static const string gstrLINE = "Line";
static const string gstrNOTE = "Note";
static const string gstrTITLE = "Title";
static const string gstrDETAIL = "Detail";
static const string gstrTEST_CASE_RESULT = "TestCaseResult";
static const string gstrEXCEPTION_TEXT = "Text";
static const string gstrEXCEPTION_HEX = "Hex";
static const string gstrSUMMARY_TAG = "Summary";
static const string gstrCOMPONENT_EXCEPTION = "ComponentException";
static const string gstrTEST_CASE_COMPARE = "TestCaseCompareData";

//-------------------------------------------------------------------------------------------------
// CTestResultLogger
//-------------------------------------------------------------------------------------------------
CTestResultLogger::CTestResultLogger()
: m_pDlg(NULL), m_eCurrentTestCaseType(kInvalidTestCaseType), m_bAddEntriesInLogWindow(true),
m_bAddEntriesForCurrentTestCase(true)
#ifdef INCLUDE_DB_SUPPORT
  ,m_tblTestHarness(&m_db), m_tblComponentTest(&m_db), m_tblTestCase(&m_db), 
  m_tblTestCaseNote(&m_db), m_tblTestCaseException(&m_db)
#endif
{
	try
	{
		// open the database
#ifdef INCLUDE_DB_SUPPORT
		const string strVARNAME = "TEST_RESULTS_DB";
		string strDBFile = getEnvironmentVariableValue(strVARNAME);

		if (strDBFile.empty())
		{
			m_bWriteToDB = false;
		}
		else
#endif
		{
			m_bWriteToDB = true;
		}

#ifdef INCLUDE_DB_SUPPORT
		if (m_bWriteToDB)
		{
			m_db.Open(strDBFile.c_str());
			m_tblTestHarness.Open("TestHarness");
			m_tblComponentTest.Open("ComponentTest");
			m_tblTestCase.Open("TestCase");
			m_tblTestCaseNote.Open("TestCaseNote");
			m_tblTestCaseException.Open("TestCaseException");
		}
#endif

		// open up the test results dialog
		showTestResultLoggerWindow();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02979")
}
//-------------------------------------------------------------------------------------------------
CTestResultLogger::~CTestResultLogger()
{
	try
	{
#ifdef INCLUDE_DB_SUPPORT
		// close each of the tables, and the database finally
		if (m_bWriteToDB)
		{
			m_tblTestCaseNote.Close();
			m_tblTestCaseException.Close();
			m_tblTestCase.Close();
			m_tblComponentTest.Close();
			m_tblTestHarness.Close();
			m_db.Close();

			// whenever we use DAO in a dll, we need to call AfxDaoTerm()
			AfxDaoTerm();
		}
		else
#endif
		{
			if (m_outputFile.is_open())
			{
				// close before open with another file
				m_outputFile.close();
			}
		}

		// close the dialog and suspend the thread associated with the dialog
		if (m_pDlg)
		{
			m_pDlg->SendMessage(WM_CLOSE);
			delete m_pDlg;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16543");
}
//-------------------------------------------------------------------------------------------------
void CTestResultLogger::showTestResultLoggerWindow()
{
	try
	{
		AFX_MANAGE_STATE(AfxGetStaticModuleState())
		TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

		m_pDlg = new TestResultLoggerDlg(NULL);
		ASSERT_RESOURCE_ALLOCATION("ELI06357", m_pDlg != __nullptr);
		m_pDlg->Create(TestResultLoggerDlg::IDD);
		m_pDlg->ShowWindow(SW_SHOW);
		string strCaption = "Test Result Logger ";
		strCaption += m_bWriteToDB ? "[DATABASE]" : "[FILE]";
		m_pDlg->SetWindowText(strCaption.c_str());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06356")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITestResultLogger,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ITestResultLogger
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::raw_StartTestHarness(BSTR strHarnessDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		_bstr_t bstrHarnessDescription(strHarnessDescription);
		string stdstrHarnessDescription = bstrHarnessDescription;

#ifdef INCLUDE_DB_SUPPORT
		// create new record
		if (m_bWriteToDB)
		{
			CDaoRecordset rs(&m_db);
			rs.Open(&m_tblTestHarness);
			rs.AddNew();

			// set the field values
			setStringFieldValue(rs, "Description", stdstrHarnessDescription.c_str());
			setNumberFieldValue(rs, "StartTime", (long) time(NULL));
			
			// remember the id of this test harness
			m_ulTestHarnessId = rs.GetFieldValue("HarnessId").lVal;

			// save and close the recordset
			rs.Update();
			rs.Close();

			string strHarnessId = "[";
			strHarnessId += asString(m_ulTestHarnessId);
			strHarnessId += "] ";
			stdstrHarnessDescription.insert(0, strHarnessId);
		}
#endif

		// update the UI
		m_pDlg->startTestHarness(stdstrHarnessDescription);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02281")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::raw_StartComponentTest(BSTR strComponentDescription,
													   BSTR strOutputFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// convert output file to std::string
	std::string strOutputFile = asString(strOutputFileName);

	try
	{
		// Check licensing
		validateLicense();

		_bstr_t bstrComponentDescription(strComponentDescription);
		string stdstrComponentDescription = bstrComponentDescription;

		m_componentLevelStats.reset();

#ifdef INCLUDE_DB_SUPPORT
		// create new record
		if (m_bWriteToDB)
		{
			CDaoRecordset rs(&m_db);
			rs.Open(&m_tblComponentTest);
			rs.AddNew();

			// set the field values
			setStringFieldValue(rs, "Description", stdstrComponentDescription.c_str());
			setNumberFieldValue(rs, "HarnessId", m_ulTestHarnessId);
			setNumberFieldValue(rs, "StartTime", (long) time(NULL));

			// remember the id of this component test
			m_ulComponentTestId = rs.GetFieldValue("ComponentTestId").lVal;

			// save and close the recordset
			rs.Update();
			rs.Close();

			string strComponentId = "[";
			strComponentId += asString(m_ulComponentTestId);
			strComponentId += "] ";
			stdstrComponentDescription.insert(0, strComponentId);
		}
		else
#endif
		{	
			// create the directory for the output file if it does not already exist
			if(getDirectoryFromFullPath(strOutputFile) != "")
			{
				if(!isValidFolder(getDirectoryFromFullPath(strOutputFile)))
				{
					createDirectory(getDirectoryFromFullPath(strOutputFile));
				}
			}

			// close before open the output file
			if (m_outputFile.is_open())
			{
				// close before open with another file
				m_outputFile.close();
			}
			
			m_outputFile.open(strOutputFile.c_str());

			if (!m_outputFile)
			{
				UCLIDException ue("ELI11929", "Unable to open specified file!");
				ue.addDebugInfo("Filename", strOutputFile);
				throw ue;
			}
		
			// record the start of the component test
			// start the component test
			m_outputFile << getStartTag(gstrCOMPONENT_TEST) << endl;
			// write the title tag
			m_outputFile << getStartTag(gstrTITLE) << endl;
			writeHTMLText(stdstrComponentDescription);
			m_outputFile << getEndTag(gstrTITLE) << endl;
		}

		// Append output filename to component description
		string strLabel = stdstrComponentDescription + " - " + strOutputFile;

		m_pDlg->startComponentTest( strLabel );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02280")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::raw_StartTestCase(BSTR strTestCaseID, BSTR strTestCaseDescription, 
											  ETestCaseType eTestCaseType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		startTestCase(asString(strTestCaseID), asString(strTestCaseDescription), eTestCaseType);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19274")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::raw_AddTestCaseNote(BSTR strTestCaseNote)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		_bstr_t bstrTestCaseNote(strTestCaseNote);
		string stdstrTestCaseNote = bstrTestCaseNote;
		
#ifdef INCLUDE_DB_SUPPORT
		if (m_bWriteToDB)
		{
			// create new record
			CDaoRecordset rs(&m_db);
			rs.Open(&m_tblTestCaseNote);
			rs.AddNew();
			
			// set the field values
			setNumberFieldValue(rs, "HarnessId", m_ulTestHarnessId);
			setNumberFieldValue(rs, "ComponentTestId", m_ulComponentTestId);
			setNumberFieldValue(rs, "TestCaseId", m_ulTestCaseId);
			setStringFieldValue(rs, "NoteText", stdstrTestCaseNote.c_str());
			
			// save and close the recordset
			rs.Update();
			rs.Close();
		}
		else
#endif
		{
			m_outputFile << getStartTag(gstrTEST_CASE_NOTE) << endl;
			writeHTMLText(stdstrTestCaseNote);
			m_outputFile << getEndTag(gstrTEST_CASE_NOTE) << endl;
		}

		if (m_bAddEntriesForCurrentTestCase)
		{
			// update the UI
			m_pDlg->addTestCaseNote(stdstrTestCaseNote);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10035")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::raw_AddTestCaseDetailNote(BSTR strTitle, BSTR strTestCaseDetailNote)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		_bstr_t bstrTestCaseDetailNote(strTestCaseDetailNote);
		string stdstrTestCaseDetailNote = bstrTestCaseDetailNote;
		
		_bstr_t bstrTitle(strTitle);
		string stdstrTitle = bstrTitle;
		
#ifdef INCLUDE_DB_SUPPORT
		if (m_bWriteToDB)
		{
			// create new record
			CDaoRecordset rs(&m_db);
			rs.Open(&m_tblTestCaseNote);
			rs.AddNew();
			
			// set the field values
			setNumberFieldValue(rs, "HarnessId", m_ulTestHarnessId);
			setNumberFieldValue(rs, "ComponentTestId", m_ulComponentTestId);
			setNumberFieldValue(rs, "TestCaseId", m_ulTestCaseId);

			string strDetailNote = stdstrTitle + string(" : ") + 
				stdstrTestCaseDetailNote;
			setStringFieldValue(rs, "NoteText", strDetailNote.c_str());
			
			// save and close the recordset
			rs.Update();
			rs.Close();
		}
		else
#endif
		{
			// write the start tag for the detail note
			m_outputFile << getStartTag(gstrTEST_CASE_DETAIL_NOTE) << endl;
			
			// write the title
			m_outputFile << getStartTag(gstrTITLE) << endl;
			writeHTMLText(stdstrTitle);
			m_outputFile << getEndTag(gstrTITLE) << endl;
			
			// write the detail
			m_outputFile << getStartTag(gstrDETAIL) << endl;
			writeHTMLText(stdstrTestCaseDetailNote);
			m_outputFile << getEndTag(gstrDETAIL) << endl;
			
			// write the end tag for the detail note
			m_outputFile << getEndTag(gstrTEST_CASE_DETAIL_NOTE) << endl;
		}

		if (m_bAddEntriesForCurrentTestCase)
		{
			// update the UI
			m_pDlg->addTestCaseDetailNote(stdstrTitle, stdstrTestCaseDetailNote);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05585")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::raw_AddTestCaseMemo(BSTR strTitle, BSTR strTestCaseMemo)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		_bstr_t bstrTestCaseMemo(strTestCaseMemo);
		string stdstrTestCaseMemo = bstrTestCaseMemo;
		
		_bstr_t bstrTitle(strTitle);
		string stdstrTitle = bstrTitle;
		
#ifdef INCLUDE_DB_SUPPORT
		// write to database
		if (m_bWriteToDB)
		{
			// create new record
			CDaoRecordset rs(&m_db);
			rs.Open(&m_tblTestCaseNote);
			rs.AddNew();
			
			// set the field values
			setNumberFieldValue(rs, "HarnessId", m_ulTestHarnessId);
			setNumberFieldValue(rs, "ComponentTestId", m_ulComponentTestId);
			setNumberFieldValue(rs, "TestCaseId", m_ulTestCaseId);

			string strMemo = stdstrTitle + string(" : ") + stdstrTestCaseMemo;
			setStringFieldValue(rs, "NoteText", strMemo.c_str());
			
			// save and close the recordset
			rs.Update();
			rs.Close();
		}
		else
#endif
		{
			// write the start tag for the test case memo
			m_outputFile << getStartTag(gstrTEST_CASE_MEMO) << endl;

			// write the title
			m_outputFile << getStartTag(gstrTITLE) << endl;
			writeHTMLText(stdstrTitle);
			m_outputFile << getEndTag(gstrTITLE) << endl;

			// write the memo details
			m_outputFile << getStartTag(gstrDETAIL) << endl;
			writeHTMLText(stdstrTestCaseMemo);
			m_outputFile << getEndTag(gstrDETAIL) << endl;

			// write the end tag for the test case memo
			m_outputFile << getEndTag(gstrTEST_CASE_MEMO) << endl;
		}

		if (m_bAddEntriesForCurrentTestCase)
		{
			// Add Compare Data to the tree and the dialog.
			m_pDlg->addTestCaseMemo(stdstrTitle, stdstrTestCaseMemo);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05752")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::raw_AddTestCaseFile(BSTR strFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		_bstr_t bstrTestCaseFile(strFileName);
		string stdstrTestCaseFile = bstrTestCaseFile;
		
#ifdef INCLUDE_DB_SUPPORT
		// write to database
		if (m_bWriteToDB)
		{
			// create new record
			CDaoRecordset rs(&m_db);
			rs.Open(&m_tblTestCaseNote);
			rs.AddNew();
			
			// set the field values
			setNumberFieldValue(rs, "HarnessId", m_ulTestHarnessId);
			setNumberFieldValue(rs, "ComponentTestId", m_ulComponentTestId);
			setNumberFieldValue(rs, "TestCaseId", m_ulTestCaseId);

			string strMemo = string("File : ") + stdstrTestCaseFile;
			setStringFieldValue(rs, "NoteText", strMemo.c_str());
			
			// save and close the recordset
			rs.Update();
			rs.Close();
		}
		else
#endif
		{
			m_outputFile << getStartTag(gstrTEST_CASE_FILE) << endl;
			writeHTMLText(stdstrTestCaseFile);
			m_outputFile << getEndTag(gstrTEST_CASE_FILE) << endl;
		}

		if (m_bAddEntriesForCurrentTestCase)
		{
			// update the UI
			m_pDlg->addTestCaseFile(stdstrTestCaseFile);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06352")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::raw_AddTestCaseException(BSTR strTestCaseException,
														 VARIANT_BOOL vbFailTestCase)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		addTestCaseException(asString(strTestCaseException), asCppBool(vbFailTestCase));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02286")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::raw_AddComponentTestException(BSTR strComponentTestException)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		_bstr_t bstrComponentTestException(strComponentTestException);
		string stdstrComponentTestException = bstrComponentTestException;

		m_vecComponentTestExceptions.push_back(stdstrComponentTestException);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10569")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::raw_EndTestCase(VARIANT_BOOL bResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		endTestCase(asCppBool(bResult));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02282")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::raw_EndComponentTest()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

#ifdef INCLUDE_DB_SUPPORT
		if (m_bWriteToDB)
		{
			// find the current component test record
			CDaoQueryDef query(&m_db);
			char pszTemp[128];
			sprintf_s(pszTemp, sizeof(pszTemp), "SELECT * FROM ComponentTest WHERE HarnessId=%d AND ComponentTestId=%d", m_ulTestHarnessId, m_ulComponentTestId);
			query.Create("", pszTemp);
			CDaoRecordset rs(&m_db);
			rs.Open(&query);
			rs.Edit();
			
			// update the end-time stamp
			setNumberFieldValue(rs, "EndTime", (long) time(NULL));

			// save and close the recordset
			rs.Update();
			rs.Close();
		}
		else
#endif
		{
			// before end the component test, add component level summary
			addSummaryTestCase(m_componentLevelStats);

			// end the component test
			m_outputFile << getEndTag(gstrCOMPONENT_TEST) << endl;
			// close current output file
			m_outputFile.close();
		}

		if(m_vecComponentTestExceptions.size() > 0)
		{
			// Create a testcase that will display end of component exceptions
			unsigned long lSize = m_vecComponentTestExceptions.size();
			startTestCase(asString(lSize), "Component Test Exceptions", kSummaryTestCase);

			for(unsigned long i = 0; i < lSize; i++)
			{
				addTestCaseException(m_vecComponentTestExceptions[i], false);
			}
			endTestCase(false);

			m_vecComponentTestExceptions.clear();
		}
		
		// update the UI
		m_pDlg->endComponentTest();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02283")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::raw_EndTestHarness()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

#ifdef INCLUDE_DB_SUPPORT
		if (m_bWriteToDB)
		{
			// find the current test harness record
			CDaoQueryDef query(&m_db);
			char pszTemp[128];
			sprintf_s(pszTemp, sizeof(pszTemp), "SELECT * FROM TestHarness WHERE HarnessId=%d", m_ulTestHarnessId);
			query.Create("", pszTemp);
			CDaoRecordset rs(&m_db);
			rs.Open(&query);
			rs.Edit();
			
			// update the end-time stamp
			setNumberFieldValue(rs, "EndTime", (long) time(NULL));

			// save and close the recordset
			rs.Update();
			rs.Close();
		}
#endif

		// update the UI
		m_pDlg->endTestHarness();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02284")
}
//-------------------------------------------------------------------------------------------------
HRESULT CTestResultLogger::raw_AddTestCaseCompareData (BSTR strTitle, BSTR strLabel1, BSTR strInput1, 
													BSTR strLabel2, BSTR strInput2)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// Check licensing
		validateLicense();

		_bstr_t bstrTitle(strTitle);
		string stdstrTitle = bstrTitle;

		_bstr_t bstrLabel1(strLabel1);
		string stdstrLabel1 = bstrLabel1;
		
		_bstr_t bstrInput1(strInput1);
		string stdstrInput1 = bstrInput1;

		_bstr_t bstrLabel2(strLabel2);
		string stdstrLabel2 = bstrLabel2;

		_bstr_t bstrInput2(strInput2);
		string stdstrInput2 = bstrInput2;
		
#ifdef INCLUDE_DB_SUPPORT
		// write to database
		if (m_bWriteToDB)
		{
			// create new record
			CDaoRecordset rs(&m_db);
			rs.Open(&m_tblTestCaseNote);
			rs.AddNew();
			
			// set the field values
			setNumberFieldValue(rs, "HarnessId", m_ulTestHarnessId);
			setNumberFieldValue(rs, "ComponentTestId", m_ulComponentTestId);
			setNumberFieldValue(rs, "TestCaseId", m_ulTestCaseId);

			string strCompareData = stdstrLabel1 + string(" : ") + stdstrInput1
						+ "\n\n" + stdstrLabel2 + " : " + stdstrInput2;
			setStringFieldValue(rs, "NoteText", strCompareData.c_str());
			
			// save and close the recordset
			rs.Update();
			rs.Close();
		}
		else
#endif
		{
			// write the start tag for the test case memo
			m_outputFile << getStartTag(gstrTEST_CASE_COMPARE) << endl;

			// write the label for the Title
			m_outputFile << getStartTag(gstrTITLE) << endl;
			writeHTMLText(stdstrTitle);
			m_outputFile << getEndTag(gstrTITLE) << endl;

			// write the label for the first entry
			m_outputFile << getStartTag(gstrTITLE) << endl;
			writeHTMLText(stdstrLabel1);
			m_outputFile << getEndTag(gstrTITLE) << endl;

			// write the first input (expected string)
			m_outputFile << getStartTag(gstrDETAIL) << endl;
			writeHTMLText(stdstrInput1);
			m_outputFile << getEndTag(gstrDETAIL) << endl;

			// write the label for the second entry
			m_outputFile << getStartTag(gstrTITLE) << endl;
			writeHTMLText(stdstrLabel2);
			m_outputFile << getEndTag(gstrTITLE) << endl;

			// write the second input (found string)
			m_outputFile << getStartTag(gstrDETAIL) << endl;
			writeHTMLText(stdstrInput2);
			m_outputFile << getEndTag(gstrDETAIL) << endl;

			// write the end tag for the test case memo
			m_outputFile << getEndTag(gstrTEST_CASE_COMPARE) << endl;
		}

		// Only update the UI if adding entries to the log window
		if (m_bAddEntriesForCurrentTestCase)
		{
			// update the UI
			m_pDlg->addTestCaseCompareData(stdstrTitle, stdstrLabel1, stdstrInput1,
										   stdstrLabel2, stdstrInput2);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19314")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::get_AddEntriesToTestLogger(VARIANT_BOOL *pvbAddEntries)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		ASSERT_ARGUMENT("ELI26635", pvbAddEntries != __nullptr);
		validateLicense();

		*pvbAddEntries = asVariantBool(m_bAddEntriesInLogWindow);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26636");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::put_AddEntriesToTestLogger(VARIANT_BOOL vbAddEntries)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		validateLicense();

		m_bAddEntriesInLogWindow = asCppBool(vbAddEntries);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26637");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestResultLogger::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Failure of validateLicense is indicated by an exception being thrown
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
#ifdef INCLUDE_DB_SUPPORT
void CTestResultLogger::setStringFieldValue(CDaoRecordset& rs, const char *pszFieldName, 
											const char *pszValue)
{
	rs.SetFieldValue(pszFieldName, pszValue);
}
//-------------------------------------------------------------------------------------------------
void CTestResultLogger::setNumberFieldValue(CDaoRecordset& rs, const char *pszFieldName, 
											long lValue)
{
	VARIANT vValue;
	vValue.lVal = lValue;
	vValue.vt = VT_I4;
	rs.SetFieldValue(pszFieldName, vValue);
}
#endif
//-------------------------------------------------------------------------------------------------
string CTestResultLogger::getTimeStamp()
{
	CString zTimeStamp;
	__time64_t curTime;
	time( &curTime );
	tm _tm;
	if (_localtime64_s( &_tm, &curTime ) != 0)
	{
		throw UCLIDException("ELI12932", "Unable to retrieve local time!");
	}
	
	// create a time stamp to be added to the file's name
	zTimeStamp.Format("(%2d-%2d-%4d)-%2d%02d%2d", 
		_tm.tm_mon + 1, 
		_tm.tm_mday, 
		_tm.tm_year + 1900, 
		_tm.tm_hour, 
		_tm.tm_min, 
		_tm.tm_sec);

	// replace any spaces with 0s
	zTimeStamp.Replace(' ', '0');

	return (LPCTSTR)zTimeStamp;
}
//-------------------------------------------------------------------------------------------------
string CTestResultLogger::getStartTag(const string& strTagName)
{
	string strResult = "<";
	strResult += strTagName;
	strResult += ">";

	return strResult;
}
//-------------------------------------------------------------------------------------------------
string CTestResultLogger::getEndTag(const string& strTagName)
{
	string strResult = "</";
	strResult += strTagName;
	strResult += ">";

	return strResult;
}
//-------------------------------------------------------------------------------------------------
void CTestResultLogger::convertToHTMLText(string& rstrText)
{
	// replace the ampersand, lesser-than, and greater-than symbols with
	// the XML equivalent
	replaceVariable(rstrText, "&", "&amp;");
	replaceVariable(rstrText, "<", "&lt;");
	replaceVariable(rstrText, ">", "&gt;");

	// XML file readers (such as IE, Altova's XML Spy, etc)
	// don't handle chars > 128 value...so convert those into a &#ddd format
	string strResult;
	long nLength = rstrText.length();
	for (int i = 0; i < nLength; i++)
	{
		unsigned char ucCode = rstrText[i];
		if (ucCode >= 128)
		{
			strResult += "&#";
			strResult += asString((unsigned long) ucCode);
			strResult += ";";
		}
		else
		{
			strResult += rstrText[i];
		}
	}

	rstrText = strResult;
}
//-------------------------------------------------------------------------------------------------
void CTestResultLogger::writeHTMLText(std::string strText)
{
	if (strText.find('\n') == string::npos)
	{
		// the text is all on one line - write it out as it is.
		convertToHTMLText(strText);
		m_outputFile << strText << endl;
	}
	else
	{
		// the text is on multiple lines. write out each line as a line-tag
		StringTokenizer st('\n');
		vector<string> vecTokens;
		st.parse(strText, vecTokens);
		vector<string>::const_iterator iter;
		for (iter = vecTokens.begin(); iter != vecTokens.end(); iter++)
		{
			string strToken = *iter;

			m_outputFile << getStartTag(gstrLINE) << endl;
			convertToHTMLText(strToken);
			replaceVariable(strToken, "\r", "");
			replaceVariable(strToken, "\n", "");
			m_outputFile << strToken << endl;
			m_outputFile << getEndTag(gstrLINE) << endl;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CTestResultLogger::addSummaryTestCase(const TestCaseStats& stats)
{
	// start the summary test case
	// get current date/time
	__time64_t currTime;
	time( &currTime );
	tm _tm;
	if (_localtime64_s( &_tm, &currTime ) != 0)
	{
		throw UCLIDException("ELI12933", "Unable to retrieve local time!");
	}
	
	char pszTime[256];
	if (asctime_s(pszTime, sizeof(pszTime), &_tm) != 0)
	{
		throw UCLIDException("ELI12934", "Unable to determine ASCII time!");
	}
	string strTimeNote = pszTime;
	strTimeNote = ::trim(strTimeNote, "", "\r\n");

	m_outputFile << getStartTag(gstrSUMMARY_TAG) << endl;
	m_outputFile << getStartTag(gstrID) << endl;
	writeHTMLText(strTimeNote);
	m_outputFile << getEndTag(gstrID) << endl;

	m_pDlg->startTestCase(strTimeNote, "Summary of Statistics:", kSummaryTestCase);

	// write the total test cases
	CString zTemp;
	zTemp.Format("Total test cases executed: %ld", stats.m_ulTotalTestCases);
	m_outputFile << getStartTag(gstrLINE) << endl;
	writeHTMLText((LPCTSTR)zTemp);
	m_outputFile << getEndTag(gstrLINE) << endl;

	m_pDlg->addTestCaseNote((LPCTSTR)zTemp);

	if (stats.m_ulTotalTestCases != 0)
	{
		// write the number of passed test cases
		zTemp.Format("Total test cases passed: %ld (%0.2f%%)", 
			stats.m_ulPassedTestCases,
			stats.m_ulPassedTestCases * 100 / (double) stats.m_ulTotalTestCases);

		m_outputFile << getStartTag(gstrLINE) << endl;
		writeHTMLText((LPCTSTR)zTemp);
		m_outputFile << getEndTag(gstrLINE) << endl;
		
		m_pDlg->addTestCaseNote((LPCTSTR)zTemp);

		// write the number of failed test cases
		unsigned long ulTotalFailedCases = stats.m_ulTotalTestCases - 
			stats.m_ulPassedTestCases;
		zTemp.Format("Total test cases failed: %ld (%0.2f%%)", 
			ulTotalFailedCases, 
			ulTotalFailedCases * 100 / (double) stats.m_ulTotalTestCases);

		m_outputFile << getStartTag(gstrLINE) << endl;
		writeHTMLText((LPCTSTR)zTemp);
		m_outputFile << getEndTag(gstrLINE) << endl;

		m_pDlg->addTestCaseNote((LPCTSTR)zTemp);
	}

	// write total elapsed time
	zTemp.Format("Total elapsed time: %ld second(s)", 
		stats.getTotalElapsedSeconds());
	m_outputFile << getStartTag(gstrLINE) << endl;
	writeHTMLText((LPCTSTR)zTemp);
	m_outputFile << getEndTag(gstrLINE) << endl;

	m_pDlg->addTestCaseNote((LPCTSTR)zTemp);

	m_outputFile << getEndTag(gstrSUMMARY_TAG) << endl;

	// end the summary test case
	m_pDlg->endTestCase(stats.m_ulPassedTestCases == stats.m_ulTotalTestCases);
}
//-------------------------------------------------------------------------------------------------
void CTestResultLogger::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI07041", "Test Result Logger" );
}
//-------------------------------------------------------------------------------------------------
void CTestResultLogger::startTestCase(const string& strTestCaseID,
									  const string& strTestCaseDescription,
									  ETestCaseType eTestCaseType)
{
	try
	{
		// Set current test type
		m_eCurrentTestCaseType = eTestCaseType;

		// start the stop watch
		m_testCaseStopWatch.reset();
		m_testCaseStopWatch.start();

#ifdef INCLUDE_DB_SUPPORT
		if (m_bWriteToDB)
		{
			// create new record
			CDaoRecordset rs(&m_db);
			rs.Open(&m_tblTestCase);
			rs.AddNew();

			// set the field values
			setStringFieldValue(rs, "Description", stdstrTestCaseDescription.c_str());
			setNumberFieldValue(rs, "HarnessId", m_ulTestHarnessId);
			setNumberFieldValue(rs, "ComponentTestId", m_ulComponentTestId);
			setNumberFieldValue(rs, "Type", eTestCaseType);
			setStringFieldValue(rs, "HRTestCaseId", stdstrTestCaseID.c_str());
			setNumberFieldValue(rs, "StartTime", (long) time(NULL));
			
			// remember the id of this test case
			m_ulTestCaseId = rs.GetFieldValue("TestCaseId").lVal;
			
			// save and close the recordset
			rs.Update();
			rs.Close();

			string strTestCaseId = "[";
			strTestCaseId += asString(m_ulTestCaseId);
			strTestCaseId += "] ";
			stdstrTestCaseDescription.insert(0, strTestCaseId);
		}
		else
#endif
		{
			// start the test case
			m_outputFile << getStartTag(gstrTEST_CASE) << endl;
			
			// write the ID tag
			m_outputFile << getStartTag(gstrID) << endl;
			writeHTMLText(strTestCaseID);
			m_outputFile << getEndTag(gstrID) << endl;
			
			// write the description
			m_outputFile << getStartTag(gstrDESCRIPTION) << endl;
			writeHTMLText(strTestCaseDescription);
			m_outputFile << getEndTag(gstrDESCRIPTION) << endl;
		}

		// [FlexIDSCore:4003, 4004]
		// Entries should be added to the log window if either m_bAddEntriesInLogWindow is true or
		// if the current test case is of type "other" (used for summary statistics).
		if (m_bAddEntriesInLogWindow || eTestCaseType == kSummaryTestCase)
		{
			m_bAddEntriesForCurrentTestCase = true;

			// update the UI
			m_pDlg->startTestCase(strTestCaseID, strTestCaseDescription, eTestCaseType);
		}
		else
		{
			m_bAddEntriesForCurrentTestCase = false;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26645");
}
//-------------------------------------------------------------------------------------------------
void CTestResultLogger::addTestCaseException(const string& strTestCaseException, bool bFailTestCase)
{
	try
	{
#ifdef INCLUDE_DB_SUPPORT
		// create new record
		if (m_bWriteToDB)
		{
			CDaoRecordset rs(&m_db);
			rs.Open(&m_tblTestCaseException);
			rs.AddNew();
			
			// set the field values
			setNumberFieldValue(rs, "HarnessId", m_ulTestHarnessId);
			setNumberFieldValue(rs, "ComponentTestId", m_ulComponentTestId);
			setNumberFieldValue(rs, "TestCaseId", m_ulTestCaseId);
			setStringFieldValue(rs, "ExceptionText", stdstrTestCaseException.c_str());
			
			// save and close the recordset
			rs.Update();
			rs.Close();
		}
		else
#endif
		{
			// write the start tag for the exception
			m_outputFile << getStartTag(gstrTEST_CASE_EXCEPTION) << endl;

			// write the exception out in a human readable format
			UCLIDException ue;
			ue.createFromString("ELI08491", strTestCaseException);
			string strEx;
			ue.asString(strEx);
			convertCppStringToNormalString(strEx);
			m_outputFile << getStartTag(gstrEXCEPTION_TEXT) << endl;
			writeHTMLText(strEx);
			m_outputFile << getEndTag(gstrEXCEPTION_TEXT) << endl;

			// also write out the hex chars for easy reconstruction of the exception
			m_outputFile << getStartTag(gstrEXCEPTION_HEX) << endl;
			writeHTMLText(strTestCaseException);
			m_outputFile << getEndTag(gstrEXCEPTION_HEX) << endl;
			
			// write the end tag for the exception
			m_outputFile << getEndTag(gstrTEST_CASE_EXCEPTION) << endl;
		}

		if (m_bAddEntriesForCurrentTestCase)
		{
			// update the UI
			m_pDlg->addTestCaseException(strTestCaseException);
		}

		// Check for fail test case
		if (bFailTestCase)
		{
			endTestCase(false);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26644");
}
//-------------------------------------------------------------------------------------------------
void CTestResultLogger::endTestCase(bool bResult)
{
	try
	{
#ifdef INCLUDE_DB_SUPPORT
		// find the current test case record
		if (m_bWriteToDB)
		{
			CDaoQueryDef query(&m_db);
			char pszTemp[128];
			sprintf_s(pszTemp, sizeof(pszTemp), "SELECT * FROM TestCase WHERE HarnessId=%d AND ComponentTestId=%d AND TestCaseId=%d", m_ulTestHarnessId, m_ulComponentTestId, m_ulTestCaseId);
			query.Create("", pszTemp);
			CDaoRecordset rs(&m_db);
			rs.Open(&query);
			rs.Edit();
			
			// update the end-time stamp
			setNumberFieldValue(rs, "EndTime", (long) time(NULL));

			// update the result field, which is of boolean type
			VARIANT vPassed;
			vPassed.bVal = (BYTE) bResult;
			vPassed.vt = VT_UI1;
			rs.SetFieldValue("Passed", vPassed);

			// save and close the recordset
			rs.Update();
			rs.Close();
		}
		else
#endif
		{
			// write the test case result
			m_outputFile << getStartTag(gstrTEST_CASE_RESULT) << endl;
			if (bResult)
			{
				m_outputFile << "SUCCESS" << endl;
			}
			else
			{
				m_outputFile << "FAILURE" << endl;
			}
			m_outputFile << getEndTag(gstrTEST_CASE_RESULT) << endl;

			// end the test case
			m_outputFile << getEndTag(gstrTEST_CASE) << endl;
		}

		// only record stats if current test case status is either kAutomatedTestCase
		// or kInteractiveTestCase
		if (m_eCurrentTestCaseType == kInteractiveTestCase 
			|| m_eCurrentTestCaseType == kAutomatedTestCase)
		{
			m_componentLevelStats.recordTestCaseStatus(bResult);
		}

		// stop the stop watch
		m_testCaseStopWatch.stop();

		if (m_bAddEntriesForCurrentTestCase)
		{
			// add test case note with the time it took to run this test case
			string strText = "Test case processing time: ";
			strText += asString(m_testCaseStopWatch.getElapsedTime());
			strText += " seconds.";
			m_pDlg->addTestCaseNote(strText);

			// update the UI
			m_pDlg->endTestCase(bResult);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26646");
}
//-------------------------------------------------------------------------------------------------