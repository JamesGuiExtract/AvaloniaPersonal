#pragma once

#include "AFCppUtils.h"
#include <string>


class EXPORT_AFCppUtils AFTagManager
{

public:
	AFTagManager();
	~AFTagManager();

	const std::string expandTagsAndFunctions(const std::string& strText, IAFDocumentPtr ipAFDoc);
	
	// Validates that if the value is a dynamic file specification that it is either an absolute
	// path for a file that exists or is a path based on a tag (and not a relative path for which
	// the base directory may not be clear).
	static void validateDynamicFilePath(const std::string& eliCode, std::string strValue);

	// For each item in the list, validates that if the value is a dynamic file specification that
	// it is either an absolute path for a file that exists or is a path based on a tag (and not a
	// relative path for which the base directory may not be clear).
	static void validateDynamicFilePath(const std::string& eliCode, IVariantVectorPtr ipList);

private:
	IAFUtilityPtr getAFUtility();

};