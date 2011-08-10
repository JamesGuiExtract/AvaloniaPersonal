// TestHarness.cpp : Implementation of CTestHarness
#include "stdafx.h"
#include "UCLIDTestingFrameworkCore.h"
#include "TestHarness.h"

#include <UCLIDException.h>
#include <CommentedTextFileReader.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <BlockExtractor.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#include <fstream>
#include <io.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CTestHarness
//-------------------------------------------------------------------------------------------------
CTestHarness::CTestHarness()
{
	// initialize the test result logger and the interactive test executer
	try
	{
		if (FAILED(m_ipTestResultLogger.CreateInstance(CLSID_TestResultLogger)))
			throw UCLIDException("ELI03535", "Unable to create TestResultLogger object!");

		if (FAILED(m_ipInteractiveTestExecuter.CreateInstance(CLSID_InteractiveTestExecuter)))
			throw UCLIDException("ELI03536", "Unable to create InteractiveTestExecuter object!");

		m_ipInteractiveTestExecuter->SetResultLogger(m_ipTestResultLogger);

		// set the default results output folder to the application data folder + "\TesterResults\"
		m_strDefaultOutputFolder = getExtractApplicationDataPath() + "\\TesterResults\\";

		m_Variables.addVariable("$(DateTimeStamp)", getTimeStamp());
		m_Variables.addVariable("$(ProgID)", "");
		m_Variables.addVariable("$(TestResultsFolder)", m_strDefaultOutputFolder);
		m_Variables.addVariable("$(VersionNumber)", getFileVersion(getCurrentProcessEXEFullPath()));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03534")
}
//-------------------------------------------------------------------------------------------------
CTestHarness::~CTestHarness()
{
	try
	{
		// destruct the test result logger and the interactive test executer
		m_ipInteractiveTestExecuter = __nullptr;
		m_ipTestResultLogger = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16542");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestHarness::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITestHarness,
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
// ITestHarness
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestHarness::raw_RunAutomatedTests()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();
		runAutoTests();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02295")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestHarness::raw_RunInteractiveTests()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();
		runInteractiveTests();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02296")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestHarness::raw_RunAllTests()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();
		runAllTests();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02297")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestHarness::raw_SetTCLFile(BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// clear the vector of progids
		m_vecTestComponents.clear();
		
		// read progids from the tcl file
		m_strTCLFile = asString(strTCLFile);
		// make sure the file exists
		validateFileOrFolderExistence(m_strTCLFile);

		// read the file line by line
		ifstream tclFile(m_strTCLFile.c_str());
		if (!tclFile)
		{
			UCLIDException ue("ELI02300", "Unable to open specified file!");
			ue.addDebugInfo("Filename", m_strTCLFile);
			throw ue;
		}

		std::list<std::string> lstTCLFile;

		convertFileToListOfStrings(tclFile, lstTCLFile);
		CommentedTextFileReader::sGetUncommentedFileContents(lstTCLFile);
		tclFile.close();

		std::vector<std::list<std::string> > vecVariableBlocks;
		BlockExtractor::getAllEnclosedBlocks(lstTCLFile, "[VARS_BEGIN]", "[VARS_END]", vecVariableBlocks, true);

		string strLine("");
		vector<string> vecTokens;
		VariableRegistry localVariables;

		if(vecVariableBlocks.size() != 0)
		{
			std::list<std::string> lstVariables;
			std::list<std::string>::iterator variableIter = vecVariableBlocks[0].begin();

			for(; variableIter != vecVariableBlocks[0].end(); variableIter++)
			{
				vecTokens.clear();
				strLine = *variableIter;

				// parse the line into multiple tokens if any
				StringTokenizer::sGetTokens(strLine, "=", vecTokens);

				// invalid if tokens size is less than one
				if (vecTokens.size() < 1)
				{
					UCLIDException ue("ELI12303", "Invalid line.");
					ue.addDebugInfo("Line", strLine);
					throw ue;
				}

				if(!m_Variables.isVariableName("$(" + vecTokens[0] + ")"))
				{
					localVariables.addVariable("$(" + vecTokens[0] + ")", vecTokens[1]);
				}
				else
				{
					UCLIDException ue("ELI12304", "Duplicate variable definition!");
					ue.addDebugInfo("Variable Name", vecTokens[0]);
					throw ue;
				}
			}
		}

		std::list<std::string>::iterator testIter;

		for(testIter = lstTCLFile.begin(); testIter != lstTCLFile.end(); testIter++)
		{
			vecTokens.clear();
			strLine = *testIter;

			// parse the line into multiple tokens if any
			StringTokenizer::sGetTokens(strLine, ";", vecTokens);
			// invalid if tokens size is less than one
			if (vecTokens.size() < 1)
			{
				UCLIDException ue("ELI10131", "Invalid line.");
				ue.addDebugInfo("Line", strLine);
				throw ue;
			}
			
			TestComponent component;
			component.m_strProgID = vecTokens[0];
			m_Variables.setVariableValue("$(ProgID)", component.m_strProgID);

			for (unsigned int n = 1; n < vecTokens.size(); n++)
			{
				m_Variables.replaceVariablesInString(vecTokens[n]);
				localVariables.replaceVariablesInString(vecTokens[n]);

				// put each additional parameter in the vector
				_variant_t _vParam(_bstr_t(vecTokens[n].c_str()));
				component.m_ipParams->PushBack(_vParam);
			}
			m_vecTestComponents.push_back(component);
		}

		// ensure that there is at least one entry in the specified file
		if (m_vecTestComponents.empty())
		{
			UCLIDException ue("ELI02301", "No component progid's found in the specified file!");
			ue.addDebugInfo("TCL File Name", m_strTCLFile);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02298")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestHarness::raw_ExecuteITCFile(BSTR strITCFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		m_strITCFile = asString(strITCFile);
		m_strTestStartTime = getTimeStamp();

		if (m_ipInteractiveTestExecuter)
		{
			startTestHarness();
			startComponentTest(m_strITCFile, "");
			m_ipInteractiveTestExecuter->ExecuteITCFile(strITCFile);
			endComponentTest();
			endTestHarness();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05609")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestHarness::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Failure of validateLicense is indicated by an exception being thrown
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
void CTestHarness::verifyValidProgID(const string& strProgID)
{
	CLSID clsID;
	_bstr_t bstrProgID(strProgID.c_str());
	if (CLSIDFromProgID(bstrProgID, &clsID) != S_OK)
	{
		UCLIDException ue("ELI02305", "The specified progID is not valid!");
		ue.addDebugInfo("ProgID", strProgID);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CTestHarness::startTestHarness()
{
	string strHarnessDescription = "Execution of TestHarnessApp";
	strHarnessDescription += " (";
	strHarnessDescription += m_strTCLFile;
	strHarnessDescription += ")";

	_bstr_t bstrHarnessDescription(strHarnessDescription.c_str());

	m_ipTestResultLogger->StartTestHarness(bstrHarnessDescription);
}
//-------------------------------------------------------------------------------------------------
void CTestHarness::endTestHarness()
{
	m_ipTestResultLogger->EndTestHarness();
}
//-------------------------------------------------------------------------------------------------
void CTestHarness::startComponentTest(const string& strComponentDescription,
									  const string& strOutputFileName)
{
	m_ipTestResultLogger->StartComponentTest(
		_bstr_t(strComponentDescription.c_str()), 
		_bstr_t(strOutputFileName.c_str()));
}
//-------------------------------------------------------------------------------------------------
void CTestHarness::endComponentTest()
{
	m_ipTestResultLogger->EndComponentTest();
}
//-------------------------------------------------------------------------------------------------
void CTestHarness::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI07038", "Test Harness" );
}
//-------------------------------------------------------------------------------------------------
void CTestHarness::runAutoTests()
{
	// mark the start of the test harness
	startTestHarness();

	m_strTestStartTime = getTimeStamp();

	for (unsigned int n = 0; n < m_vecTestComponents.size(); n++)
	{
		TestComponent component = m_vecTestComponents[n];
		string strProgID(component.m_strProgID);
		verifyValidProgID(strProgID);
		// create the testable component from the prog id
		ITestableComponentPtr ipComponent(strProgID.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI02304", ipComponent != __nullptr);

		ipComponent->SetInteractiveTestExecuter(m_ipInteractiveTestExecuter);
		ipComponent->SetResultLogger(m_ipTestResultLogger);

		// get the name of the output file
		string strFullOutputFile = getOutputFileName(component);

		// replace existing output file name with the fully pathed version
		component.m_ipParams->Remove(0, 1);
		component.m_ipParams->Insert(0, _bstr_t(strFullOutputFile.c_str()));

		// mark the start of the component test
		startComponentTest(strProgID, strFullOutputFile);
		
		// run the tests
		ipComponent->RunAutomatedTests(component.m_ipParams, get_bstr_t(m_strTCLFile));

		// mark the end of the component test
		endComponentTest();
	}

	// mark the end of the test harness
	endTestHarness();
}
//-------------------------------------------------------------------------------------------------
void CTestHarness::runInteractiveTests()
{
	// mark the start of the test harness
	startTestHarness();

	m_strTestStartTime = getTimeStamp();

	for (unsigned int n = 0; n < m_vecTestComponents.size(); n++)
	{
		TestComponent component = m_vecTestComponents[n];
		string strProgID(component.m_strProgID);
		verifyValidProgID(strProgID);
		ITestableComponentPtr ipComponent(strProgID.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI02303", ipComponent != __nullptr);

		// mark the start of the component test
		startComponentTest(strProgID, getOutputFileName(component));

		// run the tests
		ipComponent->SetInteractiveTestExecuter(m_ipInteractiveTestExecuter);
		ipComponent->SetResultLogger(m_ipTestResultLogger);
		ipComponent->RunInteractiveTests();

		// mark the end of the component test
		endComponentTest();
	}

	// mark the end of the test harness
	endTestHarness();
}
//-------------------------------------------------------------------------------------------------
void CTestHarness::runAllTests()
{
	// mark the start of the test harness
	startTestHarness();
	
	for (unsigned int n = 0; n < m_vecTestComponents.size(); n++)
	{
		TestComponent component = m_vecTestComponents[n];
		string strProgID(component.m_strProgID);
		verifyValidProgID(strProgID);
		ITestableComponentPtr ipComponent(strProgID.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI02302", ipComponent != __nullptr);
		
		// mark the start of the component test
		startComponentTest(strProgID, getOutputFileName(component));
		
		// run the tests
		ipComponent->SetInteractiveTestExecuter(m_ipInteractiveTestExecuter);
		ipComponent->SetResultLogger(m_ipTestResultLogger);
		ipComponent->RunAutomatedTests(component.m_ipParams, get_bstr_t(m_strTCLFile));
		ipComponent->RunInteractiveTests();
		
		// mark the end of the component test
		endComponentTest();
	}
	
	// mark the end of the test harness
	endTestHarness();
}
//-------------------------------------------------------------------------------------------------
string CTestHarness::getOutputFileName(const TestComponent& component)
{
	string strOutputFileName("");
	string strOutputFolder("");

	// use TCL or ITC filename if specified as part of the output folder
	if(!m_strTCLFile.empty())
	{
		strOutputFolder = m_strDefaultOutputFolder + getFileNameFromFullPath(m_strTCLFile) + " - " + m_strTestStartTime;
	}
	else if(!m_strITCFile.empty())
	{
		strOutputFolder = m_strDefaultOutputFolder + getFileNameFromFullPath(m_strITCFile) + " - " + m_strTestStartTime;
	}

	if(component.m_ipParams->Size > 0)
	{
		strOutputFileName = asString(_bstr_t(component.m_ipParams->GetItem(0)));
	}

	// if output file is empty use prog id and to identify the test and store the file in the default
	// "TesterResults" folder, else make the name specified relative to the TCL file location
	if(strOutputFileName == "")
	{
		strOutputFileName = strOutputFolder + "\\" + component.m_strProgID + " - " + getTimeStamp() + ".xml";
	}
	else
	{
		// the output file is an XML file
		strOutputFileName = strOutputFileName + ".xml";
		strOutputFileName = getAbsoluteFileName(m_strTCLFile, strOutputFileName, false);
	}

	return strOutputFileName;
}
//-------------------------------------------------------------------------------------------------
