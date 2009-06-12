//==================================================================================================
// COPYRIGHT UCLID SOFTWARE, LLC. 2002
//
// FILE:	FileDateTimeRestorer.h
//
// PURPOSE:	This class opens the specified file for reading and writing.  The access, write, and 
//             modification times are stored and then replaced when the class is destructed.
//
// NOTES:
//
// AUTHOR:	Wayne Lenius
//
//==================================================================================================

#pragma once

#include "BaseUtils.h"

#include <fstream>
#include <string>
#include <vector>

class EXPORT_BaseUtils FileDateTimeRestorer
{
public:

	//---------------------------------------------------------------------------------------------
	// Stores filetime information for specified file and restores settings at destruction
	// File must exist.
	FileDateTimeRestorer(const std::string strFileName);

	~FileDateTimeRestorer();

private:

	// Fully qualified path to file
	std::string m_strFileName;

	// Time of file creation
	FILETIME m_ftCreationTime;

	// Time of last file access
	FILETIME m_ftLastAccessTime;

	// Time of last write to file
	FILETIME m_ftLastWriteTime;

	// Times will be restored only if m_strFileName already exists
	bool m_bFileFound;
};
