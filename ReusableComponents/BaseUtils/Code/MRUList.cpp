#include "stdafx.h"
#include "MRUList.h"
#include "UCLIDException.h"

#include <algorithm>

using namespace std;

//--------------------------------------------------------------------------------------------------
MRUList::MRUList(IConfigurationSettingsPersistenceMgr* pConfigMgr,
				 const string& strPersistStoreFolderName,
				 const string& strPersistStoreEntryFormat,
				 unsigned long nSize)
:m_strSectionName(strPersistStoreFolderName),
 m_pszEntryFormat(strPersistStoreEntryFormat.c_str()),
 m_pConfigMgr(pConfigMgr)
{
	// default size to 1
	m_nMaxSize = nSize >= 1 ? nSize : 1;
	// start with an empty queue
	m_quMRUList.clear();
}
//--------------------------------------------------------------------------------------------------
void MRUList::addItem(const string& strMRUName)
{
	// if the file name already exists in the list, 
	// then remove it from where is and add it to the top
	deque<string>::iterator iter = find(m_quMRUList.begin(), m_quMRUList.end(), strMRUName);
	if (iter != m_quMRUList.end())
	{
		m_quMRUList.erase(iter);
	}

	// if current size reaches max size
	if (m_quMRUList.size() >= m_nMaxSize)
	{
		// remove the item at the bottom
		m_quMRUList.pop_back();
	}

	// push the item to the top of the queue
	m_quMRUList.push_front(strMRUName);
}
//--------------------------------------------------------------------------------------------------
const string& MRUList::at(unsigned long nIndex) const
{
	// if index is outside current queue size
	if (nIndex >= m_quMRUList.size())
	{
		UCLIDException uclidException("ELI03183", "Index is out of range.");
		uclidException.addDebugInfo("nIndex", nIndex);
		uclidException.addDebugInfo("Size", m_quMRUList.size());
		throw uclidException;
	}

	return m_quMRUList.at(nIndex);
}
//--------------------------------------------------------------------------------------------------
void MRUList::clearList()
{
	m_quMRUList.clear();
}
//--------------------------------------------------------------------------------------------------
void MRUList::readFromPersistentStore()
{
	// clear the queue before reading in
	clearList();

	// force a read from persistent store
	for (unsigned int i = 1; i <= m_nMaxSize; i++)
	{
		// Do NOT push any empty string into the list
		if (m_pConfigMgr->keyExists(m_strSectionName, getEntryName(i)))
		{
			string strKeyValue(m_pConfigMgr->getKeyValue(m_strSectionName, getEntryName(i), ""));
			if (!strKeyValue.empty())
			{
				m_quMRUList.push_back(strKeyValue);
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
void MRUList::removeItem(const std::string& strMRUName)
{
	deque<string>::iterator iter = find(m_quMRUList.begin(), m_quMRUList.end(), strMRUName);
	if (iter != m_quMRUList.end())
	{
		m_quMRUList.erase(iter);
	}
}
//--------------------------------------------------------------------------------------------------
void MRUList::removeItemAt(unsigned long nIndex)
{
	if (nIndex >= m_quMRUList.size())
	{
		UCLIDException uclidException("ELI03331", "Index is out of range.");
		uclidException.addDebugInfo("nIndex", nIndex);
		uclidException.addDebugInfo("Size", m_quMRUList.size());
		throw uclidException;
	}

	m_quMRUList.erase(m_quMRUList.begin() + nIndex);
}
//--------------------------------------------------------------------------------------------------
void MRUList::writeToPersistentStore()
{
	// first clean up the registry entry
	m_pConfigMgr->deleteFolder(m_strSectionName, false);

	unsigned long nCurrentQueueSize = m_quMRUList.size();
	for (unsigned int i = 0; i < nCurrentQueueSize; i++)
	{
		m_pConfigMgr->setKeyValue(m_strSectionName, getEntryName(i+1), m_quMRUList.at(i));
	}
}
//--------------------------------------------------------------------------------------------------
////////////////////////////////////////////////////////////////////////////////////////////////////
// Private helper functions
string MRUList::getEntryName(unsigned long ulIndex)
{
	// creating the entry name for each key depends on the format for the entry
	// For example, if format is "File%d", then entry will be File1, File2, etc.
	CString pszEntry;
	pszEntry.Format(m_pszEntryFormat, ulIndex);

	return (LPCTSTR)pszEntry;
}
