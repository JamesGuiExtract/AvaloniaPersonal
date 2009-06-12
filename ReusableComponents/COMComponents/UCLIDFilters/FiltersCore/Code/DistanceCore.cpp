#include "stdafx.h"
#include "DistanceCore.h"

#include <UCLIDException.h>
#include <cpputil.h>

#include <algorithm>

using namespace std;

EDistanceUnitType DistanceCore::m_eDefaultUnitType = kUnknownUnit;
EDistanceUnitType DistanceCore::m_eCurrentUnitType = kUnknownUnit;
MapUnitToStrings DistanceCore::m_mapUnitToStrings;
vector<string> DistanceCore::m_vecUnitsInStrings;
vector<vector<EDistanceUnitType> > DistanceCore::m_vecUnitTypeCombos;

///////////////////////////////////////////////////////
//******		  Public Methods		***************
DistanceCore::DistanceCore()
: m_bIsInputValid(false)
{
	try
	{
		HRESULT hr = m_ipDistanceConverter.CreateInstance(__uuidof(DistanceConverter));
		if (FAILED(hr) || m_ipDistanceConverter == NULL)
		{
			throw UCLIDException("ELI02967", "Failed to create object of DistanceConverter.");
		}

		// init one and only once for all objects
		static const bool bInitialized = initialize();

		reset();
	}
	CATCH_UCLID_EXCEPTION("ELI02964")
	CATCH_COM_EXCEPTION("ELI02965")
	CATCH_UNEXPECTED_EXCEPTION("ELI02966");	
}

DistanceCore::DistanceCore(const DistanceCore& distanceCore)
{
	m_ipDistanceConverter = distanceCore.m_ipDistanceConverter;
	m_bIsInputValid = distanceCore.m_bIsInputValid;
	m_strOriginalInput = distanceCore.m_strOriginalInput;
	m_vecCurrentDistance = distanceCore.m_vecCurrentDistance;
}

DistanceCore& DistanceCore::operator = (const DistanceCore& distanceCore)
{
	m_ipDistanceConverter = distanceCore.m_ipDistanceConverter;
	m_bIsInputValid = distanceCore.m_bIsInputValid;
	m_strOriginalInput = distanceCore.m_strOriginalInput;
	m_vecCurrentDistance = distanceCore.m_vecCurrentDistance;

	return *this;
}

string DistanceCore::asStringInUnit(EDistanceUnitType eOutUnitType)
{
	if (eOutUnitType == kUnknownUnit)
	{
		UCLIDException uclidException("ELI02963", "Unknown unit type");
		uclidException.addDebugInfo("Unit type", eOutUnitType);
		throw uclidException;
	}
	
	// get the value in double first
	double dDistance = getDistanceInUnit(eOutUnitType);
	
	// convert it to string with distance unit string
	CString cstrDistance("");
	string strUnit(getStringFromUnit(eOutUnitType));
	// get the first name string for this unit type
	cstrDistance.Format("%.12f %s", dDistance, strUnit.c_str());
	
	return string(cstrDistance);
}

void DistanceCore::evaluate(const string& strInput)
{
	reset();

	m_strOriginalInput = strInput;

	// preprocess string
	preprocessString(m_strOriginalInput);

	// parse the input string
	parseInput();
}

double DistanceCore::getDistanceInCurrentUnit()
{
	if (m_eCurrentUnitType == kUnknownUnit)
	{
		UCLIDException uclidException("ELI02987", "Current distance unit type is undefined.");
		uclidException.addDebugInfo("Current distance unit type", m_eCurrentUnitType);
		throw uclidException;
	}

	return getDistanceInUnit(m_eCurrentUnitType);
}

double DistanceCore::getDistanceInUnit(EDistanceUnitType eOutUnitType)
{
	if (!m_bIsInputValid)
	{
		UCLIDException uclidException("ELI02946", "Failed to retrieve distance value from an invalid input string.");
		uclidException.addDebugInfo("Oringinal input", m_strOriginalInput);
		throw uclidException;
	}
	
	double dDistance = 0.0;
	// convert current value as out unit type
	vector<DistanceInUnit>::iterator iter;
	for (iter = m_vecCurrentDistance.begin(); iter != m_vecCurrentDistance.end(); iter++)
	{
		if ((*iter).eDistanceUnitType == kUnknownUnit)
		{
			UCLIDException uclidException("ELI02954", "Distance unit is undefined.");
			uclidException.addDebugInfo("Distance value", (*iter).dDistanceValue);
			uclidException.addDebugInfo("Distance unit", (*iter).eDistanceUnitType);
			throw uclidException;
		}

		dDistance += m_ipDistanceConverter->ConvertDistanceInUnit(
			(*iter).dDistanceValue, (*iter).eDistanceUnitType, eOutUnitType);
	}

	return dDistance;
}

const vector<string>& DistanceCore::getDistanceUnitStrings() const
{
	return m_vecUnitsInStrings;
}

string DistanceCore::getOriginalInputString()
{
	return m_strOriginalInput;
}

string DistanceCore::getStringFromUnit(EDistanceUnitType eUnit)
{
	if (eUnit == kUnknownUnit)
	{
		UCLIDException uclidException("ELI02983", "Unknown unit type doesn't have a string representation");
		uclidException.addDebugInfo("Unit type", eUnit);
		throw uclidException;
	}

	string strUnit("");

	// get the string name for the unit type
	MapUnitToStrings::const_iterator iterMap = m_mapUnitToStrings.find(eUnit);
	if (iterMap != m_mapUnitToStrings.end())
	{
		// convert it to string with distance unit string
		vector<string> vecStrings = iterMap->second;
		strUnit = vecStrings[0];
	}

	return strUnit;
}

EDistanceUnitType DistanceCore::getUnitFromString(const string& strUnit)
{
	EDistanceUnitType eUnitType = kUnknownUnit;

	MapUnitToStrings::const_iterator iterMap;
	for (iterMap = m_mapUnitToStrings.begin(); iterMap != m_mapUnitToStrings.end(); iterMap++)
	{
		vector<string> vecStrings = iterMap->second;
		vector<string>::iterator iterVec;
		for (iterVec = vecStrings.begin(); iterVec != vecStrings.end(); iterVec++)
		{
			// if found a match
			if (_stricmp((*iterVec).c_str(), strUnit.c_str()) == 0)
			{
				eUnitType = iterMap->first;

				break;
			}
		}
	}

	return eUnitType;
}

bool DistanceCore::isValid()
{
	return m_bIsInputValid;
}

void DistanceCore::reset()
{
	m_strOriginalInput = "";
	m_bIsInputValid = false;
	m_vecCurrentDistance.clear();
}

////////////////////////////////////////////////////////
//*******			Private Methods		**************
bool DistanceCore::initialize()
{
	// ==========  Possible name in strings for each unit type ============
	vector<string> vecStrings;
	vecStrings.clear();

	// possible strings for feet
	vecStrings.push_back("FEET");
	vecStrings.push_back("FOOT");
	vecStrings.push_back("FT");
	vecStrings.push_back("FT.");
	vecStrings.push_back("F");
	vecStrings.push_back("'");
	vecStrings.push_back("‘");	//145<ANSI>
	vecStrings.push_back("’");	//146<ANSI>
	m_mapUnitToStrings[kFeet] = vecStrings;
	m_vecUnitsInStrings = vecStrings;

	// inches
	vecStrings.clear();
	vecStrings.push_back("INCHES");
	m_vecUnitsInStrings.push_back("INCHES");
	vecStrings.push_back("IN");
	m_vecUnitsInStrings.push_back("IN");
	vecStrings.push_back("INCH");
	m_vecUnitsInStrings.push_back("INCH");
	vecStrings.push_back("\"");
	m_vecUnitsInStrings.push_back("\"");
	vecStrings.push_back("“");	//147<ANSI>
	m_vecUnitsInStrings.push_back("“");
	vecStrings.push_back("”");	//148<ANSI>
	m_vecUnitsInStrings.push_back("”");
	m_mapUnitToStrings[kInches] = vecStrings;

	// miles
	vecStrings.clear();
	vecStrings.push_back("MILES");
	m_vecUnitsInStrings.push_back("MILES");
	vecStrings.push_back("MILE");
	m_vecUnitsInStrings.push_back("MILE");
	m_mapUnitToStrings[kMiles] = vecStrings;

	// yards
	vecStrings.clear();
	vecStrings.push_back("YARDS");
	m_vecUnitsInStrings.push_back("YARDS");
	vecStrings.push_back("YARD");
	m_vecUnitsInStrings.push_back("YARD");
	vecStrings.push_back("YDS");
	m_vecUnitsInStrings.push_back("YDS");
	vecStrings.push_back("YD");
	m_vecUnitsInStrings.push_back("YD");
	vecStrings.push_back("YD.");
	m_vecUnitsInStrings.push_back("YD.");
	m_mapUnitToStrings[kYards] = vecStrings;

	// chains
	vecStrings.clear();
	vecStrings.push_back("CHAINS");
	m_vecUnitsInStrings.push_back("CHAINS");
	vecStrings.push_back("CH");
	m_vecUnitsInStrings.push_back("CH");
	vecStrings.push_back("CHAIN");
	m_vecUnitsInStrings.push_back("CHAIN");
	m_mapUnitToStrings[kChains] = vecStrings;

	// rods
	vecStrings.clear();
	vecStrings.push_back("RODS");
	m_vecUnitsInStrings.push_back("RODS");
	vecStrings.push_back("R");
	m_vecUnitsInStrings.push_back("R");
	vecStrings.push_back("RD");
	m_vecUnitsInStrings.push_back("RD");
	vecStrings.push_back("ROD");
	m_vecUnitsInStrings.push_back("ROD");
	m_mapUnitToStrings[kRods] = vecStrings;

	// links
	vecStrings.clear();
	vecStrings.push_back("LINKS");
	m_vecUnitsInStrings.push_back("LINKS");
	vecStrings.push_back("L");
	m_vecUnitsInStrings.push_back("L");
	vecStrings.push_back("LINK");
	m_vecUnitsInStrings.push_back("LINK");
	vecStrings.push_back("LKS");
	m_vecUnitsInStrings.push_back("LKS");
	m_mapUnitToStrings[kLinks] = vecStrings;
	
	// meters
	vecStrings.clear();
	vecStrings.push_back("METERS");
	m_vecUnitsInStrings.push_back("METERS");
	vecStrings.push_back("METRES");
	m_vecUnitsInStrings.push_back("METRES");
	vecStrings.push_back("M");
	m_vecUnitsInStrings.push_back("M");
	vecStrings.push_back("M."); 
	m_vecUnitsInStrings.push_back("M.");
	vecStrings.push_back("METER");
	m_vecUnitsInStrings.push_back("METER");
	vecStrings.push_back("METRE");
	m_vecUnitsInStrings.push_back("METRE");
	m_mapUnitToStrings[kMeters] = vecStrings;

	// cetimeters
	vecStrings.clear();
	vecStrings.push_back("CENTIMETERS");
	m_vecUnitsInStrings.push_back("CENTIMETERS");
	vecStrings.push_back("CENTIMETER");
	m_vecUnitsInStrings.push_back("CENTIMETER");
	vecStrings.push_back("CM");
	m_vecUnitsInStrings.push_back("CM");
	vecStrings.push_back("CM.");
	m_vecUnitsInStrings.push_back("CM.");
	m_mapUnitToStrings[kCentimeters] = vecStrings;

	// kilometers
	vecStrings.clear();
	vecStrings.push_back("KILOMETERS");
	m_vecUnitsInStrings.push_back("KILOMETERS");
	vecStrings.push_back("KILOMETER");
	m_vecUnitsInStrings.push_back("KILOMETER");
	vecStrings.push_back("KM");
	m_vecUnitsInStrings.push_back("KM");
	vecStrings.push_back("KM.");
	m_vecUnitsInStrings.push_back("KM.");
	m_mapUnitToStrings[kKilometers] = vecStrings;


	// =============== unit type combinations (Usually based upon real world documents) =======
	vector<EDistanceUnitType> vecUnitTypes;
	// feet, inches
	vecUnitTypes.clear();
	vecUnitTypes.push_back(kFeet);
	vecUnitTypes.push_back(kInches);
	m_vecUnitTypeCombos.push_back(vecUnitTypes);

	// chains, rods, links
	vecUnitTypes.clear();
	vecUnitTypes.push_back(kChains);
	vecUnitTypes.push_back(kRods);
	vecUnitTypes.push_back(kLinks);
	m_vecUnitTypeCombos.push_back(vecUnitTypes);

	// chains, rods
	vecUnitTypes.clear();
	vecUnitTypes.push_back(kChains);
	vecUnitTypes.push_back(kRods);
	m_vecUnitTypeCombos.push_back(vecUnitTypes);

	// chains, links
	vecUnitTypes.clear();
	vecUnitTypes.push_back(kChains);
	vecUnitTypes.push_back(kLinks);
	m_vecUnitTypeCombos.push_back(vecUnitTypes);

	// rods, links
	vecUnitTypes.clear();
	vecUnitTypes.push_back(kRods);
	vecUnitTypes.push_back(kLinks);
	m_vecUnitTypeCombos.push_back(vecUnitTypes);

	return true;
}

bool DistanceCore::isNumericChar(const char& cInput)
{
	if(cInput >= '0' && cInput <= '9')
	{
		return true;
	}
	
	return false;
}

void DistanceCore::parseInput()
{
	string strForParse(m_strOriginalInput);
	// clean the contents of current distance vec
	m_vecCurrentDistance.clear();

	bool bValid = putNextPairOfNumAndUnit(strForParse);

	while (bValid && !strForParse.empty())
	{
		bValid = putNextPairOfNumAndUnit(strForParse);
	}

	if (!bValid)
	{
		m_bIsInputValid = false;
		return;
	}
	
	// if so far so good
	int size = m_vecCurrentDistance.size();
	// if there are more than one distance unit detected in the input string,
	// check whether or not these units are in the right order
	if (size > 1)
	{
		vector<EDistanceUnitType> vecUnitTypes;
		for(int i = 0; i < size; i++)
		{
			vecUnitTypes.push_back(m_vecCurrentDistance[i].eDistanceUnitType);
		}
		
		vector<vector<EDistanceUnitType> >::iterator result 
			= find(m_vecUnitTypeCombos.begin(), m_vecUnitTypeCombos.end(), vecUnitTypes);
		
		// found 
		if (result == m_vecUnitTypeCombos.end())
		{
			m_bIsInputValid = false;
			
			return;
		}
	}

	m_bIsInputValid = true;
}

void DistanceCore::preprocessString(std::string& strInput)
{
	// find and remove all commas(,)
	int commaPos = strInput.find_first_of(",");
	while (commaPos != string::npos)
	{
		// erase the comma
		strInput.erase(commaPos, 1);
		commaPos = strInput.find_first_of(",");
	}

	// find and remove all carriage returns (\r)
	string	strSpace( " " );
	int iReturnPos = strInput.find_first_of( "\r" );
	while (iReturnPos != string::npos)
	{
		// replace carriage return with blank space
		strInput.replace( iReturnPos, 1, strSpace );
		iReturnPos = strInput.find_first_of( "\r" );
	}

	// find and remove all carriage returns (\n)
	iReturnPos = strInput.find_first_of( "\n" );
	while (iReturnPos != string::npos)
	{
		// replace carriage return with blank space
		strInput.replace( iReturnPos, 1, strSpace );
		iReturnPos = strInput.find_first_of( "\n" );
	}
}

bool DistanceCore::putNextPairOfNumAndUnit(string& strUnread)
{
	trimString(strUnread);

	// get the double number first
	string strOrigin(strUnread);
	const char *pszOrigin = strUnread.c_str();
	char *pszRemain;
	double dDistance = strtod(pszOrigin, &pszRemain);

	// if there's no double number at the beginning of the string, 
	if (dDistance == 0.0 && (_stricmp(pszOrigin, pszRemain) == 0))
	{
		return false;
	}

	// remove the part of characters that have been read already
	strUnread = string(pszRemain);
	// again trim the string
	trimString(strUnread);

	// now we are going to get the unit string out
	string strUnit("");
	long nCurrentPos = 0;
	long nLen = strUnread.length() - 1;
	// we'll get out of the loop if a numeric number is encountered, or the string is empty
	while ( nCurrentPos <= nLen	&& !isNumericChar(strUnread[nCurrentPos]) )
	{
		strUnit += strUnread[nCurrentPos];
		nCurrentPos ++;
	}

	static DistanceInUnit distanceInUnit;

	// unit string can be empty only if there's no more characters following, and
	// there's one and only one number in the whole input string
	// Otherwise, this input string is invalid
	if (strUnit.empty())
	{
		string strTemp(m_strOriginalInput);
		trimString(strTemp);
		if ( nCurrentPos > nLen 
			 && _stricmp(strOrigin.c_str(), strTemp.c_str()) == 0)
		{
			distanceInUnit.dDistanceValue = dDistance;
			// since no unit string is found, set it to default unit
			distanceInUnit.eDistanceUnitType = m_eDefaultUnitType;

			// put the number in the vec
			m_vecCurrentDistance.push_back(distanceInUnit);
			strUnread = "";		
			return true;
		}
		else
		{
			strUnread = "";		
			return false;
		}

	}

	// remove the part of characters that have been read already
	strUnread = strUnread.substr(nCurrentPos, strUnread.length() - strUnit.length());

	// trim the strUnit
	trimString(strUnit);

	// validate the unit string 
	EDistanceUnitType eUnit = getUnitFromString(strUnit);
	if (eUnit == kUnknownUnit)
	{
		strUnread = "";		
		return false;
	}
	
	// put the number and unit into the vec
	distanceInUnit.dDistanceValue = dDistance;
	distanceInUnit.eDistanceUnitType = eUnit;
	m_vecCurrentDistance.push_back(distanceInUnit);

	// trim string again
	trimString(strUnread);

	return true;
}

void DistanceCore::trimString(string& strInput)
{
	CString cstring(strInput.c_str());
	cstring.TrimLeft(" \t");
	cstring.TrimRight(" \t");
	strInput = (LPCTSTR)cstring;
}
