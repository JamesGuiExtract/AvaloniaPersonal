
#ifndef NAMED_OBJECT_HPP
#define NAMED_OBJECT_HPP

#ifndef BASE_UTIL_H
#include "BaseUtils.h"
#endif
#ifndef OBJECT_WITH_DEBUG_INFO_HPP
#include "ObjectWithDebugInfo.h"
#endif

#include <string>
using namespace std;


class EXPORT_BaseUtils NamedObject : public ObjectWithDebugInfo
{
public:
	NamedObject(const string& strObjectName = "");
	void setObjectName(const string& strNewObjectName);
	const string& getObjectName() const;

	virtual void addDebugInfoTo(UCLIDException& uclidException);

private:
	string strObjectName;
};

#endif // NAMED_OBJECT_HPP
