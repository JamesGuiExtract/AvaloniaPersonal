//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateCurve.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "DrawingToolStateCurve.h"

#include "DrawingToolFSM.h"
#include "DrawingToolStateCurvePoint.h"
#include "DrawingToolStateView.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

DrawingToolStateCurve* DrawingToolStateCurve::m_pInstance = NULL;

DrawingToolState* DrawingToolStateCurve::sGetInstance(void)
{
	if (!m_pInstance)
	{
		m_pInstance = new DrawingToolStateCurve(kNone,"");
	}
	return m_pInstance->reset();
}

void DrawingToolStateCurve::sDelete(void)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}
}

DrawingToolStateCurve::DrawingToolStateCurve(EInputType eInputType,std::string strPrompt) :
	DrawingToolState(eInputType,strPrompt)
{
}

DrawingToolState* DrawingToolStateCurve::reset(void)
{
	return DrawingToolStateCurvePoint::sGetInstance();
}

void DrawingToolStateCurve::cancel(DrawingToolFSM* pFSM)
{
	changeState(pFSM,DrawingToolStateView::sGetInstance());
}

void DrawingToolStateCurve::processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput)
{
}

