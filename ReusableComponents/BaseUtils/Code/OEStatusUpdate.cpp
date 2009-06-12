//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	OEStatusUpdate.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "OEStatusUpdate.h"

// global/static variables
EventID OEStatusUpdate::ID;

OEStatusUpdate::OEStatusUpdate(const string& strStatusUpdateText)
:ObservableEvent(ID), m_strStatusUpdateText(strStatusUpdateText)
{
}

const string& OEStatusUpdate::getStatusUpdateText() const
{
	return m_strStatusUpdateText;
}