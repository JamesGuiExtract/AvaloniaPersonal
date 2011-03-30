// AFEngineTester.cpp : Implementation of CAFEngineTester
#include "stdafx.h"
#include "AFCoreTest.h"
#include "AFEngineTester.h"

#include <UCLIDException.h>

#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
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

using namespace std;

// import output handlers type library
#import "..\..\..\AFOutputHandlers\Code\AFOutputHandlers.tlb" named_guids 
using namespace UCLID_AFOUTPUTHANDLERSLib;

//-------------------------------------------------------------------------------------------------
// CAFEngineTester
//-------------------------------------------------------------------------------------------------
CAFEngineTester::CAFEngineTester()
:	m_ipResultLogger(NULL),
	m_ipAttrFinderEngine(CLSID_AttributeFinderEngine),
	m_ipAFUtility(CLSID_AFUtility)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI07196", m_ipAttrFinderEngine != __nullptr);
		ASSERT_RESOURCE_ALLOCATION("ELI07365", m_ipAFUtility != __nullptr);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07195")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineTester::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITestableComponent,
		&IID_ILicensedComponent
	};
	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		if (m_ipResultLogger == __nullptr)
		{
			throw UCLIDException("ELI07186", "Please set ResultLogger before proceeding.");
		}

		std::string strTCLFilename = asString(strTCLFile);

		// load the expected attributes from the eav file
		const string strImageFilePath = getTestFileFullPath("AFETest01.tif", pParams, strTCLFilename);

		// create constant strings with the various image names
		const string strRSDFullPath = getTestFileFullPath("AFETest.rsd", pParams, strTCLFilename);
		const string strRSDValidateFullPath = getTestFileFullPath("AFETestValidate.rsd", pParams, strTCLFilename);
		const string strTXTFilePath = strImageFilePath + ".txt";
		const string strUSSFilePath = strImageFilePath + ".uss";
		
		// load expected results
		string strEAVFile = strImageFilePath + ".eav";
		IIUnknownVectorPtr ipExpectedAttributes = 
			m_ipAFUtility->GenerateAttributesFromEAVFile(get_bstr_t(strEAVFile.c_str()));
		
		// the file AFETest.rsd is expected to have 3 attributes: page1, page2, page3 with values from
		// the indicated page,
		runTest(kAutoFile, strRSDFullPath, strImageFilePath, "Case 1", ipExpectedAttributes);
		runTest(kAutoFile, strRSDFullPath, strTXTFilePath, "Case 2", ipExpectedAttributes);
		runTest(kAutoFile, strRSDFullPath, strUSSFilePath, "Case 3", ipExpectedAttributes);
		runTest(kImageFile, strRSDFullPath, strImageFilePath, "Case 4", ipExpectedAttributes);
		runTest(kTextFile, strRSDFullPath, strTXTFilePath, "Case 5", ipExpectedAttributes);
		runTest(kUSSFile, strRSDFullPath, strUSSFilePath, "Case 6", ipExpectedAttributes);
		runTest(kText, strRSDFullPath, strTXTFilePath, "Case 7", ipExpectedAttributes);

		// Load validated expected results
		strEAVFile = getTestFileFullPath("AFETest01val.tif.eav", pParams, strTCLFilename);
		ipExpectedAttributes = 
			m_ipAFUtility->GenerateAttributesFromEAVFile(get_bstr_t(strEAVFile.c_str()));

		// Run Tests with validate Output
		runTest(kAutoFile, strRSDValidateFullPath, strImageFilePath, "Case 8", 
			ipExpectedAttributes, true);
		runTest(kAutoFile, strRSDValidateFullPath, strTXTFilePath, "Case 9", 
			ipExpectedAttributes, true);
		runTest(kAutoFile, strRSDValidateFullPath, strUSSFilePath, "Case 10", 
			ipExpectedAttributes, true);
		runTest(kImageFile, strRSDValidateFullPath, strImageFilePath, "Case 11", 
			ipExpectedAttributes, true);
		runTest(kTextFile, strRSDValidateFullPath, strTXTFilePath, "Case 12", 
			ipExpectedAttributes, true);
		runTest(kUSSFile, strRSDValidateFullPath, strUSSFilePath, "Case 13", 
			ipExpectedAttributes, true);
		runTest(kText, strRSDValidateFullPath, strTXTFilePath, "Case 14", 
			ipExpectedAttributes, true);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07187")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineTester::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// Check license
		validateLicense();

		m_ipResultLogger = pLogger;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07317")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFEngineTester::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Helper Functions
//-------------------------------------------------------------------------------------------------
void CAFEngineTester::recordStartOfTestCase(const string& strCaseDescriptionBase,
											const string& strCaseNo, 
											const int nSubCaseNumber, 
											const bool bValidateOutput)
{
	// format a string with the case number and sub-case number
	CString zSubCaseString;
	zSubCaseString.Format("%s_%d", strCaseNo.c_str(), nSubCaseNumber);
	string strCaseNoWithSubCase = zSubCaseString;
	
	// compute case description
	string strCaseDescription = strCaseDescriptionBase;
	if ( bValidateOutput ) 
	{
		strCaseDescription += " with Validate";
	}
	else
	{
		strCaseDescription += " without Validate";
	}

	// record start of test case
	m_ipResultLogger->StartTestCase(get_bstr_t(strCaseNoWithSubCase.c_str()), 
					get_bstr_t(strCaseDescription.c_str()), kAutomatedTestCase); 	
}
//-------------------------------------------------------------------------------------------------
void CAFEngineTester::addTestCaseMemoWithAttr(const string& strTitle, 
											  const IIUnknownVectorPtr& ipAttributes)
{
	string strAttributes = m_ipAFUtility->GetAttributesAsString(ipAttributes);

	// Add Test Case Memo for Expected Attributes
	m_ipResultLogger->AddTestCaseMemo( 
		get_bstr_t( strTitle.c_str() ),
		get_bstr_t( strAttributes.c_str() ) );
}
//-------------------------------------------------------------------------------------------------
void CAFEngineTester::processResults(IIUnknownVectorPtr ipExpectedAttr, 
									 IIUnknownVectorPtr ipFoundAttr,
									 bool& rbSuccess)
{
	// ensure valid arguments
	ASSERT_ARGUMENT("ELI07330", ipFoundAttr != __nullptr);
	ASSERT_ARGUMENT("ELI07331", ipExpectedAttr != __nullptr);

	// handle the situation where no attributes are found
	// handle case of no attributes found but some were expected
	if ((ipFoundAttr->Size() == 0) && (ipExpectedAttr->Size() > 0))
	{
		// add test case memo with expected attributes
		addTestCaseMemoWithAttr("Expected attributes", ipExpectedAttr);

		// Create and throw exception
		UCLIDException uclidException("ELI07214", "No Found Attributes.");
		throw uclidException;
	}

	// Compare actual found attributes with expected attributes
	rbSuccess = false;

	// Get IComparableObject pointers for each vector
	IComparableObjectPtr	ipFound = ipFoundAttr;
	ASSERT_RESOURCE_ALLOCATION("ELI07325", ipFound != __nullptr);

	IComparableObjectPtr	ipExpected = ipExpectedAttr;
	ASSERT_RESOURCE_ALLOCATION("ELI07326", ipExpected != __nullptr);
	
	// Do the comparison
	// if not equal, list all expected and found attributes
	if (ipFound->IsEqualTo( ipExpected ) == VARIANT_TRUE)
	{
		// Set flag
		rbSuccess = true;
	}

	// Always add Test Case Memos with the found and expected attributes
	string strExpectedAttr = m_ipAFUtility->GetAttributesAsString(ipExpectedAttr);
	string strFoundAttr = m_ipAFUtility->GetAttributesAsString(ipFoundAttr);

	//Add Compare Attributes to the tree and test logger window
	m_ipResultLogger->AddTestCaseCompareData("Compare Attributes",
											 _bstr_t("Expected Attributes"), _bstr_t(strExpectedAttr.c_str()), 
											 _bstr_t("Found Attributes"), _bstr_t(strFoundAttr.c_str()));
}
//-------------------------------------------------------------------------------------------------
void CAFEngineTester::runTest(ETestType eTestType, const string& strRSDFile, 
							  const string& strInputFileName, const string& strCaseNo, 
							  IIUnknownVectorPtr ipExpectedAttr, const bool bValidateOutput, 
							  int nSubCaseNumber, IVariantVectorPtr ipAttributeNames, 
							  bool bRecurse, int nPagesToRecognize)
{
	// ensure that input file is valid
	if (strInputFileName.size() == 0 ) 
	{
		UCLIDException ue("ELI07216", "No input file to test." );
		throw ue;
	}

	// compute the case description based upon ETestType
	string strCaseDescriptionBase;
	switch (eTestType)
	{
	case kAutoFile:
		strCaseDescriptionBase = "Testing FindAttributesInAutoFile";
		break;
	case kImageFile:
		strCaseDescriptionBase = "Testing FindAttributesInImageFile";
		break;
	case kTextFile:
		strCaseDescriptionBase = "Testing FindAttributesInTextFile";
		break;
	case kUSSFile:
		strCaseDescriptionBase = "Testing FindAttributesInUSSFile";
		break;
	case kText:
		strCaseDescriptionBase = "Testing FindAttributesInText";
		break;
	default:
		// we should never reach here
		THROW_LOGIC_ERROR_EXCEPTION("ELI07333")
	}

	// Start test case to test return of all attributes
	recordStartOfTestCase(strCaseDescriptionBase, strCaseNo, 
		nSubCaseNumber, bValidateOutput);
	
	// create the output handler, if desired by caller
//	IOutputHandlerPtr ipOutputHandler = __nullptr;
//	if ( bValidateOutput )
//	{
//		ipOutputHandler.CreateInstance(CLSID_RemoveInvalidEntries);
//		ASSERT_RESOURCE_ALLOCATION( "ELI07272", ipOutputHandler != __nullptr );
//	}

	bool bSuccess = false;
	bool bExceptionCaught = false;

	try
	{
		// NOTE: use of AddTestCaseFile()
		m_ipResultLogger->AddTestCaseFile(get_bstr_t(strRSDFile.c_str()));
		m_ipResultLogger->AddTestCaseFile(get_bstr_t(strInputFileName.c_str()));

		// Create AFDocument object
		CComQIPtr<IAFDocument> ipAFDoc;
		ipAFDoc.CoCreateInstance( CLSID_AFDocument );
		ASSERT_RESOURCE_ALLOCATION( "ELI07841", ipAFDoc != __nullptr );

		// find the attributes
		IIUnknownVectorPtr ipFoundAttr;
		switch (eTestType)
		{
		case kAutoFile: 
			ipFoundAttr = m_ipAttrFinderEngine->FindAttributes( ipAFDoc, 
				get_bstr_t( strInputFileName.c_str() ), -1, get_bstr_t( strRSDFile.c_str() ), 
				ipAttributeNames, VARIANT_FALSE, NULL );
			break;

		case kImageFile:
			ipFoundAttr = m_ipAttrFinderEngine->FindAttributes( ipAFDoc, 
				get_bstr_t( strInputFileName.c_str() ), nPagesToRecognize, 
				get_bstr_t( strRSDFile.c_str() ), ipAttributeNames, 
				VARIANT_FALSE, NULL );
			break;

		case kTextFile:
			ipFoundAttr = m_ipAttrFinderEngine->FindAttributes( ipAFDoc, 
				get_bstr_t( strInputFileName.c_str() ), 0, get_bstr_t( strRSDFile.c_str() ), 
				ipAttributeNames, VARIANT_FALSE, NULL );
			break;

		case kUSSFile:
			ipFoundAttr = m_ipAttrFinderEngine->FindAttributes( ipAFDoc, 
				get_bstr_t( strInputFileName.c_str() ), 0, get_bstr_t( strRSDFile.c_str() ), 
				ipAttributeNames, VARIANT_FALSE, NULL );
			break;

		case kText:
			{
				// Load text into the IAFDocument
				ISpatialStringPtr ipInputText = ipAFDoc->Text;
				ipInputText->LoadFrom(get_bstr_t(strInputFileName.c_str()), VARIANT_FALSE);

				ipFoundAttr = m_ipAttrFinderEngine->FindAttributes( ipAFDoc, 
					get_bstr_t( "" ), 0, get_bstr_t( strRSDFile.c_str() ), 
					ipAttributeNames, VARIANT_TRUE, NULL );
			}
			break;
		default:
			// we should never reach here
			THROW_LOGIC_ERROR_EXCEPTION("ELI07334");
		}

		// process results
		processResults(ipExpectedAttr, ipFoundAttr, bSuccess);
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI07215", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	// Check for success
	bSuccess = bSuccess && !bExceptionCaught;
	
	// end the test case
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));

	// if the caller didn't request recursion, just return;
	if (!bRecurse)
	{
		return;
	}

	// Start test case to return individual attributes
	
	// vector to contain the list of attributes to find
	IVariantVectorPtr ipvecAttributes(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI07286", ipvecAttributes != __nullptr);

	// vector to contain the list of values expected
	IIUnknownVectorPtr ipExpected(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI07219", ipExpected != __nullptr );

	string strLastAttributeName = "";

	// assumes the attributes are named page1, page2 and page3
	for ( int n = 0; n < 3; n++)
	{
		CString zAttributeName;
		zAttributeName.Format("page%d", n+1);
		string strAttributeName = zAttributeName;

		// move all the expected values for the attributes to find to the 
		// expected vector
		for ( int i = 0; i < ipExpectedAttr->Size(); i++ )
		{
			IAttributePtr ipCurrAttribute = ipExpectedAttr->At(i);
			if ( get_bstr_t(strAttributeName.c_str()) == ipCurrAttribute->Name) 
			{
				ipExpected->PushBack(ipCurrAttribute);
			}
		}

		// add attribute to list of attributes to find
		ipvecAttributes->PushBack(get_bstr_t(strAttributeName.c_str()));

		// record start of next sub-case
		nSubCaseNumber++;

		runTest(eTestType, strRSDFile, strInputFileName, strCaseNo, ipExpected,
			bValidateOutput, nSubCaseNumber, ipvecAttributes, false);

		// Run the image page subcases
		if ( eTestType == kImageFile )
		{
			// record start of next sub-case
			nSubCaseNumber++;
	
			// Run the image test to recognize n+1 pages
			runTest(eTestType, strRSDFile, strInputFileName, strCaseNo, ipExpected,
				bValidateOutput, nSubCaseNumber, NULL, false, n+1 );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CAFEngineTester::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI07316", "Attribute Finder Engine Tester" );
}
//-------------------------------------------------------------------------------------------------
const std::string CAFEngineTester::getTestFileFullPath(const string& strFileName, IVariantVectorPtr ipParams, const std::string &strTCLFile) const
{
	// if pParams is not empty and the second item is specified,
	// then the second item is the master dat file
	if ((ipParams != __nullptr) && (ipParams->Size > 1))
	{
		// get the DAT filename
		string strTestFileName = ::getAbsoluteFileName(strTCLFile, asString(_bstr_t(ipParams->GetItem(1))) + "\\" + strFileName, true);

		// if no folder was specified, use the TCL file location
		if(strTestFileName.empty() || (getFileNameFromFullPath(strTestFileName) == ""))
		{
			// Create and throw exception
			UCLIDException ue("ELI12280", "Required test file not found!");
			ue.addDebugInfo("Folder", getDirectoryFromFullPath(strTestFileName));
			ue.addDebugInfo("FileName", strTestFileName);
			throw ue;	
		}

		return strTestFileName;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue("ELI12281", "Required test file folder not specified in TCL file!");
		throw ue;	
	}
}
//-------------------------------------------------------------------------------------------------
