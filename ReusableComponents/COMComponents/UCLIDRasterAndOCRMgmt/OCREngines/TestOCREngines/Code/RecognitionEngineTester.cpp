#include "stdafx.h"
#include "RecognitionEngineTester.h"

#include <CommentedTextFileReader.h>
#include <StringTokenizer.hpp>
#include <UCLIDException.hpp>
#include <cpputil.hpp>
#include <TemporaryFileName.h>

#include <fstream>
#include <iostream>
#include <io.h>

using namespace std;

//--------------------------------------------------------------------------------------------------
RecognitionEngineTester::RecognitionEngineTester(const std::string& strInputFile)
: m_strInputFileName(strInputFile),
  m_nNumOfCasesSucceeded(0),
  m_nNumOfCasesFailed(0)
{
	try
	{
		// check for validity of the file
		// if the file is not readable
		validateFile(strInputFile);
		
		m_ipZoneExtracter.CreateInstance("UCLIDRasterAndOCRMgmt.ZoneExtracter.1");
		ASSERT_RESOURCE_ALLOCATION("ELI03399", m_ipZoneExtracter != NULL);
		m_ipOCREngine.CreateInstance("SSOCR.ScansoftOCR.1");
		ASSERT_RESOURCE_ALLOCATION("ELI03400", m_ipOCREngine != NULL);
		m_ipTempRasterZone.CreateInstance("UCLIDRasterAndOCRMgmt.RasterZone.1");
		ASSERT_RESOURCE_ALLOCATION("ELI03401", m_ipTempRasterZone != NULL);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03402")
}
//--------------------------------------------------------------------------------------------------
void RecognitionEngineTester::removeSpaces(std::string& strForClean)
{
	// remove any empty space, tab, carriage returns.
	::replaceVariable(strForClean, " ", "");
	::replaceVariable(strForClean, "\t", "");
	::replaceVariable(strForClean, "\n", "");
	::replaceVariable(strForClean, "\r", "");
}
//--------------------------------------------------------------------------------------------------
string RecognitionEngineTester::getIndent(unsigned long ulSize)
{
	string strResult = "";
	strResult.resize(ulSize, ' ');
	return strResult;
}
//--------------------------------------------------------------------------------------------------
void RecognitionEngineTester::processFile(const std::string& strTestFileName, unsigned long ulIndentSize)
{
	// get full path for strTestFileName
	string strInputFileFullName(::getAbsoluteFileName(m_strInputFileName, strTestFileName));
	
	unsigned long ulCurrentLine = 0;

	try
	{
		try
		{
			// first validate the input file
			validateFile(strInputFileFullName);	
			
			cout << endl;
			cout << getIndent(ulIndentSize) << "********************************************************************" << endl;
			// indicate the start of processing the file
			cout << getIndent(ulIndentSize) << "---Start processing file <" << strInputFileFullName << ">" << endl;
			
			// read the file line by line
			ifstream ifs(strInputFileFullName.c_str());
			CommentedTextFileReader fileReader(ifs, "#", true);
			static vector<string> vecTokens;
			vecTokens.clear();
			do
			{
				string strLineText(fileReader.getLineText());
				if (!strLineText.empty())
				{
					ulCurrentLine++;
					// check the first word from current line
					vecTokens = StringTokenizer::sGetTokens(strLineText, '|');
					if (vecTokens.size() < 2)
					{
						UCLIDException uclidException("ELI03397", "Invalid line of text");
						uclidException.addDebugInfo("LineText", strLineText);
						throw uclidException;
					}
					
					// if it's "process,", then call processFile() recursively
					if (strcmpi(vecTokens[0].c_str(), "process") == 0 && vecTokens.size() == 2)
					{
						processFile(vecTokens[1], ulIndentSize + 2);
					}
					else if (strcmpi(vecTokens[0].c_str(), "test") == 0 
							 && vecTokens.size() == 9)
					{
						// remove the first item, which is "test"
						vecTokens.erase(vecTokens.begin());
						
						try
						{
							try
							{
								// otherwise, process the test 
								processTestZone(vecTokens, ulIndentSize, ulCurrentLine);
							}
							catch (...)
							{
								cout << getIndent(ulIndentSize) << "Line# "  << ulCurrentLine << ", Failed to process line : \"" << strLineText;
								cout << "\". Please refer to c:\\Temp\\UCLIDExceptionLog.uex for detailed error info."<< endl;

								throw;
							}
						}
						CATCH_AND_LOG_ALL_EXCEPTIONS("ELI03422");
					}
					else
					{
						cout << getIndent(ulIndentSize) << "Line# "  << ulCurrentLine << ", Failed to process line : \"" << strLineText;
						cout << "\". Please refer to c:\\Temp\\UCLIDExceptionLog.uex for detailed error info."<< endl;

						UCLIDException uclidException("ELI03398", "Excessive info detected.");
						uclidException.addDebugInfo("LineNumber", ulCurrentLine);
						uclidException.addDebugInfo("LineText", strLineText);
						uclidException.addPossibleResolution("Do you have commas inside expected text?");
						uclidException.log();
					}
				}
			}
			while (ifs);
			
			// indicate the end of the file
			cout << getIndent(ulIndentSize) << "---Finish processing file <" << strInputFileFullName << ">" << endl;
			cout << getIndent(ulIndentSize) << "********************************************************************" << endl;
			cout << endl;
		}
		catch (...)
		{
			cout << getIndent(ulIndentSize) << "Failed to process file <" << strInputFileFullName;
			cout << ">. Please refer to c:\\Temp\\UCLIDExceptionLog.uex for detailed error info." << endl;
			
			throw;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI03421")
}
//--------------------------------------------------------------------------------------------------
void RecognitionEngineTester::processTestZone(const vector<string>& vecTokens, 
											  unsigned long ulIndentSize,
											  unsigned long ulCurrentLineNumber)
{
	// first assume that the case might be failed. We'll decrement
	// the number if the case is actually succeeded.
	m_nNumOfCasesFailed ++;
	
	// get the full path of the image file
	string strFullImageFileName(::getAbsoluteFileName(m_strInputFileName, vecTokens[0]));
	validateFile(strFullImageFileName);
	
	m_ipTempRasterZone->StartX = ::asLong(vecTokens[1]);
	m_ipTempRasterZone->StartY = ::asLong(vecTokens[2]);
	m_ipTempRasterZone->EndX = ::asLong(vecTokens[3]);
	m_ipTempRasterZone->EndY = ::asLong(vecTokens[4]);
	m_ipTempRasterZone->Height = ::asLong(vecTokens[5]);
	// page number as 7th item
	m_ipTempRasterZone->PageNumber = ::asLong(vecTokens[6]);
	
	// get image extension from image file for creating temp image file
	string strImageExtension(::getExtensionFromFullPath(strFullImageFileName));
	TemporaryFileName zoneImageFile(NULL , strImageExtension.c_str(), true);
	
	// extract the zone out from original image and put it into a temp image file
	m_ipZoneExtracter->LoadImage(_bstr_t(strFullImageFileName.c_str()));
	m_ipZoneExtracter->ExtractZoneAsImage(m_ipTempRasterZone, _bstr_t(zoneImageFile.getName().c_str()));
	// recognize the zone image and get the output text
	_bstr_t _bstrRecognizedText(m_ipOCREngine->RecognizeTextInImage(_bstr_t(zoneImageFile.getName().c_str())));
	
	// unload the image once the recognition is done
	m_ipZoneExtracter->UnloadImage();
	
	// TODO : may consider using some technique to post-process 
	// the original output text from OCR engine, for example, correcting
	// the text, remove extra space, etc.
	
	// here, we simply remove any space, tab and carriage return in the string
	string strOutputText((LPCTSTR)_bstrRecognizedText);
	removeSpaces(strOutputText);
	
	// retrieve the expected output text from the line text
	string strExpectedText(vecTokens[7]);

	string strTempExpectedText(strExpectedText);
	// remove any spaces, tabs and carriage returns from the string
	removeSpaces(strExpectedText);
	
	cout << getIndent(ulIndentSize) << ulCurrentLineNumber;
	// compare the actual output text with the expected output text
	if (strcmpi(strOutputText.c_str(), strExpectedText.c_str()) == 0)
	{
		cout << ", Pass : ";
		m_nNumOfCasesSucceeded ++;
		m_nNumOfCasesFailed --;
	}
	else
	{
		cout << ", Failed : ";
	}
	
	string strTempOutput((LPCTSTR)_bstrRecognizedText);
	replaceVariable(strTempOutput, "\n", "");
	replaceVariable(strTempOutput, "\r", "");
	cout << strTempOutput << " vs. " << strTempExpectedText << endl;
}
//--------------------------------------------------------------------------------------------------
void RecognitionEngineTester::validateFile(const std::string& strInputFile)
{
	if (_access(strInputFile.c_str(), 04) != 0)
	{
		// if the file exists
		if (_access(strInputFile.c_str(), 00) == 0)
		{
			UCLIDException uclidException("ELI03395", "The input file can't be open for read.");
			uclidException.addDebugInfo("InputFile", strInputFile);
			throw uclidException;
		}
		else
		{
			UCLIDException uclidException("ELI03396", "The input file doesn't exist.");
			uclidException.addDebugInfo("InputFile", strInputFile);
			throw uclidException;
		}
	}
}
//--------------------------------------------------------------------------------------------------
