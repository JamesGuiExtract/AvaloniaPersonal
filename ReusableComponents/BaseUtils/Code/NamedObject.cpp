
#include "stdafx.h"
#include "NamedObject.h"


NamedObject::NamedObject(const string& strObjectName)
:strObjectName(strObjectName)
{
}

void NamedObject::setObjectName(const string& strNewObjectName)
{
	strObjectName = strNewObjectName;
}

const string& NamedObject::getObjectName() const
{
	return strObjectName;
}

void NamedObject::addDebugInfoTo(UCLIDException& uclidException)
{
	if (strObjectName != "")
		uclidException.addDebugInfo("ObjectName", strObjectName);
}
