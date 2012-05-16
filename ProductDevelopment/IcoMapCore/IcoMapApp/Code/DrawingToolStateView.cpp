//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateView.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "DrawingToolStateView.h"

#include "DrawingToolFSM.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif


DrawingToolStateView* DrawingToolStateView::m_pInstance = NULL;

DrawingToolState* DrawingToolStateView::sGetInstance(void)
{
	if (!m_pInstance)
	{
		m_pInstance = new DrawingToolStateView(kNone,"Select a drawing tool");
	}
	return m_pInstance->reset();
}

void DrawingToolStateView::sDelete(void)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}
}

DrawingToolStateView::DrawingToolStateView(EInputType eInputType,std::string strPrompt) : 
	DrawingToolState(eInputType,strPrompt)
{
}

DrawingToolState* DrawingToolStateView::reset(void)
{
	return this;
}

void DrawingToolStateView::cancel(DrawingToolFSM* pFSM)
{
	changeState(pFSM,DrawingToolStateView::sGetInstance());
}

void DrawingToolStateView::processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput)
{
}
