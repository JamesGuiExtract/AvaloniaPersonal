// TestAbstractMeasurement.cpp : Implementation of CTestAbstractMeasurement
#include "stdafx.h"
#include "TestFilters.h"
#include "TestAbstractMeasurement.h"

#include <UCLIDException.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <Angle.hpp>
#include <Bearing.hpp>
#include <DistanceCore.h>
#include <cpputil.h>
#include <mathUtil.h>


using namespace std;

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestAbstractMeasurement::InterfaceSupportsErrorInfo(REFIID riid)
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
// CTestAbstractMeasurement
//-------------------------------------------------------------------------------------------------
CTestAbstractMeasurement::CTestAbstractMeasurement()
{
}

//-------------------------------------------------------------------------------------------------
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestAbstractMeasurement::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		setTestFileFolder(pParams, std::string(_bstr_t(strTCLFile)));

		testAngle();
 		testBearing();
		testDistance();
	}
	CATCH_UCLID_EXCEPTION("ELI02700")
	CATCH_COM_EXCEPTION("ELI02701")
	CATCH_UNEXPECTED_EXCEPTION("ELI02702")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestAbstractMeasurement::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestAbstractMeasurement::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestAbstractMeasurement::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CTestAbstractMeasurement::testAngle()
{
	// reading the file
	string strTestAngleFile = m_strTestFilesFolder + "\\TestAngle.dat";
	ifstream ifs(strTestAngleFile.c_str());
	CommentedTextFileReader fileReader(ifs, "//", true);

	do
	{
		// read the file a line at a time
		string strForTest(fileReader.getLineText());
		// run the test if the string is not empty
		if (!strForTest.empty())
		{
			VARIANT_BOOL bRet = VARIANT_FALSE;
			CString cstrNo("");
			static long lAngleNo = 1;
			cstrNo.Format("TestAngle_%d", lAngleNo);
			// increment the Number for next case
			lAngleNo ++;

			// start the test case
			m_ipResultLogger->StartTestCase(_bstr_t(cstrNo), _bstr_t("Test Angle"), kAutomatedTestCase); 
			try
			{
				executeAngleTest(strForTest);
				bRet = VARIANT_TRUE;
			}
			catch (UCLIDException& ue)
			{
				ue.addDebugInfo("Case No", string(cstrNo));
				string strError(ue.asStringizedByteStream());
				m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()));
				bRet = VARIANT_FALSE;
			}
			catch (...)
			{
				UCLIDException uclidException("ELI02708", "Unknown Exception is caught");
				uclidException.addDebugInfo("Case No", string(cstrNo));
				uclidException.addDebugInfo("strForTest", strForTest);
				string strError(uclidException.asStringizedByteStream());
				m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()));
				bRet = VARIANT_FALSE;
			}
			
			// end the test case
			m_ipResultLogger->EndTestCase(bRet);
		}
	}
	while (ifs);
}
//-------------------------------------------------------------------------------------------------
void CTestAbstractMeasurement::testBearing()
{
	// reading the file
	string strTestBearingFile = m_strTestFilesFolder + "\\TestBearing.dat";
	ifstream ifs(strTestBearingFile.c_str());
	CommentedTextFileReader fileReader(ifs, "//", true);
	
	do
	{
		// read the file a line at a time
		string strForTest(fileReader.getLineText());
		// run the test if the string is not empty
		if (!strForTest.empty())
		{
			VARIANT_BOOL bRet = VARIANT_FALSE;
			CString cstrNo("");
			static long lBearingNo = 1;
			cstrNo.Format("TestBearing_%d", lBearingNo);
			// increment the Number for next case
			lBearingNo ++;

			// start the test case
			m_ipResultLogger->StartTestCase(_bstr_t(cstrNo), _bstr_t("Test Bearing"), kAutomatedTestCase); 
			try
			{
				executeBearingTest(strForTest);
				bRet = VARIANT_TRUE;
			}
			catch (UCLIDException& ue)
			{
				ue.addDebugInfo("Case No", string(cstrNo));
				string strError(ue.asStringizedByteStream());
				m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()));
				bRet = VARIANT_FALSE;
			}
			catch (...)
			{
				UCLIDException uclidException("ELI02723", "Unknown Exception is caught");
				uclidException.addDebugInfo("Case No", string(cstrNo));
				uclidException.addDebugInfo("strForTest", strForTest);
				string strError(uclidException.asStringizedByteStream());
				m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()));
				bRet = VARIANT_FALSE;
			}
			
			// end the test case
			m_ipResultLogger->EndTestCase(bRet);
		}
	}
	while (ifs);
}
//-------------------------------------------------------------------------------------------------
void CTestAbstractMeasurement::testDistance()
{
	// reading the file
	string strTestDistanceFile = m_strTestFilesFolder + "\\TestDistance.dat";
	ifstream ifs(strTestDistanceFile.c_str());
	CommentedTextFileReader fileReader(ifs, "//", true);

	// set default and current unit as feet such that for
	// any number that doesn't have unit , default to feet.
	// Also we expect output to be in feet
	static DistanceCore distanceCore;
	distanceCore.setDefaultDistanceUnit(kFeet);
	distanceCore.setCurrentDistanceUnit(kFeet);

	do
	{
		// read the file a line at a time
		string strForTest(fileReader.getLineText());
		// run the test if the string is not empty
		if (!strForTest.empty())
		{
			VARIANT_BOOL bRet = VARIANT_FALSE;
			CString cstrNo("");
			static long lDistanceNo = 1;
			cstrNo.Format("TestDistance_%d", lDistanceNo);
			// increment the Number for next case
			lDistanceNo ++;

			// start the test case
			m_ipResultLogger->StartTestCase(_bstr_t(cstrNo), _bstr_t("Test Distance"), kAutomatedTestCase); 
			try
			{
				executeDistanceTest(strForTest);
				bRet = VARIANT_TRUE;
			}
			catch (UCLIDException& ue)
			{
				ue.addDebugInfo("Case No", string(cstrNo));
				string strError(ue.asStringizedByteStream());
				m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()));
				bRet = VARIANT_FALSE;
			}
			catch (...)
			{
				UCLIDException uclidException("ELI02724", "Unknown Exception is caught");
				uclidException.addDebugInfo("Case No", string(cstrNo));
				uclidException.addDebugInfo("strForTest", strForTest);
				string strError(uclidException.asStringizedByteStream());
				m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()));
				bRet = VARIANT_FALSE;
			}
			
			// end the test case
			m_ipResultLogger->EndTestCase(bRet);
		}
	}
	while (ifs);
}
//-------------------------------------------------------------------------------------------------
void CTestAbstractMeasurement::executeAngleTest(const string& strForTest)
{
	vector<CString> vecInputs = parseInput(strForTest);

	if (vecInputs.size() != 3)
	{
		UCLIDException uclidException("ELI02720", "Invalid string for test");
		uclidException.addDebugInfo("strForTest", strForTest);
		uclidException.addDebugInfo("Delimiter", ";");
		throw uclidException;
	}

	// Angle object only needs to be created once
	static Angle angle;
	angle.evaluate(vecInputs[0]);

	// 1) validate the input string 
	if (!angle.isValid())
	{
		UCLIDException uclidException("ELI02709", "Invalid Angle input!");
		uclidException.addDebugInfo("Angle Input", string(vecInputs[0]));
		throw uclidException;
	}

	// 2) compare the validated input with cstrStandareStr
	CString cstrValidatedInput(angle.asStringDMS().c_str());
	if (_strcmpi(cstrValidatedInput, vecInputs[1]) != 0)
	{
		UCLIDException uclidException("ELI02710", "The standard angle doesn't match the actual angle.");
		uclidException.addDebugInfo("Angle Input", string(vecInputs[0]));
		uclidException.addDebugInfo("Standard string", string(vecInputs[1]));
		throw uclidException;
	}

	// 3) compare the acutal angle value with the vecInputs[2] (in terms of degrees)
	double dAssumedValue = asDouble(string(vecInputs[2]));
	double dActualValue = angle.getDegrees();
	// if they are different at 1E-4, then consider they are equal
	if (fabs(dAssumedValue - dActualValue) > 1E-4)
	{
		UCLIDException uclidException("ELI02711", "Calculated Angle value doesn't match the Assumed value!");
		uclidException.addDebugInfo("Angle Input", string(vecInputs[0]));
		uclidException.addDebugInfo("Calculated value", dActualValue);
		uclidException.addDebugInfo("Assumed value (in Degrees)", string(vecInputs[2]));
		uclidException.addDebugInfo("Actual value (in Degrees)", dActualValue);
		throw uclidException;
	}

	// 4) from the assumed value, can we get the standard angle string?
	angle.evaluateRadians(dAssumedValue * MathVars::PI / 180.0);
	cstrValidatedInput = angle.asStringDMS().c_str();
	if (_strcmpi(cstrValidatedInput, vecInputs[1]) != 0)
	{
		UCLIDException uclidException("ELI19487", "The standard angle doesn't match the assumend angle value.");
		uclidException.addDebugInfo("Angle Input", string(vecInputs[0]));
		uclidException.addDebugInfo("Standard Angle String", string(vecInputs[1]));
		uclidException.addDebugInfo("Assumed Angle Value", string(vecInputs[2]));
		throw uclidException;
	}
}
//-------------------------------------------------------------------------------------------------
void CTestAbstractMeasurement::executeBearingTest(const string& strForTest)
{
	vector<CString> vecInputs = parseInput(strForTest);

	if (vecInputs.size() != 3)
	{
		UCLIDException uclidException("ELI02721", "Invalid string for test");
		uclidException.addDebugInfo("strForTest", strForTest);
		uclidException.addDebugInfo("Delimiter", ";");
		throw uclidException;
	}

	// Bearing object only needs to be created once
	static Bearing bearing;
	bearing.evaluate(vecInputs[0]);

	// 1) validate the input string 
	if (!bearing.isValid())
	{
		UCLIDException uclidException("ELI02713", "Invalid Bearing input!");
		uclidException.addDebugInfo("Bearing Input", string(vecInputs[0]));
		throw uclidException;
	}

	// 2) compare the validated input with cstrStandareStr
	CString cstrValidatedInput(bearing.interpretedValueAsString().c_str());
	if (_strcmpi(cstrValidatedInput, vecInputs[1]) != 0)
	{
		UCLIDException uclidException("ELI02714", "The standard bearing doesn't match the actual bearing.");
		uclidException.addDebugInfo("Bearing Input", string(vecInputs[0]));
		uclidException.addDebugInfo("Standard String", string(vecInputs[1]));
		throw uclidException;
	}

	// 3) compare the acutal bearing value with the vecInputs[2] (in terms of degrees)
	double dAssumedValue = asDouble(string(vecInputs[2]));
	double dActualValue = bearing.getDegrees();
	// if they are different at 1E-4, then consider they are equal
	if (fabs(dAssumedValue - dActualValue) > 1E-4)
	{
		UCLIDException uclidException("ELI02715", "Calculated Bearing value(in Degrees) doesn't match the Assumed value!");
		uclidException.addDebugInfo("Bearing Input", string(vecInputs[0]));
		uclidException.addDebugInfo("Calculated value", dActualValue);
		uclidException.addDebugInfo("Assumed value (in Degrees)", string(vecInputs[2]));
		uclidException.addDebugInfo("Actual value (in Degrees)", dActualValue);
		throw uclidException;
	}

	// 4) from the assumed value, can we get the standard bearing string?
	bearing.evaluateRadians(dAssumedValue * MathVars::PI / 180.0);
	cstrValidatedInput = bearing.interpretedValueAsString().c_str();
	if (_strcmpi(cstrValidatedInput, vecInputs[1]) != 0)
	{
		UCLIDException uclidException("ELI02716", "The standard bearing doesn't match the assumed bearing value.");
		uclidException.addDebugInfo("Bearing Input", string(vecInputs[0]));
		uclidException.addDebugInfo("Standard Bearing String", string(vecInputs[1]));
		uclidException.addDebugInfo("Assumed Bearing Value", string(vecInputs[2]));
		throw uclidException;
	}
}
//-------------------------------------------------------------------------------------------------
void CTestAbstractMeasurement::executeDistanceTest(const string& strForTest)
{
	vector<CString> vecInputs = parseInput(strForTest);

	if (vecInputs.size() != 2)
	{
		UCLIDException uclidException("ELI02722", "Invalid string for test");
		uclidException.addDebugInfo("strForTest", strForTest);
		uclidException.addDebugInfo("Delimiter", ";");
		throw uclidException;
	}

	// Distance object only needs to be created once
	static DistanceCore distance;
	distance.evaluate((LPCTSTR)vecInputs[0]);

	// 1) validate the input string 
	if (!distance.isValid())
	{
		UCLIDException uclidException("ELI02718", "Invalid Distance input!");
		uclidException.addDebugInfo("Distance Input", string(vecInputs[0]));
		throw uclidException;
	}

	// 2) compare the validated input with cstrStandareStr
	double dAssumedValue = asDouble(string(vecInputs[1]));
	// always expect output in terms of feet
	double dActualValue = distance.getDistanceInCurrentUnit();
	// if they are different at 1E-4, then consider they are equal
	if (fabs(dAssumedValue - dActualValue) > 1E-4)
	{
		UCLIDException uclidException("ELI02719", "Calculated Distance value doesn't match the Assumed value!");
		uclidException.addDebugInfo("Distance Input", string(vecInputs[0]));
		uclidException.addDebugInfo("Calculated Value", dActualValue);
		uclidException.addDebugInfo("Assumed value (in Feet)", string(vecInputs[1]));
		throw uclidException;
	}
}
//-------------------------------------------------------------------------------------------------
vector<CString> CTestAbstractMeasurement::parseInput(const string& strForTest, char cDelimiter)
{
	// parse the input string
	vector<string> vecTmpTokens;
	StringTokenizer tokenizer(cDelimiter);
	tokenizer.parse(strForTest.c_str(), vecTmpTokens);
	
	vector<CString> vecTokens;
	unsigned long nSize = vecTmpTokens.size();
	for (unsigned int ui = 0; ui < nSize; ui++)
	{
		CString cstrColumn(vecTmpTokens[ui].c_str());
		// trim the token
		cstrColumn.TrimLeft("\t ");
		cstrColumn.TrimRight("\t ");
		// push into the vector
		vecTokens.push_back(cstrColumn);
	}

	return vecTokens;
}
//-------------------------------------------------------------------------------------------------
void CTestAbstractMeasurement::setTestFileFolder(IVariantVectorPtr ipParams, const std::string &strTCLFile)
{
	// if pParams is not empty and the second item is specified,
	// then the second item is the master dat file
	if ((ipParams != NULL) && (ipParams->Size > 1))
	{
		std::string strTestFolder = _bstr_t(ipParams->GetItem(1));

		// Check for empty folder
		if (strTestFolder == "")
		{
			// Default to folder containing the TCL file
			strTestFolder = getDirectoryFromFullPath( strTCLFile );
		}

		if(strTestFolder != "")
		{
			m_strTestFilesFolder = getDirectoryFromFullPath(::getAbsoluteFileName(strTCLFile, strTestFolder + "\\dummy.txt"));

			if(!isValidFolder(m_strTestFilesFolder))
			{
				// Create and throw exception
				UCLIDException ue("ELI12330", "Required test file folder is invalid or not specified in TCL file!");
				ue.addDebugInfo("Folder", m_strTestFilesFolder);
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
	UCLIDException ue("ELI12331", "Required test file folder not specified in TCL file!");
	throw ue;	
}
//-------------------------------------------------------------------------------------------------
