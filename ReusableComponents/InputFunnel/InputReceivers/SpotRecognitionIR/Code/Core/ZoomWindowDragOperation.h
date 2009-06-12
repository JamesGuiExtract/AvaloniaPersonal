//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ZoomWindowDragOperation.h
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

class ZoomWindowDragOperation : public LButtonDragOperation
{
public:
	ZoomWindowDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl);
	~ZoomWindowDragOperation();

	virtual void processDragOperation(const CartographicPoint& p1,
		const CartographicPoint& p2);

	// by default, when we are in zoom-window mode, we should continue to be in zoom
	// window mode so that we can do multiple zoom windows in sequence.
	virtual bool autoRepeat();
};
