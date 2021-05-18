#ifndef VALUETYPEPAIR_HPP
#define VALUETYPEPAIR_HPP

#ifndef BASE_UTIL_H
#include "BaseUtils.h"
#endif

#include <string>
using namespace std;

class EXPORT_BaseUtils ValueTypePair
{ 
	public:
		// Our code depends on these values being in this order, if adding a new value, add
		// it to the end of the list
		enum EType { kString, kOctets, kInt, kLong, kUnsignedLong, kDouble, kBoolean, kNone, kInt64 };

		ValueTypePair();
		ValueTypePair(const string &_strValue);
		ValueTypePair(unsigned char *_octValue, long _lOctValueSize);
		ValueTypePair(int _iValue);
		ValueTypePair(unsigned int _iValue);
		ValueTypePair(__int64 _llValue);
		ValueTypePair(long _lValue);
		ValueTypePair(unsigned long _lValue);
		ValueTypePair(double _dValue);
		ValueTypePair(bool bValue);
		ValueTypePair(const char *pszValue);
		ValueTypePair(const variant_t vtVariant);

		virtual ~ValueTypePair();
		
		// copy constructor
		ValueTypePair(const ValueTypePair& vtpToCopy);

		// assignment operator
		ValueTypePair& operator=(const ValueTypePair& vtpToAssign);
    
		void setValue(const string &_strValue);
		void setValue(const char *pszValue);
		void setValue(unsigned char *_octValue, long _lOctValueSize);
		void setValue(long _lValue);
		void setValue(__int64 _llValue);
		void setValue(unsigned long _lValue);
		void setValue(double _dValue);
		void setValue(bool _bValue);
		void setIntValue(int iValue);

		EType getType() const;
		
		string getStringValue() const;
		unsigned char *getOctValueAsReference(long &_lOctValueSize) const;
		unsigned char *getOctValueAsCopy(long &_lOctValueSize) const;
		
		long getOctSize() const;
		int getIntValue() const;
		__int64 getInt64Value() const;
		long getLongValue() const;
		unsigned long getUnsignedLongValue() const;
		double getDoubleValue() const;
		bool getBooleanValue() const;

		// Return value as string
		string getValueAsString() const;

	private:

		string strValue;
		int iValue;
		long lValue;
		__int64 llValue;
		double dValue;
		bool bValue;
		unsigned long ulValue;

		EType eType;

		long lOctValueSize;
		unsigned char *octValue;
};

#endif // VALUETYPEPAIR_HPP