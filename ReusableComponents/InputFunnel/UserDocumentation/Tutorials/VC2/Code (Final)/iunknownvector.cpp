// Machine generated IDispatch wrapper class(es) created by Microsoft Visual C++

// NOTE: Do not modify the contents of this file.  If this class is regenerated by
//  Microsoft Visual C++, your modifications will be overwritten.


#include "stdafx.h"
#include "iunknownvector.h"


/////////////////////////////////////////////////////////////////////////////
// CIUnknownVector properties

/////////////////////////////////////////////////////////////////////////////
// CIUnknownVector operations

LPUNKNOWN CIUnknownVector::Clone()
{
	LPUNKNOWN result;
	InvokeHelper(0x2711, DISPATCH_METHOD, VT_UNKNOWN, (void*)&result, NULL);
	return result;
}

void CIUnknownVector::CopyFrom(LPUNKNOWN pObject)
{
	static BYTE parms[] =
		VTS_UNKNOWN;
	InvokeHelper(0x2712, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 pObject);
}

long CIUnknownVector::Size()
{
	long result;
	InvokeHelper(0x1, DISPATCH_METHOD, VT_I4, (void*)&result, NULL);
	return result;
}

LPUNKNOWN CIUnknownVector::At(long lPos)
{
	LPUNKNOWN result;
	static BYTE parms[] =
		VTS_I4;
	InvokeHelper(0x2, DISPATCH_METHOD, VT_UNKNOWN, (void*)&result, parms,
		lPos);
	return result;
}

void CIUnknownVector::PushBack(LPUNKNOWN pObj)
{
	static BYTE parms[] =
		VTS_UNKNOWN;
	InvokeHelper(0x3, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 pObj);
}

void CIUnknownVector::Clear()
{
	InvokeHelper(0x4, DISPATCH_METHOD, VT_EMPTY, NULL, NULL);
}

LPUNKNOWN CIUnknownVector::Front()
{
	LPUNKNOWN result;
	InvokeHelper(0x5, DISPATCH_METHOD, VT_UNKNOWN, (void*)&result, NULL);
	return result;
}

LPUNKNOWN CIUnknownVector::Back()
{
	LPUNKNOWN result;
	InvokeHelper(0x6, DISPATCH_METHOD, VT_UNKNOWN, (void*)&result, NULL);
	return result;
}

void CIUnknownVector::PopBack()
{
	InvokeHelper(0x7, DISPATCH_METHOD, VT_EMPTY, NULL, NULL);
}

void CIUnknownVector::Remove(long nIndex)
{
	static BYTE parms[] =
		VTS_I4;
	InvokeHelper(0x8, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 nIndex);
}

void CIUnknownVector::Append(LPDISPATCH pVector)
{
	static BYTE parms[] =
		VTS_DISPATCH;
	InvokeHelper(0x9, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 pVector);
}

void CIUnknownVector::Insert(long lPos, LPUNKNOWN pObj)
{
	static BYTE parms[] =
		VTS_I4 VTS_UNKNOWN;
	InvokeHelper(0xa, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 lPos, pObj);
}

void CIUnknownVector::Set(long lPos, LPUNKNOWN pObj)
{
	static BYTE parms[] =
		VTS_I4 VTS_UNKNOWN;
	InvokeHelper(0xb, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 lPos, pObj);
}

void CIUnknownVector::Swap(long lPos1, long lPos2)
{
	static BYTE parms[] =
		VTS_I4 VTS_I4;
	InvokeHelper(0xc, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 lPos1, lPos2);
}

BOOL CIUnknownVector::IsOrderFreeEqualTo(LPDISPATCH pVector)
{
	BOOL result;
	static BYTE parms[] =
		VTS_DISPATCH;
	InvokeHelper(0xd, DISPATCH_METHOD, VT_BOOL, (void*)&result, parms,
		pVector);
	return result;
}

long CIUnknownVector::FindByValue(LPUNKNOWN pObj, long nStartIndex)
{
	long result;
	static BYTE parms[] =
		VTS_UNKNOWN VTS_I4;
	InvokeHelper(0xe, DISPATCH_METHOD, VT_I4, (void*)&result, parms,
		pObj, nStartIndex);
	return result;
}

void CIUnknownVector::InsertVector(long lPos, LPDISPATCH pObj)
{
	static BYTE parms[] =
		VTS_I4 VTS_DISPATCH;
	InvokeHelper(0xf, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 lPos, pObj);
}

void CIUnknownVector::RemoveRange(long nStart, long nEnd)
{
	static BYTE parms[] =
		VTS_I4 VTS_I4;
	InvokeHelper(0x10, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 nStart, nEnd);
}

void CIUnknownVector::LoadFrom(LPCTSTR strFullFileName, BOOL bSetDirtyFlagToTrue)
{
	static BYTE parms[] =
		VTS_BSTR VTS_BOOL;
	InvokeHelper(0x11, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 strFullFileName, bSetDirtyFlagToTrue);
}

void CIUnknownVector::SaveTo(LPCTSTR strFullFileName, BOOL bClearDirty)
{
	static BYTE parms[] =
		VTS_BSTR VTS_BOOL;
	InvokeHelper(0x12, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 strFullFileName, bClearDirty);
}

void CIUnknownVector::RemoveValue(LPUNKNOWN pObj)
{
	static BYTE parms[] =
		VTS_UNKNOWN;
	InvokeHelper(0x13, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 pObj);
}

LPDISPATCH CIUnknownVector::At2(long lPos)
{
	LPDISPATCH result;
	static BYTE parms[] =
		VTS_I4;
	InvokeHelper(0x14, DISPATCH_METHOD, VT_DISPATCH, (void*)&result, parms,
		lPos);
	return result;
}

void CIUnknownVector::PushBackIfNotContained(LPUNKNOWN pObj)
{
	static BYTE parms[] =
		VTS_UNKNOWN;
	InvokeHelper(0x15, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 pObj);
}

void CIUnknownVector::FindByReference(LPUNKNOWN pObj, long nStartPos, long* pRetVal)
{
	static BYTE parms[] =
		VTS_UNKNOWN VTS_I4 VTS_PI4;
	InvokeHelper(0x16, DISPATCH_METHOD, VT_EMPTY, NULL, parms,
		 pObj, nStartPos, pRetVal);
}
