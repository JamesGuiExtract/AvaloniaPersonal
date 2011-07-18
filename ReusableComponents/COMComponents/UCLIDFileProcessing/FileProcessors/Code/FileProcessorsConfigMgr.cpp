#include "StdAfx.h"
#include "FileProcessorsConfigMgr.h"

#include <IConfigurationSettingsPersistenceMgr.h>
#include <StringTokenizer.h>
#include <RegConstants.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Registry keys
const string gstrLAST_OPENED_SOURCE_NAME = "LastOpenedSource";
const string gstrOPENED_SOURCE_HISTORY = "OpenedSourceHistory";
const string gstrLAST_OPENED_DEST_NAME = "LastOpenedDestination";
const string gstrOPENED_DEST_HISTORY = "OpenedDestinationHistory";

//Registry Path for FileProcessingDlg
const string FileProcessorsConfigMgr::FP_REGISTRY_PATH = gstrCOM_COMPONENTS_REG_PATH + "\\UCLIDFileProcessing\\FileProcessors";

//-------------------------------------------------------------------------------------------------
// FileProcessorsConfigMgr
//-------------------------------------------------------------------------------------------------
FileProcessorsConfigMgr::FileProcessorsConfigMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName)
:	m_pCfgMgr(pConfigMgr), 
	m_strCopyMoveDeleteFolderName(strSectionName)
{
}
//-------------------------------------------------------------------------------------------------
string FileProcessorsConfigMgr::getLastOpenedSourceNameFromScope()
{
	// Check if the registry key exists
	if (!m_pCfgMgr->keyExists(m_strCopyMoveDeleteFolderName, gstrLAST_OPENED_SOURCE_NAME))
	{
		// Return an empty string if the key does not exist
		return "";
	}

	// Return the key value from registry
	return m_pCfgMgr->getKeyValue(m_strCopyMoveDeleteFolderName, gstrLAST_OPENED_SOURCE_NAME, "");
}
//-------------------------------------------------------------------------------------------------
void FileProcessorsConfigMgr::setLastOpenedSourceNameFromScope(const string& strSourceName)
{
	// set the key value to registry
	m_pCfgMgr->setKeyValue(m_strCopyMoveDeleteFolderName, gstrLAST_OPENED_SOURCE_NAME, strSourceName);
}
//-------------------------------------------------------------------------------------------------
void FileProcessorsConfigMgr::getOpenedSourceHistoryFromScope(std::vector<std::string>& rvecHistory)
{
	// Check if the registry key exists
	string strList("");
	if (!m_pCfgMgr->keyExists(m_strCopyMoveDeleteFolderName, gstrOPENED_SOURCE_HISTORY))
	{
		// Create the registry key if not exists
		m_pCfgMgr->createKey(m_strCopyMoveDeleteFolderName, gstrOPENED_SOURCE_HISTORY, strList);
	}
	strList = m_pCfgMgr->getKeyValue(m_strCopyMoveDeleteFolderName, gstrOPENED_SOURCE_HISTORY, "");
	
	// each item in the list is separated by a pipe (|) character, parse them
	StringTokenizer::sGetTokens(strList, "|", rvecHistory);
}
//-------------------------------------------------------------------------------------------------
void FileProcessorsConfigMgr::setOpenedSourceHistoryFromScope(const std::vector<std::string>& vecHistory)
{
	// Go through each items and build the string
	string strList("");
	for (unsigned int n = 0; n < vecHistory.size(); n++)
	{
		if (n > 0)
		{
			strList += "|";
		}

		strList += vecHistory[n];
	}

	// Set the key value to registry
	m_pCfgMgr->setKeyValue(m_strCopyMoveDeleteFolderName, gstrOPENED_SOURCE_HISTORY, strList);
}
//-------------------------------------------------------------------------------------------------
string FileProcessorsConfigMgr::getLastOpenedDestNameFromScope()
{
	// Check if the registry key exists
	if (!m_pCfgMgr->keyExists(m_strCopyMoveDeleteFolderName, gstrLAST_OPENED_DEST_NAME))
	{
		// Return an empty string if the key does not exist
		return "";
	}

	// Return the key value from registry
	return m_pCfgMgr->getKeyValue(m_strCopyMoveDeleteFolderName, gstrLAST_OPENED_DEST_NAME, "");
}
//-------------------------------------------------------------------------------------------------
void FileProcessorsConfigMgr::setLastOpenedDestNameFromScope(const string& strDestName)
{
	// set the key value to registry
	m_pCfgMgr->setKeyValue(m_strCopyMoveDeleteFolderName, gstrLAST_OPENED_DEST_NAME, strDestName);
}
//-------------------------------------------------------------------------------------------------
void FileProcessorsConfigMgr::getOpenedDestHistoryFromScope(std::vector<std::string>& rvecHistory)
{
	// Check if the registry key exists
	string strList("");
	if (!m_pCfgMgr->keyExists(m_strCopyMoveDeleteFolderName, gstrOPENED_DEST_HISTORY))
	{
		// Create the registry key if not exists
		m_pCfgMgr->createKey(m_strCopyMoveDeleteFolderName, gstrOPENED_DEST_HISTORY, strList);
	}
	strList = m_pCfgMgr->getKeyValue(m_strCopyMoveDeleteFolderName, gstrOPENED_DEST_HISTORY, "");
	
	// each item in the list is separated by a pipe (|) character, parse them
	StringTokenizer::sGetTokens(strList, "|", rvecHistory);
}
//-------------------------------------------------------------------------------------------------
void FileProcessorsConfigMgr::setOpenedDestHistoryFromScope(const std::vector<std::string>& vecHistory)
{
	// Go through each items and build the string
	string strList("");
	for (unsigned int n = 0; n<vecHistory.size(); n++)
	{
		if (n > 0)
		{
			strList += "|";
		}

		strList += vecHistory[n];
	}

	// Set the key value to registry
	m_pCfgMgr->setKeyValue(m_strCopyMoveDeleteFolderName, gstrOPENED_DEST_HISTORY, strList);
}
//-------------------------------------------------------------------------------------------------