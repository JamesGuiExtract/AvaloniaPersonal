
#pragma once

#include "LeadUtils.h"
#include "LeadToolsBitmapFreeer.h"

//-------------------------------------------------------------------------------------------------
// LeadToolsBitmap
//-------------------------------------------------------------------------------------------------
// This class simplifies the initialization and cleanup of an image opened with leadtools.
// It will automatically initialize and free resources as needed.
class LEADUTILS_API LeadToolsBitmap
{
public:
	LeadToolsBitmap(const string strImageFileName, unsigned long ulPage, double dRotation = 0,
		int nBitsPerPixel = 1, bool bUseDithering = true, bool bUseAdaptiveThresholdToConvertToBitonal = false);

	// Returns the value of the pixel at the given point (true = black, false = white)
	// NOTE: The caller of this function should protect with LeadToolsLicenseRestrictor
	bool isPixelBlack(CPoint point);
	bool isPixelBlack(int x, int y);

	// Returns true if the point exists on the bitmap; false if it is off the image.
	bool contains(CPoint point);
	bool contains(int x, int y);

	// Provide access to the bitmap information.
	BITMAPHANDLE m_hBitmap;
	FILEINFO m_FileInfo;

private:

	string m_strImageFileName;

	// The LeadToolsBitmapFreeer will initialize m_hBitmap upon construction and
	// free m_hBitmap as this class is destroyed.
	LeadToolsBitmapFreeer m_bitmapFreeer;
};
//-------------------------------------------------------------------------------------------------