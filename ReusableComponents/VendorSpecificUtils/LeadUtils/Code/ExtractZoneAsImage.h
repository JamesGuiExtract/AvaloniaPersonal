
#pragma once

#include "LeadUtils.h"
#include <string>
#include <vector>
#include <l_bitmap.h>		// LeadTools Imaging library

//-------------------------------------------------------------------------------------------------
// PURPOSE: To extract a portion of an image as a separate image file
// REQUIRE: hBitmap is the handle of an image currently loaded in memory by the
//			LeadTools library
//			All the "long" type arguments conform to UCLID's way of defining a
//			zone as a "band" from a start position to an end position of a given
//			height.
//			The zone extracted will be from the currently loaded image page.
//			It is the caller's responsibility to ensure that the correct image
//			page is loaded into the bitmap associated with phBitmap before calling
//			this function.
// ARGS:	bAllowResize- If true, the resulting phSubImageBitmap will be resized as necessary to
//			ensure all pixels in the original zone are in the result. If false, the resulting
//			phSubImageBitmap will be the same size as the provided coordinates, but pixels in the
//			original image zone may be cropped from the result if the zone has any rotation to 
LEADUTILS_API void extractZoneAsImage(BITMAPHANDLE *phBitmap, long nStartX, 
									  long nStartY, long nEndX, long nEndY,
									  long nHeight,
									  const std::string& strZoneImageFileName,
									  L_INT iOutputImageFormat = FILE_BMP,
									  bool bAllowResize = true);

//-------------------------------------------------------------------------------------------------
// PURPOSE:	To extract a portion of an image to a bitmap
// ARGS:	bAllowResize- If true, the resulting phSubImageBitmap will be resized as necessary to
//			ensure all pixels in the original zone are in the result. If false, the resulting
//			phSubImageBitmap will be the same size as the provided coordinates, but pixels in the
//			original image zone may be cropped from the result if the zone has any rotation to it.
LEADUTILS_API void extractZoneAsBitmap(BITMAPHANDLE *phBitmap, long nStartX, 
									  long nStartY, long nEndX, long nEndY,
									  long nHeight,
									  BITMAPHANDLE *phSubImageBitmap,
									  bool bAllowResize = true);

//-------------------------------------------------------------------------------------------------
LEADUTILS_API void extractPolygonAsImage(const std::string& strImageFile,
										 std::vector<POINT> vecPolygonVertices,
										 const std::string& strOutputImageFile);

