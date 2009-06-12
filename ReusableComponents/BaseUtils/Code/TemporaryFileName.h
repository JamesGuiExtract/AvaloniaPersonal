#pragma once

#include "BaseUtils.h"
#include <string>
#include <afxmt.h>

class EXPORT_BaseUtils TemporaryFileName
{
public:
	//----------------------------------------------------------------------------------------------
	// constructor - added as per [p13 #4951] - JDS 04/09/2008
	TemporaryFileName(const std::string& strDir, const char* pszPrefix, const char* pszSuffix,
		bool bAutoDelete);
	//----------------------------------------------------------------------------------------------
	// constructor
	TemporaryFileName(const char *pszPrefix = NULL, const char *pszSuffix = NULL, 
		bool bAutoDelete = true);
	//----------------------------------------------------------------------------------------------
	// destructor
	// PROMISE: to delete the file whose name is getName(), if the file exists, and
	// if m_bAutoDelete == true.
	~TemporaryFileName();
	//----------------------------------------------------------------------------------------------
	inline const std::string& getName()
	{
		return m_strFileName;
	}
	//----------------------------------------------------------------------------------------------

private:
	//----------------------------------------------------------------------------------------------
	// Variables
	//----------------------------------------------------------------------------------------------
	std::string m_strFileName;
	bool m_bAutoDelete;

	//----------------------------------------------------------------------------------------------
	// Functions
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To create a unique temporary file in the specified directory (if no directory
	//			is specified then will create the file in the TEMP directory).  Also
	//			sets the m_strFileName to point to the specified file.  
	//
	// PROMISE: To throw an exception if unable to create the temporary file.
	void init(std::string strDir, const char* pszPrefix, const char* pszSuffix);
};