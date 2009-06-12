#pragma once

#include <string>
#include <vector>

class IConfigurationSettingsPersistenceMgr;

class FileSupplierConfigMgr
{
public:
	FileSupplierConfigMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName);

	// get and set last opened folder name from property page
	std::string getLastOpenedFolderNameFromScope();
	void setLastOpenedFolderNameFromScope(const std::string& strFolderName);

	// get and set opened folder history from property page
	void getOpenedFolderHistoryFromScope(std::vector<std::string>& rvecHistory);
	void setOpenedFolderHistoryFromScope(const std::vector<std::string>& vecHistory);

	// get last used file extension
	std::string getLastUsedFileExtension();
	void setLastUsedFileExtension(const std::string& strExt);

	// get and set file extensions
	std::vector<std::string> getFileExtensionList();
	void setFileExtensionList(const std::vector<std::string>& vecFileExtensionList);

private:
	IConfigurationSettingsPersistenceMgr* m_pCfgMgr;

	std::string m_strSectionFolderName;

};
