// SpatialStringSearcherTester.cpp : Implementation of CSpatialStringSearcherTester
#include "stdafx.h"
#include "SpatialStringSearcherTest.h"
#include "SpatialStringSearcherTester.h"

#include <UCLIDException.h>
#include <cpputil.h>

#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <ComUtils.h>
#include <string>

using std::string;
/////////////////////////////////////////////////////////////////////////////
// CSpatialStringSearcherTester

//-------------------------------------------------------------------------------------------------
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcherTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		string strMasterTestFileName = getMasterTestFileName(pParams, asString(strTCLFile));

		try
		{
			validateFileOrFolderExistence( strMasterTestFileName );
		}
		catch (UCLIDException& ue)
		{
			UCLIDException uexOuter( "ELI07916", 
				"Unable to open SpatialStringSearcher master test input file!", ue);
			throw uexOuter;
		}
		
		m_lCurrTestCase = 1;
		// Execute all the test cases
		processFile( strMasterTestFileName );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07915")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcherTester::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcherTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcherTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
const std::string CSpatialStringSearcherTester::getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const
{
	// if pParams is not empty and the second item is specified,
	// then the second item is the master dat file
	if ((ipParams != __nullptr) && (ipParams->Size > 1))
	{
		std::string strMasterTestFile = asString(_bstr_t(ipParams->GetItem(1)));

		if(strMasterTestFile != "")
		{
			return getAbsoluteFileName(strTCLFile, strMasterTestFile);
		}
	}
	else
	{
		// Create and throw exception
		UCLIDException ue("ELI12396", "Required master test .DAT file not specified in TCL file!");
		throw ue;	
	}

	return "";
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcherTester::processFile(const std::string& strFile)
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
		vector<string> vecTokens;
		StringTokenizer::sGetTokens( strLine, ';', vecTokens );
		if (vecTokens.empty())
		{
			continue;
		}
		string tok0 = vecTokens[0];
		if (tok0 == "<FILE>")
		{
			if (vecTokens.size() < 2)
			{
				// Create and throw exception
				UCLIDException ue( "ELI07929", "Unable to process line." );
				ue.addDebugInfo( "Line Text", strLine );
				throw ue;
			}
			string tok1 = vecTokens[1];
			std::string newFileName = getAbsoluteFileName( strFile, tok1 );
			try
			{
				validateFileOrFolderExistence( newFileName );
			}
			catch (UCLIDException& ue)
			{
				UCLIDException uexOuter( "ELI07930", 
					"Reference to invalid file name encountered!", ue );
				uexOuter.addDebugInfo( "FileName", newFileName );
				uexOuter.addDebugInfo( "Line", strLine );
				throw uexOuter;
			}

			// Process the file
			processFile( newFileName );
			
		}
		else if (tok0 == "<SET>")
		{
			processSet(vecTokens, strFile);
		}
		else if (tok0 == "<TESTCASE>")
		{
			processTestCase(vecTokens, strFile);
		}
		else
		{
			// Create and throw exception
			UCLIDException ue( "ELI07918", "Unable to process line." );
			ue.addDebugInfo( "Line Text", strLine );
			throw ue;
		}
	}
	while (!ifs.eof());
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcherTester::processTestCase(std::vector<std::string>& vecTokens, const std::string& strDatFile)
{
	string strTestCaseName = "";
	strTestCaseName += asString(m_lCurrTestCase);
	bool bSuccess = false;
	bool bExceptionCaught = false;

	if (vecTokens.size() < 4)
	{	
		// Create and throw exception
		UCLIDException ue("ELI07964", "Unable to process line." );
		//ue.addDebugInfo( "Line Text", strLine );
		throw ue;
	}
	string strDescription;
	if (vecTokens.size() > 4 && !vecTokens[4].empty())
	{
		strDescription = vecTokens[4];
	}
	else
	{
		strDescription = "Testing Spatial String Searcher";
	}
	m_ipResultLogger->StartTestCase(get_bstr_t(strTestCaseName.c_str()),
					get_bstr_t(strDescription.c_str()), kAutomatedTestCase); 

	try
	{	
		// Add the files to the logger
		std::string ussFileName = getAbsoluteFileName( strDatFile, vecTokens[1] );
		string strPageNum = vecTokens[2];
		long lPageNum = asLong(strPageNum);
		std::string expectedTextFileName = getAbsoluteFileName( strDatFile, vecTokens[3] );

		m_ipResultLogger->AddTestCaseFile(get_bstr_t(ussFileName.c_str()));
		m_ipResultLogger->AddTestCaseFile(get_bstr_t(expectedTextFileName.c_str()));

		// Add a memo that gives the parameters used for this test case
		std::string strSetup = getSettingsAsString();
		m_ipResultLogger->AddTestCaseMemo(
			get_bstr_t("Test Parameters"),
			get_bstr_t(strSetup.c_str()));
		
		try
		{
			validateFileOrFolderExistence( expectedTextFileName );
		}
		catch (UCLIDException& ue)
		{
			UCLIDException uexOuter( "ELI07963", 
				"Reference to invalid file name encountered!", ue );
			uexOuter.addDebugInfo( "FileName", expectedTextFileName );
			throw uexOuter;
		}

		// Get Expected text from file
		string expectedText;
		expectedText = getTextFileContentsAsString(expectedTextFileName);

		// This will contain the string that we found
		string foundText = "";

		// Set up and run the searcher
		try
		{
			validateFileOrFolderExistence( ussFileName );
		}
		catch (UCLIDException& ue)
		{
			UCLIDException uexOuter( "ELI19347", 
				"Reference to invalid .uss file name encountered!", ue );
			uexOuter.addDebugInfo( "FileName", ussFileName );
			throw uexOuter;
		}
		ISpatialStringPtr ipSS(CLSID_SpatialString);
		ipSS->LoadFrom(get_bstr_t(ussFileName.c_str()), VARIANT_FALSE);

		long lNumPages = 0;
		ISpatialStringPtr ipPage = ipSS;

		if(ipSS->HasSpatialInfo() == VARIANT_TRUE)
		{
			IIUnknownVectorPtr ipPages = ipSS->GetPages();
			lNumPages = ipPages->Size();
			if (lPageNum > lNumPages || lPageNum < 1)
			{	
				// Create and throw exception
				UCLIDException ue("ELI08046", "Invalid Page Number." );
				ue.addDebugInfo( "Page Number",  strPageNum);
				throw ue;
			}	
			ipPage = ipPages->At(lPageNum - 1);
		}

		ISpatialStringSearcherPtr ipSearcher(CLSID_SpatialStringSearcher);
		ipSearcher->InitSpatialStringSearcher(ipPage);

		if (m_bIncludeDataOnBoundary)
		{
			ipSearcher->SetIncludeDataOnBoundary(VARIANT_TRUE);
		}
		else
		{
			ipSearcher->SetIncludeDataOnBoundary(VARIANT_FALSE);
		}
	
		ipSearcher->SetBoundaryResolution(m_eBoundaryDataResolution);

		ILongRectanglePtr ipRect;
		ipRect.CreateInstance(CLSID_LongRectangle);
		ipRect->SetBounds(m_lLeft, m_lTop, m_lRight, m_lBottom);

		ISpatialStringPtr ipNewStr;
		if (m_bGetDataInRegion)
		{
			// Do not rotate the rectangle per the OCR
			ipNewStr = ipSearcher->GetDataInRegion( ipRect, VARIANT_FALSE );
		}
		else
		{
			ipNewStr = ipSearcher->GetDataOutOfRegion(ipRect);
		}

		// Get Found Text
		foundText = asString(ipNewStr->String);

		//Add Compare Data to the tree and the dialog
		m_ipResultLogger->AddTestCaseCompareData("Compare Attributes",
												 "Expected String", get_bstr_t(expectedText.c_str()),
												 "Found String", get_bstr_t(foundText.c_str()));
		// Compare Expected to Found
		if (expectedText == foundText)
		{
			bSuccess = true;
		}
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI07972", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);
	
	
	VARIANT_BOOL bRet = asVariantBool(bSuccess && !bExceptionCaught);

	m_ipResultLogger->EndTestCase(bRet); 
	m_lCurrTestCase++;
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcherTester::processSet(std::vector<std::string>& vecTokens, const std::string& strDatFile)
{
	if (vecTokens.size() < 2)
	{	
		// Create and throw exception
		UCLIDException ue("ELI07933", "Unable to process line." );
		//ue.addDebugInfo( "Line Text", strLine );
		throw ue;
	}
	unsigned int ui;
	for (ui = 1; ui < vecTokens.size(); ui++)
	{
		if(vecTokens[ui].empty())
			continue;
		vector<string> vecSubTokens;
		StringTokenizer::sGetTokens( vecTokens[ui], '=', vecSubTokens );
		string setTokType = vecSubTokens[0];
		if (setTokType == "INCLUDE_DATA_ON_BOUNDARY")
		{
			m_bIncludeDataOnBoundary = true;
		}
		else if (setTokType == "EXCLUDE_DATA_ON_BOUNDARY")
		{
			m_bIncludeDataOnBoundary = false;
		}
		else if (setTokType == "GET_DATA_IN_REGION")
		{
			m_bGetDataInRegion = true;
		}
		else if (setTokType == "GET_DATA_OUTOF_REGION")
		{
			m_bGetDataInRegion = false;
		}
		else if (setTokType == "BOUNDARY_DATA_TYPE")
		{
			if (vecSubTokens.size() < 2)
			{	
				// Create and throw exception
				UCLIDException ue("ELI07935", "Unable to process line." );
				throw ue;
			}
			if (vecSubTokens[1] == "CHAR")
			{
				m_eBoundaryDataResolution = kCharacter;
			}
			else if (vecSubTokens[1] == "WORD")
			{
				m_eBoundaryDataResolution = kWord;
			}
		}
		else if (setTokType == "BOUNDS")
		{
			if (vecSubTokens.size() < 2)
			{	
				// Create and throw exception
				UCLIDException ue("ELI19345", "Unable to process line." );
				throw ue;
			}
			vector<string> vecBounds;
			StringTokenizer::sGetTokens( vecSubTokens[1], ",", vecBounds );
			if (vecBounds.size() != 4)
			{
				// Create and throw exception
				UCLIDException ue("ELI07937", "Unable to process line." );
				throw ue;
			}
			m_lLeft = asLong(vecBounds[0]);
			m_lTop = asLong(vecBounds[1]);
			m_lRight = asLong(vecBounds[2]);
			m_lBottom = asLong(vecBounds[3]);
		}
		else
		{
			// Create and throw exception
			UCLIDException ue("ELI07939", "Unable to process line." );
			ue.addDebugInfo( "Text", setTokType );
			throw ue;
		}
	}
}

std::string CSpatialStringSearcherTester::getSettingsAsString()
{

	string strTestParams = "";
	if (m_bIncludeDataOnBoundary)
	{
		strTestParams += "<SET>;INCLUDE_DATA_ON_BOUNDARY\r\n";
	}
	else
	{
		strTestParams += "<SET>;EXCLUDE_DATA_ON_BOUNDARY\r\n";
	}

	if (m_eBoundaryDataResolution == kCharacter)
	{
		strTestParams += "<SET>;BOUNDARY_DATA_TYPE=CHAR\r\n";
	}
	else if (m_eBoundaryDataResolution == kWord)
	{
		strTestParams += "<SET>;BOUNDARY_DATA_TYPE=WORD\r\n";
	}
	else 
	{
		strTestParams += "<SET>;BOUNDARY_DATA_TYPE=ERROR:Undefined\r\n";
	}

	strTestParams += "<SET>;BOUNDS=";
	strTestParams += asString(m_lLeft); 
	strTestParams += ",";
	strTestParams += asString(m_lTop); 
	strTestParams += ",";
	strTestParams += asString(m_lRight); 
	strTestParams += ",";
	strTestParams += asString(m_lBottom); 
	strTestParams += "\r\n";

	if(m_bGetDataInRegion)
	{
		strTestParams += "<SET>;GET_DATA_IN_REGION\r\n";
	}
	else
	{
		strTestParams += "<SET>;GET_DATA_OUTOF_REGION\r\n";
	}
	return strTestParams;
}


