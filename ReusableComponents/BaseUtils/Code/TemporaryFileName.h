#pragma once

#include "BaseUtils.h"
#include "Random.h"

#include <string>
#include <afxmt.h>

using namespace std;

class EXPORT_BaseUtils TemporaryFileName
{
public:
	//----------------------------------------------------------------------------------------------
	// constructor
	TemporaryFileName(bool bSensitive, const char *pszPrefix = NULL,
		const char *pszSuffix = NULL, bool bAutoDelete = true);
	//----------------------------------------------------------------------------------------------
	// constructor - added as per [p13 #4951] - JDS 04/09/2008
	TemporaryFileName(bool bSensitive, const string& strDir, const char* pszPrefix,
		const char* pszSuffix, bool bAutoDelete);
	//----------------------------------------------------------------------------------------------
	// constructor
	TemporaryFileName(bool bSensitive, const string& strFileName, bool bAutoDelete = true);
	//----------------------------------------------------------------------------------------------
	// destructor
	// PROMISE: to delete the file whose name is getName(), if the file exists, and
	// if m_bAutoDelete == true.
	~TemporaryFileName();
	//----------------------------------------------------------------------------------------------
	inline const string& getName()
	{
		return m_strFileName;
	}
	//----------------------------------------------------------------------------------------------

private:
	//----------------------------------------------------------------------------------------------
	// Variables
	//----------------------------------------------------------------------------------------------
	string m_strFileName;
	bool m_bSensitive;
	bool m_bAutoDelete;
	Random m_Rand;

	//----------------------------------------------------------------------------------------------
	// Functions
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To create a unique temporary file in the specified directory (if no directory
	//			is specified then will create the file in the TEMP directory).  Also
	//			sets the m_strFileName to point to the specified file. If bRandomFileName is true,
	//			random characters will be inserted between the prefix and suffix, if false, the
	//			filename will simply be dir + prefix + suffix.
	//
	// PROMISE: To throw an exception if unable to create the temporary file.
	void init(string strDir, const char* pszPrefix, const char* pszSuffix, bool bRandomFileName);
};