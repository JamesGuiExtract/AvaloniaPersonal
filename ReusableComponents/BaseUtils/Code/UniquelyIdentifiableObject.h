#ifndef UNIQUELY_IDENTIFIABLE_OBJECT_HPP
#define UNIQUELY_IDENTIFIABLE_OBJECT_HPP

#ifndef BASE_UTIL_H
#include "BaseUtils.h"
#endif

class EXPORT_BaseUtils UniquelyIdentifiableObject
{
public:
	UniquelyIdentifiableObject();
	UniquelyIdentifiableObject(const UniquelyIdentifiableObject& objToCopy);	
	UniquelyIdentifiableObject& operator = (const UniquelyIdentifiableObject& objToAssign);

	virtual ~UniquelyIdentifiableObject();
	EXPORT_BaseUtils friend bool operator == (const UniquelyIdentifiableObject& obj1,
						     const UniquelyIdentifiableObject& obj2);

private:
	bool m_bOwner;
	char* m_pUniqueAddress;
};

#endif // UNIQUELY_IDENTIFIABLE_OBJECT_HPP