#pragma once

#include "LButtonDragOperation.h"
#include "SpotRecognitionIR.h"

#include <string>

class OpenSubImgInWindowDragOperation : public LButtonDragOperation
{
public:
	OpenSubImgInWindowDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl,
									UCLID_SPOTRECOGNITIONIRLib::ISpotRecognitionWindowPtr ipSRIR);

	virtual void processDragOperation(const CartographicPoint& p1,const CartographicPoint& p2);

private:
	UCLID_SPOTRECOGNITIONIRLib::ISpotRecognitionWindowPtr m_ipSRIR;

	//********************
	// Helper functions

	// crop the sub image and save it into a raster zone
	void getImagePortionInfo(const POINT& imagePoint1, const POINT& imagePoint2, IRasterZone* pRasterZone);
};
