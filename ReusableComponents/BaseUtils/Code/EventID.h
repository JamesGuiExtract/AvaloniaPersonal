#ifndef UNIQUE_ID_HPP
#define UNIQUE_ID_HPP

#ifndef BASE_UTIL_H
#include "BaseUtils.h"
#endif
#ifndef UNIQUELY_IDENTIFIABLE_OBJECT_HPP
#include "UniquelyIdentifiableObject.h"
#endif

class EXPORT_BaseUtils EventID : public UniquelyIdentifiableObject
{
public:
	virtual ~EventID(){};
};

#endif // UNIQUE_ID_HPP

