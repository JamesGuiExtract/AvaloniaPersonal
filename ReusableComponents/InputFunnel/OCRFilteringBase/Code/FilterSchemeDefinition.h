//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	FilterSchemeDefinition.h
//
// PURPOSE:	Reads, writes FSD files
//
// NOTES:	
//
// AUTHORS:	Duan Wang
//
//============================================================================
#pragma once

#include "FilterOptionsDefinition.h"

#include <string>
#include <vector>
#include <map>

const std::string DEFAULT_SCHEME = "<Default Scheme>";

class FilterSchemeDefinition
{
public:
	//==============================================================================================
	// Constructors
	FilterSchemeDefinition();
	FilterSchemeDefinition(const FilterSchemeDefinition& toCopy);
	FilterSchemeDefinition& operator = (const FilterSchemeDefinition& toAssign);
	FilterSchemeDefinition(FilterOptionsDefinitions* pFODs);

	//==============================================================================================
	// Creates a default scheme, which is not and will not be stored as any fsd file
	void createDefaultScheme();

	//==============================================================================================
	// Enable/disable for the current scheme
	void enableFiltering(bool bEnable);

	//==============================================================================================
	// Whether or not to have the specified choice id in the FSD.
	// Note: 
	// bEnable = true: If the choice id was not in the FSD, add it;
	//				   If the choice id was in the FSD, no change;
	// bEnable = false: If the choice id was not in the FSD, no change;
	//					If the choice id was in the FSD, remove it.
	// If the Input Type is not in the map, if bEnable is true,
	// the input type string must be added to the map
	void enableInputChoice(const std::string& strFODName, 
						   const std::string& strChoiceID,
						   bool bEnable);

	//==============================================================================================
	// Returns the case sensitivies for specified FOD 
	void getCaseSensitivities(const std::string& strFODName, 
							  bool& bExactCase,
							  bool& bUpperCase,
							  bool& bLowerCase);

	//==============================================================================================
	// Retrieve current FSD file name. If current FSD is not stored in a file, return empty string
	std::string getFSDFileName() {return m_strFSDFileName;}

	//==============================================================================================
	// Returns a set of all FOD Names in current FSD
	std::vector<std::string> getFODNames();

	//==============================================================================================
	// Returns a set of sub string choice ids that are selected for 
	// OCR Filtering based upon the input type for FSD file
	std::vector<std::string> getEnabledInputChoiceIDs(const std::string& strFODName);

	//==============================================================================================
	// Whether or not current filter scheme is empty
	bool isCurrentSchemeEmpty() {return m_mapFODNameToFilteringChoices.empty();}

	//==============================================================================================
	// Whether or not current scheme filtering is enabled
	bool isFilteringEnabled() {return !m_bCurrentSchemeDisabled;}

	//==============================================================================================
	// Whether or not current FSD has been modified
	bool isModified() {return m_bModified;}

	//==============================================================================================
	// Whether or not the specified choice inside the specified input type is selected
	// as one of the filtering choice for FSD File
	bool isInputChoiceEnabled(const std::string& strFODName, const std::string& strChoiceID);

	//==============================================================================================
	// Reads FSD file
	void readFromFile(const std::string& strFSDFileFullName);
	
	//==============================================================================================
	// Store case sensitivies for specified FOD
	void setCaseSensitivities(const std::string& strFODName, 
							  bool bExactCase,
							  bool bUpperCase,
							  bool bLowerCase);

	//==============================================================================================
	// Save all filter scheme definitions to the specified FSD File
	void writeToFile(const std::string& strFSDFileFullName);

private:
	// **************************
	// Helper functions

	//==============================================================================================
	// Enables all choices (case in-sensitive) for each FOD that is not included in current FSD file
	// and put it in m_mapFODNameToFilteringChoices
	void addDefaultFilteringChoicesForFODs();

	//==============================================================================================
	// Parse a line of text into filtering choice info
	void parseFilteringChoices(const std::string& strFODName, const std::string& strForParse);


	// **************************
	// Member variables

	static const std::string FILTERING_DISABLED;
	static const std::string EXACT_CASE;
	static const std::string UPPER_CASE;
	static const std::string LOWER_CASE;
	
	// a struct per FOD
	struct FilteringChoices
	{
		FilteringChoices()
		: bExactCase(true),
		  bUpperCase(false),
		  bLowerCase(false)
		{
			vecChoiceIDs.clear();
		}
		// whether or not we want to use the exact 
		// case sensitivity of each choice
		bool bExactCase;
		// whether or not we want to use all upper  
		// case sensitivity of each choice
		bool bUpperCase;
		// whether or not we want to use all lower 
		// case sensitivity of each choice
		bool bLowerCase;
		// vector of sub string choice ids
		std::vector<std::string> vecChoiceIDs;
	};

	FilterOptionsDefinitions* m_pFODs;

	// FOD name maps to its filter choices
	std::map<std::string, FilteringChoices> m_mapFODNameToFilteringChoices;

	// Current FSD File name
	std::string m_strFSDFileName;

	// Indicates whether or not current FSD has been modified
	bool m_bModified;

	// whether or not current scheme filtering is disabled
	bool m_bCurrentSchemeDisabled;
};

typedef std::map<std::string, FilterSchemeDefinition> FilterSchemeDefinitions;
