//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	BlockExtractor.h
//
// PURPOSE:	Read an enclosed block of text from a file.  Primarily used for extracting configuration 
//			settings such as:  [VARS_BEGIN] ... [VARS_END]
//			
// NOTES:	
//
// AUTHORS:	Niles Bindel
//
//==================================================================================================
#pragma once

#include "BaseUtils.h"

#include <list>
#include <vector>
#include <string>
#include <fstream>

class EXPORT_BaseUtils BlockExtractor
{
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  To read all blocks enclosed by the start and end parameters from a file and place 
	//			 them into a vector of these block's contents
	// PROMISE:  Populate vecResults with all of the text contained between the start and end blocks
	// ARGS:	 strFilename - name of the file
	//			 strStartBlock - string that indicates the start of a block
	//			 strEndBlock - string that indicates the end of a block
	//			 vecResults - vector for the results of all blocks found in the file
	static void getAllEnclosedBlocks(const std::string &strFilename, 
	 			  					 const std::string &strStartBlock, 
									 const std::string &strEndBlock, 
									 std::vector<std::list<std::string> > &vecResults);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  To read all blocks enclosed by the start and end parameters from a file and place 
	//			 them into a vector of these block's contents
	// PROMISE:  Populate vecResults with all of the text contained between the start and end blocks
	// ARGS:	 textFile - text file to read from
	//			 strStartBlock - string that indicates the start of a block
	//			 strEndBlock - string that indicates the end of a block
	//			 vecResults - vector for the results of all blocks found in the file
	static void getAllEnclosedBlocks(std::ifstream &textFile, 
	 			  					 const std::string &strStartBlock, 
									 const std::string &strEndBlock, 
									 std::vector<std::list<std::string> > &vecResults);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  To read all blocks enclosed by the start and end parameters from a file and place 
	//			 them into a vector of these block's contents
	// PROMISE:  Populate vecResults with all of the text contained between the start and end blocks
	// ARGS:	 lstTextFile - lst of strings created from a text file that contains blocks to be read
	//			 strStartBlock - string that indicates the start of a block
	//			 strEndBlock - string that indicates the end of a block
	//			 vecResults - vector for the results of all blocks found in the file
	//			 bExtractFromFile - flag to indicate whether the blocks should be removed from the
	//								lstTextFile
	static void getAllEnclosedBlocks(std::list<std::string> &lstTextFile, 
	 			  					 const std::string &strStartBlock, 
									 const std::string &strEndBlock, 
									 std::vector<std::list<std::string> > &vecResults,
									 bool bExtractFromFile = false);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:  To remove all blocks enclosed by the start and end parameters from a file
	// PROMISE:  Populate vecResults with all of the text contained between the start and end blocks
	// ARGS:	 lstTextFile - lst of strings created from a text file that contains blocks to be read
	//			 strStartBlock - string that indicates the start of a block
	//			 strEndBlock - string that indicates the end of a block
	static void removeAllEnclosedBlocks(std::list<std::string> &lstTextFile,
										const std::string &strStartBlock, 
										const std::string &strEndBlock);
};