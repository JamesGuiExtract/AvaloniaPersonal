//==================================================================================================
// COPYRIGHT UCLID SOFTWARE, LLC. 2002
//
// FILE:	CommentedTextFileReader.h
//
// PURPOSE:	This class reads text file. Any comment will be ignored.
//
// NOTES:
//
// AUTHOR:	Duan Wang
//
//==================================================================================================
//
//==================================================================================================
#pragma once

#include "BaseUtils.h"

#include <fstream>
#include <string>
#include <vector>
#include <list>

class EXPORT_BaseUtils CommentedTextFileReader
{
public:
	// NOTE:The default value for the bSkipEmptyLines ctor argument was changed
	//		from false to true because true is a more reasonable default.  Also,
	//		at the time of the change, there was no code under $/Engineering
	//		that used bSkipEmptyLines = false.
	//---------------------------------------------------------------------------------------------
	// PROMISE: To set m_bReadFromFile to true
	// ARGUMENT: strStartComment, strEndComment - for commenting a block of text
	CommentedTextFileReader(std::ifstream& rif, 
		const std::string& strSingleComment = "//", 
		bool bSkipEmptyLines = true, 
		const std::string& strStartComment = "/*", 
		const std::string& strEndComment = "*/");
	//---------------------------------------------------------------------------------------------
	// PROMISE: To set m_bReadFromFile to false
	// ARGUMENT: strStartComment, strEndComment - for commenting a block of text
	CommentedTextFileReader(std::vector<std::string>& rInputLines, 
		const std::string& strSingleComment = "//", 
		bool bSkipEmptyLines = true,
		const std::string& strStartComment = "/*", 
		const std::string& strEndComment = "*/");
	//---------------------------------------------------------------------------------------------
	// PROMISE: Return back a string per line, skip all comments
	//			If m_bReadFromFile == true, the next line is read from the file
	//			stream specified in the ctor.  If m_bReadFromFile == false, the
	//			next line is read from the vector specified in the ctor
	//			Comments will be removed, and empty lines will be skipped 
	//			if m_bSkipEmptyLines == true
	std::string getLineText();
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return true if either (m_bReadFromFile == true && !m_rif) or
	//			(m_bReadFromFile == false && m_nCurrentLine >= m_rInputLines.size())
	bool reachedEndOfStream();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To extract the comments in a file, this operation removes all of the commented text from the lstFile
	// REQUIRE: lst of strings to return the results in
	// ARGS:	lstText:	lst of strings taken from a file
	//			strSingleComment: string used to indicate the start of a single lined comment
	//			strBlockCommentStart:  string used to indicate the start of a block comment
	//			strBlockCommentEnd:  string used to indicate the end of a block comment
	static void sGetUncommentedFileContents(std::list<std::string> &lstText, 
											const std::string &strSingleComment = "//", 
											const std::string &strBlockCommentStart = "/*", 
											const std::string &strBlockCommentEnd = "*/");

private:

	///////////
	// Methods
	///////////
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return the next line from either the input file stream or
	//			the input vector<string> depending upon the value of 
	//			m_bReadFromFile. And the text that's been returned must not be 
	//			within a commented block or after a single comment string
	std::string getNextNonCommentedLine();
	//---------------------------------------------------------------------------------------------
	// PROMISE: Based on the value of m_bSkippingState, interpret the input strLine,
	//			extract out any non-comment text, update m_bSkippingState. 
	//			Retrun true to indicate that strReturnText shall not be ignored; 
	//			false otherwise.
	bool getNonCommentText(const std::string& strLine, std::string& strReturnText);


	///////////
	// Variables
	////////////
	std::ifstream& m_rif;
	std::string m_strSingleComment;
	std::string m_strStartComment;
	std::string m_strEndComment;

	// reference to the vector of input lines passed to
	// the ctor and a current-line index to the vector of lines
	std::vector<std::string>& m_rInputLines;
	long m_nCurrentLine;

	// see notes for constructors on how this
	// variable is used
	bool m_bReadFromFile;

	bool m_bSkipEmptyLines;

	// whether or not in the skipping state. i.e. if a start comment
	// string is found, m_bSkipppingState is set to true, once an end
	// comment string is encountered, m_bSkippingState is set to false.
	// Note: Single comment string shall not set this flag. Only start
	// and end comment string can set this flag.
	bool m_bSkippingState;
};