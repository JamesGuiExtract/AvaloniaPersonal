#include "stdafx.h"
#include "BlockExtractor.h"
#include "cpputil.h"
#include "UCLIDException.h"

//-------------------------------------------------------------------------------------------------
void BlockExtractor::getAllEnclosedBlocks(const std::string &strFilename, 
	 			  					   const std::string &strStartBlock, 
									   const std::string &strEndBlock, 
									   std::vector<std::list<std::string> > &vecResults)
{
	std::ifstream inputFile(strFilename.c_str());

	if(!inputFile.is_open())
	{
		UCLIDException ue("ELI12294", "Unable to open file!");
		ue.addDebugInfo("Filename", strFilename);
		throw ue;
	}

	getAllEnclosedBlocks(inputFile, strStartBlock, strEndBlock, vecResults);

	inputFile.close();
}
//-------------------------------------------------------------------------------------------------
void BlockExtractor::getAllEnclosedBlocks(std::ifstream &textFile, 
	 			  					   const std::string &strStartBlock, 
									   const std::string &strEndBlock, 
									   std::vector<std::list<std::string> > &vecResults)
{
	std::list<std::string> lstInputFile;

	convertFileToListOfStrings(textFile, lstInputFile);

	getAllEnclosedBlocks(lstInputFile, strStartBlock, strEndBlock, vecResults, false);
}
//-------------------------------------------------------------------------------------------------
void BlockExtractor::getAllEnclosedBlocks(std::list<std::string> &lstTextFile, 
									   const std::string &strStartBlock, 
									   const std::string &strEndBlock, 
									   std::vector<std::list<std::string> > &vecResults,
									   bool bExtractFromFile)
{
	if((strStartBlock == "") || (strEndBlock == ""))
	{
		UCLIDException ue("ELI12295", "Start block and/or End block cannot be an empty string!");
		ue.addDebugInfo("StartBlock", strStartBlock);
		ue.addDebugInfo("EndBlock", strEndBlock);
		throw ue;
	}

	std::list<std::string>::iterator listIter = lstTextFile.begin();
	bool bStartBlockFound = false;

	// cycle through the entire file
	while(listIter != lstTextFile.end())
	{
		std::list<std::string>::iterator startIter = listIter;
		std::string strCurLine(*listIter);
		unsigned int uiStartPos = strCurLine.find(strStartBlock);

		// if specified start block found store each line until the end block is found
		// then push it into the results vector
		if (uiStartPos != std::string::npos)
		{
			bStartBlockFound = true;

			std::string strBeforeBlock = strCurLine.substr(0, uiStartPos);
			std::string strAfterBlock = strCurLine.substr(uiStartPos + strStartBlock.size(), 
				strCurLine.size() - (uiStartPos + strStartBlock.size()));
			std::list<std::string> lstBlockContents;

			unsigned int uiEndPos = strAfterBlock.find(strEndBlock);
			
			// end and start block on same line
			if (uiEndPos != std::string::npos)
			{
				std::string strBlockContents = strAfterBlock.substr( 0, uiEndPos );

				if(strBlockContents != "")
				{
					lstBlockContents.push_back(strBlockContents);
				}

				strAfterBlock = strAfterBlock.substr( uiEndPos + strEndBlock.size(), 
					strAfterBlock.size() - (uiEndPos + strEndBlock.size()) );

				std::string strLine = strBeforeBlock + strAfterBlock;

				// remove empty lines
				if (strLine != "")
				{
					*listIter = strLine;
				}
				else
				{
					listIter = lstTextFile.erase(listIter);
				}
				vecResults.push_back(lstBlockContents);
				bStartBlockFound = false;
				listIter--;
			}
			else
			{
				if (strBeforeBlock != "")
				{
					lstTextFile.insert(listIter, strBeforeBlock);
				}

				*listIter = strAfterBlock;
														
				// cycle until end block is found
				while(listIter != lstTextFile.end())
				{
					strCurLine = *listIter;

					unsigned int uiEndPos = strCurLine.find(strEndBlock);

					if (uiEndPos != std::string::npos)
					{
						strBeforeBlock = strCurLine.substr(0, uiEndPos);
						strAfterBlock = strCurLine.substr(uiEndPos + strEndBlock.size(), 
							strCurLine.size() - (uiEndPos + strEndBlock.size()));
						
						if (strBeforeBlock != "")
						{
							lstBlockContents.push_back(strBeforeBlock);
						}

						// remove line from list if extraction is turned on
						if (bExtractFromFile)
						{
							if (strAfterBlock == "")
							{
								listIter = lstTextFile.erase(startIter, ++listIter);
							}
							else
							{
								listIter = lstTextFile.erase(startIter, listIter);
								*listIter = strAfterBlock;
							}
						}

						vecResults.push_back(lstBlockContents);
						listIter--;
						bStartBlockFound = false;
						break;
					}
					else
					{
						if (strCurLine != "")
						{
							lstBlockContents.push_back(strCurLine);
						}
					}
					listIter++;
				}
			}
		}
		listIter++;

		if (bStartBlockFound)
		{
			UCLIDException ue("ELI11823", "Start block missing end block!");
			ue.addDebugInfo("StartBlock", strStartBlock);
			throw ue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void BlockExtractor::removeAllEnclosedBlocks(std::list<std::string> &lstTextFile,
								  		  const std::string &strStartBlock, 
										  const std::string &strEndBlock)
{
	if((strStartBlock == "") || (strEndBlock == ""))
	{
		UCLIDException ue("ELI12296", "Start block and/or End block cannot be an empty string!");
		ue.addDebugInfo("StartBlock", strStartBlock);
		ue.addDebugInfo("EndBlock", strEndBlock);
		throw ue;
	}

	std::list<std::string>::iterator listIter = lstTextFile.begin();

	while(listIter != lstTextFile.end())
	{
		if((*listIter).find(strStartBlock) != std::string::npos)
		{
			unsigned int uiStartPos = (*listIter).find(strStartBlock);
			unsigned int uiEndPos = (*listIter).find(strEndBlock);

			// if end of block comment is not on same line
			if (uiEndPos == std::string::npos)
			{
				// keep any part of string prior to the start of comment block
				*listIter = (*listIter).substr( 0, uiStartPos );
				
				// if the line isn't empty keep it
				if (*listIter != "")
				{
					listIter++;
				}

				// keep track of line with start of comment block
				std::list<std::string>::iterator startIter = listIter;

				bool bEndFound = false;

				while(listIter != lstTextFile.end())
				{
					uiEndPos = (*listIter).find(strEndBlock);

					if (uiEndPos != std::string::npos)
					{
						// make the current line the end comment line
						std::list<std::string>::iterator endIter = listIter;
						std::string strAfterBlock((*listIter).substr( uiEndPos + strEndBlock.size(), 
								(*listIter).size() - (uiEndPos + strEndBlock.size()) ));
						
						// if text is found after end of block comment save it else delete the empty line
						// along with the rest of the lines in the comment block
						if (strAfterBlock == "")
						{
							listIter = lstTextFile.erase(startIter, ++endIter);
						}
						else
						{
							*listIter = strAfterBlock;
							listIter = lstTextFile.erase(startIter, endIter);
						}
						bEndFound = true;
						break;
					}
					listIter++;
				}
				
				if (bEndFound == false)
				{
					UCLIDException ue("ELI12297", "Block missing end of block indicator!");
					ue.addDebugInfo("End Block", strEndBlock);
					throw ue;
				}
			}
			else // end of block comment was on the same line
			{
				std::string strBeforeBlock = (*listIter).substr(0, uiStartPos);
				std::string strAfterBlock = (*listIter).substr( uiEndPos + strEndBlock.size(), 
						(*listIter).size() - (uiEndPos + strEndBlock.size()) );

				*listIter = strBeforeBlock + strAfterBlock;

				// if no other text is on the line remove that line
				if (*listIter == "")
				{
					listIter = lstTextFile.erase(listIter);
				}
			}
		}
		else
		{
			listIter++;
		}
	}
}
//-------------------------------------------------------------------------------------------------
