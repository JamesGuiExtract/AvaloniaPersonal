// MERSTester.cpp : Implementation of CMERSTester
#include "stdafx.h"
#include "AFUtilsTest.h"
#include "MERSTester.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <LicenseMgmt.h>

#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#include <io.h>
#include <stdio.h>
#include <fstream>

#define CRTDBG_MAP_ALLOC
#include <stdlib.h>
#include <crtdbg.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CMERSTester
//-------------------------------------------------------------------------------------------------
CMERSTester::CMERSTester()
: m_ipResultLogger(NULL),
  m_ipMERSHandler(NULL)
{
	try
	{
		m_ipAFUtility.CreateInstance( CLSID_AFUtility );
		ASSERT_RESOURCE_ALLOCATION("ELI07366", m_ipAFUtility != __nullptr);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07367")
}
//-------------------------------------------------------------------------------------------------
CMERSTester::~CMERSTester()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16336");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSTester::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITestableComponent,
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
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		// Check license
		validateLicense();

		// get the master test file name
		string strTestFile = getMasterTestFileName( pParams, asString( strTCLFile ) );
		try
		{
			validateFileOrFolderExistence(strTestFile);
		}
		catch (UCLIDException& ue)
		{
			UCLIDException uexOuter("ELI07320", "Unable to open MERS master test input file!", ue);
			throw uexOuter;
		}

		// parse each line of input file into 3 part: 
		// input mock attribute file, actual text input file, expected attributes file
		ifstream ifs(strTestFile.c_str());

		string strLine("");
		CString zCaseNo("");
		CommentedTextFileReader fileReader(ifs, "//", true);

		long nCaseNo = 1;
		do
		{
			strLine = fileReader.getLineText();
			if (strLine.empty()) continue;
			
			// tokenize current line into 3 tokens (delimiter is semicolon";")
			vector<string> vecTokens;
			StringTokenizer::sGetTokens(strLine, ";", vecTokens);
			if (vecTokens.size() != 3)
			{
				UCLIDException uclidException("ELI06290", "Invalid line.");
				uclidException.addDebugInfo("Test file name", strTestFile);
				uclidException.addDebugInfo("Line Text", strLine);
				throw uclidException;
			}
			
			// Input mock attributes file, it has same format as .eav file
			string strInputAttributesFile = getAbsoluteFileName(strTestFile,
				vecTokens[0]);

			// Actual input text file
			string strInputTextFile = getAbsoluteFileName(strTestFile,
				vecTokens[1]);

			// Expected attribute file
			string strEAVFile = getAbsoluteFileName(strTestFile,
				vecTokens[2]);;

			// ensure that none of the 3 tokens are empty
			if (strInputAttributesFile.empty() 
				|| strInputTextFile.empty() 
				|| strEAVFile.empty())
			{
				UCLIDException uclidException("ELI06291", "Invalid line - at least one of the 3 tokens is empty!");
				uclidException.addDebugInfo("Test file name", strTestFile);
				uclidException.addDebugInfo("Line Text", strLine);
				throw uclidException;
			}
			
			// Start the test case
			bool bExceptionCaught = false;
			bool bSuccess = true;
			
			CString zCaseNo;
			zCaseNo.Format("%d", nCaseNo);
			m_ipResultLogger->StartTestCase(_bstr_t(zCaseNo), _bstr_t(strInputTextFile.c_str()), kAutomatedTestCase); 
			
			try
			{
				if (m_ipMERSHandler == __nullptr)
				{
					m_ipMERSHandler.CreateInstance(CLSID_MERSHandler);
					ASSERT_RESOURCE_ALLOCATION("ELI06292", m_ipMERSHandler != __nullptr);
				}

				// get mock attributes 
				IIUnknownVectorPtr ipFoundAttributes = 
					m_ipAFUtility->GenerateAttributesFromEAVFile(_bstr_t(strInputAttributesFile.c_str()));
				
				// get input file contents
				ISpatialStringPtr ipOriginalInputText(CLSID_SpatialString);
				ASSERT_RESOURCE_ALLOCATION("ELI07321", ipOriginalInputText != __nullptr);
				ipOriginalInputText->LoadFrom(
					_bstr_t(strInputTextFile.c_str()), VARIANT_FALSE);
				
				// create a spatial string to store the contents
				IAFDocumentPtr ipOriginInput(CLSID_AFDocument);
				ASSERT_RESOURCE_ALLOCATION("ELI06302", ipOriginInput != __nullptr);
				ipOriginInput->Text = ipOriginalInputText;

				// pass them through the MERS handler
				m_ipMERSHandler->ModifyEntities(ipFoundAttributes, ipOriginInput);

				// get expected attributes
				IIUnknownVectorPtr ipExpectedAttributes = 
					m_ipAFUtility->GenerateAttributesFromEAVFile(_bstr_t(strEAVFile.c_str()));

				// compare the found attributes with the expected attributes
				bSuccess = compareAttributes(ipFoundAttributes, ipExpectedAttributes);
			}
			CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI19180", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);
			
			// end the test case
			m_ipResultLogger->EndTestCase(asVariantBool(bSuccess && !bExceptionCaught));

			// increment the case number
			nCaseNo++;
		}
		while (!ifs.eof());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06286")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSTester::raw_RunInteractiveTests()
{
	// Do nothing at this time
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		// Check license
		validateLicense();

		// Store results logger
		m_ipResultLogger = pLogger;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06287")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	// Do nothing at this time
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMERSTester::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		// Check license
		validateLicense();

		// If validateLicense doesn't throw any exception, then pbValue is true
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
bool CMERSTester::compareAttributes(IIUnknownVectorPtr ipFoundAttributes, 
									IIUnknownVectorPtr ipExpectedAttributes)
{
	// always add expected attributes as a test case memo no matter
	// the test case succeeds or fails
	string strExpectedAttr = m_ipAFUtility->GetAttributesAsString(ipExpectedAttributes);

	if (ipFoundAttributes->Size() > 0)
	{
		// only add found attributes to the memo if it's not empty
		string strFoundAttr = m_ipAFUtility->GetAttributesAsString(ipFoundAttributes);

		//Add Compare Data to the tree and the dialog.
		m_ipResultLogger->AddTestCaseCompareData("Compare Attributes",
												 _bstr_t("Expected Attributes"), _bstr_t(strExpectedAttr.c_str()),
												 _bstr_t("Found Attributes"), _bstr_t(strFoundAttr.c_str()));
		if (strExpectedAttr == strFoundAttr)
		{
			return true;
		}
	}
	else
	{	
		m_ipResultLogger->AddTestCaseMemo(_bstr_t("Expected Attributes"),
			_bstr_t(strExpectedAttr.c_str()));
		// if there's no found attributes, throw exception
		throw UCLIDException("ELI06299", "No attribute is found.");
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
void CMERSTester::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI07292", "MERS Tester" );
}
//-------------------------------------------------------------------------------------------------
const std::string CMERSTester::getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const
{
	// if pParams is not empty and the second item is specified,
	// then the second item is the master dat file
	if ((ipParams != __nullptr) && (ipParams->Size > 1))
	{
		// get the DAT filename
		string strMasterDatFileName = ::getAbsoluteFileName(strTCLFile, asString(_bstr_t(ipParams->GetItem(1))), true);

		// if no master file specified, throw an exception
		if(strMasterDatFileName.empty() || (getFileNameFromFullPath(strMasterDatFileName) == ""))
		{
			// Create and throw exception
			UCLIDException ue("ELI12292", "Required master testing .DAT file not found!");
			throw ue;
		}

		return strMasterDatFileName;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue("ELI12293", "Required master testing .DAT file not found!");
		throw ue;	
	}
}
//-------------------------------------------------------------------------------------------------
