//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	OEExceptionCaught.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "OEExceptionCaught.h"

// global/static variables
EventID OEExceptionCaught::ID;

OEExceptionCaught::OEExceptionCaught(const UCLIDException& uclidException)
:ObservableEvent(ID), m_uclidException(uclidException)
{
}

const UCLIDException& OEExceptionCaught::getException() const
{
	return m_uclidException;
}

