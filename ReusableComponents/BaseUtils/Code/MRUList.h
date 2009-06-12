////////////////////////////////////////////////////////////////////////////////////////////////////
//	Class:		MRUList
//	Purpose:	To hold file names in a fix-size FIFO (First-In-First-Out) queue in the 
//				format of strings. 
////////////////////////////////////////////////////////////////////////////////////////////////////

#pragma once

#include "BaseUtils.h"
#include "IConfigurationSettingsPersistenceMgr.h"

#include <deque>
#include <string>
#include <memory>

class EXPORT_BaseUtils MRUList
{
public:
	//==============================================================================================
	// PURPOSE:	Construct the object
	// REQUIRE: All entries will be stored per-user base
	// ARGS:	IConfigurationSettingsPersistenceMgr:	The pointer to the configuration mgr
	//			strPersistStoreSectionName:	The section folder name for storing the file list
	//										in the persist data (eg. Registry). For example, the 
	//										section name can be "\\MRUList"
	//			strPersistStoreEntryFormat:	The format for each entry which will store the actual
	//										file name. The format contain "%d", which will be 
	//										used for substituting the index of each MRU item. 
	//										For example, if the format string is "file%d" then the 
	//										entries will be named file1, file2, and so on. 
	//			nSize:						Max size of the queue. nSize > 0. Default to 1
	//
	MRUList(IConfigurationSettingsPersistenceMgr* pConfigMgr,
			const std::string& strPersistStoreFolderName,
			const std::string& strPersistStoreEntryFormat,
			unsigned long nSize);
	//==============================================================================================
	// PURPOSE:	Add the most recent file at the top of the queue
	// REQUIRE:	If the FIFO queue size reaches its maximum, every entry added to the queue will be
	//			always placed at the top, and the bottom most item will be removed from the queue.
	// ARGS:	strMRUName:	The most rectent used file name
	//
	void addItem(const std::string& strMRUName);
	//==============================================================================================
	// PURPOSE:	Retrieve the file name at nIndex in the queue
	// REQUIRE: nIndex < Current queue size
	// ARGS:	nIndex: 0-based index
	//
	const std::string& at(unsigned long nIndex) const;
	//==============================================================================================
	// PURPOSE:	To remove all items
	//
	void clearList();
	//==============================================================================================
	// PURPOSE:	To read from persistent store
	// PROMISE:	Populate the queue if persistent store is not empty
	//
	void readFromPersistentStore();
	//==============================================================================================
	// PURPOSE:	To remove an item from MRU List
	// REQUIRE: If strMRUName can't be found in the MRU list, do nothing
	//
	void removeItem(const std::string& strMRUName);
	//==============================================================================================
	// PURPOSE:	To remove an item from MRU List at the specified index
	// REQUIRE: nIndex < Current queue size
	//
	void removeItemAt(unsigned long nIndex);
	//==============================================================================================
	// PURPOSE:	To get current size of the queue
	// PROMISE:	Return current size of the queue, not the max size of the queue
	//
	unsigned long getCurrentListSize() {return (unsigned long) m_quMRUList.size();}
	//==============================================================================================
	// PURPOSE:	To get maximum size of the queue
	// PROMISE:	Return maximum size of the queue
	//
	unsigned long getMaxListSize() {return m_nMaxSize;}
	//==============================================================================================
	// PURPOSE:	To write to the persistent store
	// REQUIRE: Current queue must not be empty
	// PROMISE: Write the contents of current queue to the persistent store
	//
	void writeToPersistentStore();

private:
	// Most recent file list
	std::deque<std::string> m_quMRUList;
	// persistent mgr
	IConfigurationSettingsPersistenceMgr *m_pConfigMgr;
	// max queue size
	unsigned long m_nMaxSize;
	// section folder name
	std::string m_strSectionName;
	// key entry format
	CString m_pszEntryFormat;

	// Helper function
	std::string getEntryName(unsigned long ulIndex);
};
