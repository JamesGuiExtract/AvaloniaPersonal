//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateCurveParam1.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "DrawingToolStateCurveParam1.h"

#include "CurrentCurveTool.h"
#include "DrawingToolFSM.h"
#include "DrawingToolStateCurve.h"
#include "DrawingToolStateCurvePoint.h"
#include "DrawingToolStateCurveParam2.h"

#include <ICurveCalculationEngine.h>
#include <DirectionHelper.h>
#include <Distance.hpp>
#include <UCLIDException.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

DrawingToolStateCurveParam1* DrawingToolStateCurveParam1::m_pInstance = NULL;

//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateCurveParam1::sGetInstance(void)
{
	if (!m_pInstance)
	{
		CurrentCurveTool& curveTool = CurrentCurveTool::sGetInstance();
		m_pInstance = new DrawingToolStateCurveParam1(curveTool.getInputType(1), curveTool.getPrompt(1));
	}
	return m_pInstance->reset();
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurveParam1::sDelete(void)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}
}
//--------------------------------------------------------------------------------------------------
DrawingToolStateCurveParam1::DrawingToolStateCurveParam1(EInputType eInputType,std::string strPrompt) :
	DrawingToolState(eInputType,strPrompt)
{
}
//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateCurveParam1::reset(void)
{
	CurrentCurveTool& curveTool = CurrentCurveTool::sGetInstance();
	setExpectedInputType(curveTool.getInputType(1));
	setPrompt(curveTool.getPrompt(1));
	setCurrentCurveParameter(curveTool.getCurveParameter(1));
	return this;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurveParam1::cancel(DrawingToolFSM* pFSM)
{
	changeState(pFSM,DrawingToolStateCurve::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurveParam1::processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput)
{
	DynamicInputGridWnd* pDIG = pFSM->getDIG();
	if (pDIG)
	{
		//Use GenericInput's curve parameter1 info to update the CCE
		ECurveParameterType eCurveParam = CurrentCurveTool::sGetInstance().getCurveParameter(1);
		// if the input type is angle, bearing, or distance
		switch (m_eInputType)
		{
		case kAngle:
		case kBearing:
		case kDistance:
			{
				// get text out
				string strValue = pFSM->getText(m_eInputType, ipTextInput);
				// store the parameter value in the grid
				pDIG->setSegmentParameter(UCLID_FEATUREMGMTLib::kArc, eCurveParam, strValue, true);
			}
			break;
		// we only expect three input types: angle, bearing and distance
		default:
			{
				UCLIDException uclidException("ELI01199", "Invalid input type");
				uclidException.addDebugInfo("EInputType", (int)m_eInputType);
				throw uclidException;
			}
			break;
		}
	}

	changeState(pFSM, DrawingToolStateCurveParam2::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurveParam1::changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint)
{
	// if only the point state can not be skipped
	if (!bHasAPoint)
	{
		changeState(pFSM,DrawingToolStateCurvePoint::sGetInstance());
	}
}
//--------------------------------------------------------------------------------------------------
