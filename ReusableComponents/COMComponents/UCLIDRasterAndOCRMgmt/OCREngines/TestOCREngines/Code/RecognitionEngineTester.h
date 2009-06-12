#pragma once

#include <string>
#include <vector>

class UCLIDException;

class RecognitionEngineTester
{
public:
	// strInputFile -- must be a fully qualified file path
	RecognitionEngineTester(const std::string& strInputFile);
	// get the total number of succeeded cases
	long getNumberOfSucceededCases() {return m_nNumOfCasesSucceeded;}
	// get the total number of failed cases
	long getNumberOfFailedCases() {return m_nNumOfCasesFailed;}
	// parses the input file line by line and call appropriate method
	// to process the testing
	// strTestFileName -- could be either full qualified file name or 
	// relative file path.
	void processFile(const std::string& strTestFileName, unsigned long ulIndentSize);

private:
	//*****************************************
	// Helper functions

	// gets specified number of spaces
	std::string getIndent(unsigned long ulSize);

	// takes care of each test case, which includes extracting zone 
	// out from the image file, recognizing
	// the zone, and finally comparing the recognized the text with
	// actual text.
	void processTestZone(const std::vector<std::string>& vecTokens, 
						 unsigned long ulIndentSize,
						 unsigned long ulCurrentLineNumber);

	// clean string
	void removeSpaces(std::string& strForClean);

	// validate existence and readablity of the input file
	void validateFile(const std::string& strInputFile);


	//*****************************************
	// Member variables

	// to extract a RasterZone out from a given image
	IZoneExtracterPtr m_ipZoneExtracter;
	// to recognize a given image file and retrieve the output string
	IOCREnginePtr m_ipOCREngine;
	// temp zone for multiple uses.
	IRasterZonePtr m_ipTempRasterZone;

	// the input file name in full path.
	std::string m_strInputFileName;

	// internal count for succeeded cases and failed cases
	long m_nNumOfCasesSucceeded;
	long m_nNumOfCasesFailed;
};