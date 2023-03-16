//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ByteStreamManipulator.cpp
//
// PURPOSE:	Implementation of the ByteStreamManipulator class
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (August 2000 - present)
//
//==================================================================================================

#include "stdafx.h"
#include "ByteStreamManipulator.h"

#include "cpputil.h"
#include "UCLIDException.h"

//--------------------------------------------------------------------------------------------------
ByteStreamManipulator::ByteStreamManipulator(EMode eMode, ByteStream& byteStream)
:byteStream(byteStream), eMode(eMode)
{
	ulNumItems = 0;
	pszByteStreamCursor = NULL;

	if (eMode == kRead)
	{
		pszByteStreamCursor = byteStream.getData();
	}
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator::~ByteStreamManipulator()
{
	try
	{
		// make sure that the two vectors contain the right number of items
		if (vecBytes.size() != ulNumItems || vecLengths.size() != ulNumItems || 
			vecStorageTypes.size() != ulNumItems)
		{
			// TODO: one must really not throw exceptions in the destructor of an object.
			// At some point, this message needs to be written to the application event log
			// instead of throwing an exception.
			UCLIDException uex("ELI00455", 
				"Internal error in ByteStreamManipulator::~ByteStreamManipulator()!");
			uex.addDebugInfo("vecBytes size", vecBytes.size());
			uex.addDebugInfo("vecLengths size", vecLengths.size());
			uex.addDebugInfo("vecStorageTypes size", vecStorageTypes.size());
			uex.addDebugInfo("ulNumItems", ulNumItems);
			throw uex;
		}

		// if memory was allocated by this object, then delete that memory
		if (eMode == kWrite && ulNumItems > 0)
		{
			vector<EStorageType>::const_iterator iterStorageType;
			vector<char *>::const_iterator iterData = vecBytes.begin();
			for (iterStorageType = vecStorageTypes.begin(); iterStorageType != vecStorageTypes.end(); 
				iterStorageType++)
			{
				// delete the memory allocated
				char *pData = (char *) *iterData;
				delete[] pData;

				// advance the data iterator
				iterData++;
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16375");
}
//--------------------------------------------------------------------------------------------------
void ByteStreamManipulator::flushToByteStream(unsigned long ulRequiredNumberToBeMultiplierOf)
{
	// make sure there are some items to flush
	if (ulNumItems == 0)
	{
		UCLIDException uclidEx("ELI00456", 
			"Internal error: there are no items to flush to the byte stream!");
		uclidEx.addDebugInfo("ulNumItems", ValueTypePair((long)ulNumItems));
		throw uclidEx;
	};
	
	// make sure that the two vectors contain the right number of items
	if (vecBytes.size() != ulNumItems || vecLengths.size() != ulNumItems || 
		vecStorageTypes.size() != ulNumItems)
	{
		UCLIDException uex("ELI00457", 
			"Internal error in ByteStreamManipulator::flushToByteStream()!");
		uex.addDebugInfo("vecBytes size", vecBytes.size());
		uex.addDebugInfo("vecLengths size", vecLengths.size());
		uex.addDebugInfo("vecStorageTypes size", vecStorageTypes.size());
		uex.addDebugInfo("ulNumItems", ulNumItems);
		throw uex;
	}

	// calculate the total size of all items;
	unsigned long ulTotalSize = 0;
	vector<unsigned long>::const_iterator iter;
	for (iter = vecLengths.begin(); iter != vecLengths.end(); iter++)
	{
		ulTotalSize += *iter;
	}
	
	// for each string item in the list, the total size needs to be extended
	// by the sizeof(unsigned long) because the length of the string is written
	// to the stream before the actual string data is written to the stream
	vector<EStorageType>::const_iterator iter2;
	for (iter2 = vecStorageTypes.begin(); iter2 != vecStorageTypes.end(); iter2++)
	{
		if (*iter2 == kString || *iter2 == kByteStream)
		{
			ulTotalSize += sizeof(unsigned long);
		}
	}

	// set the total size to be a multiplier of the required number
	unsigned long ulUnusedTrailingChars = ulTotalSize % ulRequiredNumberToBeMultiplierOf;
	if (ulUnusedTrailingChars > 0)
	{
		ulUnusedTrailingChars = ulRequiredNumberToBeMultiplierOf - ulUnusedTrailingChars;
		ulTotalSize += ulUnusedTrailingChars;
	}

	// ensure that the bytestream's size is initialized as required
	byteStream.setSize(ulTotalSize);

	// flush all data to the byte stream
	char *pszDataCursor = (char *) byteStream.getData();
	for (unsigned int i = 0; i < ulNumItems; i++)
	{
		unsigned long ulDataSize = vecLengths[i];
		EStorageType eStorageType = vecStorageTypes[i];
		char *pData = vecBytes[i];

		// write each of the data items to the bytestream
		// also verify that the length of the data matches the size
		// NOTE: when upgrading from a 32 bit machine to a 64 bit machine, is where
		// we may find that existing data's sizes (from the old computer) will not
		// match the data sizes from the new computer.
		switch (eStorageType)
		{
		case kUnsignedChar:
			// verify data size is as expected
			if (ulDataSize != sizeof (unsigned char))
			{
				UCLIDException uclidEx("ELI00458", 
					"Internal error in ByteStreamManipulator::flushToByteStream() - kUnsignedChar!");
				uclidEx.addDebugInfo("ulDataSize", ValueTypePair((long)ulDataSize));
				throw uclidEx;
			}
			// copy the data
			memcpy(pszDataCursor, pData, ulDataSize);
			pszDataCursor += ulDataSize;
			break;
		case kLong:
			// verify data size is as expected
			if (ulDataSize != sizeof (long))
			{
				UCLIDException uclidEx("ELI00459", 
					"Internal error in ByteStreamManipulator::flushToByteStream() - kLong!");
				uclidEx.addDebugInfo("ulDataSize", ValueTypePair((long)ulDataSize));
				throw uclidEx;
			}
			// copy the data
			memcpy(pszDataCursor, pData, ulDataSize);
			pszDataCursor += ulDataSize;
			break;
		case kUnsignedLong:
			// verify data size is as expected
			if (ulDataSize != sizeof (unsigned long))
			{
				UCLIDException uclidEx("ELI00460", 
					"Internal error in ByteStreamManipulator::flushToByteStream() - kUnsignedLong!");
				uclidEx.addDebugInfo("ulDataSize", ValueTypePair((long)ulDataSize));
				throw uclidEx;
			}
			// copy the data
			memcpy(pszDataCursor, pData, ulDataSize);
			pszDataCursor += ulDataSize;
			break;
		case kLongLong:
			// verify data size is as expected
			if (ulDataSize != sizeof (__int64))
			{
				UCLIDException uclidEx("ELI28625", 
					"Internal error in ByteStreamManipulator::flushToByteStream() - kLongLong!");
				uclidEx.addDebugInfo("ulDataSize", ValueTypePair((long)ulDataSize));
				throw uclidEx;
			}
			// copy the data
			memcpy(pszDataCursor, pData, ulDataSize);
			pszDataCursor += ulDataSize;
			break;
		case kUnsignedShort:
			// verify data size is as expected
			if (ulDataSize != sizeof (unsigned short))
			{
				UCLIDException uclidEx("ELI06708", 
					"Internal error in ByteStreamManipulator::flushToByteStream() - kUnsignedShort!");
				uclidEx.addDebugInfo("ulDataSize", ValueTypePair((long)ulDataSize));
				throw uclidEx;
			}
			// copy the data
			memcpy(pszDataCursor, pData, ulDataSize);
			pszDataCursor += ulDataSize;
			break;
		case kDouble:
			// verify data size is as expected
			if (ulDataSize != sizeof (double))
			{
				UCLIDException uclidEx("ELI00461", 
					"Internal error in ByteStreamManipulator::flushToByteStream() - kDouble!");
				uclidEx.addDebugInfo("ulDataSize", ValueTypePair((long)ulDataSize));
				throw uclidEx;
			}
			// copy the data
			memcpy(pszDataCursor, pData, ulDataSize);
			pszDataCursor += ulDataSize;
			break;
		case kString:
			{
				// copy the string length
				memcpy(pszDataCursor, (char *) &ulDataSize, sizeof(ulDataSize));
				pszDataCursor += sizeof(ulDataSize);

				// copy the actual string data
				memcpy(pszDataCursor, pData, ulDataSize);
				pszDataCursor += ulDataSize;
			}
			break;
		case kBoolean:
			// verify data size is as expected
			if (ulDataSize != sizeof (bool))
			{
				UCLIDException uclidEx("ELI02267", 
					"Internal error in ByteStreamManipulator::flushToByteStream() - kBoolean!");
				uclidEx.addDebugInfo("ulDataSize", ValueTypePair((long)ulDataSize));
				throw uclidEx;
			}
			// copy the data
			memcpy(pszDataCursor, pData, ulDataSize);
			pszDataCursor += ulDataSize;
			break;
		case kByteStream:
			{
				// copy the bytestream length
				memcpy(pszDataCursor, (char *) &ulDataSize, sizeof(ulDataSize));
				pszDataCursor += sizeof(ulDataSize);

				// copy the data from the bytestream
				memcpy(pszDataCursor, pData, ulDataSize);
				pszDataCursor += ulDataSize;
			}
			break;
		case kGuid:
			// verify data size is as expected
			if (ulDataSize != sizeof(GUID))
			{
				UCLIDException uclidEx("ELI54043",
					"Internal error in ByteStreamManipulator::flushToByteStream() - kGuid!");
				uclidEx.addDebugInfo("ulDataSize", ValueTypePair((long)ulDataSize));
				throw uclidEx;
			}
			// copy the data
			memcpy(pszDataCursor, pData, ulDataSize);
			pszDataCursor += ulDataSize;
			break;
		default:
			// we should never reach here
			THROW_LOGIC_ERROR_EXCEPTION("ELI00462");
			break;
		};
	}

	// Ensure remaining space in buffer cleared; otherwise random chars may have unexpected effect
	// on encrypted output.
	memset(pszDataCursor, 0, ulUnusedTrailingChars);
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator& rManipulator, long lData)
{
	// this operator should only work in the kWrite mode
	if (rManipulator.eMode != ByteStreamManipulator::kWrite)
	{
		throw UCLIDException("ELI00463", 
			"ByteStreamManipulator: cannot call << operator on a read-only object!");
	}

	rManipulator.ulNumItems++;
	unsigned long ulDataSize = sizeof(lData);
	char *pData = new char[ulDataSize];
	memcpy(pData, &lData, ulDataSize);
	rManipulator.vecBytes.push_back(pData);
	rManipulator.vecLengths.push_back(ulDataSize);
	rManipulator.vecStorageTypes.push_back(ByteStreamManipulator::kLong);

	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator& rManipulator, __int64 llData)
{
	// this operator should only work in the kWrite mode
	if (rManipulator.eMode != ByteStreamManipulator::kWrite)
	{
		throw UCLIDException("ELI28626", 
			"ByteStreamManipulator: cannot call << operator on a read-only object!");
	}

	rManipulator.ulNumItems++;
	unsigned long ulDataSize = sizeof(llData);
	char *pData = new char[ulDataSize];
	memcpy(pData, &llData, ulDataSize);
	rManipulator.vecBytes.push_back(pData);
	rManipulator.vecLengths.push_back(ulDataSize);
	rManipulator.vecStorageTypes.push_back(ByteStreamManipulator::kLongLong);

	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator& rManipulator, 
									unsigned long ulData)
{
	// this operator should only work in the kWrite mode
	if (rManipulator.eMode != ByteStreamManipulator::kWrite)
	{
		throw UCLIDException("ELI00464", 
			"ByteStreamManipulator: cannot call << operator on a read-only object!");
	}

	rManipulator.ulNumItems++;
	unsigned long ulDataSize = sizeof(ulData);
	char *pData = new char[ulDataSize];
	memcpy(pData, &ulData, ulDataSize);
	rManipulator.vecBytes.push_back(pData);
	rManipulator.vecLengths.push_back(ulDataSize);
	rManipulator.vecStorageTypes.push_back(ByteStreamManipulator::kUnsignedLong);	

	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator& rManipulator, 
									unsigned short usData)
{
	// this operator should only work in the kWrite mode
	if (rManipulator.eMode != ByteStreamManipulator::kWrite)
	{
		throw UCLIDException("ELI06703", 
			"ByteStreamManipulator: cannot call << operator on a read-only object!");
	}

	rManipulator.ulNumItems++;
	unsigned long ulDataSize = sizeof(usData);
	char *pData = new char[ulDataSize];
	memcpy(pData, &usData, ulDataSize);
	rManipulator.vecBytes.push_back(pData);
	rManipulator.vecLengths.push_back(ulDataSize);
	rManipulator.vecStorageTypes.push_back(ByteStreamManipulator::kUnsignedShort);	

	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator& rManipulator, double dData)
{
	// this operator should only work in the kWrite mode
	if (rManipulator.eMode != ByteStreamManipulator::kWrite)
	{
		throw UCLIDException("ELI00465", 
			"ByteStreamManipulator: cannot call << operator on a read-only object!");
	}

	rManipulator.ulNumItems++;
	unsigned long ulDataSize = sizeof(dData);
	char *pData = new char[ulDataSize];
	memcpy(pData, &dData, ulDataSize);
	rManipulator.vecBytes.push_back(pData);
	rManipulator.vecLengths.push_back(ulDataSize);
	rManipulator.vecStorageTypes.push_back(ByteStreamManipulator::kDouble);	

	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator& rManipulator, 
									unsigned char ucData)
{
	// this operator should only work in the kWrite mode
	if (rManipulator.eMode != ByteStreamManipulator::kWrite)
	{
		throw UCLIDException("ELI00466", 
			"ByteStreamManipulator: cannot call << operator on a read-only object!");
	}

	rManipulator.ulNumItems++;
	unsigned long ulDataSize = sizeof(ucData);
	char *pData = new char[ulDataSize];
	memcpy(pData, &ucData, ulDataSize);
	rManipulator.vecBytes.push_back(pData);
	rManipulator.vecLengths.push_back(ulDataSize);
	rManipulator.vecStorageTypes.push_back(ByteStreamManipulator::kUnsignedChar);	

	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator& rManipulator, 
									const string& strData)
{
	// this operator should only work in the kWrite mode
	if (rManipulator.eMode != ByteStreamManipulator::kWrite)
	{
		throw UCLIDException("ELI00467", 
			"ByteStreamManipulator: cannot call << operator on a read-only object!");
	}

	rManipulator.ulNumItems++;
	unsigned long ulDataSize = strData.size();
	char *pData = new char[ulDataSize];
	memcpy(pData, strData.c_str(), ulDataSize);
	rManipulator.vecBytes.push_back(pData);
	rManipulator.vecLengths.push_back(ulDataSize);
	rManipulator.vecStorageTypes.push_back(ByteStreamManipulator::kString);	

	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator& rManipulator, 
									const CTime& Time)
{
	// this operator should only work in the kWrite mode
	if (rManipulator.eMode != ByteStreamManipulator::kWrite)
	{
		throw UCLIDException("ELI02236", 
			"ByteStreamManipulator: cannot call << operator on a read-only object!");
	}

	// Write the CTime object as an unsigned long time_t object
	// TESTTHIS
	rManipulator << (long) Time.GetTime();
	
	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator& rManipulator,
	const SYSTEMTIME& SystemTime)
{
	// this operator should only work in the kWrite mode
	if (rManipulator.eMode != ByteStreamManipulator::kWrite)
	{
		throw UCLIDException("ELI46477",
			"ByteStreamManipulator: cannot call << operator on a read-only object!");
	}

	FILETIME fileTime = { 0 };
	SystemTimeToFileTime(&SystemTime, &fileTime);

	rManipulator << fileTime.dwLowDateTime;
	rManipulator << fileTime.dwHighDateTime;

	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator& rManipulator, bool bData)
{
	// this operator should only work in the kWrite mode
	if (rManipulator.eMode != ByteStreamManipulator::kWrite)
	{
		throw UCLIDException("ELI02256", 
			"ByteStreamManipulator: cannot call << operator on a read-only object!");
	}

	rManipulator.ulNumItems++;
	unsigned long ulDataSize = sizeof(bData);
	char *pData = new char[ulDataSize];
	memcpy(pData, &bData, ulDataSize);
	rManipulator.vecBytes.push_back(pData);
	rManipulator.vecLengths.push_back(ulDataSize);
	rManipulator.vecStorageTypes.push_back(ByteStreamManipulator::kBoolean);

	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator& rManipulator, 
									const GUID & guidData)
{
	// this operator should only work in the kWrite mode
	if (rManipulator.eMode != ByteStreamManipulator::kWrite)
	{
		throw UCLIDException("ELI38812", 
			"ByteStreamManipulator: cannot call << operator on a read-only object!");
	}

	DWORD *pdwGUIDData = (DWORD *) &guidData;
	for (int i = 0; i < 4; i++)
	{
		rManipulator << pdwGUIDData[i];
	}
	
	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator << (ByteStreamManipulator& byteStreamManipulator, const NamedValueTypePair& namedPair)
{
	byteStreamManipulator << namedPair.GetName();
	ValueTypePair& valueTypePair = namedPair.GetPair();

	byteStreamManipulator << (unsigned long)valueTypePair.getType();

	switch (valueTypePair.getType())
	{
	case ValueTypePair::kString:
		byteStreamManipulator << valueTypePair.getStringValue();
		break;
	case ValueTypePair::kOctets:
		// TODO: octet streaming needs to be implemented.
		break;
	case ValueTypePair::kInt:
		byteStreamManipulator << (long)valueTypePair.getIntValue();
		break;
	case ValueTypePair::kInt64:
		byteStreamManipulator << valueTypePair.getInt64Value();
		break;
	case ValueTypePair::kLong:
		byteStreamManipulator << valueTypePair.getLongValue();
		break;
	case ValueTypePair::kUnsignedLong:
		byteStreamManipulator << valueTypePair.getUnsignedLongValue();
		break;
	case ValueTypePair::kDouble:
		byteStreamManipulator << valueTypePair.getDoubleValue();
		break;
	case ValueTypePair::kBoolean:
		byteStreamManipulator << (long)valueTypePair.getBooleanValue();
		break;
	case ValueTypePair::kGuid:
		byteStreamManipulator << valueTypePair.getGuidValue();
		break;
	default:
		// all other types are currently not supported.
		break;
	}

	return byteStreamManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator& rManipulator, long& rlData)
{
	// this operator should only work in the kRead mode
	if (rManipulator.eMode != ByteStreamManipulator::kRead)
	{
		throw UCLIDException("ELI00468", 
			"ByteStreamManipulator: cannot call >> operator on a write-only object!");
	}

	// ensure that the cursor is not null
	if (rManipulator.pszByteStreamCursor == NULL)
	{
		throw UCLIDException("ELI00469", 
			"Internal error in ByteStreamManipulator >> operator - rlData!");
	}

	// ensure that the required amount of bytes can be read.
	if (rManipulator.byteStream.getLength() - rManipulator.getCurPos() < sizeof(rlData))
	{
		throw UCLIDException("ELI00470", 
			"Internal error in ByteStreamManipulator >> operator - cannot read a long item!");
	}
	
	memcpy((char *) &rlData, (char *) rManipulator.pszByteStreamCursor, sizeof(rlData));
	rManipulator.pszByteStreamCursor += sizeof(rlData);
	
	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator& rManipulator, __int64& rllData)
{
	// this operator should only work in the kRead mode
	if (rManipulator.eMode != ByteStreamManipulator::kRead)
	{
		throw UCLIDException("ELI28627", 
			"ByteStreamManipulator: cannot call >> operator on a write-only object!");
	}

	// ensure that the cursor is not null
	if (rManipulator.pszByteStreamCursor == NULL)
	{
		throw UCLIDException("ELI28628", 
			"Internal error in ByteStreamManipulator >> operator - rlData!");
	}

	// ensure that the required amount of bytes can be read.
	if (rManipulator.byteStream.getLength() - rManipulator.getCurPos() < sizeof(rllData))
	{
		throw UCLIDException("ELI28629", 
			"Internal error in ByteStreamManipulator >> operator - cannot read a longlong item!");
	}
	
	memcpy((char *) &rllData, (char *) rManipulator.pszByteStreamCursor, sizeof(rllData));
	rManipulator.pszByteStreamCursor += sizeof(rllData);
	
	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator& rManipulator, 
									unsigned long& rulData)
{
	// this operator should only work in the kRead mode
	if (rManipulator.eMode != ByteStreamManipulator::kRead)
	{
		throw UCLIDException("ELI00471", 
			"ByteStreamManipulator: cannot call >> operator on a write-only object!");
	}

	// ensure that the cursor is not null
	if (rManipulator.pszByteStreamCursor == NULL)
	{
		throw UCLIDException("ELI00472", 
			"Internal error in ByteStreamManipulator >> operator - rulData!");
	}

	// ensure that the required amount of bytes can be read.
	if (rManipulator.byteStream.getLength() - rManipulator.getCurPos() < sizeof(rulData))
	{
		throw UCLIDException("ELI00473", 
			"Internal error in ByteStreamManipulator >> operator - cannot read an unsigned long item!");
	}
	
	memcpy(&rulData, rManipulator.pszByteStreamCursor, sizeof(rulData));
	rManipulator.pszByteStreamCursor += sizeof(rulData);
	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator& rManipulator, 
									unsigned short& rusData)
{
	// this operator should only work in the kRead mode
	if (rManipulator.eMode != ByteStreamManipulator::kRead)
	{
		throw UCLIDException("ELI06705", 
			"ByteStreamManipulator: cannot call >> operator on a write-only object!");
	}

	// ensure that the cursor is not null
	if (rManipulator.pszByteStreamCursor == NULL)
	{
		throw UCLIDException("ELI06706", 
			"Internal error in ByteStreamManipulator >> operator - rulData!");
	}

	// ensure that the required amount of bytes can be read.
	if (rManipulator.byteStream.getLength() - rManipulator.getCurPos() < sizeof(rusData))
	{
		throw UCLIDException("ELI06707", 
			"Internal error in ByteStreamManipulator >> operator - cannot read an unsigned short item!");
	}
	
	memcpy(&rusData, rManipulator.pszByteStreamCursor, sizeof(rusData));
	rManipulator.pszByteStreamCursor += sizeof(rusData);
	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator& rManipulator, double& rdData)
{
	// this operator should only work in the kRead mode
	if (rManipulator.eMode != ByteStreamManipulator::kRead)
	{
		throw UCLIDException("ELI00474", 
			"ByteStreamManipulator: cannot call >> operator on a write-only object!");
	}

	// ensure that the cursor is not null
	if (rManipulator.pszByteStreamCursor == NULL)
	{
		throw UCLIDException("ELI00475", 
			"Internal error in ByteStreamManipulator >> operator - rdData!");
	}

	// ensure that the required amount of bytes can be read.
	if (rManipulator.byteStream.getLength() - rManipulator.getCurPos() < sizeof(rdData))
	{
		throw UCLIDException("ELI00476", 
			"Internal error in ByteStreamManipulator >> operator - cannot read a double item!");
	}
	
	memcpy(&rdData, rManipulator.pszByteStreamCursor, sizeof(rdData));
	rManipulator.pszByteStreamCursor += sizeof(rdData);
	
	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator& rManipulator, 
									unsigned char& rucData)
{
	// this operator should only work in the kRead mode
	if (rManipulator.eMode != ByteStreamManipulator::kRead)
	{
		throw UCLIDException("ELI00477", 
			"ByteStreamManipulator: cannot call >> operator on a write-only object!");
	}

	// ensure that the cursor is not null
	if (rManipulator.pszByteStreamCursor == NULL)
	{
		throw UCLIDException("ELI00478", 
			"Internal error in ByteStreamManipulator >> operator - rucData!");
	}

	// ensure that the required amount of bytes can be read.
	if (rManipulator.byteStream.getLength() - rManipulator.getCurPos() < sizeof(rucData))
	{
		throw UCLIDException("ELI00479", 
			"Internal error in ByteStreamManipulator >> operator - cannot read an unsigned char item!");
	}
	
	memcpy(&rucData, rManipulator.pszByteStreamCursor, sizeof(rucData));
	rManipulator.pszByteStreamCursor += sizeof(rucData);
	
	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator& rManipulator, 
									string& rstrData)
{
	// this operator should only work in the kRead mode
	if (rManipulator.eMode != ByteStreamManipulator::kRead)
	{
		throw UCLIDException("ELI00480", 
			"ByteStreamManipulator: cannot call >> operator on a write-only object!");
	}

	// ensure that the cursor is not null
	if (rManipulator.pszByteStreamCursor == NULL)
	{
		throw UCLIDException("ELI00481", 
			"Internal error in ByteStreamManipulator >> operator - rstrdata!");
	}

	// ensure that the required amount of bytes can be read.
	// NOTE: for a string, the minimum number of bytes to be read is the size of unsigned long,
	// where the length of the string is represented.
	if (rManipulator.byteStream.getLength() - rManipulator.getCurPos() < sizeof(unsigned long))
	{
		throw UCLIDException("ELI00482", 
			"Internal error in ByteStreamManipulator >> operator - cannot read the length of the string item!");
	}
	
	unsigned long ulStringLength;
	memcpy(&ulStringLength, rManipulator.pszByteStreamCursor, sizeof(ulStringLength));
	rManipulator.pszByteStreamCursor += sizeof(ulStringLength);
	
	if (ulStringLength > 0)
	{
		// the string is not empty...
		// ensure that the required amount of bytes can be read
		if (rManipulator.byteStream.getLength() - rManipulator.getCurPos() < ulStringLength)
		{
			throw UCLIDException("ELI00483", 
				"Internal error in ByteStreamManipulator >> operator - cannot read the string item!");
		}

		// read the bytes out of the bytestream and update the string object
		rstrData = "";
		rstrData.append((const char *) rManipulator.pszByteStreamCursor, ulStringLength);
		rManipulator.pszByteStreamCursor += ulStringLength;
	}
	else
	{
		rstrData = "";
	}

	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator& rManipulator, 
									CTime& rTime)
{
	// this operator should only work in the kRead mode
	if (rManipulator.eMode != ByteStreamManipulator::kRead)
	{
		throw UCLIDException("ELI02237", 
			"ByteStreamManipulator: cannot call >> operator on a write-only object!");
	}

	// ensure that the cursor is not null
	if (rManipulator.pszByteStreamCursor == NULL)
	{
		throw UCLIDException("ELI02238", 
			"Internal error in ByteStreamManipulator >> operator - rTime!");
	}

	// Read the data
	// TESTTHIS
	long nTime;
	time_t time;
	rManipulator >> nTime;
	time = (time_t) nTime;

	// Convert time_t object to CTime object
	rTime = CTime(time);
	
	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator& rManipulator,
	SYSTEMTIME& rSystemTime)
{
	// this operator should only work in the kRead mode
	if (rManipulator.eMode != ByteStreamManipulator::kRead)
	{
		throw UCLIDException("ELI46475",
			"ByteStreamManipulator: cannot call >> operator on a write-only object!");
	}

	// ensure that the cursor is not null
	if (rManipulator.pszByteStreamCursor == NULL)
	{
		throw UCLIDException("ELI46476",
			"Internal error in ByteStreamManipulator >> operator - rSystemTime!");
	}

	FILETIME fileTime = { 0 };
	rManipulator >> fileTime.dwLowDateTime;
	rManipulator >> fileTime.dwHighDateTime;

	FileTimeToSystemTime(&fileTime, &rSystemTime);

	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator& rManipulator, bool& rbData)
{
	// this operator should only work in the kRead mode
	if (rManipulator.eMode != ByteStreamManipulator::kRead)
	{
		throw UCLIDException("ELI02257", 
			"ByteStreamManipulator: cannot call >> operator on a write-only object!");
	}

	// ensure that the cursor is not null
	if (rManipulator.pszByteStreamCursor == NULL)
	{
		throw UCLIDException("ELI02258", 
			"Internal error in ByteStreamManipulator >> operator - rbData!");
	}

	// ensure that 1 byte can be read.
	if (rManipulator.byteStream.getLength() - rManipulator.getCurPos() < 1)
	{
		throw UCLIDException("ELI02266", 
			"Internal error in ByteStreamManipulator >> operator - cannot read a single character!");
	}
	
	memcpy( &rbData, rManipulator.pszByteStreamCursor, sizeof(rbData) );
	rManipulator.pszByteStreamCursor += sizeof(rbData);
	
	return rManipulator;
}
//--------------------------------------------------------------------------------------------------
ByteStreamManipulator& operator >> (ByteStreamManipulator& rManipulator, 
									GUID& rguidData)
{
	// this operator should only work in the kRead mode
	if (rManipulator.eMode != ByteStreamManipulator::kRead)
	{
		throw UCLIDException("ELI38810", 
			"ByteStreamManipulator: cannot call >> operator on a write-only object!");
	}

	// ensure that the cursor is not null
	if (rManipulator.pszByteStreamCursor == NULL)
	{
		throw UCLIDException("ELI38811", 
			"Internal error in ByteStreamManipulator >> operator - rTime!");
	}

	// Read the data
	DWORD *pdwGUIDData = (DWORD *) &rguidData;
	for (int i = 0; i < 4; i++)
	{
		rManipulator >> pdwGUIDData[i];
	}

	return rManipulator;
}

ByteStreamManipulator& operator>>(ByteStreamManipulator& byteStreamManipulator, NamedValueTypePair& namedPair)
{
	string name;
	byteStreamManipulator >> name;

	unsigned long valueType;

	byteStreamManipulator >> valueType;
	ValueTypePair::EType eValueType = (ValueTypePair::EType)valueType;

	ValueTypePair valuePair;

	string strTemp;
	double dTemp;
	long lTemp;
	__int64 llTemp;
	unsigned long ulTemp;
	UUID guidTemp;

	switch (eValueType)
	{
	case ValueTypePair::kString:
		byteStreamManipulator >> strTemp;
		valuePair.setValue(strTemp);
		break;
	case ValueTypePair::kOctets:
		// TODO: octet streaming needs to be implemented.
		break;
	case ValueTypePair::kInt:
		byteStreamManipulator >> lTemp;
		valuePair.setIntValue(lTemp);
		break;
	case ValueTypePair::kInt64:
		byteStreamManipulator >> llTemp;
		valuePair.setValue(llTemp);
		break;
	case ValueTypePair::kLong:
		byteStreamManipulator >> lTemp;
		valuePair.setValue(lTemp);
		break;
	case ValueTypePair::kUnsignedLong:
		byteStreamManipulator >> ulTemp;
		valuePair.setValue(ulTemp);
		break;
	case ValueTypePair::kDouble:
		byteStreamManipulator >> dTemp;
		valuePair.setValue(dTemp);
		break;
	case ValueTypePair::kBoolean:
		byteStreamManipulator >> lTemp;
		valuePair.setValue((bool)(lTemp != 0));
		break;
	case ValueTypePair::kGuid:
		byteStreamManipulator >> guidTemp;
		valuePair.setValue(guidTemp);
		break;
	default:
		// all other types are currently not supported.
		break;
	}

	namedPair.SetName(name);
	namedPair.SetPair(valuePair);

	return byteStreamManipulator;
}

//--------------------------------------------------------------------------------------------------
unsigned long ByteStreamManipulator::getCurPos()
{
	// ensure that the cursor is not null
	if (pszByteStreamCursor == NULL)
	{
		throw UCLIDException("ELI00484", 
			"Internal error in ByteStreamManipulator::getCurPos()!");
	}

	return pszByteStreamCursor - byteStream.getData();
}
//--------------------------------------------------------------------------------------------------
void ByteStreamManipulator::read(ByteStream& rByteStream)
{
	// this operator should only work in the kRead mode
	if (eMode != ByteStreamManipulator::kRead)
	{
		throw UCLIDException("ELI07554", 
			"ByteStreamManipulator: cannot call read() on a write-only object!");
	}

	// ensure that the cursor is not null
	if (pszByteStreamCursor == NULL)
	{
		throw UCLIDException("ELI07555", 
			"Internal error in ByteStreamManipulator read()!");
	}

	// ensure that the required amount of bytes can be read.
	unsigned long ulByteStreamLength;
	if (byteStream.getLength() - getCurPos() < sizeof(ulByteStreamLength))
	{
		throw UCLIDException("ELI07556", 
			"Internal error in ByteStreamManipulator read()!");
	}

	// Get the data size
	memcpy(&ulByteStreamLength, pszByteStreamCursor, sizeof(ulByteStreamLength));
	pszByteStreamCursor += sizeof(ulByteStreamLength);
	
	if (ulByteStreamLength > 0)
	{
		// the bytestream is not empty...
		// ensure that the required amount of bytes can be read
		if (byteStream.getLength() - getCurPos() < ulByteStreamLength)
		{
			throw UCLIDException("ELI07562", 
				"Internal error in ByteStreamManipulator.read() - cannot read the ByteStream item!");
		}

		// read the bytes out of the bytestream and update the string object
		rByteStream.setSize(ulByteStreamLength);
		//unsigned char *pData = new unsigned char[ulByteStreamLength];
		//memcpy(pData, pszByteStreamCursor, ulByteStreamLength);
		memcpy(rByteStream.getData(), pszByteStreamCursor, ulByteStreamLength);
		pszByteStreamCursor += ulByteStreamLength;

		//rByteStream = ByteStream(pData, ulByteStreamLength);
	}
	else
	{
		rByteStream.setSize(0);
	}
}
//--------------------------------------------------------------------------------------------------
void ByteStreamManipulator::write(const ByteStream& bs)
{
	// this operator should only work in the kWrite mode
	if (eMode != ByteStreamManipulator::kWrite)
	{
		throw UCLIDException("ELI07553", 
			"ByteStreamManipulator: cannot call write() on a read-only object!");
	}

	// Update item counter and create data block for bytes
	ulNumItems++;
	unsigned long ulDataSize = bs.getLength();
	char *pData = new char[ulDataSize];
	
	// Copy the bytes into the data block
	memcpy( pData, bs.getData(), ulDataSize );

	// Update collections
	vecBytes.push_back( pData );
	vecLengths.push_back( ulDataSize );
	vecStorageTypes.push_back( ByteStreamManipulator::kByteStream );
}
//--------------------------------------------------------------------------------------------------
bool ByteStreamManipulator::IsEndOfStream()
{
	return byteStream.getLength() >= getCurPos();
}