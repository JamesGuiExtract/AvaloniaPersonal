//==================================================================================================
//
// COPYRIGHT (c) 2000 - 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	cppFileSystemUtil.cpp
//
// PURPOSE:	Various file system utility functions
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (August 2000 - present)
//
//==================================================================================================

#include "stdafx.h"
#include "cpputil.h"
#include "FileIterator.h"
#include "UCLIDException.h"
#include "TemporaryFileName.h"

#include <Shlwapi.h>
#include <io.h>
#include <sys/utime.h>

#include <afxmt.h>

using namespace std;

//--------------------------------------------------------------------------------------------------
// Defaults for file access
//--------------------------------------------------------------------------------------------------
const string gstrDEFAULT_FILE_ACCESS_RETRIES = "50";
const string gstrDEFAULT_FILE_ACCESS_TIMEOUT = "250"; // milliseconds

const string gstrFILE_ACCESS_RETRIES = "FileAccessRetries";
const string gstrFILE_ACCESS_TIMEOUT = "FileAccessTimeout";

//--------------------------------------------------------------------------------------------------
void getFileAccessRetryCountAndTimeout(int& riRetryCount, int& riRetryTimeout)
{
	static CMutex sMutex;
	static int siTimeout = -1;
	static int siRetries = -1;

	// if timeout or retries are still < 0 read from the registry
	if (siTimeout < 0 || siRetries < 0 )
	{
		// Lock mutex 
		CSingleLock lg(&sMutex, TRUE);

		// Check registry for file access timeout
		RegistryPersistenceMgr machineCfgMgr = RegistryPersistenceMgr( HKEY_LOCAL_MACHINE, "" );

		// Check for existence of file access timeout
		if (!machineCfgMgr.keyExists( gstrBASEUTILS_REG_PATH, gstrFILE_ACCESS_TIMEOUT ))
		{
			// Not found, set default
			machineCfgMgr.createKey( gstrBASEUTILS_REG_PATH, gstrFILE_ACCESS_TIMEOUT,
				gstrDEFAULT_FILE_ACCESS_TIMEOUT );
		}

		// Retrieve file access timeout
		string strTimeout = machineCfgMgr.getKeyValue( gstrBASEUTILS_REG_PATH,
			gstrFILE_ACCESS_TIMEOUT );
		siTimeout = asLong( strTimeout);

		// Get the number of retries
		// Check for existence of retries
		if (!machineCfgMgr.keyExists( gstrBASEUTILS_REG_PATH, gstrFILE_ACCESS_RETRIES ))
		{
			// Not found, set default
			machineCfgMgr.createKey( gstrBASEUTILS_REG_PATH, gstrFILE_ACCESS_RETRIES,
				gstrDEFAULT_FILE_ACCESS_RETRIES);
		}

		// Retrieve number of retries
		string strRetries = machineCfgMgr.getKeyValue( gstrBASEUTILS_REG_PATH,
			gstrFILE_ACCESS_RETRIES );
		siRetries = asLong( strRetries);
	}

	// Set the values
	riRetryCount = siRetries;
	riRetryTimeout = siTimeout;
}
//--------------------------------------------------------------------------------------------------
unsigned long getDiskSerialNumber()
{
	unsigned long	ulTemp = 0;
	string			strPath( "C:\\" );

	GetVolumeInformation( strPath.c_str(), NULL, NULL, &ulTemp, NULL, NULL, NULL, NULL );

	return ulTemp;
}
//--------------------------------------------------------------------------------------------------
void writeToFile(const string& strData, const string& strOutputFileName)
{
	// create the output file stream
	// [FlexIDSCore:3797] Open as binary to prevent each "\n" char from being converted to "\r\n"
	ofstream ofs(strOutputFileName.c_str(), ofstream::binary);
	
	// make sure the output file stream could be created ok
	if (!ofs)
	{
		string strMsg = "Unable to open file \"";
		strMsg += strOutputFileName;
		strMsg += "\" for writing.";
		throw UCLIDException("ELI00247", strMsg);
	}

	// write the data to the output file stream
	ofs << strData << "\r\n";
	ofs.close();

	// make sure that there were no errors in closing the output file stream
	if (!ofs)
	{
		UCLIDException uex("ELI00255", "Unexpected error occurred while writing to the file.");
		uex.addDebugInfo("Output File Name", strOutputFileName);
		throw uex;
	}

	// Wait until the file is readable
	waitForFileToBeReadable(strOutputFileName);
}
//--------------------------------------------------------------------------------------------------
void appendToFile(const string& strData, const string& strOutputFileName)
{
	// create the output file stream
	// [FlexIDSCore:3797] Open as binary to prevent each "\n" char from being converted to "\r\n"
	ofstream ofs(strOutputFileName.c_str(), ofstream::out | ofstream::app | ofstream::binary);
	 
	// make sure the output file stream could be created ok
	if (!ofs)
	{
		string strMsg = "Unable to open file \"";
		strMsg += strOutputFileName;
		strMsg += "\" for appending.";
		throw UCLIDException("ELI12466", strMsg);
	}

	// write the data to the output file stream
	ofs << strData << "\r\n";
	ofs.close();

	// make sure that there were no errors in closing the output file stream
	if (!ofs)
	{
		UCLIDException uex("ELI12467", "Unexpected error occurred while writing to file.");
		uex.addDebugInfo("Output File Name", strOutputFileName);
		throw uex;
	}

	// Wait until the file is readable
	waitForFileToBeReadable(strOutputFileName);
}
//--------------------------------------------------------------------------------------------------
string removeLastSlashFromPath(string strInput)
{
	size_t nLastChar = strInput.length()-1;
	if (strInput[nLastChar] == '\\')
	{
		strInput.erase(nLastChar);
	}

	return strInput;
}
//--------------------------------------------------------------------------------------------------
string getFileNameWithoutExtension(const string& strFullPathToFile,
								   bool bMakeLowerCase)
{
	char zDrive[_MAX_DRIVE] = {0}, zPath[_MAX_PATH] = {0},
		zFileName[_MAX_FNAME] = {0}, zExt[_MAX_EXT] = {0};

	// Break a path name into components. Trim quotes from path if they exist
	_splitpath_s(trim(strFullPathToFile, "\"", "\"").c_str(), zDrive, zPath, zFileName, zExt);

	string strRet = string(zFileName);
	if (bMakeLowerCase)
	{
		makeLowerCase(strRet);
	}

	return strRet;
}
//--------------------------------------------------------------------------------------------------
string getExtensionFromFullPath(const string& strFullFileName,
								bool bMakeLowerCase)
{
	char zDrive[_MAX_DRIVE] = {0}, zPath[_MAX_PATH] = {0},
		zFileName[_MAX_FNAME] = {0}, zExt[_MAX_EXT] = {0};

	// Break a path name into components. Trim quotes from path if they exist
	_splitpath_s(trim(strFullFileName, "\"", "\"").c_str(), zDrive, zPath, zFileName, zExt);

	// this extension has a leading "."
	string strRet = string(zExt);
	if (bMakeLowerCase)
	{
		makeLowerCase(strRet);
	}

	return strRet;
}
//--------------------------------------------------------------------------------------------------
string getDirectoryFromFullPath(const string& strFullFileName,
								bool bMakeLowerCase)
{
	char zDrive[_MAX_DRIVE] = {0}, zPath[_MAX_PATH] = {0},
		zFileName[_MAX_FNAME] = {0}, zExt[_MAX_EXT] = {0};

	// Break a path name into components. Trim quotes from path if they exist
	_splitpath_s(trim(strFullFileName, "\"", "\"").c_str(), zDrive, zPath, zFileName, zExt);

	// this directory has a trailing "\"
	string strRet = string(zDrive) + string(zPath);
	
	// remove the trailing slash
	strRet = trim(strRet, "", "\\");
	if (bMakeLowerCase)
	{
		makeLowerCase(strRet);
	}

	return strRet;
}
//--------------------------------------------------------------------------------------------------
string getFileNameFromFullPath(const string& strFullFileName,
							   bool bMakeLowerCase)
{
	char zDrive[_MAX_DRIVE] = {0}, zPath[_MAX_PATH] = {0},
		zFileName[_MAX_FNAME] = {0}, zExt[_MAX_EXT] = {0};

	// Break a path name into components. Trim quotes from path if they exist
	_splitpath_s(trim(strFullFileName, "\"", "\"").c_str(), zDrive, zPath, zFileName, zExt);

	string strRet = string(zFileName) + string(zExt);
	if (bMakeLowerCase)
	{
		makeLowerCase(strRet);
	}

	return strRet;
}
//--------------------------------------------------------------------------------------------------
string getPathAndFileNameWithoutExtension(const string& strFullFileName,
										  bool bMakeLowerCase)
{
	char zDrive[_MAX_DRIVE] = {0}, zPath[_MAX_PATH] = {0},
		zFileName[_MAX_FNAME] = {0}, zExt[_MAX_EXT] = {0};

	// Break a path name into components. Trim quotes from path if they exist
	_splitpath_s(trim(strFullFileName, "\"", "\"").c_str(), zDrive, zPath, zFileName, zExt);

	string strRet = string(zDrive) + string(zPath) + string(zFileName);
	if (bMakeLowerCase)
	{
		makeLowerCase(strRet);
	}

	return strRet;
}
//--------------------------------------------------------------------------------------------------
string getDriveFromFullPath(const string& strFullFileName, bool bMakeLowerCase)
{
	// Trim any quotes from the path
	string strFileName = trim(strFullFileName, "\"", "\"");

	// The return string
	string strRet;

	// Check for UNC path (P13 #4610, #4611)
	int nPos = strFileName.find( "\\\\", 0 );
	if (nPos == 0)
	{
		// Full path begins with \\, find the next two backslashes
		nPos = strFileName.find( "\\", nPos + 2 );
		if (nPos != string::npos)
		{
			nPos = strFileName.find( "\\", nPos + 1 );
			if (nPos != string::npos)
			{
				// Found the second backslash, these characters are the Drive
				strRet = strFileName.substr( 0, nPos + 1 );
			}
		}
	}
	else
	{
		// Not a unc path, use the split function
		char zDrive[_MAX_DRIVE] = {0}, zPath[_MAX_PATH] = {0},
			zFileName[_MAX_FNAME] = {0}, zExt[_MAX_EXT] = {0};

		// Break a path name into components. Trim quotes from path if they exist
		_splitpath_s(strFileName.c_str(), zDrive, zPath, zFileName, zExt);

		strRet = string(zDrive) + "\\";
	}

	if (bMakeLowerCase)
	{
		makeLowerCase(strRet);
	}

	return strRet;
}
//--------------------------------------------------------------------------------------------------
bool recursiveRemoveDirectory(const string &strDirectory)
{
	// if the directory doesn't exist, return true immediately.  
	// the purpose of this function is to ENSURE that a directory is gone.
	if (!directoryExists(strDirectory))
	{
		return true;
	}

	bool bRetVal = true;

	vector<string> vecSubDirs;
	
	getAllSubDirsAndDeleteAllFiles(strDirectory, vecSubDirs);
	
	// vecSubDirs now has all the directories we need to delete, and they're in an order that 
	// is safe to delete them.
	// (the order is such that for all parent directories P of a child directory C, 
	// P comes after C in the vector)
	vector<string>::iterator iter;
	for (iter = vecSubDirs.begin(); iter != vecSubDirs.end(); iter++)
	{
		try
		{
			if (RemoveDirectory(iter->c_str()))
			{
				bRetVal = true;
			}
			else
			{
				return false;
			}
		}
		catch (...)
		{
			bRetVal = false;
		}
	}
	return bRetVal;
}
//--------------------------------------------------------------------------------------------------
void createDirectory(const string& strDirectory)
{
	unsigned int uiPrevPos = 0;
	unsigned int uiCurPos = 0;

	// Test for folder already exists
	if (isFileOrFolderValid( strDirectory ))
	{
		return;
	}

	uiCurPos = strDirectory.find_first_of("\\", uiPrevPos);
	if (uiCurPos == string::npos)
	{
		UCLIDException ue("ELI00256", "You must specify a valid directory!");
		ue.addDebugInfo("Directory", strDirectory);
		throw ue;
	}

	// strDirectory may be something like \\frank\internal\common\engineering
	// or something like I:\Common\engineering
	// in either case, the earliest folder that may be missing that we can
	// try to create is the Common folder.  Set iCurPos to the slash before
	// the Common in the above example
	if (strDirectory.find("\\\\") == 0)
	{
		// get to the slash in front of name of the network share
		uiCurPos = strDirectory.find_first_of("\\", 2);
	}
	else if (strDirectory.find(":") == 1)
	{
		// get to the slash that is hopefully immediately after the colon
		uiCurPos = strDirectory.find_first_of("\\", 2);
	}
	else
	{
		UCLIDException ue("ELI09824", "Cannot create directory - invalid directory name!");
		ue.addDebugInfo("strDirectory", strDirectory);
		throw ue;
	}

	while (uiCurPos < strDirectory.length() && uiCurPos!= string::npos)
	{
		string strTempDir;
		uiPrevPos = uiCurPos + 1;
		uiCurPos = strDirectory.find_first_of("\\", uiPrevPos);
		if (uiCurPos == string::npos)
		{
			//if the directory only contains the drive letter (ex. c:\)
			if (strDirectory[uiPrevPos-2] == ':' 
				&& strDirectory.find_last_not_of(" \t") == uiPrevPos-1)
			{
				throw UCLIDException("ELI00257", "You must specify the directory!");
			}
			else
			{
				strTempDir=strDirectory;
			}
		}
		else
		{
			strTempDir = strDirectory.substr(0, uiCurPos+1);
		}

		//if the directory doesn't exist
		// using access() to tell the existence of a directory might work for Windows 95/98
		if (!directoryExists(strTempDir))
		{	
			//call windows API function to create directory
			if(CreateDirectory(strTempDir.c_str(), NULL)==0)
			{
				// Get the error code
				DWORD errorCode = GetLastError();

				// if two threads are trying to create the same directory at the same time, 
				// both threads could have entered the "if (!directoryExists(strTempDir))"
				// code block above, and one of the threads may have executed the CreateDirectory()
				// call above, and the second thread's CreateDirectory call will fail because the
				// directly already exists.  So, when the create directory call fails, before
				// throwing an exception, just double check to make sure that the directory still
				// does not exist.
				if (!directoryExists(strTempDir))
				{
					string strMessage = "The directory \"";
					strMessage += strDirectory;
					strMessage += "\" could not be created!\nPlease specify a valid "
						"and complete path of the directory to create.";
					UCLIDException uex("ELI00258", strMessage);
					uex.addWin32ErrorInfo(errorCode);
					throw uex;
				}
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
bool getAllSubDirsAndDeleteAllFiles(const string &strDirectory, vector<string> &vecSubDirs)
{
	bool bRetVal = true;

	vector<string> vecFilesToRemove;
	vector<string> vecDirectoriesToRecurse;

	// make strDir a non const copy of strDirectory, and make sure it ends in a slash
	string strDir = strDirectory;
	if (strDir[strDir.length() - 1] != '\\' || strDir[strDir.length() - 1] != '/')
	{
		strDir += '\\';
	}
	
	FileIterator fileIter(strDir + "*.*");
	
	// if the directory can't be accessed, return false... from the top level this function 
	// should only be called on directories that exist.  
	if (!fileIter.moveNext())
	{
		return false; 
	}

	// at this point we know the current directory exists, so we push it back into the vector
	// of subdirectories BEFORE we do recursion, which ensures that all its subdirectories will
	// be removed before it.  
	vecSubDirs.push_back(strDirectory);

	// go through all the files / dirs in the current directory and 
	// add them to the appropriate vector
	do 
	{
		// Check that the file name is not "." or ".."
		string strFileName = fileIter.getFileName();
		if(strFileName != "." && strFileName != "..")
		{
			string strSubDir = strDir + strFileName;
			if (fileIter.isDirectory())
			{
				// add the sub directory to a local vector, NOT the vecSubDirs that is being 
				// passed along in the recursion. the directories in this vector will each have 
				// this function called on them recursively, and will have their
				// own chance to add themselves to vecSubDirs at the appropriate time.  
				vecDirectoriesToRecurse.push_back(strSubDir);
			}
			else
			{
				// can't just delete the files now because the handle is still open, 
				// so we add them to a local vector of files to be deleted.
				vecFilesToRemove.push_back(strSubDir);
			}
		}
	}
	while (fileIter.moveNext());
	
	// close the find file handle before trying to delete any files or do any recursion
	fileIter.reset();

	vector<string>::iterator iter;

	// recurse into all immediate sub-directories of this directory
	for (iter = vecDirectoriesToRecurse.begin(); iter != vecDirectoriesToRecurse.end(); iter++)
	{
		if (!recursiveRemoveDirectory(*iter))
		{
			bRetVal = false;
		}
	}

	// delete all files in this directory
	for (iter = vecFilesToRemove.begin(); iter != vecFilesToRemove.end(); iter++)
	{
		if (remove(iter->c_str()))
		{
			bRetVal = false;
		}
	}

	// if any file removals fail at any point, we will return false all the way back out 
	// through the top level call. true is only returned if all files are deleted.  
	// all files will be attempted to be deleted even if the first one fails.  
	return bRetVal;
}
//--------------------------------------------------------------------------------------------------
bool directoryExists(const string &strDir)
{
	// first store the current working directory
	char buf[MAX_PATH] = {0};
	if (GetCurrentDirectory(MAX_PATH, buf) == 0)
	{
		UCLIDException ue("ELI00260", "Unable to get current directory.");
		ue.addWin32ErrorInfo();
		throw ue;
	}

	// change the working directory to strDir, just to find out if strDir exists
	bool bRet = (SetCurrentDirectory(strDir.c_str()) != 0);
	
	// restore the previous working directory
	if (SetCurrentDirectory(buf) == 0)
	{
		UCLIDException ue("ELI00261", "Unable to set directory.");
		ue.addWin32ErrorInfo();
		ue.addDebugInfo("Directory to set", buf);
		throw ue;
	}

	return bRet;
}
//--------------------------------------------------------------------------------------------------
string getClosestNonExistentUniqueFileName(const string& strOriginalFileName)
{
	// seperate the input string into the directory, and
	// the filename
	string strOriginalFileNameDir = getDirectoryFromFullPath(strOriginalFileName);
	string strFileName = getFileNameWithoutExtension(strOriginalFileName);
	string strExtension = strrchr(strOriginalFileName.c_str(), '.');

	// try to create a filename that exists by just appending digits to the filename
	// stop iteration when a file has been created.
	string strLastUsedFileName;
	int i = 1;
	do
	{
		strLastUsedFileName = strOriginalFileNameDir + "\\" + strFileName + 
			asString((unsigned long) i) + strExtension;
		if (_access_s(strLastUsedFileName.c_str(), giMODE_FILE_EXISTS) != 0)
			break;

		// increment the counter and try the next filename
		i++;
	} while (true);

	return strLastUsedFileName;
}
//--------------------------------------------------------------------------------------------------
//copies a specified file to the new directory, ex. copy c:\temp\a.txt to c:\newtemp
void copyFileToNewPath(const string& strOldFile, const string& strNewPath)
{
	string strPath;
	unsigned int uiPos = strNewPath.find_last_not_of(" \t");
	if (uiPos == string::npos)
	{
		throw UCLIDException("ELI00262", "Invalid directory.");
	}
	
	//strip off the ending "\\" if any
	if (strNewPath[uiPos] != '\\')
	{
		strPath = strNewPath;
	}
	else
	{
		strPath = strNewPath.substr( 0, uiPos );
	}

	string strFileName = getFileNameFromFullPath(strOldFile);

	//create the new path if it doesn't exist
	if (!directoryExists(strPath))
	{
		createDirectory(strPath);
	}

	// copy the file
	copyFile(strOldFile, strPath + "\\" + strFileName);
}
//--------------------------------------------------------------------------------------------------
//copies files from old root (deep down to all subdirectories in old root) to new root
void copyFiles(const vector<string>& vecFilesForCopy, 
			   const string& strOldRoot, 
			   const string& strNewRoot)
{
	// no file for copy
	if (vecFilesForCopy.size()==0)
	{
		return;
	}

	if (!directoryExists(strOldRoot))
	{
		throw UCLIDException("ELI00264", strOldRoot + " doesn't exist!");
	}

	// strip off the ending "\\"
	string strTempOldRoot;
	unsigned int uiPos = strOldRoot.find_last_not_of(" \t");
	if (uiPos == string::npos)
	{
		throw UCLIDException("ELI00265", "Invalid directory.");
	}
	
	// strip off the ending "\\" if any
	if (strOldRoot[uiPos] != '\\')
	{
		strTempOldRoot = strOldRoot;
	}
	else
	{
		strTempOldRoot = strOldRoot.substr( 0, uiPos );
	}

	// strip off the ending "\\"
	string strTempNewRoot;
	uiPos = strNewRoot.find_last_not_of(" \t");
	if (uiPos == string::npos)
	{
		throw UCLIDException("ELI00266", "Invalid directory.");
	}
	
	// strip off the ending "\\" if any
	if (strNewRoot[uiPos] != '\\')
	{
		strTempNewRoot = strNewRoot;
	}
	else
	{
		strTempNewRoot = strNewRoot.substr( 0, uiPos );
	}

	vector<string>::const_iterator iterFiles;
	for (iterFiles=vecFilesForCopy.begin(); iterFiles!=vecFilesForCopy.end(); iterFiles++)
	{
		//get the rest part of the string (not include the old root, 
		//ex. file string is c:\temp\a.txt, old root is c:, then the rest part is \temp\a.txt
		//Then add it to the new root
		string strNewFile = strTempNewRoot+(*iterFiles).substr(strTempOldRoot.length());
		//take the path part
		string strNewPath = getDirectoryFromFullPath(strNewFile);
		copyFileToNewPath((*iterFiles), strNewPath);
	}
}
//--------------------------------------------------------------------------------------------------
//copies sub directories from old root (deep down to all subdirectories in old root) to new root
void copySubDirectories(const vector<string>& vecSubDirsForCopy,
						const string& strOldRoot, 
						const string& strNewRoot)
{
	// no directories for copy
	if (vecSubDirsForCopy.size()==0)
	{
		return;
	}

	if (!directoryExists(strOldRoot))
	{
		throw UCLIDException("ELI00267", strOldRoot + " doesn't exist!");
	}

	// strip off the ending "\\"
	string strTempOldRoot;
	unsigned int uiPos = strOldRoot.find_last_not_of(" \t");
	if (uiPos == string::npos)
	{
		throw UCLIDException("ELI00268", "Invalid directory.");
	}
	
	//strip off the ending "\\" if any
	if (strOldRoot[uiPos] != '\\')
	{
		strTempOldRoot = strOldRoot;
	}
	else
	{
		strTempOldRoot = strOldRoot.substr( 0, uiPos );
	}

	// strip off the ending "\\"
	string strTempNewRoot;
	uiPos = strNewRoot.find_last_not_of(" \t");
	if (uiPos == string::npos)
	{
		throw UCLIDException("ELI00269", "Invalid directory.");
	}
	
	// strip off the ending "\\" if any
	if (strNewRoot[uiPos] != '\\')
	{
		strTempNewRoot = strNewRoot;
	}
	else
	{
		strTempNewRoot = strNewRoot.substr( 0, uiPos );
	}

	vector<string>::const_iterator iterDirs;
	for (iterDirs=vecSubDirsForCopy.begin(); iterDirs!=vecSubDirsForCopy.end(); iterDirs++)
	{
		string strNewSubDir = strTempNewRoot+(*iterDirs).substr(strTempOldRoot.length());
		if (!directoryExists(strNewSubDir))
		{
			createDirectory(strNewSubDir);
		}
	}
}
//--------------------------------------------------------------------------------------------------
string getAbsoluteFileName(string strParentFile, const string& strRelativeFile,
						   bool bValidate)
{
	// if strRelativeFile is a fully specified file name, return
	// example: "d:\temp\1.tif", "d:\temp\abc\..\1.tif" == "d:\temp\1.tif"
	unsigned int uiPosition = strRelativeFile.find(":\\");
	if (uiPosition != string::npos)
	{
		// validate the absolute path, if requested by the caller
		if (bValidate)
		{
			validateFileOrFolderExistence(strRelativeFile);
		}

		return strRelativeFile;
	}

	// if strRelativeFile is on a network drive, return
	// Example: "\\frank\files\1.tif"
	uiPosition = strRelativeFile.find("\\\\");
	if (uiPosition == 0)
	{
		// validate the absolute path, if requested by the caller
		if (bValidate)
		{
			validateFileOrFolderExistence(strRelativeFile);
		}

		return strRelativeFile;
	}

	// if the "parent file" is not fully specified, then specify
	// it to be in the current directory
	if (strParentFile.find('\\') == string::npos)
	{
		strParentFile.insert(0, ".\\");
	}

	// get the fully specified path out from the parent file name
	strParentFile = getDirectoryFromFullPath(strParentFile);

	string strRet = strParentFile + "\\" + strRelativeFile;
	// create the full/absolute file name
	char pszFullFileName[_MAX_PATH];
	if (_fullpath(pszFullFileName, (char*)strRet.c_str(), _MAX_PATH) == NULL)
	{
		UCLIDException ue("ELI05792", "Invalid path.");
		ue.addDebugInfo("Input path", strRet);
		throw ue;
	}

	// create the full path to the relative file
	strRet = string(pszFullFileName);

	// validate the absolute path, if requested by the caller
	if (bValidate)
	{
		validateFileOrFolderExistence(strRet);
	}

	// return the 
	return strRet;
}
//--------------------------------------------------------------------------------------------------
void simplifyPathName(string& strFile)
{
	// If it is a relative path, simply return
	if (!isAbsolutePath(strFile))
	{
		return;
	}

	// Get the position of "\.." or "\."
	int iPos = strFile.find("\\.");

	if (iPos != string::npos)
	{
		// Get the parent path and add an unexist file name "unused_file_name.txt" 
		// to call getAbsoluteFileName()
		string strParent = strFile.substr(0, iPos);
		strParent = strParent + "\\unused_file_name.txt";

		// Get the relative file name
		string strRelative = strFile.substr(iPos + 1, strFile.size() - 1);

		// Using parent file name and relative name to get the absolute file name
		strFile = getAbsoluteFileName(strParent, strRelative);
	}
}
//--------------------------------------------------------------------------------------------------
string getRelativeFileName(const string& strParentFile, const string& strAbsoluteFile)
{
	long nParentFileNameLen = strParentFile.size();
	long nAbsoluteFileNameLen = strAbsoluteFile.size();
	long nShortestStrSize = 
		(nParentFileNameLen < nAbsoluteFileNameLen) ? nParentFileNameLen : nAbsoluteFileNameLen;
	// compare these two file names and get the absolute file name with relative path
	long index = 0;
	for (; index < nShortestStrSize; index++)
	{
		// stop at where there's a discrepency
		if (toupper(strParentFile[index]) != toupper(strAbsoluteFile[index]))
		{
			break;
		}
	}

	// these two files do not have any similarity, then return the original absolute file name
	if (index == 0)
	{
		return strAbsoluteFile;
	}

	string strTempImageFileName(strAbsoluteFile);
	// get the part of the directory where both gdd and image file share the same path
	// find the closest "\" backwards from position index
	unsigned int uiCommonFolderPos = strParentFile.rfind("\\", index);
	if (uiCommonFolderPos == string::npos)
	{
		UCLIDException uclidException("ELI03323", "Invalid parent and absolute file name.");
		uclidException.addDebugInfo("Parent File", strParentFile);
		uclidException.addDebugInfo("Absolute File", strAbsoluteFile);
		throw uclidException;
	}

	// get the substr from image file name starts from nCommonFolderPos+1
	strTempImageFileName = strAbsoluteFile.substr(uiCommonFolderPos+1);
	// get the substr starts at nCommonFolderPos+1
	string strFolders = strParentFile.substr(uiCommonFolderPos+1);
	unsigned int uiSlashPos = strFolders.find_first_of("\\");
	// how many "..\" shall we put in front of the image file path
	while (uiSlashPos != string::npos)
	{
		strTempImageFileName = "..\\" + strTempImageFileName;
		uiSlashPos = strFolders.find_first_of("\\", uiSlashPos + 1);
	}

	return strTempImageFileName;
}
//--------------------------------------------------------------------------------------------------
string buildAbsolutePath(const string& strFileOrDirPath)
{
	// check to see if this is an absolute path or relative path
	if (!isAbsolutePath(strFileOrDirPath))
	{
		// relative path, build absolute path relative to current directory
		string strNewPath = getCurrentDirectory();
		strNewPath += "\\" + strFileOrDirPath;
		simplifyPathName(strNewPath);

		return strNewPath;
	}
	else
	{
		return strFileOrDirPath;
	}
}
//--------------------------------------------------------------------------------------------------
string getTextFileContentsAsString(const string& strTextFileName)
{
	string strRet;

	int length;
	char *buffer;
	ifstream ifs;
	// if we dont read as binary the newlines will get screwed up
	ifs.open (strTextFileName.c_str(), ifstream::in | ifstream::binary);
	if (!ifs) 
	{
		UCLIDException ue("ELI03544", "Unable to open specified file.");
		ue.addDebugInfo("FileName", strTextFileName);
		throw ue;
	}

	// get length of file:
	ifs.seekg (0, ios::end);
	length = ifs.tellg();
	ifs.seekg (0, ios::beg);

	// allocate memory:
	buffer = new char[length + 1];
	try
	{

		// read data as a block:
		ifs.read (buffer, length);
		// close the file
		ifs.close();

		// append a NULL
		buffer[length] = '\0';

		// return the string
		strRet = buffer;
	}
	catch (...)
	{
		delete [] buffer;
		buffer = NULL;
		throw;
	}
	if ( buffer != NULL )
	{
		delete [] buffer;
	}
	return strRet;
}
//--------------------------------------------------------------------------------------------------
string getModuleDirectory(HMODULE hModule)
{
	char pszModuleFile[MAX_PATH];
	int ret = ::GetModuleFileName(hModule, pszModuleFile, MAX_PATH);
	if (ret == 0)
	{
		UCLIDException ue("ELI03898", "Unable to retrieve module file name!");
		throw ue;
	}

	return getDirectoryFromFullPath(pszModuleFile);
}
//--------------------------------------------------------------------------------------------------
string getModuleDirectory(const string& strModuleShortFileName)
{
	HMODULE hModule = ::GetModuleHandle(strModuleShortFileName.c_str());
	if (hModule == NULL)
	{
		UCLIDException ue("ELI03897", "Unable to retrieve module handle!");
		ue.addDebugInfo("Module", strModuleShortFileName);
		throw ue;
	}
	
	return ::getModuleDirectory(hModule);
}
//--------------------------------------------------------------------------------------------------
string getCurrentProcessEXEFullPath()
{
	char pszModuleFile[MAX_PATH];
	int ret = ::GetModuleFileName(NULL, pszModuleFile, MAX_PATH);
	if (ret == 0)
	{
		UCLIDException ue("ELI03899", "Unable to retrieve module file name!");
		throw ue;
	}

	return string(pszModuleFile);
}
//--------------------------------------------------------------------------------------------------
string getCurrentProcessEXEDirectory()
{
	return getModuleDirectory((HMODULE) NULL);
}
//--------------------------------------------------------------------------------------------------
string getFileVersion(const string& strFileFullName)
{
	string strVersion("");

	DWORD	dwHandle;
	UINT	uiDataSize;
	LPVOID	lpData;
	DWORD	dwSize;
	LPVOID	lpBuffer;
	LPTSTR	lpszImageName;
	char	Data[80] = {0}; // initialize array to empty

	lpszImageName = const_cast<char*>(strFileFullName.c_str());

	dwHandle = 0; 
	lpData = (void *)(&Data);

	// Get the version information block size,
	// then use it to allocate a storage buffer.
	dwSize = ::GetFileVersionInfoSize( lpszImageName, &dwHandle );
	// check for error
	if (dwSize == 0)
	{
		// error getting the size of the version info struct
		UCLIDException ue("ELI17359", "Failed getting version info structure size!");
		ue.addWin32ErrorInfo();
		ue.log();

		// return empty string
		return string("");
	}

	lpBuffer = malloc(dwSize);

	// Get the version information block
	if(::GetFileVersionInfo( lpszImageName, 0, dwSize, lpBuffer ) == 0)
	{
		// error on getting the FileVersionInfo
		UCLIDException ue("ELI17360", "Failed getting file version info!");
		ue.addWin32ErrorInfo();
		ue.log();

		// free the lpBuffer and return empty string
		free(lpBuffer);
		return string("");
	}

	// Use the Engligh and language neutral version information blocks to obtain the product name.
	if (VerQueryValue( lpBuffer, TEXT("\\StringFileInfo\\040904B0\\FileVersion"), 
		&lpData, &uiDataSize ) == 0 &&
		VerQueryValue( lpBuffer, TEXT("\\StringFileInfo\\000004B0\\FileVersion"), 
		&lpData, &uiDataSize ) == 0)
	{
		// error on getting the FileVersion data
		UCLIDException ue("ELI17361", "Failed getting FileVersion information!");
		ue.addWin32ErrorInfo();
		ue.log();

		// free the lpBuffer and return empty string
		free(lpBuffer);
		return string("");
	}

	// Replace commas with periods in version number substring
	strVersion = (char *) lpData;
	replaceVariable(strVersion, ", ", ".");

	// Free the data buffer
	free(lpBuffer);

	return strVersion;
}
//--------------------------------------------------------------------------------------------------
void getFilesInDir(vector<string>& rvecFiles,
				   const string& strDirectory, 
				   const string& strFileExtension,
				   bool bRecursive)
{
	string strDir(strDirectory);
	if (strDir.rfind("\\") < strDir.size()-1)
	{
		strDir += "\\";
	}

	// add a slash to the end of the directory if there isn't one
	string strDirAndFileSpec = strDir;
	strDirAndFileSpec += strFileExtension;
		
	// find all files with the specified file extension in the specified directory
	FileIterator iter(strDirAndFileSpec);
	while (iter.moveNext())
	{
		if (!iter.isDirectory())
		{
			// we found a file that meets the specs.
			string strFileMeetingSpec = strDir;
			strFileMeetingSpec += iter.getFileName();
			rvecFiles.push_back(strFileMeetingSpec);
		}
	}

	iter.reset();

	// if recursion was desired by the caller, and if there exist directories under
	// the current directory, then recurse through all the sub-directories and 
	// search for files with the same file specification
	if (bRecursive)
	{
		vector<string> vecDirectories;

		// find all the sub directories
		strDirAndFileSpec = strDir + string("*.*");
		iter.reset(strDirAndFileSpec);
		while (iter.moveNext())
		{
			if (iter.isDirectory())
			{
				string strFileName = iter.getFileName();
				if (strFileName != "." && strFileName != "..")
				{
					// we found a subdirectory
					string strFileMeetingSpec = strDir;
					// add the file name to the directory
					strFileMeetingSpec += strFileName;
					vecDirectories.push_back(strFileMeetingSpec);
				}
			}
		}
		iter.reset();

		for (unsigned int n=0; n<vecDirectories.size(); n++)
		{
			// get the sub directory
			string strRecurseDir = vecDirectories[n];
			// call this function recursively to get all required files out
			getFilesInDir(rvecFiles, strRecurseDir, strFileExtension, bRecursive);
		}
	}
}
//--------------------------------------------------------------------------------------------------
string getUNCPath(const string& strLocalPath)
{
	// if strLocalPath is already a valid UNC path, simply return it
	if (PathIsUNC(strLocalPath.c_str()))
	{
		return strLocalPath;
	}

	// Before attempting to get the unc path
	// ensure that the drive is a network drive
	string drive = getDriveFromFullPath(strLocalPath, false);
	UINT dt = GetDriveType(drive.c_str());
	// if it is not a network drive return the current path
	if(dt != DRIVE_REMOTE)
	{
		return strLocalPath;
	}

	string strUNCPath(strLocalPath);

    TCHAR tszTemp[_MAX_PATH + MAX_COMPUTERNAME_LENGTH + 4];
    UNIVERSAL_NAME_INFO *uncName = (UNIVERSAL_NAME_INFO *) tszTemp;
    DWORD dwSize = _MAX_PATH + MAX_COMPUTERNAME_LENGTH + 4;

    DWORD dwRet = WNetGetUniversalName(strLocalPath.c_str(),
		UNIVERSAL_NAME_INFO_LEVEL, uncName, &dwSize);

	if (dwRet == NO_ERROR)
	{
		LPTSTR zUNCPath = uncName->lpUniversalName;
		strUNCPath = zUNCPath;
	}
	else if (dwRet != ERROR_NOT_CONNECTED) 
	{
		// throw exception if the error is other than ERROR_NOT_CONNECTED
		string strTemp;
		switch (dwRet)
		{
		case ERROR_BAD_DEVICE:
			strTemp = "ERROR_BAD_DEVICE";
			break;
		case ERROR_CONNECTION_UNAVAIL:
			strTemp = "ERROR_CONNECTION_UNAVAIL";
			break;
		case ERROR_EXTENDED_ERROR:
			strTemp = "ERROR_EXTENDED_ERROR";
			break;
		case ERROR_MORE_DATA:
			strTemp = "ERROR_MORE_DATA";
			break;
		case ERROR_NOT_SUPPORTED:
			strTemp = "ERROR_NOT_SUPPORTED";
			break;
		case ERROR_NO_NET_OR_BAD_PATH:
			strTemp = "ERROR_NO_NET_OR_BAD_PATH";
			break;
		case ERROR_NO_NETWORK:
			strTemp = "ERROR_NO_NETWORK";
			break;
		case ERROR_NOT_CONNECTED:
			strTemp = "ERROR_NOT_CONNECTED";
			break;
		};

		UCLIDException uclidEx("ELI06813", "Can't get universal path name.");
		uclidEx.addDebugInfo("Error", strTemp);
		uclidEx.addDebugInfo("File", strLocalPath);
		throw uclidEx;
	}

	return strUNCPath;
}
//--------------------------------------------------------------------------------------------------
vector<string> getSubFolderShortNames(const string& strParentFolder)
{
	vector<string> vecSubFolderNames;

	string strDir(strParentFolder);
	if (strDir.rfind("\\") < strDir.size()-1)
	{
		// add a slash to the end if there isn't a one
		strDir += "\\";
	}
		
	strDir += "*.*";

	// find all files with the specified file extension in the specified directory
	FileIterator iter(strDir);
	while (iter.moveNext())
	{
		string strFolderName = iter.getFileName();
		// if the file name is actually a folder, and is none of "." or ".."
		if (iter.isDirectory() && strFolderName != "." && strFolderName != "..")
		{
			// we found a file that meets the specs.
			vecSubFolderNames.push_back(strFolderName);
		}
	}

	return vecSubFolderNames;
}
//--------------------------------------------------------------------------------------------------
void validateFileOrFolderExistence(const string& strName, const string& strELICode)
{
	if (_access_s(strName.c_str(), giMODE_FILE_EXISTS) != 0)
	{
		// if the test file doesn't exist
		UCLIDException uclidException(strELICode.empty() ? "ELI06002" : strELICode,
			"Specified file or folder can't be found.");
		uclidException.addDebugInfo("File/Folder name", strName);
		uclidException.addWin32ErrorInfo();
		throw uclidException;
	}
}
//--------------------------------------------------------------------------------------------------
bool isFileOrFolderValid(const string& strName)
{
	bool bReturn = true;

	if (_access_s( strName.c_str(), giMODE_FILE_EXISTS ) != 0)
	{
		// Clear flag
		bReturn = false;
	}

	return bReturn;
}
//--------------------------------------------------------------------------------------------------
bool fileExistsAndIsReadable(const string& strName)
{
	// Use the C runtime function _access to determine if the file exists and is readable 
	return (_access_s(strName.c_str(), giMODE_READ_ONLY) == 0);
}
//--------------------------------------------------------------------------------------------------
bool fileExistsAndIsReadOnly(const string& strFileName)
{
	DWORD dwAttrib = GetFileAttributes(strFileName.c_str());

	return ( (dwAttrib != INVALID_FILE_ATTRIBUTES)
			&& (dwAttrib & FILE_ATTRIBUTE_READONLY)
			&& !(dwAttrib & FILE_ATTRIBUTE_DIRECTORY) );
}
//--------------------------------------------------------------------------------------------------
bool isFileReadOnly(const string& strName )
{
	bool bReturn = false;
	if ( _access_s( strName.c_str(), giMODE_WRITE_ONLY ) != 0 )
	{
		bReturn = true;
	}
	return bReturn;
}
//--------------------------------------------------------------------------------------------------
void verifyFileIsWritable(const string& strName)
{
	// ensure the file exists
	validateFileOrFolderExistence(strName);

	// check if file is read only
	if (isFileReadOnly(strName))
	{
		UCLIDException ue("ELI20439", "Specified file is not writable!");
		ue.addDebugInfo("Read-only file name", strName);
		ue.addWin32ErrorInfo();
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
int getLongPathName(const string& strShortPath, string& strLongPath)
{
	// TODO: convert a short UNC path to a long UNC path

	// if the path is define as a UNC path
	// return the short path
	if (::PathIsUNC(strShortPath.c_str()))
	{
		strLongPath = strShortPath;
		return strLongPath.size();
	}

	int nSize = strShortPath.size();
	unsigned int uiFound = strShortPath.rfind("\\");
	if (uiFound != string::npos)
	{
		// recurse to peel off components
		if (getLongPathName(strShortPath.substr(0, uiFound), strLongPath) > 0)
		{
			strLongPath += "\\";
			
			if (strShortPath[nSize-1] != '\\')
			{
				FileIterator iter(strShortPath);
				if (iter.moveNext())
				{
					// Append the long component name to the path
					strLongPath += iter.getFileName();
				}
				else
				{
					strLongPath = "";
				}
			}
		}
	}
	else
	{
		strLongPath = strShortPath;
	}
	
	return strLongPath.size();
}
//--------------------------------------------------------------------------------------------------
bool isAbsolutePath(const string& strFileOrFolderName)
{
	return (strFileOrFolderName.find(":") == 1 ||
		strFileOrFolderName.find("\\\\") == 0);
}
//--------------------------------------------------------------------------------------------------
string getCurrentDirectory()
{
	string strCurrentDir = "";

	char pszCurrentDir[512] = {0};
	unsigned long ulBufferSize = sizeof(pszCurrentDir);

	// Get the current directory
	if ( GetCurrentDirectory(ulBufferSize, pszCurrentDir ) )
	{
		strCurrentDir = pszCurrentDir;
	}
	return strCurrentDir;
}
//--------------------------------------------------------------------------------------------------
bool isPDFFile(const string& strName)
{
	// Test the file extension
	string strExt = getExtensionFromFullPath( strName, true );
	if (strExt == ".pdf")
	{
		return true;
	}
	else
	{
		return false;
	}
}
//--------------------------------------------------------------------------------------------------
bool isImageFileExtension(string strExt)
{
	// erase the leading period if any
	if (strExt[0] == '.')
	{
		strExt.erase(0, 1);
	}

	// make the extension into lowercase
	makeLowerCase(strExt);

	// initialize the vector of valid image file extensions 
	vector<string> vecExtensions;

	// Check extensions
	// This was changed from a static vector due to P13 4434  
	if (strExt == "tif" || strExt == "tiff" ||			// TIF extensions
		strExt == "gif" ||								// GIF extensions
		strExt == "jpg" || strExt == "jpeg" ||			// JPEG extensions
		strExt == "bmp" || strExt == "rle"  ||			// bitmap extensions
		strExt == "dib" ||
		strExt == "rst" || strExt == "gp4"  ||			// cals1 extensions
		strExt == "mil" || strExt == "cal"  ||
		strExt == "cg4" ||
		strExt == "flc"	|| strExt == "fli"  ||			// FLIC extensions
		strExt == "tga"	|| strExt == "pct"  ||			// other extensions
		strExt == "pcx" || strExt == "png"  ||
		strExt == "pdf" || strExt == "bin" )
	{
		// Extension is an image extension
		return true;
	}

	// Extension is not an image extension
	return false;
}
//--------------------------------------------------------------------------------------------------
bool isThreeDigitExtension(const string& strExt)
{
	return (strExt.length() == 4 && strExt[0] == '.' &&
		isDigitChar(strExt[1]) && isDigitChar(strExt[2]) &&
		isDigitChar(strExt[3]));
}
//--------------------------------------------------------------------------------------------------
EFileType getFileType(const string& strFileName)
{
	// get the extension from the filename and make it uppercase for easy comparision
	string strExt = getExtensionFromFullPath(strFileName, true);
	
	// return the correct enum constant depending upon strExt
	if (strExt == ".txt")
	{
		return kTXTFile;
	}
	else if (strExt == ".uss")
	{
		return kUSSFile;
	}
	else if (strExt == ".voa")
	{
		return kVOAFile;
	}
	else if (strExt == ".eav")
	{
		return kEAVFile;
	}
	else if (strExt == ".xml")
	{
		return kXMLFile;
	}
	else if (strExt == ".doc" || strExt == ".xls"
			|| strExt == ".csv" || strExt == ".ppt"
			|| strExt == ".mdb" || strExt == ".zip"
			|| strExt == ".cab" || strExt == ".rar"
			|| strExt == ".exe" || strExt == ".bat"
			|| strExt == ".com" || strExt == ".mp3"
			|| strExt == ".wav" || strExt == ".ra"
			|| strExt == ".ram" || strExt == ".mpg")
	{
		return kNonImageFile;
	}

	// Check utility list of image file extensions
	// also accepting three digits as indicating an image file
	else if (isImageFileExtension( strExt ) || 
		isThreeDigitExtension( strExt ))
	{
		return kImageFile;
	}
	else
	{
		return kUnknown;
	}
}
//--------------------------------------------------------------------------------------------------
ULONGLONG getSizeOfFile(const string& strFileName)
{
	try
	{
		try
		{
			// Ensure the file exists
			validateFileOrFolderExistence(strFileName);

			int iRetryCount(0), iRetryTimeout(0);
			getFileAccessRetryCountAndTimeout(iRetryCount, iRetryTimeout);
			bool bOpened = false;
			ULONGLONG ullFileLength = 0;
			int iRetries = 0;
			do
			{
				CFileException cex;
				CFile file;
				bOpened = asCppBool(file.Open(strFileName.c_str(),
					CFile::modeRead | CFile::shareDenyNone, &cex));
				if (bOpened)
				{
					// Get file length and close the file
					ullFileLength = file.GetLength();
					file.Close();
				}
				// Unable to open the file, check if this is a sharing violation
				// OR if we have exceeded the number of retries
				else
				{
					if (cex.m_cause != CFileException::sharingViolation)
					{
						// Not a sharing violation, build a UCLIDException and throw it
						char pszError[1024] = {0};
						cex.GetErrorMessage(pszError, 1024);

						UCLIDException ue("ELI25507", pszError);
						ue.addDebugInfo("OS Error Code", cex.m_lOsError);
						ue.addDebugInfo("Cause Code", cex.m_cause);
						throw ue;
					}
					// Sharing violation, check retry count
					else if (iRetries > iRetryCount)
					{
						UCLIDException ue("ELI25505", "File cannot be opened to read file size!");
						ue.addDebugInfo("Number of retries", iRetries);
						ue.addDebugInfo("Max number of retries", iRetryCount);
						throw ue;
					}

					// increment the number of retires and wait for the timeout period
					iRetries++;
					Sleep(iRetryTimeout);
				}
			}
			while (!bOpened);

			// Return the file length
			return ullFileLength;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24988");
	}
	catch(UCLIDException& uex)
	{
		UCLIDException ue("ELI09041", "Error Reading File Size.", uex);
		ue.addDebugInfo("Filename", strFileName);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
ULONGLONG getSizeOfFiles(const vector<string>& vecFileNames)
{
	unsigned long i = 0;
	try
	{
		try
		{
			ULONGLONG nSize = 0;
			for (; i < vecFileNames.size(); i++)
			{
				nSize += getSizeOfFile(vecFileNames[i]);
			}
			return nSize;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25517");
	}
	catch(UCLIDException& uex)
	{
		uex.addDebugInfo("Vector Index", i);
		throw uex;
	}
}
//--------------------------------------------------------------------------------------------------
bool doesFileMatchPatterns(const vector<string>& strMatchPatterns, const string& strFileName, 
						   bool bCaseSensitive)
{
	for (unsigned int i = 0; i < strMatchPatterns.size(); i++)
	{
		if(doesFileMatchPattern(strMatchPatterns[i], strFileName, bCaseSensitive))
		{
			return true;
		}
	}
	return false;
}
//--------------------------------------------------------------------------------------------------
bool doesFileMatchPattern(const string& strMatchPattern, const string& strFileName, 
						  bool bCaseSensitive)
{
	string strTmpMatchPattern = strMatchPattern;
	string strTmpFileName = strFileName;
	const char* wild;
	const char* text;
	if(!bCaseSensitive)
	{
		makeLowerCase(strTmpMatchPattern);
		makeLowerCase(strTmpFileName);
	}

	wild = strTmpMatchPattern.c_str();
	text = strTmpFileName.c_str();
	
	const char *cp, *mp;
	
	while ((*text) && (*wild != '*')) 
	{
		if ((*wild != *text) && (*wild != '?')) 
		{
			return false;
		}
		wild++;
		text++;
	}
	
	while (*text) 
	{
		if (*wild == '*') 
		{
			if (!*++wild) 
			{
				return true;
			}
			mp = wild;
			cp = text+1;
		} 
		else if ((*wild == *text) || (*wild == '?')) 
		{
			wild++;
			text++;
		} 
		else 
		{
			wild = mp;
			text = cp++;
		}
	}
	
	while (*wild == '*') 
	{
		wild++;
	}
	return !*wild;
}
//--------------------------------------------------------------------------------------------------
void getTempDir(string& strPath)
{
	// Get the path of the defin
	char path[1024] = {0};
	DWORD dwRet = GetTempPath(1024, path);
	if (dwRet == 0)
	{
		UCLIDException ue("ELI09187", "Unable to get path to Temporary Directory.");
		ue.addWin32ErrorInfo();
		throw ue;
	}
	strPath = path;
}
//--------------------------------------------------------------------------------------------------
bool canCreateFile(string strFQFileOrFolderName)
{
	bool bSuccess = true;

	try
	{
		// catch and rethrow to ensure there are no memory leaks from exceptions
		try
		{
			// modified as per [p13 #4952] - JDS 04/09/2008
			// if strFQFileOrFolderName is a not a valid folder, get
			// folder name from the full path
			if (!isValidFolder(strFQFileOrFolderName))
			{
				strFQFileOrFolderName = getDirectoryFromFullPath(strFQFileOrFolderName);
			}

			// just create a temporary file if an exception is thrown than the user
			// cannot create a file in the specified location.  by passing true
			// as the last argument the temporary file will be deleted when 
			// TemporaryFileName goes out of scope
			TemporaryFileName(strFQFileOrFolderName, NULL, NULL, true);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20698");
	}
	catch(...)
	{
		bSuccess = false;
	}

	return bSuccess;
}
//--------------------------------------------------------------------------------------------------
bool isValidFile(const string& strFile)
{
	DWORD dwAttrib = GetFileAttributes(strFile.c_str());
	if (dwAttrib == INVALID_FILE_ATTRIBUTES || dwAttrib & FILE_ATTRIBUTE_DIRECTORY)
	{
		return false;
	}
	else
	{
		return true;
	}
}
//--------------------------------------------------------------------------------------------------
bool isValidFolder(const string& strFolder)
{
	DWORD dwAttrib = GetFileAttributes(strFolder.c_str());
	if (dwAttrib != INVALID_FILE_ATTRIBUTES && dwAttrib & FILE_ATTRIBUTE_DIRECTORY)
	{
		return true;
	}
	else
	{
		return false;
	}
}
//--------------------------------------------------------------------------------------------------
void copyFile(const string &strSrcFileName, const string &strDstFileName, 
			  bool bUpdateFileSettings, bool bAllowReadonly)
{
	try
	{
		if (bAllowReadonly)
		{
			if (fileExistsAndIsReadOnly(strDstFileName))
			{
				setFileAttributes(strDstFileName, FILE_ATTRIBUTE_NORMAL);
			}
		}

		const char* pszOutputFile = strDstFileName.c_str();
		const char* pszSourceFile = strSrcFileName.c_str();

		// copy the file  (retry if the copy fails due to a share violation)
		int iRetryCount(-1),iTimeout(-1);
		getFileAccessRetryCountAndTimeout(iRetryCount, iTimeout);
		int iRetries = 0;
		while(!asCppBool(CopyFile(pszSourceFile, pszOutputFile, FALSE)))
		{
			// Failed to copy, get the last error
			DWORD dwError = GetLastError();

			// If the error was not a sharing violation then just throw an exception
			if (dwError != ERROR_SHARING_VIOLATION)
			{
				UCLIDException ue("ELI16606", "Unable to copy file!");
				ue.addDebugInfo("Source File", strSrcFileName);
				ue.addDebugInfo("Destination File", strDstFileName);
				ue.addWin32ErrorInfo(dwError);
				throw ue;
			}
			// Sharing violation, check if retry count has been exceeded
			else if (iRetries > iRetryCount)
			{
				// Have attempted to copy the file the
				// required number of times so throw exception
				UCLIDException ue("ELI24965", "File cannot be copied!");
				ue.addDebugInfo("Source File Name", strSrcFileName);
				ue.addDebugInfo("Destination File Name", strDstFileName);
				ue.addDebugInfo("Number of retries", iRetries);
				ue.addWin32ErrorInfo(dwError);
				throw ue;
			}

			// increment the number of retries and wait for the timeout period
			iRetries++;
			Sleep(iTimeout);
		}
		// Call wait for file with bLogException == false [LRCAU #5337]
		waitForFileToBeReadable(strDstFileName, false);

		// check if the file settings should be updated
		if(bUpdateFileSettings)
		{
			// get current file attributes
			DWORD	dwAttributes = GetFileAttributes(pszOutputFile);

			// remove read-only attribute from copied file
			dwAttributes &= ~FILE_ATTRIBUTE_READONLY;
			SetFileAttributes(pszOutputFile, dwAttributes);

			// set the file access and modification times to current system time
			time_t	tmNow;
			time(&tmNow);

			struct _utimbuf	tmSettings;
			tmSettings.actime = tmNow;
			tmSettings.modtime = tmNow;

			long lResult = _utime(pszOutputFile, &tmSettings);
			if (lResult != 0)
			{
				UCLIDException ue("ELI11361", "Unable to update file settings.");
				ue.addDebugInfo("File Name", strDstFileName);
				throw ue;
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25513");
}
//--------------------------------------------------------------------------------------------------
void moveFile(const string strSrcFileName, const string strDstFileName,
			  const bool bOverwrite, const bool bAllowReadonly)
{
	INIT_EXCEPTION_AND_TRACING("MLI01882");
	try
	{
		// If readonly allowed need to make both source and destination writeable
		DWORD dwOldAttributes = INVALID_FILE_ATTRIBUTES;
		bool bResetAttributes = false;
		_lastCodePos = "10";
		if (bAllowReadonly)
		{
			_lastCodePos = "10_A";
			if (fileExistsAndIsReadOnly(strSrcFileName))
			{
				_lastCodePos = "10_A_1";
				dwOldAttributes = GetFileAttributes(strSrcFileName.c_str());
				if (dwOldAttributes == INVALID_FILE_ATTRIBUTES)
				{
					UCLIDException uex("ELI23608", "Unable to get file attributes.");
					uex.addWin32ErrorInfo();
					uex.addDebugInfo("File To Get Attributes", strSrcFileName);
					throw uex;
				}

				_lastCodePos = "10_A_2";
				setFileAttributes(strSrcFileName, FILE_ATTRIBUTE_NORMAL);
			}

			_lastCodePos = "10_B";
			if (fileExistsAndIsReadOnly(strDstFileName))
			{
				_lastCodePos = "10_B_1";
				setFileAttributes(strDstFileName, FILE_ATTRIBUTE_NORMAL);
			}
		}
		_lastCodePos = "20";

		// Guarantee that a move performed as a copy and delete operation is flushed 
		// to disk before the function returns
		DWORD dwFlags = MOVEFILE_COPY_ALLOWED | MOVEFILE_WRITE_THROUGH;
		if (bOverwrite)
		{
			dwFlags |= MOVEFILE_REPLACE_EXISTING;
		}

		// Move the file (retry if it fails due to a share violation) [LegacyRCAndUtils #5139]
		_lastCodePos = "30";
		int iRetryCount(-1),iTimeout(-1);
		getFileAccessRetryCountAndTimeout(iRetryCount, iTimeout);
		int iRetries = 0;
		while(!asCppBool(MoveFileEx(strSrcFileName.c_str(), strDstFileName.c_str(), dwFlags)))
		{
			// Failed to move, get the last error
			DWORD dwError = GetLastError();

			// If the error was not a sharing violation then just throw an exception
			if (dwError != ERROR_SHARING_VIOLATION)
			{
				UCLIDException ue("ELI12135", "Unable to move file!");
				ue.addDebugInfo("SrcFile", strSrcFileName);
				ue.addDebugInfo("DstFile", strDstFileName);
				ue.addWin32ErrorInfo(dwError);
				throw ue;
			}
			// Sharing violation, check if retry count has been exceeded
			else if (iRetries > iRetryCount)
			{
				// Have attempted to move the file the
				// required number of times so throw exception
				UCLIDException ue("ELI24964", "File cannot be moved!");
				ue.addDebugInfo("Source File Name", strSrcFileName);
				ue.addDebugInfo("Destination File Name", strDstFileName);
				ue.addDebugInfo("Number of retries", iRetries);
				ue.addWin32ErrorInfo(dwError);
				throw ue;
			}

			// increment the number of retries and wait for the timeout period
			iRetries++;
			Sleep(iTimeout);
		}

		// Call wait for file with bLogException == false [LRCAU #5337]
		waitForFileToBeReadable(strDstFileName, false);
		_lastCodePos = "40";

		// If the attributes were changed then restore them
		if (bResetAttributes)
		{
			setFileAttributes(strDstFileName, dwOldAttributes, true);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23610");
}
//--------------------------------------------------------------------------------------------------
void deleteFile(const string strFileName, const bool bAllowReadonly)
{
	try
	{
		if (bAllowReadonly && fileExistsAndIsReadOnly(strFileName))
		{
			setFileAttributes(strFileName, FILE_ATTRIBUTE_NORMAL);
		}

		// Delete the file  (retry if the delete fails due to a share violation)
		int iRetryCount(-1),iTimeout(-1);
		getFileAccessRetryCountAndTimeout(iRetryCount, iTimeout);
		int iRetries = 0;
		const char* pszFileName = strFileName.c_str();
		while(!asCppBool(DeleteFile(pszFileName)))
		{
			// Failed to delete, get the last error
			DWORD dwError = GetLastError();

			// If the error was not a sharing violation then just throw an exception
			if (dwError != ERROR_SHARING_VIOLATION)
			{
				UCLIDException ue("ELI16605", "Unable to delete file!");
				ue.addDebugInfo("File To Delete", strFileName);
				ue.addWin32ErrorInfo(dwError);
				throw ue;
			}
			// Sharing violation, check if retry count has been exceeded
			else if (iRetries > iRetryCount)
			{
				// Have attempted to delete the file the
				// required number of times so throw exception
				UCLIDException ue("ELI24978", "File cannot be deleted!");
				ue.addDebugInfo("File To Delete", strFileName);
				ue.addDebugInfo("Number of retries", iRetries);
				ue.addWin32ErrorInfo(dwError);
				throw ue;
			}

			// increment the number of retries and wait for the timeout period
			iRetries++;
			Sleep(iTimeout);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23611");
}
//--------------------------------------------------------------------------------------------------
void convertFileToListOfStrings(ifstream &file, list<string> &lstFileContents)
{
	while (!file.eof())
	{
		char buffer[1024];

		file.getline(buffer, 1024);
				
		lstFileContents.push_back(buffer);
	}
}
//--------------------------------------------------------------------------------------------------
void waitForFileAccess(const string& strFileName, int iAccess)
{
	// Static variables
	int iTimeout = -1;
	int iRetryCount = -1;
	getFileAccessRetryCountAndTimeout(iRetryCount, iTimeout);

	// See if the file has the requested access
	int iRetries = 0;
	int iRtnCode;
	bool bRtnValue;
	do
	{
		// Check access rights
		iRtnCode = _access_s(strFileName.c_str(), iAccess);
		bRtnValue = iRtnCode == 0;

		// if file does not have the requested rights check retry count 
		if ( !bRtnValue )
		{
			// If retry count has been exceeded log exception
			if ( iRetries > iRetryCount)
			{
				// Have checked the access rights the required number of times so log exception
				UCLIDException ue("ELI20618", "File cannot be accessed with requested access!");
				ue.addDebugInfo("File Name", strFileName);
				ue.addDebugInfo("Access", iAccess);
				ue.addDebugInfo("Number of retries", iRetries);
				ue.addDebugInfo("Error code", iRtnCode);
				ue.addWin32ErrorInfo();

				// Just log the exception since this function is to give OS time 
				// to release the file.
				ue.log();
				break;
			}

			// increment the number of retires and wait for the timeout period
			iRetries++;
			Sleep(iTimeout);
		}
	}
	while (!bRtnValue);
}
//--------------------------------------------------------------------------------------------------
void waitForFileToBeReadable(const string& strFileName, bool bLogException, ifstream** ppinFile)
{
	// Static variables
	int iTimeout = -1;
	int iRetryCount = -1;
	getFileAccessRetryCountAndTimeout(iRetryCount, iTimeout);

	// See if the file has the requested access
	int iRetries = 0;
	bool bOpen = false; // Default bOpen to false
	do
	{
		// Scope for the auto_ptr
		{
			auto_ptr<ifstream> apIn;
			apIn.reset(new ifstream(strFileName.c_str(), ios::in));

			// Check if the file was successfully opened
			if (!apIn->fail())
			{
				// Set bOpen to true
				bOpen = true;

				// Check if looking for an ifstream pointer
				if (ppinFile != NULL)
				{
					// Store the ifstream pointer
					*ppinFile = apIn.release();

					// The file has been opened so just return
					return;
				}
				else
				{
					// Close the file
					apIn->close();
				}
			}
		}

		// if file does not have the requested rights check retry count 
		if (!bOpen)
		{
			// If retry count has been exceeded log exception
			if (iRetries > iRetryCount)
			{
				if (bLogException)
				{
					// Have checked the access rights the required number of times so log exception
					UCLIDException ue("ELI24024",
						"Application Trace: File cannot be opened for reading.");
					ue.addDebugInfo("File Name", strFileName);
					ue.addDebugInfo("Number of retries", iRetries);

					// Just log the exception since this function is to give OS time 
					// to release the file.
					ue.log();
				}
				break;
			}

			// increment the number of retires and wait for the timeout period
			iRetries++;
			Sleep(iTimeout);
		}
	}
	while (!bOpen);
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: Local method used to add debug information in the getSpecialFolder function.
string getCSIDLAsString(int CLSID)
{
	string strReturn = "";

	switch (CLSID)
	{
	case CSIDL_DESKTOP:
		strReturn = "CSIDL_DESKTOP";
		break;
	case CSIDL_INTERNET:
		strReturn = "CSIDL_INTERNET";
		break;
	case CSIDL_PROGRAMS:
		strReturn = "CSIDL_PROGRAMS";
		break;
	case CSIDL_CONTROLS:
		strReturn = "CSIDL_CONTROLS";
		break;
	case CSIDL_PRINTERS:
		strReturn = "CSIDL_PRINTERS";
		break;
	case CSIDL_PERSONAL:
		strReturn = "CSIDL_PERSONAL";
		break;
	case CSIDL_FAVORITES:
		strReturn = "CSIDL_FAVORITES";
		break;
	case CSIDL_STARTUP:
		strReturn = "CSIDL_STARTUP";
		break;
	case CSIDL_RECENT:
		strReturn = "CSIDL_RECENT";
		break;
	case CSIDL_SENDTO:
		strReturn = "CSIDL_SENDTO";
		break;
	case CSIDL_BITBUCKET:
		strReturn = "CSIDL_BITBUCKET";
		break;
	case CSIDL_STARTMENU:
		strReturn = "CSIDL_STARTMENU";
		break;
	case CSIDL_MYDOCUMENTS:
		strReturn = "CSIDL_MYDOCUMENTS";
		break;
	case CSIDL_MYMUSIC:
		strReturn = "CSIDL_MYMUSIC";
		break;
	case CSIDL_MYVIDEO:
		strReturn = "CSIDL_MYVIDEO";
		break;
	case CSIDL_DESKTOPDIRECTORY:
		strReturn = "CSIDL_DESKTOPDIRECTORY";
		break;
	case CSIDL_DRIVES:
		strReturn = "CSIDL_DRIVES";
		break;
	case CSIDL_NETWORK:
		strReturn = "CSIDL_NETWORK";
		break;
	case CSIDL_NETHOOD:
		strReturn = "CSIDL_NETHOOD";
		break;
	case CSIDL_FONTS:
		strReturn = "CSIDL_FONTS";
		break;
	case CSIDL_TEMPLATES:
		strReturn = "CSIDL_TEMPLATES";
		break;
	case CSIDL_COMMON_STARTMENU:
		strReturn = "CSIDL_COMMON_STARTMENU";
		break;
	case CSIDL_COMMON_PROGRAMS:
		strReturn = "CSIDL_COMMON_PROGRAMS";
		break;
	case CSIDL_COMMON_STARTUP:
		strReturn = "CSIDL_COMMON_STARTUP";
		break;
	case CSIDL_COMMON_DESKTOPDIRECTORY:
		strReturn = "CSIDL_COMMON_DESKTOPDIRECTORY";
		break;
	case CSIDL_APPDATA:
		strReturn = "CSIDL_APPDATA";
		break;
	case CSIDL_PRINTHOOD:
		strReturn = "CSIDL_PRINTHOOD";
		break;
	case CSIDL_LOCAL_APPDATA:
		strReturn = "CSIDL_LOCAL_APPDATA";
		break;
	case CSIDL_ALTSTARTUP:
		strReturn = "CSIDL_ALTSTARTUP";
		break;
	case CSIDL_COMMON_ALTSTARTUP:
		strReturn = "CSIDL_COMMON_ALTSTARTUP";
		break;
	case CSIDL_COMMON_FAVORITES:
		strReturn = "CSIDL_COMMON_FAVORITES";
		break;
	case CSIDL_INTERNET_CACHE:
		strReturn = "CSIDL_INTERNET_CACHE";
		break;
	case CSIDL_COOKIES:
		strReturn = "CSIDL_COOKIES";
		break;
	case CSIDL_HISTORY:
		strReturn = "CSIDL_HISTORY";
		break;
	case CSIDL_COMMON_APPDATA:
		strReturn = "CSIDL_COMMON_APPDATA";
		break;
	case CSIDL_WINDOWS:
		strReturn = "CSIDL_WINDOWS";
		break;
	case CSIDL_SYSTEM:
		strReturn = "CSIDL_SYSTEM";
		break;
	case CSIDL_PROGRAM_FILES:
		strReturn = "CSIDL_PROGRAM_FILES";
		break;
	case CSIDL_MYPICTURES:
		strReturn = "CSIDL_MYPICTURES";
		break;
	case CSIDL_PROFILE:
		strReturn = "CSIDL_PROFILE";
		break;
	case CSIDL_SYSTEMX86:
		strReturn = "CSIDL_SYSTEMX86";
		break;
	case CSIDL_PROGRAM_FILESX86:
		strReturn = "CSIDL_PROGRAM_FILESX86";
		break;
	case CSIDL_PROGRAM_FILES_COMMON:
		strReturn = "CSIDL_PROGRAM_FILES_COMMON";
		break;
	case CSIDL_PROGRAM_FILES_COMMONX86:
		strReturn = "CSIDL_PROGRAM_FILES_COMMONX86";
		break;
	case CSIDL_COMMON_TEMPLATES:
		strReturn = "CSIDL_COMMON_TEMPLATES";
		break;
	case CSIDL_COMMON_DOCUMENTS:
		strReturn = "CSIDL_COMMON_DOCUMENTS";
		break;
	case CSIDL_COMMON_ADMINTOOLS:
		strReturn = "CSIDL_COMMON_ADMINTOOLS";
		break;
	case CSIDL_ADMINTOOLS:
		strReturn = "CSIDL_ADMINTOOLS";
		break;
	case CSIDL_CONNECTIONS:
		strReturn = "CSIDL_CONNECTIONS";
		break;
	case CSIDL_COMMON_MUSIC:
		strReturn = "CSIDL_COMMON_MUSIC";
		break;
	case CSIDL_COMMON_PICTURES:
		strReturn = "CSIDL_COMMON_PICTURES";
		break;
	case CSIDL_COMMON_VIDEO:
		strReturn = "CSIDL_COMMON_VIDEO";
		break;
	case CSIDL_RESOURCES:
		strReturn = "CSIDL_RESOURCES";
		break;
	case CSIDL_RESOURCES_LOCALIZED:
		strReturn = "CSIDL_RESOURCES_LOCALIZED";
		break;
	case CSIDL_COMMON_OEM_LINKS:
		strReturn = "CSIDL_COMMON_OEM_LINKS";
		break;
	case CSIDL_CDBURN_AREA:
		strReturn = "CSIDL_CDBURN_AREA";
		break;
	case CSIDL_COMPUTERSNEARME:
		strReturn = "CSIDL_COMPUTERSNEARME";
		break;

	default:
		strReturn = "UNKNOWN_CSIDL";
		break;
	}
	return strReturn;
}
//--------------------------------------------------------------------------------------------------
void getSpecialFolderPath(int CSIDL, string& rstrPath)
{
	// allocate space for the return value
	char szPath[MAX_PATH] = {0};

	HRESULT hr = SHGetFolderPath(NULL, CSIDL, NULL, SHGFP_TYPE_CURRENT, szPath);

	if (hr != S_OK)
	{
		UCLIDException ue("ELI20840", "Failed to get special folder path!");
		ue.addDebugInfo("CSIDL", CSIDL);
		ue.addDebugInfo("CSIDLString", getCSIDLAsString(CSIDL));
		ue.addHresult(hr);

		throw ue;
	}

	rstrPath = szPath;
}
//--------------------------------------------------------------------------------------------------
void setFileAttributes(const string& strFileName, DWORD dwFileAttributes,
					   bool bThrowExceptionIfNotSuccess)
{
	long lRetVal = SetFileAttributes(strFileName.c_str(), dwFileAttributes);

	if (!lRetVal && bThrowExceptionIfNotSuccess)
	{
		UCLIDException uex("ELI23604", "Unable to set file attributes.");
		uex.addWin32ErrorInfo();
		uex.addDebugInfo("File To Set Attributes", strFileName);
		uex.addDebugInfo("Attributes", dwFileAttributes);
		throw uex;
	}
}
//--------------------------------------------------------------------------------------------------
string getCleanImageName(const string& strImageFileName)
{
	// e.g. "D:\test image\test\123.tif" = "D:\test image\test\123.clean.tif"
	string strCleanImageName = getDirectoryFromFullPath(strImageFileName) + "\\"
		+ getFileNameWithoutExtension(strImageFileName) + ".clean"
		+ getExtensionFromFullPath(strImageFileName);

	return strCleanImageName;
}
//-------------------------------------------------------------------------------------------------
string getCleanImageNameIfExists(const string& strImageFileName)
{
	// get the clean image name
	string strCleanImageName = getCleanImageName(strImageFileName);

	// if it exists return the clean image, otherwise return the original
	return isValidFile(strCleanImageName) ? strCleanImageName : strImageFileName;
}
//--------------------------------------------------------------------------------------------------
unsigned __int64 getFreeSpaceOnDisk(string strDirectoryName)
{
	if (strDirectoryName.empty())
	{
		getTempDir(strDirectoryName);
	}

	// Ensure there is a trailing slash
	if (strDirectoryName[strDirectoryName.length() - 1] != '\\')
	{
		strDirectoryName += '\\';
	}
	
	ULARGE_INTEGER ulTemp;
	if (GetDiskFreeSpaceEx(strDirectoryName.c_str(), NULL, NULL, &ulTemp) == FALSE)
	{
		UCLIDException uex("ELI28702", "Unable to get free space on disk.");
		uex.addDebugInfo("Directory", strDirectoryName);
		uex.addWin32ErrorInfo();
		throw uex;
	}

	return ulTemp.QuadPart;
}
//--------------------------------------------------------------------------------------------------
