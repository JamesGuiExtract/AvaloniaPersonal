//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LButtonDragOperation.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#pragma once

#include "DragOperation.h"
#include "CartographicPoint.h"

class LButtonDragOperation : public DragOperation
{
public:
	enum ERubberbandingMode {kNone, kLine, kRectangle};

	LButtonDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl, 
		ERubberbandingMode eRubberbandingMode);

	// see base class for documentation of these methods
	virtual void onMouseDown(short Button, short Shift, long x, long y);
	virtual void onMouseUp(short Button, short Shift, long x, long y);
	virtual void reset();
	virtual void init();
	virtual bool isInProcess();

	// By default, a LButtonDragOperation is a one time operation (such as zoom-window).
	// However for other types of operations which should keep running in sequence
	// (until the user cancels the drag operation somehow), it makes sense to auto-repeat
	// the drag operation.  In such cases, override this method, and return true;  This
	// class's implementation of this method returns false.
	virtual bool autoRepeat();

	// override processDragOperation() to process the drag operation
	// the drag start/end points will be passed via p1 and p2 respectively
	virtual void processDragOperation(const CartographicPoint& p1,
		const CartographicPoint& p2) = 0;

protected:
	enum EDragState {kNotApplicable, kWaitingForLButtonDown, kWaitingForLButtonUp};
	EDragState m_eDragState;

	CartographicPoint m_p1, m_p2;
	ERubberbandingMode m_eRubberbandingMode;
};
