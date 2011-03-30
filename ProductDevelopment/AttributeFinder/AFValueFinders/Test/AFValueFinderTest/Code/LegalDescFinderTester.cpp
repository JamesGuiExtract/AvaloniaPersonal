// LegalDescFinderTester.cpp : Implementation of CLegalDescFinderTester
#include "stdafx.h"
#include "AFValueFinderTest.h"
#include "LegalDescFinderTester.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>

#include <io.h>
#include <stdio.h>
#include <fstream>

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CLegalDescFinderTester
//-------------------------------------------------------------------------------------------------
CLegalDescFinderTester::CLegalDescFinderTester()
: m_ipResultLogger(NULL),
  m_ipLegalDescFinder(CLSID_LegalDescriptionFinder)
{
	try
	{
		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05751")
}
//-------------------------------------------------------------------------------------------------
CLegalDescFinderTester::~CLegalDescFinderTester()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16337");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescFinderTester::InterfaceSupportsErrorInfo(REFIID riid)
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
STDMETHODIMP CLegalDescFinderTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		std::string strTestFile = getMasterTestFileName( pParams, asString( strTCLFile ) );

		// Each line has an image file name, read in the image file
		// name and process it through the OCR engine
		ifstream ifs(strTestFile.c_str());

		// instantiate a new OCR engine
		IOCREnginePtr ipOCREngine = getOCREngine();

		string strLine("");
		CString zCaseNo("");
		CommentedTextFileReader fileReader(ifs, "//", true);
		static long nCaseNo = 1;
		do
		{
			strLine = fileReader.getLineText();
			if (strLine.empty()) continue;

			zCaseNo.Format("TestCase_%ld", nCaseNo);
			nCaseNo++;

			// test case result, fail or pass
			VARIANT_BOOL bRet = VARIANT_TRUE;

			// Initiate a test case
			m_ipResultLogger->StartTestCase(_bstr_t(zCaseNo), _bstr_t("Test Legal Description Finder"), kAutomatedTestCase); 
			
			try
			{
				// each line is an image file name
				string strImageFileName(strLine);
				// Add note : image file that is going to be processed
				m_ipResultLogger->AddTestCaseNote(_bstr_t(strImageFileName.c_str()));

				// process the image file
				ISpatialStringPtr ipText;
				ipText = ipOCREngine->RecognizeTextInImage(
					strImageFileName.c_str(), 1, -1, kNoFilter, "", kRegistry, VARIANT_TRUE, NULL);
				ASSERT_RESOURCE_ALLOCATION("ELI06788", ipText != __nullptr);
				_bstr_t _bstrOutput = ipText->String;
				
				m_ipResultLogger->AddTestCaseMemo(_bstr_t("Recognized text"), _bstrOutput);
				IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
				ISpatialStringPtr ipInputText(CLSID_SpatialString);
				ASSERT_RESOURCE_ALLOCATION("ELI25938", ipInputText != __nullptr);
				ipInputText->CreateNonSpatialString(_bstrOutput, "");
				ipAFDoc->Text = ipInputText;
				// use legal description finder to get the legal description
				IIUnknownVectorPtr ipLegalDescriptions = m_ipLegalDescFinder->ParseText(ipAFDoc, NULL);
				if (ipLegalDescriptions!=NULL)
				{
					long nSize = ipLegalDescriptions->Size();
					for (long n=0; n<nSize; n++)
					{
						// Retrieve this Attribute
						IAttributePtr ipAttr = ipLegalDescriptions->At(n);
						ASSERT_RESOURCE_ALLOCATION("ELI15585", ipAttr != __nullptr);

						// Retrieve the Value
						ISpatialStringPtr ipValue = ipAttr->Value;
						ASSERT_RESOURCE_ALLOCATION("ELI15586", ipValue != __nullptr);
						_bstr_t _bstrLegalDesc = ipValue->String;
						
						// add the found legal descriptions to the detail note
						m_ipResultLogger->AddTestCaseMemo(_bstr_t("Found legal description"), 
							_bstrLegalDesc);
					}
				}
			}
			catch (UCLIDException& ue)
			{
				ue.addDebugInfo("Case No", string(zCaseNo));
				string strError(ue.asStringizedByteStream());
				m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()), VARIANT_FALSE);
				bRet = VARIANT_FALSE;
			}
			catch (_com_error& e)
			{
				UCLIDException ue;
				_bstr_t _bstrDescription = e.Description();
				char *pszDescription = _bstrDescription;
				if (pszDescription)
				{
					ue.createFromString("ELI05748", pszDescription);
				}
				else
				{
					ue.createFromString("ELI05749", "COM exception caught!");
				}
				ue.addDebugInfo("Case No", string(zCaseNo));
				string strError(ue.asStringizedByteStream());
				m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()), VARIANT_FALSE);
				bRet = VARIANT_FALSE;
			}
			catch (...)
			{
				UCLIDException uclidException("ELI05750", "Unknown Exception is caught.");
				uclidException.addDebugInfo("Case No", string(zCaseNo));
				uclidException.addDebugInfo("Line Text", strLine);
				string strError(uclidException.asStringizedByteStream());
				m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()), VARIANT_FALSE);
				bRet = VARIANT_FALSE;
			}

			// end the test case
			m_ipResultLogger->EndTestCase(bRet);
		}
		while(!ifs.eof());

		// set case number back to 1 since we finish testing one file.
		nCaseNo = 1;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05745")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescFinderTester::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescFinderTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLegalDescFinderTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private / helper methods
//-------------------------------------------------------------------------------------------------
const std::string CLegalDescFinderTester::getMasterTestFileName(IVariantVectorPtr ipParams, const std::string &strTCLFile) const
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
			UCLIDException ue("ELI11895", "Required master testing .DAT file not found!");
			throw ue;
		}

		return strMasterDatFileName;
	}
	else
	{
		// Create and throw exception
		UCLIDException ue("ELI11897", "Required master testing .DAT file not specified in TCL file!");
		throw ue;	
	}
}
//-------------------------------------------------------------------------------------------------
IOCREnginePtr CLegalDescFinderTester::getOCREngine()
{
	// create a new instance of the SSOCR recognition engine [P13 #2909]
	IOCREnginePtr ipOCREngine(CLSID_ScansoftOCR);
	ASSERT_RESOURCE_ALLOCATION("ELI16149", ipOCREngine != __nullptr);

	// license the OCR engine
	IPrivateLicensedComponentPtr ipScansoftOCREngine = ipOCREngine;
	ASSERT_RESOURCE_ALLOCATION("ELI16150", ipScansoftOCREngine != __nullptr);
	ipScansoftOCREngine->InitPrivateLicense(LICENSE_MGMT_PASSWORD.c_str());

	return ipOCREngine;
}
//-------------------------------------------------------------------------------------------------