#include "stdafx.h"
#include "RegExLoader.h"
#include "Common.h"

#include <Misc.h>

RegExLoader::RegExLoader()
{
}
//--------------------------------------------------------------------------------------------------
void RegExLoader::loadObjectFromFile(string& strRegEx, const string& strFileName)
{
	strRegEx = getRegExpFromFile(strFileName, true, gstrAF_AUTO_ENCRYPT_KEY_PATH);
}
//--------------------------------------------------------------------------------------------------