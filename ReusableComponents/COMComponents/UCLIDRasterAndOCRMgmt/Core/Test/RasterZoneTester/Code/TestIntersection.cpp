// TestIntersection.cpp : Implementation of CTestIntersection
#include "stdafx.h"
#include "RasterZoneTester.h"
#include "TestIntersection.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <ComponentLicenseIDs.h>
#include <mathUtil.h>

#include <math.h>
#include <vector>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// The tolerance of the intersection error is 0.5%
const double gdERROR_TOLERANCE = 0.005;
const string gstrTESTCASE_HEADER = "<TEST_CASE>";

//-------------------------------------------------------------------------------------------------
// CTestIntersection
//-------------------------------------------------------------------------------------------------
CTestIntersection::CTestIntersection()
: m_ipResultLogger(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CTestIntersection::~CTestIntersection()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16556");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestIntersection::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ITestableComponent,
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
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestIntersection::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
	{
		return E_POINTER;
	}

	try
	{
		// validate license
		validateLicense();

		// Set to VARIANT_TRUE
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ITestableComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestIntersection::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Check pParams size
		if (pParams->Size < 2)
		{
			UCLIDException ue("ELI15251", "The size of pParams is less than 2.");
			ue.addDebugInfo("Size", pParams->Size);

			throw;			
		}

		if (m_ipResultLogger == __nullptr)
		{
			throw UCLIDException("ELI15197", "Please set ResultLogger before proceeding.");
		}

		// Get the testing file name
		std::string strMainTestFile = asString(_bstr_t(pParams->GetItem(1)));	
		strMainTestFile = getAbsoluteFileName(asString(strTCLFile), strMainTestFile );
	
		// Process the testing file
		processFile(strMainTestFile);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15198")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestIntersection::raw_RunInteractiveTests()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15224")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestIntersection::raw_SetResultLogger(ITestResultLogger * pLogger)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();

		m_ipResultLogger = pLogger;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19435")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTestIntersection::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15225")
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CTestIntersection::processFile(const std::string& strFile)
{
	// Read each line of the input file
	ifstream ifs( strFile.c_str() );
	CommentedTextFileReader fileReader( ifs, "//", true );
	string strLine;

	do
	{
		// Retrieve this line from the input file
		strLine = fileReader.getLineText();

		// Check if the string is empty
		if (strLine.empty()) 
		{
			continue;
		}

		// Process each testing case
		processTestCase(strLine);
	}
	while (!ifs.eof());
}
//--------------------------------------------------------------------------------------------------
void CTestIntersection::processTestCase(const std::string& strLine)
{		
	// Initialize exception and success flag
	bool bExceptionCaught = false;
	bool bSuccess = false;

	try
	{
		// The vector of string get from this line
		vector<string> vecTokens;

		StringTokenizer::sGetTokens(strLine, ';', vecTokens);
		if (vecTokens.size() != 5)
		{
			// Create and throw exception
			UCLIDException ue("ELI15199", "Unable to process line.");
			ue.addDebugInfo("Line text", strLine);
			ue.addDebugInfo("Expected number of tokens", "5");
			ue.addDebugInfo("Actual number of tokens", vecTokens.size());
			throw ue;
		}

		// Check if this line is a valid test case and process one test case
		string tok0 = vecTokens[0];
		if (tok0 != gstrTESTCASE_HEADER)
		{
			// Create and throw exception
			UCLIDException ue("ELI15228", "Unable to process line because the line header doesn't match.");
			ue.addDebugInfo("Line text", strLine);
			ue.addDebugInfo("Expected line header", gstrTESTCASE_HEADER);
			ue.addDebugInfo("Actual line header", tok0);
			throw ue;
		}

		// Create a test case ID and start test case
		string strCaseID = "TEST_" +  vecTokens[1];
		m_ipResultLogger->StartTestCase(get_bstr_t(strCaseID), get_bstr_t("Testing Polygon Intersection"), kAutomatedTestCase); 

		// Call the testIntersection() method to test each case
		// and change the order of the rectangles to test it again
		// If either one is failed, it will be considered as a failure
		bSuccess = testIntersection(vecTokens[2], vecTokens[3], vecTokens[4]);
		bSuccess = testIntersection(vecTokens[3], vecTokens[2], vecTokens[4]) && bSuccess;
	}
	CATCH_ALL_AND_ADD_TEST_CASE_EXCEPTION("ELI15203", m_ipResultLogger, bExceptionCaught, VARIANT_FALSE);

	bSuccess = bSuccess && !bExceptionCaught;
	m_ipResultLogger->EndTestCase(asVariantBool(bSuccess));
}
//--------------------------------------------------------------------------------------------------
IRasterZonePtr CTestIntersection::asRasterZone(const std::string& strRect)
{
	// Parse the string to get the start points, end points and height
	// of a rectangle and push into a vector
	vector<string> vecPoly;
	StringTokenizer::sGetTokens(strRect, ',', vecPoly);

	if (vecPoly.size() != 5)
	{
		// Create and throw exception
		UCLIDException ue("ELI15229", "Unable to process rectangle info.");
		ue.addDebugInfo("rectangle info", strRect);
		ue.addDebugInfo("Expected number of tokens", "5");
		ue.addDebugInfo("Actual number of tokens", vecPoly.size());
		throw ue;
	}

	// Create IRasterZone obj and push the start, 
	// end points and height into ipRaster
	IRasterZonePtr ipRaster(CLSID_RasterZone);
	ASSERT_RESOURCE_ALLOCATION("ELI15253", ipRaster != __nullptr)
	ipRaster->StartX = asLong(vecPoly[0]);
	ipRaster->StartY = asLong(vecPoly[1]);
	ipRaster->EndX = asLong(vecPoly[2]);
	ipRaster->EndY = asLong(vecPoly[3]);
	ipRaster->Height = asLong(vecPoly[4]);

	return ipRaster;
}
//--------------------------------------------------------------------------------------------------
bool CTestIntersection::testIntersection(const std::string & strRect1,
		const std::string & strRect2, const std::string & strExpected)
{
	bool bSuccess = false;

	// Parse the string to get the start points, end points and height
	// and convert that to a raster zone and get its area
	IRasterZonePtr ipRaster1 = asRasterZone(strRect1);
	ASSERT_RESOURCE_ALLOCATION("ELI15200", ipRaster1 != __nullptr);
	double dAreaFirst = ipRaster1->Area;

	// Parse the second string to get the start points, end points and height
	// and convert that to a raster zone and get its area
	IRasterZonePtr  ipRaster2 = asRasterZone(strRect2);
	ASSERT_RESOURCE_ALLOCATION("ELI15201", ipRaster2 != __nullptr);
	double dAreaSecond = ipRaster2->Area;

	// Get the smaller area of the two rectangles
	double dSmallRectangleArea = min(dAreaFirst, dAreaSecond);

	// Check if the dSmallRectangleArea is zero
	if (MathVars::isZero(dSmallRectangleArea))
	{
		UCLIDException ue("ELI15202", "One of the rectangle's area is zero.");
		ue.addDebugInfo("Area", dSmallRectangleArea);
		throw ue;				
	}

	// Get the intersection area of the two rectangles with the 
	// GetAreaOverlappingWith() method from IRasterZone interface
	double dIntersectionArea = ipRaster1->GetAreaOverlappingWith(ipRaster2);

	// Read the intersection area of the two rectangles got from CAD
	double dIntersectionFromCAD = asDouble(strExpected);

	// Get the difference between the intersections got from two methods
	// (GetAreaOverlappingWith() method and CAD)
	double dIntersectionDiff = abs(dIntersectionArea - dIntersectionFromCAD);

	// The error ratio of the intersection difference compared to the expected value
	double dErrorRatio = 0.0;

	// Check if the larger intersection area is zero
	if (dIntersectionFromCAD < 0 || dIntersectionArea < 0)
	{
		UCLIDException ue("ELI15231", "One of the methods get a negative intersection area.");
		ue.addDebugInfo("intersection from CAD", dIntersectionFromCAD);
		ue.addDebugInfo("intersection from IRasterZone", dIntersectionArea);
		throw ue;				
	}
	else if (MathVars::isZero(dIntersectionFromCAD) || MathVars::isZero(dIntersectionArea))
	{
		// Check if the difference between the two intersection area is very small
		// by comparing the difference with the smaller rectangle area

		// Example: The intersection got from CAD is 0.0, and the intersection are got 
		// from IRasterZone method is 1.0. Since one of them is zero, we can not get the 
		// difference of the two intersection and devided by the CAD intersection value
		// What we will do is the get the difference and devided by the dSmallRectangleArea
		// if the result is within our tolerance range, we will say it passes the test
		dErrorRatio = dIntersectionDiff/dSmallRectangleArea;

		// Set to true if dDiffRatio is larger than the tolerance value
		if (dErrorRatio <= gdERROR_TOLERANCE)
		{
			bSuccess = true;
		}
	}
	else
	{
		// Check if the difference between the two intersection area divided by 
		// the intersection area got from CAD is very small
		dErrorRatio = dIntersectionDiff/dIntersectionFromCAD;
		if (dErrorRatio <= gdERROR_TOLERANCE)
		{
			bSuccess = true;
		}
	}

	// Create and add test case memo
	CString zTemp("");
	zTemp.Format("The intersection area from RasterZone is : %f\r\n"
		"The intersection area from CAD: %f\r\n"
		"The error percentage is: %f%%", dIntersectionArea, 
		dIntersectionFromCAD, dErrorRatio*100.0);
	m_ipResultLogger->AddTestCaseMemo("Result", get_bstr_t(zTemp));

	return bSuccess;
}

//--------------------------------------------------------------------------------------------------
void CTestIntersection::validateLicense() const
{
	static const unsigned long THIS_COMPONENT_ID = gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI15226", "Rectangle Intersection Tester" );
}
//-------------------------------------------------------------------------------------------------
