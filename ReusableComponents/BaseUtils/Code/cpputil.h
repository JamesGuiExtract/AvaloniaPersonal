//==================================================================================================
//
// COPYRIGHT (c) 2000 - 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	cpputil.h
//
// PURPOSE:	Various utility functions
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (August 2000 - present)
//
//==================================================================================================

#pragma once

#include "BaseUtils.h"
#include "mathUtil.h"
#include "ProcessInformationWrapper.h"
#include "RegistryPersistenceMgr.h"
#include "RegConstants.h"

#include <atltime.h>
#include <string>
#include <vector>
#include <list>
#include <algorithm>
#include <fstream>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Enums
//-------------------------------------------------------------------------------------------------
enum EReplaceType
{
	kReplaceFirst,
	kReplaceLast,
	kReplaceAll
};
//-------------------------------------------------------------------------------------------------
enum EFileType
{
	kUnknown,
	kNonImageFile,
	kTXTFile,
	kUSSFile,
	kImageFile,
	kVOAFile,
	kEAVFile,
	kXMLFile
};

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// Mode constants for _access_s function
const int giMODE_FILE_EXISTS = 00;
const int giMODE_WRITE_ONLY = 02;
const int giMODE_READ_ONLY = 04;
const int giMODE_READ_WRITE = 06;

// String collection constants for string functions
const string gstrUPPER_ALPHA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
const string gstrLOWER_ALPHA = "abcdefghijklmnopqrstuvwxyz";
const string gstrNUMBERS = "0123456789";
const string gstrALPHA = gstrUPPER_ALPHA + gstrLOWER_ALPHA;
const string gstrALPHA_NUMERIC = gstrALPHA + gstrNUMBERS;

// Valid identifier characters
const string gstrVALID_IDENTIFIER_CHARS = gstrALPHA_NUMERIC + "_";

//-------------------------------------------------------------------------------------------------
// ********* Operating System - Misc **********
//-------------------------------------------------------------------------------------------------
// Gets the login ID of the current user
// Example: If login ID is jsmith returns jsmith
EXPORT_BaseUtils string getCurrentUserName();
//-------------------------------------------------------------------------------------------------
// Gets the human readable user name
// Example: If login ID is jsmith and User Name
// is John Smith returns John Smith
EXPORT_BaseUtils string getFullUserName(bool bThrowExceptionIfNotFound=false);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string getComputerName();
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string getCurrentProcessID();
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils unsigned long	getDiskSerialNumber();
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string	getMACAddress();
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string getTimeAsString();
//-------------------------------------------------------------------------------------------------
// PROMISE: Return a string with a timestamp from the current time in the format:
//			MM-DD-YYYY - HH.MM.SS
EXPORT_BaseUtils string getTimeStamp();
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string getMillisecondTimeAsString();
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string getDateAsString();
//-------------------------------------------------------------------------------------------------
// PROMISE: Builds human-readable time from tmInput as "March 25, 2008 14:28:32".
EXPORT_BaseUtils string getHumanTimeAsString(CTime tmInput);
//-------------------------------------------------------------------------------------------------
// PROMISE: If the environment variable is not found, an empty string will be returned.  If it is 
//			found, the value will be returned.
EXPORT_BaseUtils string getEnvironmentVariableValue(const string& strVarName);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string getPrivateProfileString(const string& strAppName, 
												const string& strKeyName, 
												const string& strDefault, 
												const string& strFileName);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void emptyWindowsMessageQueue();
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void pumpMessageQueue();
//-------------------------------------------------------------------------------------------------
// PROMISE: To return true if the current operating system supports
//			window transparency features.
EXPORT_BaseUtils bool windowTransparencyIsSupported();
//-------------------------------------------------------------------------------------------------
// REQUIRE: User32.dll must exist on the system and must be loadable.
//			hWnd must be a handle to a valid window.
// PROMISE: If bTransparent == true, then the window represented by hWnd will 
//			be made transparent using the transparency level indicated by 
//			byteTransparency.
//			If bTransparent == false, then the window represented by hWnd will
//			be made un-transparent (i.e. opague and "normal") and the byteTransparency
//			argument is ignored.
//			The return value indicates whether the window was made transparent.
//			if bTransparent == true, the return value is true only if the window
//			was successfully made transparent
//			if bTransparent == false, the return value is always false.
EXPORT_BaseUtils bool makeWindowTransparent(HWND hWnd, 
											bool bTransparent,
											BYTE byteTransparency = 64);
//-------------------------------------------------------------------------------------------------
// PROMISE: This will return the number of logical processors that are available for use by the 
//			current process
EXPORT_BaseUtils long getNumLogicalProcessors();
//-------------------------------------------------------------------------------------------------
//replaces the environment string with the environment value in the str 
//(ex. replace $(USERNAME) in str with Jimmy(user name))
EXPORT_BaseUtils void replaceExprStrsWithEnvValues(string& strForReplace);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils bool isVirtKeyCurrentlyPressed(int iVirtKey);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To return a human readable string representing the specified windows error
// REQUIRE: 
// PROMISE: 
EXPORT_BaseUtils const string getWindowsErrorString(DWORD nError);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To return a human readable string using the specified formatting
// REQUIRE: 
// PROMISE: 
EXPORT_BaseUtils const string getFormattedString(const char* szText, ...);
//-------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------
// ******** Operating System - File System ********
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string removeLastSlashFromPath(string strInput);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the filename without the extension.  For instance, if
//			strFullPathToFile is "c:\\temp\\abc.txt", the returned value is
//			"abc"
//			If bMakeLowerCase == true, the value to be returned will be converted 
//			into lowercase.  If bMakeLowerCase == false, then the value to be 
//			returned maintains its original case from strFullPathToFile.
EXPORT_BaseUtils string getFileNameWithoutExtension(const string& strFullPathToFile,
													bool bMakeLowerCase = false);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the extension of the specified file.  The returned string will include
//			the period as the first character if the specified file does have an extension.
//			If the specified file does not have an extension, then an empty string will be
//			returned.  For instance, if strFullFileName is "c:\\temp\\abc.txt", the
//			returned value is ".txt".  If strFullFileName is "c:\\temp\\abc.txt.dat",
//			the returned value is ".dat".
//			If bMakeLowerCase == true, the value to be returned will be converted 
//			into lowercase.  If bMakeLowerCase == false, then the value to be 
//			returned maintains its original case from strFullPathToFile.
EXPORT_BaseUtils string getExtensionFromFullPath(const string& strFullFileName,
												 bool bMakeLowerCase = false);
//-------------------------------------------------------------------------------------------------
// PROMISE: To compute and return the directory associated with the fully specified
//			filename strFullFileName.  The returned string will NOT have a 
//			trailing slash character.  For instance, if strFullFilename is
//			"c:\\temp\\abc.txt", the returned value is "c:\\temp"
//			If bMakeLowerCase == true, the value to be returned will be converted 
//			into lowercase.  If bMakeLowerCase == false, then the value to be 
//			returned maintains its original case from strFullPathToFile.
EXPORT_BaseUtils string getDirectoryFromFullPath(const string& strFullFileName,
												 bool bMakeLowerCase = false);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the filename without the path.  For instance if strFullFilename
//			is "c:\\temp\\abc.txt", the returned value is "abc.txt"
//			If bMakeLowerCase == true, the value to be returned will be converted 
//			into lowercase.  If bMakeLowerCase == false, then the value to be 
//			returned maintains its original case from strFullPathToFile.
EXPORT_BaseUtils string getFileNameFromFullPath(const string& strFullFileName,
												bool bMakeLowerCase = false);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the filename and path without the extension.  For instance, if
//			strFullPathToFile is "c:\\temp\\abc.txt", the returned value is
//			"c:\\temp\\abc", and if strFullPathToFile is "c:\\temp\\abc.txt.dat",
//			the returned value is "c:\\temp\\abc.txt".
//			If bMakeLowerCase == true, the value to be returned will be converted 
//			into lowercase.  If bMakeLowerCase == false, then the value to be 
//			returned maintains its original case from strFullPathToFile.
EXPORT_BaseUtils string getPathAndFileNameWithoutExtension(const string& strFullPathToFile,
														   bool bMakeLowerCase = false);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the drive name without the path.  For instance if strFullFilename
//			is "c:\\temp\\abc.txt", the returned value is "c:\\"
//			If bMakeLowerCase == true, the value to be returned will be converted 
//			into lowercase.  If bMakeLowerCase == false, then the value to be 
//			returned maintains its original case from strFullPathToFile.
EXPORT_BaseUtils string getDriveFromFullPath(const string& strFullFileName, 
											 bool bMakeLowerCase);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils bool recursiveRemoveDirectory(const string &strDirectory);
//-------------------------------------------------------------------------------------------------
// Creates a directory, and if bSetPermissionsToAllUsers = true will set the security policy such
//		that all users have access to the directory
EXPORT_BaseUtils void createDirectory(const string& strDirectory, bool bSetPermissionsToAllUsers = false);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils bool getAllSubDirsAndDeleteAllFiles(const string &strDirectory, 
													 vector<string> &vecSubDirs);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils bool directoryExists(const string &strDir);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void copyFileToNewPath(const string& strOldFile, 
										const string& strNewPath);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void copySubDirectories(const vector<string>& vecSubDirsForCopy,
										 const string& strOldRoot, 
										 const string& strNewRoot);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void copyFiles(const vector<string>& vecFilesForCopy,
								const string& strOldRoot, 
								const string& strNewRoot);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string getClosestNonExistentUniqueFileName(const string& strOriginalFileName);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To get the fully qualified path of a file when it is specifed relative to another file
//			whose full path is known.
// REQUIRE: strParentFile is fully specified, or represents the name of a file in the current
//			directory.
//			strRelativeFile can be a fully specified file name or not
// PROMISE: To return a fully specified filename which refers to strRelativeFile relative to 
//			strParentFile.  If bValidate == true, and the computed absolute path
//			is not valid, an exception will be thrown.
EXPORT_BaseUtils string getAbsoluteFileName(string strParentFile, 
											const string& strRelativeFile, 
											bool bValidate = false);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To get the fully qualified path of a file when strFile contains relative path ("\..", "\.")
// REQUIRE: strFile is fully specified name that contains "\..", "\."
// PROMISE: To return a fully specified filename from the relative path
// Example: strFile = "C:\Redaction\FPS\..\Images\my\001.tif"
// Final value: strFile = "C:\Redaction\Images\my\001.tif"
EXPORT_BaseUtils void simplifyPathName(string& strFile);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To get the file name that is relative to the parent file name
//			whose full path is known.
// REQUIRE: strParentFile must be a fully specified file name
//			strAbsoluteFile must be a fully specified file name.
// PROMISE: To return a filename which is relative to strParentFile 
//
EXPORT_BaseUtils string getRelativeFileName(const string& strParentFile, 
											const string& strAbsoluteFile);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To build an absolute path to a file/directory from a path relative to
//			getCurrentDirectory()
//
// PROMISE: To return an absolute path to the passed in file or
//			directory path. If the provided path is already
//			an absolute path, then will just return strFileOrDirPath
EXPORT_BaseUtils string buildAbsolutePath(const string& strFileOrDirPath);
//-------------------------------------------------------------------------------------------------
// Finds all files with specified extension under provided strDirectory and add them to rvecFiles
// bRecursive - true : returns all files under strDirectory and its sub directories 
//					   and its sub directories...
//				false: only returns the files under strDirectory.
// strFileExtension - by default, it's all files
// Require : directory name shall be fully qualified. It can't be empty
EXPORT_BaseUtils void  getFilesInDir(vector<string>& rvecFiles, 
									 const string& strDirectory, 
									 const string& strFileExtension = "*.*",
									 bool bRecursive = false);
//-------------------------------------------------------------------------------------------------
// Returns all subdirectories under the provided strDirectory (full paths, not just the directory
// name).
// bRecursive- true to return all descendant directories recursively. false to return only the
//		immediate sub-directories.
EXPORT_BaseUtils vector<string> getSubDirectories(string strDirectory, bool bRecursive);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Converts a fully qualified file/path name to UNC path
// Example:	I:\Common\Engineering\Tools\Utils\grep.txt  ==> 
//			\\frank\internal\Common\Engineering\Tools\Utils\grep.txt
// REQUIRE: The drive letter must be mapped to one of the network drives, otherwise
//			it will simply return the original string. For instance if 
//			strLocalPath == "C:\WINNT\temp.txt", it returns "C:\WINNT\temp.txt"
EXPORT_BaseUtils string getUNCPath(const string& strLocalPath);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Returns all sub folders's name under current folder.
// REQUIRE: strParentFolder must be a fully qualified folder name
// NOTE: The return sub folder names are not fully qualified.
//		 Also, this function only returns the first level sub folders, i.e. no sub
//		 folders under a sub folder of strParentFolder will be listed.
EXPORT_BaseUtils vector<string> getSubFolderShortNames(const string& strParentFolder);
//-------------------------------------------------------------------------------------------------
// PROMISE: To throw an exception if strName represents a non existing file
//			or folder name. If strELICode is specified, this will be used as the
//			ELI code in the exception thrown if strName does not exist.
EXPORT_BaseUtils void validateFileOrFolderExistence(const string& strName,
													const string& strELICode = "");
//-------------------------------------------------------------------------------------------------
// PROMISE: To return false if strFileName represents a non existing file
//			or folder name
EXPORT_BaseUtils bool isFileOrFolderValid(const string& strName);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return true if and only if strFileName represents an existing file that is
//			readable.
EXPORT_BaseUtils bool fileExistsAndIsReadable(const string& strName);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return true iff strFileName represents an existing file that
//			is read-only
EXPORT_BaseUtils bool fileExistsAndIsReadOnly(const string& strFileName);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return true if strName is a read only file or folder
//			
EXPORT_BaseUtils bool isFileReadOnly(const string& strName );
//-------------------------------------------------------------------------------------------------
// PROMISE: To throw an exception if strName is a read only file [p16 #2755]
// REQUIRE: strName is a valid file
EXPORT_BaseUtils void verifyFileIsWritable(const string& strName);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return true if strName is a PDF file
EXPORT_BaseUtils bool isPDFFile(const string& strName);
//-------------------------------------------------------------------------------------------------
// Takes a short path name (eg. C:\Progra~1\ABC) and converts it to a long 
// path name (eg. C:\Program Files\ABC)
// Return the length of the long path
EXPORT_BaseUtils int getLongPathName(const string& strShortPathName,
									 string& strLongPath);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return true if strFileName is an absolute path to a folder
// or file.  An absolute path is one that contains the colon character,
// (such as in c:\temp\abc) or one that starts with two slashes 
// (such as in \\rover\internal\a.txt).
// NOTE: This function does not check for the existence of strFileName.
EXPORT_BaseUtils bool isAbsolutePath(const string& strFileOrFolderName);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the current directory as a string using windows GetCurrentDirectory function.
//			The returned path will not contain a trailing slash.
EXPORT_BaseUtils string getCurrentDirectory();
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the file size in bytes
EXPORT_BaseUtils ULONGLONG getSizeOfFile(const string& strFileName);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the total file size in bytes
EXPORT_BaseUtils ULONGLONG getSizeOfFiles(const vector<string>& vecFileNames);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Returns true if the specified filename matches the name criteria, otherwise false
//			The name criteria (strMatch) is a typical windows filename that can include wildcards
//			like * and ?.  Valid values for strMatch would be *.* or 1101??.txt or *.tif.  
//			strFileName is just a filename name with no wild cards.
// REQUIRE: strMatchPattern and strFileName length must be less than MAX_PATH
EXPORT_BaseUtils bool doesFileMatchPattern(const string& strMatchPattern, 
										   const string& strFileName);
//-------------------------------------------------------------------------------------------------
// PUPROSE:	Works like doesFileMatchPattern but attempts to match strFileName against multiple
//			patterns
// REQUIRE: strMatchPatterns values and strFileName length must be less than MAX_PATH
EXPORT_BaseUtils bool doesFileMatchPatterns(const vector<string>& strMatchPatterns, 
											const string& strFileName);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Returns the path to the temp directory as defined by windows
//			 
EXPORT_BaseUtils void getTempDir(string& strPath);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the path to the specified special folder.  The returned
//			path will not contain a trailing slash.
//
// ARGS:	CSIDL - CSIDL value of the folder path to return
//			rstrPath - The string that will contain the path after the function call
EXPORT_BaseUtils void getSpecialFolderPath(int CSIDL, string& rstrPath);
//-------------------------------------------------------------------------------------------------
// POMISE: To return the path to the extract systems folder in the common application data
//		   path. The returned path will not contain a trailing slash.
EXPORT_BaseUtils string getExtractApplicationDataPath();
//-------------------------------------------------------------------------------------------------
// POMISE: To return the path to the extract systems license folder in the common application data
//		   path. The returned path will not contain a trailing slash.
EXPORT_BaseUtils string getExtractLicenseFilesPath();
//-------------------------------------------------------------------------------------------------
// PROMISE: To attempt to create a temporary file in the folder specified (or
//			in the case of a file being specified, the folder of the file specified)
//			and to return true if it is successful or false if not
//
// NOTE:	The temporary file will be deleted after being successfully created
EXPORT_BaseUtils bool canCreateFile(string strFQFileOrFolderName);
//-------------------------------------------------------------------------------------------------
// PROMISE: Will return true if strFile is a valid file(directory will return false)
EXPORT_BaseUtils bool isValidFile(const string& strFile);
//-------------------------------------------------------------------------------------------------
// PROMISE: Will return true if strFolder is a valid folder (if strFolder is a file this will 
//			return false)
EXPORT_BaseUtils bool isValidFolder(const string& strFolder);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils bool isNumericExtension(const string& strExt);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the file version
// REQUIRE: strFileFullName must be fully qualified file name
// NOTE:	If error occurred will log an exception and return an empty string
EXPORT_BaseUtils string getFileVersion(const string& strFileFullName);
//-------------------------------------------------------------------------------------------------
// PROMISE: To run an EXE from within C++.  Allows optional parameters, timeout, working directory 
//			and creation flags. For an immediate return while the new process 
//			runs independently, use dwTimeoutInMilliseconds = 0.  To wait for a specified time
//			for the new process to finish, use dwTimeoutInMilliseconds > 0.  To wait until the
//			period process exits, use dwTimeoutInMilliseconds = INFINITE.  See MSDN documentation 
//			for WaitForSingleObject() for more information.  Providing a non-NULL 
//			ProcessInformationWrapper object allows the caller to manipulate the PROCESS_INFORMATION 
//			structure after return.
EXPORT_BaseUtils void runEXE(const string& strExeFullFileName, const string& strParameters = "",
							 const DWORD dwTimeoutInMilliseconds = 0, 
							 ProcessInformationWrapper* pPIW = NULL,
							 const string& strWorkingDir = "", 
							 DWORD dwCreationFlags = DETACHED_PROCESS);
//-------------------------------------------------------------------------------------------------
// PROMISE: To run an Extract Systems EXE from within C++.  Provides /ef <filename> option for 
//			storage of any exception thrown by the EXE.  If <filename> contains an exception, 
//			the exception is loaded and rethrown.  This allows the calling scope to properly 
//			handle any errors from the outside scope.  See runEXE() for additional argument details.
EXPORT_BaseUtils void runExtractEXE(const string& strExeFullFileName, 
									const string& strParameters = "",
									const DWORD dwTimeoutInMilliseconds = 0, 
									ProcessInformationWrapper* pPIW = NULL,
									const string& strWorkingDir = "", 
									DWORD dwCreationFlags = DETACHED_PROCESS);
//-------------------------------------------------------------------------------------------------
// PROMISE: To run an EXE from within C++. This call always has a timeout of infinite. Upon
//			launching the EXE, an IdleProcessKiller will also be spawned to monitor the process.
//			Upon process completion (via normal exit, idle process killed, or some other error
//			result) the exit code will be checked and returned.
// ARGS:	strExeFullFileName - The full path of the executable to run
//			bIsExtractExe - Whether the executable is an extract exe (if true then this
//							call will behave similar to runExtractExe.
//			strParameters - Any parameters that should be passed to the executable
//			strWorkingDirectory - The working directory for the process
//			iIdleTimeout - How long a process should be idle before it is killed (in milliseconds)
//			iIdleCheckInterval - How often to check on the process (in milliseconds)
EXPORT_BaseUtils DWORD runExeWithProcessKiller(const string& strExeFullFileName,
											   bool bIsExtractExe,
											   string strParameters = "",
											   const string& strWorkingDirectory = "",
											   int iIdleTimeout=120000,
											   int iIdleCheckInterval=2000);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the directory associated with hModule.  The returned string will not have
//			a trailing slash character.
EXPORT_BaseUtils string getModuleDirectory(HMODULE hModule);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the directory associated with strModuleShortFileName.  The returned 
//			string will not have a trailing slash character.
EXPORT_BaseUtils string getModuleDirectory(const string& strModuleShortFileName);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the full path to the EXE representing the current process.
EXPORT_BaseUtils string getCurrentProcessEXEFullPath();
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the directory associated with the current process.  The returned 
//			string will have a trailing slash character.
EXPORT_BaseUtils string getCurrentProcessEXEDirectory();
//-------------------------------------------------------------------------------------------------
// PURPOSE: To copy a file from strSrcFileName to strDstFileName. If bUpdateFileSettings is true, 
//          will update strDstFileName's modification/access time to the current system time and 
//          will remove its read-only attribute if it is set.
// REQUIRE: strSrcFileName must be the name of a valid input file.
//          strDstFileName must be a valid filename to create an output file.
// PROMISE: (1) strSrcFileName will be copied to strDstFileName. 
//          (2) If strDstFileName file already exists, it will be overwritten. 
//          (3) If bUpdateFileSettings is true:
//			    (a) The read-only attribute on the output file will be removed.
//              (b) strDstFileName's access and modification times will be set to the current time.
EXPORT_BaseUtils void copyFile(const string &strSrcFileName, const string &strDstFileName,
							   bool bUpdateFileSettings=false, bool bAllowReadonly=false);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To move a file from one location to another
//			overwriting an existing file if bOverwrite = true
//			Whether the files will be moved securely will be determined by the
//			SecureDeleteAllSensitiveFiles registry setting.
// REQUIRE: 
// PROMISE: 
EXPORT_BaseUtils void moveFile(const string strSrcFileName, 
							   const string strDstFileName,
							   const bool bOverwrite = false);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To move a file from one location to another
//			overwriting an existing file if bOverwrite = true
//			if bSecureMove is true, when moving files to a new value, the old copy will be
//			securely deleted.
// REQUIRE: 
// PROMISE: 
EXPORT_BaseUtils void moveFile(const string strSrcFileName, 
							   const string strDstFileName,
							   const bool bOverwrite,
							   const bool bSecureMove);
////-------------------------------------------------------------------------------------------------
// PURPOSE: To delete a file that is not readonly.
// NOTE:	When this override is used, files will be securely deleted according to the
//			SecureDeleteAllSensitiveFiles registry value.
// REQUIRE: 
// PROMISE: 
EXPORT_BaseUtils void deleteFile(const string strFileName);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To delete a file.
// ARGS:	bAllowReadonly - If true, will delete a readonly file, otherwise it will not.
// NOTE:	When this override is used, files will be securely deleted according to the
//			SecureDeleteAllSensitiveFiles registry value.
// REQUIRE: 
// PROMISE: 
EXPORT_BaseUtils void deleteFile(const string strFileName, const bool bAllowReadonly);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To delete a file, will delete a readonly file if bAllowReadonly = true
// ARGS:	bAllowReadonly - If true, will delete a readonly file, otherwise it will not.
//			bSecureDelete - If true, will securely delete a file by overwriting and obfuscating it.
//							If false, the file will not be deleted securely regardless of the
//							the SecureDeleteAllSensitiveFiles registry value.
//			bThrowIfUnableToDeleteSecurely - If true, when securely deleteing a file, but the file
//				could not be securely overwritten, an exception will be throw before attempting
//				the actual deletion.
// REQUIRE: 
// PROMISE: 
EXPORT_BaseUtils void deleteFile(const string strFileName, const bool bAllowReadonly,
	bool bSecureDelete, bool bThrowIfUnableToDeleteSecurely = false);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To get the retry count and timeout from the registry keys.  These values are
//			used by waitForFileAccess, waitForFileToBeReadable, and waitForStgFileAccess.
// NOTE:	Registry keys -
//			HKLM\Software\Extract Systems\ReusableComponents\BaseUtils\FileAccessRetries
//			HKLM\Software\Extract Systems\ReusableComponents\BaseUtils\FileAccessTimeout
EXPORT_BaseUtils void getFileAccessRetryCountAndTimeout(int& riRetryCount, int& riRetryTimeout);
//-------------------------------------------------------------------------------------------------
// PURPOSE: Waits for a file to be accessable with the given access permissons 
// NOTE:	This method uses access_s to check accessablity and will retry a number of times
//			given by the registry key : HKLM\Software\Extract Systems\ReusableComponents\BaseUtils\FileAccessRetries
//			and waits between retries the number of seconds in key 
//			HKLM\Software\Extract Systems\ReusableComponents\BaseUtils\FileAccessTimeout
//			If the number of retries is exceeded an exception will be thrown
EXPORT_BaseUtils void waitForFileAccess(const string& strFileName, int iAccess);
//-------------------------------------------------------------------------------------------------
// PURPOSE: Waits for a file to be readable (performs an open file call as opposed
//			to waitForFileAccess which merely performs an _access check)
//			If bLogException == true an exception will be logged if the file us unable to be
//			read, if bLogException == false no exception will be logged. (No exception
//			will be thrown for either case, this method is simply a way to provide the OS time
//			to free a file for reading). If the file is successfully opened for reading and
//			ppinFile != __nullptr then ppinFile will point to the open ifstream object.
EXPORT_BaseUtils void waitForFileToBeReadable(const string& strFileName,
											  bool bLogException = true,
											  ifstream** ppinFile = NULL,
											  int nOpenMode = ios::in);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To set the attributes of a particular file.  If bThrowExceptionIfNotSuccess = true
//			then will throw a UCLIDException if the operation fails, otherwise the error is
//			ignored.
EXPORT_BaseUtils void setFileAttributes(const string& strFileName, DWORD dwFileAttributes,
										bool bThrowExceptionIfNotSuccess = true);
//-------------------------------------------------------------------------------------------------
// PURPOSE: Returns the cleaned image name corresponding to strImageFileName
EXPORT_BaseUtils string getCleanImageName(const string& strImageFileName);
//---------------------------------------------------------------------------------------------
// PURPOSE: If the cleaned image exists returns the clean image name corresponding to 
//          strImageFileName, otherwise returns strImageFileName.
EXPORT_BaseUtils string getCleanImageNameIfExists(const string& strImageFileName);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To return the amount of free space on the specified disk (if no
//			path is specified then returns the amount of free space on the disk
//			containing the TEMP directory)
EXPORT_BaseUtils unsigned __int64 getFreeSpaceOnDisk(string strPath = "");

//-------------------------------------------------------------------------------------------------
// Char Operations 
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils inline char getLowerCaseChar(char cChar)
{
	return (cChar >= 'A' && cChar <= 'Z') ? cChar + ('a' - 'A') : cChar;
}
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils inline char getUpperCaseChar(char cChar)
{
	return (cChar >= 'a' && cChar <= 'z') ? cChar - ('a' - 'A') : cChar;
}
//-------------------------------------------------------------------------------------------------
// if rucChar is a lower-case hex char, it will be converted into an upper-case hex-char
EXPORT_BaseUtils inline bool isHexChar(unsigned char& rucChar)
{
	if (rucChar >= 'a' && rucChar <= 'f')
		rucChar -= 'a' - 'A';

	return (rucChar >= '0' && rucChar <= '9') || (rucChar >= 'A' && rucChar <= 'F');
}
//-------------------------------------------------------------------------------------------------
// PROMISE: return true if usChar is a whitespace character
EXPORT_BaseUtils inline bool isWhitespaceChar(unsigned short usChar)
{
	return (usChar == ' ' || usChar == '\r' || usChar == '\n' || usChar == '\t');
}
//-------------------------------------------------------------------------------------------------
// PROMISE: return true if usChar is a digit character
EXPORT_BaseUtils inline bool isDigitChar(unsigned short usChar)
{
	return (usChar >= '0' && usChar <= '9');
}
//-------------------------------------------------------------------------------------------------
// PROMISE: return true if usChar is one of the 26 alphabet characters (case insensitive)
EXPORT_BaseUtils inline bool isAlphaChar(unsigned short usChar)
{
	return (usChar >= 'a' && usChar <= 'z') || (usChar >= 'A' && usChar <= 'Z');
}
//-------------------------------------------------------------------------------------------------
// PROMISE: return true if ucChar is a printable character
EXPORT_BaseUtils inline bool isPrintableChar(unsigned char ucChar)
{
	return (ucChar >= 0x20 || ucChar == 0x9 || ucChar == 0xA || ucChar == 0xD);
}
//-------------------------------------------------------------------------------------------------
// REQUIRE: isHexChar(ucChar) == true.
EXPORT_BaseUtils unsigned char getValueOfHexChar(unsigned char ucChar);

//-------------------------------------------------------------------------------------------------
// String Operations 
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils bool replaceVariable(string& s, 
									  const string& t1, 
									  const string& t2);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils bool replaceVariable(string& s, 
									  const char * t1, 
									  const string& t2);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils bool replaceVariable(string& s, 
									  const string& t1, 
									  const string& t2, 
									  EReplaceType eReplaceType);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils int getNumberOfDigitsAfterPosition(const string &strText, 
													int iPos = -1);
//-------------------------------------------------------------------------------------------------
//EXPORT_BaseUtils bool truncateUntilSeparator(string& strOrigString, 
//											 string strSeparator);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void makeUpperCase(string & strInput);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void makeLowerCase(string & strInput);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void makeTitleCase(string & strInput);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void makeSentenceCase(string& strInput);
//-------------------------------------------------------------------------------------------------
// Reverse the characters of strText
// i.e. "Hello World!" ---> "!dlroW olleH"
EXPORT_BaseUtils void reverseString(string& strText);
//-------------------------------------------------------------------------------------------------
// find the first position in strText starting at iStartingPosition that contains a numeral or 
// a period.
EXPORT_BaseUtils unsigned int getPositionOfFirstNumeric(const string &strText, 
														unsigned int uiStartingPosition);
//-------------------------------------------------------------------------------------------------
// find the first position in strText starting at iStartingPosition that contains a non-numeral,
// non-period character
EXPORT_BaseUtils unsigned int getPositionOfFirstAlpha(const string &strText, 
								 					  unsigned int uiStartingPosition);
//-------------------------------------------------------------------------------------------------
// PROMISE: return a string with all of the strings in the vecLines vector concatenated together
//			with the strSeparator specified between them and all white space ( \f\n\r\t\v) removed
//			if the bRemoveWhiteSpace is set to true, the whitespace will be removed before the 
//			separator is added
EXPORT_BaseUtils string asString(vector<string> vecLines, 
								 bool bRemoveWhiteSpace = true, 
								 const string& strSeparator = "" );
//-------------------------------------------------------------------------------------------------
// PROMISE: return double quantity if valid string, else throw UCLID Exception
EXPORT_BaseUtils double asDouble(const string &strValue);
//-------------------------------------------------------------------------------------------------
// PROMISE: return long quantity if valid string, else throw UCLID Exception
EXPORT_BaseUtils long asLong(const string &strValue);
//-------------------------------------------------------------------------------------------------
// PROMISE: return unsigned long quantity if valid string, else throw UCLID Exception
EXPORT_BaseUtils unsigned long asUnsignedLong(const string &strValue);
//-------------------------------------------------------------------------------------------------
// PROMISE: return longlong quantity if valid string, else throw UCLID Exception
EXPORT_BaseUtils LONGLONG asLongLong(const string& strValue);
//-------------------------------------------------------------------------------------------------
// PROMISE: return unsigned longlong quantity if valid string, else throw UCLID Exception
EXPORT_BaseUtils ULONGLONG asUnsignedLongLong(const string& strValue);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string trim(const string& s, 
							 const string& strBefore, 
							 const string& strAfter);
//-------------------------------------------------------------------------------------------------
//if the current position in strText is an alpha-numeric character
EXPORT_BaseUtils bool isAlphaNumeric(const string& strText, 
									 const unsigned int& uiPos);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils bool containsNonWhitespaceChars(const string& strText);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils bool containsAlphaNumericChar(const string& strText);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils bool containsAlphaChar(const string& strText);
//-------------------------------------------------------------------------------------------------
// convert input cpp format string into a normal format string.
// Example: "hello \n hello"(actual display is "hello <new line character> hello") 
//			 --> "hello \\n hello" (actual display is "hello \n hello")
//			"hello \\t hello" (actual display is "hello \t hello") 
//			 --> "hello \\\\t hello"(actual display is "hello \\t hello") 
EXPORT_BaseUtils void convertCppStringToNormalString(string& strCppStr);
//-------------------------------------------------------------------------------------------------
// convert normal string to cpp string
// Example: "hello \\\\n hello" --> "hello \\n hello"
//			"hello \\x2c hello" --> "hello , hello"
//			This will not support converting more that 2 hex digits after \\x nor will it 
//			convert octal digits e.g. "\\54" will not be changed to ",' but stay "\\54"
EXPORT_BaseUtils void convertNormalStringToCppString(string& strNormalStr);
//-------------------------------------------------------------------------------------------------
// make a string suitable for using as part of a regular expression
// i.e. put a back-slash in front of every character that has special meaning in the context
// of regular expressions
// Example: "Why?" --> "Why\?"
EXPORT_BaseUtils void convertStringToRegularExpression(string& str);
//-------------------------------------------------------------------------------------------------
// replace specified all zAsciiChar inside strForReplace with hexadecimal notation
// Example: "0" --> "\x30", "," --> "\x2c"
EXPORT_BaseUtils void replaceASCIICharWithHex(char zAsciiChar, 
											  string& strForReplace);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Replaces each pattern string contained in the input string into
//			a meaningful string based on the vector of interpretations, and returns
//			the final result. If % is the special character. It must be followed by
//			a number or another %.
// Example:	Input string: "They are %1 and %2. %% is a special character.", 
//			Interpretations: {Tommy, Jerry}
//			Return string: "They are Tommy and Jerry. % is a special character."
EXPORT_BaseUtils string combine(const string& strInput,
									 const vector<string>& vecInterpretations,
									 const string& strSpecialCharacter = "%");
//-------------------------------------------------------------------------------------------------
// Replace strWordToBeReplaced in the input string that doesn't have any leading
// or trailing alpha-numeric characters with strReplacementWord
// bReplaceFirstMatchOnly --  whether or not to replace all matches
//							  in the input text
EXPORT_BaseUtils void replaceWord(string& strInputText, 
								  const string& strWordToBeReplaced,
								  const string& strReplacementWord,
								  bool bReplaceFirstMatchOnly,
								  bool bCaseSensitive);
//-------------------------------------------------------------------------------------------------
// PROMISE: Trims specified words from beginning of string. Returns true if anything was trimmed
EXPORT_BaseUtils bool trimLeadingWord(string& strInput, 
									  const string& strWord);
//-------------------------------------------------------------------------------------------------
// PROMISE: Will remove any low-order ASCII unprintable characters from strInput.
//          Examples include: 0x1E as seen in P16 #1413
//          Valid characters: { 0x09, 0x0A, 0x0D, 0x20, 0x21, ..., 0x7F }
EXPORT_BaseUtils string removeUnprintableCharacters(const string& strInput);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the number of times cChar appears in strText
EXPORT_BaseUtils unsigned long getCountOfSpecificCharInString(const string& strText, 
															  char cChar);
//-------------------------------------------------------------------------------------------------
// PURPOSE: Searches strInput for any character in strChars and replaces it with strReplace.
//			Then consolidates strReplace into 1 sequential occurrence.
// ARGS:	strInput: the string to be consolidated
//			strChars: the individual characters, which if found in sequence should
//					  be consolidated. (ie: " \r\n\t" will clear all white spaces.)
//			strReplace: The string to replace with.
//			bCaseSensitive represents the case-sensitivity of character comparisions
//			done in thie function
// PROMISE:	To return the consolidated string
// NOTES:	For example, if input string is "ABBBCYYYYZC", strChars is "BYZ", and strReplace
//			is 'X',	the returned string will be AXCXC.
EXPORT_BaseUtils string replaceMultipleCharsWithOne(const string& strInput, 
															 const string& strChars, 
															 const string& strReplace,
															 bool bCaseSensitive);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void makeFirstCharToUpper(string& strInput);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string makeFirstCharToUpper(const string& strInput);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils int getNextDelimiterPosition(const string &strText, 
											  int iStartingPosition, 
											  const string& strDelimiters);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string incrementNumericalSuffix(const string& strInput);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string padCharacter(const string& strString, 
									 bool bPadBeginning, 
									 char cPadChar, 
									 unsigned long ulTotalStringLength);
//-------------------------------------------------------------------------------------------------
// whether or not the strWord exists in the strInput and is a stand-alone word. i.e. there's
// no alpha-char on either side of the word.
// Returns the start position of the word in the input string. Returns -1 if not found 
// or is not a stand-alone word.
EXPORT_BaseUtils int findWordMatch(const string& strInput, 
								   const string& strWord, 
								   int nStartPos = 0,		// start searching position
								   bool bCaseSensitive = false);
//-------------------------------------------------------------------------------------------------
// Checks whether or not the provided string is in sentence case (i.e. only the first
// word has been capitalized)
EXPORT_BaseUtils bool isSentenceCase(const string& strText);

//-------------------------------------------------------------------------------------------------
// ********* Number Operations **********
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the rounded value of a number, closest to the nearest
//			integer.
//			Example: -5.5 is rounded to -6
//					  1.4 is rounded to 1
//					  2.6 is rounded to 3
//					  0.4 is rounded to 0
//					 -3.4 is rounded to -3
EXPORT_BaseUtils long round(double dNum);
//-------------------------------------------------------------------------------------------------
// PROMISE: Will convert nNum (12340095) to a comma formatted number string (12,340,095)
EXPORT_BaseUtils string commaFormatNumber(LONGLONG nNum);
//-------------------------------------------------------------------------------------------------
// PROMISE: Will convert nNum (12340095.5523) to a comma formatted number string (12,340,095.5523)
//			nPrecision specifies the number of decimal places desired
EXPORT_BaseUtils string commaFormatNumber(double fNum, 
										  long nPrecision = 6);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string asString(unsigned long ulValue);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string asString(unsigned int uiValue);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string asString(double dValue);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string asString(double dValue, unsigned int uiNumDecimalPlaces);
//-------------------------------------------------------------------------------------------------
// PROMISE: Converts dValue to a string with the precision specified by uiMaxDecimalPlaces, except
//			trailing zeros will trimmed until there are only uiMinDecimalPlaces decimal places
//			remaining.
EXPORT_BaseUtils string asString(double dValue, unsigned int uiMinDecimalPlaces,
								 unsigned int uiMaxDecimalPlaces);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string asString(long lValue);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string asString(int iValue);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string asString(LONGLONG llValue);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string asString(ULONGLONG ullValue);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string asString(const CLSID& clsID);
//-------------------------------------------------------------------------------------------------
// PROMISE: Will check whether the Commas in a string are in right position
//			It doesn't check whether it is a valid number.
bool isValidCommaFormat(const string &strValue);
//-------------------------------------------------------------------------------------------------
// PROMISE: To validate the Comma format in a string containing an integer 
//			and then remove the commas
void validateRemoveCommaInteger(string& str);
//-------------------------------------------------------------------------------------------------
// PROMISE: To validate the Comma format in a string containing a double 
//			and then remove the commas
void validateRemoveCommaDouble(string& str);

//-------------------------------------------------------------------------------------------------
// ********* File I/O *********
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void writeToFile(const string& strData, 
								  const string& strOutputFileName);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void appendToFile(const string& strData, 
								  const string& strOutputFileName);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils string getTextFileContentsAsString(const string& strTextFileName);
//-------------------------------------------------------------------------------------------------
EXPORT_BaseUtils void convertFileToListOfStrings(ifstream &file, 
												 list<string> &lstFileContents);

//-------------------------------------------------------------------------------------------------
// ********* Misc **********
//-------------------------------------------------------------------------------------------------
// PURPOSE:	To retrieve arguments as a vector of strings
// REQUIRE:	argc, and argv are valid parameters passed to the application
// PROMISE:	All arguments except the first argument (argv[0], which is the
//			full path to the current application), will be added as strings
//			to the result vector.  Each of the strings will be made into
//			uppercase if bMakeUpperCase == true;
EXPORT_BaseUtils vector<string> getArgumentsAsVector(int argc, 
															   char *argv[], 
															   bool bMakeUpperCase);
//-------------------------------------------------------------------------------------------------
// PROMISE:	To return true if there exists at least one item in vecStrings that
//			begins with strTextToFind.  If true is returned, then rnIndex will
//			contain the earliest index at which there is a string in vecStrings 
//			with strTextToFind as its prefix.
EXPORT_BaseUtils bool vectorContainsStringWithPrefix(const vector<string>& vecStrings, 
													 const string& strTextToFind,
													 long& rnIndex);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return true if strExt is a recognized image file extension.  
// It does not matter whether strExt is of the format ".tif" or "tif" (the
// leading period does not matter)
EXPORT_BaseUtils bool isImageFileExtension(string strExt);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the file type if it is recognized based upon file extension, otherwise
//			returns kFileUnknown
EXPORT_BaseUtils EFileType getFileType(const string& strFileName);
//-------------------------------------------------------------------------------------------------
// PROMISE:	returns true if the string in strName is a valid identifier i.e. _*[a-zA-Z][a-zA-Z0-9_]*
//			 
EXPORT_BaseUtils bool isValidIdentifier(const string& strName);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Throws UCLIDException if strName is NOT a valid identifier i.e. _*[a-zA-Z][a-zA-Z0-9_]*
//			 
EXPORT_BaseUtils void validateIdentifier(const string& strName);
//-------------------------------------------------------------------------------------------------
// PROMISE:	returns the ending position of an identifier embedded in strText that starts at nStartPos
//			For instance say:
//			strText = "I hate %name so much"
//			nStartPos = 8
//			In this case the function would return 12 or the position of the ' ' following "name" 
//			becuase that character is the end of the identifier + 1
//			The entire identifer is [nStartPos, retVal)
//
EXPORT_BaseUtils long getIdentifierEndPos(const string& strText, 
										  long nStartPos);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Returns the position in the string where a "scope" is closed
//			For instance say:
//			strText = "(I love(to nest)nesting paren(theses) )but who) ) ) doesn't"
//			nStartPos = 0
//			cScopeOpen = '('  
//			cScopeClose = ')'
//			The function would return 39 or the poisition of the letter 'b' in "but"
//			because the ')' before 'b' closes the scope opened with the '(' at zero
//
//			nStartPos must always be the index of a cScope open character
//			It always returns the index of the closing character + 1
//			The entire scope is [nStartPos, retVal)
//			
EXPORT_BaseUtils long getCloseScopePos(const string& strText, 
									   long nStartPos, 
									   char cScopeOpen, 
									   char cScopeClose);
//-------------------------------------------------------------------------------------------------
// PROMISE:	works like the above but the scope open and close can be strings rather than just characters
//			in this case the character at nStartPos must always be the first character of a strScopeOpen
//			and the return value will be the final character of strScopeClose + 1
//
EXPORT_BaseUtils long getCloseScopePos(const string& strText, 
									   long nStartPos, 
									   string strScopeOpen, 
									   string strScopeClose);
//-------------------------------------------------------------------------------------------------
// PROMISE:	Converts "r.g.b" string into RGB value via RGB_MAKE(r,g,b).  RGB separator defaults 
//          to period.  Returns -1 if strInput = "".
// REQUIRE:	0 <= r <= 255, 0 <= g <= 255, 0 <= b <= 255
EXPORT_BaseUtils long getRGBFromString(const string& strInput, 
									   char cSeparator = '.');
//-------------------------------------------------------------------------------------------------
// PROMISE:	Inverts the specified RGB color. (e.g. black becomes white, white becomes black, etc.)
EXPORT_BaseUtils COLORREF invertColor(COLORREF crColor);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To display Message box, with formatted text
// REQUIRE: szText, and the variable argument list must 
//			adhere to the printf formatting guidelines
// PROMISE: This will display a MessageBox with text formatted
//			like printf
EXPORT_BaseUtils int formatMessageBox(const char* szText, ...);

//-------------------------------------------------------------------------------------------------
// PURPOSE: To return the bool value represented in the strBool
// PROMISE: Returns corresponding bool value if lowercase conversion of strBool is "true" or "false" 
//			otherwise will throw an exception
EXPORT_BaseUtils bool asCppBool( string strBool);

//-------------------------------------------------------------------------------------------------
inline bool asCppBool(VARIANT_BOOL bValue)
{
	return (bValue == VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
inline bool asCppBool(BOOL bValue)
{
	return (bValue == TRUE);
}
//-------------------------------------------------------------------------------------------------
inline BOOL asMFCBool(VARIANT_BOOL bValue)
{
	return (bValue == VARIANT_TRUE) ? TRUE : FALSE;
}
//-------------------------------------------------------------------------------------------------
inline BOOL asMFCBool(bool bValue)
{
	return bValue ? TRUE : FALSE;
}
//-------------------------------------------------------------------------------------------------
inline int asBSTChecked(bool bValue)
{
	return bValue ? BST_CHECKED: BST_UNCHECKED;
}
//-------------------------------------------------------------------------------------------------
inline int asBSTChecked(VARIANT_BOOL bValue)
{
	return (bValue == VARIANT_TRUE) ? BST_CHECKED: BST_UNCHECKED;
}
//-------------------------------------------------------------------------------------------------
inline VARIANT_BOOL asVariantBool(BOOL bValue)
{
	return (bValue == TRUE) ? VARIANT_TRUE : VARIANT_FALSE;
}
//-------------------------------------------------------------------------------------------------
inline VARIANT_BOOL asVariantBool(bool bValue)
{
	return bValue ? VARIANT_TRUE : VARIANT_FALSE;
}
//-------------------------------------------------------------------------------------------------
inline bool isEqual(VARIANT_BOOL bValue1, BOOL bValue2)
{
	return asMFCBool(bValue1) == bValue2;
}
//-------------------------------------------------------------------------------------------------
inline bool isEqual(BOOL bValue1, VARIANT_BOOL bValue2)
{
	return asMFCBool(bValue2) == bValue1;
}
//-------------------------------------------------------------------------------------------------
inline bool isEqual(VARIANT_BOOL bValue1, bool bValue2)
{
	return asCppBool(bValue1) == bValue2;
}
//-------------------------------------------------------------------------------------------------
inline bool isEqual(bool bValue1, VARIANT_BOOL bValue2)
{
	return asCppBool(bValue2) == bValue1;
}
//-------------------------------------------------------------------------------------------------
inline bool isEqual(BOOL bValue1, bool bValue2)
{
	return asCppBool(bValue1) == bValue2;
}
//-------------------------------------------------------------------------------------------------
inline bool isEqual(bool bValue1, BOOL bValue2)
{
	return asCppBool(bValue2) == bValue1;
}
//-------------------------------------------------------------------------------------------------
// PROMISE: Requests windows shell to open the specified document with the application registered
//			to the filetype of the document. Throws an execption if the request fails.
// ARGS:	strFilename- The filename of the document to open
EXPORT_BaseUtils void shellOpenDocument(const string& strFilename);
//-------------------------------------------------------------------------------------------------
