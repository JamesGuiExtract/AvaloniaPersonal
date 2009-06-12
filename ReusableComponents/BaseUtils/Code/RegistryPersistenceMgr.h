//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	RegistryPersistenceMgr.h
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

class EXPORT_BaseUtils RegistryPersistenceMgr : public IConfigurationSettingsPersistenceMgr
{
public:
	//==============================================================================================
	// PURPOSE:	create persistence manager for a specific registry key
	//
	RegistryPersistenceMgr(HKEY hkeyRoot, 
		const std::string &strRootWin32RegistryKeyFullPath);
	//==============================================================================================
	// PURPOSE:	
	//
	RegistryPersistenceMgr(const RegistryPersistenceMgr& registryPersistenceMgr);
	//==============================================================================================
	// PURPOSE:	
	//
	virtual ~RegistryPersistenceMgr() {};
	//==============================================================================================
	// PURPOSE:	
	//
	RegistryPersistenceMgr& operator=(const RegistryPersistenceMgr& registryPersistenceMgr);
	//==============================================================================================
	// PURPOSE: Get all sub keys' names
	// REQUIRE: 
	//
	virtual std::vector<std::string> getSubFolders(const std::string& strParentFolderFullPath);
	//==============================================================================================
	// PURPOSE:	for a given key, get all its sub keys
	//
	virtual std::vector<std::string> getKeysInFolder(const std::string& strFolderFullPath, 
													 bool bReturnFullPaths = false);
	//==============================================================================================
	// PURPOSE:	create a key under a certain section
	//
	virtual void createKey(const std::string& strFolderFullPath, 
						   const std::string& strFullKeyName, 
						   const std::string& strKeyValue,
						   bool bFailIfAlreadyExists = false);
	//==============================================================================================
	// PURPOSE: create a DWORD key under a certain section [p13 #4953]
	virtual void createKey(const std::string& strFolderFullPath,
							const std::string& strFullKeyName,
							DWORD dwKeyValue, 
							bool bFailIfAlreadyExists = false);
	//==============================================================================================
	// PURPOSE:	get a certain key value from a certain section
	//
	virtual std::string getKeyValue(const std::string& strFolderFullPath, 
									const std::string& strFullKeyName);
	//==============================================================================================
	// PURPOSE:	set the key value from under a given seciton
	// REQUIRE:	
	//
	virtual void setKeyValue(const std::string& strFolderFullPath,
							 const std::string& strFullKeyName, 
							 const std::string& strKeyValue, 
							 bool bCreateKeyIfNecessary = true);
	//==============================================================================================
	// PURPOSE:	set the DWORD key value from under a given seciton [p13 #4953]
	// REQUIRE:	
	//
	virtual void setKeyValue(const std::string& strFolderFullPath,
							 const std::string& strFullKeyName, 
							 DWORD dwKeyValue, 
							 bool bCreateKeyIfNecessary = true);
	//==============================================================================================
	// PURPOSE:	get the multiple string key value( REG_MULTI_SZ )
	// REQUIRE:	
	//
	virtual std::vector<std::string> getKeyMultiStringValue(const std::string& strFolderFullPath, 
															const std::string& strFullKeyName);
	//==============================================================================================
	// PURPOSE:	rename the key under a given folder
	// REQUIRE:	delete existing key, create the key with new name, and assign a default value to it
	//
	virtual void renameKey(const std::string& strFolderFullPath, 
						   const std::string& strKeyToRename, 
						   const std::string& strNewKeyName);
	//==============================================================================================
	// PURPOSE:	if the key under a given section exists
	//
	virtual bool keyExists(const std::string& strFolderFullPath, 
						   const std::string& strFullKeyName);
	//==============================================================================================
	// PURPOSE:	delete whole folder including all keys in it
	//
	virtual void deleteFolder(const std::string& strFolderFullPath, 
							  bool bFailIfNotExists = false);
	//==============================================================================================
	// PURPOSE:	delete the key under a given section
	//
	virtual void deleteKey(const std::string& strFolderFullPath, 
						   const std::string& strFullKeyName, 
						   bool bFailIfNotExists = false);
	//==============================================================================================
	// PURPOSE:	Create the specified folder
	//
	virtual void createFolder(const std::string& strFolderFullPath, 
							  bool bFailIfAlreadyExists = false);
	//==============================================================================================
	// PURPOSE:	Rename the specified folder to strNewFolderName
	//
	virtual void renameFolder(const std::string& strFolderCurrentFullPath, 
							  const std::string& strNewFolderName);
	//==============================================================================================
	// PURPOSE:	Move strFolderCurrentFullPath folder under strNewParentFolderFullPath
	//
	virtual void moveFolder(const std::string& strFolderCurrentFullPath, 
							const std::string& strNewParentFolderFullPath);
	//==============================================================================================
	// PURPOSE:	If the folder exists under strFolderFullPath
	//
	virtual bool folderExists(const std::string& strFolderFullPath);

private:
	// root folder which doesn't include HKEY_LOCAL_MACHINE or HKEY_CURRENT_USER
	// if the full path is HKEY_LOCAL_MACHINE\Software\UCLID Software\IcoMap\Configuration
	// m_strRootKeyFullPath = Software\UCLID Software\IcoMap\Configuration 
	// notice that no slash ("\") should be placed in front of "Software"
	std::string m_strRootKeyFullPath;

	// the root persistence store location (such as HKEY_LOCAL_MACHINE)
	HKEY m_hkeyRoot;
};
