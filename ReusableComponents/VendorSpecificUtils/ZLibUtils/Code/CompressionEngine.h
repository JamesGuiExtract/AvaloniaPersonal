
#pragma once

#include "ZLibUtils.h"
#include <string>

class EXT_ZLIB_UTILS_DLL CompressionEngine
{
public:
	//---------------------------------------------------------------------------------------------
	// REQUIRE: strInputFile is the file to be compressed, and is readable.
	//			strOutputFileName is a writable filename, and is the name of the
	//			desired compressed file
	static void compressFile(const std::string& strInputFile, 
		const std::string& strOutputFile);
	//---------------------------------------------------------------------------------------------
	// REQUIRE: strInputFile is the file to be decompressed, and is readable.
	//			strOutputFileName is a writable filename, and is the name of the
	//			desired decompressed file
	static void decompressFile(const std::string& strInputFile, 
		const std::string& strOutputFile);
	//---------------------------------------------------------------------------------------------
	static bool isZipFile(const std::string& strFile);
	//---------------------------------------------------------------------------------------------
	static bool isGZipFile(const std::string& strFile);
	//---------------------------------------------------------------------------------------------
};
