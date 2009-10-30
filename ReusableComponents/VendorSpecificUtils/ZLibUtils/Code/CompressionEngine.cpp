
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
	char* pszData = NULL;
	try
	{
		try
		{
			// Validate the input files existence
			validateFileOrFolderExistence(strInputFile, "ELI28330");

			// open the file that needs to be compressed
			ifstream infile(strInputFile.c_str(), ios::binary);

			if (!infile)
			{
				UCLIDException ue("ELI06747", "Unable to open file to compress!");
				throw ue;
			}

			// read the contents of the file that need to be compressed
			infile.seekg(0, ios::end);

			streampos endPos = infile.tellg();
			pszData = new char[endPos];
			ASSERT_RESOURCE_ALLOCATION("ELI06746", pszData != NULL);

			infile.seekg(0, ios::beg);

			if (!infile.read(pszData, endPos))
			{
				UCLIDException ue("ELI06755", "Unable to read compressed data from file!");
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
				throw ue;
			}

			// write the compressed output to the file
			if (!gzip.WriteBuffer(pszData, endPos))
			{
				UCLIDException ue("ELI06749", "Unable to write compressed data to file!");
				throw ue;
			}

			// close the compressed file
			if (!gzip.Close())
			{
				UCLIDException ue("ELI06750", "Unable to close compressed file!");
				throw ue;

				// Wait for the output file to be readable
				waitForFileToBeReadable(strOutputFile);
			}

			// Delete the buffer if it was allocated
			if(pszData != NULL)
			{
				delete [] pszData;
				pszData = NULL;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28329");
	}
	catch(UCLIDException& uex)
	{
		// Ensure the buffer gets cleaned up
		if(pszData != NULL)
		{
			delete [] pszData;
			pszData = NULL;
		}

		// Add the input and output file information
		uex.addDebugInfo("File To Compress", strInputFile);
		uex.addDebugInfo("File To Output", strOutputFile);
		throw uex;
	}
}
//-------------------------------------------------------------------------------------------------
void CompressionEngine::decompressFile(const std::string& strInputFile, 
									   const std::string& strOutputFile)
{
	unsigned char* pszBuffer = NULL;
	try
	{
		try
		{
			// Check that the input file exists
			validateFileOrFolderExistence(strInputFile, "ELI28317");

			// Check for 0 byte file
			if (getSizeOfFile(strInputFile) == 0)
			{
				UCLIDException uex("ELI28318", "Cannot decompress a 0 byte file.");
				throw uex;
			}

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
			size_t nSize = 0;
			if (!gzip.ReadBuffer((void **) &pszBuffer, nSize))
			{
				UCLIDException ue("ELI06752", "Unable to decompress data!");
				ue.addDebugInfo("nSize", nSize);
				throw ue;
			}

			// create the decompressed file
			ofstream outfile(strOutputFile.c_str(), ios::binary);

			if (!outfile)
			{
				UCLIDException ue("ELI06753", "Unable to create decompressed file!");
				throw ue;
			}

			// write the decompressed contents to the specified file
			if (!outfile.write((char *)pszBuffer, nSize))
			{
				UCLIDException ue("ELI06754", "Unable to write decompressed data to file!");
				throw ue;
			}
			else
			{
				outfile.close();
				waitForFileToBeReadable(strOutputFile);
			}

			// Delete the buffer if it was allocated
			if(pszBuffer != NULL)
			{
				delete [] pszBuffer;
				pszBuffer = NULL;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28319");
	}
	catch(UCLIDException& uex)
	{
		// Ensure the buffer is cleaned up
		if (pszBuffer != NULL)
		{
			delete [] pszBuffer;
			pszBuffer = NULL;
		}

		// Add the file to decompress and the output file info
		uex.addDebugInfo("File To Decompress", strInputFile);
		uex.addDebugInfo("File To Output", strOutputFile);
		throw uex;
	}
}
//-------------------------------------------------------------------------------------------------
