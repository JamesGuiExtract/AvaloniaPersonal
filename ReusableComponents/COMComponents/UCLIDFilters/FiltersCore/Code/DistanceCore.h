#pragma once

#include "FiltersCore.h"

#include <string>
#include <vector>
#include <map>

typedef std::map<EDistanceUnitType, std::vector<std::string> > MapUnitToStrings;

class EXPORT_FILTERS_CORE DistanceCore
{
public:
	DistanceCore();
	DistanceCore(const DistanceCore& distanceCore);
	DistanceCore& operator = (const DistanceCore& distanceCore);

	// Gets the evaluated distance in string in the unit as specified
	std::string asStringInUnit(EDistanceUnitType eOutUnitType);
	// Evaluates the input string without getting the actual distance value out
	void evaluate(const std::string& strInput);
	static EDistanceUnitType getCurrentDistanceUnit() {return m_eCurrentUnitType;}
	// get default distance unit
	static EDistanceUnitType& getDefaultDistanceUnit() {return m_eDefaultUnitType;}
	// Gets the distance value in double in current unit
	double getDistanceInCurrentUnit();
	// Gets the distance value in double in the unit as specified
	double getDistanceInUnit(EDistanceUnitType eOutUnitType);
	// returns all possible distance unit name in strings, for instance, feet, m., Chains.
	const std::vector<std::string>& getDistanceUnitStrings() const;
	// return original input string if only the input string is valid
	std::string getOriginalInputString();
	// for a given unit type, retrieve its standard string representation
	std::string getStringFromUnit(EDistanceUnitType eUnit);
	// for a given string, get the unit type
	EDistanceUnitType getUnitFromString(const std::string& strUnit);
	// Whether or not the original input string for distance is valid
	bool isValid();
	// reset member variables
	void reset();
	// set current distance unit type
	static void setCurrentDistanceUnit(EDistanceUnitType eCurrentUnit) {m_eCurrentUnitType = eCurrentUnit;}
	// set default unit type for input strings that only have numbers
	static void setDefaultDistanceUnit(EDistanceUnitType eDefaultUnit) {m_eDefaultUnitType = eDefaultUnit;} 
	
private:
	/////////////////////////////
	// Helper functions
	////////////////////////////////////////////////////////////////////////////////
	//***				Type Defines		******
	///////////////////////////////////////////////////////////////////////////////
	typedef struct
	{
		double dDistanceValue;
		EDistanceUnitType eDistanceUnitType;
	}DistanceInUnit;


	////////////////////////////////////////////////////////////////////////////////
	//***				Member Variables	******
	///////////////////////////////////////////////////////////////////////////////
	// default distance unit type if input string doesn't specify any unit
	static EDistanceUnitType m_eDefaultUnitType;

	// current distance unit
	static EDistanceUnitType m_eCurrentUnitType;

	// stores the original input string for distance
	std::string m_strOriginalInput;

	// whether or not the original input string for distance is valid
	bool m_bIsInputValid;

	// Distance converter
	UCLID_DISTANCECONVERTERLib::IDistanceConverterPtr m_ipDistanceConverter;

	// possible name in strings for each unit 
	static MapUnitToStrings m_mapUnitToStrings;

	static std::vector<std::string> m_vecUnitsInStrings;

	// vector of possible combination units
	// Note: the sequence of unit type is critical
	static std::vector<std::vector<EDistanceUnitType> > m_vecUnitTypeCombos;

	// current input string parsed into number + unit. The vector holds
	// one or more number + unit
	// Note: each element in this vector must be valid, as well as the sequence
	// of unit type. For instance, 23 feet 12 inches is valid, 12 inches 12 feet 
	// is invalid.
	std::vector<DistanceInUnit> m_vecCurrentDistance;


	////////////////////////////////////////////////////////////////////////////////
	//***				Helper Functions		******
	///////////////////////////////////////////////////////////////////////////////
	// find and return matching unit type for a certain name string.
	// If not found, return kUnknownUnit
	EDistanceUnitType findStringInMap(const std::string& strForSearch);

	// retrieve a pair of number followed by unit from the strUnread, 
	// and convert them and put them in m_vecCurrentDistance. At the same
	// time remove the part that has been read, and return the rest of the
	// string back to the caller.
	// return false if the strUnread is invalid
	bool putNextPairOfNumAndUnit(std::string& strUnread);

	// initialize member variables like those maps
	static bool initialize();

	// if the char is numberic character
	bool isNumericChar(const char& cInput);

	// parse the input string
	void parseInput();

	// preprocess input string
	void preprocessString(std::string& strInput);

	// trim off spaces, tabs
	void trimString(std::string& strInput);

};