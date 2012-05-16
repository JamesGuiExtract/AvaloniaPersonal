//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateLine.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "DrawingToolStateLine.h"

#include "DrawingToolFSM.h"
#include "DrawingToolStateLinePoint.h"
#include "DrawingToolStateView.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

DrawingToolStateLine* DrawingToolStateLine::m_pInstance = NULL;

//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateLine::sGetInstance(void)
{
	if (!m_pInstance)
	{
		m_pInstance = new DrawingToolStateLine(kNone,"");
	}
	return m_pInstance->reset();
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLine::sDelete(void)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}
}
//--------------------------------------------------------------------------------------------------
DrawingToolStateLine::DrawingToolStateLine(EInputType eInputType,std::string strPrompt) :
	DrawingToolState(eInputType,strPrompt)
{
}
//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateLine::reset(void)
{
	return DrawingToolStateLinePoint::sGetInstance();
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLine::cancel(DrawingToolFSM* pFSM)
{
	changeState(pFSM,DrawingToolStateView::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLine::processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput)
{
}
//--------------------------------------------------------------------------------------------------
