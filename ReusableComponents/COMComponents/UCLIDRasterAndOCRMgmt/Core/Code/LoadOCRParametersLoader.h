#pragma once

#include <CachedObjectFromFile.h>

#include <UCLIDException.h>
#include <COMUtils.h>
#include <string>

class LoadOCRParametersLoader : public FileObjectLoaderBase
{
public:
	void LoadOCRParametersLoader::loadObjectFromFile(UCLID_RASTERANDOCRMGMTLib::ILoadOCRParametersPtr ipLoadOCRParametersPtr,
	const string& strFile)
{
	try
	{
		ASSERT_ARGUMENT("ELI45958", ipLoadOCRParametersPtr != __nullptr);

		ipLoadOCRParametersPtr->LoadOCRParameters(get_bstr_t(strFile));
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45959");
}
};
