#pragma once

#include "BaseUtils.h"

#include <string>

using namespace std;

class EXPORT_BaseUtils FileIterator
{
public:

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Initializes the FileIterator
	// PARAMS:  strPathAndSpec - The full path in which to search. May include wildcard characters, 
	//          for example, an asterisk (*) or a question mark (?).
	FileIterator(const string& strPathAndSpec);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Closes all handles opened during iteration.
	~FileIterator();
	
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns true if another file is found; returns false otherwise.
	// NOTE:    This method must be called before any accessor will be valid.
	bool moveNext();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Closes all open handles. It is safe to call this method multiple times.
	// PARAMS:  strPathAndSpec - A new directory or path in which to search. May include wildcard
	//			characters, for example, an asterisk (*) or a question mark (?).
	// NOTE:    This method is called automatically by the destructor, but errors are logged 
	//          in the destructor. Call this method directly if you want errors to be thrown.
	void reset();
	void reset(const string& strPathAndSpec);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets the name of the current file being iterated.
	string getFileName();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets the size of the current file in bytes.
	ULONGLONG getFileSize();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns true if the current file is a directory; false if it is a file.
	bool isDirectory();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns true if the current file is used exclusively by the operating system.
	bool isSystemFile();

private:

	// The search path for finding files.
	string m_strPathAndSpec;

	// The file search spec (taken from m_strPathAndSpec)
	string m_strSpec;

	// Handle to the current file being iterated. NULL if iteration has not yet begun.
	HANDLE m_hCurrent;
	
	// Data associated with the current file being iterated.
	WIN32_FIND_DATA m_findData;
};
