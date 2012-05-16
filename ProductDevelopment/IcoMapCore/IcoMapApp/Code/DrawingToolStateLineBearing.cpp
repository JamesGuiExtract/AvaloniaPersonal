//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateLineBearing.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "DrawingToolStateLineBearing.h"

#include "DrawingToolFSM.h"
#include "DrawingToolStateLine.h"
#include "DrawingToolStateLinePoint.h"
#include "DrawingToolStateLineDistance.h"
#include "LineCalculationEngine.h"

#include <IcoMapOptions.h>
#include <DirectionHelper.h>
#include <UCLIDException.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

DrawingToolStateLineBearing* DrawingToolStateLineBearing::m_pInstance = NULL;

//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateLineBearing::sGetInstance(void)
{
	if (!m_pInstance)
	{
		m_pInstance = new DrawingToolStateLineBearing(kBearing,"Enter direction");
	}
	return m_pInstance->reset();
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLineBearing::sDelete(void)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}
}
//--------------------------------------------------------------------------------------------------
DrawingToolStateLineBearing::DrawingToolStateLineBearing(EInputType eInputType,std::string strPrompt) : 
	DrawingToolState(eInputType,strPrompt) 
{
}
//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateLineBearing::reset(void)
{
	m_pInstance->setCurrentCurveParameter(kLineBearing);
	return this;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLineBearing::cancel(DrawingToolFSM* pFSM)
{
	changeState(pFSM,DrawingToolStateLine::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLineBearing::changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint)
{
	// if only the point state needs to be skipped
	if (!bHasAPoint)
	{
		changeState(pFSM, DrawingToolStateLinePoint::sGetInstance());
	}	
}
//--------------------------------------------------------------------------------------------------
string DrawingToolStateLineBearing::getPrompt()
{
	// check current status: whether its default to deflection angle (true)
	// or internal angle (false)
	EDirection eDirectionType = static_cast<EDirection>(IcoMapOptions::sGetInstance().getInputDirection());
	switch (eDirectionType)
	{
	case kBearingDir:
		m_strPrompt = "Enter direction (as Bearing)";
		break;
	case kPolarAngleDir:
		m_strPrompt = "Enter direction (as Polar angle)";
		break;
	case kAzimuthDir:
		m_strPrompt = "Enter direction (as Azimuth)";
		break;
	}

	return m_strPrompt;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateLineBearing::processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput)
{
	if (m_eInputType == kBearing)
	{
		try
		{
			try
			{
				DynamicInputGridWnd* pDIG = pFSM->getDIG();
				// get text of the input
				string strText = pFSM->getText(m_eInputType, ipTextInput);
				pDIG->setSegmentParameter(UCLID_FEATUREMGMTLib::kLine, kLineBearing, strText, true);

				changeState(pFSM, DrawingToolStateLineDistance::sGetInstance());
			}
			catch(...)
			{
				pFSM->startLineDrawing();
				throw;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI12151");
	}
	else
	{
		UCLIDException uclidException("ELI01089","DrawingToolStateLineBearing received invalid input.");
		throw uclidException;
	}
}
//--------------------------------------------------------------------------------------------------

