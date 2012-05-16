//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateLineDistance.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "DrawingToolStateLineDistance.h"

#include "DrawingToolFSM.h"
#include "DrawingToolStateLine.h"
#include "DrawingToolStateLinePoint.h"
#include "DrawingToolStateLineBearing.h"
#include "LineCalculationEngine.h"

#include <Distance.hpp>
#include <UCLIDException.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

DrawingToolStateLineDistance* DrawingToolStateLineDistance::m_pInstance = NULL;

//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateLineDistance::sGetInstance(void)
{
	if (!m_pInstance)
	{
		m_pInstance = new DrawingToolStateLineDistance(kDistance,"Enter distance");
	}
	return m_pInstance->reset();
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLineDistance::sDelete(void)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}
}
//--------------------------------------------------------------------------------------------------
DrawingToolStateLineDistance::DrawingToolStateLineDistance(EInputType eInputType,std::string strPrompt) : 
	DrawingToolState(eInputType,strPrompt) 
{
}
//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateLineDistance::reset(void)
{
	m_pInstance->setCurrentCurveParameter(kLineDistance);
	return this;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLineDistance::cancel(DrawingToolFSM* pFSM)
{
	changeState(pFSM,DrawingToolStateLine::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLineDistance::processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput)
{
	if (m_eInputType  == kDistance)
	{
		try
		{
			try
			{
				DynamicInputGridWnd* pDIG = pFSM->getDIG();
				// get text of the input
				string strText = pFSM->getText(m_eInputType, ipTextInput);
				pDIG->setSegmentParameter(UCLID_FEATUREMGMTLib::kLine, kLineDistance, strText);
				// add this curve to the end of the sketch
				pDIG->addCurrentSegment();

				// it's time to update default values since the curve is already drawn
				pFSM->updateDefaultValues();
					
				changeState(pFSM,DrawingToolStateLineBearing::sGetInstance());
			}
			catch (...)
			{
				pFSM->startLineDrawing();
				throw;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI01276");
	}
	else
	{
		UCLIDException uclidException("ELI01090","DrawingToolStateLineDistance received invalid input.");
		throw uclidException;
	}
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLineDistance::changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint)
{
	// if only the point state needs to be skipped
	if (bHasAPoint)
	{
		changeState(pFSM,DrawingToolStateLineBearing::sGetInstance());
		return;
	}	
	
	changeState(pFSM, DrawingToolStateLinePoint::sGetInstance());
}
//--------------------------------------------------------------------------------------------------


