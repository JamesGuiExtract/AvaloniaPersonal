//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateCurveParam3.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "DrawingToolStateCurveParam3.h"

#include "CurrentCurveTool.h"
#include "DrawingToolFSM.h"
#include "DrawingToolStateCurve.h"
#include "DrawingToolStateCurvePoint.h"
#include "DrawingToolStateCurveParam1.h"

#include <ICurveCalculationEngine.h>
#include <DirectionHelper.h>
#include <Distance.hpp>
#include <UCLIDException.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

using namespace std;

DrawingToolStateCurveParam3* DrawingToolStateCurveParam3::m_pInstance = NULL;

//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateCurveParam3::sGetInstance(void)
{
	if (!m_pInstance)
	{
		CurrentCurveTool& curveTool = CurrentCurveTool::sGetInstance();
		m_pInstance = new DrawingToolStateCurveParam3(curveTool.getInputType(3),curveTool.getPrompt(3));
	}
	return m_pInstance->reset();
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurveParam3::sDelete(void)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}
}
//--------------------------------------------------------------------------------------------------
DrawingToolStateCurveParam3::DrawingToolStateCurveParam3(EInputType eInputType,std::string strPrompt) :
	DrawingToolState(eInputType,strPrompt)
{
}
//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateCurveParam3::reset(void)
{
	CurrentCurveTool& curveTool = CurrentCurveTool::sGetInstance();
	setExpectedInputType(curveTool.getInputType(3));
	setPrompt(curveTool.getPrompt(3));
	setCurrentCurveParameter(curveTool.getCurveParameter(3));
	return this;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurveParam3::cancel(DrawingToolFSM* pFSM)
{
	changeState(pFSM,DrawingToolStateCurve::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurveParam3::processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput)
{
	try
	{
		CurrentCurveTool& curveTool = CurrentCurveTool::sGetInstance();

		try
		{
			DynamicInputGridWnd* pDIG = pFSM->getDIG();
			if (pDIG)
			{
				// Use curve parameter2 info to update the CCE
				ECurveParameterType eCurveParam = curveTool.getCurveParameter(3);
				// if the input type is angle, bearing, or distance
				switch (m_eInputType)
				{
				case kAngle:
				case kBearing:
				case kDistance:
					{
						// get text out
						string strValue = pFSM->getText(m_eInputType, ipTextInput);
						pDIG->setSegmentParameter(UCLID_FEATUREMGMTLib::kArc, eCurveParam, strValue);
					}
					break;
					// we only expect three input types: angle, bearing and distance
				default:
					{
						UCLIDException uclidException("ELI01201", "Invalid input type");
						uclidException.addDebugInfo("EInputType", (int)m_eInputType);
						throw uclidException;
					}
					break;
				}
			}
			
			DrawingToolState* pNextState = NULL;
			
			bool bToggleDirectionEnabled = curveTool.isToggleCurveDirectionEnabled();
			bool bToggleDeltaEnabled = curveTool.isToggleCurveDeltaAngleEnabled();
			
			bool bLeft = true, bGreaterThan180 = false;
			pFSM->getCurrentToggleInputs(bLeft, bGreaterThan180);

			if (bToggleDirectionEnabled)
			{
				pDIG->setSegmentParameter(UCLID_FEATUREMGMTLib::kArc, kArcConcaveLeft, bLeft ? "1" : "0");
			}

			if (bToggleDeltaEnabled)
			{
				pDIG->setSegmentParameter(UCLID_FEATUREMGMTLib::kArc, kArcDeltaGreaterThan180Degrees, bGreaterThan180 ? "1" : "0");
			}
			
			// add this curve to the end of the sketch
			pDIG->addCurrentSegment();

			// it's time to update default values since the curve is already drawn
			pFSM->updateDefaultValues();
			changeState(pFSM, DrawingToolStateCurveParam1::sGetInstance());
		}
		catch (...)
		{
			pFSM->startCurveDrawing();
			throw;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI01274");
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurveParam3::changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint)
{
	// if only the point state needs to be skipped
	if (bHasAPoint)
	{
		changeState(pFSM,DrawingToolStateCurveParam1::sGetInstance());
		return;
	}
	
	changeState(pFSM,DrawingToolStateCurvePoint::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
