//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateLinePoint.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "DrawingToolStateLinePoint.h"

#include "DrawingToolFSM.h"
#include "DrawingToolStateLine.h"
#include "DrawingToolStateLineBearing.h"
#include "LineCalculationEngine.h"

#include <Point.h>
#include <UCLIDException.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

DrawingToolStateLinePoint* DrawingToolStateLinePoint::m_pInstance = NULL;

//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateLinePoint::sGetInstance(void)
{
	if (!m_pInstance)
	{
		m_pInstance = new DrawingToolStateLinePoint(kPoint,"Enter starting point");
	}
	return m_pInstance->reset();
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLinePoint::sDelete(void)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}
}
//--------------------------------------------------------------------------------------------------
DrawingToolStateLinePoint::DrawingToolStateLinePoint(EInputType eInputType,std::string strPrompt) :
	DrawingToolState(eInputType,strPrompt)
{
}
//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateLinePoint::reset(void)
{
	return this;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLinePoint::cancel(DrawingToolFSM* pFSM)
{
	changeState(pFSM,DrawingToolStateLine::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLinePoint::processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput)
{
	if (m_eInputType == kPoint)
	{
		IUnknownPtr ipUnknown(ipTextInput->GetValidatedInput());
		// Get the input point.
		ICartographicPointPtr ipCP(ipUnknown);
		if (ipCP)
		{	
			DynamicInputGridWnd* pDIG = pFSM->getDIG();
			pDIG->setStartPointForPart(ipCP);
			
			changeState(pFSM,DrawingToolStateLineBearing::sGetInstance());
		}
	}
	else
	{
		UCLIDException uclidException("ELI01066","DrawingToolStateLinePoint received invalid input.");
		throw uclidException;
	}
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLinePoint::changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint)
{
	// if only the point state needs to be skipped
	if (bHasAPoint)
	{
		changeState(pFSM,DrawingToolStateLineBearing::sGetInstance());
	}	
}
//--------------------------------------------------------------------------------------------------
