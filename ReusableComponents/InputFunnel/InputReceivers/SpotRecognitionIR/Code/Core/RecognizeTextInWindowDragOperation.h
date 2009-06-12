//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	RecognizeTextInWindowDragOperation.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#pragma once

#include "LButtonDragOperation.h"
#include "SpotRecognitionDlg.h"

class RecognizeTextInWindowDragOperation : public LButtonDragOperation
{
public:

	RecognizeTextInWindowDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl, 
									   SpotRecognitionDlg* pSpotRecDlg);

	virtual void processDragOperation(const CartographicPoint& p1,
									  const CartographicPoint& p2);

private:
	SpotRecognitionDlg *m_pSpotRecDlg;

	// PURPOSE: To limit the Cartographic points based on the image
	//			bounds and to modify the points to fit within the image
	//			if they fall outside those bounds
	void limitAndModifyCartographicPoints(CartographicPoint& rp1,
		CartographicPoint& rp2);
};