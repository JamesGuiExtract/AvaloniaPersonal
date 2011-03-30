// TestSafeNetUtils.cpp : Implementation of CTestSafeNetUtils
#include "stdafx.h"
#include "SafeNetUtilsTest.h"
#include "TestSafeNetUtils.h"
#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>

//--------------------------------------------------------------------------------------------------
// CTestSafeNetUtils
//--------------------------------------------------------------------------------------------------
CTestSafeNetUtils::CTestSafeNetUtils()
	: m_snlmLicense( gusblFlexIndex )
{
}
//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CTestSafeNetUtils::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITestSafeNetUtils
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}
//--------------------------------------------------------------------------------------------------
// ITestableComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CTestSafeNetUtils::raw_RunAutomatedTests(IVariantVector * pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_ipResultLogger == __nullptr)
		{
			throw UCLIDException("ELI11738", "Please set ResultLogger before proceeding.");
		}
		runTestCase1();
		runTestCase2();
		runTestCase3();
		runTestCase4();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11737")
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CTestSafeNetUtils::raw_RunInteractiveTests()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CTestSafeNetUtils::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	m_ipResultLogger = pLogger;

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CTestSafeNetUtils::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CTestSafeNetUtils::runTestCase1()
{
	//This test case tests the incrementing of each of the counters
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_1_1"), get_bstr_t("Testing Counter Increment Indexing"), kAutomatedTestCase); 
	int i;
	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		// Get the original value and set the value of the counter to 0
		SP_DWORD dwOriginalValue = m_snlmLicense.setCellValue(gdcellFlexIndexingCounter, 0);
		for ( i = 0; i < 100; i++ )
		{
			// Increase cell value by 1
			m_snlmLicense.increaseCellValue(gdcellFlexIndexingCounter, 1 );
		}
		SP_DWORD dwTestResult = m_snlmLicense.setCellValue(gdcellFlexIndexingCounter, dwOriginalValue);
		if ( dwTestResult != 100 )
		{
			bSuccess = false;
		}
		CString zTemp("");
		zTemp.Format("Expected Counter Value: 100\r\nCounter Value: %lu", dwTestResult );
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI11739", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	bSuccess = bSuccess && !bExceptionCaught;
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));

	//Test the Pagination Counter
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_1_2"), get_bstr_t("Testing Counter Increment Pagination"), kAutomatedTestCase); 
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// Get the original value and set the value of the counter to 0
		SP_DWORD dwOriginalValue = m_snlmLicense.setCellValue(gdcellFlexPaginationCounter, 0);
		for ( i = 0; i < 100; i++ )
		{
			// Increase cell value by 1
			m_snlmLicense.increaseCellValue(gdcellFlexPaginationCounter, 1 );
		}
		SP_DWORD dwTestResult = m_snlmLicense.setCellValue(gdcellFlexPaginationCounter, dwOriginalValue);
		if ( dwTestResult != 100 )
		{
			bSuccess = false;
		}
		CString zTemp("");
		zTemp.Format("Expected Counter Value: 100\r\nCounter Value: %lu", dwTestResult );
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI11746", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	bSuccess = bSuccess && !bExceptionCaught;
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));

	//Test the Redaction Counter
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_1_3"), get_bstr_t("Testing Counter Increment Redaction"), kAutomatedTestCase); 
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// Get the original value and set the value of the counter to 0
		SP_DWORD dwOriginalValue = m_snlmLicense.setCellValue(gdcellIDShieldRedactionCounter, 0);
		for ( i = 0; i < 100; i++ )
		{
			// Increase cell value by 1
			m_snlmLicense.increaseCellValue(gdcellIDShieldRedactionCounter, 1 );
		}
		SP_DWORD dwTestResult = m_snlmLicense.setCellValue(gdcellIDShieldRedactionCounter, dwOriginalValue);
		if ( dwTestResult != 100 )
		{
			bSuccess = false;
		}
		CString zTemp("");
		zTemp.Format("Expected Counter Value: 100\r\nCounter Value: %lu", dwTestResult );
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI11747", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	bSuccess = bSuccess && !bExceptionCaught;
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
}
//--------------------------------------------------------------------------------------------------
void CTestSafeNetUtils::runTestCase2()
{
	//This test case tests the decrementing of each of the counters
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_2_1"), get_bstr_t("Testing Counter Decrement Indexing"), kAutomatedTestCase); 
	int i;
	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		// Get the original value and set the value of the counter to 0
		SP_DWORD dwOriginalValue = m_snlmLicense.setCellValue(gdcellFlexIndexingCounter, 100);
		for ( i = 0; i < 100; i++ )
		{
			// Decrease cell value by 1
			m_snlmLicense.decreaseCellValue(gdcellFlexIndexingCounter, 1 );
		}
		SP_DWORD dwTestResult = m_snlmLicense.setCellValue(gdcellFlexIndexingCounter, dwOriginalValue);
		if ( dwTestResult != 0 )
		{
			bSuccess = false;
		}
		CString zTemp("");
		zTemp.Format("Expected Counter Value: 0\r\nCounter Value: %lu", dwTestResult );
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI11745", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	bSuccess = bSuccess && !bExceptionCaught;
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));

	//Test the Pagination Counter
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_2_2"), get_bstr_t("Testing Counter Decrement Pagination"), kAutomatedTestCase); 
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// Get the original value and set the value of the counter to 0
		SP_DWORD dwOriginalValue = m_snlmLicense.setCellValue(gdcellFlexPaginationCounter, 100);
		for ( i = 0; i < 100; i++ )
		{
			// Decrease cell value by 1
			m_snlmLicense.decreaseCellValue(gdcellFlexPaginationCounter, 1 );
		}
		SP_DWORD dwTestResult = m_snlmLicense.setCellValue(gdcellFlexPaginationCounter, dwOriginalValue);
		if ( dwTestResult != 0 )
		{
			bSuccess = false;
		}
		CString zTemp("");
		zTemp.Format("Expected Counter Value: 0\r\nCounter Value: %lu", dwTestResult );
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI11740", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	bSuccess = bSuccess && !bExceptionCaught;
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));

	//Test the Redaction Counter
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_2_3"), get_bstr_t("Testing Counter Decrement Redaction"), kAutomatedTestCase); 
	bExceptionCaught = false;
	bSuccess = true;
	try
	{
		// Get the original value and set the value of the counter to 0
		SP_DWORD dwOriginalValue = m_snlmLicense.setCellValue(gdcellIDShieldRedactionCounter, 100);
		for ( i = 0; i < 100; i++ )
		{
			// Decrease cell value by 1
			m_snlmLicense.decreaseCellValue(gdcellIDShieldRedactionCounter, 1 );
		}
		SP_DWORD dwTestResult = m_snlmLicense.setCellValue(gdcellIDShieldRedactionCounter, dwOriginalValue);
		if ( dwTestResult != 0 )
		{
			bSuccess = false;
		}
		CString zTemp("");
		zTemp.Format("Expected Counter Value: 0\r\nCounter Value: %lu", dwTestResult );
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI11741", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	bSuccess = bSuccess && !bExceptionCaught;
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
}
//--------------------------------------------------------------------------------------------------
//ThreadDataClass
//--------------------------------------------------------------------------------------------------
CTestSafeNetUtils::ThreadDataClass::ThreadDataClass(SafeNetLicenseMgr &rsnlmLM, DataCell &rdcCell )
: m_rsnlmLM(rsnlmLM), m_rdcCell(rdcCell), m_bException(false)
{
}
//--------------------------------------------------------------------------------------------------
CTestSafeNetUtils::ThreadDataClass::~ThreadDataClass()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16490");
}
//--------------------------------------------------------------------------------------------------
void CTestSafeNetUtils::ThreadDataClass::resetEvents()
{
	m_threadStartedEvent.reset();
	m_threadEndedEvent.reset();
	m_threadStopRequest.reset();
}
//--------------------------------------------------------------------------------------------------
UINT decrementThreadFunc(void *pData)
{
	CTestSafeNetUtils::ThreadDataClass* pTDC = (CTestSafeNetUtils::ThreadDataClass *) pData;
	try
	{
		pTDC->m_threadStartedEvent.signal();
		for ( int i = 0; i < 10; i++ )
		{
			pTDC->m_rsnlmLM.decreaseCellValue(pTDC->m_rdcCell, 1);
			if ( pTDC->m_threadStopRequest.isSignaled() )
			{
				break;
			}
		}
	}
	catch ( UCLIDException ue )
	{
		pTDC->m_ue = ue;
		pTDC->m_bException = true;
	}
	catch(...)
	{
		UCLIDException ue("ELI11742", "Unexpected Exception!" );
		pTDC->m_ue = ue;
		pTDC->m_bException = true;
	}
	pTDC->m_threadEndedEvent.signal();
	return 0;
}
//--------------------------------------------------------------------------------------------------
UINT incrementThreadFunc(void *pData)
{
	CTestSafeNetUtils::ThreadDataClass* pTDC = (CTestSafeNetUtils::ThreadDataClass *) pData;
	try
	{
		pTDC->m_threadStartedEvent.signal();
		for ( int i = 0; i < 10; i++ )
		{
			pTDC->m_rsnlmLM.increaseCellValue(pTDC->m_rdcCell, 10);
			if ( pTDC->m_threadStopRequest.isSignaled() )
			{
				break;
			}
		}
	}
	catch ( UCLIDException ue )
	{
		pTDC->m_ue = ue;
		pTDC->m_bException = true;
	}
	catch(...)
	{
		UCLIDException ue("ELI11743", "Unexpected Exception!" );
		pTDC->m_ue = ue;
		pTDC->m_bException = true;
	}
	pTDC->m_threadEndedEvent.signal();

	return 0;

}
//--------------------------------------------------------------------------------------------------
void CTestSafeNetUtils::runTestCase3()
{
	//This test case tests the decrementing of each of the counters with multiple threads
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_3_1"), get_bstr_t("Testing Multi Thread Counter Decrement and Increment"), kAutomatedTestCase); 
	
	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{
		// Get the original value and set the value of the counter to 100
		SP_DWORD dwOriginalIndexing = m_snlmLicense.setCellValue(gdcellFlexIndexingCounter, 100);
		SP_DWORD dwOriginalPagination = m_snlmLicense.setCellValue(gdcellFlexPaginationCounter, 100);
		SP_DWORD dwOriginalRedaction = m_snlmLicense.setCellValue(gdcellIDShieldRedactionCounter, 100 );
		
		// Initialize thread data
		CTestSafeNetUtils::ThreadDataClass tdcIncIndexing(m_snlmLicense, gdcellFlexIndexingCounter);
		CTestSafeNetUtils::ThreadDataClass tdcDecIndexing(m_snlmLicense, gdcellFlexIndexingCounter);
		CTestSafeNetUtils::ThreadDataClass tdcIncPagination(m_snlmLicense, gdcellFlexPaginationCounter);
		CTestSafeNetUtils::ThreadDataClass tdcDecPagination(m_snlmLicense, gdcellFlexPaginationCounter);
		CTestSafeNetUtils::ThreadDataClass tdcIncRedaction(m_snlmLicense, gdcellIDShieldRedactionCounter);
		CTestSafeNetUtils::ThreadDataClass tdcDecRedaction(m_snlmLicense, gdcellIDShieldRedactionCounter);


		// Begin each thread
		tdcIncIndexing.m_pThread = AfxBeginThread(incrementThreadFunc, &tdcIncIndexing);
		ASSERT_RESOURCE_ALLOCATION("ELI11748", tdcIncIndexing.m_pThread != __nullptr );
		tdcDecIndexing.m_pThread = AfxBeginThread(decrementThreadFunc, &tdcDecIndexing);
		ASSERT_RESOURCE_ALLOCATION("ELI11749", tdcDecIndexing.m_pThread != __nullptr );
		tdcIncPagination.m_pThread = AfxBeginThread(incrementThreadFunc, &tdcIncPagination);
		ASSERT_RESOURCE_ALLOCATION("ELI11750", tdcIncPagination.m_pThread != __nullptr );
		tdcDecPagination.m_pThread = AfxBeginThread(decrementThreadFunc, &tdcDecPagination);
		ASSERT_RESOURCE_ALLOCATION("ELI11751", tdcDecPagination.m_pThread != __nullptr );
		tdcIncRedaction.m_pThread = AfxBeginThread(incrementThreadFunc, &tdcIncRedaction);
		ASSERT_RESOURCE_ALLOCATION("ELI11752", tdcIncRedaction.m_pThread != __nullptr );
		tdcDecRedaction.m_pThread = AfxBeginThread(decrementThreadFunc, &tdcDecRedaction);
		ASSERT_RESOURCE_ALLOCATION("ELI11753", tdcDecRedaction.m_pThread != __nullptr );

		// Wait for all threads to start
		tdcIncIndexing.m_threadStartedEvent.wait();
		tdcDecIndexing.m_threadStartedEvent.wait();
		tdcIncPagination.m_threadStartedEvent.wait();
		tdcDecPagination.m_threadStartedEvent.wait();
		tdcIncRedaction.m_threadStartedEvent.wait();
		tdcDecRedaction.m_threadStartedEvent.wait();

		// Wait for all threads to  finish
		tdcIncIndexing.m_threadEndedEvent.wait();
		tdcDecIndexing.m_threadEndedEvent.wait();
		tdcIncPagination.m_threadEndedEvent.wait();
		tdcDecPagination.m_threadEndedEvent.wait();
		tdcIncRedaction.m_threadEndedEvent.wait();
		tdcDecRedaction.m_threadEndedEvent.wait();
		
		// Get the final results and restore original value
		SP_DWORD dwIndexing = m_snlmLicense.setCellValue(gdcellFlexIndexingCounter, dwOriginalIndexing); 
		SP_DWORD dwPagination = m_snlmLicense.setCellValue(gdcellFlexPaginationCounter, dwOriginalPagination);
		SP_DWORD dwRedaction = m_snlmLicense.setCellValue(gdcellIDShieldRedactionCounter, dwOriginalRedaction);


		// Post Exceptions

		if ( tdcIncIndexing.m_bException )
		{
			m_ipResultLogger->AddComponentTestException(tdcIncIndexing.m_ue.asStringizedByteStream().c_str());
		}
		if ( tdcDecIndexing.m_bException )
		{
			m_ipResultLogger->AddComponentTestException(tdcDecIndexing.m_ue.asStringizedByteStream().c_str());
		}
		if ( tdcIncPagination.m_bException )
		{
			m_ipResultLogger->AddComponentTestException(tdcIncPagination.m_ue.asStringizedByteStream().c_str());
		}
		if ( tdcDecPagination.m_bException )
		{
			m_ipResultLogger->AddComponentTestException(tdcDecPagination.m_ue.asStringizedByteStream().c_str());
		}
		if ( tdcIncRedaction.m_bException )
		{
			m_ipResultLogger->AddComponentTestException(tdcIncRedaction.m_ue.asStringizedByteStream().c_str());
		}
		if ( tdcIncRedaction.m_bException )
		{
			m_ipResultLogger->AddComponentTestException(tdcDecRedaction.m_ue.asStringizedByteStream().c_str());
		}

		// Display the Results
		CString zTemp("");
		zTemp.Format(	"Expected Indexing Counter: 190\r\n"
						"Counter Value: %lu", dwIndexing );
		m_ipResultLogger->AddTestCaseMemo("Indexing Result", get_bstr_t(zTemp));
		zTemp.Format(	"Expected Pagination Counter: 190\r\n"
						"Counter Value: %lu", dwPagination );
		m_ipResultLogger->AddTestCaseMemo("Pagination Result", get_bstr_t(zTemp));
		
		zTemp.Format(	"Expected Redaction  Counter: 190\r\n"
						"Counter Value: %lu", dwRedaction );
		m_ipResultLogger->AddTestCaseMemo("Redaction Result", get_bstr_t(zTemp));

		// Determine if the case passed
		if ( dwIndexing != 190 || dwPagination != 190 || dwRedaction != 190)
		{
			bSuccess = false;
		}
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI11744", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	bSuccess = bSuccess && !bExceptionCaught;
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));

}
//--------------------------------------------------------------------------------------------------
void CTestSafeNetUtils::runTestCase4()
{
	//This test case tests the incrementing of each of the counters
	m_ipResultLogger->StartTestCase(get_bstr_t("TEST_4"), get_bstr_t("Testing Lock Reset"), kAutomatedTestCase); 
	int i;
	bool bExceptionCaught = false;
	bool bSuccess = true;
	try
	{

		// Get the original value and set the value of the counter to 0
		SP_DWORD dwOriginalValue = m_snlmLicense.setCellValue(gdcellFlexIndexingCounter, 0);
		
		// Lock the cell - to have it locked before the call to increaseCell value
		// The cell is locked to force increaseCellValue to auto reset the lock
		// in order to successfully update the key
		SafeNetLicenseMgr::CellLock cellLock(m_snlmLicense, gdcellFlexIndexingCounter);
		for ( i = 0; i < 2;  i++ )
		{
			// Increase cell value by 1
			m_snlmLicense.increaseCellValue(gdcellFlexIndexingCounter, 1 );

			// Add Note indicating that the increase value completed which means the key was unlocked
			string strNote = "Unlocked sucessfully " + asString(i+1) + " time(s).";
			m_ipResultLogger->AddTestCaseNote(strNote.c_str());

			// Lock the cell again
			cellLock.lock();
		}
		// Unlock the cell
		cellLock.unlock();

		SP_DWORD dwTestResult = m_snlmLicense.setCellValue(gdcellFlexIndexingCounter, dwOriginalValue);
		if ( dwTestResult != 2 )
		{
			bSuccess = false;
		}
		CString zTemp("");
		zTemp.Format("Expected Counter Value: 2\r\nCounter Value: %lu", dwTestResult );
		m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI18564", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	bSuccess = bSuccess && !bExceptionCaught;
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
}
//--------------------------------------------------------------------------------------------------
