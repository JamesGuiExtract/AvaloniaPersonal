//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateCurvePoint.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "DrawingToolStateCurvePoint.h"

#include "DrawingToolFSM.h"
#include "DrawingToolStateCurve.h"
#include "DrawingToolStateCurveParam1.h"
#include "DrawingToolStateView.h"

#include <ICurveCalculationEngine.h>
#include <UCLIDException.h>
#include <Point.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

DrawingToolStateCurvePoint* DrawingToolStateCurvePoint::m_pInstance = NULL;

//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateCurvePoint::sGetInstance(void)
{
	if (!m_pInstance)
	{
		m_pInstance = new DrawingToolStateCurvePoint(kPoint,"Enter starting point");
	}
	return m_pInstance->reset();
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurvePoint::sDelete(void)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}
}
//--------------------------------------------------------------------------------------------------
DrawingToolStateCurvePoint::DrawingToolStateCurvePoint(EInputType eInputType,std::string strPrompt) :
	DrawingToolState(eInputType,strPrompt)
{
}
//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateCurvePoint::reset(void)
{
	return this;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurvePoint::cancel(DrawingToolFSM* pFSM)
{
	changeState(pFSM,DrawingToolStateCurve::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurvePoint::processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput)
{
	DynamicInputGridWnd* pDIG = pFSM->getDIG();
	if (pDIG)
	{
		if (m_eInputType == kPoint)
		{
			IUnknownPtr ipUnknown(ipTextInput->GetValidatedInput());
			ICartographicPointPtr ipCP(ipUnknown);

			// set the starting point for the part
			pDIG->setStartPointForPart(ipCP);
		}
		else
		{
			UCLIDException uclidException("ELI01202", "Invalid input type");
			uclidException.addDebugInfo("EInputType", (int)m_eInputType);
			throw uclidException;
		}
	}

	changeState(pFSM, DrawingToolStateCurveParam1::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurvePoint::changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint)
{
	// if only the point state needs to be skipped
	if (bHasAPoint)
	{
		changeState(pFSM, DrawingToolStateCurveParam1::sGetInstance());
	}
}
//--------------------------------------------------------------------------------------------------
