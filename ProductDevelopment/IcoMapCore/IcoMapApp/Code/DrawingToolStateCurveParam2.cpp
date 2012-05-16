//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DrawingToolStateCurveParam2.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "DrawingToolStateCurveParam2.h"

#include "CurrentCurveTool.h"
#include "DrawingToolFSM.h"
#include "DrawingToolStateCurve.h"
#include "DrawingToolStateCurvePoint.h"
#include "DrawingToolStateCurveParam1.h"
#include "DrawingToolStateCurveParam3.h"

#include <ICurveCalculationEngine.h>
#include <DirectionHelper.h>
#include <Distance.hpp>
#include <UCLIDException.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

DrawingToolStateCurveParam2* DrawingToolStateCurveParam2::m_pInstance = NULL;

//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateCurveParam2::sGetInstance(void)
{
	if (!m_pInstance)
	{
		CurrentCurveTool& curveTool = CurrentCurveTool::sGetInstance();
		m_pInstance = new DrawingToolStateCurveParam2(curveTool.getInputType(2), curveTool.getPrompt(2));
	}
	return m_pInstance->reset();
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurveParam2::sDelete(void)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}
}
//--------------------------------------------------------------------------------------------------
DrawingToolStateCurveParam2::DrawingToolStateCurveParam2(EInputType eInputType,std::string strPrompt) :
	DrawingToolState(eInputType,strPrompt)
{
}
//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateCurveParam2::reset(void)
{
	CurrentCurveTool& curveTool = CurrentCurveTool::sGetInstance();
	setExpectedInputType(curveTool.getInputType(2));
	setPrompt(curveTool.getPrompt(2));
	setCurrentCurveParameter(curveTool.getCurveParameter(2));
	return this;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurveParam2::cancel(DrawingToolFSM* pFSM)
{
	changeState(pFSM,DrawingToolStateCurve::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurveParam2::processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput)
{
	DynamicInputGridWnd* pDIG = pFSM->getDIG();
	if (pDIG)
	{
		//Use GenericInput's curve parameter1 info to update the CCE
		ECurveParameterType eCurveParam = CurrentCurveTool::sGetInstance().getCurveParameter(2);
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
				UCLIDException uclidException("ELI01200", "Invalid input type");
				uclidException.addDebugInfo("EInputType", (int)m_eInputType);
				throw uclidException;
			}
			break;
		}
	}

	changeState(pFSM, DrawingToolStateCurveParam3::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateCurveParam2::changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint)
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
