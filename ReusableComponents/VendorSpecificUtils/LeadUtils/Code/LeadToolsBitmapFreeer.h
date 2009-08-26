
#pragma once

#include "LeadUtils.h"
#include "MiscLeadUtils.h"

#include <l_bitmap.h>		// LeadTools Imaging library

//-------------------------------------------------------------------------------------------------
class LEADUTILS_API LeadToolsBitmapFreeer
{
public:
	LeadToolsBitmapFreeer(BITMAPHANDLE& bitmapHandle, bool bInitializeBMH = false)
	:m_bitmapHandle(bitmapHandle)
	{
		// Initialize the BITMAPHANDLE if desired
		if (bInitializeBMH)
		{
			L_InitBitmap( &m_bitmapHandle, sizeof( BITMAPHANDLE ), 0, 0, 0 );
		}
	}

	~LeadToolsBitmapFreeer()
	{
		// Free the BITMAPHANDLE only if it has been allocated
		if (m_bitmapHandle.Flags.Allocated)
		{
			L_FreeBitmap(&m_bitmapHandle);
		}
	}

private:
	BITMAPHANDLE& m_bitmapHandle;
};
//-------------------------------------------------------------------------------------------------
