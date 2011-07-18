//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IConfigurationSettingsPersistenceMgr.h
//
// PURPOSE:	To be used for persistence settings
//
// NOTES:	
//
// AUTHORS:	Duan Wang
//
//==================================================================================================
#pragma once

#include "BaseUtils.h"

#include <vector>
#include <string>

const std::string ROOT = "\\";

class EXPORT_BaseUtils IConfigurationSettingsPersistenceMgr
{
public:
	virtual ~IConfigurationSettingsPersistenceMgr() {};
	//==============================================================================================
	// PURPOSE:	For a given folder, get all its subfolders
	//
	virtual std::vector<std::string> getSubFolders(const std::string& strParentFolderFullPath) = 0;
	//==============================================================================================
	// PURPOSE:	for a given folder, get all ites keys
	//
	virtual std::vector<std::string> getKeysInFolder(const std::string& strFolderFullPath, 
													 bool bReturnFullPaths = false) = 0;
	//==============================================================================================
	// PURPOSE:	create a folder
	//
	virtual void createFolder(const std::string& strFolderFullPath, 
							  bool bFailIfAlreadyExists = false) = 0;
	//==============================================================================================
	// PURPOSE:	create a key under a certain folder
	//
	virtual void createKey(const std::string& strFolderFullPath, 
						   const std::string& strFullKeyName, 
						   const std::string& strKeyValue, 
						   bool bFailIfAlreadyExists = false) = 0;
	//==============================================================================================
	// PURPOSE:	get a certain key value from a certain folder
	//
	virtual std::string getKeyValue(const std::string& strFolderFullPath, 
									const std::string& strFullKeyName,
									const std::string& strDefaultValue) = 0;
	//==============================================================================================
	// PURPOSE:	get the key value from under a given folder
	// REQUIRE:	
	//
	virtual void setKeyValue(const std::string& strFolderFullPath,
							 const std::string& strFullKeyName, 
							 const std::string& strKeyValue, 
							 bool bCreateKeyIfNecessary = true) = 0;
	//==============================================================================================
	// PURPOSE:	get the multiple string key value( REG_MULTI_SZ )
	// REQUIRE:	
	//
	virtual std::vector<std::string> getKeyMultiStringValue(const std::string& strFolderFullPath, 
									const std::string& strFullKeyName) = 0;
	//==============================================================================================
	// PURPOSE:	rename the key under a given folder
	// REQUIRE:	
	//
	virtual void renameKey(const std::string& strFolderFullPath, 
						   const std::string& strKeyToRename, 
						   const std::string& strNewKeyName) = 0;
	//==============================================================================================
	// PURPOSE:	rename the folder
	//
	virtual void renameFolder(const std::string& strFolderCurrentFullPath, 
							  const std::string& strNewFolderName) = 0;
	//==============================================================================================
	// PURPOSE:	move the folder under a new parent folder
	//
	virtual void moveFolder(const std::string& strFolderCurrentFullPath, 
							const std::string& strNewParentFolderFullPath) = 0;
	//==============================================================================================
	// PURPOSE:	if the folder exists
	//
	virtual bool folderExists(const std::string& strFolderFullPath) = 0;
	//==============================================================================================
	// PURPOSE:	if the key under a given folder exists
	//
	virtual bool keyExists(const std::string& strFolderFullPath, 
						   const std::string& strFullKeyName) = 0;
	//==============================================================================================
	// PURPOSE:	delete the folder
	//
	virtual void deleteFolder(const std::string& strFolderFullPath, 
							  bool bFailIfNotExists = false) = 0;
	//==============================================================================================
	// PURPOSE:	delete the key under a given folder
	//
	virtual void deleteKey(const std::string& strFolderFullPath, 
						   const std::string& strFullKeyName, 
						   bool bFailIfNotExists = false) = 0;
	//==============================================================================================
	// PURPOSE:	export the whole hierachy of a given folder to ...
	//
	void exportFolder(const std::string& /*strFolderFullPath*/, 
		bool /*bRecursive*/, const std::string& /*strExportFileUNCPath*/)
	{
		//TODO: implementation
	};
	//==============================================================================================
	// PURPOSE:	import a whole hierachy of a given folder from ...
	//
	void import(const std::string& /*strImportFileUNCPath*/)
	{
		//TODO: implementation
	};

};
