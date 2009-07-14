
#include "stdafx.h"
#include "CompressionEngine.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <fstream>
using namespace std;

#include <gzip.h>
using namespace zlib;

//-------------------------------------------------------------------------------------------------
void CompressionEngine::compressFile(const std::string& strInputFile, 
									 const std::string& strOutputFile)
{
	// open the file that needs to be compressed
	ifstream infile(strInputFile.c_str(), ios::binary);

	if (!infile)
	{
		UCLIDException ue("ELI06747", "Unable to open file to compress!");
		ue.addDebugInfo("File", strInputFile);
		throw ue;
	}

	// read the contents of the file that need to be compressed
	infile.seekg(0, ios::end);
	
	streampos endPos = infile.tellg();
	char *pszData = new char[endPos];
	ASSERT_RESOURCE_ALLOCATION("ELI06746", pszData != NULL);

	try
	{
		infile.seekg(0, ios::beg);
		
		if (!infile.read(pszData, endPos))
		{
			UCLIDException ue("ELI06755", "Unable to read compressed data from file!");
			ue.addDebugInfo("File", strInputFile);
			throw ue;
		}
		else
		{
			infile.close();
		}

		// create an instance of the gZip compression engine and create
		// the compressed file
		CGZip gzip;
		if (!gzip.Open(strOutputFile.c_str(), CGZip::ArchiveModeWrite))
		{
			UCLIDException ue("ELI06748", "Unable to create compressed file!");
			ue.addDebugInfo("File", strOutputFile);
			throw ue;
		}

		// write the compressed output to the file
		if (!gzip.WriteBuffer(pszData, endPos))
		{
			UCLIDException ue("ELI06749", "Unable to write compressed data to file!");
			ue.addDebugInfo("File", strOutputFile);
			throw ue;
		}

		// close the compressed file
		if (!gzip.Close())
		{
			UCLIDException ue("ELI06750", "Unable to close compressed file!");
			ue.addDebugInfo("File", strOutputFile);
			throw ue;
		}

		// Wait for the output file to be readable
		waitForFileToBeReadable(strOutputFile);
	}
	catch(...)
	{
		if(pszData != NULL)
		{
			delete [] pszData;
			pszData = NULL;
		}
		throw;
	}

	if(pszData != NULL)
	{
		delete [] pszData;
	}
}
//-------------------------------------------------------------------------------------------------
void CompressionEngine::decompressFile(const std::string& strInputFile, 
									   const std::string& strOutputFile)
{
	// open the compressed file
	CGZip gzip;
	if (!gzip.Open(strInputFile.c_str(), CGZip::ArchiveModeRead))
	{
		// Get the retry count and time out value
		int iRetryCount, iTimeOut;
		getFileAccessRetryCountAndTimeout(iRetryCount, iTimeOut);

		int iRetries=0;
		bool bOpened = false;
		do
		{
			// Attempt to open the file
			bOpened = gzip.Open(strInputFile.c_str(), CGZip::ArchiveModeRead);

			// Check if the file was opened
			if (!bOpened)
			{
				// Check if the retry limit has been reached
				if (iRetries > iRetryCount)
				{
					UCLIDException ue("ELI06751", "Unable to open compressed file!");
					ue.addDebugInfo("File", strInputFile);
					ue.addDebugInfo("Retries", iRetries);
					ue.addDebugInfo("Max Retries", iRetryCount);
					throw ue;
				}

				iRetries++;
				Sleep(iTimeOut);
			}
		}
		while(!bOpened);

		// Opened the file successfully after retry, log the information
		UCLIDException ue("ELI24062",
			"Application Trace: Opened compressed file after retry.");
		ue.addDebugInfo("Retries", iRetries+1);
		ue.log();
	}

	// read the contents of the compressed file and decompress it
	unsigned char* pszBuffer = NULL;
	size_t nSize = 0;

	try
	{
		if (!gzip.ReadBuffer((void **) &pszBuffer, nSize))
		{
			UCLIDException ue("ELI06752", "Unable to decompress data!");
			ue.addDebugInfo("File", strInputFile);
			ue.addDebugInfo("nSize", nSize);
			throw ue;
		}

		// create the decompressed file
		ofstream outfile(strOutputFile.c_str(), ios::binary);

		if (!outfile)
		{
			UCLIDException ue("ELI06753", "Unable to create decompressed file!");
			ue.addDebugInfo("File", strOutputFile);
			throw ue;
		}

		// write the decompressed contents to the specified file
		if (!outfile.write((char *)pszBuffer, nSize))
		{
			UCLIDException ue("ELI06754", "Unable to write decompressed data to file!");
			ue.addDebugInfo("File", strOutputFile);
			throw ue;
		}
		else
		{
			outfile.close();
			waitForFileToBeReadable(strOutputFile);
		}
	}
	catch(...)
	{
		if(pszBuffer != NULL)
		{
			delete [] pszBuffer;
			pszBuffer = 0;
		}
		throw;
	}
	
	if(pszBuffer != NULL)
	{
		delete [] pszBuffer;
	}
}
//-------------------------------------------------------------------------------------------------
