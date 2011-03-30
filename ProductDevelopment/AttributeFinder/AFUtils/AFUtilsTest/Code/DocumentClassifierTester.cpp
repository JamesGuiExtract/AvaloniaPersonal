// DocumentClassifierTester.cpp : Implementation of CDocumentClassifierTester
#include "stdafx.h"
#include "AFUtilsTest.h"
#include "DocumentClassifierTester.h"

#include <SpecialStringDefinitions.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#include <cstdio>
#include <fstream>
#include <cstdlib>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// DocType keywords - for special handling
const string gstrKeywordPassIfZero("<PassIfZero>");
const string gstrKeywordFailIfZero("<FailIfZero>");
const string gstrKeywordFailIfUnique("<FailIfUnique>");
const string gstrKeywordFailIfMultipleAtMinLevel("<FailIfMultipleAtMinLevel>");
const string gstrKeywordFailIfExcluding("<FailIfExcluding>");

//-------------------------------------------------------------------------------------------------
// CDocumentClassifierTester
//-------------------------------------------------------------------------------------------------
CDocumentClassifierTester::CDocumentClassifierTester()
: m_ipResultLogger(NULL),
  m_bAllPages(true),
  m_lTestCaseCounter(0),
  m_lSuccessfulTestCounter(0)
{
	try
	{
		// Create the Document Classifier
		m_ipDocClassifier.CreateInstance( CLSID_DocumentClassifier );
		ASSERT_RESOURCE_ALLOCATION("ELI06004", m_ipDocClassifier != __nullptr);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06031")
}
//-------------------------------------------------------------------------------------------------
CDocumentClassifierTester::~CDocumentClassifierTester()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16335");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifierTester::InterfaceSupportsErrorInfo(REFIID riid)
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
STDMETHODIMP CDocumentClassifierTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// *******************************
		// Format of TestDocClassifier.dat
		// *******************************
		// It will contain three formats of lines:
		// 1) Simply the .uss file to be processed, followed by pages to 
		//    use in the image (empty string to indicate ALL pages), 
		//    followed by the document industry category, and followed 
		//    by the expected document type. They shall be separated by 
		//    a semicolon(;). 
		//    Note that the .uss file name shall either be a fully 
		//    qualified file name or a file without any directory info. 
		//    In the later case, it will be considered as a file
		//    existing in the same directory as the TestDocClassifer.dat.
		// 2) <FOLDER> keyword immediately followed by a fully qualified
		//    directory name, followed by a semicolon, followed by pages 
		//    to use in the image (empty string to indicate ALL pages), 
		//    followed by a semicolon, followed by the document industry 
		//    category name, followed by a semicolon, and followed by the 
		//    expected document type.
		// 3) <FILE> keyword immediately followed by a fully qualified
		//    file name.  The format of lines in the file is as above.

		// Reset the test case counters
		m_lTestCaseCounter = 0;
		m_lSuccessfulTestCounter = 0;

		// find and process the Master test file
		string strTestFile = getMasterTestFileName( pParams, asString( strTCLFile ) );
		processDatFile( strTestFile );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05997")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifierTester::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifierTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		m_ipResultLogger = pLogger;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07291")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifierTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifierTester::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
bool CDocumentClassifierTester::compareTags(const string& strDocName,
											IAFDocumentPtr ipAFDoc, 
											const string& strExpectedDocTypes)
{
	bool bRet = true;

	// Retrieve information from the String Tags
	IStrToStrMapPtr ipStringTags = ipAFDoc->StringTags;
	if (ipStringTags != __nullptr && ipStringTags->Size > 0)
	{
		// Get the document probability
		string strActualProbability = asString(ipStringTags->GetValue(_bstr_t(DOC_PROBABILITY.c_str())));
		strActualProbability = getDocumentProbabilityString(strActualProbability);

		// display the actual probability
		CString zMsg("");
		zMsg.Format("Probability : %s", strActualProbability.c_str());
		m_ipResultLogger->AddTestCaseNote(_bstr_t(zMsg));
		
		// Get block ID
		string strBlockID = getObjectID(ipAFDoc, DCC_BLOCK_ID_TAG);
		if (!strBlockID.empty())
		{
			// Display the block ID
			strBlockID = "<BlockID>: " + strBlockID;
			m_ipResultLogger->AddTestCaseNote(get_bstr_t(strBlockID.c_str()));
		}

		// Get rule ID
		string strRuleID = getObjectID(ipAFDoc, DCC_RULE_ID_TAG);
		if (!strRuleID.empty())
		{
			// Display the rule ID
			strRuleID = "<RuleID>: " + strRuleID;
			m_ipResultLogger->AddTestCaseNote(get_bstr_t(strRuleID.c_str()));
		}

		// Handle Zero probability case
		if (strActualProbability == "Zero")
		{
			// Check for <PassIfZero> or <FailIfUnique> keywords
			if ((strExpectedDocTypes == gstrKeywordPassIfZero) || 
				(strExpectedDocTypes == gstrKeywordFailIfUnique))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		
		// Get the document types
		string strActualDocTypes("");
		IStrToObjectMapPtr ipObjTags = ipAFDoc->ObjectTags;

		if (ipObjTags != __nullptr && ipObjTags->Size > 0)
		{
			// get found document types
			IVariantVectorPtr ipDocTypes = ipObjTags->GetValue(_bstr_t(DOC_TYPE.c_str()));
			if (ipDocTypes != __nullptr)
			{
				long lSize = ipDocTypes->Size;
				for (long n = 0; n < lSize; n++)
				{
					if (n == 0)
					{
						strActualDocTypes = asString(_bstr_t(ipDocTypes->GetItem(n)));
						continue;
					}
					
					strActualDocTypes += "|" + asString(_bstr_t(ipDocTypes->GetItem(n)));
				}

				/////////////////////////////////////////////////////
				// Check ExpectedDocTypes for a Document Type keyword
				/////////////////////////////////////////////////////
				if ((strExpectedDocTypes.length() > 0) && strExpectedDocTypes[0] == '<')
				{
					// Handle <FailIfZero> case 
					if (strExpectedDocTypes == gstrKeywordFailIfZero)
					{
						// Add Detail Note
						CString zMsg("");
						zMsg.Format("Expected Document Type is : %s\r\n\r\nAnalyzed Document Type is : %s", 
							strExpectedDocTypes.c_str(), strActualDocTypes.c_str());

						m_ipResultLogger->AddTestCaseDetailNote( 
							_bstr_t( "Found Document Type Keyword" ),
							_bstr_t( zMsg ) );

						// Automatically pass this result since it is not Zero
						return true;
					}

					// Handle <FailIfUnique> case 
					if (strExpectedDocTypes == gstrKeywordFailIfUnique)
					{
						// Add Detail Note
						CString zMsg("");
						zMsg.Format("Expected Document Type is : %s\r\n\r\nAnalyzed Document Type is : %s", 
							strExpectedDocTypes.c_str(), strActualDocTypes.c_str());

						CString zCount;
						zCount.Format( "%d Document Type(s) found", ipDocTypes->Size );
						
						m_ipResultLogger->AddTestCaseDetailNote( 
							_bstr_t( LPCTSTR(zCount) ),
							_bstr_t( zMsg ) );

						// Check size of DocTypes vector
						if (ipDocTypes->Size == 1)
						{
							// Single type found, therefore Fail
							return false;
						}
						else
						{
							// Multiple types found, therefore Pass
							return true;
						}
					}

					// Handle <FailIfExcluding> case 
					int nKeywordPos = strExpectedDocTypes.find( gstrKeywordFailIfExcluding );
					if (nKeywordPos == 0)
					{
						// Retrieve appended Document Type
						long lAppendLength = strExpectedDocTypes.length() - 
							gstrKeywordFailIfExcluding.length();
						if (lAppendLength > 0)
						{
							string	strDocType = strExpectedDocTypes.substr( 
								gstrKeywordFailIfExcluding.length(), lAppendLength );

							// Check ActualDocTypes for this type
							long lDocPos = strActualDocTypes.find( strDocType );

							// Add Detail Note
							CString zMsg("");
							zMsg.Format("Expected Document Type is : %s\r\n\r\nAnalyzed Document Type is : %s", 
								strExpectedDocTypes.c_str(), strActualDocTypes.c_str());

							m_ipResultLogger->AddTestCaseDetailNote( 
								(lDocPos >= 0) ? _bstr_t( "Desired DocType Found" ) : _bstr_t( "Desired DocType Not Found" ),
								_bstr_t( zMsg ) );

							if (lDocPos != string::npos)
							{
								return true;
							}
							else
							{
								return false;
							}
						}
						else
						{
							// Invalid parameter, throw Exception
							UCLIDException ue( "ELI08566", "Invalid <FailIfExcluding> parameter." );
							ue.addDebugInfo( "Expected Doc Types", strExpectedDocTypes );
							throw ue;
						}
					}

					// Handle <FailIfMultipleAtMinLevel> case 
					nKeywordPos = strExpectedDocTypes.find( gstrKeywordFailIfMultipleAtMinLevel );
					if (nKeywordPos == 0)
					{
						long lSize = ipDocTypes->Size;

						// Add Detail Note
						CString zMsg("");
						zMsg.Format("Expected Document Type is : %s\r\n\r\nAnalyzed Document Type is : %s", 
							strExpectedDocTypes.c_str(), strActualDocTypes.c_str());

						m_ipResultLogger->AddTestCaseDetailNote( 
							(lSize == 1) ? _bstr_t( "Single Document Type Found" ) : _bstr_t( "Multiple Document Types Found" ),
							_bstr_t( zMsg ) );

						// Check size of DocTypes vector
						if (lSize == 1)
						{
							// Not multiple items, therefore Pass
							return true;
						}

						// Retrieve appended Confidence
						long lAppendLength = strExpectedDocTypes.length() - 
							gstrKeywordFailIfMultipleAtMinLevel.length();
						if (lAppendLength > 0)
						{
							string	strConfidence = strExpectedDocTypes.substr( 
								gstrKeywordFailIfMultipleAtMinLevel.length(), 
								lAppendLength );

							// Check for matching Probability
							if (strActualProbability == strConfidence)
							{
								// Multiple items AT minimum level, therefore Fail
								return false;
							}
							// Check for Actual > Probable
							else if ((strConfidence == "Probable") && 
								(strActualProbability == "Sure"))
							{
								// Multiple items ABOVE minimum level, therefore Fail
								return false;
							}
							// Check for Actual > Maybe
							else if ((strConfidence == "Maybe") && 
								((strActualProbability == "Sure") || (strActualProbability == "Probable")))
							{
								// Multiple items ABOVE minimum level, therefore Fail
								return false;
							}
							// Actual probability is too low
							else
							{
								// Therefore Pass
								return true;
							}
						}
						else
						{
							// Invalid parameter, throw Exception
							UCLIDException ue( "ELI19179", "Invalid <FailIfExcluding> parameter." );
							ue.addDebugInfo( "Expected Doc Types", strExpectedDocTypes );
							throw ue;
						}
					}
				}

				///////////////////////////////////////////////////
				// compare actual document types with expected ones
				///////////////////////////////////////////////////
				if (strExpectedDocTypes != strActualDocTypes)
				{
					bRet = false;
					CString zMsg("");
					zMsg.Format("Expected Document Type is : %s\r\n\r\nAnalyzed Document Type is : %s", 
						strExpectedDocTypes.c_str(), strActualDocTypes.c_str());
					m_ipResultLogger->AddTestCaseDetailNote(
						_bstr_t("Document Types do not match"),
						_bstr_t(zMsg));
				}
			}
		}
	}
	else
	{
		UCLIDException ue("ELI06009", "Failed to create tags for this document.");
		ue.addDebugInfo("Document Name", strDocName);
		throw ue;
	}

	return bRet;
}
//-------------------------------------------------------------------------------------------------
string CDocumentClassifierTester::getDocumentProbabilityString(const string& strProbability)
{
	static string strDescriptions[] = { "Zero", "Maybe", "Probable", "Sure" };
	string strRet("");

	long nIndex = asLong(strProbability);
	if (nIndex < 0 || nIndex > sizeof(strDescriptions))
	{
		UCLIDException ue("ELI06074", "Invalid probability.");
		ue.addDebugInfo("strProbability", strProbability);
		throw ue;
	}

	return strDescriptions[nIndex];
}
//-------------------------------------------------------------------------------------------------
void CDocumentClassifierTester::processDatFile(const string& strDatFileName)
{
	try
	{
		validateFileOrFolderExistence( strDatFileName );
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI07303", "Unable to read Document Classifier test input file.", ue);
		uexOuter.addDebugInfo( "Filename", strDatFileName );
		throw uexOuter;
	}

	// parse each line of input file into 3 part: 
	// input document name, industry category name, expected document type
	ifstream ifs( strDatFileName.c_str() );

	string strLine("");
	CString zCaseNo("");
	CommentedTextFileReader fileReader(ifs, "//", true);

	// Define processing keywords
	string strFolderKeyword("<FOLDER>");
	string strFileKeyword("<FILE>");

	do
	{
		strLine = fileReader.getLineText();
		if (strLine.empty()) 
		{
			continue;
		}
		
		// tokenize current line (delimiter is semicolon";")
		vector<string> vecTokens;
		StringTokenizer::sGetTokens(strLine, ';', vecTokens);

		string strFileOrFolder;
		if (vecTokens.size() == 1)
		{
			// the first token must be a DAT file name
			strFileOrFolder = vecTokens[0];

			int nFileKeywordPos = strFileOrFolder.find( strFileKeyword );
			if (nFileKeywordPos == 0)
			{
				// Get the file name
				string strFile = strFileOrFolder.substr( strFileKeyword.size() );

				// File may be relative to the location of the current file
				strFile = getAbsoluteFileName( strDatFileName, strFile );

				// Process the file
				processDatFile( strFile );

				continue;
			}
			else
			{
				UCLIDException ue( "ELI08573", "Invalid line." );
				ue.addDebugInfo( "Test file name", strDatFileName );
				ue.addDebugInfo( "Line Text", strLine );
				throw ue;
			}
		}
		else if (vecTokens.size() != 4)
		{
			UCLIDException uclidException("ELI06003", "Invalid line.");
			uclidException.addDebugInfo("Test file name", strDatFileName);
			uclidException.addDebugInfo("Line Text", strLine);
			throw uclidException;
		}

		// Get file or folder name
		strFileOrFolder = vecTokens[0];
		// get the pages to process
		m_strPagesToProcess = vecTokens[1];
		// get the industry category name
		string strCategoryName(vecTokens[2]);
		// get the document type
		// NOTE: This text may instead contain a special keyword
		//       with or without subsequent information
		string strDocType(vecTokens[3]);

		// Check for page restriction
		if (m_strPagesToProcess.empty())
		{
			m_bAllPages = true;
		}
		else
		{
			m_bAllPages = false;
		}

		// Validate other input items:
		//   strFileOrFolder must be defined
		//   strCategoryName must be defined
		//   strDocType must be defined
		if (strFileOrFolder.empty() || strCategoryName.empty() || strDocType.empty())
		{
			UCLIDException uclidException("ELI06029", "Invalid line.");
			uclidException.addDebugInfo("Test file name", strDatFileName);
			uclidException.addDebugInfo("Line Text", strLine);
			throw uclidException;
		}
		
		bool bRet = false;

		// ... if strFileOrFolder starts with the keyword <FOLDER>
		int nFolderKeywordPos = strFileOrFolder.find(strFolderKeyword);
		if (nFolderKeywordPos == 0)
		{
			// get the folder name
			string strFolder = strFileOrFolder.substr( strFolderKeyword.size() );
			
			// the specified folder may be relative to the location
			// of the file being parsed.
			strFolder = getAbsoluteFileName( strDatFileName, strFolder );

			// validate the folder name
			validateFileOrFolderExistence( strFolder );

			// get all .uss files under the folder, recursively
			vector<string> vecTextFiles;
			::getFilesInDir( vecTextFiles, strFolder, "*.uss", true );

			for (unsigned int ui = 0; ui < vecTextFiles.size(); ui++)
			{
				bRet = processTestCase( vecTextFiles[ui], strCategoryName, strDocType );
				if (bRet)
				{
					// Update success counter
					m_lSuccessfulTestCounter++;
				}
			}
		}
		else
		{
			string strInputTextFileName = getAbsoluteFileName( strDatFileName, strFileOrFolder );
			validateFileOrFolderExistence( strInputTextFileName );

			bRet = processTestCase( strInputTextFileName, strCategoryName, strDocType );

			// increment the successful test case count if appropriate
			if (bRet)
			{
				// Update success counter
				m_lSuccessfulTestCounter++;
			}
		}
	}
	while (!ifs.eof());
}
//-------------------------------------------------------------------------------------------------
bool CDocumentClassifierTester::processTestCase(const string& strInputFileName, 
												const string& strIndustryCategoryName,
												const string& strExpectedDocType)
{
	bool bSuccess = false;
	bool bExceptionCaught = false;

	// Update test counter
	CString zCaseNo;
	zCaseNo.Format( "%d", ++m_lTestCaseCounter );

	// Initiate a test case
	m_ipResultLogger->StartTestCase(_bstr_t(zCaseNo), _bstr_t(strInputFileName.c_str()), kAutomatedTestCase); 
	m_ipResultLogger->AddTestCaseFile(_bstr_t(strInputFileName.c_str()));

	// the test file is of the form a.tif.txt or a.tif.uss
	// get filename without the last extension refers to the image name
	// add a test case file link to the image name
	string strImageName = getDirectoryFromFullPath(strInputFileName) + "\\" +
		getFileNameWithoutExtension(strInputFileName);
	m_ipResultLogger->AddTestCaseFile(_bstr_t(strImageName.c_str()));

	try
	{			
		// create a new AFDocument for holding the value and tags
		IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI06005", ipAFDoc != __nullptr);

		// Read the input file into a Spatial String
		ISpatialStringPtr ipText = ipAFDoc->Text;
		ipText->LoadFrom(_bstr_t(strInputFileName.c_str()), VARIANT_TRUE);
		
		// Restrict Spatial String to specified pages, if desired
		if (!m_bAllPages)
		{
			// Parse the Pages To Process string
			vector<string> vecTokens;
			StringTokenizer::sGetTokens( m_strPagesToProcess, '-', vecTokens );

			ISpatialStringPtr ipSomeText(NULL);

			// Check for single page
			if (vecTokens.size() == 1)
			{
				long lStart = asLong( vecTokens[0] );
				ipSomeText = ipText->GetSpecifiedPages( lStart, lStart );
			}
			// Two tokens are expected
			else if (vecTokens.size() == 2)
			{
				long lStart = asLong( vecTokens[0] );
				long lEnd;
				if ( (vecTokens[1]).empty() )
				{
					lEnd = -1;
				}
				else
				{
					lEnd = asLong( vecTokens[1] );
				}

				ipSomeText = ipText->GetSpecifiedPages( lStart, lEnd );
			}

			// Apply this text to the AFDocument object
			ASSERT_RESOURCE_ALLOCATION( "ELI08565", ipSomeText != __nullptr );
			ipAFDoc->Text = ipSomeText;
		}

		m_ipDocClassifier->IndustryCategoryName = _bstr_t(strIndustryCategoryName.c_str());
		// run the AFDoc through the classifier to create tags on the doc
		IDocumentPreprocessorPtr ipPreprocessor(m_ipDocClassifier);
		ipPreprocessor->Process(ipAFDoc, NULL);
		
		// Next, we're going to compare the result given by the DocumentClassifier
		// with the expected values.
		bSuccess = compareTags(strInputFileName, ipAFDoc, strExpectedDocType);
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI06166", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);
	
	bSuccess = bSuccess && !bExceptionCaught;

	// end the test case
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
	
	return bSuccess;
}
//-------------------------------------------------------------------------------------------------
std::string CDocumentClassifierTester::getObjectID(IAFDocumentPtr ipAFDoc, const std::string& strTagName)
{
	string strObjectID("");

	IStrToObjectMapPtr ipObjMap = ipAFDoc->ObjectTags;
	if (ipObjMap != __nullptr && ipObjMap->Size > 0)
	{
		// Check if the the object ID that need to be found is inside 
		if (ipObjMap->Contains(get_bstr_t(strTagName.c_str())) == VARIANT_TRUE)
		{
			// Get the related object ID
			IVariantVectorPtr ipObjectWorked = ipObjMap->GetValue(get_bstr_t(strTagName.c_str()));
			if (ipObjectWorked)
			{
				// assume there's always one rule or block that extracts the attributes
				strObjectID = asString( _bstr_t(ipObjectWorked->GetItem(0)));
			}
		}
	}

	return strObjectID;
}
//-------------------------------------------------------------------------------------------------
void CDocumentClassifierTester::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI07290", "Document Classifier Tester" );
}
//-------------------------------------------------------------------------------------------------
const std::string CDocumentClassifierTester::getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const
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
			UCLIDException ue("ELI12288", "Required master testing .DAT file not found.");
			throw ue;
		}

		return strMasterDatFileName;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue("ELI12289", "Required master testing .DAT file not found.");
		throw ue;	
	}
}
//-------------------------------------------------------------------------------------------------
