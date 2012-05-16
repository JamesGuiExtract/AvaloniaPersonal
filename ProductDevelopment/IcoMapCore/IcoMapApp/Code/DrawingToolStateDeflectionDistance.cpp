#include "stdafx.h"
#include "DrawingToolStateDeflectionDistance.h"

#include "DrawingToolFSM.h"
#include "DrawingToolStateLine.h"
#include "DrawingToolStateLinePoint.h"
#include "DrawingToolStateLineBearing.h"
#include "DrawingToolStateDeflectionAngle.h"
#include "LineCalculationEngine.h"
#include "IcoMapOptions.h"

#include <Distance.hpp>
#include <UCLIDException.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

DrawingToolStateDeflectionDistance* DrawingToolStateDeflectionDistance::m_pInstance = NULL;

//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateDeflectionDistance::sGetInstance(void)
{
	if (!m_pInstance)
	{
		m_pInstance = new DrawingToolStateDeflectionDistance(kDistance, "Enter distance");
	}
	return m_pInstance->reset();
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateDeflectionDistance::sDelete(void)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}
}
//--------------------------------------------------------------------------------------------------
DrawingToolStateDeflectionDistance::DrawingToolStateDeflectionDistance(EInputType eInputType,std::string strPrompt) : 
	DrawingToolState(eInputType,strPrompt) 
{
}
//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateDeflectionDistance::reset(void)
{
	setCurrentCurveParameter(kLineDistance);
	return this;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateDeflectionDistance::cancel(DrawingToolFSM* pFSM)
{
	changeState(pFSM,DrawingToolStateLine::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateDeflectionDistance::processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput)
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

				// set distance
				pDIG->setSegmentParameter(UCLID_FEATUREMGMTLib::kLine, kLineDistance, strText);

				// angle direction is always required
				bool bLeft = true, bDummy;
				pFSM->getCurrentToggleInputs(bLeft, bDummy);
				string strToggleInput = bLeft ? "1" : "0";
				pDIG->setSegmentParameter(UCLID_FEATUREMGMTLib::kLine, kArcConcaveLeft, strToggleInput);

				// add this curve to the end of the sketch
				pDIG->addCurrentSegment();

				// it's time to update default values since the curve is already drawn
				pFSM->updateDefaultValues();

				changeState(pFSM, DrawingToolStateDeflectionAngle::sGetInstance());
			}
			catch(...)
			{
				pFSM->startLineDeflectionAngleDrawing();
				throw;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI02919");
	}
	else
	{
		UCLIDException uclidException("ELI02920","DrawingToolStateDeflectionDistance received invalid input.");
		throw uclidException;
	}
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateDeflectionDistance::changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint)
{
	if (!bHasAPoint)
	{
		changeState(pFSM, DrawingToolStateLinePoint::sGetInstance());

		return;
	}	
	
	try
	{
		// This is a test to see if there's at least one segment in the current sketch
		// Therefore, do not care about the tangent-out angle value
		pFSM->getLastSegmentTanOutAngleInRadians();
		// It's safe to change state to DrawingToolStateDeflectionAngle
		changeState(pFSM, DrawingToolStateDeflectionAngle::sGetInstance());
	}
	catch (...)
	{
		// If you get here, that means there's no segment in current drawing sketch
		// Change state to line bearing
		changeState(pFSM,DrawingToolStateLineBearing::sGetInstance());
	}
}
//--------------------------------------------------------------------------------------------------
