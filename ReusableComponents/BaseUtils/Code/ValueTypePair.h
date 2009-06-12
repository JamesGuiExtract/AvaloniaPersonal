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
		enum EType { kString, kOctets, kInt, kLong, kUnsignedLong, kDouble, kBoolean, kNone };

		ValueTypePair();
		ValueTypePair(const string &_strValue);
		ValueTypePair(unsigned char *_octValue, long _lOctValueSize);
		ValueTypePair(int _iValue);
		ValueTypePair(unsigned int _iValue);
		ValueTypePair(long _lValue);
		ValueTypePair(unsigned long _lValue);
		ValueTypePair(double _dValue);
		ValueTypePair(bool bValue);
		ValueTypePair(const char *pszValue);

		virtual ~ValueTypePair();
		
		// copy constructor
		ValueTypePair(const ValueTypePair& vtpToCopy);

		// assignment operator
		ValueTypePair& operator=(const ValueTypePair& vtpToAssign);
    
		void setValue(const string &_strValue);
		void setValue(const char *pszValue);
		void setValue(unsigned char *_octValue, long _lOctValueSize);
		void setValue(long _lValue);
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
		double dValue;
		bool bValue;
		unsigned long ulValue;

		EType eType;

		long lOctValueSize;
		unsigned char *octValue;
};

#endif // VALUETYPEPAIR_HPP