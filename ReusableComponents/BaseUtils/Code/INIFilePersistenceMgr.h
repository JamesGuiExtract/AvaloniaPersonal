//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	INIFilePersistenceMgr.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (Aug 2001 to present)
//
//==================================================================================================

#pragma once

#include "IConfigurationSettingsPersistenceMgr.h"

class EXPORT_BaseUtils INIFilePersistenceMgr : public IConfigurationSettingsPersistenceMgr
{
public:
	//==============================================================================================
	// PURPOSE:	create ini file persistence manager for a specific ini file
	//
	INIFilePersistenceMgr(const std::string& strINIFileName);
	//==============================================================================================
	// PURPOSE:	
	//
	INIFilePersistenceMgr(const INIFilePersistenceMgr& iniFilePersistenceMgr);
	//==============================================================================================
	// PURPOSE:	
	//
	virtual ~INIFilePersistenceMgr() {};
	//==============================================================================================
	// PURPOSE:	
	//
	INIFilePersistenceMgr& operator=(const INIFilePersistenceMgr& iniPersistenceMgr);
	//==============================================================================================
	// PURPOSE:	TODO: If necessary, implement this function!!!
	//
	virtual void setPersistenceStoreLocation(bool /*bPerMachine*/){};
	//==============================================================================================
	// PURPOSE:	For a given folder, get all its subfolders
	//
	virtual std::vector<std::string> getSubFolders(const std::string& strParentFolderFullPath);
	//==============================================================================================
	// PURPOSE:	for a given folder, get all ites keys
	//
	virtual std::vector<std::string> getKeysInFolder(const std::string& strFolderFullPath, 
													 bool bReturnFullPaths = false);
	//==============================================================================================
	// PURPOSE:	create a folder
	//
	virtual void createFolder(const std::string& strFolderFullPath, 
							  bool bFailIfAlreadyExists = false);
	//==============================================================================================
	// PURPOSE:	create a key under a certain folder
	//
	virtual void createKey(const std::string& strFolderFullPath, 
						   const std::string& strFullKeyName, 
						   const std::string& strKeyValue, 
						   bool bFailIfAlreadyExists = false);
	//==============================================================================================
	// PURPOSE:	get a certain key value from a certain folder
	//
	virtual std::string getKeyValue(const std::string& strFolderFullPath, 
									const std::string& strFullKeyName,
									const std::string& strDefaultValue);
	//==============================================================================================
	// PURPOSE:	get the key value from under a given folder
	// REQUIRE:	
	//
	virtual void setKeyValue(const std::string& strFolderFullPath,
							 const std::string& strFullKeyName, 
							 const std::string& strKeyValue, 
							 bool bCreateKeyIfNecessary = true);
	//==============================================================================================
	// PURPOSE:	get the multiple string key value( REG_MULTI_SZ )
	// REQUIRE:	
	// NOTE: This has not been implemented and will always throw an exception
	virtual std::vector<std::string> getKeyMultiStringValue(const std::string& strFolderFullPath, 
		const std::string& strFullKeyName);
	//==============================================================================================
	// PURPOSE:	rename the key under a given folder
	// REQUIRE:	
	//
	virtual void renameKey(const std::string& strFolderFullPath, 
						   const std::string& strKeyToRename, 
						   const std::string& strNewKeyName);
	//==============================================================================================
	// PURPOSE:	rename the folder
	//
	virtual void renameFolder(const std::string& strFolderCurrentFullPath, 
							  const std::string& strNewFolderName);
	//==============================================================================================
	// PURPOSE:	move the folder under a new parent folder
	//
	virtual void moveFolder(const std::string& strFolderCurrentFullPath, 
							const std::string& strNewParentFolderFullPath);
	//==============================================================================================
	// PURPOSE:	if the folder exists
	//
	virtual bool folderExists(const std::string& strFolderFullPath);
	//==============================================================================================
	// PURPOSE:	if the key under a given folder exists
	//
	virtual bool keyExists(const std::string& strFolderFullPath, 
						   const std::string& strFullKeyName);
	//==============================================================================================
	// PURPOSE:	delete the folder
	//
	virtual void deleteFolder(const std::string& strFolderFullPath, 
							  bool bFailIfNotExists = false);
	//==============================================================================================
	// PURPOSE:	delete the key under a given folder
	//
	virtual void deleteKey(const std::string& strFolderFullPath, 
						   const std::string& strFullKeyName, 
						   bool bFailIfNotExists = false);
protected:
	//==============================================================================================
	// PURPOSE: parse the pass-in string to get section and key names
	//
	std::string getSectionName(const std::string &strFolderFullPath);

	//==============================================================================================
	// PURPOSE: parse input string with a delimeter as '\0' (NULL) and put tokens into a vector
	// REQUIRE: This function is only used to parse string of characters retrieved 
	//			from ::GetPrivateProfileString() 
	//
	std::vector<std::string> stringTokenizer(const TCHAR *zInput);

private:
	std::string m_strINIFileName;
};
