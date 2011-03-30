
#include "stdafx.h"
#include "StringLoader.h"
#include <COMUtils.h>

#include <UCLIDException.h>

StringLoader::StringLoader()
{
	if(m_ipMiscUtils == __nullptr)
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI14616", m_ipMiscUtils != __nullptr );
	}
}
//--------------------------------------------------------------------------------------------------
StringLoader::~StringLoader()
{
	try
	{
		// Reset the misc utils pointer
		m_ipMiscUtils = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27085");
}
//--------------------------------------------------------------------------------------------------
void StringLoader::loadObjectFromFile(string& strFromFile, const string& strFile)
{
	// Call GetStringOptionallyFromFile() to load the string
	strFromFile = asString( m_ipMiscUtils->GetStringOptionallyFromFile(strFile.c_str()) );
}
//--------------------------------------------------------------------------------------------------
void StringLoader::loadObjectFromFile(IVariantVectorPtr & ipVector, const string& strFile)
{
	// Call GetColumnStringsOptionallyFromFile() to load the string list
	ipVector = m_ipMiscUtils->GetColumnStringsOptionallyFromFile(strFile.c_str()) ;
}
//--------------------------------------------------------------------------------------------------