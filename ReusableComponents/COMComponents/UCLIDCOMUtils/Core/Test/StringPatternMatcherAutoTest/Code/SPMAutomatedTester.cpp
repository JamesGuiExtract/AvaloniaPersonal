// SPMAutomatedTester.cpp : Implementation of CSPMAutomatedTester
#include "stdafx.h"
#include "StringPatternMatcherAutoTest.h"
#include "SPMAutomatedTester.h"
#include "TestData.h"

#include <UCLIDException.h>

#include <cpputil.h>
#include <comutils.h>

//-------------------------------------------------------------------------------------------------
// CSPMAutomatedTester
//-------------------------------------------------------------------------------------------------
CSPMAutomatedTester::CSPMAutomatedTester()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMAutomatedTester::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISPMAutomatedTester,
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
STDMETHODIMP CSPMAutomatedTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	bool bExceptionCaught = false;
	
	try
	{
		m_ipResultLogger->StartTestCase(_bstr_t("Initialization"), _bstr_t("Reading test data file"), kAutomatedTestCase); 
		
		// ensure that the test result logger has been set
		if (m_ipResultLogger == __nullptr)
		{
			throw UCLIDException("ELI05985", "Test Result Logger object not set!");
		}
		
		setTestFileFolder( pParams, asString( strTCLFile ) );

		// load the test data (treat as a separate test case)
		TestData td(m_strTestFilesFolder);

		m_ipResultLogger->EndTestCase(VARIANT_TRUE); 

		// execute each of the test cases
		unsigned long ulNumTestCases = td.getNumTestCases();
		unsigned long nNumFailedCases = 0;

		for (unsigned int i = 0; i < ulNumTestCases; i++)
		{
			// create instance of the string pattern matcher object
			// NOTE: we are purposely creating this inside the for loop
			// so that the object's attributes are guaranteed to be at
			// their default state before each test case is executed 
			// (and consequently, running a test case does not modify
			// the state of the string pattern matcher object for
			// subsequent test cases)
			IStringPatternMatcherPtr ipSPM(CLSID_StringPatternMatcher);
			ASSERT_RESOURCE_ALLOCATION("ELI05986", ipSPM != __nullptr);

			// retrieve the test case information
			TestCaseData tc = td[i];

			// Initiate a test case
			string strIndex = asString(i);
			m_ipResultLogger->StartTestCase(_bstr_t(strIndex.c_str()), _bstr_t("StringPatternMatcher Test"), kAutomatedTestCase); 

			// add a detail note with the test case info (in XML)
			m_ipResultLogger->AddTestCaseDetailNote(_bstr_t("XML"), tc.m_bstrXML);

			// before executing the test case, set the
			// case-sensitivity flag
			if (tc.m_iCaseSensitive != -1)
			{
				ipSPM->CaseSensitive = tc.m_iCaseSensitive == 1 ? 
					VARIANT_TRUE : VARIANT_FALSE;
			}

			// before executing the test case, set the
			// TreatMultipleWSAsOne flag
			if (tc.m_iTreatMultipleWSAsOne != -1)
			{
				ipSPM->TreatMultipleWSAsOne = tc.m_iTreatMultipleWSAsOne == 1 ? 
					VARIANT_TRUE : VARIANT_FALSE;
			}

			// execute the test case
			IStrToObjectMapPtr ipResults = ipSPM->Match1(tc.m_bstrInput, 
				tc.m_bstrPattern, tc.m_ipExprMap, 
				tc.m_bGreedy ? VARIANT_TRUE : VARIANT_FALSE);

			// add test case notes with the matches found
			long nNumMatches = ipResults->Size;
			if (nNumMatches == 0)
			{
				m_ipResultLogger->AddTestCaseNote(_bstr_t("No matches found!"));
			}
			else
			{
				for (int j = 0; j < nNumMatches; j++)
				{
					string strMsg = "Match found: <";
					CComBSTR bstrVariableName(L"");
					IUnknownPtr ipUnknown;
					ipResults->GetKeyValue(j, &bstrVariableName, &ipUnknown);
					ITokenPtr ipToken = ipUnknown;
					ASSERT_RESOURCE_ALLOCATION("ELI06271", ipToken != __nullptr);
					_bstr_t _bstrMatch = ipToken->Value;
					strMsg += asString(bstrVariableName);
					strMsg += "> = <";
					strMsg += _bstrMatch;
					strMsg += ">";
					m_ipResultLogger->AddTestCaseNote(_bstr_t(strMsg.c_str()));
				}
			}

			// check the results and determine pass/fail for the test case
			// end the test case
			VARIANT_BOOL bRet = VARIANT_TRUE;
			
			// if the expected and actual results are not the same size,
			// the test case failed
			if (ipResults->Size != tc.m_ipExpectedMatches->Size)
			{
				bRet = VARIANT_FALSE;
				UCLIDException ue("ELI05992", "Incorrect number of matches found!");
				ue.addDebugInfo("Expected", tc.m_ipExpectedMatches->Size);
				ue.addDebugInfo("Actual", ipResults->Size);
				m_ipResultLogger->AddTestCaseException(
					_bstr_t(ue.asStringizedByteStream().c_str()), VARIANT_FALSE);
			}
			else
			{
				// if the contents of the expected results are not the
				// same as the contents of the actual results, then
				// the the test case failed
				for (int j = 0; j < ipResults->Size; j++)
				{
					// get the actual match
					CComBSTR bstrMatchVariableName;
					IUnknownPtr ipUnkMatchValue;
					ipResults->GetKeyValue(j, &bstrMatchVariableName, &ipUnkMatchValue);
					_bstr_t _bstrMatchVariableName(bstrMatchVariableName);

					// get the actual match value
					ITokenPtr ipToken = ipUnkMatchValue;
					ASSERT_RESOURCE_ALLOCATION("ELI06273", ipToken != __nullptr);
					_bstr_t _bstrActualMatchValue = ipToken->Value;

					// check if the actual match is found in the expected matches
					// list
					if (tc.m_ipExpectedMatches->Contains(_bstrMatchVariableName))
					{
						// the match variable was found in the expected matches list
						// now check if the values match
						_bstr_t _bstrExpectedValue = 
							tc.m_ipExpectedMatches->GetValue(_bstrMatchVariableName);
						
						// get the expected and actual match values as strings
						string strExpectedValue = _bstrExpectedValue;
						string strActualValue = _bstrActualMatchValue;

						// remove leading and trailing whitespace before
						// comparing matches
						string strWS = " \n\r\t";
						strExpectedValue = trim(strExpectedValue, strWS, strWS);
						strActualValue = trim(strActualValue, strWS, strWS);
						
						if (strActualValue != strExpectedValue)
						{
							// an invalid match value was found!
							UCLIDException ue("ELI05989", "Invalid match found!");
							ue.addDebugInfo("MatchVariableName", (char *) _bstrMatchVariableName);
							ue.addDebugInfo("ExpectedValue", strExpectedValue);
							ue.addDebugInfo("ActualValue", strActualValue);
							bRet = VARIANT_FALSE;
							m_ipResultLogger->AddTestCaseException(
								_bstr_t(ue.asStringizedByteStream().c_str()), VARIANT_FALSE);
						}
					}
					else
					{
						// an unexpected match was found!
						UCLIDException ue("ELI06276", "Unexpected match found!");
						ue.addDebugInfo("MatchVariableName", (char *) _bstrMatchVariableName);
						ue.addDebugInfo("ActualValue", (char *) _bstrActualMatchValue);
						bRet = VARIANT_FALSE;
						m_ipResultLogger->AddTestCaseException(
							_bstr_t(ue.asStringizedByteStream().c_str()), VARIANT_FALSE);
					}
				}
			}

			// log the pass/fail status of the test case
			m_ipResultLogger->EndTestCase(bRet);
			
			// increment the number of failed cases, if appropriate
			if (bRet == VARIANT_FALSE)
			{
				nNumFailedCases++;
			}
		}

		// print the summary results
		m_ipResultLogger->StartTestCase(_bstr_t("Summary"), _bstr_t("String Pattern Matching results"), kAutomatedTestCase); 
		CString zTemp;
		
		zTemp.Format("Total test cases executed: %d", ulNumTestCases);
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		
		zTemp.Format("Total passed: %d", ulNumTestCases - nNumFailedCases);
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		
		zTemp.Format("Total failed: %d", nNumFailedCases);
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		
		if (ulNumTestCases != 0)
		{
			zTemp.Format("Percentage success: %.2f%%", 
				(double)((ulNumTestCases - nNumFailedCases) * 100) /
				(double) (ulNumTestCases));
			m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));
		}

		m_ipResultLogger->EndTestCase(nNumFailedCases == 0 ? VARIANT_TRUE : VARIANT_FALSE);
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI05971", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE)

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMAutomatedTester::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMAutomatedTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSPMAutomatedTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CSPMAutomatedTester::setTestFileFolder(IVariantVectorPtr ipParams, const std::string &strTCLFile)
{
	// if pParams is not empty and the second item is specified,
	// then the second item is the master dat file
	if ((ipParams != __nullptr) && (ipParams->Size > 1))
	{
		std::string strTestFolder = asString(_bstr_t(ipParams->GetItem(1)));

		if(strTestFolder != "")
		{
			m_strTestFilesFolder = getDirectoryFromFullPath(::getAbsoluteFileName(strTCLFile, strTestFolder + "\\dummy.txt"));

			if(!isValidFolder(m_strTestFilesFolder))
			{
				// Create and throw exception
				UCLIDException ue("ELI12341", "Required test file folder is invalid or not specified in TCL file!");
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
	UCLIDException ue("ELI15706", "Required test file folder not specified in TCL file!");
	throw ue;	
}
//-------------------------------------------------------------------------------------------------
