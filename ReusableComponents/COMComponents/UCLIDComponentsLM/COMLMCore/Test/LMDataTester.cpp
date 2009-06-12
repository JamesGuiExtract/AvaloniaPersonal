//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LMData.cpp
//
// PURPOSE:	Implementation of the CLMDataTester class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#include "stdafx.h"
#include "LMCoreAutoTest.h"
#include "LMDataTester.h"
#include "LMData.h"

#include <cpputil.hpp>
#include <UCLIDException.hpp>

// Strings used for creation and comparison of data object
const std::string	gstrLicensee = "Licensee name";
const std::string	gstrOrganization = "Organization name";

/////////////////////////////////////////////////////////////////////////////
// CLMDataTester
/////////////////////////////////////////////////////////////////////////////
STDMETHODIMP CLMDataTester::RunAutomatedTests()
{
	// Prepare master data object
	prepareData();

	// Setup and run each test
	executeTest1();
	executeTest2();

	// Free any allocated memory
	releaseData();

	return S_OK;
}

/////////////////////////////////////////////////////////////////////////////
STDMETHODIMP CLMDataTester::RunInteractiveTests()
{
	// Interactive tests are not defined at this time
	return E_NOTIMPL;
}

/////////////////////////////////////////////////////////////////////////////
STDMETHODIMP CLMDataTester::SetResultLogger(ITestResultLogger * pLogger)
{
	// Store pointer to the logger
	m_ipLogger = pLogger;

	return S_OK;
}

/////////////////////////////////////////////////////////////////////////////
STDMETHODIMP CLMDataTester::SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	// Interactive tests are not defined at this time
	return E_NOTIMPL;
}

/////////////////////////////////////////////////////////////////////////////
// Private methods
/////////////////////////////////////////////////////////////////////////////
void CLMDataTester::executeTest1()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "1";
	_bstr_t	bstrTestCaseDescription = "Test of compress and extract methods plus strings";

	/////////////////
	// Start the test
	/////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, kAutomatedTestCase );
	
	// Check for data existence
	if (m_pLMData == NULL)
	{
		// Failure
		_bstr_t	bstrFail = "Data object is NULL.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( FALSE );
		return;
	}

	try
	{
		// Compress the data
		m_strLicenseString = m_pLMData->compressData();

		// Create an empty test object
		m_pLMTest = new LMData();

		// Populate the test data object with the license key
		//  in order to test the extract() method.
		m_pLMTest->extractData( m_strLicenseString );
	}
	catch (UCLIDException ue)
	{
		// Failure
		_bstr_t	bstrFail = "An exception was thrown during compress() or extract().";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( FALSE );
		return;
	}

	///////////////////
	// Test string data
	///////////////////
	bool		bStringSuccess = true;
	std::string	strTest;

	// Issuer name
	strTest = m_pLMTest->getIssuerName();
	if (strcmpi( strTest.c_str(), getCurrentUserName().c_str() ) != 0)
	{
		bStringSuccess = false;
	}

	// Licensee name
	strTest = m_pLMTest->getLicenseeName();
	if (strcmpi( strTest.c_str(), gstrLicensee.c_str() ) != 0)
	{
		bStringSuccess = false;
	}

	// Organization name
	strTest = m_pLMTest->getOrganizationName();
	if (strcmpi( strTest.c_str(), gstrOrganization.c_str() ) != 0)
	{
		bStringSuccess = false;
	}

	//////////////////////////////////
	// Report results and end the test
	//////////////////////////////////
	if (bStringSuccess)
	{
		// Success
		_bstr_t	bstrOK = "String tests were successful.";
		m_ipLogger->AddTestCaseNote( bstrOK );

		m_ipLogger->EndTestCase( TRUE );
	}
	else
	{
		// Failure
		_bstr_t	bstrFail = "At least one string test failed.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( FALSE );
	}
}

/////////////////////////////////////////////////////////////////////////////
void CLMDataTester::executeTest2()
{
	// Prepare strings for test logger
	_bstr_t	bstrTestCaseID = "2";
	_bstr_t	bstrTestCaseDescription = "Test of component licensing";

	/////////////////
	// Start the test
	/////////////////
	m_ipLogger->StartTestCase( bstrTestCaseID, bstrTestCaseDescription, kAutomatedTestCase );
	
	// Check for data existence
	if (m_pLMData == NULL)
	{
		// Failure
		_bstr_t	bstrFail = "Data object is NULL.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( FALSE );
		return;
	}

	// Check for previous data extraction
	if (m_pLMTest == NULL)
	{
		// Failure
		_bstr_t	bstrFail = "Test object is NULL.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( FALSE );
		return;
	}

	//////////////////
	// Test components
	//////////////////
	bool	bComponentSuccess = true;

	// Licensed components
	if (!m_pLMTest->isLicensed( 100 ))
	{
		bComponentSuccess = false;
	}
	if (!m_pLMTest->isLicensed( 200 ))
	{
		bComponentSuccess = false;
	}

	// Unlicensed component that should have expired
	if (m_pLMTest->isLicensed( 300 ))
	{
		bComponentSuccess = false;
	}

	// Unlicensed component that should not have expired yet
	if (!m_pLMTest->isLicensed( 400 ))
	{
		bComponentSuccess = false;
	}

	// Unknown component that should not be licensed
	if (m_pLMTest->isLicensed( 111 ))
	{
		bComponentSuccess = false;
	}

	//////////////////////////////////
	// Report results and end the test
	//////////////////////////////////
	if (bComponentSuccess)
	{
		// Success
		_bstr_t	bstrOK = "Component tests were successful.";
		m_ipLogger->AddTestCaseNote( bstrOK );

		m_ipLogger->EndTestCase( TRUE );
	}
	else
	{
		// Failure
		_bstr_t	bstrFail = "At least one component test failed.";
		m_ipLogger->AddTestCaseNote( bstrFail );

		m_ipLogger->EndTestCase( FALSE );
	}
}

/////////////////////////////////////////////////////////////////////////////
void CLMDataTester::prepareData()
{
	if ((m_pLMData != NULL) || (m_pLMTest != NULL))
	{
		releaseData();
	}

	// Create an empty data object
	m_pLMData = new LMData();

	// Issue date
	m_pLMData->setIssueDateToToday();

	// Issuer name
	m_pLMData->setIssuerName( getCurrentUserName() );

	// Licensee name
	m_pLMData->setLicenseeName( gstrLicensee );

	// Organization name
	m_pLMData->setOrganizationName( gstrOrganization );

	// Add licensed components
	m_pLMData->addLicensedComponent( 100 );
	m_pLMData->addLicensedComponent( 200 );

	// Create expiration date for now and for tomorrow
	CTime	timeYesterday = CTime::GetCurrentTime();

	CTime	timeTomorrow = CTime::GetCurrentTime();
	CTimeSpan	spanDay( 1, 0, 0, 0 );
	timeYesterday -= spanDay;
	timeTomorrow += spanDay;

	// Add an unlicensed component that expired yesterday
	m_pLMData->addUnlicensedComponent( 300, timeYesterday );

	// Add an unlicensed component that expires tomorrow
	m_pLMData->addUnlicensedComponent( 400, timeTomorrow );
}

/////////////////////////////////////////////////////////////////////////////
void CLMDataTester::releaseData()
{
	// Free the allocated memory
	if (m_pLMData != NULL)
	{
		delete m_pLMData;
		m_pLMData = NULL;
	}

	if (m_pLMTest != NULL)
	{
		delete m_pLMTest;
		m_pLMTest = NULL;
	}
}

/////////////////////////////////////////////////////////////////////////////
