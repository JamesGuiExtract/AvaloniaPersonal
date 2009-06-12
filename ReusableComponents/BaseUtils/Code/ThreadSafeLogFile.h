
#pragma once

#include "Win32CriticalSection.h"
#include "Win32Event.h"

#include <string>
#include <vector>
#include <map>
//#include <list>

class EXPORT_BaseUtils ThreadSafeLogFile
{
public:
	// Set filename and encryption requirement
	ThreadSafeLogFile(); // write to default log file, unencrypted
	ThreadSafeLogFile(const std::string& strPath, bool bEncrypt=false);
	~ThreadSafeLogFile();

	// Open the file, write the line of information, and close the file
	void writeLine(const std::string& strLine, bool bIgnoreExceptions = true);

	// Provide delimiter character used between getTimeStamp() elements
	// and also between collected elements and writeLine() text
	static char getDelimiter();

	// Provide decrypted log file contents to strFileName
	static void decrypt(const std::string& strInputFileName, const std::string& strOutputFileName);

private:

	void init();

	///////
	// Data
	///////
	// Critical section used to protect against simultaneous to any of these static variables
	static Win32CriticalSection ms_cs;
	static std::map<std::string, unsigned long> ms_mapLogFileNameToID;
	static std::map<unsigned long, std::vector<std::string> > ms_mapLogFileIDToLines;
	static std::map<unsigned long, LONG> ms_mapLogFileIDToUsageCount;

	// Path to actual log file
	std::string	m_strLogFileName;

	// True if each line in log file is to be encrypted
	bool		m_bEncrypt;

	unsigned long m_ulLogFileID;

	//////////
	// Methods
	//////////

	// Provides Computer + Date + Time + Process ID + Thread ID
	void getTimeStamp(std::string& rTimeStamp);

	// Defines default log file and creates directory if needed
	static const std::string& getDefaultLogFileName();

	// Finds and reads INI file checking for logging ON / OFF
	// Absence of INI file or key implies logging is OFF
	bool	isLoggingEnabled();

	// Used to encrypt line of text before writing to m_strLogFileName
	static void scramble(std::string& rstrText);

	// Used to decrypt line of log file text
	static void unscramble(std::string& rstrText);

	// Encrypt/decrypt method used by scramble() and unscramble()
	static char transpose(char c);

	friend UINT asyncFileWritingThread(LPVOID pData);
	void keepWritingLoggedTextToDisk();
};
