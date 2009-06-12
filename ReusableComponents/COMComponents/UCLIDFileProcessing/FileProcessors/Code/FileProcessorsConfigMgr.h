#pragma once

#include <string>
#include <vector>

class IConfigurationSettingsPersistenceMgr;

class FileProcessorsConfigMgr
{
public:
	FileProcessorsConfigMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName);

	// get and set last opened source file name from property page
	std::string getLastOpenedSourceNameFromScope();
	void setLastOpenedSourceNameFromScope(const std::string& strSourceName);

	// get and set opened source file name history from property page
	void getOpenedSourceHistoryFromScope(std::vector<std::string>& rvecHistory);
	void setOpenedSourceHistoryFromScope(const std::vector<std::string>& vecHistory);

	// get and set last opened destination file name from property page
	std::string getLastOpenedDestNameFromScope();
	void setLastOpenedDestNameFromScope(const std::string& strDestName);

	// get and set opened destination file name history from property page
	void getOpenedDestHistoryFromScope(std::vector<std::string>& rvecHistory);
	void setOpenedDestHistoryFromScope(const std::vector<std::string>& vecHistory);

	// The registry path for the file processors
	static const std::string FP_REGISTRY_PATH;

private:
	IConfigurationSettingsPersistenceMgr* m_pCfgMgr;

	std::string m_strCopyMoveDeleteFolderName;
};
