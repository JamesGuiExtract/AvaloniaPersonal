//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	HTIRTester.cpp
//
// PURPOSE:	Implementation of CHTIRTester class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#include "stdafx.h"
#include "HTIRAutoTest.h"
#include "HTIRTester.h"

#include <cpputil.h>

#include <UCLIDException.h>
#include <COMUtils.h>

#include <string>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CHTIRTester
/////////////////////////////////////////////////////////////////////////////
STDMETHODIMP CHTIRTester::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IHTIRTester
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

/////////////////////////////////////////////////////////////////////////////
STDMETHODIMP CHTIRTester::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	bool bExceptionCaught = false;

	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "1";
	_bstr_t	bstrTestCaseDescription = "Test of initialization";

	/////////////////
	// Start the test
	/////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, kAutomatedTestCase );

	try
	{
		// Prepare master data object
		prepareData();

		setTestFileFolder( pParams, asString( strTCLFile ) );

		// Setup and run the basic tests
		runProperInitializationTest();
		runHasWindowTest();
		runShowWindowTest();
		runComponentDescriptionTest();

		// Run the elaborate tests
		runEnableInputTest();
		runSetTextTest();
		runOpenTest();
		runSaveTest();

		// Test entity-specific items
		runEntityTextTest();
		runEntityMarkedTest();
		runEntitySourceTest();

		return S_OK;
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI12337", m_ipLogger, bExceptionCaught)

	return S_OK;
}

/////////////////////////////////////////////////////////////////////////////
STDMETHODIMP CHTIRTester::raw_RunInteractiveTests()
{
	UCLIDException ue("ELI12061", "Interactive tests for this component!");
	throw ue;

	// Open the helper app
	string strHelperApp = "";
//	strHelperApp += "\\Engineering\\ProductDevelopment\\InputFunnel\\InputReceivers\\HighlightedTextIR\\Test\\HighlightedTextIRAutoTest\\TestHelperApps\\VBTest\\TestHighlightTextIR.exe";

	// Run the helper app
	runEXE( strHelperApp, "" );

	// Bring up the interactive test case executer
	string strITCFile = m_strInteractiveTestFilesFolder;
	strITCFile += "\\MCRText.itc";
	m_ipITCExecuter->ExecuteITCFile( _bstr_t( strITCFile.c_str() ) );

	return S_OK;
}

/////////////////////////////////////////////////////////////////////////////
STDMETHODIMP CHTIRTester::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	// Store pointer to the logger
	m_ipLogger = pLogger;

	return S_OK;
}

/////////////////////////////////////////////////////////////////////////////
STDMETHODIMP CHTIRTester::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	// Store pointer to the executor
	m_ipITCExecuter = pInteractiveTestExecuter;

	return S_OK;
}

/////////////////////////////////////////////////////////////////////////////
// Private methods
/////////////////////////////////////////////////////////////////////////////
void CHTIRTester::prepareData()
{
	// Create an instance of the Highlighted Text IR
	m_ipHTIR.CreateInstance(__uuidof(HighlightedTextWindow));
}

/////////////////////////////////////////////////////////////////////////////
void CHTIRTester::runProperInitializationTest()
{
	try
	{
		////////
		// Tests
		////////
		bool	bTestSuccess = true;

		// Get pointer to input receiver
		VARIANT_BOOL		vbTest;
		IInputReceiverPtr	ipInputReceiver = m_ipHTIR;

		// By default, the window is not shown
		HRESULT hr = ipInputReceiver->get_WindowShown( &vbTest );
		if (FAILED(hr))
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_WindowShown()!"));
			bTestSuccess = false;
		}
		// Check that the window is hidden
		else if (vbTest == VARIANT_TRUE) 
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("HT window is shown before ShowWindow(TRUE) is called!"));
			bTestSuccess = false;
		}

		// By default, input is not enabled
		hr = ipInputReceiver->get_InputIsEnabled( &vbTest );
		if (FAILED(hr))
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_InputIsEnabled()!"));
			bTestSuccess = false;
		}
		// Check that input is disabled
		else if (vbTest == VARIANT_TRUE) 
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("HT window startes with input enabled"));
			bTestSuccess = false;
		}

		//////////////////////////////////
		// Report results and end the test
		//////////////////////////////////
		if (bTestSuccess)
		{
			// Success
			_bstr_t	bstrOK = "Initialization tests were successful.";
			m_ipLogger->AddTestCaseNote( bstrOK );

			m_ipLogger->EndTestCase( VARIANT_TRUE );
		}
		else
		{
			// Failure
			_bstr_t	bstrFail = "At least one initialization test failed.";
			m_ipLogger->AddTestCaseNote( bstrFail );

			m_ipLogger->EndTestCase( VARIANT_FALSE );
		}
	}
	catch(...)
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Exception caught during runProperInitializationTest()!"));
	}
}

/////////////////////////////////////////////////////////////////////////////
void CHTIRTester::runHasWindowTest()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "2";
	_bstr_t	bstrTestCaseDescription = "Test of HasWindow";

	/////////////////
	// Start the test
	/////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	////////
	// Tests
	////////
	bool	bTestSuccess = true;

	// Get pointer to input receiver
	VARIANT_BOOL		vbTest;
	IInputReceiverPtr	ipInputReceiver = m_ipHTIR;

	// Window is expected to exist
	HRESULT hr = ipInputReceiver->get_HasWindow( &vbTest );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_HasWindow()!"));
		bTestSuccess = false;
	}
	// Check that the window exists
	else if (vbTest == VARIANT_FALSE) 
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("HT window does not exist!"));
		bTestSuccess = false;
	}

	// Window is expected to have a window handle
	long	lHandle = 0;
	hr = ipInputReceiver->get_WindowHandle( &lHandle );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_WindowHandle()!"));
		bTestSuccess = false;
	}
	// Check that the window handle exists
	else if (lHandle == 0) 
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("HT window handle not found!"));
		bTestSuccess = false;
	}

	//////////////////////////////////
	// Report results and end the test
	//////////////////////////////////
	if (bTestSuccess)
	{
		// Success
		_bstr_t	bstrOK = "HasWindow() tests were successful.";
		m_ipLogger->AddTestCaseNote( bstrOK );

		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		// Failure
		_bstr_t	bstrFail = "At least one HasWindow() test failed.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}

/////////////////////////////////////////////////////////////////////////////
void CHTIRTester::runShowWindowTest()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "3";
	_bstr_t	bstrTestCaseDescription = "Test of ShowWindow()";

	/////////////////
	// Start the test
	/////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	////////
	// Tests
	////////
	bool	bTestSuccess = true;

	// Get pointer to input receiver
	VARIANT_BOOL		vbShow;
	VARIANT_BOOL		vbTest;
	IInputReceiverPtr	ipInputReceiver = m_ipHTIR;

	// Make window visible
	vbShow = VARIANT_TRUE;
	HRESULT hr = ipInputReceiver->ShowWindow( vbShow );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call ShowWindow()!"));
		bTestSuccess = false;
	}

	// Check that the window is now visible
	hr = ipInputReceiver->get_WindowShown( &vbTest );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_WindowShown()!"));
		bTestSuccess = false;
	}
	// Check that the window is visible
	else if (vbTest == VARIANT_FALSE) 
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("ShowWindow(TRUE) failed!"));
		bTestSuccess = false;
	}

	// Make window hidden
	vbShow = VARIANT_FALSE;
	hr = ipInputReceiver->ShowWindow( vbShow );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call ShowWindow()!"));
		bTestSuccess = false;
	}

	// Check that the window is now hidden
	hr = ipInputReceiver->get_WindowShown( &vbTest );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_WindowShown()!"));
		bTestSuccess = false;
	}
	// Check that the window is hidden
	else if (vbTest == VARIANT_TRUE) 
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("ShowWindow(FALSE) failed!"));
		bTestSuccess = false;
	}

	//////////////////////////////////
	// Report results and end the test
	//////////////////////////////////
	if (bTestSuccess)
	{
		// Success
		_bstr_t	bstrOK = "ShowWindow() tests were successful.";
		m_ipLogger->AddTestCaseNote( bstrOK );

		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		// Failure
		_bstr_t	bstrFail = "At least one ShowWindow() test failed.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}

/////////////////////////////////////////////////////////////////////////////
void CHTIRTester::runComponentDescriptionTest()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "4";
	_bstr_t	bstrTestCaseDescription = "Test of Component Description";

	/////////////////
	// Start the test
	/////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	////////
	// Tests
	////////
	bool	bTestSuccess = true;

	// Retrieve the component description
	ICategorizedComponentPtr	ipComponent = m_ipHTIR;
	_bstr_t _bstrDescription(ipComponent->GetComponentDescription());
	string stdstrDescription(_bstrDescription);
	if (stdstrDescription == "")
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("getComponentDescription() returned empty string!"));
		bTestSuccess = false;
	}

	//////////////////////////////////
	// Report results and end the test
	//////////////////////////////////
	if (bTestSuccess)
	{
		// Success
		_bstr_t	bstrOK = "Component Description test was successful.";
		m_ipLogger->AddTestCaseNote( bstrOK );

		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		// Failure
		_bstr_t	bstrFail = "Component Description test failed.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}

/////////////////////////////////////////////////////////////////////////////
void CHTIRTester::runEnableInputTest()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "5";
	_bstr_t	bstrTestCaseDescription = "Test of enable/disable input";

	/////////////////
	// Start the test
	/////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	////////
	// Tests
	////////
	bool	bTestSuccess = true;

	// Enable input
	_bstr_t bstrInputType( "Text" );
	_bstr_t bstrPrompt( "Please select some text" );

	IInputReceiverPtr ipInputReceiver = m_ipHTIR;
	HRESULT hr = ipInputReceiver->EnableInput( bstrInputType, bstrPrompt );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote( _bstr_t("Unable to call EnableInput()!") );
		bTestSuccess = false;
	}

	// Check if input is enabled
	VARIANT_BOOL vbEnabled;
	ipInputReceiver->get_InputIsEnabled( &vbEnabled );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_InputIsEnabled()!"));
		bTestSuccess = false;
	}
	else if (vbEnabled == VARIANT_FALSE)
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("EnableInput() called, but InputIsEnabled() is returning false!"));
		bTestSuccess = false;
	}
	
	// Disable input
	ipInputReceiver->DisableInput();
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call DisableInput()!"));
		bTestSuccess = false;
	}

	// Check if input is disabled
	ipInputReceiver->get_InputIsEnabled( &vbEnabled );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Unable to call get_InputIsEnabled()!"));
		bTestSuccess = false;
	}
	else if (vbEnabled == VARIANT_TRUE)
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("DisableInput() called, but InputIsEnabled() is returning true!"));
		bTestSuccess = false;
	}

	//////////////////////////////////
	// Report results and end the test
	//////////////////////////////////
	if (bTestSuccess)
	{
		// Success
		_bstr_t	bstrOK = "Enable/disable input tests were successful.";
		m_ipLogger->AddTestCaseNote( bstrOK );

		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		// Failure
		_bstr_t	bstrFail = "At least one enable/disable input test failed.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}

/////////////////////////////////////////////////////////////////////////////
void CHTIRTester::runSetTextTest()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "6";
	_bstr_t	bstrTestCaseDescription = "Test of SetText()";

	/////////////////
	// Start the test
	/////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	////////
	// Tests
	////////
	bool	bTestSuccess = true;

	// Get pointer to Input Receiver
	IInputReceiverPtr ipInputReceiver = m_ipHTIR;

	// Check IsModified flag
	VARIANT_BOOL	vbModified;
	vbModified = m_ipHTIR->IsModified();
	if (vbModified == VARIANT_TRUE)
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("IsModified() is returning true at start!"));
		bTestSuccess = false;
	}

	// Set text to a default string
	_bstr_t _bstrText = "word 123 another 456 78";

	HRESULT hr = m_ipHTIR->SetText( _bstrText );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("SetText() failed!"));
		bTestSuccess = false;
	}
	// Check the text
	else
	{
		// Retrieve the current text
		_bstr_t _bstrActualText = m_ipHTIR->GetText();

		// Compare with expected text
		if (_bstrActualText != _bstrText)
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("GetText() did not return the expected text!"));
			bTestSuccess = false;
		}
	}

	// Check IsModified flag
	vbModified = m_ipHTIR->IsModified();
	if (vbModified == VARIANT_FALSE)
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("IsModified() is returning false after SetText()!"));
		bTestSuccess = false;
	}

	// Clear the contents
	hr = m_ipHTIR->Clear();
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote( _bstr_t("Clear() failed!") );
		bTestSuccess = false;
	}

	// Close the window
	ipInputReceiver->ShowWindow( VARIANT_FALSE );

	//////////////////////////////////
	// Report results and end the test
	//////////////////////////////////
	if (bTestSuccess)
	{
		// Success
		_bstr_t	bstrOK = "SetText() tests were successful.";
		m_ipLogger->AddTestCaseNote( bstrOK );

		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		// Failure
		_bstr_t	bstrFail = "At least one SetText() test failed.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}

/////////////////////////////////////////////////////////////////////////////
void CHTIRTester::runOpenTest()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "7";
	_bstr_t	bstrTestCaseDescription = "Test of Open()";

	/////////////////
	// Start the test
	/////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	////////
	// Tests
	////////
	bool	bTestSuccess = true;

	// Get pointer to Input Receiver
	IInputReceiverPtr ipInputReceiver = m_ipHTIR;

	// Open a text file
	string strFile = m_strAutomatedTestFilesFolder;
	strFile += "\\legal.txt";
	_bstr_t _bstrFile = strFile.c_str();

	HRESULT hr = m_ipHTIR->Open( _bstrFile );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Open() failed!"));
		bTestSuccess = false;
	}
	// Check the file name
	else
	{
		// Retrieve the current filename
		_bstr_t _bstrActualFileName = m_ipHTIR->GetFileName();

		// Compare with expected filename
		if (_bstrActualFileName != _bstrFile)
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("Open() did not store the proper filename!"));
			bTestSuccess = false;
		}
	}

	// Clear the contents
	hr = m_ipHTIR->Clear();
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote( _bstr_t("Clear() failed!") );
		bTestSuccess = false;
	}
	// Check that filename has been cleared
	else
	{
		// Retrieve the current filename
		_bstr_t _bstrActualFileName = m_ipHTIR->GetFileName();

		// Compare with expected filename
		if (_bstrActualFileName != _bstr_t(""))
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("Clear() called, but filename is non-empty!"));
			bTestSuccess = false;
		}
	}

	// Close the window
	ipInputReceiver->ShowWindow( VARIANT_FALSE );

	//////////////////////////////////
	// Report results and end the test
	//////////////////////////////////
	if (bTestSuccess)
	{
		// Success
		_bstr_t	bstrOK = "Open() tests were successful.";
		m_ipLogger->AddTestCaseNote( bstrOK );

		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		// Failure
		_bstr_t	bstrFail = "At least one Open() test failed.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}

/////////////////////////////////////////////////////////////////////////////
void CHTIRTester::runSaveTest()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "8";
	_bstr_t	bstrTestCaseDescription = "Test of Save()";

	/////////////////
	// Start the test
	/////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	////////
	// Tests
	////////
	bool	bTestSuccess = true;

	// Get pointer to Input Receiver
	IInputReceiverPtr ipInputReceiver = m_ipHTIR;

	// Set text to a default string
	_bstr_t _bstrText = "word 123 another 456 78";

	HRESULT hr = m_ipHTIR->SetText( _bstrText );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("SetText() failed!"));
		bTestSuccess = false;
	}

	// Save contents to a file
	_bstr_t	bstrFileName( "C:\\TextTest.txt" );
	hr = m_ipHTIR->SaveAs( bstrFileName );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote( _bstr_t("SaveAs() failed!") );
		bTestSuccess = false;
	}
	// Check that filename has been defined
	else
	{
		// Retrieve the current filename
		_bstr_t _bstrActualFileName = m_ipHTIR->GetFileName();

		// Compare with expected filename
		if (_bstrActualFileName != bstrFileName)
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("SaveAs() called, but filename is incorrect!"));
			bTestSuccess = false;
		}
	}

	// Check IsModified flag
	VARIANT_BOOL	vbModified;
	vbModified = m_ipHTIR->IsModified();
	if (vbModified == VARIANT_TRUE)
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("IsModified() is returning true after SaveAs()!"));
		bTestSuccess = false;
	}

	// Set text to a different string
	_bstr_t _bstrText2 = "NEW TEXT word 123 another 456 78";

	hr = m_ipHTIR->SetText( _bstrText2 );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Second SetText() failed!"));
		bTestSuccess = false;
	}

	// Test Save() into the same file
	hr = m_ipHTIR->Save();
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote( _bstr_t("Save() failed!") );
		bTestSuccess = false;
	}
	// Check that filename has been retained
	else
	{
		// Retrieve the current filename
		_bstr_t _bstrActualFileName = m_ipHTIR->GetFileName();

		// Compare with expected filename
		if (_bstrActualFileName != bstrFileName)
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("Save() called, but filename is incorrect!"));
			bTestSuccess = false;
		}
	}

	// Check IsModified flag again
	vbModified = m_ipHTIR->IsModified();
	if (vbModified == VARIANT_TRUE)
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("IsModified() is returning true after Save()!"));
		bTestSuccess = false;
	}

	// Close the window
	ipInputReceiver->ShowWindow( VARIANT_FALSE );

	//////////////////////////////////
	// Report results and end the test
	//////////////////////////////////
	if (bTestSuccess)
	{
		// Success
		_bstr_t	bstrOK = "Save() tests were successful.";
		m_ipLogger->AddTestCaseNote( bstrOK );

		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		// Failure
		_bstr_t	bstrFail = "At least one Save() test failed.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}

/////////////////////////////////////////////////////////////////////////////
void CHTIRTester::runEntityTextTest()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "9";
	_bstr_t	bstrTestCaseDescription = "Test of GetEntityText()";

	/////////////////
	// Start the test
	/////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	////////
	// Tests
	////////
	bool	bTestSuccess = true;

	// Get pointer to Input Receiver
	IInputReceiverPtr ipInputReceiver = m_ipHTIR;

	// Set the Input Finder to Word Input Finder
	_bstr_t _bstrFinder = "All Words";
	HRESULT hr = m_ipHTIR->SetInputFinder( _bstrFinder );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("SetInputFinder() failed!"));
		bTestSuccess = false;
	}
	// Check that the Finder has been defined
	else
	{
		// Retrieve the current name
		_bstr_t _bstrFinderName = m_ipHTIR->GetInputFinderName();

		// Compare with expected name
		if (_bstrFinderName != _bstrFinder)
		{
			m_ipLogger->AddTestCaseNote(_bstr_t("SetInputFinder() called, but name is incorrect!"));
			bTestSuccess = false;
		}
	}

	// Set text to a default string
	_bstr_t _bstrText = "word 123 another 456 78";
	_bstr_t	bstrFirst( "word" );
	_bstr_t	bstrNew( "different" );
	_bstr_t	bstrNumber( "123" );

	hr = m_ipHTIR->SetText( _bstrText );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("SetText() failed!"));
		bTestSuccess = false;
	}

	// Retrieve first entity
	IInputEntityManagerPtr	ipManager = m_ipHTIR;

	// Compare retrieved text with expected text
	_bstr_t	_bstrActual = ipManager->GetText( _bstr_t("0") );
	if (_bstrActual != bstrFirst)
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("GetText() called, but entity does not match!"));
		bTestSuccess = false;
	}

	// Modify the first entity
	hr = ipManager->SetText( _bstr_t( "0" ), bstrNew );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("SetText( ID, text ) failed!"));
		bTestSuccess = false;
	}

	// Retrieve the (modified) first entity
	_bstrActual = ipManager->GetText( _bstr_t("0") );

	// Compare retrieved text with expected text
	if (_bstrActual != bstrNew)
	{
		m_ipLogger->AddTestCaseNote(
			_bstr_t("SetText() called, but retrieved entity does not match!"));
		bTestSuccess = false;
	}

	// Set the Input Finder to MCRText Input Finder
	_bstrFinder = "Mathematical Content";
	hr = m_ipHTIR->SetInputFinder( _bstrFinder );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Second SetInputFinder() failed!"));
		bTestSuccess = false;
	}

	// Retrieve first entity after fresh parse
	_bstrActual = ipManager->GetText( _bstr_t("0") );

	// Compare retrieved text with expected text
	if (_bstrActual != bstrNumber)
	{
		m_ipLogger->AddTestCaseNote(
			_bstr_t("GetText() called again, but entity does not match!"));
		bTestSuccess = false;
	}

	// Check that entities cannot be deleted
	VARIANT_BOOL	vbTest = ipManager->CanBeDeleted( _bstr_t("0") );
	if (vbTest == VARIANT_TRUE) 
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Entity thinks it can be deleted!"));
		bTestSuccess = false;
	}

	// Attempt to Delete the first entity
	// NOTE: Delete is not allowed for the Highlighted Text Input Receiver
	hr = ipManager->Delete( _bstr_t("0") );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Delete( ID ) failed!"));
		bTestSuccess = false;
	}

	// Retrieve the "new" first entity
	_bstrActual = ipManager->GetText( _bstr_t("0") );

	// Compare retrieved text with expected text
	if (_bstrActual != bstrNumber)
	{
		m_ipLogger->AddTestCaseNote(
			_bstr_t("Delete( ID ) called, and entity was actually removed!"));
		bTestSuccess = false;
	}

	// Modify entity using invalid ID
	bool	bExpectedFailureOccurred = false;
	try
	{
		hr = ipManager->SetText( _bstr_t( "1000" ), bstrNew );
	}
	catch (_com_error& err)
	{
		err;
		bExpectedFailureOccurred = true;
	}

	if (!bExpectedFailureOccurred)
	{
		m_ipLogger->AddTestCaseNote(
			_bstr_t("SetText( ID ) succeeded when using an invalid ID!"));
		bTestSuccess = false;
	}

	// Retrieve entity using invalid ID
	bExpectedFailureOccurred = false;
	try
	{
		_bstrActual = ipManager->GetText( _bstr_t("1000") );
	}
	catch (_com_error& err)
	{
		err;
		bExpectedFailureOccurred = true;
	}

	if (!bExpectedFailureOccurred)
	{
		m_ipLogger->AddTestCaseNote(
			_bstr_t("GetText( ID ) succeeded when using an invalid ID!"));
		bTestSuccess = false;
	}

	// Delete entity using invalid ID
	bExpectedFailureOccurred = false;
	try
	{
		hr = ipManager->Delete( _bstr_t("1000") );
	}
	catch (_com_error& err)
	{
		err;
		bExpectedFailureOccurred = true;
	}

	if (!bExpectedFailureOccurred)
	{
		m_ipLogger->AddTestCaseNote(
			_bstr_t("Delete( ID ) succeeded when using an invalid ID!"));
		bTestSuccess = false;
	}

	// Close the window
	ipInputReceiver->ShowWindow( VARIANT_FALSE );

	//////////////////////////////////
	// Report results and end the test
	//////////////////////////////////
	if (bTestSuccess)
	{
		// Success
		_bstr_t	bstrOK = "GetEntityText() tests were successful.";
		m_ipLogger->AddTestCaseNote( bstrOK );

		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		// Failure
		_bstr_t	bstrFail = "At least one GetEntityText() test failed.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}

/////////////////////////////////////////////////////////////////////////////
void CHTIRTester::runEntityMarkedTest()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "10";
	_bstr_t	bstrTestCaseDescription = "Test of Marked As Used methods";

	/////////////////
	// Start the test
	/////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	////////
	// Tests
	////////
	bool	bTestSuccess = true;

	// Get pointer to Input Receiver
	IInputReceiverPtr ipInputReceiver = m_ipHTIR;

	// Set the Input Finder to Word Input Finder
	_bstr_t _bstrFinder = "All Words";
	HRESULT hr = m_ipHTIR->SetInputFinder( _bstrFinder );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("SetInputFinder() failed!"));
		bTestSuccess = false;
	}

	// Set text to a default string
	_bstr_t _bstrText = "word 123 another 456 78";
	hr = m_ipHTIR->SetText( _bstrText );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("SetText() failed!"));
		bTestSuccess = false;
	}

	// Check MarkedAsUsed details about first entity
	IInputEntityManagerPtr	ipManager = m_ipHTIR;
	VARIANT_BOOL	vbTest = ipManager->CanBeMarkedAsUsed( _bstr_t("0") );

	// Check that entities can be marked as used
	if (vbTest == VARIANT_FALSE) 
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Entity cannot be marked as used!"));
		bTestSuccess = false;
	}

	// Check current state
	vbTest = ipManager->IsMarkedAsUsed( _bstr_t("0") );
	if (vbTest == VARIANT_TRUE) 
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Entity is already marked as used!"));
		bTestSuccess = false;
	}

	// Mark first entity as used
	hr = ipManager->MarkAsUsed( _bstr_t("0"), VARIANT_TRUE );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("MarkAsUsed( TRUE ) failed!"));
		bTestSuccess = false;
	}

	// Check current state
	vbTest = ipManager->IsMarkedAsUsed( _bstr_t("0") );
	if (vbTest == VARIANT_FALSE) 
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Entity was marked as used but returned FALSE!"));
		bTestSuccess = false;
	}

	// Mark second entity as unused
	hr = ipManager->MarkAsUsed( _bstr_t("1"), VARIANT_FALSE );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("MarkAsUsed( FALSE ) failed!"));
		bTestSuccess = false;
	}

	// Check current state
	vbTest = ipManager->IsMarkedAsUsed( _bstr_t("1") );
	if (vbTest == VARIANT_TRUE) 
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Entity was marked as unused but returned TRUE!"));
		bTestSuccess = false;
	}

	// Close the window
	ipInputReceiver->ShowWindow( VARIANT_FALSE );

	//////////////////////////////////
	// Report results and end the test
	//////////////////////////////////
	if (bTestSuccess)
	{
		// Success
		_bstr_t	bstrOK = "Marked As Used tests were successful.";
		m_ipLogger->AddTestCaseNote( bstrOK );

		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		// Failure
		_bstr_t	bstrFail = "At least one Marked As Used test failed.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}

/////////////////////////////////////////////////////////////////////////////
void CHTIRTester::runEntitySourceTest()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "11";
	_bstr_t	bstrTestCaseDescription = "Test of Persistent Source methods";

	/////////////////
	// Start the test
	/////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, 
		kAutomatedTestCase );

	////////
	// Tests
	////////
	bool	bTestSuccess = true;

	// Get pointer to Input Receiver
	IInputReceiverPtr ipInputReceiver = m_ipHTIR;

	// Set the Input Finder to Word Input Finder
	_bstr_t _bstrFinder = "All Words";
	HRESULT hr = m_ipHTIR->SetInputFinder( _bstrFinder );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("SetInputFinder() failed!"));
		bTestSuccess = false;
	}

	// Set text to a default string www
	_bstr_t _bstrText = "word 123 another 456 78";
	hr = m_ipHTIR->SetText( _bstrText );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("SetText() failed!"));
		bTestSuccess = false;
	}

	// Check PersistentSource details about first entity
	IInputEntityManagerPtr	ipManager = m_ipHTIR;
	VARIANT_BOOL	vbTest = ipManager->IsFromPersistentSource( _bstr_t("0") );

	// Check that SetText() entity is not from a persistent source
	if (vbTest == VARIANT_TRUE) 
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Entity unexpectedly from a persistent source!"));
		bTestSuccess = false;
	}

	// Open a text file
	string strFile = m_strAutomatedTestFilesFolder;
	strFile += "\\legal.txt";
	_bstr_t _bstrFile = strFile.c_str();

	hr = m_ipHTIR->Open( _bstrFile );
	if (FAILED(hr))
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Open() failed!"));
		bTestSuccess = false;
	}

	// Check that Open() entity is from a persistent source
	vbTest = ipManager->IsFromPersistentSource( _bstr_t("0") );
	if (vbTest == VARIANT_FALSE) 
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Entity unexpectedly not from a persistent source!"));
		bTestSuccess = false;
	}

	// Get the persistent source name
	_bstr_t _bstrActual = ipManager->GetPersistentSourceName( _bstr_t("0") );

	// Compare with expected filename
	if (_bstrActual != _bstrFile)
	{
		m_ipLogger->AddTestCaseNote(
			_bstr_t("Open() did not store the expected persistent source name!"));
		bTestSuccess = false;
	}

	// Check that entity has not been OCRed
	vbTest = ipManager->HasBeenOCRed( _bstr_t("0") );
	if (vbTest == VARIANT_TRUE) 
	{
		m_ipLogger->AddTestCaseNote(_bstr_t("Entity has been OCR'ed!"));
		bTestSuccess = false;
	}

	// Close the window
	ipInputReceiver->ShowWindow( VARIANT_FALSE );

	//////////////////////////////////
	// Report results and end the test
	//////////////////////////////////
	if (bTestSuccess)
	{
		// Success
		_bstr_t	bstrOK = "Persistent Source tests were successful.";
		m_ipLogger->AddTestCaseNote( bstrOK );

		m_ipLogger->EndTestCase( VARIANT_TRUE );
	}
	else
	{
		// Failure
		_bstr_t	bstrFail = "At least one Persistent Source test failed.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( VARIANT_FALSE );
	}
}
//--------------------------------------------------------------------------------------------------
void CHTIRTester::setTestFileFolder(IVariantVectorPtr ipParams, const std::string &strTCLFile)
{
	// if pParams is not empty and the second item is specified,
	// then the second item is the master dat file
	if ((ipParams != NULL) && (ipParams->Size > 1))
	{
		std::string strTestFolder = asString(_bstr_t(ipParams->GetItem(1)));

		if(strTestFolder != "")
		{
			m_strAutomatedTestFilesFolder = getDirectoryFromFullPath(::getAbsoluteFileName(strTCLFile, strTestFolder + "\\dummy.txt"));

			if(!isValidFolder(m_strAutomatedTestFilesFolder))
			{
				// Create and throw exception
				UCLIDException ue("ELI12333", "Required test file folder is invalid or not specified in TCL file!");
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
	UCLIDException ue("ELI12334", "Required test file folder not specified in TCL file!");
	throw ue;	
}
//-------------------------------------------------------------------------------------------------
