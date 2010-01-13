#pragma once

#include "BaseUtils.h"

#include <vector>
#include <string>
#include <set>

using namespace std;

//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils vector<string> convertFileToLines(const string& strFilename);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To write a vector of lines to a file.  If bAppend == true will just append the
//			lines to the end of the file, if bAppend == false will overwrite the file.
EXPORT_BaseUtils void writeLinesToFile(const vector<string>& vecLines, const string& strFileName,
									   bool bAppend = false);
//-------------------------------------------------------------------------------------------------
// PURPOSE: Load a regular expression from a data file
// REQUIRE: - strFilename must exist unless it is a .XXX.etf file and its corresponding .XXX file
//			exists.  In the second case autoEncryption must be on both at the c++ and registty level.
//			- strFilename must also be a valid regular expression file.  This requires that any
//			#import statements it uses must follow the correct syntax.
//			SYNTAX:
//				#import:
//				<expression> #import <filename>
//				After a #import is found on a line the rest of the line is assumed to be the 
//				name of the file to import.  Whitespace is allowed between the end of the #import
//				text and the beginning of the filename, but not following the filename.
//				- filename must either be absolute or relative to the directory in which strFileName
//				resides
//			
EXPORT_BaseUtils string getRegExpFromFile(const string& strFilename, bool bAutoEncrypt = false, 
									   const string& strAutoEncrtyptKey = "");
//-------------------------------------------------------------------------------------------------
// PURPOSE: Load a regular expression from a string of text that is in the format of a regular
//			Expression file.
// REQUIRE: - strText must be valid regular expression file text as defined above.
//			- strRootFolder must exist if any #import statements are used in strText
//			- filename must either be absolute or relative to strRootFolder
//			
EXPORT_BaseUtils string getRegExpFromText(const string& strText, const string& strRootFolder,
										  bool bAutoEncrypt = false,
										  const string& strAutoEncrtyptKey = "");
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void autoEncryptFile(const string& strFile, const string& strRegistryKey);
//-------------------------------------------------------------------------------------------------
// PURPOSE: Validate strSpecifiedPageNumber. Throws exception if the string is invalid
// ARGUMENT: strSpecifiedPageNumbers - string containing specified page numbers in vary format.
// REQUIRE: Valid page number format: single pages (eg. 2, 5), a range of pages (eg. 4-8),
//			or last X number of pages (eg. -3). They must be seperated by comma (,). When
//			a range of pages is specified, starting page number must be less than ending page number.
// 
EXPORT_BaseUtils void validatePageNumbers(const string& strSpecifiedPageNumbers);
//-------------------------------------------------------------------------------------------------
// PURPOSE: Based on the total number of pages and specified page numbers, return a vector of
//			page numbers in an ascending order. 
// ARGUMENT: strSpecifiedPageNumbers - string containing specified page numbers in vary format.
// REQUIRE: No duplicate page number is allowed in the return vector. Whoever calls this function,
//			must have already called validatePageNumbers() to make sure the validity of 
//			strSpecifiedPageNumbers.
// 
EXPORT_BaseUtils vector<int> getPageNumbers(int nTotalNumberOfPages, const string& strSpecifiedPageNumbers);
//-------------------------------------------------------------------------------------------------
// PURPOSE: Same as above with all above requirements, but instead of returning a sorted vector
//			of page numbers, this method will return a set of page numbers.
EXPORT_BaseUtils set<int> getPageNumbersAsSet(int nTotalNumberOfPages, const string& strSpecifiedPageNumbers);
//-------------------------------------------------------------------------------------------------
// PURPOSE: Load a list of files from a file vector of files
// ARGUMENT:	strFileName is the name of the file with the list. Any lines or blocks with comments (// or /* */) are ignored
//				rvecFiles is the vector the files will be added. The contents of this vector are assumed to be unique if bUnique is true
//				bUnique if true indicates that each file should only appear in the list once
// REQUIRE: The file strFileName must exist and contain a list of files.  
EXPORT_BaseUtils void getFileListFromFile(const string &strFileName, vector<string> &rvecFiles, bool bUnique = true);

