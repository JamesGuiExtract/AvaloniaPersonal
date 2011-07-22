
#pragma once

#include "LeadUtils.h"

#include <TemporaryFileName.h>
#include <UCLIDException.h>
#include <l_bitmap.h>		// LeadTools Imaging library

#include <string>
#include <vector>
#include <map>

using namespace std;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// constant for the number of retries on saving a page before failing
const int gnNUMBER_ATTEMPTS_BEFORE_FAIL = 3;

// constant for the length of time to sleep before retrying a page save operation
// (currently only used in the getNumberOfPagesInImage method
const int gnSLEEP_BETWEEN_RETRY_MS = 200;

// number of times to retry saving an output image if the page count is wrong (P16 #2593)
const int gnOUTPUT_IMAGE_RETRIES = 3;

//--------------------------------------------------------------------------------------------------
// Structs
//--------------------------------------------------------------------------------------------------
// Structure that defines a raster zone on a page with a specified line color and border color
class PageRasterZone
{
public:
	// Initialize struct to empty data
	PageRasterZone()
		: m_nStartX(0),
          m_nStartY(0),
          m_nEndX(0),
          m_nEndY(0),
          m_nHeight(0),
          m_nPage(0),
          m_strText(""),
          m_crFillColor(0),
          m_crBorderColor(0),
          m_crTextColor(0),
          m_iPointSize(0)
	{
		memset(&m_font, 0, sizeof(LOGFONT));
	}
	
	bool isEmptyZone() const
	{
		return m_nPage == 0 && m_nStartX == 0 && m_nStartY == 0 && m_nEndX == 0 && m_nEndY == 0;
	}

	long m_nStartX; 
	long m_nStartY; 
	long m_nEndX; 
	long m_nEndY; 
	long m_nHeight; 
	long m_nPage;
	string m_strText;
	LOGFONT m_font;
	int m_iPointSize;
	COLORREF m_crFillColor;
	COLORREF m_crBorderColor;
	COLORREF m_crTextColor;
};

//------------------------------------------------------------------------------------------------
// Class used to maintain a collection of Brushes that can be reused
class BrushCollection
{
public:
	// Initializes an empty brush collection
	BrushCollection() {}
	
	// Release all of the brush objects
	~BrushCollection()
	{
		try
		{
			for(map<COLORREF, HBRUSH>::iterator it = m_mapColorToBrush.begin();
				it != m_mapColorToBrush.end(); it++)
			{
				DeleteObject(it->second);
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI23574");
	}

	// Gets a specific color brush from the collection (creating it if necessary)
	HBRUSH getColoredBrush(COLORREF color)
	{
		// Find the color in the map
		map<COLORREF, HBRUSH>::iterator it = m_mapColorToBrush.find(color);

		// Get the brush from the map if it was found, otherwise create a new brush,
		// add it to the map and then return the new brush
		HBRUSH hBrush = NULL;
		if (it == m_mapColorToBrush.end())
		{
			// Create the brush
			hBrush = CreateSolidBrush(color);

			// Ensure the brush was created
			if (hBrush == NULL)
			{
				UCLIDException ue("ELI27295", "Unable to create brush!");
				ue.addWin32ErrorInfo();
				throw ue;
			}

			m_mapColorToBrush[color] = hBrush;
		}
		else
		{
			hBrush = it->second;
		}

		return hBrush;
	}

private:
	map<COLORREF, HBRUSH> m_mapColorToBrush;
};

//------------------------------------------------------------------------------------------------
// Class used to maintain a collection of pens that can be reused
class PenCollection
{
public:
	// Initializes an empty pen collection
	PenCollection() {}

	// Release all of the allocated pen objects
	~PenCollection()
	{
		try
		{
			for(map<COLORREF, HPEN>::iterator it = m_mapColorToPen.begin();
				it != m_mapColorToPen.end(); it++)
			{
				DeleteObject(it->second);
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI23575");
	}

	// Gets a specific color brush from the collection (creating it if necessary)
	HPEN getColoredPen(COLORREF color)
	{
		// Find the color in the map
		map<COLORREF, HPEN>::iterator it = m_mapColorToPen.find(color);

		// Get the pen from the map if it was found, otherwise create a new brush,
		// add it to the map and then return the new brush
		HPEN hPen = NULL;
		if (it == m_mapColorToPen.end())
		{
			hPen = CreatePen(PS_SOLID, 1, color);

			// Ensure the pen was created
			if (hPen == NULL)
			{
				UCLIDException ue("ELI27296", "Unable to create pen!");
				ue.addWin32ErrorInfo();
				throw ue;
			}

			m_mapColorToPen[color] = hPen;
		}
		else
		{
			hPen = it->second;
		}

		return hPen;
	}

private:
	map<COLORREF, HPEN> m_mapColorToPen;
};

//------------------------------------------------------------------------------------------------
// PURPOSE: Retrieves a user friendly description of the specified Leadtools error code.
LEADUTILS_API string getErrorCodeDescription(int iErrorCode);
//------------------------------------------------------------------------------------------------
// PURPOSE: To throw an UCLIDException if iErrorCode is not SUCCESS.
LEADUTILS_API void throwExceptionIfNotSuccess(int iErrorCode, 
	const string& strELICode, const string& strErrorDescription,
	const string& strFileName = "");
//-------------------------------------------------------------------------------------------------
// PROMISE: To fill (i.e. redact) an area of the image with the specified color.
LEADUTILS_API void fillImageArea(const string& strImageFileName, 
								 const string& strOutputImageName,
								 long nLeft, long nTop, long nRight, long nBottom, 
								 long nPage, const COLORREF color, bool bRetainAnnotations, 
								 bool bApplyAsAnnotations,
								 const string& strUserPassword = "",
								 const string& strOwnerPassword = "",
								 int nPermissions = 0);
//-------------------------------------------------------------------------------------------------
LEADUTILS_API void fillImageArea(const string& strImageFileName,
								 const string& strOuputImageName,
								 long nLeft, long nTop, long nRight, long nBottom,
								 long nPage, const COLORREF crFillColor,
								 const COLORREF crBorderColor,
								 const string& strText,
								 const COLORREF crTextColor,
								 bool bRetainAnnotations, 
								 bool bApplyAsAnnotations,
								 const string& strUserPassword = "",
								 const string& strOwnerPassword = "",
								 int nPermissions = 0);
//-------------------------------------------------------------------------------------------------
// PROMISE: To fill (i.e. redact) all areas of the image specified in specified in vecZones,
//			with the color and text specified within each zone
// ARGS:	vecFillAreas contains multiple FillAreaStruct elements to fill
// NOTE:	rvecZones will be sorted by page number after a call to this method
LEADUTILS_API void fillImageArea(const string& strImageFileName, 
								 const string& strOutputImageName,
								 vector<PageRasterZone>& rvecZones, 
								 bool bRetainAnnotations, 
								 bool bApplyAsAnnotations,
								 const string& strUserPassword = "",
								 const string& strOwnerPassword = "",
								 int nPermissions = 0);
//-------------------------------------------------------------------------------------------------
// PROMISE: To take a vector of image file names and combine them into a multipage file with
//			the name of strOutputFileName
LEADUTILS_API void createMultiPageImage(vector<string> vecImageFiles, 
										string strOutputFileName,
										bool bOverwriteExistingFile);
//-------------------------------------------------------------------------------------------------
// Gets the file info from the specified image file. (NOTE: This will zero out the passed in
// FILEINFO and will fill it with information from the specified file name)
LEADUTILS_API void getFileInformation(const string& strFileName, bool includePageCount,
	FILEINFO& rFileInfo, LOADFILEOPTION* pLfo = __nullptr);
//-------------------------------------------------------------------------------------------------
// PROMISE: To take an image file name and return the number of pages in the image
LEADUTILS_API int getNumberOfPagesInImage( const string& strImageFileName );
//-------------------------------------------------------------------------------------------------
// PROMISE: To fill the riXResolution and riYResolution variables with the X and Y resolution
//			of the specified image [p13 #4809]
LEADUTILS_API void getImageXAndYResolution(const string& strImageFileName, int& riXResolution,
										   int& riYResolution, int nPageNum = 1);
//-------------------------------------------------------------------------------------------------
// PROMISE: To fill the riHeight and riWidth variables with the height and width
//			of the specified one-based page of an image in pixels [p13 #4809]
LEADUTILS_API void getImagePixelHeightAndWidth(const string& strImageFileName, int& riHeight, 
											   int& riWidth, int nPageNum = 1);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the handle of a bitmap for the given page in the file
// the bitmap will have to be freed by caller
LEADUTILS_API void loadImagePage(const string& strImageFileName, unsigned long ulPage, 
								 BITMAPHANDLE &rBitmap, bool bChangeViewPerspective = true);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the handle of a bitmap for the given page in the file
// the bitmap will have to be freed by caller, the rflInfo will be filled
// with file information after this call
LEADUTILS_API void loadImagePage(const string& strImageFileName, unsigned long ulPage,
								 BITMAPHANDLE &rBitmap, FILEINFO& rflInfo, bool bChangeViewPerspective = true);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the handle of a bitmap for the specified file loaded based upon
//			the provided LOADFILEOPTION (will load the page set in the lfo.PageNumber field)
//			the bitmap will have to be freed by caller
LEADUTILS_API void loadImagePage(const string& strImageFileName, BITMAPHANDLE &rBitmap,
								 FILEINFO& rflInfo, LOADFILEOPTION& lfo,
								 bool bChangeViewPerspective = true);
//-------------------------------------------------------------------------------------------------
// PROMISE: To save the bitmap to the specified image at the specified page number
// NOTE:	This code will internally perform multiple retry attempts when saving the page.
LEADUTILS_API void saveImagePage(BITMAPHANDLE& hBitmap, const string& strOutputFile,
								 FILEINFO& flInfo, long lPageNumber);
//-------------------------------------------------------------------------------------------------
// PROMISE: To save the bitmap to the specified image based on the SAVEFILEOPTION struct.
// NOTE:	This code will internally perform multiple retry attempts when saving the page.
LEADUTILS_API void saveImagePage(BITMAPHANDLE& hBitmap, const string& strOutputFile,
								 FILEINFO& flInfo, SAVEFILEOPTION& sfo);
//-------------------------------------------------------------------------------------------------
// PROMISE: To save the bitmap to the specified image based on the SAVEFILEOPTION struct.
//			If bLockPDF == true then a LeadtoolsPDFLoadLocker will be instantiated.
// NOTE:	This code will internally perform multiple retry attempts when saving the page.
LEADUTILS_API void saveImagePage(BITMAPHANDLE& hBitmap, const string& strOutputFile,
								 int nFileFormat, int nCompressionFactor, int nBitsPerPixel,
								 SAVEFILEOPTION& sfo);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the color of the designated pixel
LEADUTILS_API COLORREF getPixelColor(BITMAPHANDLE &rBitmap, int iRow, int iCol);
//-------------------------------------------------------------------------------------------------
// PROMISE: To unlock PDF Read/Write capabilities and to set the default open Resolution,
//          if and only if PDF Read/Write support is licensed.
LEADUTILS_API void initPDFSupport();
//-------------------------------------------------------------------------------------------------
// PROMISE: Returns ViewPerspective field from the FILEINFO structure.
//			1 = TOP_LEFT
//			2 = TOP_RIGHT
//			3 = BOTTOM_RIGHT
//			4 = BOTTOM_LEFT
//			5 = LEFT_TOP
//			6 = RIGHT_TOP or TOP_LEFT90 ( rotated 90 degrees clockwise )
//			7 = RIGHT_BOTTOM
//			8 = LEFT_BOTTOM or TOP_LEFT270 ( rotated 270 degrees clockwise )
// NOTE:	nPageNum is a 1-based page number.
LEADUTILS_API int getImageViewPerspective(const string& strImageFileName, int nPageNum);
//-------------------------------------------------------------------------------------------------
// PROMISE: To unlock the LeadTools Document Toolkit if Annotation support is licensed.
LEADUTILS_API void unlockDocumentSupport();
//-------------------------------------------------------------------------------------------------
// PROMISE: Returns true if all L_xxx() calls to LeadTools functions will be serialized via 
//			CMutex
LEADUTILS_API bool isLeadToolsSerialized();
//-------------------------------------------------------------------------------------------------
// PURPOSE: To convert a TIF image into a PDF image.  This function does not return 
//			until the conversion is complete.  If the TIF was from TemporaryFileName, auto-deletion 
//			when the variable goes out of scope is acceptable.
//			If bRetainAnnotations is true then any redaction type annotations in the tif will
//			be burned into the PDF [FIDSC #3131 - JDS - 12/17/2008].
LEADUTILS_API void convertTIFToPDF(const string& strTIF, const string& strPDF,
								   bool bRetainAnnotations = false,
								   const string& strUserPassword = "",
								   const string& strOwnerPassword = "", int nPermissions = 0);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To convert a PDF image into a TIF image.  This function does not return 
//			until the conversion is complete.
LEADUTILS_API void convertPDFToTIF(const string& strPDF, const string& strTIF);
//-------------------------------------------------------------------------------------------------
// PROMISE: To calculate the 4 corner points of the raster zone given in rZone
//			aPoints[0] = point above start point
//			aPoints[1] = point above end point
//			aPoints[2] = point below end point
//			aPoints[3] = point below start point
LEADUTILS_API void pageZoneToPoints(const PageRasterZone &rZone, POINT &p1, POINT &p2, POINT &p3, 
									POINT &p4);
//-------------------------------------------------------------------------------------------------
// PROMISE: Returns true if the Leadtools file format number corresponds to a tiff file.
LEADUTILS_API bool isTiff(int iFormat);
LEADUTILS_API bool isTiff(const string& strImageFile);
//-------------------------------------------------------------------------------------------------
// PROMISE: Returns true if the Leadtools file format number corresponds to a PDF file.
LEADUTILS_API bool isPDF(int iFormat);
LEADUTILS_API bool isPDF(const string& strImageFile);
//-------------------------------------------------------------------------------------------------
// PROMISE: Returns true if the specified file page has an annotations tag, false otherwise.
LEADUTILS_API bool hasAnnotations(const string& strFilename, LOADFILEOPTION &lfo, int iFileFormat);
LEADUTILS_API bool hasAnnotations(const string& strFilename, int iPageNumber);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the compression factor for a particular file format based on the
//			value in the registry.
LEADUTILS_API int getCompressionFactor(const string& strFormat);
LEADUTILS_API int getCompressionFactor(int nFormat);
//-------------------------------------------------------------------------------------------------
// PROMISE: To draw the specified redaction zone on the specified device context
LEADUTILS_API void drawRedactionZone(HDC hDC, const PageRasterZone& rZone, int nYResolution,
									 BrushCollection& rBrushes, PenCollection& rPens);
LEADUTILS_API void drawRedactionZone(HDC hDC, const PageRasterZone& rZone, int nYResolution);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To create a device context and assign it to hDC.  If hDC is not NULL no new context
//			will be created.
LEADUTILS_API void createLeadDC(HDC& hDC, BITMAPHANDLE& hBitmap);
//-------------------------------------------------------------------------------------------------
LEADUTILS_API void deleteLeadDC(HDC& hDC);
//-------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------
// Sets the PDF save options for the current thread to the specified settings
// Reverts the settings when the class is destructed
class LEADUTILS_API PDFSecuritySettings
{
public:
	PDFSecuritySettings(const string& strUserPassword, const string& strOwnerPassword,
		long nPermissions, bool bSetPDFLoadOptions = false);
	~PDFSecuritySettings();

private:
	bool m_bSetLoadOptions;

	unique_ptr<FILEPDFSAVEOPTIONS> m_pOriginalOptions;

	unique_ptr<FILEPDFOPTIONS> m_pOriginalLoadOptions;

	void setPDFSaveOptions(const string& strUserPassword,
		const string& strOwnerPassword, long nPermissions);

	unsigned int getLeadtoolsPermissions(long nSecuritySettings);
};

//-------------------------------------------------------------------------------------------------
// Class used to manage a lead tools device context object
class LeadtoolsDCManager
{
public:
	// The device context that this class is managing
	HDC m_hDC;

	LeadtoolsDCManager() : m_hDC(NULL)
	{
	}

	LeadtoolsDCManager(BITMAPHANDLE& hBitmap) : m_hDC(NULL)
	{
		try
		{
			createLeadDC(m_hDC, hBitmap);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28153");
	}

	void createFromBitmapHandle(BITMAPHANDLE& hBitmap)
	{
		try
		{
			// Delete any existing handle
			deleteLeadDC(m_hDC);

			// Create the new handle
			createLeadDC(m_hDC, hBitmap);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28154");
	}

	void deleteDC()
	{
		try
		{
			deleteLeadDC(m_hDC);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28155");
	}

	~LeadtoolsDCManager()
	{
		try
		{
			deleteLeadDC(m_hDC);
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28129");
	}
};

//-------------------------------------------------------------------------------------------------
// PURPOSE: To initialize a LeadTools sized struct.
// ARGS:	The value to which the Flags parameter should be initialized
// Coded with the structures we currently use in mind:
// FILEINFO, LOADFILEOPTION, SAVEFILEOPTION, FILEPDFOPTIONS
// [Note: FILEPDFOPTIONS does not contain member Flag, so incoming arg is unused]
// Should work with any LeadTools struct that needs zero initialization
// keeping an eye open for any additional parameters that require initialization
// or that don't contain uStructSize (in which case a runtime exception will be thrown)
template <typename T>
T GetLeadToolsSizedStruct(unsigned int Flags)
{
	// Create the struct and zero the memory
	T result;
	memset( &result, 0, sizeof( T ) );

	// Ensure uStructSize variable exists, then initialize it.
	__if_exists ( T::uStructSize )
	{
		result.uStructSize = sizeof( T );
	}

	__if_not_exists ( T::uStructSize )
	{
		UCLIDException ue("ELI17336", "GetLeadToolsSizedStruct called on unsupported type");
		ue.addDebugInfo("Missing member","uStructSize");
		throw ue;
	}

	// Ensure Flags variable exists, then initialize it.
	__if_exists ( T::Flags )
	{
		result.Flags = Flags;
	}
	// Note: FILEPDFOPTIONS does not contain member flag; don't throw exception

	return result;
}
//------------------------------------------------------------------------------------------------