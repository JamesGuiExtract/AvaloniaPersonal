#include "stdafx.h"
#include "LeadToolsBitmap.h"
#include "MiscLeadUtils.h"
#include "PDFInputOutputMgr.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Minimum hundredths of a degree of rotation requested for rotation to be applied in
// LeadToolsBitmap constructor
const L_INT gnMIN_ROTATION = 10;

// The most significant bit of an unsigned char
const UCHAR gucFIRST_BIT = 0x80;

//-------------------------------------------------------------------------------------------------
// LeadToolsBitmap
//-------------------------------------------------------------------------------------------------
LeadToolsBitmap::LeadToolsBitmap(const string strImageFileName, unsigned long ulPage, 
								 double dRotation/* = 0*/)
: m_bitmapFreeer(m_hBitmap, true)
, m_strImageFileName(strImageFileName)
{
	// Initialize FILEINFO and LOADFILEOPTION for L_LoadBitmap call
	m_FileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);
	LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_IGNOREVIEWPERSPECTIVE);
	lfo.PageNumber = ulPage;

	// Convert the PDF input image to a temporary TIF
	PDFInputOutputMgr ltPDF(m_strImageFileName, true);

	// Load the image using the specified number of bits per pixel.
	L_INT nRet = L_LoadBitmap(_bstr_t(ltPDF.getFileName().c_str()), &m_hBitmap, 
		sizeof(BITMAPHANDLE), 1, ORDER_RGB, &lfo, &m_FileInfo);
	throwExceptionIfNotSuccess(nRet, "ELI22243", 
		"Internal error: Unable to load image!", m_strImageFileName);
	
	// ViewPerspective appears to be valid in situations where ELO_IGNOREVIEWPERSPECTIVE
	// does not work.  (For instance bmps can be loaded with a valid BOTTOM_LEFT view perspective,
	// but ELO_IGNOREVIEWPERSPECTIVE will not ignore that view perspective).  For that reason, 
	// compensate for view perspective.  It's important to use m_FileInfo's ViewPerspective here
	// rather than m_hBitmap's.  
	if (m_FileInfo.ViewPerspective != TOP_LEFT)
	{
		nRet = L_ChangeBitmapViewPerspective(&m_hBitmap, &m_hBitmap, sizeof(BITMAPHANDLE), TOP_LEFT);
		throwExceptionIfNotSuccess(nRet, "ELI22244", 
			"Internal error: ChangeBitmapViewPerspective operation failed!", m_strImageFileName); 
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
			"Internal error: Unable to apply rotation to image!", m_strImageFileName);

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
				"Internal error: Unable to trim rotated image!", m_strImageFileName);
		}
	}

	// Set the image palette to white, black to ensure consistency in reading image data
	// (0 = white, 1 = black)
	L_RGBQUAD palette[2] = { {255, 255, 255, 0}, {0, 0, 0, 0} };
	nRet = L_ColorResBitmap(&m_hBitmap, &m_hBitmap, sizeof(BITMAPHANDLE), 
		m_hBitmap.BitsPerPixel,	CRF_USERPALETTE, palette, NULL, 2, NULL, NULL);
	throwExceptionIfNotSuccess(nRet, "ELI22238", 
		"Internal error: Failed to set image palette!", m_strImageFileName);
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsBitmap::isPixelBlack(CPoint point)
{
	return isPixelBlack(point.x, point.y);
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsBitmap::isPixelBlack(int x, int y)
{
	// Get a pointer to the character containing the pixel we are looking for.
	UCHAR *pChar = m_hBitmap.Addr.Windows.pData + y * m_hBitmap.BytesPerLine + x / 8;
			
	// Set a mask to obtain the bit we need.
	UCHAR ucMask = gucFIRST_BIT >> (x % 8);

	// Return the value of the bit of at this position
	return (*pChar & ucMask) != 0;
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