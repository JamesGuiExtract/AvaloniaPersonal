// NamedValueTypePair.h: interface for the NamedValueTypePair class.

#pragma once

#include "BaseUtils.h"
#include "ValueTypePair.h"

#include <string>

class EXPORT_BaseUtils NamedValueTypePair 
{
public:
	NamedValueTypePair();
	NamedValueTypePair(const string& strName,const ValueTypePair& valueTypePair);
	virtual ~NamedValueTypePair();

	inline void SetName(const string& strName) {m_strName = strName;}
	inline string GetName(void) const {return m_strName;}
	inline void SetPair(const ValueTypePair& valueTypePair) {m_pair = valueTypePair;}
	inline ValueTypePair GetPair(void) const {return m_pair;}

private:
	string m_strName;
	ValueTypePair m_pair;

};
