#include "stdafx.h"
#include "ValueTypePair.h"
#include "UCLIDException.h"
#include "COMUtils.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair()
{
	octValue = NULL;
	eType = kNone;
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair(const ValueTypePair& vtpToCopy)
{
	ValueTypePair::ValueTypePair();

	switch(vtpToCopy.eType)
	{
	case kBoolean:
		setValue(vtpToCopy.getBooleanValue());
		break;
	case kString:
		setValue(vtpToCopy.strValue);
		break;
	case kDouble:
		setValue(vtpToCopy.dValue);
		break;
	case kInt:
		setIntValue(vtpToCopy.iValue);
		break;
	case kInt64:
		setValue(vtpToCopy.llValue);
		break;
	case kLong:
		setValue(vtpToCopy.lValue);
		break;
	case kUnsignedLong:
		setValue(vtpToCopy.ulValue);
		break;
	case kOctets:
		unsigned char *pOctets;
		long lSize;
		pOctets = vtpToCopy.getOctValueAsReference(lSize);
		setValue(pOctets, lSize);
		break;
	case kNone:
		eType = kNone;
		octValue = NULL;
		break;
	case kGuid:
		eType = kGuid;
		guidValue = vtpToCopy.guidValue;
		break;
	default:
		// we should never reach here!
		THROW_LOGIC_ERROR_EXCEPTION("ELI00512");
	};
}
//--------------------------------------------------------------------------------------------------
ValueTypePair& ValueTypePair::operator=(const ValueTypePair& vtpToAssign)
{
	octValue = NULL;
	eType = kNone;

	// TODO: clean up - this code was cut and pasted from above!
	switch(vtpToAssign.eType)
	{
	case kBoolean:
		setValue(vtpToAssign.getBooleanValue());
		break;
	case kString:
		setValue(vtpToAssign.strValue);
		break;
	case kDouble:
		setValue(vtpToAssign.dValue);
		break;
	case kInt:
		setIntValue(vtpToAssign.iValue);
		break;
	case kInt64:
		setValue(vtpToAssign.llValue);
		break;
	case kLong:
		setValue(vtpToAssign.lValue);
		break;
	case kUnsignedLong:
		setValue(vtpToAssign.ulValue);
		break;
	case kOctets:
		unsigned char *pOctets;
		long lSize;
		pOctets = vtpToAssign.getOctValueAsReference(lSize);
		setValue(pOctets, lSize);
		break;
	case kNone:
		eType = kNone;
		octValue = NULL;
		break;
	case kGuid:
		eType = kGuid;
		guidValue = vtpToAssign.guidValue;
		break;
	default:
		// we should never reach here!
		THROW_LOGIC_ERROR_EXCEPTION("ELI00513");
	};

	return *this;
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair(const string &_strValue)
{
	ValueTypePair::ValueTypePair();

	setValue(_strValue);
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair(const char *pszValue)
{
	ValueTypePair::ValueTypePair();

	setValue(string(pszValue));
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair(unsigned char *_octValue, long _lOctValueSize)
{
	ValueTypePair::ValueTypePair();

	setValue(_octValue, _lOctValueSize);
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair(int _iValue)
{
	ValueTypePair::ValueTypePair();

	setIntValue(_iValue);
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair(unsigned int _iValue)
{
	ValueTypePair::ValueTypePair();
	
	//set as unsigned long value 
	unsigned long _ulValue = _iValue;
	setValue(_ulValue);
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair(__int64 _llValue)
{
	ValueTypePair::ValueTypePair();

	setValue(_llValue);
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair(long _lValue)
{
	ValueTypePair::ValueTypePair();

	setValue(_lValue);
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair(unsigned long _ulValue)
{
	ValueTypePair::ValueTypePair();

	setValue(_ulValue);
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair(bool _bValue)
{
	ValueTypePair::ValueTypePair();

	setValue(_bValue);
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair(double _dValue)
{
	ValueTypePair::ValueTypePair();

	setValue(_dValue);
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair(GUID _guidValue)
{
	setValue(_guidValue);
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::ValueTypePair(const variant_t vtVariant)
{
	try
	{
		switch (vtVariant.vt)
		{
		case VT_EMPTY:
			setValue("<Empty>");
			break;
		case VT_NULL:
			setValue("<NULL>");
			break;
		case VT_I1:
		case VT_I2:
		case VT_I4:
		case VT_INT:
			setValue((long)vtVariant);
			break;
		case VT_I8:
			setValue((long long)vtVariant);
			break;
		case VT_UI1:
		case VT_UI2:
		case VT_UI4:
		case VT_UINT:
			setValue((unsigned long)vtVariant);
			break;
		case VT_R4:
		case VT_R8:
		case VT_DECIMAL:
			setValue((double)vtVariant);
			break;
		case VT_BOOL:
			setValue(asCppBool((VARIANT_BOOL)vtVariant));
			break;
		case VT_DATE:
			setValue(vtVariant.date);
			break;
		case VT_BSTR:
			setValue(asString(vtVariant.bstrVal));
			break;
		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI51734");
		}
	}
	catch (UCLIDException &ue)
	{
		ue.addDebugInfo("VariantType", vtVariant.vt);
		UCLIDException logExecption("ELI51735", "Unable to convert variant value", ue);
		logExecption.log();
		setValue("Unable to convert variant_t of type " + asString(vtVariant.vt));
	}
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::~ValueTypePair()
{
	try
	{
		if (eType == kOctets)
		{
			// release memory
			delete[] octValue;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16415");
}
//--------------------------------------------------------------------------------------------------
void ValueTypePair::setValue(const string &_strValue)
{
	eType = kString;
	strValue = _strValue;
}
//--------------------------------------------------------------------------------------------------
void ValueTypePair::setValue(const char *pszValue)
{
	eType = kString;
	strValue = string(pszValue);
}
//--------------------------------------------------------------------------------------------------
void ValueTypePair::setValue(unsigned char *_octValue, long _lOctValueSize)
{
	eType = kOctets;
	octValue = new unsigned char[_lOctValueSize];
	memcpy((void *) octValue, (void *) _octValue, _lOctValueSize);
	lOctValueSize = _lOctValueSize;
}
//--------------------------------------------------------------------------------------------------
void ValueTypePair::setIntValue(int _iValue)
{
	eType = kInt;
	iValue = _iValue;
}
//--------------------------------------------------------------------------------------------------
void ValueTypePair::setValue(long _lValue)
{
	eType = kLong;
	lValue = _lValue;
}
//--------------------------------------------------------------------------------------------------
void ValueTypePair::setValue(__int64 _llValue)
{
	eType = kInt64;
	llValue = _llValue;
}
//--------------------------------------------------------------------------------------------------
void ValueTypePair::setValue(unsigned long _ulValue)
{
	eType = kUnsignedLong;
	ulValue = _ulValue;
}
//--------------------------------------------------------------------------------------------------
void ValueTypePair::setValue(bool _bValue)
{
	eType = kBoolean;
	bValue = _bValue;
}
//--------------------------------------------------------------------------------------------------
void ValueTypePair::setValue(double _dValue)
{
	eType = kDouble;
	dValue = _dValue;
}
void ValueTypePair::setValue(GUID _guidValue)
{
	eType = kGuid;
	guidValue = _guidValue;
}
//--------------------------------------------------------------------------------------------------
ValueTypePair::EType ValueTypePair::getType() const
{
	return eType;
}
//--------------------------------------------------------------------------------------------------
string ValueTypePair::getValueAsString() const
{
	string strText;

	switch (eType)
	{
	case kString:
		strText = strValue;
		break;

	case kInt:
		strText = asString( iValue );
		break;

	case kLong:
		strText = asString( lValue );
		break;

	case kUnsignedLong:
		strText = asString( ulValue );
		break;

	case kBoolean:
		strText = asString( bValue );
		break;

	case kDouble:
		strText = asString( dValue );
		break;

	case kOctets:
		// Write out each of the bytes as two characters
		for (long i = 0; i < lOctValueSize; i++)
		{
			char pszTemp[3];
			sprintf_s( pszTemp, sizeof(pszTemp), "%02x", octValue[i] );
			strText += pszTemp;
		}
		break;
	case kInt64:
		strText = asString(llValue);
		break;
	case kGuid:
		strText = asString(guidValue);
		break;
	}

	return strText;
}
//--------------------------------------------------------------------------------------------------
string ValueTypePair::getStringValue() const
{
	if (eType != kString)
	{
		throw UCLIDException("ELI00447", "Internal error: getStringValue() called on a non-string ValueTypePair.");
	}

	return strValue;
}
//--------------------------------------------------------------------------------------------------
unsigned char *ValueTypePair::getOctValueAsReference(long &_lOctValueSize) const
{
	if (eType != kOctets)
	{
		throw UCLIDException("ELI00448", "Internal error: getOctValueReference() called on a non-oct ValueTypePair.");
	}
	
	_lOctValueSize = lOctValueSize;
	
	return octValue;
}
//--------------------------------------------------------------------------------------------------
unsigned char *ValueTypePair::getOctValueAsCopy(long &_lOctValueSize) const
{
	if (eType != kOctets)
	{
		throw UCLIDException("ELI00449", "Internal error: getOctValueCopy() called on a non-oct ValueTypePair.");
	}
	
	_lOctValueSize = lOctValueSize;

	unsigned char *octCopy = new unsigned char[lOctValueSize];
	memcpy((void *) octCopy, (void *) octValue, lOctValueSize);
	return octCopy;
}
//--------------------------------------------------------------------------------------------------
long ValueTypePair::getOctSize(void) const
{
	if (eType != kOctets)
	{
		throw UCLIDException("ELI00450", "Internal error: getOctValueCopy() called on a non-oct ValueTypePair.");
	}

	return lOctValueSize;
}
//--------------------------------------------------------------------------------------------------
long ValueTypePair::getLongValue() const
{
	if (eType != kLong)
	{
		throw UCLIDException("ELI00451", "Internal error: getLongValue() called on a non-long ValueTypePair.");
	}

	return lValue;
}
//--------------------------------------------------------------------------------------------------
unsigned long ValueTypePair::getUnsignedLongValue() const
{
	if (eType != kUnsignedLong)
	{
		throw UCLIDException("ELI00511", "Internal error: getUnsignedLongValue() called on a non-unsigned-long ValueTypePair.");
	}

	return ulValue;
}
//--------------------------------------------------------------------------------------------------
int ValueTypePair::getIntValue() const
{
	if (eType != kInt)
	{
		throw UCLIDException("ELI00452", "Internal error: getIntValue() called on a non-int ValueTypePair.");
	}

	return iValue;
}
//--------------------------------------------------------------------------------------------------
__int64 ValueTypePair::getInt64Value() const
{
	if (eType != kInt64)
	{
		throw UCLIDException("ELI28630", "Internal error: getInt64Value() called on a non-int64 ValueTypePair.");
	}

	return llValue;
}
//--------------------------------------------------------------------------------------------------
double ValueTypePair::getDoubleValue() const
{
	if (eType != kDouble)
	{
		throw UCLIDException("ELI00453", "Internal error: getDoubleValue() called on a non-double ValueTypePair.");
	}

	return dValue;
}
//--------------------------------------------------------------------------------------------------
bool ValueTypePair::getBooleanValue() const
{
	if (eType != kBoolean)
	{
		throw UCLIDException("ELI00454", "getBooleanValue() called on a non-boolean ValueTypePair.");
	}

	return bValue;
}
//--------------------------------------------------------------------------------------------------
UUID ValueTypePair::getGuidValue() const
{
	if (eType != kGuid)
	{
		throw UCLIDException("ELI54038", "getGuidValue() called on a non-Guid ValueTypePair.");
	}
	return guidValue;
}
//--------------------------------------------------------------------------------------------------
