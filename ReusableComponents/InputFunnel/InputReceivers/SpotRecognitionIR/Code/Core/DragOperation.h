//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DragOperation.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#pragma once

#include "UCLIDGenericDisplay.h"

class DragOperation
{
public:
	// derived DragOperation objects may need to use the UCLIDGenericDisplayCtrl
	// A reference to a UCLIDGenericDisplayCtrl object is being kept within each
	// drag operation because multiple instances of the UCLIDGenericDisplayCtrl may
	// be up on the screen, and simultaneous drag operations could be happening at
	// the same time.
	DragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl);
	
	// virtual destructor so that derived classes can clean up after themselves
	// properly.
	virtual ~DragOperation();

	// the following methods are to be implemented by all DragOperation derived objects
	// the parameters to the following methods are the same as the parameters to the
	// methods of the same name associated with the ActiveX stock events.
	virtual void onMouseDown(short Button, short Shift, long x, long y) {}
	virtual void onMouseMove(short Button, short Shift, long x, long y) {}
	virtual void onMouseUp(short Button, short Shift, long x, long y) {}
	virtual void onDblClick() {}
	// every sub-class must implement this method
	// Whether or not current drag operation is in process
	virtual bool isInProcess() = 0;
	
	// override this method and return a boolean value indicating this drag operation
	// should automatically "rerun" on a mouse up event.
	virtual bool autoRepeat() {return false;}

	// override reset() to reset the drag operation to its originally created state
	// the reset() method should also restore the state of the m_UCLIDGenericDisplayCtrl
	// to a meaningful state.
	virtual void reset() {}

	// override init() to initialize the drag operation to its first state
	virtual void init() {}

protected:
	CUCLIDGenericDisplay& m_UCLIDGenericDisplayCtrl;
};
