#include "stdafx.h"
#include "OpenSubImgInWindowDragOperation.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <l_bitmap.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

OpenSubImgInWindowDragOperation::OpenSubImgInWindowDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl,
																 UCLID_SPOTRECOGNITIONIRLib::ISpotRecognitionWindowPtr ipSRIR)
:LButtonDragOperation(rUCLIDGenericDisplayCtrl, kRectangle),
 m_ipSRIR(ipSRIR)
{
}

void OpenSubImgInWindowDragOperation::processDragOperation(const CartographicPoint& p1,
														   const CartographicPoint& p2)
{
	UCLID_SPOTRECOGNITIONIRLib::ISubImageHandlerPtr ipSubImageHandler;
	CComBSTR bstrTooltip, bstrTrainingFile;
	m_ipSRIR->GetSubImageHandler(&ipSubImageHandler, &bstrTooltip, &bstrTrainingFile);

	if (ipSubImageHandler == __nullptr)
	{
		return;
	}

	string strSubImageFile("");
	try
	{	
		// convert the two cartographic points that are in world coordinates into
		// image pixel coordinates.
		POINT imagePoint1, imagePoint2;
		long nPageNum = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();
		m_UCLIDGenericDisplayCtrl.convertWorldToImagePixelCoords(p1.m_dX, p1.m_dY, 
			&imagePoint1.x, &imagePoint1.y, nPageNum);
		m_UCLIDGenericDisplayCtrl.convertWorldToImagePixelCoords(p2.m_dX, p2.m_dY, 
			&imagePoint2.x, &imagePoint2.y, nPageNum);

		// Retrieve original base rotation
		double dOriginalAngle = m_UCLIDGenericDisplayCtrl.getBaseRotation( nPageNum );

		// make a new raster zone for the new sub image
		IRasterZonePtr ipTempImageZone(__uuidof(RasterZone));
		VARIANT_BOOL bIsImagePortionOpened(VARIANT_FALSE);
		bIsImagePortionOpened = m_ipSRIR->IsImagePortionOpened();
		if (bIsImagePortionOpened)
		{
			IRasterZonePtr ipImagePortion = m_ipSRIR->GetImagePortion();
			if (ipImagePortion)
			{
				ipImagePortion->CopyDataTo(ipTempImageZone);
			}
		}
		// crop the rectangular from the original image
		getImagePortionInfo(imagePoint1, imagePoint2, ipTempImageZone);
		// now open the portion of the original image in another spot rec window
		ipSubImageHandler->NotifySubImageCreated(m_ipSRIR, ipTempImageZone, dOriginalAngle);
	}
	catch(...)
	{
		if (!strSubImageFile.empty())
		{
			try
			{
				// if an error occurs, delete the temp sub image file
				deleteFile(strSubImageFile, true);
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI25128");
		}
		
		throw;
	}
}

///////////////////////////////////
// Helper functions
void OpenSubImgInWindowDragOperation::getImagePortionInfo(const POINT& imagePoint1, 
														  const POINT& imagePoint2,
														  IRasterZone* pRasterZone)
{
	// determine the smallest and biggest coordinates among the four corners.
	long nMinX = min(imagePoint1.x, imagePoint2.x);
	long nMinY = min(imagePoint1.y, imagePoint2.y);
	long nMaxX = max(imagePoint1.x, imagePoint2.x);
	long nMaxY = max(imagePoint1.y, imagePoint2.y);
	// sub image height
	long nSubImageHeight = nMaxY - nMinY;
	long nSubImageWidth = nMaxX - nMinX;

	// if current image is a sub image from some original image, then original offsets are non-zeros
	long nOriginalOffsetX = pRasterZone->StartX;
	long nOriginalOffsetY = pRasterZone->StartY - (pRasterZone->Height/2);	
	long nPageNum = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();
	// put page number in raster zone if page number is not set yet
	if (pRasterZone->PageNumber <= 0 || nPageNum > 1)
	{
		pRasterZone->PageNumber = nPageNum;
	}
	
	// the bounding rectangular creates the new sub image, the offsets for 
	// this sub image shall be related to the original image.
	long nNewOffsetX = nOriginalOffsetX + nMinX;
	long nNewOffsetY = nOriginalOffsetY + nMinY;

	// reset the offset point (the "Upper left corner", which is the smallest coordinates)
	pRasterZone->StartX = nNewOffsetX;
	pRasterZone->StartY = nNewOffsetY + nSubImageHeight/2;
	pRasterZone->EndX = nNewOffsetX + nSubImageWidth;
	pRasterZone->EndY = nNewOffsetY + nSubImageHeight/2;
	// reset the zone height and width
	pRasterZone->Height = nSubImageHeight;
}