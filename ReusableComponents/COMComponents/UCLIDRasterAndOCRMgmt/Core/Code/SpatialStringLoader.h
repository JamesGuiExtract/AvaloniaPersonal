
#pragma once

#include <CachedObjectFromFile.h>

#include <string>

class SpatialStringLoader : public FileObjectLoaderBase
{
public:
	void loadObjectFromFile(UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipSpatialString, const std::string& strFile);
};