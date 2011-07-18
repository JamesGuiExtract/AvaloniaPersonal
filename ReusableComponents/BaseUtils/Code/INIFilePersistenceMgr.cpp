//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	INIFilePersistenceMgr.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (Aug 2001 to present)
//
//==================================================================================================

#include "stdafx.h"
#include "INIFilePersistenceMgr.h"
#include "UCLIDException.h"
#include "cpputil.h"

#define MAX 32768

using namespace std;

//-------------------------------------------------------------------------------------------------
INIFilePersistenceMgr::INIFilePersistenceMgr(const string& strINIFileName)
:m_strINIFileName(strINIFileName)
{
}
//-------------------------------------------------------------------------------------------------
INIFilePersistenceMgr::INIFilePersistenceMgr(const INIFilePersistenceMgr& iniFilePersistenceMgr)
{
	m_strINIFileName = iniFilePersistenceMgr.m_strINIFileName;
}
//-------------------------------------------------------------------------------------------------
void INIFilePersistenceMgr::setKeyValue(const string& strFolderFullPath,
										const string& strFullKeyName, 
										const string& strKeyValue, 
										bool /*bCreateKeyIfNecessary*/)
{
	string strSectionName(getSectionName(strFolderFullPath));

	BOOL resVal = ::WritePrivateProfileString(strSectionName.c_str(), strFullKeyName.c_str(), 
											strKeyValue.c_str(), m_strINIFileName.c_str());
	if (!resVal)
	{
		DWORD iGetLastErrorCode = ::GetLastError();
		UCLIDException uclidException("ELI01093", "Can't set key value.");
		uclidException.addDebugInfo("LastErrorCode", (int)iGetLastErrorCode);
		uclidException.addDebugInfo("INI File", m_strINIFileName);
		uclidException.addDebugInfo("Key Name", strFullKeyName);
		uclidException.addDebugInfo("Key Value", strKeyValue);
		throw uclidException;
	}
}
//-------------------------------------------------------------------------------------------------
INIFilePersistenceMgr& INIFilePersistenceMgr::operator=(const INIFilePersistenceMgr& iniPersistenceMgr)
{
	m_strINIFileName = iniPersistenceMgr.m_strINIFileName;
	return *this;
}
//-------------------------------------------------------------------------------------------------
string INIFilePersistenceMgr::getKeyValue(const string& strFolderFullPath,
	const string& strFullKeyName, const string& strDefaultValue)
{
	string strSectionName(getSectionName(strFolderFullPath));
	
	// Validate the existence of ini file
	validateFileOrFolderExistence(m_strINIFileName);

	TCHAR pszKeyValue[MAX_PATH];
	//default key value is "" in case the key value is null
	::GetPrivateProfileString(strSectionName.c_str(), strFullKeyName.c_str(), strDefaultValue.c_str(), 
							pszKeyValue, sizeof(pszKeyValue), m_strINIFileName.c_str());
	
	return string(pszKeyValue);
}
//-------------------------------------------------------------------------------------------------
vector<string> INIFilePersistenceMgr::getKeyMultiStringValue(const std::string& strFolderFullPath, 
															const std::string& strFullKeyName)
{
	throw UCLIDException("ELI20380", "Method is not implemented!");
}
//-------------------------------------------------------------------------------------------------
void INIFilePersistenceMgr::deleteKey(const string& strFolderFullPath, 
									  const string& strFullKeyName, 
									  bool /*bFailIfNotExists*/)
{
	string strSectionName(getSectionName(strFolderFullPath));
	
	//set key value to null so that we can delete this key from the specified section
	BOOL resVal = ::WritePrivateProfileString(strSectionName.c_str(), strFullKeyName.c_str(), 
											NULL, m_strINIFileName.c_str());
	if (!resVal)
	{
		UCLIDException ue("ELI01094", "Can't delete the key.");
		ue.addDebugInfo("strFolderFullPath", strFolderFullPath);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void INIFilePersistenceMgr::deleteFolder(const string& strFolderFullPath, bool /*bFailIfNotExists*/)
{
	string strSectionName(getSectionName(strFolderFullPath));
	
	BOOL resVal = ::WritePrivateProfileString(strSectionName.c_str(), NULL, 
											NULL, m_strINIFileName.c_str());
	if (!resVal)
	{
		UCLIDException ue("ELI01098", "Can't delete the section.");
		ue.addDebugInfo("strSectionName", strSectionName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
bool INIFilePersistenceMgr::keyExists(const string& strFolderFullPath, const string& strFullKeyName)
{
	string strSectionName(getSectionName(strFolderFullPath));
	
	TCHAR pszKeyValue[MAX_PATH];
	const CString zDefault("*+*+*.***++++23**");
	//default key value is "" in case the key value is null
	::GetPrivateProfileString(strSectionName.c_str(), strFullKeyName.c_str(), zDefault, 
							pszKeyValue, sizeof(pszKeyValue), m_strINIFileName.c_str());

	bool bKeyExists = false;
	if (_stricmp(pszKeyValue, zDefault) != 0)
	{
		bKeyExists = true;
	}

	return bKeyExists;
}
//-------------------------------------------------------------------------------------------------
void INIFilePersistenceMgr::createKey(const string& strFolderFullPath,
									  const string& strFullKeyName, 
									  const string& strKeyValue, 
									  bool /*bFailIfAlreadyExists*/)
{
	setKeyValue(strFolderFullPath, strFullKeyName, strKeyValue);
}
//-------------------------------------------------------------------------------------------------
vector<string> INIFilePersistenceMgr::getKeysInFolder(const string& strFolderFullPath, 
													  bool /*bReturnFullPaths*/)
{
	string strSectionName(getSectionName(strFolderFullPath));
	
	TCHAR pszKeyValue[MAX];
	::GetPrivateProfileString(strSectionName.c_str(), NULL, "", 
							pszKeyValue, sizeof(pszKeyValue), m_strINIFileName.c_str());


	return stringTokenizer(pszKeyValue);

}
//-------------------------------------------------------------------------------------------------
void INIFilePersistenceMgr::renameKey(const string& strFolderFullPath, 
									  const string& strKeyToRename, 
									  const string& strNewKeyName)
{
	//if key exists, delete old key
	if (keyExists(strFolderFullPath, strKeyToRename))
	{
		string strKeyValue = getKeyValue(strFolderFullPath, strKeyToRename, "");
		deleteKey(strFolderFullPath, strKeyToRename);
		
		//create the new key 
		createKey(strFolderFullPath, strNewKeyName, strKeyValue);
	}
}
//-------------------------------------------------------------------------------------------------
string  INIFilePersistenceMgr::getSectionName(const std::string &strFolderFullPath)
{
	string strSectionName(strFolderFullPath);
	// find last separator, then get the string from there on
	unsigned int uiPos = strSectionName.find_last_of("\\");
	// if "\\" is the last charactor, remove it
	if (uiPos == strSectionName.size() - 1)
	{
		strSectionName = strSectionName.substr( 0, uiPos );

		uiPos = strSectionName.find_last_of("\\");
	}
	
	if (uiPos != string::npos)
	{
		strSectionName = strFolderFullPath.substr( uiPos + 1 );
	}

	return strSectionName;
}
//-------------------------------------------------------------------------------------------------
vector<string> INIFilePersistenceMgr::stringTokenizer(const TCHAR *zInput)
{
	//each token is delimetered by a NULL charactor, if two NULLs next to 
	//each other, means end of the string.
	//If zInput's length is zero, return an empty vector

	int i = 0;
	int j = 0;
	vector<string> vecTokens;
	vecTokens.clear();
	
	while (i < MAX)
	{
		//if the character is garbage, it means no more value can be obtained from zInput
		if (zInput[i] == -52)
		{
			break;
		}

		CString cstrTemp;

		if (zInput[i] != 0)
		{
			for (j = 0; j < 50; j++)
			{
				//add this character to the end of cstrTemp
				cstrTemp.Insert(j, zInput[i++]);
				if (zInput[i] ==0)
				{
					i++;
					break;
				}
			}
			
			vecTokens.push_back((LPCTSTR)cstrTemp);
			
			//skip all the NULL characters
			if (zInput[i] ==0)
			{
				for ( ; i < MAX; i++)
				{
					if (zInput[i] !=0)
					{
						break;
					}
				}
			}
		}
		else
		{
			i++;
		}
	}

	return vecTokens;
}
//-------------------------------------------------------------------------------------------------
// ************ N/A ************************
bool INIFilePersistenceMgr::folderExists(const string& /*strFolderFullPath*/)
{
	throw UCLIDException("ELI01179", "folderExists() is not applicable in INIFilePersistenceMgr.");
//	return false;
}
//-------------------------------------------------------------------------------------------------
// ************ N/A ************************
void INIFilePersistenceMgr::createFolder(const string& /*strFolderFullPath*/, 
										 bool /*bFailIfAlreadyExists*/)
{
	throw UCLIDException("ELI01180", "createFolder() is not applicable in INIFilePersistenceMgr.");
}
//-------------------------------------------------------------------------------------------------
// ************ N/A ************************
void INIFilePersistenceMgr::renameFolder(const string& /*strFolderCurrentFullPath*/, 
										 const string& /*strNewFolderName*/)
{
	throw UCLIDException("ELI01185", "renameFolder() is not applicable in INIFilePersistenceMgr.");
}
//-------------------------------------------------------------------------------------------------
// ************ N/A ************************
void INIFilePersistenceMgr::moveFolder(const string& /*strFolderCurrentFullPath*/, 
									   const string& /*strNewParentFolderFullPath*/)
{
	throw UCLIDException("ELI01186", "moveFolder() is not applicable in INIFilePersistenceMgr.");
}
//-------------------------------------------------------------------------------------------------
// ************ N/A ************************
vector<string> INIFilePersistenceMgr::getSubFolders(const string& /*strParentFolderFullPath*/)
{
	throw UCLIDException("ELI01868", "getSubFolders() is not applicable in INIFilePersistenceMgr.");
//	vector<string> vecSubFolders;
//	vecSubFolders.clear();
//	return vecSubFolders;
}
//-------------------------------------------------------------------------------------------------
