//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	EntityAttributes.cpp
//
// PURPOSE:	This is an implementation file for EntityAttributes() class.
//			Where the EntityAttributes() class is the base class.
//			The code written in this file makes it possible to implement the various
//			application methods in the user interface.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// EntityAttibutes.cpp : implementation file
//

#include "stdafx.h"
#include "EntityAttributes.h"
#include "UCLIDException.h"

/////////////////////////////////////////////////////////////////////////////
// EntityAttributes
//==========================================================================================
EntityAttributes::EntityAttributes()
{

}
//==========================================================================================
//EntityAttributes const& EntityAttributes::operator = (EntityAttributes const& );
EntityAttributes::EntityAttributes(string zName, string zValue)
{
	//	set the string name and value
	m_strName		= zName;
	m_strValue		= zValue;
}
//==========================================================================================
EntityAttributes::~EntityAttributes()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16448");
}
//==========================================================================================
