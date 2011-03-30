// EntityNameDataScorerTester.cpp : Implementation of CEntityNameDataScorerTester
#include "stdafx.h"
#include "AFDataScorersTest.h"
#include "EntityNameDataScorerTester.h"

#include <cpputil.h>
#include <UCLIDException.h>

#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CEntityNameDataScorerTester
//-------------------------------------------------------------------------------------------------
CEntityNameDataScorerTester::CEntityNameDataScorerTester()
:	m_ipResultLogger(NULL),
	m_ipENDScorer(CLSID_EntityNameDataScorer)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI09104", m_ipENDScorer != __nullptr);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09103")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorerTester::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
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
STDMETHODIMP CEntityNameDataScorerTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		// Check license
		validateLicense();
		if (m_ipResultLogger == __nullptr)
		{
			throw UCLIDException("ELI09101", "Please set ResultLogger before proceeding.");
		}

		// Find and validate the test file
		string strTestFile = getMasterTestFileName(pParams, asString(strTCLFile));
		try
		{
			validateFileOrFolderExistence( strTestFile );
		}
		catch (UCLIDException& ue)
		{
			UCLIDException uexOuter( "ELI09107", 
				"Unable to open Entity Name Data Scorer master test input file!", ue);
			throw uexOuter;
		}

		// Parse each line of input file
		processFile( strTestFile );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09102")
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorerTester::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorerTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// Check license
		validateLicense();

		m_ipResultLogger = pLogger;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09100")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorerTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityNameDataScorerTester::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CEntityNameDataScorerTester::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI09161", "EntityNameDataScorerTester");
}
//-------------------------------------------------------------------------------------------------
const std::string CEntityNameDataScorerTester::getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const
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
			UCLIDException ue("ELI12282", "Required master testing .DAT file not found!");
			throw ue;
		}

		return strMasterDatFileName;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue("ELI12283", "Required master testing .DAT file not found!");
		throw ue;	
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityNameDataScorerTester::processFile(std::string strFile)
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
		processLine( strLine, strFile );
	}
	while (!ifs.eof());
}
//-------------------------------------------------------------------------------------------------
void CEntityNameDataScorerTester::processLine(std::string strLine, const std::string& strFileWithLine )
{
	// Convert string from Normal to CPP format
	::convertNormalStringToCppString( strLine );

	// Tokenize the line
	vector<string> vecTokens;
	StringTokenizer::sGetTokens( strLine, '|', vecTokens );

	// Line type keywords
	string strFileKeyword( "<FILE>" );
	string strCaseKeyword( "<TESTCASE>" );
	string strENDSLogKeyword( "<ENDSLog>" );

	// Retrieve the tokens
	string strKey = "";

	if(vecTokens.size() > 0)
	{
		strKey = vecTokens[0];
	}

	string strFileOrLabel;
	string strText;

	if (vecTokens.size() > 1)
	{
		// Either a filename or a test label
		strFileOrLabel = vecTokens[1];
	}

	// Validate the tokens
	bool	bValid = false;
	if (!strKey.empty() && !strFileOrLabel.empty())
	{
		// Strings non-empty, check for keyword 
		// and valid filename
		if (strKey == strFileKeyword || strKey == strENDSLogKeyword)
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
				UCLIDException uexOuter( "ELI09108", 
					"Reference to invalid file name encountered!", ue );
				uexOuter.addDebugInfo( "strFileOrLabel", strFileOrLabel );
				uexOuter.addDebugInfo( "strLine", strLine );
				throw uexOuter;
			}

			// Process the file
			if ( strKey == strENDSLogKeyword )
			{
				// Default is to do Single and Group tests 
				bool bGroupOnly = false;
				if ( vecTokens.size() == 3 )
				{
					if ( vecTokens[2] == "GroupOnly" )
					{
						bGroupOnly = true;
					}
				}
				processENDSLog( strFileOrLabel, bGroupOnly );
			}
			else 
			{
				processFile( strFileOrLabel );
			}
		}
		else if (strKey == strCaseKeyword)
		{
			if ( vecTokens.size() > 2 )
			{
				// token 1 will be the score and 2 the attribute
				long nExpectedScore = asLong(vecTokens[1]);
				string strAttributeValue = vecTokens[2];
				// Execute the test
				doSingleTest( nExpectedScore, strAttributeValue );
			}
			// Done processing this line, just return
			return;
		}
		// Invalid input on line
		else 
		{
			// Create and throw exception
			UCLIDException ue( "ELI09109", "Unable to process line." );
			ue.addDebugInfo( "Line Text", strLine );
			throw ue;
		}
	}
	else
	{
		// Create and throw exception
		UCLIDException ue( "ELI09110", "Unable to process line." );
		ue.addDebugInfo( "Line Text", strLine );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityNameDataScorerTester::processENDSLog( std::string strFile, bool bGroupOnly )
{
	// Get the entire contents of the file
	string strENDSLogContents = getTextFileContentsAsString( strFile );

	// Separate into lines
	vector<string> vecLines;
	StringTokenizer::sGetTokens( strENDSLogContents, "\r\n" , vecLines );
	// Read each line of the input file
//	ifstream ifs( strFile.c_str() );
//	CommentedTextFileReader fileReader( ifs, "//", false );
	
	// Process the lines
	vector<string> vecAttrValues;
	long nNumberOfLines = vecLines.size();
	//while (! fileReader.reachedEndOfStream() )
	for ( int i = 0; i < nNumberOfLines; i++ )
	{
		string strLine = vecLines[i];

		// Remove Comments
		// NOTE: Using this method instead of CommentedTextFileReader
		//			because the CommentedTextFileReader trims leading and trailing spaces
		int nCommentPos = strLine.find("//");
		if (nCommentPos != string::npos)
		{
			strLine = strLine.substr(0, nCommentPos);
		}
		// if the line is blank clear the attribute values vector
		if ( strLine.empty() )
		{
			vecAttrValues.clear();
		}
		else
		{
			// Replace the \r\n place holders
			::convertNormalStringToCppString( strLine );
			// Tokenize the line
			vector<string> vecTokens;
			StringTokenizer::sGetTokens( strLine, '|', vecTokens );
			// There should be 2 tokens on each line
			if ( vecTokens.size() == 2 )
			{
				// First token on line is a score value
				long nExpectedLineScore = asLong( vecTokens[0] );

				// Second token is either the attribute to be scored or the string TotalGroupScore
				// indicating the end of a group of attributes to be scored together
				string strAttrValue = vecTokens[1];
				if ( strAttrValue == "TotalGroupScore" )
				{
					// do the test for group attributes using the score on the current line for the total score
					doGroupTest( nExpectedLineScore, vecAttrValues );
					// Clear the Attribute vector
					vecAttrValues.clear();
				}
				else
				{
					// Put the attribute value to test in the current group list
					vecAttrValues.push_back( strAttrValue );
					// Test the single attribute on the line
					if ( !bGroupOnly )
					{
						doSingleTest( nExpectedLineScore, strAttrValue );
					}
				}
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityNameDataScorerTester::doSingleTest( long nExpectedScore, std::string& strAttributeValue )
{
	// Start the test case
	m_ipResultLogger->StartTestCase( _bstr_t("Test Single"), _bstr_t("Test Entity Name Data Scorer"), 
		kAutomatedTestCase );

	bool bSuccess = false;
	bool bExceptionCaught = false;
	long nCalculatedScore = 0;
	
	try
	{
		// Initialize objects
		IDataScorerPtr ipENDS( CLSID_EntityNameDataScorer );
		ASSERT_RESOURCE_ALLOCATION("ELI09112", ipENDS != __nullptr );

		// Build attribute value from strAttributeValue
		IAttributePtr ipAttr ( CLSID_Attribute );
		ASSERT_RESOURCE_ALLOCATION("ELI09113", ipAttr != __nullptr );
		
		ISpatialStringPtr ipValue ( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION("ELI09114", ipValue != __nullptr );
		ipValue->CreateNonSpatialString(strAttributeValue.c_str(), "");

		ipAttr->Value = ipValue;

		// Get the attribute score
		nCalculatedScore = ipENDS->GetDataScore1( ipAttr );

		// Output results to logger
		string strExpected = asString(nExpectedScore);
		string strCalculated = asString(nCalculatedScore);
		m_ipResultLogger->AddTestCaseMemo(_bstr_t("Attribute Value"), _bstr_t(strAttributeValue.c_str()));

		//Add Compare Scores to the tree and the test logger window
		m_ipResultLogger->AddTestCaseCompareData("Compare Scores",
												 _bstr_t("Expected Score"), _bstr_t(strExpected.c_str()),
												 _bstr_t("Calculated Score"), _bstr_t(strCalculated.c_str()));

		// Set bSuccess to indicate results
		bSuccess = (nCalculatedScore == nExpectedScore);
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI09111", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	bSuccess = bSuccess && !bExceptionCaught;

	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
}
//-------------------------------------------------------------------------------------------------
void CEntityNameDataScorerTester::doGroupTest( long nExpectedGroupScore, vector<std::string>& vecAttrValues )
{
		// Start the test case
	m_ipResultLogger->StartTestCase( _bstr_t("Test Group"), _bstr_t("Test Entity Name Data Scorer"), 
		kAutomatedTestCase );

	bool bSuccess = false;
	bool bExceptionCaught = false;
	long nCalculatedScore = 0;
	
	try
	{
		// Initialize objects
		IDataScorerPtr ipENDS( CLSID_EntityNameDataScorer );
		ASSERT_RESOURCE_ALLOCATION("ELI09115", ipENDS != __nullptr );

		// Create vector to hold attributes to test
		IIUnknownVectorPtr ipAttributes ( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION("ELI09119", ipAttributes != __nullptr );

		// Declare string for individual attribute value
		string strAttributeValue;

		// Build Vector of attributes to pass to GetDataScore2
		long nNumValues = vecAttrValues.size();
		for ( int i = 0; i < nNumValues; i++ )
		{
			// Build the attribute
			IAttributePtr ipAttr ( CLSID_Attribute );
			ASSERT_RESOURCE_ALLOCATION("ELI09116", ipAttr != __nullptr );
		
			strAttributeValue = vecAttrValues[i];

			ISpatialStringPtr ipValue ( CLSID_SpatialString );
			ASSERT_RESOURCE_ALLOCATION("ELI09117", ipValue != __nullptr );
			ipValue->CreateNonSpatialString(strAttributeValue.c_str(), "");

			ipAttr->Value = ipValue;

			// Add the attribute to group list
			ipAttributes->PushBack( ipAttr );
			
			// add the attribute value to the test logger
			string strAttrValueLabel = "Attribute Value " + asString(i);
			m_ipResultLogger->AddTestCaseMemo(_bstr_t(strAttrValueLabel.c_str()), _bstr_t(strAttributeValue.c_str()));
		}

		// Get the group score
		nCalculatedScore = ipENDS->GetDataScore2( ipAttributes );

		// Log results to test logger
		string strExpected = asString(nExpectedGroupScore);
		string strCalculated = asString(nCalculatedScore);
		
		//Add the Compare Scores to the tree and results dialog.
		m_ipResultLogger->AddTestCaseCompareData("Compare Scores",
												 _bstr_t("Expected Score"), _bstr_t(strExpected.c_str()),
												 _bstr_t("Calculated Score"), _bstr_t(strCalculated.c_str()));
		// Set the results
		bSuccess = (nCalculatedScore == nExpectedGroupScore);
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI09118", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	bSuccess = bSuccess && !bExceptionCaught;

	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
}
//-------------------------------------------------------------------------------------------------
