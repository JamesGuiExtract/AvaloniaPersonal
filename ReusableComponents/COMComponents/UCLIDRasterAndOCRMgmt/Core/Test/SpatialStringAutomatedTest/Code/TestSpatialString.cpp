// TestSpatialString.cpp : Implementation of CTestSpatialString
#include "stdafx.h"
#include "SpatialStringAutomatedTest.h"
#include "TestSpatialString.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <CPPLetter.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CTestSpatialString
//-------------------------------------------------------------------------------------------------
CTestSpatialString::CTestSpatialString()
: m_ipResultLogger(NULL)
{
	try
	{
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI12970", ipMiscUtils != __nullptr );

		m_ipRegExParser = ipMiscUtils->GetNewRegExpParserInstance("TestSpatialString");
		ASSERT_RESOURCE_ALLOCATION("ELI06720", m_ipRegExParser != __nullptr);

		m_ipSpatialString.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI06683", m_ipSpatialString != __nullptr);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06684")
}
//-------------------------------------------------------------------------------------------------
CTestSpatialString::~CTestSpatialString()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16491");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestSpatialString::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITestableComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestSpatialString::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		setTestFileFolder(pParams, asString(strTCLFile));

		if (m_ipResultLogger == __nullptr)
		{
			throw UCLIDException("ELI06682", "Please set ResultLogger before proceeding.");
		}

		runTestCase2();
		runTestCase4();
		runTestCase5();
		runTestCase6();
		runTestCase7();
		runTestCase8();
		runTestCase9();
		runTestCase10();
		runTestCase11();
		runTestCase12();
		runTestCase13();
		runTestCase14();
		runTestCase15();
		runTestCase16();
		runTestCase17();
		runTestCase18();
		runTestCase19();
		runTestCase20();
		runTestCase21();
		runTestCase22();
		runTestCase23();
		runTestCase24();
		runTestCase25();
		runTestCase26();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06681")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestSpatialString::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestSpatialString::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestSpatialString::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase2()
{
	// Test HasSpatialInfo()
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_2"), get_bstr_t("Testing HasSpatialInfo() method"), kAutomatedTestCase); 
	
	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strTestStr("This is a test");
		m_ipSpatialString->CreateNonSpatialString(strTestStr.c_str(), "");
		
		if (m_ipSpatialString->HasSpatialInfo() == VARIANT_TRUE)
		{
			bSuccess = false;
		}

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06696", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase4()
{
	// Test Size
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_4"), get_bstr_t("Testing Size property"), kAutomatedTestCase); 
	
	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{		
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strTestStr("This is a test");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strTestStr.c_str(), "");
			
		long nExpectedSize = strTestStr.size();
		long nObtainedSize = m_ipSpatialString->Size;
		if (nExpectedSize != nObtainedSize)
		{
			bSuccess =  false;
		}
		CString zTemp("");
		zTemp.Format("The expected size is : %d\r\n"
			"The obtained size is : %d", nExpectedSize, nObtainedSize);
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06698", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase5()
{
	// Test At()
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_5"), get_bstr_t("Testing At() method"), kAutomatedTestCase); 
	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{	
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strTestStr("This is a test");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strTestStr.c_str(), "");
		
		// pick an index
		long nAtPos = strTestStr.size() - 1;

		// get the character at this position
		char cExpected = strTestStr.at(nAtPos);
		char cObtained = (char)m_ipSpatialString->GetChar(nAtPos);
		
		if (cExpected != cObtained)
		{
			bSuccess =  false;
		}		
		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected character is : %c\r\n"
			"The obtained character is : %c", 
			strTestStr.c_str(), cExpected, cObtained);
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06699", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase6()
{
	// Test Insert()

	//****************
	// Test - 1
	//****************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_6_1"), get_bstr_t("Testing Insert() method"), kAutomatedTestCase); 
	// description for this test case
	m_ipResultLogger->AddTestCaseMemo(get_bstr_t("Description"),
		get_bstr_t("Inserting a SpatialString into the middle of another SpatialString"));

	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strTestStr("This is a test");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strTestStr.c_str(), "");

		// Create a spatial string
		ISpatialStringPtr ipToBeInserted(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI06701", ipToBeInserted != __nullptr);

		// put the string
		string strToBeInserted("n insertion");
		ipToBeInserted->CreateNonSpatialString(strToBeInserted.c_str(), "");

		// insert the string right after "a"
		long nInsertPos = 9;
		string strOrigin(strTestStr);

		// insert the string into the strTestStr
		strTestStr.insert(nInsertPos, strToBeInserted);

		m_ipSpatialString->Insert(nInsertPos, ipToBeInserted);

		// check the result
		string strTemp = asString(m_ipSpatialString->String);
		if (strTemp != strTestStr)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(),strTestStr.c_str(), strTemp.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06700", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//****************
	// Test - 2
	//****************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_6_2"), get_bstr_t("Testing Insert() method 2"), kAutomatedTestCase); 
	// description for this test case
	m_ipResultLogger->AddTestCaseMemo(get_bstr_t("Description"),
		get_bstr_t("Inserting a SpatialString into the beginning of another SpatialString"));

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strTestStr("This is a test");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strTestStr.c_str(), "");

		// Create a spatial string
		ISpatialStringPtr ipToBeInserted(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI19324", ipToBeInserted != __nullptr);

		// put the string
		string strToBeInserted("Insert something in front of : ");
		ipToBeInserted->CreateNonSpatialString(strToBeInserted.c_str(), "");

		// insert the string right after "a"
		long nInsertPos = 0;
		string strOrigin(strTestStr);

		// insert the string into the strTestStr
		strTestStr.insert(nInsertPos, strToBeInserted);

		m_ipSpatialString->Insert(nInsertPos, ipToBeInserted);

		// check the result
		string strTemp = asString(m_ipSpatialString->String);
		if (strTemp != strTestStr)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(),strTestStr.c_str(), strTemp.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI19323", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase7()
{
	// test Append()
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_7"), get_bstr_t("Testing Append() method"), kAutomatedTestCase); 

	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strTestStr("The string to be appended is ");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strTestStr.c_str(), "");

		// Create a spatial string
		ISpatialStringPtr ipToBeAppended(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI06704", ipToBeAppended != __nullptr);

		// put the string
		string strToBeAppended("ABC");
		ipToBeAppended->CreateNonSpatialString(strToBeAppended.c_str(), "");

		strTestStr.append(strToBeAppended);
		m_ipSpatialString->Append(ipToBeAppended);

		// check the result
		string strTemp = asString(m_ipSpatialString->String);
		if (strTemp != strTestStr)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The expected string is : %s\r\n"
			"The obtained string is : %s", strTestStr.c_str(), strTemp.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06702", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase8()
{
	// test GetSubString()

	//*************
	// Test 1
	//*************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_8_1"), get_bstr_t("Testing GetSubString() method"), kAutomatedTestCase); 

	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strTestStr("Take the Sub String out from the original string.");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strTestStr.c_str(), "");

		// start and end position for the sub string
		long nStartPos = 9, nEndPos = 18;

		ISpatialStringPtr ipSubString = m_ipSpatialString->GetSubString(nStartPos, nEndPos);
		ASSERT_RESOURCE_ALLOCATION("ELI25896", ipSubString != __nullptr);

		// check the result
		string strObtained = asString(ipSubString->String);
		string strExpected = strTestStr.substr(nStartPos, nEndPos-nStartPos+1);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strTestStr.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06709", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//*************
	// Test 2
	//*************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_8_2"), get_bstr_t("Testing GetSubString() method 2"), kAutomatedTestCase); 

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strTestStr("The sub string starts here till the end");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strTestStr.c_str(), "");

		// start position for the sub string
		long nStartPos = 22;

		ISpatialStringPtr ipSubString = m_ipSpatialString->GetSubString(nStartPos, -1);
		ASSERT_RESOURCE_ALLOCATION("ELI25897", ipSubString != __nullptr);

		// check the result
		string strObtained = asString(ipSubString->String);
		string strExpected = strTestStr.substr(nStartPos, strTestStr.size()-nStartPos);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strTestStr.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI19325", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase9()
{
	// test Remove

	//************
	// Test 1
	//************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_9_1"), get_bstr_t("Testing Remove() method"), kAutomatedTestCase); 

	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strTestStr("The mid part will be removed and the remaining part is nothing.");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strTestStr.c_str(), "");

		// start and end position for remove
		long nStartPos = 3, nEndPos = 35;

		m_ipSpatialString->Remove(nStartPos, nEndPos);
		string strOrigin(strTestStr);
		strTestStr.erase(nStartPos, nEndPos-nStartPos+1);

		// check the result
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strTestStr)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strTestStr.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06710", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//************
	// Test 2
	//************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_9_2"), get_bstr_t("Testing Remove() method 2"), kAutomatedTestCase); 

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strTestStr("The rest of the string will be removed.");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strTestStr.c_str(), "");

		// start position for remove
		long nStartPos = 22;

		m_ipSpatialString->Remove(nStartPos, -1);
		string strOrigin(strTestStr);
		strTestStr.erase(nStartPos, strTestStr.size()-nStartPos);

		// check the result
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strTestStr)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strTestStr.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI19326", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase10()
{
	// test Replace()

	//*************************************
	// Test 1 - use no regular expression, case sensitive
	//*************************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_10_1"), 
		get_bstr_t("Testing Replace() method : Use no regular expression. All comparisons are case sensitive"), 
		kAutomatedTestCase); 

	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strTestStr("Please replace this with THIS.");
		string strToBeReplaced("this");
		string strReplacement("THIS");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strTestStr.c_str(), "");

		string strOrigin(strTestStr);

		m_ipSpatialString->Replace(get_bstr_t(strToBeReplaced.c_str()),
			get_bstr_t(strReplacement.c_str()), VARIANT_TRUE, 0, NULL);
		int nStartPos = strTestStr.find(strToBeReplaced);
		strTestStr.replace(nStartPos, strToBeReplaced.size(), strReplacement);

		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strTestStr)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strTestStr.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06711", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//*************************************
	// Test 2 - use no regular expression, case insensitive
	//*************************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_10_2"), 
		get_bstr_t("Testing Replace() method 2: Use no regular expression. All comparisons are case insensitive"), 
		kAutomatedTestCase); 

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strTestStr("Please replace this and This.");
		string strToBeReplaced("this");
		string strReplacement("THIS");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strTestStr.c_str(), "");

		string strOrigin(strTestStr);

		m_ipSpatialString->Replace(get_bstr_t(strToBeReplaced.c_str()),
			get_bstr_t(strReplacement.c_str()), VARIANT_FALSE, 0, NULL);

		string strAllLowercase(strTestStr);
		::makeLowerCase(strAllLowercase);
		unsigned int nStartPos = strAllLowercase.find(strToBeReplaced);
		while (nStartPos != string::npos && nStartPos < strTestStr.size())
		{
			// replace string in the strTestStr
			strTestStr.replace(nStartPos, strToBeReplaced.size(), strReplacement);
			nStartPos += strToBeReplaced.size();
			strAllLowercase = strTestStr;
			::makeLowerCase(strAllLowercase);
			nStartPos = strAllLowercase.find(strToBeReplaced, nStartPos);
		}

		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strTestStr)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strTestStr.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI19327", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//*************************************
	// Test 3 - use regular expression, case sensitive
	//*************************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_10_3"), 
		get_bstr_t("Testing Replace() method 3: Use regular expression. All comparisons are case sensitive"), 
		kAutomatedTestCase); 

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strOrigin("Please replace A-E, G and K.");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->Replace("\\b[A-Z]\\b", "5", VARIANT_TRUE, 0, m_ipRegExParser);

		string strExpected("Please replace 5-5, 5 and 5.");

		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI19328", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//*************************************
	// Test 4 - use regular expression, case sensitive
	//*************************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_10_4"), 
		get_bstr_t("Testing Replace() method 4: Use regular expression. All comparisons are case insensitive"), 
		kAutomatedTestCase); 

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strOrigin("the thethethethethe the");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->Replace("\\B(the)\\B", "$1_00", VARIANT_TRUE, 0, m_ipRegExParser);

		string strExpected("the thethe_00the_00the_00the the");

		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06937", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//*************************************
	// Test 5 - use none regular expression, 
	// replace sub string with an empty string
	//*************************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_10_5"), 
		get_bstr_t("Testing Replace() method 5: Use no regular expression. "
		"All comparisons are case insensitive. Replace sub string with an empty string."), 
		kAutomatedTestCase); 

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strOrigin("Without RegularExpression.");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->Replace("out", "", VARIANT_FALSE, 0, NULL);

		string strExpected("With RegularExpression.");

		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06841", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//*************************************
	// Test 6 - use none regular expression, 
	// replace an empty string with some string
	//*************************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_10_6"), 
		get_bstr_t("Testing Replace() method 6: Use no regular expression. "
		"All comparisons are case insensitive. Replace empty string with some string."), 
		kAutomatedTestCase); 

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strOrigin("Without RegularExpression.");

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->Replace("", "ABC", VARIANT_FALSE, 0, NULL);
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06842", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);
	
	// in this case, it is supposed to throw an exception
	bSuccess = bSuccess && bExceptionCaught;
	m_ipResultLogger->AddTestCaseMemo("Result", 
		get_bstr_t("An exception must be caught for this test case. Please make sure"
		" this exception is throwing from Replace( ) method."));
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase11()
{
	// test ConsolidateChars()

	//*************************
	// Test 1 - Case sensitive
	//*************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_11_1"), 
		get_bstr_t("Testing ConsolidateChars() method : All comparisons are case sensitive"), kAutomatedTestCase); 

	bool bExceptionCaught = false;
	bool bSuccess = true;
	string strOrigin("Consolidate this : AAaaBbbBaaaAaaaaaaaa");
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->ConsolidateChars(get_bstr_t("a"), VARIANT_TRUE);
		m_ipSpatialString->ConsolidateChars(get_bstr_t("b"), VARIANT_TRUE);

		string strExpected("Consolidate this : AAaBbBaAa");
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06712", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//*************************
	// Test 2 - Case insensitive
	//*************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_11_2"), 
		get_bstr_t("Testing ConsolidateChars() method 2 : All comparisons are case insensitive"), kAutomatedTestCase); 

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->ConsolidateChars(get_bstr_t("a"), VARIANT_FALSE);
		m_ipSpatialString->ConsolidateChars(get_bstr_t("b"), VARIANT_FALSE);

		string strExpected("Consolidate this : ABa");
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI19329", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//*************************
	// Test 3 - Consolidate multiple characters
	//*************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_11_3"), 
		get_bstr_t("Testing ConsolidateChars() method : Consolidate multiple characters"), kAutomatedTestCase); 
	strOrigin = "AbcdefAbcdefAbcdefAbcdef";
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->ConsolidateChars(get_bstr_t("abcdef"), VARIANT_FALSE);

		string strExpected("A");
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06901", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//*************************
	// Test 4 - No change
	//*************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_11_4"), 
		get_bstr_t("Testing ConsolidateChars() method : No change"), kAutomatedTestCase); 
	strOrigin = "Consolidate this : 123ab123ab";
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->ConsolidateChars(get_bstr_t("ab"), VARIANT_FALSE);
		m_ipSpatialString->ConsolidateChars(get_bstr_t("123"), VARIANT_TRUE);

		string strExpected("Consolidate this : 1a1a");
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06902", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//*************************
	// Test 5 - No change
	//*************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_11_5"), 
		get_bstr_t("Testing ConsolidateChars() method : No change"), kAutomatedTestCase); 
	strOrigin = "Consolidate this : 1a1a1a1a";
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->ConsolidateChars(get_bstr_t("a"), VARIANT_FALSE);
		m_ipSpatialString->ConsolidateChars(get_bstr_t("1"), VARIANT_TRUE);

		string strExpected(strOrigin);
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06903", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase12()
{
	//*************************
	// Test 1 - Consolidate multiple characters
	//*************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_12_1"), 
		get_bstr_t("Testing ConsolidateString() method : Consolidate multiple characters"), kAutomatedTestCase); 

	bool bExceptionCaught = false;
	bool bSuccess = true;
	string strOrigin = "This test will contain three lines:\r\n\r\n"
				"This is the second line.\r\n\r\n"
				"This is the third line.";
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->ConsolidateString(get_bstr_t("\r\n"), VARIANT_TRUE);

		string strExpected("This test will contain three lines:\r\n"
						 "This is the second line.\r\n"
						 "This is the third line.");
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06907", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//*************************
	// Test 2 - Consolidate multiple characters
	//*************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_12_2"), 
		get_bstr_t("Testing ConsolidateString() method : Consolidate multiple characters"), kAutomatedTestCase); 
	strOrigin = "AbcdefAbcdefAbcdefAbcdef";
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->ConsolidateString(get_bstr_t("abcdef"), VARIANT_FALSE);

		string strExpected("Abcdef");
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06908", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase13()
{
	// test Trim()

	//**************************
	// Test 1 - Trimming leading only
	//**************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_13_1"), 
		get_bstr_t("Testing Trim() method : Trimming leading characters only"), kAutomatedTestCase); 

	bool bExceptionCaught = false;
	bool bSuccess = true;
	string strOrigin("baOOOOOab");
	string strToBeTrimed("ab");
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->Trim(get_bstr_t(strToBeTrimed.c_str()), get_bstr_t(""));

		string strExpected("OOOOOab");
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06713", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//**************************
	// Test 2 - Trimming trailing only
	//**************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_13_2"), 
		get_bstr_t("Testing Trim() method 2: Trimming trailing characters only"), kAutomatedTestCase); 

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->Trim(get_bstr_t(""), get_bstr_t(strToBeTrimed.c_str()));

		string strExpected("baOOOOO");
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI19330", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//**************************
	// Test 3 - Trimming both ends
	//**************************
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_13_3"), 
		get_bstr_t("Testing Trim() method 3: Trimming both ends"), kAutomatedTestCase); 

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->Trim(get_bstr_t(strToBeTrimed.c_str()), get_bstr_t(strToBeTrimed.c_str()));

		string strExpected("OOOOO");
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI19331", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase14()
{
	// test Tokenize()
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_14"), get_bstr_t("Testing Tokenize() method"), kAutomatedTestCase); 

	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		string strOrigin(",token1,token2,token3,");
		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		IIUnknownVectorPtr ipTokens = m_ipSpatialString->Tokenize(get_bstr_t(","));

		vector<string> vecExpectedTokens;
		vecExpectedTokens.push_back("token1");
		vecExpectedTokens.push_back("token2");
		vecExpectedTokens.push_back("token3");
		vecExpectedTokens.push_back("");

		long nObtainedTokensSize = ipTokens->Size();
		if (vecExpectedTokens.size() != nObtainedTokensSize)
		{
			bSuccess = false;
		}
		else
		{
			for (long n=0; n<nObtainedTokensSize; n++)
			{
				ISpatialStringPtr ipToken = ipTokens->At(n);
				ASSERT_RESOURCE_ALLOCATION("ELI25898", ipToken != __nullptr);

				string strObtained = asString(ipToken->String);
				string strExpected = vecExpectedTokens[n];
				if (strExpected != strObtained)
				{
					CString zTemp("");
					zTemp.Format("The original string is : %s\r\n"
						"Expected token number %d is : %s\r\n"
						"Obtained token number %d is : %s", 
						strOrigin.c_str(), 
						n, strExpected.c_str(),
						n, strObtained.c_str());
					m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));
					bSuccess = false;
					break;
				}
			}
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s", strOrigin.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06714", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase15()
{
	// test ToLowerCase()
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_15"), get_bstr_t("Testing ToLowerCase() method"), kAutomatedTestCase); 

	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		string strOrigin("EVERYTHING NEEDS TO BE CONVERTED INTO LOWERCASE.");

		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->ToLowerCase();

		string strExpected("everything needs to be converted into lowercase.");
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06715", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase16()
{
	// test ToUpperCase()
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_16"), get_bstr_t("Testing ToUpperCase() method"), kAutomatedTestCase); 

	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		string strOrigin("everything needs to be converted into uppercase.");

		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->ToUpperCase();

		string strExpected("EVERYTHING NEEDS TO BE CONVERTED INTO UPPERCASE.");
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06716", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase17()
{
	// test ToTitleCase()
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_17"), get_bstr_t("Testing ToTitleCase() method"), kAutomatedTestCase); 

	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		string strOrigin("eVERYtHING nEeDS tO Be conVErtED iNTO tiTLeCaSE.");

		// clear the spatial string first
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->ToTitleCase();

		string strExpected("Everything Needs To Be Converted Into Titlecase.");
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string is : %s\r\n"
			"The expected string is : %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), strExpected.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06717", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase18()
{
	// Character to be used in the test
	char c = 'A';

	ISpatialStringPtr ipFakeSpatial(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI06821", ipFakeSpatial != __nullptr);

	string strUSSFile = m_strTestFilesFolder + "000008.TIF.uss";
	// Load the spatial string with a spatial letter 'A'
	ipFakeSpatial->LoadFrom(get_bstr_t(strUSSFile.c_str()), VARIANT_FALSE);

	// Find the first instance of 'A'
	long nSpot = ipFakeSpatial->FindFirstInstanceOfChar(c, 0);

	ISpatialStringPtr ipWithSpatial( ipFakeSpatial->GetSubString(nSpot, nSpot ) );
	ASSERT_RESOURCE_ALLOCATION("ELI19332", ipWithSpatial != __nullptr);
	
	// the original string
	string strOrigin("I'm  SpatialString.");
	long nInsertPos = 4;
	// clear the spatial string first
	m_ipSpatialString->Clear();

	// put string
	m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");
	m_ipSpatialString->Insert(nInsertPos, ipWithSpatial);

	//**********************
	// Test - 1
	// Testing the Insert method
	//**********************
	// test methods combination
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_18_1"), 
		get_bstr_t("Testing Insert() method"), 
		kAutomatedTestCase); 
	// description for this test case
	m_ipResultLogger->AddTestCaseMemo(get_bstr_t("Description"),
		get_bstr_t("Inserting a SpatialString with IsSpatial turned on into "
		"a SpatialString with IsSpatial off."));

	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		string strExpected("I'm A SpatialString.");
		string strObtained = asString(m_ipSpatialString->String);
		if (strObtained != strExpected)
		{
			bSuccess =  false;
		}

		CString zTemp("");
		zTemp.Format("The original string with IsSpatial turned off is : %s\r\n"
			"The inserted spatial string with IsSpatial turned on is : %c\r\n"
			"The expected string is: %s\r\n"
			"The obtained string is : %s", 
			strOrigin.c_str(), c, strExpected.c_str(), strObtained.c_str());

		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06830", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
	
	bSuccess = bSuccess && !bExceptionCaught;
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));

	//**********************
	// Test - 2
	// Testing HasSpatialInfo() on inserted string
	//**********************
	// test methods combination
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_18_2"), 
		get_bstr_t("Testing HasSpatialInfo() method on inserted string"), 
		kAutomatedTestCase); 
	// description for this test case
	m_ipResultLogger->AddTestCaseMemo(get_bstr_t("Description"),
		get_bstr_t("Inserting a SpatialString with IsSpatial turned on into "
		"a SpatialString with IsSpatial off."));

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		VARIANT_BOOL bIsSpatial = m_ipSpatialString->HasSpatialInfo();
		if (bIsSpatial == VARIANT_FALSE)
		{
			bSuccess = false;
		}

		string strIsSpatial = bSuccess ? "true" : "false";
		CString zTemp("");
		zTemp.Format("The result of checking IsSptial() = %s (supposed to be true)", strIsSpatial.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06831", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//**********************
	// Test - 3
	//**********************
	// test methods combination
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_18_3"), 
		get_bstr_t("Testing combination of GetSubString(), HasSpatialInfo()"), 
		kAutomatedTestCase); 
	// description for this test case
	m_ipResultLogger->AddTestCaseMemo(get_bstr_t("Description"),
		get_bstr_t("Inserting a SpatialString with IsSpatial turned on into "
		"a SpatialString with IsSpatial off."));

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// get a sub string from the string 
		long nStartPos = 6, nEndPos = 12;
		ISpatialStringPtr ipSubString = m_ipSpatialString->GetSubString(nStartPos, nEndPos);
		ASSERT_RESOURCE_ALLOCATION("ELI25899", ipSubString != __nullptr);

		// check if the sub string still retains the IsSpatial from original string
		VARIANT_BOOL bIsSpatial = ipSubString->HasSpatialInfo();
		if (bIsSpatial == VARIANT_TRUE)
		{
			bSuccess = false;
		}

		string strIsSpatial = bSuccess ? "false" : "true";
		string strSubString = asString(ipSubString->String);
		CString zTemp("");
		zTemp.Format("The original string with IsSpatial turned off is : %s\r\n;"
			"The inserted spatial string with IsSpatial turned on is : %c\r\n"
			"The sub string is : %s ==> HasSpatialInfo() = %s (supposed to be false)",
			strOrigin.c_str(), c, strSubString.c_str(), strIsSpatial.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06832", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//**********************
	// Test - 4
	//**********************
	// test methods combination
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_18_4"), 
		get_bstr_t("Testing combination of GetSubString(), HasSpatialInfo()"), 
		kAutomatedTestCase); 
	// description for this test case
	m_ipResultLogger->AddTestCaseMemo(get_bstr_t("Description"),
		get_bstr_t("Inserting a SpatialString with IsSpatial turned on into "
		"a SpatialString with IsSpatial off."));

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// get a sub string from the string 
		long nStartPos = 4, nEndPos = 12;
		ISpatialStringPtr ipSubString = m_ipSpatialString->GetSubString(nStartPos, nEndPos);
		ASSERT_RESOURCE_ALLOCATION("ELI25900", ipSubString != __nullptr);

		// check if the sub string still retains the IsSpatial from original string
		VARIANT_BOOL bIsSpatial = ipSubString->HasSpatialInfo();
		if (bIsSpatial == VARIANT_FALSE)
		{
			bSuccess = false;
		}

		string strIsSpatial = bSuccess ? "true" : "false";
		string strSubString = asString(ipSubString->String);
		CString zTemp("");
		zTemp.Format("The original string with IsSpatial turned off is : %s\r\n"
			"The inserted spatial string with IsSpatial turned on is : %c\r\n"
			"The sub string is : %s ==> HasSpatialInfo() = %s (supposed to be true)",
			strOrigin.c_str(), c, strSubString.c_str(), strIsSpatial.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06833", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//**********************
	// Test - 5
	//**********************
	// test methods combination
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_18_5"), 
		get_bstr_t("Testing combination of Replace(), HasSpatialInfo()"), 
		kAutomatedTestCase); 
	// description for this test case
	m_ipResultLogger->AddTestCaseMemo(get_bstr_t("Description"),
		get_bstr_t("Inserting a SpatialString with IsSpatial turned off into "
		"a SpatialString with IsSpatial turned on."));

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		string strAfterInsertion = asString(m_ipSpatialString->String);

		// replace the "A" with "None"
		m_ipSpatialString->Replace("A", "None", VARIANT_TRUE, 0, NULL);
		string strAfterReplacement = asString(m_ipSpatialString->String);

		// check if the sub string still retains the IsSpatial from original string
		bool bIsSpatial = (m_ipSpatialString->GetMode() == kSpatialMode);
		if (!bIsSpatial)
		{
			bSuccess = false;
		}

		string strIsSpatial = bSuccess ? "true" : "false";
		CString zTemp("");
		zTemp.Format("The original string with IsSpatial turned off is : %s\r\n"
			"The inserted spatial string with IsSpatial turned on is : %c\r\n"
			"The string after insertion is : %s\r\n"
			"The string after replacement is : %s ==> GetMode() == kSpatialMode = %s (supposed to be true)",
			strOrigin.c_str(), c, strAfterInsertion.c_str(), 
			strAfterReplacement.c_str(), strIsSpatial.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06834", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);


	//**********************
	// Test - 6
	//**********************
	// test methods combination
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_18_6"), 
		get_bstr_t("Testing combination of HasSpatialInfo(), Remove()"), 
		kAutomatedTestCase); 
	// description for this test case
	m_ipResultLogger->AddTestCaseMemo(get_bstr_t("Description"),
		get_bstr_t("Inserting a SpatialString with IsSpatial turned on into "
		"a SpatialString with IsSpatial off."));

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		m_ipSpatialString->Clear();

		// put string
		m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "");

		m_ipSpatialString->Insert(nInsertPos, ipWithSpatial);

		string strAfterInsertion = asString(m_ipSpatialString->String);
		long nStartPos = 4, nEndPos = 5;
		m_ipSpatialString->Remove(nStartPos, nEndPos);
		string strAfterRemove = asString(m_ipSpatialString->String);

		// check if the sub string is now hybrid mode
		bool bIsHybrid = m_ipSpatialString->GetMode() == kHybridMode;
		if (!bIsHybrid)
		{
			bSuccess = false;
		}

		string strIsHybrid = bIsHybrid ? "true" : "false";
		CString zTemp("");
		zTemp.Format("The original string with IsSpatial turned off is : %s\r\n"
			"The inserted spatial string with IsSpatial turned on is : %c\r\n"
			"The string after insertion is : %s\r\n"
			"The string after remove is : %s ==> GetMode() == kHybridMode "
			"= %s (supposed to be true)",
			strOrigin.c_str(), c, strAfterInsertion.c_str(), 
			strAfterRemove.c_str(), strIsHybrid.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06835", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase19()
{
	// create a letter object with IsSpatial turned on
	char c = 'A';
	CPPLetter letter(c, c, c, 10, 20, 10, 20, 1, false, false, true, 10, 95, LTR_BOLD);
	vector<CPPLetter> vecLetters;
	vecLetters.push_back(letter);
	
	// Create a spatial page info map for the string
	ISpatialPageInfoPtr ipInfo(CLSID_SpatialPageInfo);
	ASSERT_RESOURCE_ALLOCATION("ELI25901", ipInfo != __nullptr);
	ipInfo->Initialize(2000, 3000, kRotNone, 0.0);

	ILongToObjectMapPtr ipPageInfoMap(CLSID_LongToObjectMap);
	ASSERT_RESOURCE_ALLOCATION("ELI25902", ipPageInfoMap != __nullptr);
	ipPageInfoMap->Set(1, ipInfo);

	// Create a spatial string from the letters
	ISpatialStringPtr ipWithSpatial(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI06837", ipWithSpatial != __nullptr);
	ipWithSpatial->CreateFromLetterArray(vecLetters.size(), (void*)&(vecLetters[0]), "test.tif",
		ipPageInfoMap);
	
	// the original string
	string strOrigin("I'm aaaaaaa.");
	long nInsertPos = 4;
	// clear the spatial string first
	m_ipSpatialString->Clear();

	// put string
	m_ipSpatialString->CreateNonSpatialString(strOrigin.c_str(), "test.tif");
	m_ipSpatialString->Insert(nInsertPos, ipWithSpatial);
	
	//**********************
	// Testing the ConsolidateChars method
	//**********************
	// test methods combination
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_19"), 
		get_bstr_t("Testing combination of Insert(), ConsolidateChars(), HasSpatialInfo()"), 
		kAutomatedTestCase); 
	// description for this test case
	m_ipResultLogger->AddTestCaseMemo(get_bstr_t("Description"),
		get_bstr_t("Inserting a SpatialString with IsSpatial turned on into "
		"a SpatialString with IsSpatial off."));

	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		string strAfterInsertion = asString(m_ipSpatialString->String);
		m_ipSpatialString->ConsolidateChars("A", VARIANT_FALSE);
		string strAfterConsolidate = asString(m_ipSpatialString->String);
		VARIANT_BOOL bIsSpatial = m_ipSpatialString->HasSpatialInfo();

		if (bIsSpatial == VARIANT_FALSE)
		{
			bSuccess = false;
		}

		string strIsSpatial = bSuccess ? "true" : "false";
		CString zTemp("");
		zTemp.Format("The original string with IsSpatial turned off is : %s\r\n"
			"The spatial string to be inserted with IsSpatial turned on is : %c\r\n"
			"The string after insertion is: %s\r\n"
			"The string after consolidating is : %s ==> HasSpatialInfo() = %s (supposed to be true)",
			strOrigin.c_str(), c, strAfterInsertion.c_str(), 
			strAfterConsolidate.c_str(), strIsSpatial.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06839", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase20()
{
	// Test LoadFrom() and SourceDocName()
	m_ipSpatialString->Clear();
	string strUSSFile = m_strTestFilesFolder + "00000112.TIF.uss";
	m_ipSpatialString->LoadFrom(get_bstr_t(strUSSFile.c_str()), VARIANT_FALSE);

	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_20_1"), 
		get_bstr_t("Testing combination of LoadFrom() and SourceDocName()"), 
		kAutomatedTestCase); 
	// description for this test case
	m_ipResultLogger->AddTestCaseMemo(get_bstr_t("Description"),
		get_bstr_t("Load a .uss file and get the source document name by reading the uss file."));
	//******************
	// Test - 1 
	// Load from a .uss file and then read the source doc name
	//******************
	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{	
		string strExpectedSourceFileName = "D:\\TEMP\\MORTGAGE BANK DOCUMENT\\00000112.TIF";
		string strObtainedSourceFileName = asString(m_ipSpatialString->SourceDocName);
		if (_strcmpi(strExpectedSourceFileName.c_str(), strObtainedSourceFileName.c_str()) != 0)
		{
			bSuccess = false;
		}

		CString zTemp("");
		zTemp.Format("The actual source document name is : %s\r\n"
			"The obtained source document name is : %s",
			strExpectedSourceFileName.c_str(), strObtainedSourceFileName.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06840", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	//******************
	// Test - 2
	// Changed the source doc name
	//******************	
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_20_2"), 
		get_bstr_t("Testing SourceDocName()"), kAutomatedTestCase); 
	// description for this test case
	m_ipResultLogger->AddTestCaseMemo(get_bstr_t("Description"),
		get_bstr_t("Load a .uss file and get the source document name by reading the uss file."));

	bExceptionCaught = false;
	bSuccess = true;
	try
	{	
		// change the source name
		string strChangeTo = "D:\\TEMP\\00000112.TIF";
		// reset the source doc name
		m_ipSpatialString->SourceDocName = strChangeTo.c_str();
		string strObtained = asString(m_ipSpatialString->SourceDocName);
		if (_strcmpi(strChangeTo.c_str(), strObtained.c_str()) != 0)
		{
			bSuccess = false;
		}

		CString zTemp("");
		zTemp.Format("The source document name is changed to : %s\r\n"
			"The obtained source document name is : %s",
			strObtained.c_str(), strObtained.c_str());
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI19333", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------

void CTestSpatialString::runTestCase21()
{
	// Set up Test data for Tests for GetWords, GetLines, GetParagraphs, and GetZones

	// clear the spatial string first
	m_ipSpatialString->Clear();
		
	vector<CPPLetter> vecLetters;

	string strTestStr("one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen");

	// Vectors to contain the expected output of the functions
	vector<string> vecExpectedWords;
	vector<string> vecExpectedLines;
	vector<string> vecExpectedParagraphs;
	vector<string> vecExpectedZones;

	// Now the current line c
	vecExpectedLines.push_back(strTestStr);

	// Strings used to build the expected output
	string strCurrWord("");
	string strCurrLine("");
	string strCurrParagraph("");
	string strCurrZone("");
	
	// Word Counter
	int iWordCount = 0;

	unsigned short usWidth = 1000 / (unsigned short)strTestStr.size();
	unsigned short usNextCharPos = 10;

	// make each char into a letter object
	for (unsigned int n = 0; n < strTestStr.size(); n++)
	{
		char c = strTestStr[n];
		CPPLetter letter(c, c, c, 20, 30, usNextCharPos, usNextCharPos + usWidth, 1,
			false, false, true, 10, 95, 0);
		usNextCharPos += usWidth;
		
		strCurrWord += strTestStr[n];
		strCurrLine += strTestStr[n];
		strCurrParagraph += strTestStr[n];
		strCurrZone += strTestStr[n];

		if ((strTestStr[n] == ' ') || (n == strTestStr.size()-1)) 
		{
			// Mark Spaces and end of string as end of word
			// get rid of the space
			if (strTestStr[n] == ' ')
			{
				strCurrWord.erase(strCurrWord.size() - 1);
			}
			vecExpectedWords.push_back(strCurrWord);
			strCurrWord = "";
			iWordCount++;
		
			// Mark every 4 words as end of Paragraph
			if ( (iWordCount % 4 ) == 0 )
			{
				letter.m_bIsEndOfParagraph = true;
				vecExpectedParagraphs.push_back(strCurrParagraph);
				strCurrParagraph = "";
			}
			// Mark every 8 words as end of zone
			if ( (iWordCount % 8 ) == 0 ) 
			{
				letter.m_bIsEndOfZone = true;
				vecExpectedZones.push_back(strCurrZone);
				strCurrZone = "";
			}
		}

		// put the letter into the vector
		vecLetters.push_back(letter);
	}

	// Create a spatial page info map for the string
	ISpatialPageInfoPtr ipInfo(CLSID_SpatialPageInfo);
	ASSERT_RESOURCE_ALLOCATION("ELI25903", ipInfo != __nullptr);
	ipInfo->Initialize(2000, 3000, kRotNone, 0.0);

	ILongToObjectMapPtr ipPageInfoMap(CLSID_LongToObjectMap);
	ASSERT_RESOURCE_ALLOCATION("ELI25904", ipPageInfoMap != __nullptr);
	ipPageInfoMap->Set(1, ipInfo);

	// assign the input letters to the spatial string
	m_ipSpatialString->CreateFromLetterArray(vecLetters.size(), (void*)&(vecLetters[0]),
		"test.tif", ipPageInfoMap);

	/////////////////////////////////////////////////
	// Test GetWords() Test case 21_1
	/////////////////////////////////////////////////

	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_21_1"),
		get_bstr_t("Testing GetWords() method"), kAutomatedTestCase); 

	CString zResultMemo("");
	
	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		IIUnknownVectorPtr ipWords = m_ipSpatialString->GetWords();
		ASSERT_RESOURCE_ALLOCATION("ELI25905", ipWords != __nullptr);
		
		if ( vecExpectedWords.size() != ipWords->Size() ) 
		{
			bSuccess = false;
			CString zTemp("");
			zTemp.Format ("Number of Words expected : %d\r\n"
				"Number of Words obtained : %d\r\n",
				vecExpectedWords.size(),
				ipWords->Size());
			m_ipResultLogger->AddTestCaseMemo("Error Result ", get_bstr_t(zTemp));

		} else
		{
			CString zTemp("");
			zTemp.Format("Processing %d Words \r\n", vecExpectedWords.size() );
			zResultMemo += zTemp;
			for (unsigned int n = 0; n < vecExpectedWords.size() ; n++ )
			{
				ISpatialStringPtr ipWord = ipWords->At(n);
				ASSERT_RESOURCE_ALLOCATION("ELI25906", ipWord != __nullptr);
				string strObtained = asString(ipWord->String);
				string strExpected = vecExpectedWords[n];
				zTemp.Format(" Expected : %s  Obtained : %s \r\n",
					strExpected.c_str(), strObtained.c_str() );
				zResultMemo += zTemp;
				if ( strExpected != strObtained )
				{
					CString zTemp("");
					zTemp.Format("Expected word number %d is : %s\r\n"
						"Obtained word number %d is : %s\r\n",
						n, strExpected.c_str(),
						n, strObtained.c_str());
					m_ipResultLogger->AddTestCaseMemo("Error Result", get_bstr_t(zTemp));
					bSuccess = false;
					break;
				}
			}
		}
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zResultMemo));
		
		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI07180", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	////////////////////////////////////////////
	// Intialize for GetLines Test Case 21_2
	////////////////////////////////////////////

	zResultMemo = "";
	
	bExceptionCaught = false;
	bSuccess = true;

	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_21_2"), 
		get_bstr_t("Testing GetLines() method"), kAutomatedTestCase); 

	try
	{
		IIUnknownVectorPtr ipLines = m_ipSpatialString->GetLines();
		ASSERT_RESOURCE_ALLOCATION("ELI25907", ipLines != __nullptr);
		
		if ( vecExpectedLines.size() != ipLines->Size())
		{
			bSuccess = false;
			CString zTemp("");
			zTemp.Format ("Number of Lines expected : %d\r\n"
				"Number of Lines obtained : %d\r\n",
				vecExpectedLines.size(),
				ipLines->Size());
			m_ipResultLogger->AddTestCaseMemo("Error Result ", get_bstr_t(zTemp));
		} else
		{
			CString zTemp("");
			zTemp.Format("Processing %d lines\r\n", vecExpectedLines.size());
			zResultMemo += zTemp;
			for (unsigned int n = 0; n < vecExpectedLines.size() ; n++ )
			{
				ISpatialStringPtr ipLine = ipLines->At(n);
				ASSERT_RESOURCE_ALLOCATION("ELI25908", ipLine != __nullptr);
				
				string strObtained = asString(ipLine->String);
				string strExpected = vecExpectedLines[n];
				
				CString zTemp("");
				zTemp.Format("  Expected : %s  Obtained : %s \r\n",
					strExpected.c_str(), strObtained.c_str() );
				zResultMemo += zTemp;
				if ( strExpected != strObtained )
				{
					CString zTemp("");
					zTemp.Format("Expected line number %d is : %s\r\n"
						"Obtained line number %d is : %s\r\n",
						n, strExpected.c_str(),
						n, strObtained.c_str());
					m_ipResultLogger->AddTestCaseMemo("Error Result", get_bstr_t(zTemp));
					bSuccess = false;
					break;
				}					
			}
		}
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zResultMemo));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI07181", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	
	/////////////////////////////////////////////
	// Intialize for GetParagraphs Test Case 21_3
	/////////////////////////////////////////////

	zResultMemo = "";
	
	bExceptionCaught = false;
	bSuccess = true;

	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_21_3"), 
		get_bstr_t("Testing GetParagraphs() method"), kAutomatedTestCase); 

	try
	{
		IIUnknownVectorPtr ipParagraphs = m_ipSpatialString->GetParagraphs();
		ASSERT_RESOURCE_ALLOCATION("ELI25909", ipParagraphs != __nullptr);
		
		if ( vecExpectedParagraphs.size() != ipParagraphs->Size())
		{
			bSuccess = false;
			CString zTemp("");
			zTemp.Format ("Number of Paragraphs expected : %d\r\n"
				"Number of Paragraphs obtained : %d\r\n",
				vecExpectedParagraphs.size(),
				ipParagraphs->Size());
			m_ipResultLogger->AddTestCaseMemo("Error Result ", get_bstr_t(zTemp));
		} else
		{
			CString zTemp("");
			zTemp.Format("Processing %d Paragraphs\r\n", vecExpectedParagraphs.size());
			zResultMemo += zTemp;
			for (unsigned int n = 0; n < vecExpectedParagraphs.size(); n++)
			{
				ISpatialStringPtr ipParagraph = ipParagraphs->At(n);
				ASSERT_RESOURCE_ALLOCATION("ELI25910", ipParagraph != __nullptr);
				
				string strObtained = asString(ipParagraph->String);
				string strExpected = vecExpectedParagraphs[n];
				
				CString zTemp("");
				zTemp.Format("  Expected : %s  Obtained : %s \r\n",
					strExpected.c_str(), strObtained.c_str() );
				zResultMemo += zTemp;
				if ( strExpected != strObtained )
				{
					CString zTemp("");
					zTemp.Format("Expected Paragraph number %d is : %s\r\n"
						"Obtained Paragraph number %d is : %s\r\n",
						n, strExpected.c_str(),
						n, strObtained.c_str());
					m_ipResultLogger->AddTestCaseMemo("Error Result", get_bstr_t(zTemp));
					bSuccess = false;
					break;
				}					
			}
		}
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zResultMemo));

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI07182", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------

void CTestSpatialString::runTestCase22()
{
	// This Test case test the functionality of LoadFromMultipleFiles method
	bool bExceptionCaught = false;
	bool bSuccess = false;

	// Test case 22_1 load 3 files into one spatial string and compare against single uss file with
	// all pages 
	m_ipResultLogger->StartTestCase( get_bstr_t("TEST_22_1" ),
		get_bstr_t( "Testing LoadFromMultipleFiles() method with Multiple USS files" ), kAutomatedTestCase ); 

	try
	{
		m_ipSpatialString->Clear();
		string strUSSFile = m_strTestFilesFolder + "080910.TIF.uss";
		m_ipSpatialString->LoadFrom( get_bstr_t( strUSSFile.c_str() ), VARIANT_FALSE );
		
		// Build vector of files to load
		IVariantVectorPtr ipvecFiles( CLSID_VariantVector );
		ASSERT_RESOURCE_ALLOCATION("ELI25911", ipvecFiles != __nullptr);
		string strFileName = m_strTestFilesFolder + "000008.tif.uss";
		ipvecFiles->PushBack( strFileName.c_str() );
		strFileName = m_strTestFilesFolder + "000009.tif.uss";
		ipvecFiles->PushBack( strFileName.c_str() );
		strFileName = m_strTestFilesFolder + "000010.tif.uss";
		ipvecFiles->PushBack( strFileName.c_str() );
		
		// Load the files
		ISpatialStringPtr ipMultipleSpatialString ( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION("ELI25912", ipMultipleSpatialString != __nullptr);
		ipMultipleSpatialString->LoadFromMultipleFiles( ipvecFiles,  m_ipSpatialString->SourceDocName );
		
		// Compare the data loaded
		IComparableObjectPtr ipComplete = m_ipSpatialString;
		ASSERT_RESOURCE_ALLOCATION( "ELI09073", ipComplete != __nullptr );
		
		IComparableObjectPtr ipMulti = ipMultipleSpatialString;
		ASSERT_RESOURCE_ALLOCATION( "ELI09074", ipMulti != __nullptr );
		
		if ( m_ipSpatialString->SourceDocName == ipMultipleSpatialString->SourceDocName &&
			ipComplete->IsEqualTo( ipMulti ) == VARIANT_TRUE )
		{
			// Check the page numbers 
			CPPLetter* pMultiLetters = NULL;
			CPPLetter* pCompleteLetters = NULL;
			long lMultiLetters, lCompleteLetters;
			ipMultipleSpatialString->GetOCRImageLetterArray(&lMultiLetters, (void**)&pMultiLetters);
			m_ipSpatialString->GetOCRImageLetterArray(&lCompleteLetters, (void**)&pCompleteLetters);

			// set page numbers match to true
			bool bPageNumberMatches = true;

			// if the size of letter objects is the same check the pagenumbers of all letter objects
			if ( lMultiLetters == lCompleteLetters )
			{
				for ( long i = 0; i < lMultiLetters; i++ )
				{
					const CPPLetter& multiLetter = pMultiLetters[i];
					const CPPLetter& completeLetter = pCompleteLetters[i];

					// Only compare the page numbers if the characters are spatial
					if( multiLetter.m_bIsSpatial && completeLetter.m_bIsSpatial )
					{
						if ( multiLetter.m_usPageNumber != completeLetter.m_usPageNumber )
						{
							bPageNumberMatches = false;
							string strPageNumbers = "Single File Page # "
								+ asString(completeLetter.m_usPageNumber) + "\r\n"
								+ "Multiple File Page # " + asString (multiLetter.m_usPageNumber);
							m_ipResultLogger->AddTestCaseMemo("Page Numbers don't Match", strPageNumbers.c_str() );
							// Don't need to check any more letter objects
							break;
						}
					}
				}
			}
			else
			{
				string strSizes = "Single File letters size: " + asString (lCompleteLetters) +
					"\r\n" + "Multiple File letters size: " + asString(lMultiLetters);
				m_ipResultLogger->AddTestCaseMemo("Different size letter objects", strSizes.c_str() );
				bPageNumberMatches = false;
			}

			// Successfull test if both spatial strings are equal
			bSuccess = bPageNumberMatches;
		}
		// Post contents of spatial strings
		m_ipResultLogger->AddTestCaseMemo("Single File SourceDoc", m_ipSpatialString->SourceDocName );
		m_ipResultLogger->AddTestCaseMemo("Single File Load Result", m_ipSpatialString->String );
		m_ipResultLogger->AddTestCaseMemo("Multiple File Load SourceDoc", ipMultipleSpatialString->SourceDocName  );
		m_ipResultLogger->AddTestCaseMemo("Multiple File Load Result", ipMultipleSpatialString->String );

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI09075", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);


	// Text case 22_2 Test Loading a single USS file with LoadFromMultipleFiles
	bSuccess = false;
	bExceptionCaught = false;
	
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_22_2"),
		get_bstr_t("Testing LoadFromMultipleFiles() method with Single USS file"), kAutomatedTestCase); 

	try
	{
		m_ipSpatialString->Clear();
		string strUSSFile = m_strTestFilesFolder + "000008.tif.uss";
		m_ipSpatialString->LoadFrom(get_bstr_t(strUSSFile.c_str()), VARIANT_FALSE);
		
		// Build vector of files to load
		IVariantVectorPtr ipvecFiles(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI25913", ipvecFiles != __nullptr);
		ipvecFiles->PushBack( strUSSFile.c_str() );

		// Load the files
		ISpatialStringPtr ipMultipleSpatialString ( CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI25914", ipMultipleSpatialString != __nullptr);
		ipMultipleSpatialString->LoadFromMultipleFiles( ipvecFiles, m_ipSpatialString->SourceDocName );
		
		// Compare the data loaded
		IComparableObjectPtr ipComplete = m_ipSpatialString;
		ASSERT_RESOURCE_ALLOCATION("ELI09076", ipComplete != __nullptr );
		
		IComparableObjectPtr ipMulti = ipMultipleSpatialString;
		ASSERT_RESOURCE_ALLOCATION("ELI09077", ipMulti != __nullptr );

		if ( m_ipSpatialString->SourceDocName == ipMultipleSpatialString->SourceDocName &&
			ipComplete->IsEqualTo( ipMulti ) == VARIANT_TRUE )
		{
			// Check the page numbers 
			CPPLetter* pMultiLetters = NULL;
			CPPLetter* pCompleteLetters = NULL;
			long lMultiLetters, lCompleteLetters;
			ipMultipleSpatialString->GetOCRImageLetterArray(&lMultiLetters, (void**)&pMultiLetters);
			m_ipSpatialString->GetOCRImageLetterArray(&lCompleteLetters, (void**)&pCompleteLetters);

			// set page numbers match to true
			bool bPageNumberMatches = true;

			// if the size of letter objects is the same check the pagenumbers of all letter objects
			if ( lMultiLetters == lCompleteLetters )
			{
				for ( long i = 0; i < lMultiLetters; i++ )
				{
					const CPPLetter& multiLetter = pMultiLetters[i];
					const CPPLetter& completeLetter = pCompleteLetters[i];

					// Only compare the page numbers if the characters are spatial
					if( multiLetter.m_bIsSpatial && completeLetter.m_bIsSpatial )
					{
						if ( multiLetter.m_usPageNumber != completeLetter.m_usPageNumber )
						{
							bPageNumberMatches = false;
							string strPageNumbers = "Single File Page # "
								+ asString(completeLetter.m_usPageNumber) + "\r\n"
								+ "Multiple File Page # " + asString(multiLetter.m_usPageNumber);
							m_ipResultLogger->AddTestCaseMemo("Page Numbers don't Match", strPageNumbers.c_str() );
							break;
						}
					}
				}
			}
			else
			{
				string strSizes = "Single File letters size: " + asString(lCompleteLetters) +
					"\r\n" + "Multiple File letters size: " + asString(lMultiLetters);
				m_ipResultLogger->AddTestCaseMemo("Different size letter objects", strSizes.c_str() );
				bPageNumberMatches = false;
			}

			// Successfull test if both spatial strings are equal
			bSuccess = bPageNumberMatches;
		}
		// Post contents of spatial strings
		m_ipResultLogger->AddTestCaseMemo("Single File SourceDoc", m_ipSpatialString->SourceDocName );
		m_ipResultLogger->AddTestCaseMemo("Single File Load Result", m_ipSpatialString->String );
		m_ipResultLogger->AddTestCaseMemo("Multiple File Load SourceDoc", ipMultipleSpatialString->SourceDocName  );
		m_ipResultLogger->AddTestCaseMemo("Multiple File Load Result", ipMultipleSpatialString->String );

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI09078", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	// Test case 22_3 Load multiple txt files with LoadFromMultipleFiles
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_22_3"),
		get_bstr_t("Testing LoadFromMultipleFiles() method with Multiple txt files"), kAutomatedTestCase); 

	try
	{
		m_ipSpatialString->Clear();
		string strUSSFile = m_strTestFilesFolder + "080910.TIF.txt";
		m_ipSpatialString->LoadFrom(get_bstr_t(strUSSFile.c_str()), VARIANT_FALSE);
		
		// Build vector of files to load
		IVariantVectorPtr ipvecFiles(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI25915", ipvecFiles != __nullptr);
		string strFileName = m_strTestFilesFolder + "000008.tif.txt";
		ipvecFiles->PushBack( strFileName.c_str() );
		strFileName = m_strTestFilesFolder + "000009.tif.txt";
		ipvecFiles->PushBack( strFileName.c_str() );
		strFileName = m_strTestFilesFolder + "000010.tif.txt";
		ipvecFiles->PushBack( strFileName.c_str() );

		// Load the files
		ISpatialStringPtr ipMultipleSpatialString ( CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI25916", ipMultipleSpatialString != __nullptr);
		ipMultipleSpatialString->LoadFromMultipleFiles( ipvecFiles,  m_ipSpatialString->SourceDocName );
		
		// Compare the data loaded
		IComparableObjectPtr ipComplete = m_ipSpatialString;
		ASSERT_RESOURCE_ALLOCATION("ELI09085", ipComplete != __nullptr );
		
		IComparableObjectPtr ipMulti = ipMultipleSpatialString;
		ASSERT_RESOURCE_ALLOCATION("ELI09086", ipMulti != __nullptr );

		if ( m_ipSpatialString->SourceDocName == ipMultipleSpatialString->SourceDocName &&
			ipComplete->IsEqualTo( ipMulti ) == VARIANT_TRUE  && !m_ipSpatialString->HasSpatialInfo() &&
			!ipMultipleSpatialString->HasSpatialInfo() )
		{
			// Successful test if both spatial strings are equal
			bSuccess = true;
		}
		// Post contents of spatial strings
		m_ipResultLogger->AddTestCaseMemo("Single File SourceDoc", m_ipSpatialString->SourceDocName );
		m_ipResultLogger->AddTestCaseMemo("Single File Load Result", m_ipSpatialString->String );
		m_ipResultLogger->AddTestCaseMemo("Multiple File Load SourceDoc", ipMultipleSpatialString->SourceDocName  );
		m_ipResultLogger->AddTestCaseMemo("Multiple File Load Result", ipMultipleSpatialString->String );

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI09087", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	// Test Loading a single txt file with LoadFromMultipleFiles
	bSuccess = false;
	bExceptionCaught = false;
	
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_22_4"),
		get_bstr_t("Testing LoadFromMultipleFiles() method with Single TXT file"), kAutomatedTestCase); 

	try
	{
		m_ipSpatialString->Clear();
		string strUSSFile = m_strTestFilesFolder + "000008.tif.txt";
		m_ipSpatialString->LoadFrom(get_bstr_t(strUSSFile.c_str()), VARIANT_FALSE);
		
		// Build vector of files to load
		IVariantVectorPtr ipvecFiles(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI25917", ipvecFiles != __nullptr);
		ipvecFiles->PushBack( strUSSFile.c_str() );

		// Load the files
		ISpatialStringPtr ipMultipleSpatialString ( CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI25918", ipMultipleSpatialString != __nullptr);
		ipMultipleSpatialString->LoadFromMultipleFiles( ipvecFiles, m_ipSpatialString->SourceDocName );
		
		// Compare the data loaded
		IComparableObjectPtr ipComplete = m_ipSpatialString;
		ASSERT_RESOURCE_ALLOCATION("ELI09088", ipComplete != __nullptr );
		
		IComparableObjectPtr ipMulti = ipMultipleSpatialString;
		ASSERT_RESOURCE_ALLOCATION("ELI09089", ipMulti != __nullptr );

		if ( m_ipSpatialString->SourceDocName == ipMultipleSpatialString->SourceDocName &&
			ipComplete->IsEqualTo( ipMulti ) == VARIANT_TRUE  && (m_ipSpatialString->GetMode() == kSpatialMode) &&
			(ipMultipleSpatialString->GetMode() == kSpatialMode))
		{
			// Successful test if both spatial strings are equal
			bSuccess = true;
		}
		// Post contents of spatial strings
		m_ipResultLogger->AddTestCaseMemo("Single File SourceDoc", m_ipSpatialString->SourceDocName );
		m_ipResultLogger->AddTestCaseMemo("Single File Load Result", m_ipSpatialString->String );
		m_ipResultLogger->AddTestCaseMemo("Multiple File Load SourceDoc", ipMultipleSpatialString->SourceDocName  );
		m_ipResultLogger->AddTestCaseMemo("Multiple File Load Result", ipMultipleSpatialString->String );

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI09090", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//--------------------------------------------------------------------------------------------------
// added as per [p13 #4942] - 03/28/2008 - JDS
void CTestSpatialString::runTestCase23()
{
	// test GetWordDistCount method
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_23"), 
		get_bstr_t("Testing GetWordLengthDist"), kAutomatedTestCase); 
	
	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		// clear the spatial string first
		m_ipSpatialString->Clear();
		
		string strTestStr("This is testing the String, ! This@Is*LongWord. Don't worry\r\n"
			"This is\tonly\t\t\r\na\tsimple test   333-22-4444 333 - 22 - 4444");
		m_ipResultLogger->AddTestCaseMemo("Test string", strTestStr.c_str());

		long lExpectedCount = 21;

		// build map of expected data
		map<long, long> mapExpectedData;
		mapExpectedData[1] = 4;
		mapExpectedData[2] = 3;
		mapExpectedData[3] = 2;
		mapExpectedData[4] = 5;
		mapExpectedData[5] = 2;
		mapExpectedData[6] = 1;
		mapExpectedData[7] = 2;
		mapExpectedData[11] = 1;
		mapExpectedData[17] = 1;

		// put string
		m_ipSpatialString->CreateNonSpatialString(strTestStr.c_str(), "");

		long lWordCount = 0;

		// now get the WordDistCount
		ILongToLongMapPtr ipWordCountMap = m_ipSpatialString->GetWordLengthDist(&lWordCount);
		ASSERT_RESOURCE_ALLOCATION("ELI20624", ipWordCountMap != __nullptr);

		long lMapSize = ipWordCountMap->Size;

		// check to be sure the correct number of words was found
		bSuccess = lWordCount == lExpectedCount;

		// check that the two maps are the same size
		bSuccess = bSuccess && (lMapSize == mapExpectedData.size());

		map<long, long> mapDataFound;
		for (long i=0; i < lMapSize; i++)
		{
			// get each key and value from the LongToLong map
			long lKey(0), lVal(0);
			ipWordCountMap->GetKeyValue(i, &lKey, &lVal);
			
			// add the data to our found map
			mapDataFound[lKey] = lVal;

			// if no failure found yet, continue checking each item
			if (bSuccess)
			{
				map<long, long>::iterator it = mapExpectedData.find(lKey);
				
				if ((it == mapExpectedData.end()) || (it->second != lVal))
				{
					bSuccess = false;
				}
			}
		}

		// build result message
		// add expected first
		string strResult = "Expected:\t";
		for (map<long, long>::iterator it = mapExpectedData.begin(); 
			it != mapExpectedData.end(); it++)
		{
			strResult += "(" + asString(it->first) + ":" + asString(it->second) + ")";
		}
		strResult += "\t" + asString(lExpectedCount) + " words\r\n";

		// now add found
		strResult += "Found:\t\t";
		for (map<long, long>::iterator it = mapDataFound.begin();
			it != mapDataFound.end(); it++)
		{
			strResult += "(" + asString(it->first) + ":" + asString(it->second) + ")";
		}
		strResult += "\t" + asString(lWordCount) + " words";
		
		m_ipResultLogger->AddTestCaseMemo("Result", strResult.c_str());

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI20625", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//--------------------------------------------------------------------------------------------------
// Added based on [LegacyRCAndUtils #4976] - JDS - 05/14/2008
void CTestSpatialString::runTestCase24()
{
	// this test case tests the functionality of GetLines() method on a multi-paged spatial string
	bool bExceptionCaught = false;
	bool bSuccess = false;

	// start the test case
	m_ipResultLogger->StartTestCase( get_bstr_t("TEST_24" ),
		get_bstr_t( "Testing GetLines() on a multi-page spatial string where page 1 ends with a "
			"space character immediately followed by a spatial character on page 2."), 
			kAutomatedTestCase ); 
	try
	{
		// clear the spatial string
		m_ipSpatialString->Clear();

		// get the name of the USS file to load for this test
		string strUSSFile = m_strTestFilesFolder + "MultiPage.uss";
		m_ipSpatialString->LoadFrom( get_bstr_t( strUSSFile.c_str() ), VARIANT_FALSE );

		// get the lines from the spatial string
		IIUnknownVectorPtr ipLines = m_ipSpatialString->GetLines();
		ASSERT_RESOURCE_ALLOCATION("ELI21193", ipLines != __nullptr);

		// get the line count
		long lCount = ipLines->Size();

		// check that there are only 2 lines returned
		bSuccess = lCount == 2;

		// loop through each line and ensure that it is not multipage
		for (long i = 0; i < lCount && bSuccess; i++)
		{
			ISpatialStringPtr ipTemp = ipLines->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI21194", ipTemp != __nullptr);

			bSuccess = !asCppBool(ipTemp->IsMultiPage());
		}

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI21192", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//--------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase25()
{
	// this test case tests the functionality of CreateFromSpatialStrings() method on a multi-paged spatial string
	bool bExceptionCaught = false;
	bool bSuccess = false;

	// For creating the name of the file to load
	string strUSSFile;
	vector<string> vecFilesToCombine;

	// start the test case
	m_ipResultLogger->StartTestCase( get_bstr_t("TEST_25_1" ),
		get_bstr_t( "Testing CreateFromSpatialStrings() by creating a new string from 2 1 page docs "), 
			kAutomatedTestCase ); 
	try
	{
		// get the name of the USS file to load for this test
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.1.uss";
		vecFilesToCombine.push_back(strUSSFile);

		// get the name of the 2nd USS file
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.2.uss";
		vecFilesToCombine.push_back(strUSSFile);
		
		// get the name of the USS file the has the expected results
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.1-2.uss";
		
		bSuccess = testCase25Helper(vecFilesToCombine, strUSSFile);

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI37802", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	
	vecFilesToCombine.clear();

	// start the test case
	m_ipResultLogger->StartTestCase( get_bstr_t("TEST_25_2" ),
		get_bstr_t( "Testing CreateFromSpatialStrings() by creating a new string from 2 2 page docs"), 
			kAutomatedTestCase ); 
	try
	{
		// get the name of the USS file to load for this test
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.1-2.uss";
		vecFilesToCombine.push_back(strUSSFile);

		// get the name of the 2nd USS file
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.3-4.uss";
		vecFilesToCombine.push_back(strUSSFile);
		
		// Get the name of the file with the expected results
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.uss";
		
		bSuccess = testCase25Helper(vecFilesToCombine, strUSSFile);

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI37804", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

		
	vecFilesToCombine.clear();

	// start the test case
	m_ipResultLogger->StartTestCase( get_bstr_t("TEST_25_3" ),
		get_bstr_t( "Testing CreateFromSpatialStrings() by creating a new string from 2 1 page docs and 1 2 page doc"), 
			kAutomatedTestCase ); 
	try
	{
		// get the name of the USS file to load for this test
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.1.uss";
		vecFilesToCombine.push_back(strUSSFile);

		// get the name of the 2nd USS file
		strUSSFile = m_strTestFilesFolder +  "TestImage003.tif.2.uss";
		vecFilesToCombine.push_back(strUSSFile);
		
		// get the name of the 3rd USS file
		strUSSFile = m_strTestFilesFolder +  "TestImage003.tif.3-4.uss";
		vecFilesToCombine.push_back(strUSSFile);
	
		// Get the name of the USS file with the expected results
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.uss";
		
		bSuccess = testCase25Helper(vecFilesToCombine, strUSSFile);

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));		
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI37808", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

			
	vecFilesToCombine.clear();

	// start the test case
	m_ipResultLogger->StartTestCase( get_bstr_t("TEST_25_4" ),
		get_bstr_t( "Testing CreateFromSpatialStrings() pages out of order - testing that an exception is thrown"), 
			kAutomatedTestCase ); 
	try
	{
		// get the name of the USS file to load for this test
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.2.uss";
		vecFilesToCombine.push_back(strUSSFile);

		// get the name of the 2nd USS file
		strUSSFile = m_strTestFilesFolder +  "TestImage003.tif.1.uss";
		vecFilesToCombine.push_back(strUSSFile);
		
		// get the name of the 3rd USS file
		strUSSFile = m_strTestFilesFolder +  "TestImage003.tif.3-4.uss";
		vecFilesToCombine.push_back(strUSSFile);
	
		// Get the name of the USS file with the expected results
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.uss";
		
		// The combine is expected to fail so if it doesn't throw an exception the case fails
		try
		{
			testCase25Helper(vecFilesToCombine, strUSSFile);
			bSuccess = false;
		}
		catch(...)
		{
			bSuccess = true;
		}

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));		
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI37809", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	vecFilesToCombine.clear();

	// start the test case
	m_ipResultLogger->StartTestCase( get_bstr_t("TEST_25_5" ),
		get_bstr_t( "Testing CreateFromSpatialStrings() with 1 page nonspatial and others spatial - testing that an exception is thrown"), 
			kAutomatedTestCase ); 
	try
	{
		// get the name of the USS file to load for this test
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.1.uss.txt";
		vecFilesToCombine.push_back(strUSSFile);

		// get the name of the 2nd USS file
		strUSSFile = m_strTestFilesFolder +  "TestImage003.tif.2.uss";
		vecFilesToCombine.push_back(strUSSFile);
		
		// get the name of the 3rd USS file
		strUSSFile = m_strTestFilesFolder +  "TestImage003.tif.3-4.uss";
		vecFilesToCombine.push_back(strUSSFile);
	
		// Get the name of the USS file with the expected results
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.uss";
		
		// The combine is expected to fail so if it doesn't throw an exception the case fails
		try
		{
			testCase25Helper(vecFilesToCombine, strUSSFile);
			m_ipResultLogger->AddTestCaseNote("Combined string was created, but should not have been because of the nonspatial string");
			bSuccess = false;
		}
		catch(...)
		{
			bSuccess = true;
		}

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));		
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI37810", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	vecFilesToCombine.clear();

	// start the test case
	m_ipResultLogger->StartTestCase( get_bstr_t("TEST_25_6" ),
		get_bstr_t( "Testing CreateFromSpatialStrings() with 1 page hybrid and others spatial - testing that an exception is thrown"), 
			kAutomatedTestCase ); 
	try
	{
		// get the name of the USS file to load for this test
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.1.hybrid.uss";
		vecFilesToCombine.push_back(strUSSFile);

		// get the name of the 2nd USS file
		strUSSFile = m_strTestFilesFolder +  "TestImage003.tif.2.uss";
		vecFilesToCombine.push_back(strUSSFile);
		
		// get the name of the 3rd USS file
		strUSSFile = m_strTestFilesFolder +  "TestImage003.tif.3-4.uss";
		vecFilesToCombine.push_back(strUSSFile);
	
		// Get the name of the USS file with the expected results
		strUSSFile = m_strTestFilesFolder + "TestImage003.tif.uss";
		
		// The combine is expected to fail so if it doesn't throw an exception the case fails
		try
		{
			testCase25Helper(vecFilesToCombine, strUSSFile);
			m_ipResultLogger->AddTestCaseNote("Combined string was created, but should not have been because of the Hybrid string");
			bSuccess = false;
		}
		catch(...)
		{
			bSuccess = true;
		}

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));		
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI38278", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
bool CTestSpatialString::testCase25Helper(vector<string>& rvecFilesToJoin, string& expected)
{
		// Create the vector to hold the strings to combine
	IIUnknownVectorPtr ipStringsToCombine (CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI37799", ipStringsToCombine != __nullptr);

	// Create string used to temp load from uss file
	ISpatialStringPtr ipSS(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI37800", ipSS != __nullptr);

	// load the strings to combine
	for (size_t i = 0; i < rvecFilesToJoin.size(); i++)
	{
		// Create string used to temp load from uss file
		ISpatialStringPtr ipSS(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI38277", ipSS != __nullptr);

		ipSS->LoadFrom(get_bstr_t(rvecFilesToJoin[i]), VARIANT_FALSE);

		ipStringsToCombine->PushBack(ipSS);
	}

	// clear the spatial string
	m_ipSpatialString->Clear();

	ipSS.CreateInstance(CLSID_SpatialString);

	// Join the 2 strings
	ipSS->CreateFromSpatialStrings(ipStringsToCombine, VARIANT_TRUE);

	m_ipSpatialString->LoadFrom( get_bstr_t( expected ), VARIANT_FALSE );

	// Compare the strings
	IComparableObjectPtr ipCompare = m_ipSpatialString;
	ASSERT_RESOURCE_ALLOCATION("ELI37807", ipCompare != __nullptr);

	return compareSpatialStrings(m_ipSpatialString, ipSS);
}
//--------------------------------------------------------------------------------------------------
void CTestSpatialString::runTestCase26()
{
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_26_1"), 
		get_bstr_t("Testing GetPages(vbIncludeBlankPages, strTextForBlankPage) method - VARIANT_TRUE, \"__EMPTY___\""), kAutomatedTestCase); 
	
	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		m_ipSpatialString->Clear();

		// Load test document that has 3 blank pages - 1, 3, 5 with pages 2 and 4 having text
		string strUSSFile = m_strTestFilesFolder + "TestImageWithBlanks.tif.uss";
		m_ipSpatialString->LoadFrom( get_bstr_t( strUSSFile.c_str() ), VARIANT_FALSE );

		IIUnknownVectorPtr ipPages = m_ipSpatialString->GetPages(VARIANT_TRUE, "___EMPTY___");

		string strExpectedPagesFile = m_strTestFilesFolder + "TestImageWithBlanks.tif.vpss";

		bSuccess = testCase26Helper(strExpectedPagesFile, ipPages);

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI39545", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_26_2"), 
		get_bstr_t("Testing GetPages(vbIncludeBlankPages, strTextForBlankPage) method - VARIANT_FALSE, \"\" "), kAutomatedTestCase); 
	
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		IIUnknownVectorPtr ipPages = m_ipSpatialString->GetPages(VARIANT_FALSE, "");

		string strExpectedPagesFile = m_strTestFilesFolder + "TestImageWithBlanks_NoBlanks.tif.vpss";

		bSuccess = testCase26Helper(strExpectedPagesFile, ipPages);

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI39587", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_26_3"), 
		get_bstr_t("Testing GetPages(vbIncludeBlankPages, strTextForBlankPage) method - VARIANT_TRUE, \"\" "), kAutomatedTestCase); 
	
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		IIUnknownVectorPtr ipPages = m_ipSpatialString->GetPages(VARIANT_TRUE, "");

		string strExpectedPagesFile = m_strTestFilesFolder + "TestImageWithBlanks_NoBlanks_EmptyString.tif.vpss";

		bSuccess = testCase26Helper(strExpectedPagesFile, ipPages);

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI39678", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_26_4"), 
		get_bstr_t("Testing GetPages(vbIncludeBlankPages, strTextForBlankPage) method - VARIANT_TRUE, \"__EMPTY___\" with hybrid string"), kAutomatedTestCase); 
	
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// convert the spatial string we are working with to hybrid
		m_ipSpatialString->DowngradeToHybridMode();

		IIUnknownVectorPtr ipPages = m_ipSpatialString->GetPages(VARIANT_TRUE, "___EMPTY___");
		
		string strExpectedPagesFile = m_strTestFilesFolder + "TestImageWithBlanks_Hybrid.tif.vpss";
		
		bSuccess = testCase26Helper(strExpectedPagesFile, ipPages);

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI39590", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_26_5"), 
		get_bstr_t("Testing GetPages(vbIncludeBlankPages, strTextForBlankPage) method - VARIANT_FALSE, \"\" with hybrid string"), kAutomatedTestCase); 
	
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		IIUnknownVectorPtr ipPages = m_ipSpatialString->GetPages(VARIANT_FALSE, "");

		string strExpectedPagesFile = m_strTestFilesFolder + "TestImageWithBlanks_NoBlanks_Hybrid.tif.vpss";
		
		bSuccess = testCase26Helper(strExpectedPagesFile, ipPages);

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI39592", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	string strUSSFile = m_strTestFilesFolder + "image2blankTogether.tif.uss";
	m_ipSpatialString->LoadFrom( get_bstr_t( strUSSFile.c_str() ), VARIANT_FALSE );
		
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_26_6"), 
	get_bstr_t("Testing GetPages(vbIncludeBlankPages, strTextForBlankPage) method with 2 blank pages together - VARIANT_TRUE, \"___EMPTY___\" "), 
		kAutomatedTestCase); 
	
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		IIUnknownVectorPtr ipPages = m_ipSpatialString->GetPages(VARIANT_TRUE, "___EMPTY___");

		string strExpectedPagesFile = m_strTestFilesFolder + "image2blankTogether_WithBlanks.tif.vpss";
		
		bSuccess = testCase26Helper(strExpectedPagesFile, ipPages);

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI39677", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_26_7"), 
		get_bstr_t("Testing GetPages(vbIncludeBlankPages, strTextForBlankPage) method with" 
		"2 blank pages together - VARIANT_TRUE, \"___EMPTY___\" with hybrid string"), kAutomatedTestCase); 


	// convert the spatial string we are working with to hybrid
	m_ipSpatialString->DowngradeToHybridMode();
	
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		IIUnknownVectorPtr ipPages = m_ipSpatialString->GetPages(VARIANT_TRUE, "___EMPTY___");

		string strExpectedPagesFile = m_strTestFilesFolder + "image2blankTogether_WithBlanks_Hybrid.tif.vpss";
		
		bSuccess = testCase26Helper(strExpectedPagesFile, ipPages);

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI39679", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);

	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_26_8"), 
		get_bstr_t("Testing GetPages(vbIncludeBlankPages, strTextForBlankPage) method with" 
		"Empty non spatial string for image with 4 pages - VARIANT_TRUE, \"___EMPTY___\""), kAutomatedTestCase); 

	// Testing a blank spatial string with existing image (needed to get the page count)
	string strSourceDocName = m_strTestFilesFolder + "image2blankTogether.tif";
	m_ipSpatialString->Clear();
	m_ipSpatialString->SourceDocName = strSourceDocName.c_str();

	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		IIUnknownVectorPtr ipPages = m_ipSpatialString->GetPages(VARIANT_TRUE, "___EMPTY___");

		string strExpectedPagesFile = m_strTestFilesFolder + "image2blankTogether_EmptyString.tif.vpss";
		
		bSuccess = testCase26Helper(strExpectedPagesFile, ipPages);

		m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI40373", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//--------------------------------------------------------------------------------------------------
bool CTestSpatialString::testCase26Helper(string strExpectedPagesFile, IIUnknownVectorPtr ipPages)
{
	bool bSuccess = true;

	IIUnknownVectorPtr ipExpectedPages(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI39583", ipExpectedPages != __nullptr);
	
	ipExpectedPages->LoadFrom(strExpectedPagesFile.c_str(), VARIANT_FALSE);

	long nPages = ipPages->Size();

	if (nPages != ipExpectedPages->Size())
	{
		bSuccess = false;
		string strMsg = "Expected " + asString(ipExpectedPages->Size()) + " but returned " + asString(nPages);
		m_ipResultLogger->AddTestCaseNote(strMsg.c_str());
	}
	else
	{
		// Make sure the pages match
		for (long i = 0; i < nPages; i++)
		{
			ISpatialStringPtr ipPage = ipPages->At(i);
			ISpatialStringPtr ipExpectedPage = ipExpectedPages->At(i);

			// Set source doc to be the same so that comparisons succeed
			ipExpectedPage->SourceDocName = ipPage->SourceDocName;

			if (!compareSpatialStrings(ipPage, ipExpectedPage))
			{
				m_ipResultLogger->AddTestCaseNote(("Page " + asString(i) + " did not match expected.").c_str());
				bSuccess = false;
			}
		}
	}
	return bSuccess;
}
//--------------------------------------------------------------------------------------------------
bool CTestSpatialString::compareSpatialStrings(ISpatialStringPtr ipSS1, ISpatialStringPtr ipSS2)
{
	IComparableObjectPtr ipCompare1 = ipSS1;

	// First compare the non spatial info
	if (ipCompare1->IsEqualTo(ipSS2) == VARIANT_FALSE)
	{
		return false;
	}
	if (!ipSS1->HasSpatialInfo() && !ipSS2->HasSpatialInfo())
	{
		return true;
	}

	// Compare the spatial info
	ILongToObjectMapPtr ipPageInfo1 = ipSS1->SpatialPageInfos;
	ILongToObjectMapPtr ipPageInfo2 = ipSS2->SpatialPageInfos;
	IVariantVectorPtr ipKeys1 = ipPageInfo1->GetKeys();
	IVariantVectorPtr ipKeys2 = ipPageInfo2->GetKeys();

	int nNumKeys = ipKeys1->Size;
	
	// Compare the keys
	if (nNumKeys != ipKeys2->Size )
	{
		return false;
	}

	// Compare each of the Page info structures
	int i;

	for (i = 0; i < nNumKeys; i++)
	{
		long nKey = ipKeys1->GetItem(i);
		if (nKey != ipKeys2->GetItem(i).lVal)
		{
			return false;
		}
		ISpatialPageInfoPtr ipSP1 = ipPageInfo1->GetValue(nKey);
		ISpatialPageInfoPtr ipSP2 = ipPageInfo2->GetValue(nKey);
		if (ipSP1->Equal(ipSP2, VARIANT_FALSE) == VARIANT_FALSE)
		{
			return false;
		}
	}
	return true;
}
//--------------------------------------------------------------------------------------------------
void CTestSpatialString::setTestFileFolder(IVariantVectorPtr ipParams, const std::string &strTCLFile)
{
	// if pParams is not empty and the second item is specified,
	// then the second item is the master dat file
	if ((ipParams != __nullptr) && (ipParams->Size > 1))
	{
		std::string strTestFolder = asString(_bstr_t(ipParams->GetItem(1)));

		if(strTestFolder != "")
		{
			m_strTestFilesFolder = getAbsoluteFileName(strTCLFile, strTestFolder );

			if(!isValidFolder(m_strTestFilesFolder))
			{
				// Create and throw exception
				UCLIDException ue("ELI19409", "Required test file folder is invalid or not specified in TCL file!");
				ue.addDebugInfo("Folder", m_strTestFilesFolder);
				throw ue;	
			}
			else
			{
				// Folder was specified and exists, return successfully
				return;
			}
		}
	}

	// Create and throw exception
	UCLIDException ue("ELI12342", "Required test file folder not specified in TCL file!");
	throw ue;	
}
//-------------------------------------------------------------------------------------------------
