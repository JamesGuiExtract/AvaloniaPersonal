
#include "stdafx.h"
#include "SpotRecIRAutoTest.h"
#include "SpotRecIRTester.h"

#include <cpputil.h>
#include <comutils.h>
#include <UCLIDException.h>

#include <io.h>
#include <string>
using namespace std;

const unsigned long gulDELAY = 1000;

//--------------------------------------------------------------------------------------------------
CSpotRecIRTester::CSpotRecIRTester()
{
	try
	{
		// create an instance of the spot-rec IR
		m_ipSpotRecIR.CreateInstance(__uuidof(SpotRecognitionWindow));
		ASSERT_RESOURCE_ALLOCATION("ELI12321", m_ipSpotRecIR != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI12323");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecIRTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	setTestFileFolder(pParams, asString(strTCLFile));

	runProperInitializationTest();
	runEnableInputTest();
	runHasWindowTest();
	runShowWindowTest();
	runComponentDescriptionTest();
	runOpenImageTest();
	runWindowHandleTest();
	runPageNumberTest();
	runOpenGDDFileTest();

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecIRTester::raw_RunInteractiveTests()
{
	try
	{
		UCLIDException ue("ELI12062", "Interactive test for this component are disabled!");
		throw ue;

		// open the helper app
		string strHelperApp = "";
		//	strHelperApp += "\\Engineering\\ProductDevelopment\\InputFunnel\\InputReceivers\\SpotRecognitionIR\\Test\\SpotRecIRAutoTest\\TestHelperApps\\VBTest\\TestSpotRecIR.exe";
		runEXE(strHelperApp, "");

		// bring up the interactive test case executer
		string strITCFile = m_strInteractiveTestFilesFolder;
		strITCFile += "\\spotrec.itc";
		m_ipITCExecuter->ExecuteITCFile(_bstr_t(strITCFile.c_str()));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30417");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecIRTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	m_ipLogger = pLogger;
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpotRecIRTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	m_ipITCExecuter = pInteractiveTestExecuter;
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CSpotRecIRTester::runProperInitializationTest()
{
	// this test is the first automated test that is run.  It ensures that the spot-rec
	// component comes up to the correct initial state
	
	bool bSomeOperationFailed = false;

	_bstr_t bstrID("atc00");
	_bstr_t bstrDescription("Proper initialization test");

	// notify start of test case
	m_ipLogger->StartTestCase(bstrID, bstrDescription, kAutomatedTestCase);
	
	// ensure that by default, the window is not shown.
	VARIANT_BOOL vbWindowShown;
	IInputReceiverPtr ipInputReceiver = m_ipSpotRecIR;
	HRESULT hr = ipInputReceiver->get_WindowShown(&vbWindowShown);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_WindowShown()!"));
		bSomeOperationFailed = true;
	}
	else if (vbWindowShown == VARIANT_TRUE) // ensure that the window is hidden
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Image Viewer is shown before ShowWindow(TRUE) is called!"));
		bSomeOperationFailed = true;
	}

	// ensure that no image is currently loaded
	_bstr_t _bstrExpectedImageFileName = "";
	_bstr_t _bstrActualImageFileName;
	_bstrActualImageFileName = m_ipSpotRecIR->GetImageFileName();
	if (_bstrExpectedImageFileName != _bstrActualImageFileName)
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Upon initialization, GetImageFileName() returned a non-empty string!"));
		bSomeOperationFailed = true;
	}

	// ensure that no GDD file is currently loaded
	_bstr_t _bstrExpectedGDDFileName = "";
	_bstr_t _bstrActualGDDFileName;
	_bstrActualGDDFileName = m_ipSpotRecIR->GetGDDFileName();
	if (_bstrExpectedGDDFileName != _bstrActualGDDFileName)
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Upon initialization, GetGDDFileName() returned a non-empty string!"));
		bSomeOperationFailed = true;
	}

	// notify end of test case
	m_ipLogger->EndTestCase(bSomeOperationFailed ? VARIANT_FALSE : VARIANT_TRUE);

}
//--------------------------------------------------------------------------------------------------
void CSpotRecIRTester::runEnableInputTest()
{
	bool bSomeOperationFailed = false;

	_bstr_t bstrID("atc01");
	_bstr_t bstrDescription("Enabling/Disabling Input test");

	// notify start of test case
	m_ipLogger->StartTestCase(bstrID, bstrDescription, kAutomatedTestCase);
	
	// enable input
	_bstr_t bstrInputType("Text");
	_bstr_t bstrPrompt("Please select some text");

	IInputReceiverPtr ipInputReceiver = m_ipSpotRecIR;
	HRESULT hr = ipInputReceiver->EnableInput(bstrInputType, bstrPrompt);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call EnableInput()!"));
		bSomeOperationFailed = true;
	}

	// check to see if input is enabled
	VARIANT_BOOL vbEnabled;
	ipInputReceiver->get_InputIsEnabled(&vbEnabled);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_InputIsEnabled()!"));
		bSomeOperationFailed = true;
	}
	else if (vbEnabled == VARIANT_FALSE)
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("EnableInput() called, but InputIsEnabled() is returning false!"));
		bSomeOperationFailed = true;
	}
	
	// disable the input
	ipInputReceiver->DisableInput();
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call DisableInput()!"));
		bSomeOperationFailed = true;
	}

	// check to see if input is disabled
	ipInputReceiver->get_InputIsEnabled(&vbEnabled);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_InputIsEnabled()!"));
		bSomeOperationFailed = true;
	}
	else if (vbEnabled == VARIANT_TRUE)
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("DisableInput() called, but InputIsEnabled() is returning true!"));
		bSomeOperationFailed = true;
	}

	// notify end of test case
	m_ipLogger->EndTestCase(bSomeOperationFailed ? VARIANT_FALSE : VARIANT_TRUE);
}
//--------------------------------------------------------------------------------------------------
void CSpotRecIRTester::runHasWindowTest()
{
	bool bSomeOperationFailed = false;

	_bstr_t bstrID("atc02");
	_bstr_t bstrDescription("HasWindow test");

	// notify start of test case
	m_ipLogger->StartTestCase(bstrID, bstrDescription, kAutomatedTestCase);
	
	// check to see if the spot rec IR has a window
	VARIANT_BOOL vbHasWindow;
	IInputReceiverPtr ipInputReceiver = m_ipSpotRecIR;
	HRESULT hr = ipInputReceiver->get_HasWindow(&vbHasWindow);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_InputIsEnabled()!"));
		bSomeOperationFailed = true;
	}
	else if (vbHasWindow == VARIANT_FALSE) // ensure that the spot-rec IR has a window
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("get_HasWindow() returned false!"));
		bSomeOperationFailed = true;
	}

	// notify end of test case
	m_ipLogger->EndTestCase(bSomeOperationFailed ? VARIANT_FALSE : VARIANT_TRUE);
}
//--------------------------------------------------------------------------------------------------
void CSpotRecIRTester::runShowWindowTest()
{
	bool bSomeOperationFailed = false;

	_bstr_t bstrID("atc03");
	_bstr_t bstrDescription("ShowWindow test");

	// notify start of test case
	m_ipLogger->StartTestCase(bstrID, bstrDescription, kAutomatedTestCase);
	
	// show the window
	IInputReceiverPtr ipInputReceiver = m_ipSpotRecIR;
	HRESULT hr = ipInputReceiver->ShowWindow(VARIANT_TRUE);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call ShowWindow(TRUE)!"));
		bSomeOperationFailed = true;
	}

	// ensure that the window is currently shown
	VARIANT_BOOL vbWindowShown;
	hr = ipInputReceiver->get_WindowShown(&vbWindowShown);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_WindowShown()!"));
		bSomeOperationFailed = true;
	}
	else if (vbWindowShown == VARIANT_FALSE) // ensure that the window is shown
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("ShowWindow(TRUE) called, but get_WindowShown returned false!"));
		bSomeOperationFailed = true;
	}

	// hide the window
	hr = ipInputReceiver->ShowWindow(VARIANT_FALSE);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call ShowWindow(FALSE)!"));
		bSomeOperationFailed = true;
	}

	// ensure that the window is currently hidden
	hr = ipInputReceiver->get_WindowShown(&vbWindowShown);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_WindowShown()!"));
		bSomeOperationFailed = true;
	}
	else if (vbWindowShown == VARIANT_TRUE) // ensure that the window is hidden
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("ShowWindow(FALSE) called, but get_WindowShown returned true!"));
		bSomeOperationFailed = true;
	}

	// notify end of test case
	m_ipLogger->EndTestCase(bSomeOperationFailed ? VARIANT_FALSE : VARIANT_TRUE);
}
//--------------------------------------------------------------------------------------------------
void CSpotRecIRTester::runComponentDescriptionTest()
{
	bool bSomeOperationFailed = false;

	_bstr_t bstrID("atc04");
	_bstr_t bstrDescription("Component description test");

	// notify start of test case
	m_ipLogger->StartTestCase(bstrID, bstrDescription, kAutomatedTestCase);
	
	// check to see that the spot-rec ir does have a component description
	ICategorizedComponentPtr ipComponent(m_ipSpotRecIR);
	_bstr_t _bstrComponentDescription(ipComponent->GetComponentDescription());
	string stdstrComponentDescription(_bstrComponentDescription);
	if (stdstrComponentDescription == "")
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("getComponentDescription() returned empty string!"));
		bSomeOperationFailed = true;
	}

	// notify end of test case
	m_ipLogger->EndTestCase(bSomeOperationFailed ? VARIANT_FALSE : VARIANT_TRUE);
}
//--------------------------------------------------------------------------------------------------
void CSpotRecIRTester::runOpenImageTest()
{
	bool bSomeOperationFailed = false;

	_bstr_t bstrID("atc05");
	_bstr_t bstrDescription("Open image test");

	// notify start of test case
	m_ipLogger->StartTestCase(bstrID, bstrDescription, kAutomatedTestCase);
	
	// show the window so that a developer can see what is going on
	IInputReceiverPtr ipInputReceiver = m_ipSpotRecIR;
	ipInputReceiver->ShowWindow(VARIANT_TRUE);

	// try to open a single page tiff
	string strImageFile1 = m_strAutomatedTestFilesFolder;
	strImageFile1 += "\\other.tif";
	_bstr_t _bstrImageFile1 = strImageFile1.c_str();
	
	HRESULT hr = m_ipSpotRecIR->OpenImageFile(_bstrImageFile1);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("OpenImageFile() call for a single-page image failed!"));
		bSomeOperationFailed = true;
	}
	else // ensure that file was opened
	{
		_bstr_t _bstrActualImageFileName = m_ipSpotRecIR->GetImageFileName();
		if (_bstrActualImageFileName != _bstrImageFile1)
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("OpenImageFile() did not work for a single-page image!"));
			bSomeOperationFailed = true;
		}
	}

	// for developer sake, keep the window in its current state for some time
	Sleep(gulDELAY);

	// try to open a multipage tiff
	string strImageFile2 = m_strAutomatedTestFilesFolder;
	strImageFile2 += "\\multipage.tif";
	_bstr_t _bstrImageFile2 = strImageFile2.c_str();
	hr = m_ipSpotRecIR->OpenImageFile(_bstrImageFile2);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("OpenImageFile() call for a multi-page image failed!"));
		bSomeOperationFailed = true;
	}
	else // ensure that file was opened
	{
		_bstr_t _bstrActualImageFileName = m_ipSpotRecIR->GetImageFileName();
		if (_bstrActualImageFileName != _bstrImageFile2)
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("OpenImageFile() did not work for a multi-page image!"));
			bSomeOperationFailed = true;
		}
	}


	// for developer sake, keep the window in its current state for some time
	Sleep(gulDELAY);

	// clear the contents
	hr = m_ipSpotRecIR->Clear();
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Clear() call failed!"));
		bSomeOperationFailed = true;
	}
	else // ensure that file was opened
	{
		_bstr_t _bstrActualImageFileName = m_ipSpotRecIR->GetImageFileName();
		if (_bstrActualImageFileName != _bstr_t(""))
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("Clear() called, but GetImageFileName() returns a non-empty string!"));
			bSomeOperationFailed = true;
		}
	}


	// for developer sake, keep the window in its current state for some time
	Sleep(gulDELAY);

	// close the window
	ipInputReceiver->ShowWindow(VARIANT_FALSE);

	// notify end of test case
	m_ipLogger->EndTestCase(bSomeOperationFailed ? VARIANT_FALSE : VARIANT_TRUE);
}
//--------------------------------------------------------------------------------------------------
void CSpotRecIRTester::runWindowHandleTest()
{
	bool bSomeOperationFailed = false;

	_bstr_t bstrID("atc06");
	_bstr_t bstrDescription("WindowHandle test");

	// notify start of test case
	m_ipLogger->StartTestCase(bstrID, bstrDescription, kAutomatedTestCase);

	// show the input receiver window
	IInputReceiverPtr ipInputReceiver = m_ipSpotRecIR;
	HRESULT hr = ipInputReceiver->ShowWindow(VARIANT_TRUE);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call ShowWindow(TRUE)!"));
		bSomeOperationFailed = true;
	}

	// ensure that the window is shown
	VARIANT_BOOL vbWindowShown;
	hr = ipInputReceiver->get_WindowShown(&vbWindowShown);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_WindowShown()!"));
		bSomeOperationFailed = true;
	}
	else if (vbWindowShown == VARIANT_FALSE) // ensure that the window is shown
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("ShowWindow(TRUE) called, but get_WindowShown returned false!"));
		bSomeOperationFailed = true;
	}

	// get the handle of the IR window
	long lHandle;
	ipInputReceiver->get_WindowHandle(&lHandle);
	HWND hWnd = (HWND) lHandle;
	
	// hide the window associated with the returned handle
	ShowWindow(hWnd, SW_HIDE);

	// ensure that the window is not shown now
	hr = ipInputReceiver->get_WindowShown(&vbWindowShown);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_WindowShown()!"));
		bSomeOperationFailed = true;
	}
	else if (vbWindowShown == VARIANT_TRUE) // ensure that the window is hidden
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Win32 API call ShowWindow(SW_HIDE) called with the handle of the window, but the window is still shown!"));
		bSomeOperationFailed = true;
	}

	// notify end of test case
	m_ipLogger->EndTestCase(bSomeOperationFailed ? VARIANT_FALSE : VARIANT_TRUE);
}
//--------------------------------------------------------------------------------------------------
void CSpotRecIRTester::runPageNumberTest()
{
	bool bSomeOperationFailed = false;

	_bstr_t bstrID("atc07");
	_bstr_t bstrDescription("Page number test");

	// notify start of test case
	m_ipLogger->StartTestCase(bstrID, bstrDescription, kAutomatedTestCase);
	
	// show the window so that a developer can see what is going on
	IInputReceiverPtr ipInputReceiver = m_ipSpotRecIR;
	ipInputReceiver->ShowWindow(VARIANT_TRUE);

	// try to open a single page tiff
	string strImageFile1 = m_strAutomatedTestFilesFolder;
	strImageFile1 += "\\other.tif";
	_bstr_t _bstrImageFile1 = strImageFile1.c_str();
	
	HRESULT hr = m_ipSpotRecIR->OpenImageFile(_bstrImageFile1);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("OpenImageFile() call for a single-page image failed!"));
		bSomeOperationFailed = true;
	}
	else // ensure that file was opened
	{
		_bstr_t _bstrActualImageFileName = m_ipSpotRecIR->GetImageFileName();
		if (_bstrActualImageFileName != _bstrImageFile1)
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("OpenImageFile() did not work for a single-page image!"));
			bSomeOperationFailed = true;
		}
	}

	// check the number of pages in the opened image...
	long lNumPages = m_ipSpotRecIR->GetTotalPages();
	if (lNumPages != 1) // ensure that the image only contains 1 pages
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("GetTotalPages() returned invalid value for a single-page image!"));
		bSomeOperationFailed = true;
	}

	// check the current page number
	long lCurrentPage = m_ipSpotRecIR->GetCurrentPageNumber();
	if (lCurrentPage != 1) // ensure that the current page is page 1
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("GetCurrentPageNumber() returned invalid value for a single-page image!"));
		bSomeOperationFailed = true;
	}

	// for developer sake, keep the window in its current state for some time
	Sleep(gulDELAY);

	// try to open a multipage tiff
	string strImageFile2 = m_strAutomatedTestFilesFolder;
	strImageFile2 += "\\multipage.tif";
	_bstr_t _bstrImageFile2 = strImageFile2.c_str();
	hr = m_ipSpotRecIR->OpenImageFile(_bstrImageFile2);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("OpenImageFile() call for a multi-page image failed!"));
		bSomeOperationFailed = true;
	}
	else // ensure that file was opened
	{
		_bstr_t _bstrActualImageFileName = m_ipSpotRecIR->GetImageFileName();
		if (_bstrActualImageFileName != _bstrImageFile2)
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("OpenImageFile() did not work for a multi-page image!"));
			bSomeOperationFailed = true;
		}
	}

	// check the number of pages in the opened image...
	lNumPages = m_ipSpotRecIR->GetTotalPages();
	if (lNumPages != 3) // ensure that the image only contains 3 pages
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("GetTotalPages() returned invalid value for a multi-page image!"));
		bSomeOperationFailed = true;
	}

	// check the current page number
	lCurrentPage = m_ipSpotRecIR->GetCurrentPageNumber();
	if (lCurrentPage != 1) // ensure that the current page is page 1
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("GetCurrentPageNumber() returned invalid value for a multi-page image!"));
		bSomeOperationFailed = true;
	}

	// for developer sake, keep the window in its current state for some time
	Sleep(gulDELAY);

	// set the current page number to page 2, and verify that the current page is page 2
	m_ipSpotRecIR->SetCurrentPageNumber(2);
	lCurrentPage = m_ipSpotRecIR->GetCurrentPageNumber();
	if (lCurrentPage != 2) // ensure that the current page is page 2
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("GetCurrentPageNumber() returned invalid value for a multi-page image, after a call to SetCurrentPageNumber(2)!"));
		bSomeOperationFailed = true;
	}

	// for developer sake, keep the window in its current state for some time
	Sleep(gulDELAY);

	// set the current page number to page 3, and verify that the current page is page 3
	m_ipSpotRecIR->SetCurrentPageNumber(3);
	lCurrentPage = m_ipSpotRecIR->GetCurrentPageNumber();
	if (lCurrentPage != 3) // ensure that the current page is page 3
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("GetCurrentPageNumber() returned invalid value for a multi-page image, after a call to SetCurrentPageNumber(3)!"));
		bSomeOperationFailed = true;
	}

	// for developer sake, keep the window in its current state for some time
	Sleep(gulDELAY);

	// close the window
	ipInputReceiver->ShowWindow(VARIANT_FALSE);

	// notify end of test case
	m_ipLogger->EndTestCase(bSomeOperationFailed ? VARIANT_FALSE : VARIANT_TRUE);
}
//--------------------------------------------------------------------------------------------------
void CSpotRecIRTester::runOpenGDDFileTest()
{
	bool bSomeOperationFailed = false;

	_bstr_t bstrID("atc08");
	_bstr_t bstrDescription("Open GDD file test");

	// notify start of test case
	m_ipLogger->StartTestCase(bstrID, bstrDescription, kAutomatedTestCase);
	
	// show the window so that a developer can see what is going on
	IInputReceiverPtr ipInputReceiver = m_ipSpotRecIR;
	ipInputReceiver->ShowWindow(VARIANT_TRUE);

	// try to open a single page tiff's gdd file
	string strGDDFile1 = m_strAutomatedTestFilesFolder;
	strGDDFile1 += "\\other.tif.gdd";
	_bstr_t _bstrGDDFile1 = strGDDFile1.c_str();
	
	HRESULT hr = m_ipSpotRecIR->OpenGDDFile(_bstrGDDFile1);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("OpenGDDFile() call for a single-page image's .gdd file failed!"));
		bSomeOperationFailed = true;
	}
	else // ensure that file was opened
	{
		_bstr_t _bstrActualGDDFileName = m_ipSpotRecIR->GetGDDFileName();
		if (_bstrActualGDDFileName != _bstrGDDFile1)
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("OpenGDDFile() did not work for a single-page image!"));
			m_ipLogger->AddTestCaseNote(_bstrGDDFile1);
			m_ipLogger->AddTestCaseNote(_bstrActualGDDFileName);
			bSomeOperationFailed = true;
		}
	}

	// if a file called "other_new.tif.gdd" exists, delete it
	string strNewGDDFile1 = m_strAutomatedTestFilesFolder;
	strNewGDDFile1 += "\\other_new.tif.gdd";
	if (isFileOrFolderValid( strNewGDDFile1 ))
	{
		// delete the file
		try
		{
			deleteFile(strNewGDDFile1, true);
		}
		catch(UCLIDException& uex)
		{
			uex.log();
			m_ipLogger->AddTestCaseNote(_bstr_t("Unable to delete file other_new.tif.gdd!"));
			bSomeOperationFailed = true;
		}
	}

	// test the saveAs method
	_bstr_t _bstrNewGDDFile1 = strNewGDDFile1.c_str();
	hr = m_ipSpotRecIR->SaveAs(_bstrNewGDDFile1);
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("SaveAs() call for a single-page image failed!"));
		bSomeOperationFailed = true;
	}
	else // ensure that file was saved
	{
		if (!isFileOrFolderValid( strNewGDDFile1 ))
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("SaveAs() did not actually save a file for a single-page image!"));
			bSomeOperationFailed = true;
		}

		// TODO: need to compare actual contents of saved file with expected contents
		
		// check to see if the returned GDD file name is the name passed in to the
		// SaveAs method
		_bstr_t _bstrActualGDDFileName = m_ipSpotRecIR->GetGDDFileName();
		_bstr_t _bstrExpectedGDDFileName = strNewGDDFile1.c_str();
		if (_bstrActualGDDFileName != _bstrExpectedGDDFileName)
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("GetGDDFileName() call failed after a call to SaveAs()!"));
			bSomeOperationFailed = true;
		}
	}

	// now delete the file which was saved using the SaveAs() method, and invoke
	// the Save() operation, which should 
	try
	{
		deleteFile(strNewGDDFile1, true);
	}
	catch(UCLIDException& uex)
	{
		uex.log();
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to delete file other_new.tif.gdd after the SaveAS() call!"));
		bSomeOperationFailed = true;
	}

	// invoke the Save() method
	hr = m_ipSpotRecIR->Save();
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Save() call for a single-page image failed!"));
		bSomeOperationFailed = true;
	}
	else // ensure that file was saved
	{
		if (!isFileOrFolderValid( strNewGDDFile1 ))
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("Save() did not actually save a file for a single-page image!"));
			bSomeOperationFailed = true;
		}

		// TODO: need to compare actual contents of saved file with expected contents
		
		// check to see if the returned GDD file name is the name passed in to the
		// SaveAs method
		_bstr_t _bstrActualGDDFileName = m_ipSpotRecIR->GetGDDFileName();
		_bstr_t _bstrExpectedGDDFileName = strNewGDDFile1.c_str();
		if (_bstrActualGDDFileName != _bstrExpectedGDDFileName)
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("GetGDDFileName() call failed after a call to SaveAs()!"));
			bSomeOperationFailed = true;
		}
	}

	// for developer sake, keep the window in its current state for some time
	Sleep(gulDELAY);

	// clear the contents
	hr = m_ipSpotRecIR->Clear();
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Clear() call failed!"));
		bSomeOperationFailed = true;
	}
	else // ensure that file was opened
	{
		_bstr_t _bstrActualImageFileName = m_ipSpotRecIR->GetImageFileName();
		if (_bstrActualImageFileName != _bstr_t(""))
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("Clear() called, but GetImageFileName() returns a non-empty string!"));
			bSomeOperationFailed = true;
		}
	}


	// for developer sake, keep the window in its current state for some time
	Sleep(gulDELAY);

	// close the window
	ipInputReceiver->ShowWindow(VARIANT_FALSE);

	// notify end of test case
	m_ipLogger->EndTestCase(bSomeOperationFailed ? VARIANT_FALSE : VARIANT_TRUE);
}
//--------------------------------------------------------------------------------------------------
/*
void CSpotRecIRTester::template()
{
	bool bSomeOperationFailed = false;

	_bstr_t bstrID("atc05");
	_bstr_t bstrDescription("Open image test");

	// notify start of test case
	m_ipLogger->StartTestCase(bstrID, bstrDescription, kAutomatedTestCase);
	
	// notify end of test case
	m_ipLogger->EndTestCase(bSomeOperationFailed ? VARIANT_FALSE : VARIANT_TRUE);
}
*/
//--------------------------------------------------------------------------------------------------
void CSpotRecIRTester::setTestFileFolder(IVariantVectorPtr ipParams, const std::string &strTCLFile)
{
	// if pParams is not empty and the second item is specified,
	// then the second item is the master dat file
	if ((ipParams != __nullptr) && (ipParams->Size > 1))
	{
		std::string strTestFolder = asString(_bstr_t(ipParams->GetItem(1)));

		if(strTestFolder != "")
		{
			m_strAutomatedTestFilesFolder = getDirectoryFromFullPath(::getAbsoluteFileName(strTCLFile, strTestFolder + "\\dummy.txt"));

			if(!isValidFolder(m_strAutomatedTestFilesFolder))
			{
				// Create and throw exception
				UCLIDException ue("ELI12317", "Required test file folder is invalid or not specified in TCL file!");
				ue.addDebugInfo("Folder", m_strAutomatedTestFilesFolder);
				throw ue;	
			}
			else
			{
				// Folder was specified and exists, return successfully
				return;
			}
		}
	}

	// Create and throw exception
	UCLIDException ue("ELI19408", "Required test file folder not specified in TCL file!");
	throw ue;	
}
//-------------------------------------------------------------------------------------------------
