//==================================================================================================
// COPYRIGHT UCLID SOFTWARE, LLC. 2004
//
// FILE:	EncryptedFileManager.h
//
// PURPOSE:	This class reads or writes an encrypted file.  The file can be either text or binary.
//
// NOTES:
//
// AUTHOR:	Wayne Lenius
//
//==================================================================================================

#pragma once

#include "BaseUtils.h"

#include <string>
#include <vector>

class EXPORT_BaseUtils EncryptedFileManager
{
public:
	EncryptedFileManager();

	//---------------------------------------------------------------------------------------------
	// PROMISE: To convert a file into an encrypted file
	void encrypt(const std::string& strInputFile, const std::string& strOutputFile);

	//---------------------------------------------------------------------------------------------
	// PROMISE: To convert an encrypted binary file into a collection of bytes
	// REQUIRES: Calling method is responsible for deleting the returned bytes
	unsigned char* decryptBinaryFile(const std::string& strInputFile, 
		unsigned long *pulByteCount);

	//---------------------------------------------------------------------------------------------
	// PROMISE: To convert an encrypted text file into a vector of unencrypted strings
	std::vector<std::string> decryptTextFile(const std::string& strInputFile);

	// static variables
	// NOTE: This class does not check for licenses.  However, this ID is
	// exposed here so that other locations in code that wrap encryption functionality
	// (and which use this class) can use the same component ID for checking
	// licensed state.  Only the component ID is exposed here because this dll
	// cannot use COMLMCore.dll, because of which license state checking cannot
	// be done here.
//	static unsigned long COMPONENT_ID;	// component ID used for licensing.
};
