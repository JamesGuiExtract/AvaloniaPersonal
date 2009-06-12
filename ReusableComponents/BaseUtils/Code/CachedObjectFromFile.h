
#pragma once

#include "stdafx.h"
#include "cpputil.h"
#include "ExtractMFCUtils.h"

#include <string>

//-------------------------------------------------------------------------------------------------
// PURPOSE:	This template class can be used whenever an object is loaded from
//			a file, and the object needs to be kept in memory, but refreshed from
//			the file only when the file has been changed.
// REQUIRE: FileObjectLoader is a class name which has a method called 
//			loadObjectFromFile() that takes two arguments - the first argument of type
//			ObjectType (representing the object that needs to be loaded from a file)
//			and the second argument of type const std::string& (representing the
//			filename from which the object should be loaded).
//-------------------------------------------------------------------------------------------------
template <class ObjectType, class FileObjectLoader>
class CachedObjectFromFile
{
public:
	// publicly accessible object member variable
	ObjectType m_obj;

	// method to load object from a file, if it's not already in the cache
	void loadObjectFromFile(const std::string& strFile)
	{
		// Check if the file exists (do not validate existence, allow
		// validation to be handled by the object loader - this will
		// allow for objects which make use of the auto-encryption code
		// to perform the auto-encryption for non-existent files, see
		// [FlexIDSCore #3413]). If the file exists and has already been
		// seen, check the time stamp and load accordingly.
		if (isValidFile(strFile) && m_strFile == strFile)
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
				return;
			}
		}

		// object is not in cache, or cached copy is out of date
		// load the object from the file
		FileObjectLoader ol;
		ol.loadObjectFromFile(m_obj, strFile);

		// object loading was successful.  Update the member vars
		m_strFile = strFile;
		m_fileTimeAtLastLoad = getFileModificationTimeStamp(m_strFile);
	}

	// Resets CacheObjectFromFile so that no object is cached.
	void Clear()
	{
		m_obj = NULL;
		m_strFile = "";
		m_fileTimeAtLastLoad = NULL;
	}

private:
	std::string m_strFile;
	CTime m_fileTimeAtLastLoad;
};
//-------------------------------------------------------------------------------------------------
// Example usage:
// 
// class IntLoader
// {
// public:
//    void loadObjectFromFile(int& rInt, const std::string& strFile)
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
