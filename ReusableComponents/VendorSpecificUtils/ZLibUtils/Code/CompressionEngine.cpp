
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
			static UCLIDException ueInsufficientMemory("ELI29530",
				"Unable to create compressed file. Insufficient memory available.");

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
			unique_ptr<char[]> pszData(new char[(unsigned int)endPos]);
			ASSERT_RESOURCE_ALLOCATION("ELI06746", pszData.get() != __nullptr);

			infile.seekg(0, ios::beg);

			if (!infile.read(pszData.get(), endPos))
			{
				UCLIDException ue("ELI06755", "Unable to read to-be-compressed data from file!");
				throw ue;
			}
			else
			{
				infile.close();
			}

			// create an instance of the gZip compression engine and create
			// the compressed file
			CGZip gzip;
			bool bInsufficientMemory = false;
			if (!gzip.Open(strOutputFile.c_str(), CGZip::ArchiveModeWrite, &bInsufficientMemory))
			{
				if (bInsufficientMemory)
				{
					throw ueInsufficientMemory;
				}
				else
				{
					UCLIDException ue("ELI06748", "Unable to create compressed file!");
					throw ue;
				}
			}

			// write the compressed output to the file
			if (!gzip.WriteBuffer(pszData.get(), (size_t) endPos))
			{
				UCLIDException ue("ELI06749", "Unable to write compressed data to file!");
				throw ue;
			}

			// close the compressed file
			if (!gzip.Close())
			{
				UCLIDException ue("ELI06750", "Unable to close compressed file!");
				throw ue;
			}

			// Wait for the output file to be readable
			waitForFileToBeReadable(strOutputFile);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28329");
	}
	catch(UCLIDException& uex)
	{
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
	try
	{
		try
		{
			static UCLIDException ueInsufficientMemory("ELI29531",
				"Unable to open compressed file. Insufficient memory available.");

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
			bool bInsufficientMemory = false;
			if (!gzip.Open(strInputFile.c_str(), CGZip::ArchiveModeRead, &bInsufficientMemory))
			{
				if (bInsufficientMemory)
				{
					throw ueInsufficientMemory;
				}

				// Get the retry count and time out value
				int iRetryCount, iTimeOut;
				getFileAccessRetryCountAndTimeout(iRetryCount, iTimeOut);

				int iRetries=0;
				bool bOpened = false;
				do
				{
					// Attempt to open the file
					bOpened = gzip.Open(strInputFile.c_str(), CGZip::ArchiveModeRead, &bInsufficientMemory);

					// Check if the file was opened
					if (!bOpened)
					{
						if (bInsufficientMemory)
						{
							throw ueInsufficientMemory;
						}

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
			unique_ptr<char[]> pszBuffer(__nullptr);
			char* pszTemp = __nullptr;
			size_t nSize = 0;
			bool bSuccess = gzip.ReadBuffer((void **) &pszTemp, nSize) != 0;
			pszBuffer.reset(pszTemp); // Make unique pointer owner of the allocated data
			if (!bSuccess)
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
			if (!outfile.write(pszBuffer.get(), nSize))
			{
				UCLIDException ue("ELI06754", "Unable to write decompressed data to file!");
				throw ue;
			}
			else
			{
				outfile.close();
				waitForFileToBeReadable(strOutputFile);
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28319");
	}
	catch(UCLIDException& uex)
	{
		// Add the file to decompress and the output file info
		uex.addDebugInfo("File To Decompress", strInputFile);
		uex.addDebugInfo("File To Output", strOutputFile);
		throw uex;
	}
}
//-------------------------------------------------------------------------------------------------
bool CompressionEngine::isZipFile(const std::string & strFile)
{
	ifstream stream(strFile);
    std::string result(4, ' ');
    stream.read(&result[0], 4);
	return result == "PK\x3\x4" || result == "PK\x5\x6";
}
//-------------------------------------------------------------------------------------------------
bool CompressionEngine::isGZipFile(const std::string & strFile)
{
	ifstream stream(strFile);
    std::string result(2, ' ');
    stream.read(&result[0], 2);
	return result == "\x1f\x8b";
}
//-------------------------------------------------------------------------------------------------
