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
		// Rotate the bitmap.  Do not use ROTATE_RESIZE parameter: if we change the 
		// dimensions of the page we are working on, it may cause problems.
		nRet = L_RotateBitmap(&m_hBitmap, nRotation, 0, RGB(255, 255, 255));
		throwExceptionIfNotSuccess(nRet, "ELI22117", 
			"Internal error: Unable to apply rotation to image!", m_strImageFileName);
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