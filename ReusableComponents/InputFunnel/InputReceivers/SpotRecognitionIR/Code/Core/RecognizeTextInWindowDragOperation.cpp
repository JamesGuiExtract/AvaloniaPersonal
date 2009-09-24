//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	RecognizeTextInWindowDragOperation.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "RecognizeTextInWindowDragOperation.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <TemporaryFileName.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//--------------------------------------------------------------------------------------------------
RecognizeTextInWindowDragOperation::RecognizeTextInWindowDragOperation(
CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl, SpotRecognitionDlg* pSpotRecDlg)
:LButtonDragOperation(rUCLIDGenericDisplayCtrl, kRectangle), 
 m_pSpotRecDlg(pSpotRecDlg)
{
}
//--------------------------------------------------------------------------------------------------
void RecognizeTextInWindowDragOperation::processDragOperation(const CartographicPoint& p1,
															  const CartographicPoint& p2)
{
	try
	{
		// get original image file that contains the zone
		string strOriginImageFile(m_UCLIDGenericDisplayCtrl.getImageName());

		try
		{
			try
			{
				// copy the CartographicPoints and limit (modifying if necessary)
				// [p13 #4726]
				CartographicPoint cpLocalp1 = p1;
				CartographicPoint cpLocalp2 = p2;
				limitAndModifyCartographicPoints(cpLocalp1, cpLocalp2);

				// convert the two cartographic points that are in world coordinates into
				// image pixel coordinates.
				POINT imagePoint1, imagePoint2;
				long nPageNumber = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();
				m_UCLIDGenericDisplayCtrl.convertWorldToImagePixelCoords(cpLocalp1.m_dX, cpLocalp1.m_dY, 
					&imagePoint1.x, &imagePoint1.y, nPageNumber);
				m_UCLIDGenericDisplayCtrl.convertWorldToImagePixelCoords(cpLocalp2.m_dX, cpLocalp2.m_dY, 
					&imagePoint2.x, &imagePoint2.y, nPageNumber);

				RECT rect;
				rect.left = min(imagePoint1.x, imagePoint2.x);
				rect.top = min(imagePoint1.y, imagePoint2.y);
				rect.right = max(imagePoint1.x, imagePoint2.x);
				rect.bottom = max(imagePoint1.y, imagePoint2.y);

				m_pSpotRecDlg->processImageForParagraphText(strOriginImageFile, nPageNumber, nPageNumber,
					true, 0, 0, &rect);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27714")
		}
		catch(UCLIDException& ue)
		{
			// Add the image file as debug data
			ue.addDebugInfo("Original Image File", strOriginImageFile);
			throw ue;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02388")
}
//--------------------------------------------------------------------------------------------------
void RecognizeTextInWindowDragOperation::limitAndModifyCartographicPoints(CartographicPoint &rp1,
																			CartographicPoint &rp2)
{
		// get the max X and Y limits for the image
		double dWidth(0.0), dHeight(0.0);
		m_UCLIDGenericDisplayCtrl.getImageExtents(&dWidth, &dHeight);

		// ensure points are within image bounds [p16 #4726]
		// points are always >= 0.0
		rp1.m_dX = min(rp1.m_dX, dWidth);
		rp1.m_dY = min(rp1.m_dY, dHeight);
		rp2.m_dX = min(rp2.m_dX, dWidth);
		rp2.m_dY = min(rp2.m_dY, dHeight);
}
//--------------------------------------------------------------------------------------------------