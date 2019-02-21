#include "stdafx.h"
#include "LeadToolsBitmap.h"
#include "MiscLeadUtils.h"

#include <UCLIDException.h>
#include "LeadToolsLicenseRestrictor.h"

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Minimum hundredths of a degree of rotation requested for rotation to be applied in
// LeadToolsBitmap constructor
const L_INT gnMIN_ROTATION = 10;

// A COLORREF representation of black.
const COLORREF gnCOLOR_BLACK = RGB(0, 0, 0);

//-------------------------------------------------------------------------------------------------
// LeadToolsBitmap
//-------------------------------------------------------------------------------------------------
LeadToolsBitmap::LeadToolsBitmap(const string strImageFileName, unsigned long ulPage, 
								 double dRotation/* = 0*/, int nBitsPerPixel/* = 1*/,
								 bool bUseDithering/* = true*/, bool bUseAdaptiveThresholdToConvertToBitonal/* = false*/)
: m_bitmapFreeer(m_hBitmap, true)
, m_strImageFileName(strImageFileName)
{
	try
	{
		// Initialize FILEINFO and LOADFILEOPTION for L_LoadBitmap call
		L_INT nRet;
		m_FileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);
		LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_IGNOREVIEWPERSPECTIVE);
		lfo.PageNumber = ulPage;

		// Load the image using the original bits per pixel and then convert so as to not be affected by currently
		// configured dithering method
		// https://extract.atlassian.net/browse/ISSUE-14596
		loadImagePage(m_strImageFileName, m_hBitmap, m_FileInfo, lfo);
		
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;

		// Convert to specified bpp if necessary
		if (nBitsPerPixel != m_FileInfo.BitsPerPixel)
		{
			// If specified, when converting from color/grayscale to bitonal use an adaptive threshold algorithm
			// https://extract.atlassian.net/browse/ISSUE-14842
			if (bUseAdaptiveThresholdToConvertToBitonal && nBitsPerPixel == 1)
			{
				unlockDocumentSupport();
				nRet = L_AutoBinarizeBitmap(&m_hBitmap, 0, AUTO_BINARIZE_PRE_AUTO | AUTO_BINARIZE_THRESHOLD_AUTO);
				throwExceptionIfNotSuccess(nRet, "ELI44666",
					"Internal error: Unable to binarize image!", strImageFileName);
			}

			const long nDEFAULT_NUMBER_OF_COLORS = 0;
			L_UINT flags = CRF_FIXEDPALETTE | (bUseDithering ? CRF_ORDEREDDITHERING : CRF_NODITHERING);
			nRet = L_ColorResBitmap(&m_hBitmap, &m_hBitmap, sizeof(BITMAPHANDLE), nBitsPerPixel, flags,
				NULL, NULL, nDEFAULT_NUMBER_OF_COLORS, NULL, NULL);

			throwExceptionIfNotSuccess(nRet, "ELI42166",
				"Internal error: Unable to convert image to specified bits-per-pixel!", strImageFileName);
		}

		// L_RotateBitmap takes the rotation degrees in hundredths of a degree.  Convert
		// to hundredths of a degree
		L_INT nRotation = (L_INT)(dRotation * 100.0);

		// If the specified rotation is >= gnMIN_ROTATION, rotate the image as requested 
		if (abs(nRotation) >= gnMIN_ROTATION)
		{
			// Determine if the image is to be rotated such that it is to be closer to perpendicular
			// to the original orientation than parallel.
			bool perpendicular = (round(dRotation / 90) % 2 != 0);
			CSize sizeOrig(m_hBitmap.Width, m_hBitmap.Height);

			// Rotate the bitmap.
			// Allow resizing of the image if it is to be rotated perpendicular to the original
			// orientation-- otherwise unless the image is square, text is likely to extend out of the
			// image bounds in one direction while there will be empty whitespace at either end in the
			// other direction.
			nRet = L_RotateBitmap(&m_hBitmap, nRotation, perpendicular ? ROTATE_RESIZE : 0, 
				RGB(255, 255, 255));
			throwExceptionIfNotSuccess(nRet, "ELI22117", 
				"Internal error: Unable to apply rotation to image.", m_strImageFileName);

			// If the image was rotated and allowed to be resized, it needs to be trimmed back to the
			// original page dimensions so that OCR coordinates remain valid. (Otherwise they will
			// be offset by the amount of space that was added to the left & top edges of the image.
			if (perpendicular)
			{
				int	nXTrimAmount = (m_hBitmap.Width - sizeOrig.cy) / 2;
				int	nYTrimAmount = (m_hBitmap.Height - sizeOrig.cx) / 2;

				nRet = L_TrimBitmap(&m_hBitmap, nXTrimAmount, nYTrimAmount,
					m_hBitmap.Width - (2 * nXTrimAmount), m_hBitmap.Height - (2 * nYTrimAmount));
				throwExceptionIfNotSuccess(nRet, "ELI27740", 
					"Internal error: Unable to trim rotated image.", m_strImageFileName);
			}
		}

		// Set the image palette to white, black to ensure consistency in reading image data
		// (0 = white, 1 = black)
		L_RGBQUAD palette[2] = { {255, 255, 255, 0}, {0, 0, 0, 0} };
		nRet = L_ColorResBitmap(&m_hBitmap, &m_hBitmap, sizeof(BITMAPHANDLE), 
			m_hBitmap.BitsPerPixel,	CRF_USERPALETTE, palette, NULL, 2, NULL, NULL);
		throwExceptionIfNotSuccess(nRet, "ELI22238", 
			"Internal error: Failed to set image palette.", m_strImageFileName);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32213");
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsBitmap::isPixelBlack(CPoint point)
{
	return isPixelBlack(point.x, point.y);
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsBitmap::isPixelBlack(int x, int y)
{
	return L_GetPixelColor(&m_hBitmap, y, x) == gnCOLOR_BLACK;
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsBitmap::contains(CPoint point)
{
	return contains(point.x, point.y);
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsBitmap::contains(int x, int y)
{
	return x >= 0 && y >= 0 && x < m_FileInfo.Width && y < m_FileInfo.Height;
}
//-------------------------------------------------------------------------------------------------