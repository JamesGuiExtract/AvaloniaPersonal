
#pragma once

#include "stdafx.h"
#include "cpputil.h"
#include "ExtractMFCUtils.h"
#include "Misc.h"

#include <map>
#include <string>
using namespace std;

//-------------------------------------------------------------------------------------------------
// PURPOSE:	A base class all FileObjectLoader instantiations used by CachedObjectFromFile must
//			derive from.
// REQUIRE: isModified must be overriden if the test to see if a file has been modified requires
//			anything more than checking the file modification time. For example, a given file may
//			import contents from one or more addditional files. In this case, isModified should be
//			overriden to check to the modification times of the referenced files.
//-------------------------------------------------------------------------------------------------
class FileObjectLoaderBase
{
public:
	// As this is a helper class for CachedObjectFromFile, allow it access to this class's members.
	template <class ObjectType, class FileObjectLoader> friend class CachedObjectFromFile;

	FileObjectLoaderBase(const string& strAutoEncryptKey = "") 
		: m_strAutoEncryptKey(strAutoEncryptKey)
	{
	}

	// Loads a regular expression from a file.
	// The input file is auto-encrypted if out of date and if specified by the RDT settings.
	virtual void loadObjectFromFile(string& strRegEx, const string& strFileName) abstract;

	// PURPOSE:	Indicates whether a file has been modified since it was last loaded.
	// RETURNS:	true if the file is modified and needs to be reloaded, false otherwise.
	virtual bool isModified(const string& strFile)
	{ 
		// If the file has already been seen, check the time stamp and load accordingly.
		if (m_strFile == strFile)
		{
			// file names are equal
			// now check timestamp
			CTime currFileTime(getFileModificationTimeStamp(strFile));
			
			// NOTE: from a cache perspective, we want to use == instead of <=
			// in the following line.  If file with time t2 was loaded into the
			// cache, and the user replaces the file with a file with time t1
			// where t1 < t2, then we do want to load the object back from t1.
			if (currFileTime == m_fileTimeAtLastLoad)
			{
				// object is in cache...just return
				return false;
			}
		}

		return true;
	}

protected:
	
	string m_strFile;
	CTime m_fileTimeAtLastLoad;
	string m_strAutoEncryptKey;
};

//-------------------------------------------------------------------------------------------------
// PURPOSE:	This template class can be used whenever an object is loaded from
//			a file, and the object needs to be kept in memory, but refreshed from
//			the file only when the file has been changed.
// REQUIRE: FileObjectLoader is a class name which has a method called 
//			loadObjectFromFile() that takes two arguments - the first argument of type
//			ObjectType (representing the object that needs to be loaded from a file)
//			and the second argument of type const string& (representing the
//			filename from which the object should be loaded).
//-------------------------------------------------------------------------------------------------
template <class ObjectType, class FileObjectLoader>
class CachedObjectFromFile
{
public:
	CachedObjectFromFile::CachedObjectFromFile(const string& strAutoEncryptKey = "")
	{
		m_fileLoader.m_strAutoEncryptKey = strAutoEncryptKey;
	}

	// publicly accessible object member variable
	ObjectType m_obj;

	// Checks whether the data in the file has been modified.
	bool isModified(const string& strFile)
	{
		return m_fileLoader.isModified(strFile);
	}

	// Method to load object from a file, if it's not already in the cache.
	void loadObjectFromFile(const string& strFile)
	{
		// If an auto-encryption registry key has been specified, auto-encrypt the file before
		// loading it.
		if (!m_fileLoader.m_strAutoEncryptKey.empty())
		{
			autoEncryptFile(strFile, m_fileLoader.m_strAutoEncryptKey);
		}

		// [FlexIDSCore:3413]
		// After the file has been auto-encrypted, we can assert its existance.
		validateFileOrFolderExistence(strFile);

		// If the data in the file has not been modifed since the last time it was loaded, there is
		// nothing else to do.
		if (!m_fileLoader.isModified(strFile))
		{
			return;
		}

		// object is not in cache, or cached copy is out of date load the object from the file.
		m_fileLoader.loadObjectFromFile(m_obj, strFile);

		// object loading was successful.  Update the member vars
		m_fileLoader.m_strFile = strFile;
		m_fileLoader.m_fileTimeAtLastLoad = getFileModificationTimeStamp(strFile);
	}

	// Resets CacheObjectFromFile so that no object is cached.
	void Clear()
	{
		m_obj = NULL;
		m_fileLoader.m_strFile = "";
		m_fileLoader.m_fileTimeAtLastLoad = NULL;
	}

private:

	FileObjectLoader m_fileLoader;
};

//-------------------------------------------------------------------------------------------------
// Example usage:
// 
// class IntLoader
// {
// public:
//    void loadObjectFromFile(int& rInt, const string& strFile)
//	  {
//		ifstream infile(strFile.c_str());
//		infile >> rInt;
//	  }
// };
//
// void useCachedObject()
// {
//   CachedObjectFromFile<int, IntLoader> x;
//
//	 do
//	 {
//		// load object into cache from file - only if necessary
//		x.loadObjectFromFile("c:\\a.txt");
//
//		// display the value of the cached object
//		cout << x.m_obj << endl;
//
//		// wait for some time
//		Sleep(1000);
//	 } while (true);
// }
//-------------------------------------------------------------------------------------------------
