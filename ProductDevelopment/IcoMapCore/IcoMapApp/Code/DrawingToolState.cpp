//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolState.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "DrawingToolState.h"

#include "DrawingToolFSM.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

DrawingToolState::DrawingToolState(EInputType eInputType,std::string& strPrompt) : 
	m_eInputType(eInputType),
	m_strPrompt(strPrompt)
{
}

void DrawingToolState::changeState(DrawingToolFSM* pFSM, DrawingToolState* pState)
{
	pFSM->changeState(pState);
}

