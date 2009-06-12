
#include "stdafx.h"
#include "StringLoader.h"
#include <COMUtils.h>

#include <UCLIDException.h>

using namespace std;

StringLoader::StringLoader()
{
	if(m_ipMiscUtils == NULL)
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI14616", m_ipMiscUtils != NULL );
	}
}
//--------------------------------------------------------------------------------------------------
void StringLoader::loadObjectFromFile(std::string& strFromFile, const std::string& strFile)
{
	// Call GetStringOptionallyFromFile() to load the string
	strFromFile = asString( m_ipMiscUtils->GetStringOptionallyFromFile(strFile.c_str()) );
}
//--------------------------------------------------------------------------------------------------
void StringLoader::loadObjectFromFile(IVariantVectorPtr & ipVector, const std::string& strFile)
{
	// Call GetColumnStringsOptionallyFromFile() to load the string list
	ipVector = m_ipMiscUtils->GetColumnStringsOptionallyFromFile(strFile.c_str()) ;
}
//--------------------------------------------------------------------------------------------------