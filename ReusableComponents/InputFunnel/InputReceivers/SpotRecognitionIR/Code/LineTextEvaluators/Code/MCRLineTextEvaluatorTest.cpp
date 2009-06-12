
#include "stdafx.h"
#include "LineTextEvaluators.h"
#include "MCRLineTextEvaluator.h"

//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRLineTextEvaluator::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	bool bSomeTestFailed = false;

	// start test
	m_ipLogger->StartTestCase(_bstr_t("atc01"), _bstr_t("Simple score test"), kAutomatedTestCase);

	// create some input strings
	_bstr_t _bstrInput1 = "S32°23'E";
	_bstr_t _bstrInput2 = "S3Z02BE";

	// get the scores associated with the input strings
	_bstr_t _bstrInputType = "Bearing";
	long lScore1 = GetTextScore(_bstrInput1, _bstrInputType);
	long lScore2 = GetTextScore(_bstrInput2, _bstrInputType);

	// consider the test passed as long as lScore1 > lScore2
	if (lScore1 <= lScore2)
	{
		bSomeTestFailed = true;
		m_ipLogger->AddTestCaseNote(_bstr_t("Evaluated test scores were not as expected."));
	}

	VARIANT_BOOL bSuccess = bSomeTestFailed ? VARIANT_FALSE : VARIANT_TRUE;
	m_ipLogger->EndTestCase(bSuccess);

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRLineTextEvaluator::raw_RunInteractiveTests()
{
	// nothing to do
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRLineTextEvaluator::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	// store the reference to the result logger
	m_ipLogger = pLogger;

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRLineTextEvaluator::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	// no need to remember the interactive test executor instance, as this
	// component does not have an interactive test.
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
