//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	PasteImage.h
//
// PURPOSE:	Header file for the leadUtils image pasting functionality.
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================
#pragma once

#include "LeadUtils.h"

#include <l_bitmap.h>		// LeadTools Imaging library

#include <string>

using namespace std;

//--------------------------------------------------------------------------------------------------
// Exported functions
//--------------------------------------------------------------------------------------------------
// PURPOSE: To take and paste one image onto another image at a specified percent offset and page
//			and save the new image.
//
// ARGS:	strInputImage	- the name of the source image
//			strOuputImage	- the name of the new image that will be saved
//			strPasteImage	- the name of the image that will be pasted onto the source image
//			dHorizPercent	- the horizontal percentage offset from the top left of the image
//			dVertPercent 	- the vertical percentage offset from the top left of the image
//			lPage			- the page to paste the stamp on
LEADUTILS_API void pasteImageAtLocation(const string& strInputImage, const string& strOutputImage,
						  const string& strPasteImage, double dHorizPercent, 
						  double dVertPercent, long lPage);

//--------------------------------------------------------------------------------------------------
// Non-exported helper functions
//--------------------------------------------------------------------------------------------------
// PURPOSE: To load the specified stamp bitmap
//
// ARGS:	strPasteImage	- the name of the image to load
//			flInInfo		- the FILEINFO struct for the source image
//			rflPasteInfo	- an FILEINFO struct that will be loaded with the information of 
//							  the loaded bitmap
//			rbHandle		- a BITMAPHANDLE that will be set to point to the specified image
//
// REQUIRE:	rbHandle not already loaded with another image, otherwise memory leak will occur
//
// PROMISE: to load the stamp bitmap and match its BitsPerPixel to that of the input image
void loadStampBitmap(const string& strPasteImage, const FILEINFO& flInInfo, 
					 FILEINFO& rflPasteInfo, BITMAPHANDLE& rbHandle);
//--------------------------------------------------------------------------------------------------
// PURPOSE:	To compute an X coordinate based on a specified percentage and the image width
//
// ARGS:	dHorizPercent	- the horizontal percentage offset from the left of the image
//			liWidth			- the width of the image	
long getXCoordinate(double dHorizPercent, L_INT liWidth);
//--------------------------------------------------------------------------------------------------
// PURPOSE:	To compute an Y coordinate based on a specified percentage and the image height
//
// ARGS:	dVertPercent	- the vertical percentage offset from the top of the image
//			liHeight		- the width of the image	
long getYCoordinate(double dVertPercent, L_INT liHeight);
//--------------------------------------------------------------------------------------------------
// PURPOSE: To validate the specified X and Y coordinates based on the size of the input image
//			and the size of the stamp
//
// ARGS:	lX				- the X coordinate for the image
//			lY				- the Y coordinate for the image
//			hBmpSource		- the BITMAPHANDLE for the source image page to be stamped
//			hBmpStamp		- the BITMAPHANDLE for the image that will be stamped
//
// PROMISE: To throw an exception if pasting the image would result pasting pixels outside
//			the bounds of the orignial image
void validateStampLocation(long lX, long lY, const BITMAPHANDLE& hBmpSource, 
						   const BITMAPHANDLE& hBmpStamp);
//--------------------------------------------------------------------------------------------------
// PURPOSE: To validate the size of the stamp image
//
// ARGS:	hBmpSource		- the BITMAPHANDLE for the source image page to be stamped
//			hBmpStamp		- the BITMAPHANDLE for the image that will be stamped
//
// PROMISE:	To throw an exception if the stamp image is bigger than the source image
void validateStampImageSize(const BITMAPHANDLE& hBmpSource, const BITMAPHANDLE& hBmpStamp);
//--------------------------------------------------------------------------------------------------
// PURPOSE:	To validate the page number specified to apply the stamp on
//
// ARGS:	lPageNumber		- the page to apply the stamp to
//			lNumberOfPages	- the number of pages in the source bitmap
//
// PROMISE:	To throw an exception if the pagenumber requested to place the stamp on
//			is greater than the number of pages in the image
void validatePageNumber(long lPageNumber, long lNumberOfPages);
//--------------------------------------------------------------------------------------------------
// PURPOSE: To paste the stamp image onto the source image
//
// ARGS:	hPasteBmp		- the BITMAPHANDLE for the stamp image
//			hInBitmapList	- the BITMAPLIST for the source image
//			lPageNumber		- the page number to place the stamp on
//			dHorizPercent	- the horizontal percentage offset from the top left of the image
//			dVertPercent 	- the vertical percentage offset from the top left of the image
void placeStamp(BITMAPHANDLE& hPasteBmp, HBITMAPLIST& hInBitmapList, 
				long lPageNumber, double dHorizPercent, double dVertPercent);
//--------------------------------------------------------------------------------------------------
// PURPOSE: To save the now stamped image to the specified output image file
//
// ARGS:	hInBitmapList	- the BITMAPLIST for the source image
//			lNumberOfPages	- the number of pages in the source image
//			strOutImage		- the name of the output image file
//			flInInfo		- the FILEINFO struct for the source image
void saveStampedImage(HBITMAPLIST& hInBitmapList, long lNumberOfPages, const string& strOutImage,
					  const FILEINFO& flInInfo);
//--------------------------------------------------------------------------------------------------
// PURPOSE: To make the color palette of the destination bitmap match the color palette of
//			the source bitmap
//
// ARGS:	hBmpSource		- the source bitmap
//			hBmpDest		- the destination bitmap
//
// NOTE:	this function assumes the BitsPerPixel of each image matches, if they do not
//			match the behavior will not be as expected (it will still change the palette
//			but the overall colors may not match)
void matchPalette(BITMAPHANDLE& hBmpSource, BITMAPHANDLE& rhBmpDest);
//--------------------------------------------------------------------------------------------------