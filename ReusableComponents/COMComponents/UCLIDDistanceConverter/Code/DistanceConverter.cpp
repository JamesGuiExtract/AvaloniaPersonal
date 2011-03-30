// DistanceConverter.cpp : Implementation of CDistanceConverter
#include "stdafx.h"
#include "UCLIDDistanceConverter.h"
#include "DistanceConverter.h"

#include <UCLIDException.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <cpputil.h>
#include <comutils.h>

#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#include <comdef.h>
#include <fstream>
#include <Math.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceConverter::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDistanceConverter,
		&IID_ILicensedComponent,
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
STDMETHODIMP CDistanceConverter::raw_RunAutomatedTests(IVariantVector* pParams, BSTR strTCLFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		setTestFileFolder(pParams, asString(strTCLFile));

		testConverter();
	}
	CATCH_UCLID_EXCEPTION("ELI02725")
	CATCH_COM_EXCEPTION("ELI02726")
	CATCH_UNEXPECTED_EXCEPTION("ELI02727")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceConverter::raw_RunInteractiveTests()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceConverter::raw_SetResultLogger(ITestResultLogger * pLogger)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	m_ipResultLogger = pLogger;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceConverter::raw_SetInteractiveTestExecuter(IInteractiveTestExecuter * pInteractiveTestExecuter)
{
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceConverter::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		validateLicense();
		// if validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// CDistanceConverter
//-------------------------------------------------------------------------------------------------
CDistanceConverter::CDistanceConverter()
{
	// initialize the type strings
	initTypeToValueMap();
}
//-------------------------------------------------------------------------------------------------
CDistanceConverter::~CDistanceConverter()
{
	try
	{
		m_mapUnitTypeToValueInCM.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16521");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDistanceConverter::ConvertDistanceInUnit(double dInValue, 
													   EDistanceUnitType eInUnit, 
													   EDistanceUnitType eOutUnit, 
													   double* dOutValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		// init dOutput value to the input value
		double dOutput = dInValue;
		
		// find the eInUnit in map, get the value in centimeters
		map<EDistanceUnitType, double>::iterator iter = m_mapUnitTypeToValueInCM.find(eInUnit);
		if (iter != m_mapUnitTypeToValueInCM.end())
		{
			// first convert the original input value into centimeters
			dOutput *= iter->second;
			
			// find the out unit, then convert the value in centimeters to
			// the specified the out distance unit
			iter = m_mapUnitTypeToValueInCM.find(eOutUnit);
			if (iter != m_mapUnitTypeToValueInCM.end())
			{
				dOutput /= iter->second;
				
				// return the value
				*dOutValue = dOutput;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03123");


	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private helper methods
//-------------------------------------------------------------------------------------------------
void CDistanceConverter::executeDistConverterTest(const string& strForTest)
{
	vector<string> vecInputs = parseInput(strForTest);

	if (vecInputs.size() != 4)
	{
		UCLIDException uclidException("ELI02731", "Invalid string for testing.");
		uclidException.addDebugInfo("strForTest", strForTest);
		uclidException.addDebugInfo("Delimiter", ";");
		throw uclidException;
	}

	// first column of the strForTest is the input value
	double dInValue = asDouble(vecInputs[0]);

	// second column is the input unit
	EDistanceUnitType eInUnit = kUnknownUnit;
	map<string, int>::iterator iter = m_mapUnitStrToType.find(vecInputs[1]);
	if (iter == m_mapUnitStrToType.end())
	{
		UCLIDException uclidException("ELI02732", "Invalid input unit string.");
		uclidException.addDebugInfo("strForTest", strForTest);
		uclidException.addDebugInfo("Input Unit", vecInputs[1]);
		throw uclidException;
	}
	// get the input unit in the form of EDistanceUnitType
	eInUnit = static_cast<EDistanceUnitType>(iter->second);

	// third column is the output value
	double dAssumedOutValue = asDouble(vecInputs[2]);

	// fourth column is the output unit
	EDistanceUnitType eOutUnit = kUnknownUnit;
	iter = m_mapUnitStrToType.find(vecInputs[3]);
	if (iter == m_mapUnitStrToType.end())
	{
		UCLIDException uclidException("ELI02733", "Invalid output unit string.");
		uclidException.addDebugInfo("strForTest", strForTest);
		uclidException.addDebugInfo("Output Unit", vecInputs[3]);
		throw uclidException;
	}
	// get the output unit in the form of EDistanceUnitType
	eOutUnit = static_cast<EDistanceUnitType>(iter->second);

	double dOutValue = 0.0;
	// use Distance Converter to calculate the output value
	HRESULT hr = ConvertDistanceInUnit(dInValue, eInUnit, eOutUnit, &dOutValue);
	if (FAILED(hr))
	{
		UCLIDException uclidException("ELI02734", "Failed to use Distance Converter.");
		uclidException.addDebugInfo("HRESULT", hr);
		uclidException.addDebugInfo("strForTest", strForTest);
		throw uclidException;
	}

	// compare the actual value with the assumed value
	if (fabs(dOutValue - dAssumedOutValue) > 1E-4)
	{
		UCLIDException uclidException("ELI02735", "Calculated output value doesn't match the assumed value!");
		uclidException.addDebugInfo("strForTest", strForTest);
		uclidException.addDebugInfo("Assumed output value", string(vecInputs[2]));
		uclidException.addDebugInfo("Calculated output value", dOutValue);
		throw uclidException;

	}
}
//-------------------------------------------------------------------------------------------------
void CDistanceConverter::initTypeToValueMap()
{
	m_mapUnitTypeToValueInCM[kFeet] = 30.48;		// 1 foot = 30.48 centimeters
	m_mapUnitTypeToValueInCM[kInches] = 2.54;		// 1 inch = 2.54 centimeter
	m_mapUnitTypeToValueInCM[kMiles] = 160934.4;	// 1 mile = 5280 feet = 160934.4 centimeters
	m_mapUnitTypeToValueInCM[kYards] = 91.44;		// 1 yard = 3 feet = 91.44 centimeters
	m_mapUnitTypeToValueInCM[kChains] = 2011.68;	// 1 chain = 66 feet = 2011.68 centimeters
	m_mapUnitTypeToValueInCM[kRods] = 502.92;		// 1 rod = 16.5 feet = 502.92 centimeters
	m_mapUnitTypeToValueInCM[kLinks] = 20.1168;		// 1 link = 1/100 chains = 66/100 feet = 20.1168 centimeters
	m_mapUnitTypeToValueInCM[kMeters] = 100;		// 1 meter = 100 centimeters
	m_mapUnitTypeToValueInCM[kCentimeters] = 1;		// 1 centimeter = 1 centimeter
	m_mapUnitTypeToValueInCM[kKilometers] = 100000;	// 1 kilometer = 1000 meters = 100000 centimeters
}
//-------------------------------------------------------------------------------------------------
void CDistanceConverter::initUnitStrings()
{
	m_mapUnitStrToType.clear();

	// read in the unit definition file
	string strUnitDefFile = m_strTestFilesFolder + "\\UnitDef.dat";
	ifstream ifs(strUnitDefFile.c_str());
	if (!ifs)
	{
		UCLIDException uclidException("ELI02728", "Failed to open UnitDef.dat file.");
		throw uclidException;
	}

	CommentedTextFileReader fileReader(ifs, "//", true);
	int nIndex = 0;

	do
	{
		string strLine(fileReader.getLineText());

		if (!strLine.empty())
		{
			// trim the string
			CString cstr(strLine.c_str());
			cstr.TrimLeft("\t ");
			cstr.TrimRight("\t ");

			nIndex ++;
			m_mapUnitStrToType[string(cstr)] = nIndex;
		}
	}
	while(!ifs.eof());
}
//-------------------------------------------------------------------------------------------------
vector<string> CDistanceConverter::parseInput(const string& strForTest, char cDelimiter)
{
	// parse the input string
	vector<string> vecTmpTokens;
	StringTokenizer tokenizer(cDelimiter);
	tokenizer.parse(strForTest.c_str(), vecTmpTokens);
	
	vector<string> vecTokens;
	unsigned long nSize = vecTmpTokens.size();
	for (unsigned long i = 0; i < nSize; i++)
	{
		string strColumn(vecTmpTokens[i]);
		if (!strColumn.empty())
		{
			CString cstrColumn(strColumn.c_str());
			// trim the token
			cstrColumn.TrimLeft("\t ");
			cstrColumn.TrimRight("\t ");
			// push into the vector
			vecTokens.push_back(string(cstrColumn));
		}
	}

	return vecTokens;
}
//-------------------------------------------------------------------------------------------------
void CDistanceConverter::testConverter()
{
	// initialize string representation for the distance units that we support here.
	// Note that these strings can be modified by the tester inside UnitDef.dat file
	initUnitStrings();

	// reading the file
	string strTestDCFile = m_strTestFilesFolder + "\\TestDistanceConverter.dat";
	ifstream ifs(strTestDCFile.c_str());
	
	if (!ifs)
	{
		UCLIDException uclidException("ELI02729", "Failed to open TestDistanceConverter.dat file.");
		throw uclidException;
	}

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
			static long lNo = 1;
			cstrNo.Format("DistCnvt_%d", lNo);
			// increment the Number for next case
			lNo ++;

			// start the test case
			m_ipResultLogger->StartTestCase(_bstr_t(cstrNo), _bstr_t("Test Distance Converter"), kAutomatedTestCase); 
			try
			{
				executeDistConverterTest(strForTest);
				bRet = VARIANT_TRUE;
			}
			catch (UCLIDException& ue)
			{
				ue.addDebugInfo("Case No", string(cstrNo));
				string strError(ue.asStringizedByteStream());
				m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()), VARIANT_FALSE);
				bRet = VARIANT_FALSE;
			}
			catch (...)
			{
				UCLIDException uclidException("ELI02730", "Unknown Exception is caught!");
				uclidException.addDebugInfo("Case No", string(cstrNo));
				uclidException.addDebugInfo("strForTest", strForTest);
				string strError(uclidException.asStringizedByteStream());
				m_ipResultLogger->AddTestCaseException(_bstr_t(strError.c_str()), VARIANT_FALSE);
				bRet = VARIANT_FALSE;
			}
			
			// end the test case
			m_ipResultLogger->EndTestCase(bRet);
		}
	}
	while (!ifs.eof());
}
//-------------------------------------------------------------------------------------------------
void CDistanceConverter::validateLicense()
{
	static const unsigned long DISTANCE_CONVERTER_COMPONENT_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE( DISTANCE_CONVERTER_COMPONENT_ID, "ELI03122", "Distance Converter" );
}
//-------------------------------------------------------------------------------------------------
void CDistanceConverter::setTestFileFolder(IVariantVectorPtr ipParams, const std::string &strTCLFile)
{
	// if pParams is not empty and the second item is specified,
	// then the second item is the master dat file
	if ((ipParams != __nullptr) && (ipParams->Size > 1))
	{
		std::string strTestFolder = asString(_bstr_t(ipParams->GetItem(1)));

		if(strTestFolder != "")
		{
			m_strTestFilesFolder = getDirectoryFromFullPath(::getAbsoluteFileName(strTCLFile, strTestFolder + "\\dummy.txt"));

			if(!isValidFolder(m_strTestFilesFolder))
			{
				// Create and throw exception
				UCLIDException ue("ELI12328", "Required test file folder is invalid or not specified in TCL file!");
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
	UCLIDException ue("ELI12329", "Required test file folder not specified in TCL file!");
	throw ue;	
}
//-------------------------------------------------------------------------------------------------
