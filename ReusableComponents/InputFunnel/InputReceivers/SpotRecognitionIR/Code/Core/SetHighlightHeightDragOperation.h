//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SetHighlightHeightDragOperation.h
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

class SetHighlightHeightDragOperation : public LButtonDragOperation
{
public:
	SetHighlightHeightDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl);

	virtual void processDragOperation(const CartographicPoint& p1,
		const CartographicPoint& p2);
};
