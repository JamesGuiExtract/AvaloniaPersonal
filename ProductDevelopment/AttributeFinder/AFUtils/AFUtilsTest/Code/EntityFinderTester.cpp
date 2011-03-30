// EntityFinderTester.cpp : Implementation of CEntityFinderTester
#include "stdafx.h"
#include "AFUtilsTest.h"
#include "EntityFinderTester.h"

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
// CEntityFinderTester
//-------------------------------------------------------------------------------------------------
CEntityFinderTester::CEntityFinderTester()
: m_ipResultLogger(NULL)
{
	try
	{
		// Create the Entity Finder object
		m_ipEntityFinder.CreateInstance( CLSID_EntityFinder );
		ASSERT_RESOURCE_ALLOCATION( "ELI06172", m_ipEntityFinder != __nullptr );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06179")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinderTester::InterfaceSupportsErrorInfo(REFIID riid)
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
STDMETHODIMP CEntityFinderTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		// Check license
		validateLicense();

		// ******************************
		// Format of TestEntityFinder.dat
		// ******************************
		//
		// Two line formats are supported:
		// 1) Label | Input | Output
		//    where:
		//    Label  = a number or text string used to identify the test case
		//    Input  = the string used as input to the Entity Finder
		//    Output = the string expected as output from the Entity Finder
		// 2) <FILE> | Path
		//    where:
		//    <FILE> = a keyword indicating that the next argument is a filename
		//    Path   = a fully qualified path to a text file used as input.
		//             NOTE: The text file must also satisfy these format requirements

		////////////////////
		// Prepare test info
		////////////////////
		prepareTests();

		// Find and validate the test file
		string strTestFile = getMasterTestFileName( pParams, asString( strTCLFile ) );
		try
		{
			validateFileOrFolderExistence(strTestFile);
		}
		catch (UCLIDException& ue)
		{
			UCLIDException uexOuter("ELI07318", 
				"Unable to open Entity Finder master test input file!", ue);
			throw uexOuter;
		}

		// Parse each line of input file
		processFile( strTestFile );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06157")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinderTester::raw_RunInteractiveTests()
{
	// Do nothing at this time
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinderTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		// Check license
		validateLicense();

		// Store results logger
		m_ipResultLogger = pLogger;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06158")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinderTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	// Do nothing at this time
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityFinderTester::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Private methods
//-------------------------------------------------------------------------------------------------
bool CEntityFinderTester::checkTestResults()
{
	// Default to expected success
	bool bReturn = true;

	// Get IComparableObject pointer from Test string
	IComparableObjectPtr	ipCompTest = m_ipTestString;

	// Get IComparableObject pointer from Expected string
	IComparableObjectPtr	ipCompExp = m_ipExpectedString;

	// Compare the strings
	if (ipCompTest->IsEqualTo( ipCompExp ) == VARIANT_FALSE)
	{
		// Clear flag
		bReturn = false;
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
void CEntityFinderTester::doTest(std::string strLabel)
{
	VARIANT_BOOL	vbRet = VARIANT_TRUE;

	// Start the test case
	m_ipResultLogger->StartTestCase( _bstr_t(strLabel.c_str()), _bstr_t("Test Entity Finder"), 
		kAutomatedTestCase );

	try
	{
		// Call Entity Finder
		HRESULT hr = m_ipEntityFinder->FindEntities( m_ipTestString );
		if (FAILED(hr))
		{
			UCLIDException uclidException( "ELI06165", "Failed to find an Entity." );
			uclidException.addDebugInfo( "HRESULT", hr );
			uclidException.addDebugInfo( "Input String", string( m_ipTestString->String ) );
			uclidException.addDebugInfo( "Expected String", string( m_ipExpectedString->String ) );
			throw uclidException;
		}

		// Check the results
		if (!checkTestResults())
		{
			CString	zTemp;

			// Add Note with Expected Text
			zTemp.Format( "EXPECTED TEXT: \"%s\"", string( m_ipExpectedString->String ).c_str() );
			m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));

			// Add Note with Actual Text
			zTemp.Format( "ACTUAL TEXT: \"%s\"", string( m_ipTestString->String ).c_str() );
			m_ipResultLogger->AddTestCaseNote(_bstr_t(zTemp));

			// Set failure flag
			vbRet = VARIANT_FALSE;
		}
	}
	// Handle a UCLID Exception
	catch (UCLIDException& ue)
	{
		string strError( ue.asStringizedByteStream() );
		m_ipResultLogger->AddTestCaseException( _bstr_t(strError.c_str()), VARIANT_FALSE );
		vbRet = VARIANT_FALSE;
	}
	// Handle a COM Error
	catch (_com_error& e)
	{
		UCLIDException ue;
		_bstr_t _bstrDescription = e.Description();
		char *pszDescription = _bstrDescription;
		if (pszDescription)
		{
			ue.createFromString( "ELI06167", pszDescription );
		}
		else
		{
			ue.createFromString( "ELI06168", "COM exception caught!" );
		}
		string strError( ue.asStringizedByteStream() );
		m_ipResultLogger->AddTestCaseException( _bstr_t(strError.c_str()), VARIANT_FALSE );
		vbRet = VARIANT_FALSE;
	}
	// Handle some other type of exception
	catch (...)
	{
		UCLIDException uclidException( "ELI06169", "Unknown Exception caught." );
		string strError( uclidException.asStringizedByteStream() );
		m_ipResultLogger->AddTestCaseException( _bstr_t(strError.c_str()), VARIANT_FALSE );
		vbRet = VARIANT_FALSE;
	}

	// End the test case
	m_ipResultLogger->EndTestCase( vbRet );
}
//-------------------------------------------------------------------------------------------------
void CEntityFinderTester::prepareTests()
{
	// Create a string for testing
	m_ipTestString.CreateInstance( CLSID_SpatialString );
	ASSERT_RESOURCE_ALLOCATION( "ELI06160", m_ipTestString != __nullptr );

	// Create a string for comparison
	m_ipExpectedString.CreateInstance( CLSID_SpatialString );
	ASSERT_RESOURCE_ALLOCATION( "ELI06164", m_ipExpectedString != __nullptr );
}
//-------------------------------------------------------------------------------------------------
void CEntityFinderTester::processFile(const std::string& strFile)
{
	// Read each line of the input file
	ifstream ifs( strFile.c_str() );
	CommentedTextFileReader fileReader( ifs, "//", true );
	string strLine("");

	do
	{
		// Retrieve this line from the input file
		strLine = fileReader.getLineText();
		if (strLine.empty()) 
		{
			continue;
		}

		// Process each line
		processLine( strLine, strFile );
	}
	while (!ifs.eof());
}
//-------------------------------------------------------------------------------------------------
void CEntityFinderTester::processLine(std::string strLine, 
									  const std::string& strFileWithLine)
{
	// Convert string from Normal to CPP format
	::convertNormalStringToCppString( strLine );

	// Tokenize the line
	vector<string> vecTokens;
	StringTokenizer::sGetTokens( strLine, '|', vecTokens );

	// Check for nested file of input data
	const string strKeyword( "<FILE>" );
	if (vecTokens.size() == 2)
	{
		// Retrieve the tokens
		string strKey( vecTokens[0] );
		string strFile( vecTokens[1] );

		// Validate the tokens
		if (!strKey.empty() && !strFile.empty())
		{
			// Strings non-empty, check for keyword 
			// and valid filename
			if (strKey == strKeyword)
			{
				// the filename mentioned may be a relative filename
				// convert it to an absolute filename, relative to the
				// file from which this line was read
				strFile = getAbsoluteFileName(strFileWithLine, strFile);

				try
				{
					validateFileOrFolderExistence(strFile);
				}
				catch (UCLIDException& ue)
				{
					UCLIDException uexOuter("ELI07319", "Reference to invalid file name encountered!", ue);
					uexOuter.addDebugInfo("strFileWithLine", strFileWithLine);
					uexOuter.addDebugInfo("strLine", strLine);
					throw uexOuter;
				}

				// Process the file
				processFile( strFile );
			}
			// First token is not a keyword, process as Input and Output without Label
			else
			{
				// Store input and output in data members
				m_ipTestString->ReplaceAndDowngradeToNonSpatial(strKey.c_str());
				m_ipExpectedString->ReplaceAndDowngradeToNonSpatial(strFile.c_str());

				// Do the test and check the results
				doTest( "No Label" );

				// Done processing this line, just return
				return;
			}
		}
		else
		{
			UCLIDException ue( "ELI06181", "Invalid input line: at least one of the tokens in line is empty!" );
			ue.addDebugInfo( "Line Text", strLine );
			throw ue;
		}
	}
	// Check for input data
	else if (vecTokens.size() == 3)
	{
		// Retrieve the tokens
		string strLabel( vecTokens[0] );
		string strInput( vecTokens[1] );
		string strOutput( vecTokens[2] );

		// Validate the tokens
		if (strLabel.empty() || strInput.empty() || strOutput.empty())
		{
			UCLIDException ue( "ELI06182", "Invalid input line: at least one of the tokens in line is empty!" );
			ue.addDebugInfo( "Line Text", strLine );
			throw ue;
		}
		else
		{
			// Store input and output in data members
			m_ipTestString->ReplaceAndDowngradeToNonSpatial(strInput.c_str());
			m_ipExpectedString->ReplaceAndDowngradeToNonSpatial(strOutput.c_str());

			// Do the test and check the results
			doTest( strLabel );
		}
	}
	else
	{
		UCLIDException ue( "ELI06180", "Invalid number of tokens on input line." );
		ue.addDebugInfo( "Token count", vecTokens.size() );
		ue.addDebugInfo( "Line Text", strLine );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityFinderTester::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI07289", "Entity Finder Tester" );
}
//-------------------------------------------------------------------------------------------------
const std::string CEntityFinderTester::getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const
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
			UCLIDException ue("ELI12290", "Required master testing .DAT file not found!");
			throw ue;
		}

		return strMasterDatFileName;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue("ELI12291", "Required master testing .DAT file not found!");
		throw ue;	
	}
}
//-------------------------------------------------------------------------------------------------
