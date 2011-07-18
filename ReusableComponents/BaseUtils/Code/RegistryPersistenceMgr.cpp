//=================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	RegistryPersistenceMgr.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (Aug 2001 to present)
//
//=================================================================================================

#include "stdafx.h"
#include "RegistryPersistenceMgr.h"
#include "UCLIDException.h"

using namespace std;

//-------------------------------------------------------------------------------------------------
RegistryPersistenceMgr::RegistryPersistenceMgr(HKEY hkeyRoot, const std::string &strRootWin32RegistryKeyFullPath)
:m_strRootKeyFullPath(strRootWin32RegistryKeyFullPath), m_hkeyRoot(hkeyRoot)
{
}
//-------------------------------------------------------------------------------------------------
RegistryPersistenceMgr::RegistryPersistenceMgr(const RegistryPersistenceMgr& registryPersistenceMgr)
{
	m_strRootKeyFullPath = registryPersistenceMgr.m_strRootKeyFullPath;
	m_hkeyRoot = registryPersistenceMgr.m_hkeyRoot;
}
//-------------------------------------------------------------------------------------------------
RegistryPersistenceMgr& RegistryPersistenceMgr::operator=(const RegistryPersistenceMgr& registryPersistenceMgr)
{
	m_strRootKeyFullPath = registryPersistenceMgr.m_strRootKeyFullPath;
	m_hkeyRoot = registryPersistenceMgr.m_hkeyRoot;

	return *this;
}
//-------------------------------------------------------------------------------------------------
void RegistryPersistenceMgr::setKeyValue(const string& strFolderFullPath,
										 const string& strFullKeyName, 
										 const string& strKeyValue, 
										 bool bCreateKeyIfNecessary)
{
	if (strKeyValue.empty())
	{
		// delete the key value
		deleteKey(strFolderFullPath, strFullKeyName);
	}
	else
	{
		HKEY hKey;
		string strKey = m_strRootKeyFullPath + strFolderFullPath;
		// open the key first 
		LONG ret = ::RegOpenKeyEx(m_hkeyRoot,       // handle to open key
			TEXT(strKey.c_str()),		// key name
			0,						// reserved
			KEY_SET_VALUE,			// security access mask
			&hKey);				 // handle to open key
		
		DWORD size = strKeyValue.size();
		// key doesn't exist
		if (ret != ERROR_SUCCESS && bCreateKeyIfNecessary)
		{
			::RegCloseKey(hKey);
			createKey(strFolderFullPath, strFullKeyName, strKeyValue);
		}
		else
		{
			// assign the value to the key
			ret = ::RegSetValueEx(hKey,		// handle to key
				TEXT(strFullKeyName.c_str()),	// value name
				0,						// reserved
				REG_SZ,					// value type
				(LPBYTE)strKeyValue.c_str(),			// value data
				size);		// size of value data
			
			if (ret != ERROR_SUCCESS)
			{
				::RegCloseKey(hKey);
				
				UCLIDException uclidException("ELI01832", "Failed to assign value to a registry key.");
				uclidException.addDebugInfo("ErrorCode", ret);
				uclidException.addDebugInfo("Key Path", strKey);
				uclidException.addDebugInfo("Key name", strFullKeyName);
				uclidException.addDebugInfo("Key value", strKeyValue);
				uclidException.addDebugInfo("ErrorMessage", getWindowsErrorString(ret));
				throw uclidException;
			}
			
			::RegCloseKey(hKey);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void RegistryPersistenceMgr::setKeyValue(const string& strFolderFullPath,
										 const string& strFullKeyName, 
										 DWORD dwKeyValue,
										 bool bCreateKeyIfNecessary)
{
	HKEY hKey;
	string strKey = m_strRootKeyFullPath + strFolderFullPath;
	// open the key first 
	LONG ret = ::RegOpenKeyEx(m_hkeyRoot,       // handle to open key
		TEXT(strKey.c_str()),		// key name
		0,						// reserved
		KEY_SET_VALUE,			// security access mask
		&hKey);				 // handle to open key
	
	DWORD size = sizeof(dwKeyValue);

	// key doesn't exist
	if (ret != ERROR_SUCCESS && bCreateKeyIfNecessary)
	{
		::RegCloseKey(hKey);
		createKey(strFolderFullPath, strFullKeyName, dwKeyValue);
	}
	else
	{
		// assign the value to the key
		ret = ::RegSetValueEx(hKey,		// handle to key
			TEXT(strFullKeyName.c_str()),	// value name
			0,						// reserved
			REG_DWORD,					// value type
			(LPBYTE)&dwKeyValue,
			size);		// size of value data
		
		if (ret != ERROR_SUCCESS)
		{
			::RegCloseKey(hKey);
			
			UCLIDException uclidException("ELI20809", "Failed to assign value to a registry key.");
			uclidException.addDebugInfo("ErrorCode", ret);
			uclidException.addDebugInfo("Key Path", strKey);
			uclidException.addDebugInfo("Key name", strFullKeyName);
			uclidException.addDebugInfo("Key value", asString(dwKeyValue));
			uclidException.addDebugInfo("ErrorMessage", getWindowsErrorString(ret));
			throw uclidException;
		}
		
		::RegCloseKey(hKey);
	}
}
//-------------------------------------------------------------------------------------------------
string RegistryPersistenceMgr::getKeyValue(const string& strFolderFullPath,
	const string& strFullKeyName, const std::string& strDefaultValue)
{
	HKEY hKey;
	string strKey = m_strRootKeyFullPath + strFolderFullPath;
	// open the key first 
	LONG ret = ::RegOpenKeyEx(m_hkeyRoot,         // handle to open key
							  TEXT(strKey.c_str()),					  // subkey name
							  0,							// reserved
							  KEY_QUERY_VALUE,					// security access mask
							  &hKey);				 // handle to open key

	// set max length to be 500
	TCHAR szValue[500];
	DWORD dwBufLen = 500;

	// if key doesn't exist, return the default value
	if (ret != ERROR_SUCCESS)
	{
		::RegCloseKey(hKey);
		return strDefaultValue;
	}
	
	ret = ::RegQueryValueEx(hKey,
							TEXT(strFullKeyName.c_str()),
							NULL,
							NULL,
							(LPBYTE)szValue,
							&dwBufLen);
	
	if (ret != ERROR_SUCCESS)
	{
		::RegCloseKey(hKey);

		// If the reason the query failed is that the registry value doesn't exist, return the
		// default value.
		if (ret == ERROR_FILE_NOT_FOUND)
		{
			return strDefaultValue;
		}

		UCLIDException uclidException("ELI01837", "Failed to get the value of a registry key.");
		uclidException.addDebugInfo("ErrorCode", ret);
		uclidException.addDebugInfo("Key Path", strKey);
		uclidException.addDebugInfo("Full Key Name", strFullKeyName);
		uclidException.addDebugInfo("ErrorMessage", getWindowsErrorString(ret));
		throw uclidException;
	}

	// Release the key
	::RegCloseKey(hKey);

	// Check the returned length
	if (dwBufLen == 0)
	{
		// Just return an empty string
		return "";
	}
	else
	{
		// Copy the data buffer to a string and return it
		CString cstrKeyValue(szValue);

		return (string)cstrKeyValue;
	}
}
//-------------------------------------------------------------------------------------------------
vector<string> RegistryPersistenceMgr::getKeyMultiStringValue(const std::string& strFolderFullPath, 
															const std::string& strFullKeyName)
{
	HKEY hKey;
	string strKey = m_strRootKeyFullPath + strFolderFullPath;
	// open the key first 
	LONG ret = ::RegOpenKeyEx(m_hkeyRoot,         // handle to open key
							  TEXT(strKey.c_str()),					  // subkey name
							  0,							// reserved
							  KEY_QUERY_VALUE,					// security access mask
							  &hKey);				 // handle to open key

	// set max length to be 500
	TCHAR szValue[500];
	DWORD dwBufLen = 500;

	// Initialize the return vector
	vector<string> vecStrings;

	// if key doesn't exist, return empty
	if (ret != ERROR_SUCCESS)
	{
		::RegCloseKey(hKey);
		return vecStrings;
	}
	
	ret = ::RegQueryValueEx(hKey,
							TEXT(strFullKeyName.c_str()),
							NULL,
							NULL,
							(LPBYTE)szValue,
							&dwBufLen);
	
	if (ret != ERROR_SUCCESS)
	{
		::RegCloseKey(hKey);

		// If the reason the query failed is that the registry value doesn't exist, return empty.
		if (ret == ERROR_FILE_NOT_FOUND)
		{
			return vecStrings;
		}

		UCLIDException uclidException("ELI20387", "Failed to get the value of a registry key.");
		uclidException.addDebugInfo("ErrorCode", ret);
		uclidException.addDebugInfo("Key Path", strKey);
		uclidException.addDebugInfo("Full Key Name", strFullKeyName);
		throw uclidException;
	}

	// Release the key
	::RegCloseKey(hKey);

	// Check the returned length
	if (dwBufLen == 0)
	{
		// Just return an empty string
		return vecStrings;
	}

	// Extract each string - each string ends with a null value 
	TCHAR *szCurr = szValue;
	int iStrLength = strlen(szCurr);
	int iEndCurrPos = iStrLength + 1;

	// The last string will have a zero length
	while ( iStrLength != 0 )
	{
		// save the current string
		vecStrings.push_back(szCurr);

		// Set current to the beginning of the next string
		szCurr = (TCHAR *)&szValue[iEndCurrPos];

		// Get the length of the current string
		iStrLength = strlen(szCurr);
		iEndCurrPos += iStrLength + 1;
	}

	// Return the vector of strings
	return vecStrings;
}
//-------------------------------------------------------------------------------------------------
void RegistryPersistenceMgr::deleteKey(const string& strFolderFullPath, 
									   const string& strFullKeyName, 
									   bool bFailIfNotExists)
{
	string strKey = m_strRootKeyFullPath + strFolderFullPath;
	// open the key first
	HKEY hKey; 
	LONG ret = ::RegOpenKeyEx(m_hkeyRoot,         // handle to open key
							  TEXT(strKey.c_str()),			// key name
							  0,							// reserved
							  KEY_WRITE,					// security access mask
							  &hKey);				 // handle to open key
	
	if (ret == ERROR_SUCCESS)
	{
		LONG ret = ::RegDeleteValue(hKey,         // handle to open key
									TEXT(strFullKeyName.c_str()));  // subkey name
		
		
		if (ret != ERROR_SUCCESS && bFailIfNotExists)
		{
			::RegCloseKey(hKey);
			
			UCLIDException uclidException("ELI01830", "Failed to delete a registry key value.");
			uclidException.addDebugInfo("ErrorCode", ret);
			uclidException.addDebugInfo("Key Path", strKey);
			uclidException.addDebugInfo("Full Key Name", strFullKeyName);
			uclidException.addDebugInfo("ErrorMessage", getWindowsErrorString(ret));
			throw uclidException;
		}
	}
	
	::RegCloseKey(hKey);
}
//-------------------------------------------------------------------------------------------------
void RegistryPersistenceMgr::deleteFolder(const string& strFolderFullPath, 
										  bool bFailIfNotExists)
{
	// if the folder does not exist, and bFailIfNotExists is false, then just
	// return
	if (!folderExists(strFolderFullPath))
	{
		if (bFailIfNotExists)
		{
			UCLIDException ue("ELI05090", "Specified registry key does not exist!");
			ue.addDebugInfo("m_strRootKeyFullPath", m_strRootKeyFullPath);
			ue.addDebugInfo("strRegFolder", strFolderFullPath);
			throw ue;
		}

		return;
	}

	// delete all the keys in this folder
	vector<string> vecKeys = getKeysInFolder(strFolderFullPath);
	vector<string>::const_iterator iter;
	for (iter = vecKeys.begin(); iter != vecKeys.end(); iter++)
	{
		deleteKey(strFolderFullPath, *iter, bFailIfNotExists);
	}

	// delete all folders underneath this folder
	vector<string> vecSubFolders = getSubFolders(strFolderFullPath);
	for (iter = vecSubFolders.begin(); iter != vecSubFolders.end(); iter++)
	{
		string strSubFolderFullPath = strFolderFullPath + string("\\") + *iter;
		deleteFolder(strSubFolderFullPath, bFailIfNotExists);
	}

	// delete this folder itself
	string strRegFolder = m_strRootKeyFullPath + strFolderFullPath;
	LONG nResult = RegDeleteKey(m_hkeyRoot, strRegFolder.c_str());
	if (nResult != ERROR_SUCCESS)
	{
		UCLIDException ue("ELI01867", "Failed to delete a registry key.");
		ue.addDebugInfo("Key Path", strRegFolder);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
bool RegistryPersistenceMgr::keyExists(const string& strFolderFullPath, const string& strFullKeyName)
{

	HKEY hKey;
	string strKey = m_strRootKeyFullPath + strFolderFullPath;

	LONG ret = ::RegOpenKeyEx(m_hkeyRoot,         // handle to open key
							  TEXT(strKey.c_str()),			// subkey name
							  0,							// reserved
							  KEY_READ,					// security access mask
							  &hKey);				 // handle to open key
	// failed to open this key or it's not there
	if (ret != ERROR_SUCCESS)
	{
		::RegCloseKey(hKey);
		return false;
	}

	TCHAR szValue[500];
	DWORD dwBufLen = 500;

	ret = ::RegQueryValueEx(hKey,
							TEXT(strFullKeyName.c_str()),
							NULL,
							NULL,
							(LPBYTE)szValue,
							&dwBufLen);
	
	if (ret != ERROR_SUCCESS)
	{
		::RegCloseKey(hKey);
		return false;
	}

	::RegCloseKey(hKey);

	return true;
}
//-------------------------------------------------------------------------------------------------
void RegistryPersistenceMgr::createKey(const string& strFolderFullPath, 
									   const string& strFullKeyName, 
									   const string& strKeyValue,
									   bool /*bFailIfAlreadyExists*/)
{
	string strKey = m_strRootKeyFullPath + strFolderFullPath;
	// create the key if it's not there
	HKEY hKey;
	DWORD disposition = 0;
	LONG ret = ::RegCreateKeyEx(m_hkeyRoot, 
								TEXT(strKey.c_str()),
								0,
								NULL,
								REG_OPTION_NON_VOLATILE,
								KEY_ALL_ACCESS,
								NULL,
								&hKey,
								&disposition);

	// if failed..
	if (ret != ERROR_SUCCESS)
	{
		::RegCloseKey(hKey);

		// To prevent our software from behaving badly when running with limited permissiongs,
		// ignore ERROR_ACCESS_DENIED when creating a key for the first time if not in the current
		// user hive.
		if (ret == ERROR_ACCESS_DENIED && m_hkeyRoot != HKEY_CURRENT_USER)
		{
			return;
		}
		else
		{
			UCLIDException uclidException("ELI01816", "Failed to create a registry key.");
			uclidException.addDebugInfo("ErrorCode", ret);
			uclidException.addDebugInfo("Key Path", strKey);
			uclidException.addDebugInfo("Full Key Name", strFullKeyName);
			uclidException.addDebugInfo("ErrorMessage", getWindowsErrorString(ret));
			throw uclidException;
		}
	}

	DWORD size = strKeyValue.size();
	// assign the value to the key
	ret = ::RegSetValueEx(hKey,	// handle to key
						  TEXT(strFullKeyName.c_str()),	// value name
						  0,						// reserved
						  REG_SZ,					// value type
						  (LPBYTE)strKeyValue.c_str(),			// value data
						  size);		// size of value data

	if (ret != ERROR_SUCCESS)
	{
		::RegCloseKey(hKey);

		UCLIDException uclidException("ELI01827", "Failed to assign value to a registry key.");
		uclidException.addDebugInfo("ErrorCode", ret);
		uclidException.addDebugInfo("Key Path", strKey);
		uclidException.addDebugInfo("Key name", strFullKeyName);
		uclidException.addDebugInfo("Key value", strKeyValue);
		uclidException.addDebugInfo("ErrorMessage", getWindowsErrorString(ret));
		throw uclidException;
	}

	::RegCloseKey(hKey);
}
//-------------------------------------------------------------------------------------------------
void RegistryPersistenceMgr::createKey(const string& strFolderFullPath,
									   const string& strFullKeyName,
									   DWORD dwKeyValue,
									   bool /*bFailIfAlreadyExists*/)
{
	string strKey = m_strRootKeyFullPath + strFolderFullPath;

	// create the key if it's not there
	HKEY hKey;
	DWORD disposition = 0;
	LONG ret = ::RegCreateKeyEx(m_hkeyRoot, 
								TEXT(strKey.c_str()),
								0,
								NULL,
								REG_OPTION_NON_VOLATILE,
								KEY_ALL_ACCESS,
								NULL,
								&hKey,
								&disposition);

	// if failed..
	if (ret != ERROR_SUCCESS)
	{
		::RegCloseKey(hKey);

		// To prevent our software from behaving badly when running with limited permissiongs,
		// ignore ERROR_ACCESS_DENIED when creating a key for the first time if not in the current
		// user hive.
		if (ret == ERROR_ACCESS_DENIED && m_hkeyRoot != HKEY_CURRENT_USER)
		{
			return;
		}
		else
		{
			UCLIDException uclidException("ELI20728", "Failed to create a registry key.");
			uclidException.addDebugInfo("ErrorCode", ret);
			uclidException.addDebugInfo("Key Path", strKey);
			uclidException.addDebugInfo("Key name", strFullKeyName);
			uclidException.addDebugInfo("ErrorMessage", getWindowsErrorString(ret));
			throw uclidException;
		}
	}

	DWORD size = sizeof(dwKeyValue);
	// assign the value to the key
	ret = ::RegSetValueEx(hKey,	// handle to key
						  TEXT(strFullKeyName.c_str()),	// value name
						  0,						// reserved
						  REG_DWORD,				// value type
						  (LPBYTE)&dwKeyValue,		// value data
						  size);		// size of value data

	if (ret != ERROR_SUCCESS)
	{
		::RegCloseKey(hKey);

		UCLIDException uclidException("ELI20729", "Failed to assign value to a registry key.");
		uclidException.addDebugInfo("ErrorCode", ret);
		uclidException.addDebugInfo("Key Path", strKey);
		uclidException.addDebugInfo("Key name", strFullKeyName);
		uclidException.addDebugInfo("Key value", dwKeyValue);
		uclidException.addDebugInfo("ErrorMessage", getWindowsErrorString(ret));
		throw uclidException;
	}

	::RegCloseKey(hKey);
}
//-------------------------------------------------------------------------------------------------
vector<string> RegistryPersistenceMgr::getKeysInFolder(const string& strFolderFullPath, 
													  bool /*bReturnFullPaths*/)
{
	vector<string> vecKeys;
	HKEY hKey;
	string strKey = m_strRootKeyFullPath + strFolderFullPath;

	LONG ret = ::RegOpenKeyEx(m_hkeyRoot,         // handle to open key
							  TEXT(strKey.c_str()),			// subkey name
							  0,							// reserved
							  KEY_READ,					// security access mask
							  &hKey);				 // handle to open key
	// failed to open this key or it's not there
	if (ret != ERROR_SUCCESS)
	{
		vecKeys.clear();
		return vecKeys;
	}

	char szValueName[256];
	DWORD bufsize = sizeof(szValueName);

	DWORD index = 0;
	ret = 0;
	LPDWORD lpdword = NULL;
	ret = ::RegEnumValue(hKey, 
						index,     
						szValueName,	
						&bufsize,  
						NULL,        
						lpdword,             
						NULL,
						NULL);         
	
	while (ret == ERROR_SUCCESS && ret != ERROR_NO_MORE_ITEMS)
	{
		vecKeys.push_back(static_cast<string>(szValueName));

		index = index + 1;
		bufsize = sizeof(szValueName);
		ret = ::RegEnumValue(hKey, 
							index,     
							szValueName,	
							&bufsize,  
							NULL,        
							lpdword,             
							NULL,
							NULL);         
	} 


	::RegCloseKey(hKey);

	return vecKeys;
}
//-------------------------------------------------------------------------------------------------
void RegistryPersistenceMgr::renameKey(const string& /*strFolderFullPath*/, 
									  const string& /*strKeyToRename*/, 
									  const string& /*strNewKeyName*/)
{
	throw UCLIDException("ELI01836", "renameKey is not implemented yet.");
}
//-------------------------------------------------------------------------------------------------
vector<string> RegistryPersistenceMgr::getSubFolders(const string& strParentFolderFullPath)
{
	vector<string> vecSubFolders;

	HKEY hKey;
	// open the folder first
	string strParentFolder = m_strRootKeyFullPath + strParentFolderFullPath;
	LONG ret = ::RegOpenKeyEx(m_hkeyRoot,         // handle to open key
							  TEXT(strParentFolder.c_str()),					  // subkey name
							  0,							// reserved
							  KEY_READ,					// security access mask
							  &hKey);				 // handle to open key

	// failed to open this key or it's not there
	if (ret != ERROR_SUCCESS)
	{
		vecSubFolders.clear();
		return vecSubFolders;
	}

	char szSubKeyName[256];
	DWORD bufsize = sizeof(szSubKeyName);
	PFILETIME lpftLastWriteTime = NULL;

	DWORD index = 0;
	ret = 0;
	
	ret = ::RegEnumKeyEx(hKey,  // handle to key to enumerate
						index,              // subkey index
						szSubKeyName,		// subkey name
						&bufsize,            // size of subkey buffer
						NULL,         // reserved
						NULL,             // class string buffer
						NULL,           // size of class string buffer
						lpftLastWriteTime); // last write time
	
	while (ret == ERROR_SUCCESS && ret != ERROR_NO_MORE_ITEMS)
	{
		// put the key name into the vector
		vecSubFolders.push_back(static_cast<string>(szSubKeyName));

		index = index + 1;
		bufsize = sizeof(szSubKeyName);
		ret = ::RegEnumKeyEx(hKey,  // handle to key to enumerate
							 index,              // subkey index
							 szSubKeyName,		// subkey name
							 &bufsize,            // size of subkey buffer
							 NULL,         // reserved
							 NULL,             // class string buffer
							 NULL,           // size of class string buffer
							 lpftLastWriteTime); // last write time
	} 

	::RegCloseKey(hKey);

	return vecSubFolders;
}
//-------------------------------------------------------------------------------------------------
bool RegistryPersistenceMgr::folderExists(const string& strFolderFullPath)
{
	HKEY hKey;
	string strKey = m_strRootKeyFullPath + strFolderFullPath;

	LONG ret = ::RegOpenKeyEx(m_hkeyRoot,         // handle to open key
							  TEXT(strKey.c_str()),			// key name
							  0,							// reserved
							  KEY_READ,					// security access mask
							  &hKey);				 // handle to open key

	::RegCloseKey(hKey);

	// key doesn't exist
	if (ret != ERROR_SUCCESS)
	{
		return false;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void RegistryPersistenceMgr::createFolder(const string& strFolderFullPath, 
										  bool /*bFailIfAlreadyExists*/)
{
	string strKey = m_strRootKeyFullPath + strFolderFullPath;
	// create the key if it's not there
	HKEY hKey;
	DWORD disposition = 0;
	LONG ret = ::RegCreateKeyEx(m_hkeyRoot, 
								TEXT(strKey.c_str()),
								0,
								NULL,
								REG_OPTION_NON_VOLATILE,
								KEY_ALL_ACCESS,
								NULL,
								&hKey,
								&disposition);

	// if failed..
	if (ret != ERROR_SUCCESS)
	{
		UCLIDException uclidException("ELI01833", "Failed to create a registry key.");
		uclidException.addDebugInfo("ErrorCode", ret);
		uclidException.addDebugInfo("Folder full path", strKey);
		throw uclidException;
	}
	
	::RegCloseKey(hKey);
}
//-------------------------------------------------------------------------------------------------
void RegistryPersistenceMgr::renameFolder(const string& strFolderCurrentFullPath, 
										 const string& /*strNewFolderName*/)
{
	throw UCLIDException("ELI01835", "renameFolder is not implemented yet.");
}
//-------------------------------------------------------------------------------------------------
void RegistryPersistenceMgr::moveFolder(const string& /*strFolderCurrentFullPath*/, 
									   const string& /*strNewParentFolderFullPath*/)
{
	throw UCLIDException("ELI01834", "moveFolder is not implemented yet.");
}
//-------------------------------------------------------------------------------------------------
