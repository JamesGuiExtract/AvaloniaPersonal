
#include "stdafx.h"
#include "ParagraphTextCorrectors.h"
#include "LegalDescriptionTextCorrector.h"

#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionTextCorrector::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	bool bSomeTestFailed = false;
	
	// start the test
	m_ipLogger->StartTestCase(_bstr_t("atc01"), 
		_bstr_t("Simple legal description correction test"), kAutomatedTestCase);

	// perform the test
	// the following input/output pair test to ensure that when spaces appear in "North" and "East,
	// they are eliminated.  The pair also tests to ensure that any sequence of whitespace chars is
	// replaced with a single space.
	_bstr_t _bstrInput = "The following is a bearing: N  orth 32 degrees Eas t.  Thank you.";
	_bstr_t _bstrExpectedOutput = "The following is a bearing: North 32 degrees East. Thank you.";
	
	ISpatialStringPtr ipActualOutput(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI06515", ipActualOutput != NULL);
	ipActualOutput->CreateNonSpatialString(_bstrInput, "");
	CorrectText(ipActualOutput);
	
	_bstr_t _bstrActualOutput = ipActualOutput->String;

	if (_bstrActualOutput != _bstrExpectedOutput)
	{
		bSomeTestFailed = true;
		m_ipLogger->AddTestCaseNote(_bstr_t("Actual output does not match expected output!"));
		_bstr_t _bstrMsg = "Actual output is: ";
		_bstrMsg += _bstrActualOutput;
		m_ipLogger->AddTestCaseNote(_bstrMsg);
	}

	VARIANT_BOOL bSuccess = bSomeTestFailed ? VARIANT_FALSE : VARIANT_TRUE;
	m_ipLogger->EndTestCase(bSuccess);

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionTextCorrector::raw_RunInteractiveTests()
{
	// nothing to do.
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionTextCorrector::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	// remember the logger
	m_ipLogger = pLogger;

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescriptionTextCorrector::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	// nothing to do, because no interactive test
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
