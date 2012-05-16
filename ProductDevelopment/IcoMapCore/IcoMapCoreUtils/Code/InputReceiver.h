//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	InputReceiver.h
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
#include <Observer.h>
#include <ObservableSubject.h>

class EXPORT_IcoMapCoreUtils InputReceiver  : public Observer, public ObservableSubject
{
protected:
	InputReceiver();
	virtual ~InputReceiver();
};
