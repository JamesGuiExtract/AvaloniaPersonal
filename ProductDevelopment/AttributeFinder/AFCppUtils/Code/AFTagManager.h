#pragma once

#include "AFCppUtils.h"
#include <string>


class EXPORT_AFCppUtils AFTagManager
{

public:
	AFTagManager();
	~AFTagManager();

	const std::string expandTagsAndFunctions(const std::string& strText, IAFDocumentPtr ipAFDoc);

private:
	IAFUtilityPtr getAFUtility();

};