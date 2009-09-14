#ifndef EVENT_DATA_HPP
#define EVENT_DATA_HPP

#ifndef BASE_UTIL_H
#include "BaseUtils.h"
#endif
#ifndef VALUETYPEPAIR_HPP
#include "ValueTypePair.h"
#endif

#include <vector>
#include <string>
#include <map>
using namespace std;

EXPIMP_TEMPLATE_BASEUTILS template class EXPORT_BaseUtils std::map<string, ValueTypePair>;

class EXPORT_BaseUtils EventData : public map<string, ValueTypePair>
{
public:
	// a method to determine if a certain key exists as part of the data
	bool has(const string& strKey) const;

	// a method to replace the [] operator
	const ValueTypePair& get(const string& strKey) const;

	// since we have hid the underlying [] operator
	// the user will no longer be able to add to the map
	// by using the [] operator.  So, provide a method to
	// allow the user to add to the map
	void add(const string& strKey, const ValueTypePair& vtpData);

	// need a way to determine all the event data key's
	vector<string> getKeys(void) const;

protected:
	// we need to protect the [] operator because of the fact
	// that it returns a reference...
	// allow derived classes to use the [] operator but not
	// outside classes.  Outside classes can access data
	// only via the get() method (which returns a const reference)
	ValueTypePair& operator[] (const string& strKey);
};

#endif // EVENT_DATA_HPP