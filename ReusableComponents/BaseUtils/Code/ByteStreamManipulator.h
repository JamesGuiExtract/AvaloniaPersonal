//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ByteStreamManipulator.h
//
// PURPOSE:	Definition of the ByteStreamManipulator class
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (August 2000 - present)
//
//==================================================================================================

#pragma once

//==================================================================================================
//== I N C L U D E S ===============================================================================
//==================================================================================================
#include "BaseUtils.h"
#include "ByteStream.h"

#include <string>
#include <vector>

using namespace std;


class EXPORT_BaseUtils ByteStreamManipulator
{
public:
	enum EMode {kRead, kWrite};
	enum EStorageType 
	{
		kUnsignedChar,
		kLong,
		kLongLong,
		kUnsignedLong,
		kDouble,
		kString,
		kBoolean,
		kUnsignedShort,
		kByteStream
	};

	// byteStream is the bytestream to read or write data from/to
	ByteStreamManipulator(EMode eMode, ByteStream& byteStream);
	~ByteStreamManipulator();

	// following methods only work in kWrite mode
	void flushToByteStream(unsigned long ulRequiredNumberToBeMultiplierOf = 1);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator << (ByteStreamManipulator& byteStreamManipulator, long lData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator << (ByteStreamManipulator& byteStreamManipulator, __int64 llData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator << (ByteStreamManipulator& byteStreamManipulator, unsigned long ulData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator << (ByteStreamManipulator& byteStreamManipulator, unsigned short usData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator << (ByteStreamManipulator& byteStreamManipulator, double dData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator << (ByteStreamManipulator& byteStreamManipulator, unsigned char ucData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator << (ByteStreamManipulator& byteStreamManipulator, const string& strData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator << (ByteStreamManipulator& byteStreamManipulator, const CTime& Time);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator << (ByteStreamManipulator& byteStreamManipulator, const SYSTEMTIME& Time);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator << (ByteStreamManipulator& byteStreamManipulator, bool bData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator << (ByteStreamManipulator& byteStreamManipulator, const GUID &guidData);

	// the following methods only work in kRead mode
	EXPORT_BaseUtils friend ByteStreamManipulator& operator >> (ByteStreamManipulator& byteStreamManipulator, long& rlData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator >> (ByteStreamManipulator& byteStreamManipulator, __int64& rllData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator >> (ByteStreamManipulator& byteStreamManipulator, unsigned long& rulData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator >> (ByteStreamManipulator& byteStreamManipulator, unsigned short& rusData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator >> (ByteStreamManipulator& byteStreamManipulator, double& rdData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator >> (ByteStreamManipulator& byteStreamManipulator, unsigned char& rucData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator >> (ByteStreamManipulator& byteStreamManipulator, string& rstrData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator >> (ByteStreamManipulator& byteStreamManipulator, CTime& rTime);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator >> (ByteStreamManipulator& byteStreamManipulator, SYSTEMTIME& rTime);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator >> (ByteStreamManipulator& byteStreamManipulator, bool& bData);
	EXPORT_BaseUtils friend ByteStreamManipulator& operator >> (ByteStreamManipulator& byteStreamManipulator, GUID& guidData);

	// public methods to read/write ByteStreams from/to ByteStreams
	void write(const ByteStream& bs);
	void read(ByteStream& rByteStream);

private:
	// following attributes used in both kRead and kWrite modes
	EMode eMode;
	ByteStream& byteStream;
	
	// following attributes only used ONLY in kWrite mode
	unsigned long ulNumItems;
	vector<char *> vecBytes;
	vector<unsigned long> vecLengths;
	vector<EStorageType> vecStorageTypes;

	// following attributes used in ONLY in kRead mode
	unsigned char *pszByteStreamCursor;
	unsigned long getCurPos();
};
