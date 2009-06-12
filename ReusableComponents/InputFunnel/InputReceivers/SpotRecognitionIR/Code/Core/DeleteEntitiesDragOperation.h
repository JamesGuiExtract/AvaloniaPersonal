//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DeleteEntitiesDragOperation.h
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

class TPPolygon;

class DeleteEntitiesDragOperation : public LButtonDragOperation
{
public:
	DeleteEntitiesDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl);

	virtual void processDragOperation(const CartographicPoint& p1,
		const CartographicPoint& p2);

private:
	// delete specified entity if it is within the crossingSelectionWindow
	void deleteEntity(unsigned long ulEntityID, const TPPolygon& crossingSelectionWindow);
};
