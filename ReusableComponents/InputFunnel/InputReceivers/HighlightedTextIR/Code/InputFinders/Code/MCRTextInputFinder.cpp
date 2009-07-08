// MCRTextInputFinder.cpp : Implementation of CMCRTextInputFinder
#include "stdafx.h"
#include "InputFinders.h"
#include "MCRTextInputFinder.h"
#include "MCRTextFinderEngine.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRTextInputFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInputFinder,
		&IID_ICategorizedComponent,
		&IID_ILicensedComponent,
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
// IInputFinder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRTextInputFinder::ParseString(BSTR strInput, IIUnknownVector **ippTokenPositions)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		validateLicense();

		CComPtr<IIUnknownVector> ipUnknownVec;
		HRESULT hr = ipUnknownVec.CoCreateInstance(__uuidof(IUnknownVector));
		if (FAILED(hr))
		{
			UCLIDException uclidException("ELI02761", "Failed to create IUnknownVector");
			uclidException.addDebugInfo("hr", hr);
			throw uclidException;
		}

		// use the MCRTextFinderEngine to spot all MCR Text positions
		MCRTextFinderEngine finder;
		finder.parseString( asString( strInput ) );
		vector<MCRStringInfo> vecMCRTextPositions = finder.getMCRStringsInfo();

		// store the position info into the IToken, then push them into the unknown vec
		for (unsigned long n = 0 ; n < vecMCRTextPositions.size(); n++)
		{
			CComPtr<IToken> ipTokenPos;
			HRESULT hr = ipTokenPos.CoCreateInstance(__uuidof(Token));
			if (FAILED(hr))
			{
				UCLIDException uclidException("ELI02744", 
					"Failed to create Token object");
				uclidException.addDebugInfo("HRESULT", hr);
				throw uclidException;
			}

			hr = ipTokenPos->InitToken(vecMCRTextPositions[n].ulStartCharPos, 
				vecMCRTextPositions[n].ulEndCharPos, 
				_bstr_t(( vecMCRTextPositions[n].strTypeInfo ).c_str()), 
				_bstr_t(( vecMCRTextPositions[n].strText ).c_str()) );
			if (FAILED(hr))
			{
				UCLIDException uclidException("ELI02745", "Failed to initialize Token");
				uclidException.addDebugInfo("HRESULT", hr);
				throw uclidException;
			}

			ipUnknownVec->PushBack(ipTokenPos);
		}

		*ippTokenPositions = ipUnknownVec.Detach();

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02743")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRTextInputFinder::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		validateLicense();

		*pbstrComponentDescription = _bstr_t("Mathematical Content").copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02740")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRTextInputFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())		
	try
	{
		validateLicense();
		// if validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRTextInputFinder::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	try
	{
		automatedTest1();
	}
	CATCH_UCLID_EXCEPTION("ELI02756")
	CATCH_COM_EXCEPTION("ELI02757")
	CATCH_UNEXPECTED_EXCEPTION("ELI02759")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRTextInputFinder::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRTextInputFinder::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMCRTextInputFinder::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CMCRTextInputFinder::validateLicense()
{
	static const unsigned long MCR_TEXT_INPUT_FINDER_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( MCR_TEXT_INPUT_FINDER_COMPONENT_ID, 
		"ELI02739", "MCRText Input Finder" );
}
//-------------------------------------------------------------------------------------------------
void CMCRTextInputFinder::automatedTest1()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestMCRTextIF_1"), _bstr_t("Validate Token Positions"), kAutomatedTestCase); 
	VARIANT_BOOL bRet = VARIANT_FALSE;
		
	try
	{
		// set a string for testing
		_bstr_t bstrInput("	N223344W, \n\r1234 feet  \n\r\n\r.");

		MCRStringInfo token;
		token.ulStartCharPos = 1;
		token.ulEndCharPos = 8;
		vector<MCRStringInfo> vecTokenPos;
		vecTokenPos.push_back(token);

		token.ulStartCharPos = 13;
		token.ulEndCharPos = 21;
		vecTokenPos.push_back(token);

		CComPtr<IIUnknownVector> ipTokenPositions;
		HRESULT hr = ParseString(bstrInput, &ipTokenPositions);
		if (FAILED(hr))
		{
			UCLIDException uclidException("ELI13153", "Failed to parse the string");
			uclidException.addDebugInfo("hr", hr);
			uclidException.addDebugInfo("String for parse", string(bstrInput));
			throw uclidException;
		}

		for (int i = 0; i < ipTokenPositions->Size(); i++)
		{
			ITokenPtr ipToken(ipTokenPositions->At(i));

			// Get start and end positions - don't care about BSTR's
			long	nStart;
			long	nEnd;
			ipToken->GetTokenInfo( &nStart, &nEnd, NULL, NULL );
			if (nStart != vecTokenPos[i].ulStartCharPos || nEnd != vecTokenPos[i].ulEndCharPos)
			{
				bRet = VARIANT_FALSE;
				break;
			}

			bRet = VARIANT_TRUE;
		}
	}
	catch (UCLIDException& ue)
	{
		ue.addDebugInfo("Test No", "TestMCRTextIF_1");
		string strError("");
		ue.asString(strError);
		m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()), VARIANT_FALSE);
		bRet = VARIANT_FALSE;
	}
	catch (...)
	{
		UCLIDException uclidException("ELI02763", "Unknown Exception is caught");
		uclidException.addDebugInfo("Test No", "TestMCRTextIF_1");
		string strError("");
		uclidException.asString(strError);
		m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()), VARIANT_FALSE);
		bRet = VARIANT_FALSE;
	}
	
	m_ipResultLogger->EndTestCase(bRet);
}
//-------------------------------------------------------------------------------------------------
