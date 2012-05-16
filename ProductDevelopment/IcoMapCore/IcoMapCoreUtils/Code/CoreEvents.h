//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CoreEvents.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================
#pragma once

#include "IcoMapCoreUtils.h"
#include <EventID.h>

class EXPORT_IcoMapCoreUtils CoreEvents  
{
public:
	virtual ~CoreEvents();

	static EventID ENABLE_POINT_INPUT;
	static EventID DISABLE_POINT_INPUT;
	static EventID ENABLE_TEXT_INPUT;
	static EventID DISABLE_TEXT_INPUT;
	static EventID ENABLE_TOGGLE_CURVE_INPUT;
	static EventID DISABLE_TOGGLE_CURVE_INPUT;
};
