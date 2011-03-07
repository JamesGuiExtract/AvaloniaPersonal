// StringCSISTester.cpp : Implementation of CStringCSISTester

#include "stdafx.h"
#include "StringCSISTester.h"

#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <StringCSIS.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>

using namespace std;
//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrBASIC_STRING_SPECIFIER = "BS";
const string gstrCHAR_POINTER_SPECIFIER = "CP";
const string gstrCHAR_POINTER_WITH_COUNT_SPECIFIER = "CPC";
const string gstrCHAR_SPECIFIER = "C";

//-------------------------------------------------------------------------------------------------
// CStringCSISTester
//-------------------------------------------------------------------------------------------------
CStringCSISTester::CStringCSISTester()
{
}
//-------------------------------------------------------------------------------------------------
HRESULT CStringCSISTester::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CStringCSISTester::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringCSISTester::InterfaceSupportsErrorInfo(REFIID riid)
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
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringCSISTester::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringCSISTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		// Check license
		validateLicense();

		// *********************************
		// NOTE:	See the StringCSISTest.syntax.txt file for the syntax expected to
		//			be used for the test input file.
		// *********************************

		// Find and validate the test file
		string strTestFile = getMasterTestFileName( pParams, asString( strTCLFile ) );
		try
		{
			validateFileOrFolderExistence( strTestFile );
		}
		catch (UCLIDException& ue)
		{
			UCLIDException uexOuter( "ELI15939", 
				"Unable to open stringCSIS master test input file!", ue);
			uexOuter.addDebugInfo("Input filename", strTestFile);
			throw uexOuter;
		}

		// Parse each line of input file
		processFile( strTestFile );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15938")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringCSISTester::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringCSISTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		// Check license
		validateLicense();

		// Store results logger
		m_ipResultLogger = pLogger;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15936")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringCSISTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CStringCSISTester::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI15937", "StringCSIS Tester" );
}
//-------------------------------------------------------------------------------------------------
const std::string CStringCSISTester::getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const
{
	// if pParams is not empty and the second item is specified,
	// then the second item is the master dat file
	if ((ipParams != NULL) && (ipParams->Size > 1))
	{
		// get the DAT filename
		string strMasterDatFileName = ::getAbsoluteFileName(strTCLFile, asString(_bstr_t(ipParams->GetItem(1))), false);

		// if no master file specified, throw an exception
		if(strMasterDatFileName.empty() || (getFileNameFromFullPath(strMasterDatFileName) == ""))
		{
			// Create and throw exception
			UCLIDException ue("ELI15940", "Required master testing .DAT file not found!");
			throw ue;
		}

		return strMasterDatFileName;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue("ELI15941", "Required master testing .DAT file not found!");
		ue.addDebugInfo("TCL File", strTCLFile);
		throw ue;	
	}
}
//-------------------------------------------------------------------------------------------------
void CStringCSISTester::processFile(const std::string& strFile)
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

		try
		{
			try
			{
				// Process each line
				processLine( strLine, strFile, ifs );
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16017");
		}
		catch ( UCLIDException& ue )
		{
			// add history record
			UCLIDException uexOuter("ELI16018", "Unable to process line in test input file!", ue);

			// Add debug info to give the line of text that caused exceptions
			uexOuter.addDebugInfo("Line Text", strLine);

			// Add a componentTest exception
			m_ipResultLogger->AddComponentTestException(get_bstr_t(uexOuter.asStringizedByteStream()));
		}
	}
	while (!ifs.eof());
}
//-------------------------------------------------------------------------------------------------
void CStringCSISTester::processFileKeyword(const string& strTCLFile,
										   const string& strLine, vector<string>& rvecTokens)
{
	try
	{
		string strFileToCall;

		// Second token should be a filename
		size_t lNumOfTokens = rvecTokens.size();
		if ( lNumOfTokens == 2 )
		{
			strFileToCall = rvecTokens[1];
		}
		else
		{
			UCLIDException ue("ELI16020", "Name of referenced file not defined!");
			ue.addDebugInfo("Number of Tokens", (unsigned int) lNumOfTokens );
			throw ue;
		}

		// Convert the filename to an absolute filename, relative 
		// to the file from which this line was read
		strFileToCall = getAbsoluteFileName( strTCLFile, strFileToCall );

		// if the file does not exist throw an exception
		try
		{
			validateFileOrFolderExistence( strFileToCall );
		}
		catch (UCLIDException& ue)
		{
			UCLIDException uexOuter( "ELI15942", "Reference to invalid file name encountered!", ue );
			uexOuter.addDebugInfo( "FileName", strFileToCall );
			throw uexOuter;
		}

		// Process the file
		processFile( strFileToCall );
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI16021", "Unable to process file keyword line in test input file!", ue);
		uexOuter.addDebugInfo("Tescase", strLine);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
void CStringCSISTester::processTestCase(const string& strLine, vector<string>& rvecTokens)
{
	try
	{
		string strFunction;

		// Second token should be the function or operator to test
		size_t lNumOfTokens = rvecTokens.size();
		if (lNumOfTokens > 1)
		{
			strFunction = rvecTokens[1];
		}
		else
		{
			UCLIDException ue("ELI16019", "Test function / operator not defined!");
			ue.addDebugInfo("Number of Tokens", (unsigned int) lNumOfTokens );
			throw ue;
		}

		// There must be a min of 7 tokens if test case is valid
		if ( lNumOfTokens < 7 ) 
		{
			UCLIDException ue("ELI15945", "TESTCASE must have a min of 7 tokens. ");
			ue.addDebugInfo("Number of Tokens", (unsigned int) lNumOfTokens );
			throw ue;
		}

		// Get the strings
		stringCSIS strString1(rvecTokens[2], asCppBool(rvecTokens[3]));
		stringCSIS strString2(rvecTokens[4], asCppBool(rvecTokens[5]));

		vector<string> vecValues;
		long lCount = 0;
		string strFindType = "";
		unsigned long lStartOfValues = 6;

		// if the strFunction string does not have "!=<>" it is a "find" function
		// which has more tokens to process
		if ( !isOperator(strFunction) )
		{
			// Get the find type ("BS, C, CP, CPC)
			strFindType = rvecTokens[6];
			makeUpperCase(strFindType);
			lStartOfValues++;

			// If the find type is CPC need to get the count of chars to compare
			if ( strFindType == gstrCHAR_POINTER_WITH_COUNT_SPECIFIER )
			{
				// need to get the count of the number of characters to compare
				// with the find
				lCount = asLong ( rvecTokens[lStartOfValues]);
				lStartOfValues++;
			}
		}

		// get the expected values and place in a vector of strings
		for ( unsigned long i = lStartOfValues; i < lNumOfTokens; i++ )
		{
			vecValues.push_back ( rvecTokens[i] );
		}

		// do the test
		doTest( strFunction, strString1, strString2, strFindType, lCount, vecValues);
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI16022", "Unable to process test case!", ue);
		uexOuter.addDebugInfo("Tescase", strLine);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
void CStringCSISTester::processLine(std::string strLine, const std::string& strFileWithLine, 
									std::ifstream &ifs)
{
	// Convert string from Normal to CPP format
	::convertNormalStringToCppString( strLine );

	// Tokenize the line
	vector<string> vecTokens;
	StringTokenizer::sGetTokens( strLine, '|', vecTokens );

	// Line type keywords
	const string& strFILE_KEYWORD( "<FILE>" );
	const string& strCASE_KEYWORD( "<TESTCASE>" );

	// Retrieve the tokens
	string strKey("");

	// First Token will be the <TESTCASE> or <FILE>
	if (vecTokens.size() > 0)
	{
		strKey = vecTokens[0];
	}

	// Both strKey and the strFileOrOperator should be defined
	if (!strKey.empty())
	{
		// Strings non-empty, check for keyword 
		// and valid filename
		if (strKey == strFILE_KEYWORD)
		{
			processFileKeyword(strFileWithLine, strLine, vecTokens);
		}
		else if (strKey == strCASE_KEYWORD)
		{
			processTestCase(strLine, vecTokens);
		}
		// First token is not a keyword, this is an error
		else
		{
			// Create and throw exception
			UCLIDException ue( "ELI15943", "Unable to process line." );
			ue.addDebugInfo( "Line Text", strLine );
			ue.addDebugInfo("Key", strKey);
			throw ue;
		}
	}
	else
	{
		// Create and throw exception
		UCLIDException ue( "ELI15944", "Unable to process line." );
		ue.addDebugInfo( "Line Text", strLine );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CStringCSISTester::doTest(const string& strOperation, const stringCSIS& str1, const stringCSIS& str2, 
		const std::string strFindType, const long lCount, const std::vector<std::string>& vecValues )
{
	// Default to success
	VARIANT_BOOL	vbRet = VARIANT_TRUE;
	bool bExceptionCaught = false;

	// String for the test case label
	string strTestCaseLabel;

	// If the operation to perform is one of !=, ==, <, <=, >, >= display String1 <operator> String2 
	if ( isOperator(strOperation) )
	{
		strTestCaseLabel = "String1 " + strOperation + " String2";
	}
	// If the operation is one of the find operations String1.<find operation> ( String2,...
	else
	{
		// CPC needs to display the count String1.<find operation> (String2, <offset>, Count )
		if ( strFindType == gstrCHAR_POINTER_WITH_COUNT_SPECIFIER )
		{
			strTestCaseLabel = "String1." + strOperation + "( String2, <offset>, " + asString(lCount) + " )";
		}
		// All others display String1.<find operation>( String2 )
		else
		{
			strTestCaseLabel = "String1." + strOperation + "( String2 )";
		}
	}

	// Start the test case with the label
	m_ipResultLogger->StartTestCase( _bstr_t(strTestCaseLabel.c_str()), _bstr_t("Test stringCSIS"), kAutomatedTestCase );


	// Create the values detail to show String1 and String2 values with case sensitive flag value
	string strDetail = "";
	strDetail = "String1 = '";
	strDetail += str1;
	strDetail +=  "' Case sensitive = ";
	strDetail += (str1.isCaseSensitive()) ? "true" : "false";
	strDetail += "\r\n";
	strDetail += "String2 = '";
	strDetail +=  str2.c_str();
	strDetail += "' Case sensitive = ";
	strDetail += (str2.isCaseSensitive()) ? "true" : "false";

	// Add values node to result logger
	m_ipResultLogger->AddTestCaseDetailNote("Values", strDetail.c_str() );

	// Get the results
	try
	{
		bool bSuccess;

		// Perform the test
		if ( !performOperatorTest(strOperation, str1, str2, vecValues[0], bSuccess ))
		{
			// strOperation was not an Operator so perform the find operation
			if ( !performFindOperation ( strOperation, str1, str2, strFindType, lCount, vecValues, bSuccess ))
			{
				// strOperation was not an operator or find so throw an exception
				UCLIDException ue("ELI15948", "Invalid operation.");
				ue.addDebugInfo("Operation", strOperation );
				throw ue;
			}
		}

		// Set vbRet to indicate success or failure
		vbRet = asVariantBool(bSuccess);
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI15946", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	// if an exception was caught the test case fails otherwise use the result
	vbRet = bExceptionCaught ? VARIANT_FALSE : vbRet;

	// End the test case
	m_ipResultLogger->EndTestCase( vbRet );
}
//-------------------------------------------------------------------------------------------------
void CStringCSISTester::getFindResults( const stringCSIS& str1, const stringCSIS& str2, const std::string strFindType, 
									   const long lCount, std::set<size_t>& setResults )
{
	// Set lOffset to 0 - we will find all occurrences of the chars in str2
	size_t lOffset =  0;
	size_t lPos;

	// if str2 is empty the result set will be empty
	if ( str2.empty())
	{
		return;
	}

	// loop to find all chars in str2 found in str1
	do
	{
		// Use basic_string
		if (  strFindType == gstrBASIC_STRING_SPECIFIER )
		{
			lPos = (long)str1.find( str2, lOffset );
		}
		// Use first char
		else if (  strFindType == gstrCHAR_SPECIFIER )
		{
			lPos = (long)str1.find( str2[0], lOffset );
		}
		// Use char * with count
		else if ( strFindType == gstrCHAR_POINTER_WITH_COUNT_SPECIFIER )
		{
			lPos = (long)str1.find( str2.c_str(), lOffset, lCount );
		}
		// Use char * with no count
		else if (  strFindType == gstrCHAR_POINTER_SPECIFIER )
		{
			lPos = (long)str1.find( str2.c_str(), lOffset );
		}
		// All other values for strFindType are invalid so throw an exceptions
		else
		{
			UCLIDException ue("ELI15996", "Invalid FindType!");
			ue.addDebugInfo("FindType", strFindType);
			throw ue;
		}

		// If char was found lPos != npos
		if ( lPos != string::npos )
		{
			// Offset for next search +1 to the found position
			lOffset = lPos + 1;

			// Add found pos to results 
			setResults.insert(lPos);
		}
	}
	while ( lPos != string::npos );
}
//-------------------------------------------------------------------------------------------------
void CStringCSISTester::getFindFirstNotOfResults(const stringCSIS& str1, const stringCSIS& str2, const std::string strFindType, 
						 const long lCount, std::set<size_t>& setResults )
{
	// SetlOffset to 0 - we will find all occurrences of the chars in str2
	long lOffset =  0;
	long lPos;

	// if str2 is an empty string the results should be empty
	if (str2.empty())
	{
		return;
	}

	// loop to find all chars in str2 found in str1
	do
	{
		// Use basic_string
		if (  strFindType == gstrBASIC_STRING_SPECIFIER )
		{
			lPos = (long)str1.find_first_not_of( str2, lOffset );
		}
		// Use first char
		else if (  strFindType == gstrCHAR_SPECIFIER )
		{
			lPos = (long)str1.find_first_not_of( str2[0], lOffset );
		}
		// Use char * with count
		else if ( strFindType == gstrCHAR_POINTER_WITH_COUNT_SPECIFIER )
		{
			lPos = (long)str1.find_first_not_of( str2.c_str(), lOffset, lCount );
		}
		// Use char * with no count
		else if (  strFindType == gstrCHAR_POINTER_SPECIFIER )
		{
			lPos = (long)str1.find_first_not_of( str2.c_str(), lOffset );
		}
		// All other values for strFindType are invalid so throw an exceptions
		else
		{
			UCLIDException ue("ELI16001", "Invalid FindType!");
			ue.addDebugInfo("FindType", strFindType);
			throw ue;
		}

		// If char was found lPos != npos
		if ( lPos != string::npos )
		{
			// Offset for next search +1 to the found position
			lOffset = lPos + 1;

			// Add found pos to results 
			setResults.insert(lPos);
		}
	}
	while ( lPos != string::npos );
}
//-------------------------------------------------------------------------------------------------
void CStringCSISTester::getFindFirstOfResults(const stringCSIS& str1, const stringCSIS& str2, const std::string strFindType, 
					  const long lCount, std::set<size_t>& setResults )
{
	// SetlOffset to 0 - we will find all occurrences of the chars in str2
	long lOffset =  0;
	long lPos;

	// if str2 is empty the results should be empty
	if (str2.empty())
	{
		return;
	}

	// loop to find all chars in str2 found in str1
	do
	{
		// Use basic_string
		if (  strFindType == gstrBASIC_STRING_SPECIFIER )
		{
			lPos = (long)str1.find_first_of( str2, lOffset );
		}
		// Use first char
		else if (  strFindType == gstrCHAR_SPECIFIER )
		{
			lPos = (long)str1.find_first_of( str2[0], lOffset );
		}
		// Use char * with count
		else if ( strFindType == gstrCHAR_POINTER_WITH_COUNT_SPECIFIER )
		{
			lPos = (long)str1.find_first_of( str2.c_str(), lOffset, lCount );
		}
		// Use char * with no count
		else if (  strFindType == gstrCHAR_POINTER_SPECIFIER )
		{
			lPos = (long)str1.find_first_of( str2.c_str(), lOffset );
		}
		// All other values for strFindType are invalid so throw an exceptions
		else
		{
			UCLIDException ue("ELI16000", "Invalid FindType!");
			ue.addDebugInfo("FindType", strFindType);
			throw ue;
		}

		// If char was found lPos != npos
		if ( lPos != string::npos )
		{
			// Offset for next search +1 to the found position
			lOffset = lPos + 1;

			// Add found pos to results 
			setResults.insert(lPos);
		}
	}
	while ( lPos != string::npos );

}
//-------------------------------------------------------------------------------------------------
void CStringCSISTester::getFindLastNotOfResults(const stringCSIS& str1, const stringCSIS& str2, const std::string strFindType, 
						const long lCount, std::set<size_t>& setResults )
{
	// Set lOffset to length of string since search is from the end and we will find all occurrences of the chars in str2
	long lOffset = (long)string::npos;
	long lPos;

	// if str2 is empty the results should be empty
	if (str2.empty())
	{
		return;
	}

	// loop to find all chars in str2 found in str1
	do
	{
		// Use basic_string
		if (  strFindType == gstrBASIC_STRING_SPECIFIER )
		{
			lPos = (long)str1.find_last_not_of( str2, lOffset );
		}
		// Use first char
		else if (  strFindType == gstrCHAR_SPECIFIER )
		{
			lPos = (long)str1.find_last_not_of( str2[0], lOffset );
		}
		// Use char * with count
		else if ( strFindType == gstrCHAR_POINTER_WITH_COUNT_SPECIFIER )
		{
			lPos = (long)str1.find_last_not_of( str2.c_str(), lOffset, lCount );
		}
		// Use char * with no count
		else if (  strFindType == gstrCHAR_POINTER_SPECIFIER )
		{
			lPos = (long)str1.find_last_not_of( str2.c_str(), lOffset );
		}
		// All other values for strFindType are invalid so throw an exceptions
		else
		{
			UCLIDException ue("ELI15999", "Invalid FindType!");
			ue.addDebugInfo("FindType", strFindType);
			throw ue;
		}

		// If char was found lPos != npos
		if ( lPos != string::npos )
		{
			// Offset for next search -1 to the found position
			lOffset = lPos - 1;

			// Add found pos to results 
			setResults.insert(lPos);
		}
	}
	while ( lPos != string::npos && lPos > 0);
}
//-------------------------------------------------------------------------------------------------
void CStringCSISTester::getFindLastOfResults(const stringCSIS& str1, const stringCSIS& str2, const std::string strFindType, 
					 const long lCount, std::set<size_t>& setResults )
{
	// Set lOffset to length of string since search is from the end and we will find all occurrences of the chars in str2
	long lOffset =  (long)string::npos;
	long lPos;

	// if str2 is empty the results should be empty
	if (str2.empty())
	{
		return;
	}

	// loop to find all chars in str2 found in str1
	do
	{
		// Use basic_string
		if (  strFindType == gstrBASIC_STRING_SPECIFIER )
		{
			lPos = (long)str1.find_last_of( str2, lOffset );
		}
		// Use first char
		else if (  strFindType == gstrCHAR_SPECIFIER )
		{
			lPos = (long)str1.find_last_of( str2[0], lOffset );
		}
		// Use char * with count
		else if ( strFindType == gstrCHAR_POINTER_WITH_COUNT_SPECIFIER )
		{
			lPos = (long)str1.find_last_of( str2.c_str(), lOffset, lCount );
		}
		// Use char * with no count
		else if (  strFindType == gstrCHAR_POINTER_SPECIFIER )
		{
			lPos = (long)str1.find_last_of( str2.c_str(), lOffset );
		}
		// All other values for strFindType are invalid so throw an exceptions
		else
		{
			UCLIDException ue("ELI15998", "Invalid FindType!");
			ue.addDebugInfo("FindType", strFindType);
			throw ue;
		}

		// If char was found lPos != npos
		if ( lPos != string::npos )
		{
			// Offset for next search -1 to the found position
			lOffset = lPos - 1;

			// Add found pos to results 
			setResults.insert(lPos);
		}
	}
	while ( lPos != string::npos && lPos > 0);
}
//-------------------------------------------------------------------------------------------------
void CStringCSISTester::getRfindResults(const stringCSIS& str1, const stringCSIS& str2, const std::string strFindType, 
				const long lCount, std::set<size_t>& setResults )
{
	// Set lOffset to length of string since search is from the end and we will find all occurrences of the chars in str2
	long lOffset = (long)string::npos;
	long lPos;

	// if str2 is empty the results should be empty
	if (str2.empty())
	{
		return;
	}

	// loop to find all chars in str2 found in str1
	do
	{
		// Use basic_string
		if (  strFindType == gstrBASIC_STRING_SPECIFIER )
		{
			lPos = (long)str1.rfind( str2, lOffset );
		}
		// Use first char
		else if (  strFindType == gstrCHAR_SPECIFIER )
		{
			lPos = (long)str1.rfind( str2[0], lOffset );
		}
		// Use char * with count
		else if ( strFindType == gstrCHAR_POINTER_WITH_COUNT_SPECIFIER )
		{
			lPos = (long)str1.rfind( str2.c_str(), lOffset, lCount );
		}
		// Use char * with no count
		else if (  strFindType == gstrCHAR_POINTER_SPECIFIER )
		{
			lPos = (long)str1.rfind( str2.c_str(), lOffset );
		}
		// All other values for strFindType are invalid so throw an exceptions
		else
		{
			UCLIDException ue("ELI15997", "Invalid FindType!");
			ue.addDebugInfo("FindType", strFindType);
			throw ue;
		}

		// If char was found lPos != npos
		if ( lPos != string::npos )
		{
			// Offset for next search -1 to the found position
			lOffset = lPos - 1;

			// Add found pos to results 
			setResults.insert(lPos);
		}
	}
	while ( lPos != string::npos && lPos > 0);
}
//-------------------------------------------------------------------------------------------------
std::string CStringCSISTester::getSetValueString( const std::string strSetName, std::set<size_t> setValues )
{
	// Create the return string
	string strRtn = strSetName + " ( ";
	
	// If the set is not empty add the values separated by a ", "
	if ( !setValues.empty() ) 
	{
		// Do not want the first value preceded by a comma 
		string strComma = "";

		// Loop through all of the values
		for each ( long l in setValues )
		{
			// Append to the return string
			strRtn += strComma + asString(l);
			strComma = ", ";
		}
	}

	// Close )
	strRtn += " )";
	
	// return the value
	return strRtn;
}
//-------------------------------------------------------------------------------------------------
bool CStringCSISTester::performOperatorTest(const string& strOperation, 
											const stringCSIS& str1, const stringCSIS& str2,
											const string& strExpectedResult,
											bool& rbSuccess )
{
	bool bOperatorResult;

	// Perform the operation in strOperation
	if ( strOperation == "!=" )
	{
		bOperatorResult = str1 != str2;
	}
	else if ( strOperation == "==" )
	{
		bOperatorResult = str1 == str2;
	}
	else if ( strOperation == "<" )
	{
		bOperatorResult = str1 < str2;
	}
	else if ( strOperation == "<=" )
	{
		bOperatorResult = str1 <= str2;
	}
	else if ( strOperation == ">" )
	{
		bOperatorResult = str1 > str2;
	}
	else if ( strOperation == ">=" )
	{
		bOperatorResult = str1 >= str2;
	}
	else
	{
		// strOperation is not an operator
		return false;
	}

	// test was successful if expected result == the result
	rbSuccess = asCppBool(strExpectedResult) ==  bOperatorResult;

	string strResult;

	// Add Results detail to result logger
	strResult = "String1 " + strOperation + " String2" + " = ";
	strResult += (bOperatorResult) ? "true" : "false";
	strResult += (rbSuccess) ? " as expected" : "  is incorrect";
	m_ipResultLogger->AddTestCaseDetailNote("Results", strResult.c_str() );

	// return true because strOperation was an operator
	return true;

}
//-------------------------------------------------------------------------------------------------
bool CStringCSISTester::performFindOperation (const string& strOperation, const stringCSIS& str1, const stringCSIS& str2,
		const std::string strFindType, const long lCount, const std::vector<std::string>& vecValues,
		bool& bSuccess )
{
	// result set
	std::set<size_t> setResults;

	// Perform the find operation in strOperation
	if ( strOperation == "find" )
	{
		getFindResults( str1, str2, strFindType, lCount, setResults );
	}
	else if ( strOperation == "find_first_not_of" )
	{
		getFindFirstNotOfResults( str1, str2, strFindType, lCount, setResults );
	}
	else if ( strOperation == "find_first_of" )
	{
		getFindFirstOfResults( str1, str2, strFindType, lCount, setResults );
	}
	else if ( strOperation == "find_last_not_of" )
	{
		getFindLastNotOfResults( str1, str2, strFindType, lCount, setResults );
	}
	else if ( strOperation == "find_last_of" )
	{
		getFindLastOfResults( str1, str2, strFindType, lCount, setResults );
	}
	else if ( strOperation == "rfind" )
	{
		getRfindResults( str1, str2, strFindType, lCount, setResults );
	}
	else
	{
		// strOperation was not a find operation
		return false;
	}

	// compare expected results with the found results
	bSuccess = analyzeFindResults( setResults, vecValues );

	// a find operation was performed
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CStringCSISTester::analyzeFindResults( const set<size_t>& setResults, const vector<string>& vecValues )
{
	// set sucess to to true
	bool bSuccess = true;

	// Set for expected values;
	set<size_t> setExpected;

	// Loop throw all of the expected values in the vecValues vector
	for ( unsigned int i = 0; i < vecValues.size(); i++ )
	{
		// Convert the string to long value
		long lExpectedValue = asLong(vecValues[i]);

		// If the value to find is -1 then there are should be no values found
		if ( lExpectedValue != -1 )
		{
			// Add value to the expected set
			setExpected.insert(lExpectedValue);

			// If the expected value is not in the result set the value was not found
			if ( setResults.find(lExpectedValue) == setResults.end() )
			{
				// Set success flag to false
				bSuccess = false;
			}
		}
	}

	// If the size of the expected set is different from the size of the result set
	// there were either more items found then expected or fewer
	if ( setResults.size() !=  setExpected.size() )
	{
		bSuccess = false;
	}

	// Get the string representatino of the expected set
	string strExpected = getSetValueString("Expected", setExpected);
	string strFound  = getSetValueString ("Found", setResults);

			// Put expected and found on different lines in the detail not
	string strResult = strExpected + "\r\n" + strFound;

	// Add detail not with expected and found results
	m_ipResultLogger->AddTestCaseDetailNote("Results", strResult.c_str() );

	return bSuccess;
}
//-------------------------------------------------------------------------------------------------
bool CStringCSISTester::isOperator( const std::string strOperation )
{
	// Return true if the string is  "==", "!=", "<", "<=", ">=", or ">"
	if ( strOperation == "==" || strOperation == "!=" ||
		 strOperation == "<" || strOperation == "<=" ||
		 strOperation == ">" || strOperation == ">=" )
	{
		return true;
	}

	// string is not one of"==", "!=", "<", "<=", ">=", or ">"
	return false;
}
//-------------------------------------------------------------------------------------------------

