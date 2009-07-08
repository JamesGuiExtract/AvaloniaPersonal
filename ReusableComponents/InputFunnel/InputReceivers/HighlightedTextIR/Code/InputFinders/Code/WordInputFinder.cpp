// WordInputFinder.cpp : Implementation of CWordInputFinder
#include "stdafx.h"
#include "InputFinders.h"
#include "WordInputFinder.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWordInputFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInputFinder,
		&IID_ICategorizedComponent,
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
// IInputFinder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWordInputFinder::ParseString(BSTR strInput, IIUnknownVector ** ippTokenPositions)
{		
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// find each and every word in strInput, and store the positions in the vector
		findWordsPositions( asString( strInput ), ippTokenPositions);
		
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02738")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWordInputFinder::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		validateLicense();

		*pbstrComponentDescription = CComBSTR("All Words");
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02737")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWordInputFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
STDMETHODIMP CWordInputFinder::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	try
	{
		automatedTest1();
	}
	CATCH_UCLID_EXCEPTION("ELI02753")
	CATCH_COM_EXCEPTION("ELI02754")
	CATCH_UNEXPECTED_EXCEPTION("ELI02755")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWordInputFinder::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWordInputFinder::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWordInputFinder::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CWordInputFinder::automatedTest1()
{
	// start the test case
	m_ipResultLogger->StartTestCase(_bstr_t("TestWordIF_1"), _bstr_t("Validate Word Positions"), kAutomatedTestCase); 
	VARIANT_BOOL bRet = VARIANT_FALSE;
		
	try
	{
		// set a string for testing
		_bstr_t bstrInput("AWord, \n\r1234 t  \n\rRREET\n\rOpp. \t");

		WordPos token;
		token.ulStart = 0;
		token.ulEnd = 5;
		vector<WordPos> vecTokenPos;
		vecTokenPos.push_back(token);

		token.ulStart = 9;
		token.ulEnd = 12;
		vecTokenPos.push_back(token);

		token.ulStart = 14;
		token.ulEnd = 14;
		vecTokenPos.push_back(token);

		token.ulStart = 19;
		token.ulEnd = 23;
		vecTokenPos.push_back(token);

		token.ulStart = 26;
		token.ulEnd = 29;
		vecTokenPos.push_back(token);

		CComPtr<IIUnknownVector> ipTokenPositions;
		HRESULT hr = ParseString(bstrInput, &ipTokenPositions);
		if (FAILED(hr))
		{
			UCLIDException uclidException("ELI02762", "Failed to parse the string");
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
			if (nStart != vecTokenPos[i].ulStart || nEnd != vecTokenPos[i].ulEnd)
			{
				bRet = VARIANT_FALSE;
				break;
			}

			bRet = VARIANT_TRUE;
		}
	}
	catch (UCLIDException& ue)
	{
		ue.addDebugInfo("Test No", "TestWordIF_1");
		string strError("");
		ue.asString(strError);
		m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()), VARIANT_FALSE);
		bRet = VARIANT_FALSE;
	}
	catch (...)
	{
		UCLIDException uclidException("ELI02766", "Unknown Exception is caught");
		uclidException.addDebugInfo("Test No", "TestWordIF_1");
		string strError("");
		uclidException.asString(strError);
		m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()), VARIANT_FALSE);
		bRet = VARIANT_FALSE;
	}
	
	m_ipResultLogger->EndTestCase(bRet);
}
//-------------------------------------------------------------------------------------------------
void CWordInputFinder::findWordsPositions(const string& strInput, IIUnknownVector **ippTokenPositions)
{
	//replace all \n, \r with spaces
	unsigned int iLen = strInput.size();
	// FIXTHIS auto_ptr set to an new array
	char * cStr = new char[iLen+1];
	string strForParse = "";

	try
	{
		strcpy_s(cStr, iLen+1, strInput.data());
		char *pDest = NULL;
		int iValueArr[] = {'\n', '\r'};
		for(int i = 0; i < 2; i++)
		{
			pDest = strchr(cStr, iValueArr[i]);
			while(pDest != NULL)
			{
				int iResultPos;	
				iResultPos = pDest - cStr;
				//replace with the space
				cStr[iResultPos] = ' ';
				pDest = strchr(cStr, iValueArr[i]);
			}
		}
		strForParse = cStr;
	}
	catch(...)
	{
		delete [] cStr;
		throw;
	}
	delete [] cStr;

	// create the vector
	CComPtr<IIUnknownVector> ipUnknownVec;
	HRESULT hr = ipUnknownVec.CoCreateInstance(__uuidof(IUnknownVector));
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02764", "Failed to create IUnknownVector");
		uclidException.addDebugInfo("hr", hr);
		throw uclidException;
	}

	//space, tab
	char* blank = " \t";
	// start search position
	unsigned int nWordStartPos = 0;
	unsigned int nWordEndPos = 0;

	// search from start to end
	while (nWordStartPos < strForParse.length() - 1)
	{
		// search every word that is seperated by space(s)
		nWordEndPos = strForParse.find_first_not_of(blank, nWordStartPos);
		// can not find any word other than space/tab, get out of the loop
		if (nWordEndPos == string::npos)
		{
			break;
		}

		// create token object for each word
		CComPtr<IToken> ipTokenPos;
		HRESULT hr = ipTokenPos.CoCreateInstance(__uuidof(Token));
		if (FAILED(hr))
		{
			UCLIDException uclidException("ELI02751", "Failed to create Token object");
			uclidException.addDebugInfo("HRESULT", hr);
			throw uclidException;
		}
		
		// else, start pos of the word found, search for the end of the word
		nWordStartPos = nWordEndPos;
		nWordEndPos = strForParse.find_first_of(blank, nWordStartPos);
		if (nWordEndPos == string::npos)
		{
			// no more word found in the string, the end of the string is reached
			ipTokenPos->InitToken( nWordStartPos, strForParse.length() - 1, 
				"Word", strForParse.c_str() );
			ipUnknownVec->PushBack(ipTokenPos);
			// then return
			break;
		}

		// else, the next space/tab is found, which means that the end pos of 
		// this word is found
		ipTokenPos->InitToken( nWordStartPos, nWordEndPos - 1, "Word", 
			strForParse.c_str() );
		ipUnknownVec->PushBack(ipTokenPos);

		// set the start pos to nWordEndPos
		nWordStartPos = nWordEndPos;

	}

	*ippTokenPositions = ipUnknownVec.Detach();
}
//-------------------------------------------------------------------------------------------------
void CWordInputFinder::validateLicense()
{
	static const unsigned long WORD_INPUT_FINDER_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( WORD_INPUT_FINDER_COMPONENT_ID, "ELI02736", 
		"Word Input Finder" );
}
//-------------------------------------------------------------------------------------------------
