//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	HighlightedTextDlg.h
//
// PURPOSE:	Declaration of CHTIRTester class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CHTIRTester
class ATL_NO_VTABLE CHTIRTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CHTIRTester, &CLSID_HTIRTester>,
	public ISupportErrorInfo,
	public IDispatchImpl<IHTIRTester, &IID_IHTIRTester, &LIBID_HTIRAUTOTESTLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLID_TESTINGFRAMEWORKINTERFACESLib>
{
public:
	CHTIRTester()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_HTIRTESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CHTIRTester)
	COM_INTERFACE_ENTRY(IHTIRTester)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IHTIRTester)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IHTIRTester
public:
// ITestableComponent
	STDMETHOD(raw_RunAutomatedTests)(IVariantVector* pParams, BSTR strTCLFile);

	STDMETHOD(raw_RunInteractiveTests)();

	STDMETHOD(raw_SetResultLogger)(ITestResultLogger * pLogger);

	STDMETHOD(raw_SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

protected:

	//=======================================================================
	// PURPOSE: Creates objects for unit testing.  Note that objects should 
	//				be tested by one or more of the runXyzTest() methods.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	void	prepareData();

	//=============================================================================
	// PURPOSE: Tests initialization of the input receiver and checks that the 
	//				window is not shown and that input is not enabled at startup.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void runProperInitializationTest();

	//=============================================================================
	// PURPOSE: Tests EnableInput(), DisableInput() and get_InputIsEnabled().
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void runEnableInputTest();

	//=============================================================================
	// PURPOSE: Tests get_HasWindow().
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void runHasWindowTest();

	//=============================================================================
	// PURPOSE: Tests ShowWindow() and get_WindowShown().
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void runShowWindowTest();

	//=============================================================================
	// PURPOSE: Tests getComponentDescription().
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void runComponentDescriptionTest();

	//=============================================================================
	// PURPOSE: Tests SetText(), IsModified() and GetText().
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void runSetTextTest();

	//=============================================================================
	// PURPOSE: Tests Open(), GetFileName() and Clear().
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void runOpenTest();

	//=============================================================================
	// PURPOSE: Tests SetText(), Save(), SaveAs(), GetFileName() and IsModified().
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void runSaveTest();

	//=============================================================================
	// PURPOSE: Tests SetInputFinder(), GetInputFinder(), SetText(), Delete(), 
	//				CanBeDeleted(), and GetText() for specific entities.  Note that 
	//				CanBeDeleted() returns FALSE, thus Delete() should actually not 
	//				delete the entity for the Highlighted Text Input Receiver.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void runEntityTextTest();

	//=============================================================================
	// PURPOSE: Tests CanBeMarkedAsUsed(), MarkAsUsed() and IsMarkedAsUsed().
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void runEntityMarkedTest();

	//=============================================================================
	// PURPOSE: Tests IsFromPersistentSource(), GetPersistentSourceName() and 
	//				HasBeenOCRed().
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void runEntitySourceTest();

	//=============================================================================
	// PURPOSE: To set the test file folder based on TCL file specification
	void setTestFileFolder(IVariantVectorPtr ipParams, const std::string &strTCLFile);


private:

	// Folder for test files used in specific automated tests
	std::string					m_strAutomatedTestFilesFolder;

	// Folder for test files used in specific intereactive tests
	std::string					m_strInteractiveTestFilesFolder;
	
	// Pointer to test result logger that stores and displays test results
	ITestResultLoggerPtr		m_ipLogger;

	// Pointer to executor for interactive tests
	IInteractiveTestExecuterPtr	m_ipITCExecuter;

	// Pointer to Highlighted Text Window object being tested
	IHighlightedTextWindowPtr	m_ipHTIR;
};
