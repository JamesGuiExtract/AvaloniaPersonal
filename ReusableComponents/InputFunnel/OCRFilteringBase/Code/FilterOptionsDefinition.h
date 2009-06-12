//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	FilterOptionsDefinition.h
//
// PURPOSE:	Reads, writes and parses FOD files
//
// NOTES:	
//
// AUTHORS:	Duan Wang
//
//============================================================================
#pragma once

#include "InputChoiceInfo.h"

#include <string>
#include <vector>
#include <map>
#include <fstream>

class FilterOptionsDefinition
{
public:
	FilterOptionsDefinition();
	FilterOptionsDefinition(const FilterOptionsDefinition& toCopy);
	FilterOptionsDefinition& operator = (const FilterOptionsDefinition& toAssign);
	//==============================================================================================
	// Constructor, which takes the name of the FOD file for parsing
	// Note: strFODFileFullName must be a fully qualified file name
	FilterOptionsDefinition(const std::string& strFODFileFullName);

	//==============================================================================================
	// Adds a new input choice info and returns the choice id
	std::string addInputChoiceInfo(const std::string& strChoiceDescription,
								   const std::string& strChoiceChars);

	//==============================================================================================
	// Adds a new input type 
	// Note : this function is intended for application that is modifying FOD files
	void addInputType(const std::string& strInputType);

	//==============================================================================================
	// Returns a set of characters that will always be included in the filter
	// For example: For distance type, characters 0-9 are always enabled.
	const std::string& getCharsAlwaysEnabled() const {return m_strCharsAlwaysEnabled;}

	//==============================================================================================
	// Retrives a string of characters that are existing inside one input choice.
	std::string getCharsForInputChoice(const std::string& strChoiceID);

	//==============================================================================================
	// Retrievs a string of characters that are existing inside the set of input choices.
	// Note: no duplicates are allowed
	std::string getCharsForInputChoices(const std::vector<std::string>& vecChoiceIDs);

	//==============================================================================================
	// Get the name for displaying in the Input Categories list box
	// Note: It must be FOD name from the FOD file without extension
	std::string getDisplayName();

	//==============================================================================================
	// Returns current FOD file name in full path. Empty if current FOD doesn't have one
	const std::string& getFODFileName() const {return m_strFODFileName;}

	//==============================================================================================
	// Returns input choice's description inside the FOD based upon the choice id
	std::string getInputChoiceDescription(const std::string& strChoiceID);

	//==============================================================================================
	// Returns all input choices' descriptions inside the FOD based upon the choice ids
	std::vector<std::string> getInputChoiceDescriptions(const std::vector<std::string>& vecChoiceIDs);

	//==============================================================================================
	// Returns all input choice's ids inside the FOD file
	std::vector<std::string> getInputChoiceIDs();

	//==============================================================================================
	// get all input types that are affected in this FOD
	std::vector<std::string> getInputTypes() {return m_vecInputTypes;}

	//==============================================================================================
	// Whether or not current FOD is modified
	bool isModified() {return m_bModified;}

	//==============================================================================================
	// Existence of the input choice id in the m_mapIDToInputChoice
	bool existsInputChoiceID(const std::string& strInputChoiceID);

	//==============================================================================================
	// Removes the specified input choice info
	void removeInputChoiceInfo(const std::string& strInputChoiceID);

	//==============================================================================================
	// Removes an input type 
	// Note : this function is intended for application that is modifying FOD files
	void removeInputType(const std::string& strInputType);

	//==============================================================================================
	// Creates a set of chars that none of the chars is duplicate of one another 
	// from the input string
	// Note: the output will contain space if the input string has spaces
	static std::string sCreateUniqueCharsSet(const std::string& strInput);

	//==============================================================================================
	// Set the set of chars that will be always enabled
	void setCharsAlwaysEnabled(const std::string& strChars);

	//==============================================================================================
	// Set choice chars, no duplication shall allow
	void setInputChoiceChars(const std::string& strChoiceID, 
								 const std::string& strChoiceChars);

	//==============================================================================================
	// Set choice description
	void setInputChoiceDescription(const std::string& strChoiceID, 
									   const std::string& strChoiceDescription);

	//==============================================================================================
	// Saves the current info to the specified FOD file
	// Note: strFODFileFullName must be a fully qualified file name
	void writeToFile(const std::string& strFODFileFullName);

private:
	// **************************
	// Helper functions
	
	//==============================================================================================
	// Generates a unique choice id for current FOD file
	std::string createUniqueChoiceID();

	//==============================================================================================
	// Return reference to a InputChoiceInfo object based upon the specified choid id
	InputChoiceInfo* getInputChoiceInfo(const std::string& strChoiceID);

	//==============================================================================================
	// Whether the input string is qualified as a section header, i.e.
	// with [] around the name for the section
	// strLine: in-out parameter, which is passed in as string for the line
	// and the out value shall be the actual section name, ex. InputTypes, Choices, etc.
	bool isSectionHeader(std::string& strLine);

	//==============================================================================================
	// parse the FOD file
	// Note: strFODFileFullName must be a fully qualified file name
	void parse(const std::string& strFODFileFullName);

	//==============================================================================================
	// Parse strUnparsedInputChoice into choice id, description and chars
	// and create choice info object from description and chars then put them
	// in the map
	void parseInputChoice(const std::string& strLine);
	
	//==============================================================================================
	// Remove any space inside the string
	void removeAllSpaces(std::string& strInOut);

	//==============================================================================================
	// Write a line of text to the output file
	void writeLine(std::ofstream& ofs, const std::string& strLine);


	// **************************
	// Member variables

	// const string for CharsAlwaysEnabled section in FOD file
	static const std::string CHARS_ALWAYS_ENABLED;
	// const string for InputTypes section in FOD file
	static const std::string INPUT_TYPES;
	// const string for CHOICES section in FOD file
	static const std::string CHOICES;

	// A set if chars that are always enabled
	std::string m_strCharsAlwaysEnabled;

	// all input types that are affected in this FOD file
	// Ex: bearing has angle part, direction has start/due directions, etc.
	std::vector<std::string> m_vecInputTypes;

	InputChoices m_mapIDToInputChoice;

	std::string m_strFODFileName;

	// Whether or not current FOD is modified
	bool m_bModified;

};

typedef std::map<std::string, FilterOptionsDefinition> FilterOptionsDefinitions;
