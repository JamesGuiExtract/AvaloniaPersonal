// AddressSplitterTester.cpp : Implementation of CAddressSplitterTester

#include "stdafx.h"
#include "AFSplittersTest.h"
#include "AddressSplitterTester.h"

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

//-------------------------------------------------------------------------------------------------
// CAddressSplitterTester
//-------------------------------------------------------------------------------------------------
CAddressSplitterTester::CAddressSplitterTester()
: m_ipResultLogger(NULL),
  m_ipAddressSplitter(NULL)
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitterTester::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITestableComponent,
		&IID_ILicensedComponent
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
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
STDMETHODIMP CAddressSplitterTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		// Check license
		validateLicense();

		// *********************************
		// Format of TestAddressSplitter.dat
		// *********************************
		//
		// Two line formats are supported:
		// 1) <TESTCASE> | Label | Input
		//    where:
		//    Label  = a number or text string used to identify the test case
		//    Input  = the string used as input to the Name Splitter
		// followed by the expected output presented in the following manner
		//    Address | Input
		//    .Recipient1 | 1st Recipient line from Input
		//    .Recipient2 | 2nd Recipient line from Input, if present
		//    .Recipient3 | 3rd Recipient line from Input, if present
		//    .Address1 | 1st Address line from Input
		//    .Address2 | 2nd Address line from Input, if present
		//    .Address3 | 3rd Address line from Input, if present
		//    .City | City from Input
		//    .State | State from Input
		//    .Zip Code | Zip Code from Input
		//
		// 2) <FILE> | Path
		//    where:
		//    <FILE> = a keyword indicating that the next argument is a filename
		//    Path   = a fully qualified path to a text file used as input.
		//             NOTE: The text file must also satisfy these format requirements
		//
		// 3) <SETTING>|REPLACE_RECIPIENTS_WITH_ADDRESS_LINES=TRUE
		//    This will enable the conversion of recipient lines into address lines.
		//

		///////////////////////
		// Prepare test info
		// for default settings
		///////////////////////
		prepareTests();

		// Find and validate the test file
		string strTestFile = getMasterTestFileName( pParams, asString( strTCLFile ) );
		try
		{
			validateFileOrFolderExistence( strTestFile );
		}
		catch (UCLIDException& ue)
		{
			UCLIDException uexOuter( "ELI07224", 
				"Unable to open Address Splitter master test input file!", ue);
			uexOuter.addDebugInfo("Input filename", strTestFile);
			throw uexOuter;
		}

		// Parse each line of input file
		processFile( strTestFile );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07225")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitterTester::raw_RunInteractiveTests()
{
	// Do nothing at this time
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitterTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		// Check license
		validateLicense();

		// Store results logger
		m_ipResultLogger = pLogger;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07223")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitterTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	// Do nothing at this time
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAddressSplitterTester::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CAddressSplitterTester::addAttributeLevel(IAttributePtr ipNewAttribute, int iLevel)
{
	// Check existence of vector at specified level
	long lSize = m_vecCurrentLevels.size();
	if (lSize <= iLevel)
	{
		// Throw exception, Cannot add Attribute at this level
		UCLIDException	ue( "ELI07242", "Unable to add Attribute." );
		ue.addDebugInfo( "Desired Attribute level", iLevel );
		throw ue;
	}

	// Retrieve vector of Attributes or SubAttributes at specified level
	IIUnknownVectorPtr	ipLevelVector = m_vecCurrentLevels.at( iLevel );
	if (ipLevelVector == __nullptr)
	{
		// Throw exception, Cannot retrieve specified Attribute collection
		UCLIDException	ue( "ELI07243", "Unable to retrieve Attributes." );
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
string CAddressSplitterTester::attributeAsString(IAttributePtr ipAttribute, int iChildLevel)
{
	string strAttribute;

	// Prepend period for each Child Level
	int i;
	for (i = 0; i < iChildLevel; i++)
	{
		strAttribute += ".";
	}

	// Include Name
	strAttribute += ipAttribute->Name;
	strAttribute += "|";

	// Retrieve Value
	ISpatialStringPtr ipValue = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION( "ELI15573", ipValue != __nullptr );
	string strValue = ipValue->String;

	// Convert any cpp string (ex. \r, \n, etc. )to normal string
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
	ASSERT_RESOURCE_ALLOCATION("ELI15574", ipSubs != __nullptr);

	// Handle each sub-attribute
	long lSubCount = ipSubs->Size();
	for (i = 0; i < lSubCount; i++)
	{
		// Add line break to separate from parent Attribute
		strAttribute += "\r\n";

		// Retrieve the sub-attribute
		IAttributePtr ipSubAttribute = ipSubs->At( i );
		ASSERT_RESOURCE_ALLOCATION("ELI15575", ipSubAttribute != __nullptr);

		// Add the stringized subattribute - with next child level
		strAttribute += attributeAsString( ipSubAttribute, iChildLevel + 1 );
	}

	return strAttribute;
}
//-------------------------------------------------------------------------------------------------
bool CAddressSplitterTester::checkTestResults(IAttributePtr ipTest, IAttributePtr ipExpected)
{
	// Default to expected success
	bool bReturn = true;

	// Add test case memo for Expected and Test Attributes
	string strTestAttr = attributeAsString( ipTest, 0 );
	string strExpectedAttr = attributeAsString( ipExpected, 0 );

	//Add Compare Data to the tree and the output dialog.
	m_ipResultLogger->AddTestCaseCompareData("Compare Attributes", 
											 _bstr_t( "Expected Attributes" ), _bstr_t( strExpectedAttr.c_str() ),
											 _bstr_t( "Test Attributes" ), _bstr_t( strTestAttr.c_str() ) );

	// Get IComparableObject pointer from Test and Expected Attributes
	IComparableObjectPtr	ipCompTest = ipTest;
	ASSERT_RESOURCE_ALLOCATION( "ELI07240", ipCompTest != __nullptr );

	IComparableObjectPtr	ipCompExp = ipExpected;
	ASSERT_RESOURCE_ALLOCATION( "ELI07241", ipCompExp != __nullptr );

	// Compare the Attributes
	if (ipCompTest->IsEqualTo( ipCompExp ) == VARIANT_FALSE)
	{
		// Clear flag
		bReturn = false;
	}

	return bReturn;
}
//-------------------------------------------------------------------------------------------------
void CAddressSplitterTester::doTest(std::string strLabel, IAttributePtr ipTest, 
									IAttributePtr ipExpected)
{
	// Start the test case
	m_ipResultLogger->StartTestCase( _bstr_t(strLabel.c_str()), _bstr_t("Test Address Splitter"), 
		kAutomatedTestCase );

	bool bExceptionCaught = false;
	try
	{
		ASSERT_ARGUMENT("ELI20475", ipTest != __nullptr);
		ASSERT_ARGUMENT("ELI20476", ipExpected != __nullptr);

		// Call Address Splitter
		try
		{
			try
			{
				m_ipAddressSplitter->SplitAttribute( ipTest, NULL, NULL );
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20474");
		}
		catch(UCLIDException& ue)
		{
			UCLIDException uexOuter("ELI07233", "Failed to split the Attribute.", ue);
			ISpatialStringPtr ipValue = ipTest->Value;
			if (ipValue != __nullptr)
			{
				uexOuter.addDebugInfo( "Input String", asString( ipValue->String ) );
			}
			throw uexOuter;
		}

		// Check the results and end the test case
		m_ipResultLogger->EndTestCase(asVariantBool(checkTestResults(ipTest, ipExpected)));
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI26639", m_ipResultLogger, bExceptionCaught, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CAddressSplitterTester::getAttribute(std::ifstream &ifs)
{
	// Create the main Attribute
	IAttributePtr	ipAttribute( CLSID_Attribute );
	ASSERT_RESOURCE_ALLOCATION( "ELI07237", ipAttribute != __nullptr );

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
				UCLIDException ue( "ELI07238", "Invalid attribute info on the line!" );
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
					UCLIDException ue( "ELI07239", "Unable to create new Attribute object!" );
					throw ue;
				}

				// Add Name, Value, Type information
				ipNewAttribute->Name = _bstr_t( strName.c_str() );
				ISpatialStringPtr ipValue = ipNewAttribute->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI25937", ipValue != __nullptr);
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
				ASSERT_RESOURCE_ALLOCATION("ELI25942", ipValue != __nullptr);
				ipValue->CreateNonSpatialString(strValue.c_str(), "");
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
int CAddressSplitterTester::getAttributeLevel(std::string strName)
{
	// Check string length
	long lSize = m_vecCurrentLevels.size();
	if (strName.length() == 0)
	{
		// Throw exception, Empty Name string
		UCLIDException	ue( "ELI07244", "Attribute name must not be blank!" );
		throw ue;
	}

	// Find first non-period character in Name
	int iCount = strName.find_first_not_of( '.', 0 );

	// Make sure a character was found
	if (iCount == string::npos)
	{
		// Throw exception, Invalid Name string
		UCLIDException	ue( "ELI07245", "Invalid Attribute name!" );
		ue.addDebugInfo( "Attribute name", strName );
		throw ue;
	}

	// Return count = number of leading period characters
	return iCount;
}
//-------------------------------------------------------------------------------------------------
const std::string CAddressSplitterTester::getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const
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
			UCLIDException ue("ELI19154", "Required master testing .DAT file not found!");
			throw ue;
		}

		return strMasterDatFileName;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue("ELI19155", "Required master testing .DAT file not found!");
		throw ue;	
	}
}
//-------------------------------------------------------------------------------------------------
void CAddressSplitterTester::prepareTests()
{
	// Create a Name Splitter for testing
	if (m_ipAddressSplitter == __nullptr)
	{
		m_ipAddressSplitter.CreateInstance( CLSID_AddressSplitter );
		ASSERT_RESOURCE_ALLOCATION( "ELI07227", m_ipAddressSplitter != __nullptr );
	}
}
//-------------------------------------------------------------------------------------------------
void CAddressSplitterTester::processFile(std::string strFile)
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
void CAddressSplitterTester::processLine(std::string strLine, const std::string& strFileWithLine, 
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
	string strSettingKeyword( "<SETTING>" );

	string strKey("");
	string strFileOrLabel("");
	string strText("");

	unsigned int iNumOfTokens = vecTokens.size();

	// Retrieve the tokens
	if(iNumOfTokens > 0)
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
				UCLIDException uexOuter( "ELI07340", "Reference to invalid file name encountered!", ue );
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
					UCLIDException ue( "ELI07228", "Unable to retrieve expected Attribute." );
					ue.addDebugInfo( "Test Label", strFileOrLabel.c_str() );
					ue.addDebugInfo( "Input Text", strText.c_str() );
					throw ue;
				}

				// Create the Attribute to be split
				IAttributePtr	ipTest( CLSID_Attribute );
				ASSERT_RESOURCE_ALLOCATION( "ELI07229", ipTest != __nullptr );

				// Provide Name and Value for Attribute
				ipTest->Name = "Address";
				ISpatialStringPtr ipValue = ipTest->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI25943", ipValue != __nullptr);
				ipValue->ReplaceAndDowngradeToNonSpatial(strText.c_str());

				// Execute the test
				doTest( strFileOrLabel, ipTest, ipExpected );

				// Done processing this line, just return
				return;
			}
		}
		else if ( strKey == strSettingKeyword )
		{
			std::vector<std::string> vecSettings;

			StringTokenizer::sGetTokens(strFileOrLabel, "=", vecSettings);

			// confirm that a valid setting was found
			if (vecSettings.size() == 2)
			{
				// confirm the setting name
				if ( vecSettings[0] == "REPLACE_RECIPIENTS_WITH_ADDRESS_LINES")
				{
					bool bCombinedAddress = false;

					if (vecSettings[1] == "TRUE")
					{
						bCombinedAddress = true;
					}
					else if (vecSettings[1] == "FALSE")
					{
						bCombinedAddress = false;
					}
					else
					{
						UCLIDException ue("ELI13032", "Invalid setting value!  Please specify either \"TRUE\" or \"FALSE\".");
						ue.addDebugInfo("Value", vecSettings[1]);
						throw ue;
					}

					// Get pointer to IAddressSplitter object
					IAddressSplitterPtr	ipSplitter = m_ipAddressSplitter;
					ASSERT_RESOURCE_ALLOCATION( "ELI08501", ipSplitter != __nullptr );

					// Apply the setting
					ipSplitter->CombinedNameAddress = bCombinedAddress ? VARIANT_TRUE : VARIANT_FALSE;
				}
				else
				{
					UCLIDException ue("ELI13033", "Unknown <SETTING> name!");
					ue.addDebugInfo("Name", vecSettings[0]);
					throw ue;
				}
			}
			else
			{
				UCLIDException ue("ELI13034", "Invalid <SETTING> syntax!");
				ue.addDebugInfo("Setting", strFileOrLabel);
				throw ue;
			}
		}
		// First token is not a keyword, this is an error
		else
		{
			// Create and throw exception
			UCLIDException ue( "ELI07230", "Unable to process line." );
			ue.addDebugInfo( "Line Text", strLine );
			throw ue;
		}
	}
	else
	{
		// Create and throw exception
		UCLIDException ue( "ELI07231", "Unable to process line." );
		ue.addDebugInfo( "Line Text", strLine );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CAddressSplitterTester::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI07327", "Address Splitter Tester" );
}
//-------------------------------------------------------------------------------------------------
