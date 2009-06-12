//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	OEExceptionCaught.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#ifndef OE_EXCEPTION_CAUGHT_H
#define OE_EXCEPTION_CAUGHT_H

#include "BaseUtils.h"
#include "ObservableEvent.h"
#include "UCLIDException.h"

class EXPORT_BaseUtils OEExceptionCaught : public ObservableEvent
{
public:
	static EventID ID;

	OEExceptionCaught(const UCLIDException& uclidException);

	const UCLIDException& getException() const;

private:
	const UCLIDException& m_uclidException;
};

#endif // OE_EXCEPTION_CAUGHT_H