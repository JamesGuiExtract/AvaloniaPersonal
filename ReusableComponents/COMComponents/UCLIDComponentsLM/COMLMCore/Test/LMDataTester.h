//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ComponentData.h
//
// PURPOSE:	Definition of the LMDataTester class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

#include "resource.h"       // main symbols

#include <string>

class LMData;

/////////////////////////////////////////////////////////////////////////////
// CLMDataTester
class ATL_NO_VTABLE CLMDataTester : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLMDataTester, &CLSID_LMDataTester>,
	public IDispatchImpl<ILMDataTester, &IID_ILMDataTester, &LIBID_LMCOREAUTOTESTLib>,
	public IDispatchImpl<ITestableComponent, &IID_ITestableComponent, &LIBID_UCLIDTESTINGFRAMEWORKINTERFACESLib>
{
public:
	CLMDataTester()
	{
		// Clear the data members
		m_pLMData = NULL;
		m_pLMTest = NULL;
	}

DECLARE_REGISTRY_RESOURCEID(IDR_LMDATATESTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLMDataTester)
	COM_INTERFACE_ENTRY(ILMDataTester)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY2(IDispatch, ILMDataTester)
	COM_INTERFACE_ENTRY(ITestableComponent)
END_COM_MAP()

// ILMDataTester
public:
// ITestableComponent
	STDMETHOD(RunAutomatedTests)();
	STDMETHOD(RunInteractiveTests)();
	STDMETHOD(SetResultLogger)(ITestResultLogger * pLogger);
	STDMETHOD(SetInteractiveTestExecuter)(IInteractiveTestExecuter * pInteractiveTestExecuter);

protected:

	//=======================================================================
	// PURPOSE: Tests compression and extraction of a data set.  Also 
	//				compares the various string objects in the original and 
	//				extracted data sets.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	void	executeTest1();

	//=======================================================================
	// PURPOSE: Compares the original and extracted ComponentData objects.
	//				Also checks the license state of a component that was not 
	//				part of the original data set.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	void	executeTest2();

	//=======================================================================
	// PURPOSE: Creates a data set suitable for unit testing.  Note that 
	//				items added to the data set should be tested by one or 
	//				more of the executeTestN() methods.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	void	prepareData();

	//=======================================================================
	// PURPOSE: Releases memory allocated to hold the original and the 
	//				extracted data sets.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	void	releaseData();

private:

	// Pointer to test result logger that stores and displays test results
	ITestResultLoggerPtr	m_ipLogger;

	// Collection of data to be compressed, extracted, and tested
	// Contents to be tested include:
	// - Issuer name : string
	// - Licensee name : string
	// - Organization name : string
	// - Licensed component #1 : ComponentData
	// - Licensed component #2 : ComponentData
	// - Unlicensed component #1 : ComponentData (expired yesterday)
	// - Unlicensed component #2 : ComponentData (expires tomorrow)
	LMData*					m_pLMData;

	// Collection of data created via extraction from license string
	// The expectation is that the contents match that noted above
	LMData*					m_pLMTest;

	// Results of compression of m_pLMData object
	std::string				m_strLicenseString;
};
