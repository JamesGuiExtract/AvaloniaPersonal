//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DragOperation.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "DragOperation.h"
#include "UCLIDException.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

DragOperation::DragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl)
:m_UCLIDGenericDisplayCtrl(rUCLIDGenericDisplayCtrl)
{
}

DragOperation::~DragOperation()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16497");
}