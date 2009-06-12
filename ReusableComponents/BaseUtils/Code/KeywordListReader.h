//=================================================================================================
// COPYRIGHT UCLID SOFTWARE, LLC. 2004
//
// FILE:	KeywordListReader.h
//
// PURPOSE:	This class reads a text file containing keywords and associated lines of text. 
//			The keywords can be nested so that the lines of text associated with any previously 
//			defined keywords can be associated as a group with subsequent keywords.
//			Any comment lines will be ignored.
//
// NOTES:	File Syntax is:
//
//			<Keyword01>
//			Text line 1
//			Text line 2
//			<Keyword02>
//			#Keyword01
//			Text line 3
//
//			where Keyword01 has two associated lines and Keyword02 has three associated lines
//
// AUTHOR:	Wayne Lenius
//
//=================================================================================================
#pragma once

#include "BaseUtils.h"
#include "CommentedTextFileReader.h"

#include <fstream>
#include <map>
#include <vector>
#include <string>

// TESTTHIS: class design was changed slightly.

class EXPORT_BaseUtils KeywordListReader
{
public:
	//---------------------------------------------------------------------------------------------
	// REQUIRE: the lifetime of the reference objects passed in to the constructor
	//			must extend beyond the lifetime of this object
	KeywordListReader(std::ifstream& rif);
	KeywordListReader(std::vector<std::string>& rInputLines);
	~KeywordListReader();
	//---------------------------------------------------------------------------------------------
	// PROMISE: Provides a vector of strings associated with rstrKeyword.
	//			Returns True if rstrKeyword was found, otherwise False.
	bool GetStringsForKeyword(const std::string& rstrKeyword, std::vector<std::string>& rvecStrings);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Reads the keywords and returns a vector of Keywords
	//			Comments will be removed, and empty lines will be skipped 
	void ReadKeywords(std::vector<std::string>& rvecKeywords);
	//---------------------------------------------------------------------------------------------

private:
	///////
	// Data
	///////
	// Input file
	std::ifstream *m_pInputStream;

	// Reference to the vector of input lines passed to
	// the ctor and a current-line index to the vector of lines
	std::vector<std::string> *m_pvecInputLines;
	long m_nCurrentLine;

	// Set by object constructor
	//   True if input is coming from stream
	//   False if input is coming from vector of strings
	bool m_bReadFromFile;

	// Map of keywords to vector of associated strings
	std::map<std::string, std::vector<std::string> > m_mapKeywordsToStrings;

	//////////
	// Methods
	//////////
	// Returns true iff rstrText is of the form <Keyword> where Keyword length > 0
	bool	isNewKeyword(std::string& rstrText);

	// Returns true iff rstrText is of the form #Keyword 
	// where: Keyword length > 0
	bool	isPastReference(std::string& rstrText);

	// Processes lines from file or vector and provides collection of keywords found.
	// Called by ReadKeywords().
	void	processLines(CommentedTextFileReader& rFR, 
		std::vector<std::string>& rvecKeywords);
};
