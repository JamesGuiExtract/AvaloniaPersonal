
#include "stdafx.h"
#include "ThreadSafeLogFile.h"
#include "ByteStream.h"
#include "cpputil.h"
#include "UCLIDException.h"

#include <fstream>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Global and Static Data
//-------------------------------------------------------------------------------------------------
// Locking object for thread safety
Win32CriticalSection ThreadSafeLogFile::ms_cs;
map<std::string, unsigned long> ThreadSafeLogFile::ms_mapLogFileNameToID;
map<unsigned long, std::vector<std::string> > ThreadSafeLogFile::ms_mapLogFileIDToLines;
map<unsigned long, LONG> ThreadSafeLogFile::ms_mapLogFileIDToUsageCount;
extern AFX_EXTENSION_MODULE BaseUtilsDLL;

// INI file settings
const string gstrTHREAD_SAFE_SECTION_NAME = "ThreadSafeLogging";
const string gstrTHREAD_SAFE_LOGGING_ENABLED = "LoggingEnabled";

//-------------------------------------------------------------------------------------------------
// Thread that writes the entries from the memory to the log file
//-------------------------------------------------------------------------------------------------
UINT asyncFileWritingThread(LPVOID pData)
{
	try
	{
		ThreadSafeLogFile *pLogFile = (ThreadSafeLogFile *) pData;
		pLogFile->keepWritingLoggedTextToDisk();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19414")

	return 0;
}

//-------------------------------------------------------------------------------------------------
// ThreadSafeLogFile public methods
//-------------------------------------------------------------------------------------------------
ThreadSafeLogFile::ThreadSafeLogFile(const std::string& strPath, bool bEncrypt)
:	m_strLogFileName(strPath),
	m_bEncrypt(bEncrypt),
	m_ulLogFileID(0)
{
	// initialize
	init();
}
//-------------------------------------------------------------------------------------------------
ThreadSafeLogFile::ThreadSafeLogFile()
:	m_bEncrypt(false),
	m_ulLogFileID(0)
{
	// if the default ctor is used, then assume that the user wants to log
	// text to the default log file
	m_strLogFileName = getDefaultLogFileName();

	// initialize
	init();
}
//-------------------------------------------------------------------------------------------------
ThreadSafeLogFile::~ThreadSafeLogFile()
{
	try
	{
		// ensure that only one thread is in here at any given time accessing the static variables
		Win32CriticalSectionLockGuard lock(ms_cs);

		// Only cleanup map member 
		// if log file name has been defined
		if (!m_strLogFileName.empty())
		{
			// decrement the usage count associated with the particular log file
			ms_mapLogFileIDToUsageCount[m_ulLogFileID]--;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16409");
}
//-------------------------------------------------------------------------------------------------
void ThreadSafeLogFile::init()
{
	try
	{
		// ensure that only one thread is in here at any given time accessing the static variables
		Win32CriticalSectionLockGuard lock(ms_cs);

		// Check for logging enabled
		if (!isLoggingEnabled())
		{
			return;
		}

		// Only initialize map members and start the thread 
		// if log file name has been defined
		if (m_strLogFileName.empty())
		{
			return;
		}

		// Make sure that directory of log file exists
		string strDirectory = getDirectoryFromFullPath( m_strLogFileName );
		createDirectory( strDirectory, true );

		static unsigned long ls_ulLastUsedID = 0;
		
		// check to see if an instance of this object has already been initialized with the
		// same log file name specified to this instance.
		string strTemp = m_strLogFileName;
		makeUpperCase(strTemp);

		std::map<std::string, unsigned long>::iterator iter;
		iter = ms_mapLogFileNameToID.find(strTemp);
		if (iter == ms_mapLogFileNameToID.end())
		{
			// assign a new ID to the log filename associated with this object
			ls_ulLastUsedID++;
			m_ulLogFileID = ls_ulLastUsedID;
			ms_mapLogFileNameToID[strTemp] = m_ulLogFileID;
			vector<string> vecLines; // empty vector
			ms_mapLogFileIDToLines[m_ulLogFileID] = vecLines;
			ms_mapLogFileIDToUsageCount[m_ulLogFileID] = 1;

			// Begin separate thread that actually writes the data to the log file
			AfxBeginThread( asyncFileWritingThread, this );
		}
		else
		{
			ms_mapLogFileIDToUsageCount[m_ulLogFileID]++;
			m_ulLogFileID = iter->second;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12808")
}
//-------------------------------------------------------------------------------------------------
// get access to the file, write the line of information, and close the file
void ThreadSafeLogFile::writeLine(const std::string& strLine, bool bIgnoreExceptions)
{
	try
	{
		// Just return if logging is disabled
		if (!isLoggingEnabled())
		{
			return;
		}

		// Construct line of text as :
		//	TIMESTAMP etc | strLine
		string strTS;
		getTimeStamp( strTS );
		string strText( strTS );
		strText += getDelimiter();
		strText += strLine;

		// Encrypt text if desired
		if (m_bEncrypt)
		{
			scramble(strText);
		}

		// Prevent multiple threads from writing to a log file at the same time
		// TODO: enhance this code so that multiple threads can write to different log files
		// at the same time
		Win32CriticalSectionLockGuard lg( ms_cs );

		// Only add this text to the vector
		// if log file name has been defined
		if (!m_strLogFileName.empty())
		{
			ms_mapLogFileIDToLines[m_ulLogFileID].push_back(strText);
		}
	}
	catch (...)
	{
		if (bIgnoreExceptions)
		{
			return;
		}
		else
		{
			// Rethrow the exception to the next scope
			throw;
		}
	}
}
//-------------------------------------------------------------------------------------------------
char ThreadSafeLogFile::getDelimiter()
{
	return '|';
}
//-------------------------------------------------------------------------------------------------
void ThreadSafeLogFile::decrypt(const std::string& strInputFileName, 
								const std::string& strOutputFileName)
{
	// Prevent other threads from accessing the log file
	// TODO: enhance this code so that other threads can access other log files
	// at the same time
	Win32CriticalSectionLockGuard lg(ms_cs);

	// Open the encrypted input file
	ifstream infile(strInputFileName.c_str());
	if (!infile)
	{
		UCLIDException ue("ELI12755", "Unable to open file for reading!");
		ue.addDebugInfo("File", strInputFileName.c_str());
		throw ue;
	}

	// Open the output file
	ofstream outfile(strOutputFileName.c_str());
	if (!outfile)
	{
		UCLIDException ue("ELI12756", "Unable to open file for writing!");
		ue.addDebugInfo("File", strOutputFileName.c_str());
		throw ue;
	}

	// Copy the input file to the output file
	// one line at a time, unscrambling it each time
	while (!infile.eof())
	{
		string strLine("");

		getline(infile, strLine);

		if (!strLine.empty())
		{
			unscramble(strLine);
			outfile << strLine << endl;
		}
	}

	// Close the outfile and wait for it to be readable
	outfile.close();
	waitForFileToBeReadable(strOutputFileName);
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void ThreadSafeLogFile::getTimeStamp(std::string& rTimeStamp)
{
	// String format is:
	//	COMPUTER_NAME | DATE | TIME | PROCESS_ID | THREAD_ID
	rTimeStamp = getComputerName();
	rTimeStamp += getDelimiter();
	rTimeStamp += getDateAsString();
	rTimeStamp += getDelimiter();
	rTimeStamp += getMillisecondTimeAsString();
	rTimeStamp += getDelimiter();
	rTimeStamp += asString(GetCurrentProcessId());
	rTimeStamp += getDelimiter();
	rTimeStamp += asString(GetCurrentThreadId());
}
//-------------------------------------------------------------------------------------------------
const string& ThreadSafeLogFile::getDefaultLogFileName()
{
	static string ls_strDefaultLogFileName;

	try
	{
		// compute the default log file name if it hasn't yet been computed
		if (ls_strDefaultLogFileName.empty())
		{
			ls_strDefaultLogFileName = getExtractApplicationDataPath() + "\\LogFiles\\Default.log";
		}

		// return the name of the default log file
		return ls_strDefaultLogFileName;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12804")

	// If we reached here, it's because of an invalid path.  
	// Return the empty string so that no thread 
	// will be started and no logging will be done.
	return ls_strDefaultLogFileName;
}
//-------------------------------------------------------------------------------------------------
bool ThreadSafeLogFile::isLoggingEnabled()
{
	static bool	ls_bINIChecked = false;
	static bool	ls_bLoggingEnabled = false;

	try
	{
		// Search for INI file containing logging-related settings
		if (!ls_bINIChecked)
		{
			// Accept default of Logging OFF
			ls_bINIChecked = true;

			// Compute the path to the INI file
			string strDefaultINIFileName = getModuleDirectory( BaseUtilsDLL.hModule );
			strDefaultINIFileName += "\\LogFiles.ini";

			// Locate the file
			if (isFileOrFolderValid( strDefaultINIFileName ))
			{
				// Locate Logging key within file, default to 0 for OFF
				long lValue = 0;
				TCHAR pszKeyValue[MAX_PATH];
				::GetPrivateProfileString( gstrTHREAD_SAFE_SECTION_NAME.c_str(), 
					gstrTHREAD_SAFE_LOGGING_ENABLED.c_str(), "0", pszKeyValue, 
					sizeof(pszKeyValue), strDefaultINIFileName.c_str() );

				// Convert retrieved string to integer and bool
				lValue = asLong( pszKeyValue );
				ls_bLoggingEnabled = (lValue == 0) ? false : true;
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12821")

	return ls_bLoggingEnabled;
}
//-------------------------------------------------------------------------------------------------
void ThreadSafeLogFile::scramble(std::string& rstrText)
{
	// Convert the text into a hex string
	ByteStream bs((const unsigned char *) rstrText.c_str(), rstrText.length());
	string strHexString = bs.asString();

	// Confirm that hex string has even # of chars
	int iHexStringLength = strHexString.length();
	if (iHexStringLength % 2 != 0)
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI12757");
	}

	// Reverse the hex string so that the last char is now first and the first is now last
	int iHalfOfHexStringLength = iHexStringLength / 2;
	for (int i = 0; i < iHalfOfHexStringLength; i++)
	{
		char ithChar = strHexString[i];
		int iOppositeCharPos = (iHexStringLength - 1) - i;

		// Transpose the chars at the same time that we are reversing them
		strHexString[i] = transpose( strHexString[iOppositeCharPos] );
		strHexString[iOppositeCharPos] = transpose( ithChar );
	}

	// Provide scrambled string to caller
	rstrText = strHexString;
}
//-------------------------------------------------------------------------------------------------
void ThreadSafeLogFile::unscramble(std::string& rstrText)
{
	// The input is the hex string
	string strHexString = rstrText;

	// Confirm that hex string has even # of chars
	int iHexStringLength = strHexString.length();
	if (iHexStringLength % 2 != 0)
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI12758");
	}

	// Reverse the hex string so that the last char is now first and the first is now last
	int iHalfOfHexStringLength = iHexStringLength / 2;
	for (int i = 0; i < iHalfOfHexStringLength; i++)
	{
		char ithChar = strHexString[i];
		int iOppositeCharPos = (iHexStringLength - 1) - i;

		// Transpose the chars at the same time that we are reversing them
		strHexString[i] = transpose( strHexString[iOppositeCharPos] );
		strHexString[iOppositeCharPos] = transpose( ithChar );
	}

	// Convert the text into a hex string
	ByteStream bs(strHexString);
	rstrText.assign((char *) bs.getData(), bs.getLength());
}
//-------------------------------------------------------------------------------------------------
char ThreadSafeLogFile::transpose(char c)
{
	// If c is a lower case hex char, then convert it to upper case
	if (c >= 'a' && c <= 'f')
	{
		c = 'A' + (c - 'a');
	}

	// Validate 'c' as a hex character
	bool bValidChar = (c >='0' && c <= '9') || (c >='A' && c <= 'F');
	if (!bValidChar)
	{
		UCLIDException ue("ELI12759", "Invalid hex character!");
		ue.addDebugInfo("c", c);
		throw ue;
	}

	// Return the transposed char
	switch (c)
	{
		case '0': return '9';
		case '9': return '0';

		case '1': return 'C';
		case 'C': return '1';

		case '2': return 'F';
		case 'F': return '2';
	
		case '3': return '8';
		case '8': return '3';
	
		case '4': return 'B';
		case 'B': return '4';
	
		case '5': return 'D';
		case 'D': return '5';

		case '6': return 'A';
		case 'A': return '6';
		
		case '7': return 'E';
		case 'E': return '7';
	
	default:
		// we should never reach here
		THROW_LOGIC_ERROR_EXCEPTION("ELI12760")
	}
}
//-------------------------------------------------------------------------------------------------
void ThreadSafeLogFile::keepWritingLoggedTextToDisk()
{
	bool bContinue = true;
	bool bCleanupThreadRelatedObjects = false;

	// cache the log file ID and filename locally so that the while loop below
	// does not need access to the actual ThreadSafeObject itself with which
	// it is associated.
	unsigned long ulLogFileID = m_ulLogFileID;
	string strLogFileName = m_strLogFileName;

	while (bContinue)
	{
		// only write to the log file every so often
		Sleep(100);

		// get access to the vector of lines pending to be written 
		// copy the vector so that we are not keeping other threads waiting
		vector<string> vecLines;
		{
			Win32CriticalSectionLockGuard lg(ms_cs);
			vector<string>& rvecLines = ms_mapLogFileIDToLines[ulLogFileID];
			if (!rvecLines.empty())
			{
				vecLines = rvecLines;
				rvecLines.clear();
			}

			// if all objects writing to this log file have been destructed, 
			// then exit the file-logging thread and update the appropriate structures
			if (ms_mapLogFileIDToUsageCount[ulLogFileID] == 0)
			{
				bCleanupThreadRelatedObjects = true;
				bContinue = false;
			}
		}

		// Continue only if items need to be logged
		if (vecLines.size() != 0)
		{
			// Open the log file
			CStdioFile	log;
			CFileException	e;
			bool		bLogLines = true;
			CTime firstFileOpenAttemptTime = CTime::GetCurrentTime();

			while (true)
			{
				if (!log.Open( strLogFileName.c_str(), 
					CFile::modeCreate | CFile::modeNoTruncate | 
					CFile::modeWrite | CFile::shareExclusive, 
					&e ))
				{
					CTime now = CTime::GetCurrentTime();
					CTimeSpan duration = now - firstFileOpenAttemptTime;

					if (duration.GetTotalSeconds() > 180)
					{
						// Failed to open log file for 3 minutes
						UCLIDException ue("ELI12773", "Unable to open log file!");
						ue.addDebugInfo( "Log", m_strLogFileName );
						int iSize = vecLines.size();
						ue.addDebugInfo( "Item Count", iSize );
						if (iSize > 0)
						{
							ue.addDebugInfo( "First Item", vecLines.at( 0 ) );
						}
						ue.log();
						bLogLines = false;
						break;
					}

					Sleep(0); // yield to other threads
				}
				else
				{
					// Log file opened in exclusive mode, continue with write()
					break;
				}
			}

			// Write all the lines to the log file
			if (bLogLines)
			{
				// Append entries to end of log file
				log.SeekToEnd();
				vector<string>::const_iterator iter;
				for (iter = vecLines.begin(); iter != vecLines.end(); iter++)
				{
					const string& strText = *iter;
					log.WriteString( strText.c_str() );
					log.WriteString( "\n" );
				}

				// Close the log file and wait until it can be read
				log.Close();
				waitForFileToBeReadable(strLogFileName);
			}
		}
	}

	if (bCleanupThreadRelatedObjects)
	{
		Win32CriticalSectionLockGuard lg(ms_cs);
		ms_mapLogFileIDToUsageCount.erase(ulLogFileID);
		ms_mapLogFileIDToLines.erase(ulLogFileID);
		makeUpperCase(strLogFileName);
		ms_mapLogFileNameToID.erase(strLogFileName);
	}
}
//-------------------------------------------------------------------------------------------------
