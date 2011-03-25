
#include "stdafx.h"
#include "ExtractZoneAsImage.h"
#include "LeadToolsBitmapFreeer.h"
#include "MiscLeadUtils.h"

#include <UCLIDException.h>
#include <mathUtil.h>
#include <cpputil.h>

#include <Math.h>

using namespace std;

#define MAX_LEN 100

//-------------------------------------------------------------------------------------------------
double getSlope(long lTempStartX, long lTempStartY, long lTempEndX, long lTempEndY)
{
	//  formula for calculating slope:
	//  slope = (y2 - y1) / (x2 - x1)

	double dSlope = 0;

	// calculate X difference & Y differences
	double dDiffX = lTempEndX - lTempStartX;
	double dDiffY = lTempEndY - lTempStartY;
		
	// To avoid Division by zero
	if (dDiffX != 0)
	{
		dSlope =  atan(dDiffY / dDiffX);

		// the angle is 180°
		if (dDiffY == 0 && dDiffX < 0)
		{
			dSlope = MathVars::PI;
		}
	}
	else
	{
		if (dDiffY < 0)
		{
			dSlope = -MathVars::PI / 2.0;
		}
		else if (dDiffY > 0)
		{
			dSlope = MathVars::PI / 2.0;
		}
		else
		{
			dSlope = 0;
		}
	}

	return dSlope;
}
//-------------------------------------------------------------------------------------------------
void getBoundingRectangle(long nStartX, long nStartY, long nEndX, 
						  long nEndY, long nHeight, double dSlope, 
						  RECT& rZoneRect)
{
	// temp variables for storing the coordinates of vertices of the Zone
	long lX1 = 0, lY1 = 0, lX2 = 0, lY2 = 0, lX3 = 0, lY3 = 0, lX4 = 0, lY4 = 0;
		
	long lDiffX  = 0;
	long lDiffY  = 0;
	lDiffX = long ((nHeight / 2) * sin(dSlope));
	lDiffY = long ((nHeight / 2) * cos(dSlope));
	
	lX1 = nStartX - lDiffX;
	lY1 = nStartY + lDiffY;
	
	lX2 = nStartX + lDiffX;
	lY2 = nStartY - lDiffY;
	
	lX3 = nEndX + lDiffX;
	lY3 = nEndY - lDiffY;
	
	lX4 = nEndX - lDiffX;
	lY4 = nEndY + lDiffY;
	
	// for coordinates of the Zone	along X-axis & Y-axis											
	long lLeft= 0,lTop = 0,lRight  = 0, lBottom = 0;
	
	// identify the Left coordinate along X-axis
	lLeft = min(min(lX1,lX2),min(lX3,lX4));
	lLeft = max( 0, lLeft );
	
	// identify the Top coordinate along Y-axis
	lTop = min(min(lY1,lY2),min(lY3,lY4));
	lTop = max( 0, lTop );
	
	// identify the Right coordinate along X-axis
	lRight = max(max(lX1,lX2),max(lX3,lX4));

	// identify the Bottom coordinate along Y-axis
	lBottom = max(max(lY1,lY2),max(lY3,lY4));
	
	// Checking for the -ve values which are not valid
	if (lLeft < 0 || lTop < 0 || lRight < 0 || lBottom < 0)
	{
		UCLIDException ue("ELI02835", "Invalid Zone coordinates.");
		ue.addDebugInfo("Left", lLeft);
		ue.addDebugInfo("Top", lTop);
		ue.addDebugInfo("Right", lRight);
		ue.addDebugInfo("Bottom", lBottom);
		throw ue;			
	}			

	// create the bounding rectangle
	rZoneRect.left = lLeft;
	rZoneRect.top  = lTop;
	rZoneRect.right = lRight;
	rZoneRect.bottom = lBottom;
}
//-------------------------------------------------------------------------------------------------
void extractZoneAsBitmap(BITMAPHANDLE *phBitmap, long nStartX, long nStartY, long nEndX, long nEndY,
									  long nHeight, BITMAPHANDLE *phSubImageBitmap)
{
	// Check bitmap handles
	ASSERT_ARGUMENT("ELI15834", phBitmap != NULL);
	ASSERT_ARGUMENT("ELI15835", phSubImageBitmap != NULL);

	// determine the bounding Rectangle of the inclined ImageZone
	// this rectangle is actually used to rotate the text to 0 degree inclination
	// The following code is all related to get this bounding rectangle

	// checking for invalid coordinates
	if (nStartX == nEndX && nStartY == nEndY)
	{
		throw UCLIDException("ELI03344", "Invalid Zone coordinates.");
	}

	// get the bounds of the rectangle that is around the zone
	RECT rectImageZone;
	double dSlope = getSlope(nStartX, nStartY, nEndX, nEndY);
	getBoundingRectangle(nStartX, nStartY, nEndX, nEndY,
		nHeight, dSlope, rectImageZone);

	// Make sure coordinates are in View Perspective of the Bitmap
	L_INT nRet;
	nRet = L_RectToBitmap( phBitmap, TOP_LEFT, &rectImageZone );
	throwExceptionIfNotSuccess(nRet, "ELI16873", "Could not adjust coordinates.");

	// for inclined text
	double dImageZoneAngle = 0;
	if (dSlope != 0)
	{
		// copy the rectangular bounds of the image that surround the zone
		BITMAPHANDLE hBitmapImageZone;
		LeadToolsBitmapFreeer freeer( hBitmapImageZone, true );

		nRet = L_CopyBitmapRect(&hBitmapImageZone, phBitmap, sizeof(BITMAPHANDLE), rectImageZone.left,
			rectImageZone.top, abs(rectImageZone.right - rectImageZone.left),
			abs(rectImageZone.top - rectImageZone.bottom)); 
		throwExceptionIfNotSuccess(nRet, "ELI03351", "Unable to copy portion of bitmap!");

		// if slope is -ve
		if (dSlope < 0)
		{
			// checking start and end Y - coordinates and rotating accordingly
			if (nEndY < nStartY)
			{
				dImageZoneAngle = -1 * ((dSlope * 180) / MathVars::PI);
				nRet = L_RotateBitmap(&hBitmapImageZone, (L_INT32) (dImageZoneAngle * 100), 
					ROTATE_RESIZE, RGB(255,255,255));
				throwExceptionIfNotSuccess(nRet, "ELI03352", "Unable to rotate image!");
			}
			else
			{
				dImageZoneAngle = -1 * (-180 + (dSlope * 180) / MathVars::PI);
				nRet = L_RotateBitmap(&hBitmapImageZone, (L_INT32) (dImageZoneAngle * 100),
					ROTATE_RESIZE, RGB(255,255,255));
				throwExceptionIfNotSuccess(nRet, "ELI03353", "Unable to rotate image!");
			}
		}
		// if slope is +ve
		else
		{
			// checking start and end Y - coordinates and rotating accordingly
			if (nEndY < nStartY)
			{
				dImageZoneAngle =  (180 - (dSlope * 180) / MathVars::PI);
				nRet = L_RotateBitmap(&hBitmapImageZone, (L_INT32) (dImageZoneAngle * 100),
					ROTATE_RESIZE, RGB(255,255,255));
				throwExceptionIfNotSuccess(nRet, "ELI03354", "Unable to rotate image!");
			}
			else
			{
				dImageZoneAngle = -1 * ((dSlope * 180) / MathVars::PI);
				nRet = L_RotateBitmap(&hBitmapImageZone, (L_INT32) (dImageZoneAngle * 100),
					ROTATE_RESIZE, RGB(255,255,255));
				throwExceptionIfNotSuccess(nRet, "ELI03355", "Unable to rotate image!");	
			}
		}

		long lHalfBitmapHeight = BITMAPHEIGHT(&hBitmapImageZone) / 2;
		long lHalfZoneHeight = nHeight / 2;
		RECT finalRect = {0, lHalfBitmapHeight - lHalfZoneHeight,
			BITMAPWIDTH(&hBitmapImageZone), lHalfBitmapHeight + lHalfZoneHeight};

		{
			nRet = L_CopyBitmapRect(phSubImageBitmap, &hBitmapImageZone, sizeof(BITMAPHANDLE), finalRect.left,
				finalRect.top, abs(finalRect.right - finalRect.left),
				abs(finalRect.top - finalRect.bottom));
			throwExceptionIfNotSuccess(nRet, "ELI03356", "Unable to copy portion of rotated image!");	
		}
	}
	else
	{
		L_INT nRet = L_CopyBitmapRect(phSubImageBitmap, phBitmap, sizeof(BITMAPHANDLE), rectImageZone.left,
			rectImageZone.top, abs(rectImageZone.right - rectImageZone.left),
			abs(rectImageZone.top - rectImageZone.bottom));			 
		throwExceptionIfNotSuccess(nRet, "ELI03358", "Unable to copy portion of original image!");	
 	}
}
//-------------------------------------------------------------------------------------------------
void extractZoneAsImage(BITMAPHANDLE *phBitmap, long nStartX, 
						long nStartY, long nEndX, long nEndY,
						long nHeight, const string& strZoneImageFileName,
						L_INT iOutputImageFormat)

{
	// Check bitmap handle
	ASSERT_ARGUMENT("ELI15836", phBitmap != NULL);

	BITMAPHANDLE hBitmapImageZone;
	LeadToolsBitmapFreeer freeer( hBitmapImageZone, true );
	extractZoneAsBitmap( phBitmap, nStartX, nStartY, nEndX, nEndY, nHeight, &hBitmapImageZone);
	
	L_INT nRet = L_SaveBitmap( (char*) strZoneImageFileName.c_str(), &hBitmapImageZone, 
		iOutputImageFormat, hBitmapImageZone.BitsPerPixel, 2, NULL);
	throwExceptionIfNotSuccess(nRet, "ELI03359", "Unable to copy portion of rotated image!",
		strZoneImageFileName);	

	// Wait for file access
	waitForFileToBeReadable(strZoneImageFileName);
}
//-------------------------------------------------------------------------------------------------
void getPolygonBoundingRect(const vector<POINT>& vecPolygonVertices, int &nOriginX, int &nOriginY, 
							int &nWidth, int &nHeight)
{
	if (vecPolygonVertices.size() <= 2)
	{
		throw UCLIDException("ELI04816", "A polygon must contain at least three points!");
	}

	// pick the first point as a start
	nOriginX = vecPolygonVertices[0].x;
	nOriginY = vecPolygonVertices[0].y;
	nWidth = nOriginX;
	nHeight = nOriginY;
	
	double dDeltaX = (double)(vecPolygonVertices[1].x - nOriginX);
	double dDeltaY = (double)(vecPolygonVertices[1].y - nOriginY);
	double dSlope = (dDeltaY==0.0) ? 0.0 : dDeltaX / dDeltaY ;

	// a flag to indicate whether all points are on the same line or not
	// Init it to true till we find evidence to deny it
	bool bInLine = true;
	for (unsigned int i = 1; i<vecPolygonVertices.size(); i++)
	{
		// get the smallest value for x and y as the origin point for the bounding rect
		nOriginX = min(nOriginX, vecPolygonVertices[i].x);
		nOriginY = min(nOriginY, vecPolygonVertices[i].y);
		nWidth = max(nWidth, vecPolygonVertices[i].x);
		nHeight = max(nHeight, vecPolygonVertices[i].y);

		if (i+1 < vecPolygonVertices.size() && bInLine)
		{
			dDeltaX = (double)(vecPolygonVertices[i+1].x - vecPolygonVertices[i].x);
			dDeltaY = (double)(vecPolygonVertices[i+1].y - vecPolygonVertices[i].y);
			// check slope formed by previous point and the next point
			double dTempSlope = (dDeltaY==0.0) ? 0.0 : dDeltaX / dDeltaY ;
			if (!MathVars::isEqual(dTempSlope, dSlope))
			{
				bInLine = false;
			}
		}
	}

	// if all points are in one line, then throw exception
	if (bInLine)
	{
		throw UCLIDException("ELI04817", "All of the polygon vertices can not be in one line!");
	}
	
	nWidth = nWidth - nOriginX;
	nHeight = nHeight - nOriginY;
}
//-------------------------------------------------------------------------------------------------
void resetOrigin(vector<POINT> & vecPolygonVertices, int nNewStartX, int nNewStartY)
{
	for (unsigned int i=0; i<vecPolygonVertices.size(); i++)
	{
		vecPolygonVertices[i].x = vecPolygonVertices[i].x-nNewStartX;
		vecPolygonVertices[i].y = vecPolygonVertices[i].y-nNewStartY;
	}
}
//-------------------------------------------------------------------------------------------------
void extractPolygonAsImage(const string& strImageFile, 
						   vector<POINT> vecPolygonVertices,
						   const string& strOutputImageFile)
{
	BITMAPHANDLE hBitmap;
	LeadToolsBitmapFreeer freeerBM( hBitmap, true );
	
	// Get initialized FILEINFO struct
	FILEINFO fileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);
	
	// Load the bitmap
	L_INT nRet = L_LoadBitmap( (char*) strImageFile.c_str(), &hBitmap, 
		sizeof(BITMAPHANDLE), 0, ORDER_RGB, NULL, &fileInfo);
	throwExceptionIfNotSuccess(nRet, "ELI15923", "Unable to load bitmap.", 
		strImageFile);

	if (hBitmap.ViewPerspective != TOP_LEFT)
	{
		L_ChangeBitmapViewPerspective(NULL, &hBitmap, sizeof(BITMAPHANDLE), TOP_LEFT);
	}
	
	// crop the region bounded within a smallest rectangular inside the original image
	int nStartX, nStartY, nWidth, nHeight;
	getPolygonBoundingRect(vecPolygonVertices, nStartX, nStartY, nWidth, nHeight);

	// Individual scope for L_CopyBitmapRect()
	BITMAPHANDLE hFinal;
	LeadToolsBitmapFreeer freeerFinal( hFinal, true );

	nRet = L_CopyBitmapRect(&hFinal, &hBitmap, sizeof(BITMAPHANDLE), nStartX, 
		nStartY, nWidth, nHeight); 
	
	// calculate the origin (0,0)(i.e. top-left corner) of the sub image according to the start point
	resetOrigin(vecPolygonVertices, nStartX, nStartY);
	
	// create a region inside the new bitmap
	int nLen = vecPolygonVertices.size();
	POINT PolyPt[MAX_LEN]; // Array of points that defines the polygon 
	L_POINT* pPolyPt = PolyPt; // Pointer to the array of points 
	
	// Specify the vertices of the polygon 
	for (unsigned int i = 0; i<vecPolygonVertices.size(); i++)
	{
		pPolyPt[i].x = vecPolygonVertices[i].x;
		pPolyPt[i].y = vecPolygonVertices[i].y;
	}
	
	// set origin at top-left
	RGNXFORM XForm;
	XForm.uViewPerspective = TOP_LEFT;  // origin is top left of the bitmap (0,0)
	XForm.nXScalarNum = hFinal.Width;
	XForm.nXScalarDen = hFinal.Width;
	XForm.nYScalarNum = hFinal.Height;
	XForm.nYScalarDen = hFinal.Height;
	XForm.nXOffset = 0;
	XForm.nYOffset = 0;

	// Create a polygonal region 
	nRet = L_SetBitmapRgnPolygon(&hFinal, &XForm, pPolyPt, nLen, L_POLY_WINDING, L_RGN_SETNOT);

	// fill the region with whatever is the background color
	// take the top-left pixel's color as the background color
	COLORREF bgdColor = L_GetPixelColor(&hBitmap, 0, 0);
	
	// wipe out anything outside the region with bgdColor
	nRet = L_FillBitmap(&hFinal, bgdColor);
	L_FreeBitmapRgn(&hFinal);

	// Get the correct compression factor
	int nCompression = getCompressionFactor(fileInfo.Format);

	// now save the new bitmap to a file
	nRet = L_SaveBitmap( (char*) strOutputImageFile.c_str(), &hFinal, fileInfo.Format, 
		hFinal.BitsPerPixel, nCompression, NULL);
	throwExceptionIfNotSuccess(nRet, "ELI23548", "Unable to copy portion of image.",
		strOutputImageFile);	

	// Wait for file access
	waitForFileToBeReadable(strOutputImageFile);
}
//-------------------------------------------------------------------------------------------------
