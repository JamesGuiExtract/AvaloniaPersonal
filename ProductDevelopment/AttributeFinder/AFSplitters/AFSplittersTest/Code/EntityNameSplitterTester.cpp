// EntityNameSplitterTester.cpp : Implementation of CEntityNameSplitterTester
#include "stdafx.h"
#include "AFSplittersTest.h"
#include "EntityNameSplitterTester.h"

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
// CEntityNameSplitterTester
//-------------------------------------------------------------------------------------------------
CEntityNameSplitterTester::CEntityNameSplitterTester()
: m_ipResultLogger(NULL)
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitterTester::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITestableComponent,
		&IID_ILicensedComponent
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitterTester::raw_RunAutomatedTests(IVariantVector* pParams, 
															  BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		// Check license
		validateLicense();

		// ******************************
		// Format of TestNameSplitter.dat
		// ******************************
		//
		// Two line formats are supported:
		// 1) <TESTCASE> | Label | Input
		//    where:
		//    Label  = a number or text string used to identify the test case
		//    Input  = the string used as input to the Name Splitter
		// followed by the expected output presented in the following manner
		//    Names | Input
		//    .Person | Title First Middle Last Suffix (some or all of these items)
		//    ..First | Text
		//    ..Last | Text
		//    ..Middle | Text
		//    ..Suffix | Text
		//    ..Title | Text
		//    .Person | Same stuff but for next person (depends on input)
		//
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
			validateFileOrFolderExistence( strTestFile );
		}
		catch (UCLIDException& ue)
		{
			UCLIDException uexOuter( "ELI06477", 
				"Unable to open Entity Name Splitter master test input file!", ue);
			throw uexOuter;
		}

		// Parse each line of input file
		processFile( strTestFile );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06478")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitterTester::raw_RunInteractiveTests()
{
	// Do nothing at this time
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitterTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		// Check license
		validateLicense();

		// Store results logger
		m_ipResultLogger = pLogger;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06479")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitterTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	// Do nothing at this time
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameSplitterTester::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CEntityNameSplitterTester::addAttributeLevel(IAttributePtr ipNewAttribute, int iLevel)
{
	// Check existence of vector at specified level
	long lSize = m_vecCurrentLevels.size();

	if (lSize <= iLevel)
	{
		// Throw exception, Cannot add Attribute at this level
		UCLIDException	ue( "ELI06480", "Unable to add Attribute." );
		ue.addDebugInfo( "Desired Attribute level", iLevel );
		throw ue;
	}

	// Retrieve vector of Attributes or SubAttributes at specified level
	IIUnknownVectorPtr	ipLevelVector = m_vecCurrentLevels.at( iLevel );

	if (ipLevelVector == __nullptr)
	{
		// Throw exception, Cannot retrieve specified Attribute collection
		UCLIDException	ue( "ELI06481", "Unable to retrieve Attributes." );
		ue.addDebugInfo( "Desired Attribute level", iLevel );
		throw ue;
	}

	// Remove outdated child vectors
	int iNumRemoved = lSize - iLevel - 1;

	for (int i = 0; i < iNumRemoved; i++)
	{
		m_vecCurrentLevels.pop_back();
	}

	// Add new Attribute at specified level
	ipLevelVector->PushBack( ipNewAttribute );

	// Update current levels vector
	m_vecCurrentLevels.push_back( ipNewAttribute->GetSubAttributes() );
}
//-------------------------------------------------------------------------------------------------
string CEntityNameSplitterTester::attributeAsString(IAttributePtr ipAttribute, int iChildLevel)
{
	string strAttribute("");

	// Prepend period for each Child Level
	
	for(int i = 0; i < iChildLevel; i++)
	{
		strAttribute += ".";
	}

	// Include Name
	strAttribute += ipAttribute->Name;
	strAttribute += "|";

	// Add Value
	ISpatialStringPtr ipValue = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI15576", ipValue != __nullptr);
	string strValue = ipValue->String;

	// convert any cpp string (ex. \r, \n, etc. )to normal string
	// (ex. \\r, \\n, etc.) for display purpose
	convertCppStringToNormalString( strValue );
	strAttribute += strValue;

	// Add type if only it's not empty
	string strType = ipAttribute->Type;
	if (!strType.empty())
	{
		strAttribute += "|";
		strAttribute += strType;
	}

	// Retrieve collected sub-attributes
	IIUnknownVectorPtr ipSubs = ipAttribute->SubAttributes;
	ASSERT_RESOURCE_ALLOCATION("ELI15577", ipSubs != __nullptr);

	// Handle each sub-attribute
	long lSubCount = ipSubs->Size();
	for (int i = 0; i < lSubCount; i++)
	{
		// Add line break to separate from parent Attribute
		strAttribute += "\r\n";

		// Retrieve the sub-attribute
		IAttributePtr ipSubAttribute = ipSubs->At( i );
		ASSERT_RESOURCE_ALLOCATION("ELI15578", ipSubs != __nullptr);

		// Add the stringized subattribute - with next child level
		strAttribute += attributeAsString( ipSubAttribute, iChildLevel + 1 );
	}

	return strAttribute;
}
//-------------------------------------------------------------------------------------------------
bool CEntityNameSplitterTester::checkTestResults(IAttributePtr ipTest, IAttributePtr ipExpected)
{
	// Default to expected success
	bool bReturn = true;

	// Add test case memo for Expected Attribute / Test Attribute
	string strExpectedAttr = attributeAsString( ipExpected, 0 );
	string strTestAttr = attributeAsString( ipTest, 0 );

	//Add Compare Results to the tree and the dialog.
	m_ipResultLogger->AddTestCaseCompareData("Compare Results",
											 "Expected Attributes", _bstr_t( strExpectedAttr.c_str() ),
											 "Test Attributes", _bstr_t( strTestAttr.c_str() ));
	// Get IComparableObject pointer from Test and Expected Attributes
	IComparableObjectPtr	ipCompTest = ipTest;
	ASSERT_RESOURCE_ALLOCATION( "ELI06482", ipCompTest != __nullptr );

	IComparableObjectPtr	ipCompExp = ipExpected;
	ASSERT_RESOURCE_ALLOCATION( "ELI06483", ipCompExp != __nullptr );

	// Compare the Attributes
	if (ipCompTest->IsEqualTo( ipCompExp ) == VARIANT_FALSE)
	{
		// Clear flag
		bReturn = false;
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitterTester::doTest(std::string strLabel, IAttributePtr ipTest, 
									   IAttributePtr ipExpected)
{
	// Start the test case
	m_ipResultLogger->StartTestCase( _bstr_t(strLabel.c_str()), _bstr_t("Test Entity Name Splitter"), kAutomatedTestCase );

	bool bExceptionCaught = false;
	try
	{
		ASSERT_ARGUMENT("ELI20477", ipTest != __nullptr);
		ASSERT_ARGUMENT("ELI20478", ipExpected != __nullptr);

		// Call Entity Name Splitter
		try
		{
			try
			{
				m_ipNameSplitter->SplitAttribute( ipTest, NULL, NULL );
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20479");
		}
		catch(UCLIDException& ue)
		{
			UCLIDException uexOuter( "ELI06485", "Failed to split the Attribute.", ue );
			ISpatialStringPtr ipValue = ipTest->Value;
			if (ipValue != __nullptr)
			{
				uexOuter.addDebugInfo( "Input String", asString( ipValue->String ) );
			}
			throw uexOuter;
		}

		// Check the test results and end the test case
		m_ipResultLogger->EndTestCase(asVariantBool(checkTestResults(ipTest, ipExpected)));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI26640", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CEntityNameSplitterTester::getAttribute(std::ifstream &ifs)
{
	// Create the main Attribute
	IAttributePtr	ipAttribute( CLSID_Attribute );
	ASSERT_RESOURCE_ALLOCATION( "ELI06489", ipAttribute != __nullptr );

	// Read each line of the Attribute
	bool	bDone = false;
	while (!ifs.eof() && !bDone)
	{
		// Store the current stream position
		streampos iOriginalPos = ifs.tellg();
		// Use CTFR to retrieve the next non-comment line
		CommentedTextFileReader fileReader( ifs, "//", true );
		string strLine = fileReader.getLineText();

		// Skip blank lines
		if (strLine.empty())
		{
			continue;
		}

		// Check first character
		if (strLine[0] == '<')
		{
			// This is starting the next TEXTCASE or FILE item
			// so reset the seek position to the original
			ifs.seekg( iOriginalPos );

			// Finished processing this Attribute
			bDone = true;
		}
		else
		{
			// Parse the line into name and value, delimited by "|"
			vector<string> vecTokens;
			
			StringTokenizer::sGetTokens( strLine, '|', vecTokens);
			
			long lCount = vecTokens.size();

			if ((lCount < 2) || (lCount > 3))
			{
				UCLIDException ue( "ELI06490", "Invalid attribute info on the line!" );
				ue.addDebugInfo( "Line Text", strLine );
				throw ue;
			}

			// Store Name and Value strings
			string strName( vecTokens[0] );
			string strValue( vecTokens[1] );
			string strType("");

			if (lCount == 3)
			{
				strType = vecTokens[2];
			}

			// Get desired Attribute level
			int iLevel = getAttributeLevel( strName );

			// Trim the prefix characters from the text
			if (iLevel > 0)
			{
				strName = strName.substr( iLevel, strName.length() - iLevel );
			}

			// Replace special chars with cpp format, 
			// i.e. new line chars into \n
			::convertNormalStringToCppString( strValue );

			// Create Sub-attribute, if necessary
			if (iLevel > 0)
			{
				// Create a new Attribute object for this Name/Value pair
				IAttributePtr	ipNewAttribute( CLSID_Attribute );
				if (ipNewAttribute == __nullptr)
				{
					// Throw exception
					UCLIDException ue( "ELI06491", "Unable to create new Attribute object!" );
					throw ue;
				}

				// Add Name, Value, Type information
				ipNewAttribute->Name = _bstr_t( strName.c_str() );
				ISpatialStringPtr ipValue = ipNewAttribute->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI25944", ipValue != __nullptr);
				ipValue->CreateNonSpatialString(strValue.c_str(), "");
				ipNewAttribute->Type = _bstr_t( strType.c_str() );

				// Add this Attribute to the appropriate collection of (Sub)Attributes
				addAttributeLevel( ipNewAttribute, iLevel );
			}
			else
			{
				// Add Name, Value, Type information to Main Attribute
				ipAttribute->Name = _bstr_t( strName.c_str() );
				ISpatialStringPtr ipValue = ipAttribute->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI25945", ipValue != __nullptr);
				ipValue->ReplaceAndDowngradeToNonSpatial(strValue.c_str());
				ipAttribute->Type = _bstr_t( strType.c_str() );

				// Clear the previous collection of Attribute nodes
				m_vecCurrentLevels.clear();

				// Level zero collection is the Main Attribute
				m_vecCurrentLevels.push_back( ipAttribute );

				// Level one collection is the collection of sub-attributes
				// of the Main attribute
				m_vecCurrentLevels.push_back( ipAttribute->GetSubAttributes() );
			}
		}
	}

	// Return the Attribute object
	return ipAttribute;
}
//-------------------------------------------------------------------------------------------------
int CEntityNameSplitterTester::getAttributeLevel(std::string strName)
{
	// Check string length
	long lSize = m_vecCurrentLevels.size();

	if (strName.length() == 0)
	{
		// Throw exception, Empty Name string
		UCLIDException	ue( "ELI06492", "Attribute name must not be blank!" );
		throw ue;
	}

	// Find first non-period character in Name
	int iCount = strName.find_first_not_of( '.', 0 );

	// Make sure a character was found
	if (iCount == string::npos)
	{
		// Throw exception, Invalid Name string
		UCLIDException	ue( "ELI06493", "Invalid Attribute name!" );
		ue.addDebugInfo( "Attribute name", strName );
		throw ue;
	}

	// Return count = number of leading period characters
	return iCount;
}
//-------------------------------------------------------------------------------------------------
const std::string CEntityNameSplitterTester::getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const
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
			UCLIDException ue("ELI12286", "Required master testing .DAT file not found!");
			throw ue;
		}

		return strMasterDatFileName;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue("ELI12287", "Required master testing .DAT file not found!");
		throw ue;	
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitterTester::prepareTests()
{
	// Create a Name Splitter for testing
	m_ipNameSplitter.CreateInstance( CLSID_EntityNameSplitter );
	ASSERT_RESOURCE_ALLOCATION( "ELI06494", m_ipNameSplitter != __nullptr );
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitterTester::processFile(std::string strFile)
{
	// Read each line of the input file
	ifstream ifs( strFile.c_str() );
	CommentedTextFileReader fileReader( ifs, "//", true );
	string strLine;

	do
	{
		// Retrieve this line from the input file
		strLine = fileReader.getLineText();
		if (strLine.empty()) 
		{
			continue;
		}

		// Process each line
		processLine( strLine, strFile, ifs );
	}
	while (!ifs.eof());
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitterTester::processLine(std::string strLine, const std::string& strFileWithLine, 
											std::ifstream &ifs)
{
	// Convert string from Normal to CPP format
	::convertNormalStringToCppString( strLine );

	// Tokenize the line
	vector<string> vecTokens;
	StringTokenizer::sGetTokens( strLine, '|', vecTokens );

	// Line type keywords
	string strFileKeyword( "<FILE>" );
	string strCaseKeyword( "<TESTCASE>" );

	unsigned int iNumOfTokens = vecTokens.size();

	// Retrieve the tokens
	string strKey("");
	string strFileOrLabel("");
	string strText("");

	if (iNumOfTokens > 0)
	{
		strKey = vecTokens[0];
	}

	if (iNumOfTokens > 1)
	{
		// Either a filename or a test label
		strFileOrLabel = vecTokens[1];
	}

	if (iNumOfTokens > 2)
	{
		// Input text - to be split
		strText = vecTokens[2];
	}

	// Validate the tokens
	bool bValid = false;

	if (!strKey.empty() && !strFileOrLabel.empty())
	{
		// Strings non-empty, check for keyword 
		// and valid filename
		if (strKey == strFileKeyword)
		{
			// Convert the filename to an absolute filename, relative 
			// to the file from which this line was read
			strFileOrLabel = getAbsoluteFileName( strFileWithLine, strFileOrLabel );

			try
			{
				validateFileOrFolderExistence( strFileOrLabel );
			}
			catch (UCLIDException& ue)
			{
				UCLIDException uexOuter( "ELI07339", "Reference to invalid file name encountered!", ue );
				uexOuter.addDebugInfo( "strFileOrLabel", strFileOrLabel );
				uexOuter.addDebugInfo( "strLine", strLine );
				throw uexOuter;
			}

			// Process the file
			processFile( strFileOrLabel );
		}
		else if (strKey == strCaseKeyword)
		{
			// Must have non-empty input string
			if (!strText.empty())
			{
				// Retrieve the Attribute that follows the input line
				IAttributePtr	ipExpected = getAttribute( ifs );

				// Check for valid Attribute
				if (ipExpected == __nullptr)
				{
					// Create and throw exception
					UCLIDException ue( "ELI06495", "Unable to retrieve expected Attribute." );
					ue.addDebugInfo( "Test Label", strFileOrLabel.c_str() );
					ue.addDebugInfo( "Input Text", strText.c_str() );
					throw ue;
				}

				// Create the Attribute to be split
				IAttributePtr	ipTest( CLSID_Attribute );
				ASSERT_RESOURCE_ALLOCATION( "ELI06496", ipTest != __nullptr );

				// Provide Name and Value for Attribute
				ipTest->Name = "Names";
				ISpatialStringPtr ipValue = ipTest->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI25946", ipValue != __nullptr);
				ipValue->ReplaceAndDowngradeToNonSpatial(strText.c_str());

				// Execute the test
				doTest( strFileOrLabel, ipTest, ipExpected );

				// Done processing this line, just return
				return;
			}
		}
		// First token is not a keyword, this is an error
		else
		{
			// Create and throw exception
			UCLIDException ue( "ELI06497", "Unable to process line." );
			ue.addDebugInfo( "Line Text", strLine );
			throw ue;
		}
	}
	else
	{
		// Create and throw exception
		UCLIDException ue( "ELI06498", "Unable to process line." );
		ue.addDebugInfo( "Line Text", strLine );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityNameSplitterTester::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI07329", "Entity Name Splitter Tester" );
}
//-------------------------------------------------------------------------------------------------
