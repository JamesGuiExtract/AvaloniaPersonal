
#ifndef OBJECT_WITH_DEBUG_INFO_HPP
#define OBJECT_WITH_DEBUG_INFO_HPP

#ifndef BASE_UTIL_H
#include "BaseUtils.h"
#endif
#ifndef UCLID_EXCEPTION_HPP
#include "UCLIDException.h"
#endif

class EXPORT_BaseUtils ObjectWithDebugInfo
{
public:
	virtual void addDebugInfoTo(UCLIDException& uclidException) = 0;
};

#endif OBJECT_WITH_DEBUG_INFO_HPP