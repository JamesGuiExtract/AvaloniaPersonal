#include "StdAfx.h"
#include "FileSupplierConfigMgr.h"
#include <IConfigurationSettingsPersistenceMgr.h>
#include <StringTokenizer.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Registry keys
const string gstrLAST_OPENED_FOLDER_NAME = "LastOpenedFolder";
const string gstrOPENED_FOLDER_HISTORY = "OpenedFolderHistory";
const string gstrFILE_EXTENSION_LIST = "FileExtensionList";
const string gstrLAST_USED_FILE_EXTENSION = "LastUsedFileExtension";

const string gstrDEFAULT_LAST_USED_FILE_EXTENSION = "*.*";

//-------------------------------------------------------------------------------------------------
// FileSupplierConfigMgr
//-------------------------------------------------------------------------------------------------
FileSupplierConfigMgr::FileSupplierConfigMgr(IConfigurationSettingsPersistenceMgr* pConfigMgr, const std::string& strSectionName)
:	m_pCfgMgr(pConfigMgr), 
	m_strSectionFolderName(strSectionName)
{
	
}
//-------------------------------------------------------------------------------------------------
string FileSupplierConfigMgr::getLastOpenedFolderNameFromScope()
{
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, gstrLAST_OPENED_FOLDER_NAME))
	{
		return "";
	}

	return m_pCfgMgr->getKeyValue(m_strSectionFolderName, gstrLAST_OPENED_FOLDER_NAME, "");
}
//-------------------------------------------------------------------------------------------------
void FileSupplierConfigMgr::setLastOpenedFolderNameFromScope(const string& strFolderName)
{
	m_pCfgMgr->setKeyValue(m_strSectionFolderName, gstrLAST_OPENED_FOLDER_NAME, strFolderName);
}
//-------------------------------------------------------------------------------------------------
vector<string> FileSupplierConfigMgr::getFileExtensionList()
{
	vector<string> vecExtList;
	// default set of images support by our current SSOCR
	string strList = "*.tif"
					 "|*.bmp"
					 "|*.bmp*;*.rle*;*.dib*;*.rst*;*.gp4*;*.mil*;*.cal*;*.cg4*;"
					 "*.flc*;*.fli*;*.gif*;*.jpg*;*.pcx*;*.pct*;*.png*;*.tga*;*.tif*"
					 "|*.*";

	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, gstrFILE_EXTENSION_LIST))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, gstrFILE_EXTENSION_LIST, strList);
	}

	if (strList.empty())
	{
		strList = m_pCfgMgr->getKeyValue(m_strSectionFolderName, gstrFILE_EXTENSION_LIST, strList);
	}

	// each item in the list is separated by a pipe (|) character, parse them
	StringTokenizer::sGetTokens(strList, "|", vecExtList);

	return vecExtList;
}
//-------------------------------------------------------------------------------------------------
void FileSupplierConfigMgr::setFileExtensionList(const vector<string>& vecFileExtensionList)
{
	string strList("");
	for (unsigned int n = 0; n < vecFileExtensionList.size(); n++)
	{
		if (n > 0)
		{
			strList += "|";
		}

		strList += vecFileExtensionList[n];
	}

	m_pCfgMgr->setKeyValue(m_strSectionFolderName, gstrFILE_EXTENSION_LIST, strList);
}
//-------------------------------------------------------------------------------------------------
void FileSupplierConfigMgr::getOpenedFolderHistoryFromScope(std::vector<std::string>& rvecHistory)
{
	string strList("");
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, gstrOPENED_FOLDER_HISTORY))
	{
		m_pCfgMgr->createKey(m_strSectionFolderName, gstrOPENED_FOLDER_HISTORY, strList);
	}
	strList = m_pCfgMgr->getKeyValue(m_strSectionFolderName, gstrOPENED_FOLDER_HISTORY, strList);
	

	// each item in the list is separated by a pipe (|) character, parse them
	StringTokenizer::sGetTokens(strList, "|", rvecHistory);
}
//-------------------------------------------------------------------------------------------------
void FileSupplierConfigMgr::setOpenedFolderHistoryFromScope(const std::vector<std::string>& vecHistory)
{
	string strList("");
	for (unsigned int n = 0; n<vecHistory.size(); n++)
	{
		if (n > 0)
		{
			strList += "|";
		}

		strList += vecHistory[n];
	}

	m_pCfgMgr->setKeyValue(m_strSectionFolderName, gstrOPENED_FOLDER_HISTORY, strList);
}
//-------------------------------------------------------------------------------------------------
std::string FileSupplierConfigMgr::getLastUsedFileExtension()
{
	if (!m_pCfgMgr->keyExists(m_strSectionFolderName, gstrLAST_USED_FILE_EXTENSION))
	{
		return gstrDEFAULT_LAST_USED_FILE_EXTENSION;
	}

	return m_pCfgMgr->getKeyValue(m_strSectionFolderName, gstrLAST_USED_FILE_EXTENSION,
		gstrDEFAULT_LAST_USED_FILE_EXTENSION);
}
//-------------------------------------------------------------------------------------------------
void FileSupplierConfigMgr::setLastUsedFileExtension(const std::string& strExt)
{
	m_pCfgMgr->setKeyValue(m_strSectionFolderName, gstrLAST_USED_FILE_EXTENSION, strExt);
}
//-------------------------------------------------------------------------------------------------
