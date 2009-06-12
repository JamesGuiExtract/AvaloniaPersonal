//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	PanWindowDragOperation.h
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

class PanWindowDragOperation : public LButtonDragOperation
{
public:
	PanWindowDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl);

	void PanWindowDragOperation::onMouseMove(short Button, short Shift, long x, long y);

	virtual void processDragOperation(const CartographicPoint& p1,
		const CartographicPoint& p2);

	// by default, when we are in pan mode, we should continue to be in pan mode
	// so that we can do multiple pans in sequence.
	virtual bool autoRepeat() {return true;}
};
