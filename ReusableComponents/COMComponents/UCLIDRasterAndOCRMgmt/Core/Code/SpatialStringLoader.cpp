
#include "stdafx.h"
#include "SpatialStringLoader.h"

#include <UCLIDException.h>

using namespace std;

//--------------------------------------------------------------------------------------------------
void SpatialStringLoader::loadObjectFromFile(
										UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipSpatialString,
										const string& strFile)
{
	try
	{
		ASSERT_ARGUMENT("ELI36671", ipSpatialString != NULL);

		ipSpatialString->LoadFrom(strFile.c_str(), VARIANT_FALSE);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36672");
}
//--------------------------------------------------------------------------------------------------
