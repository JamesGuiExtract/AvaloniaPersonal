#include "stdafx.h"
#include "DrawingToolStateDeflectionAngle.h"

#include "DrawingToolFSM.h"
#include "DrawingToolStateLine.h"
#include "DrawingToolStateLinePoint.h"
#include "DrawingToolStateLineBearing.h"
#include "DrawingToolStateDeflectionDistance.h"
#include "LineCalculationEngine.h"

#include <IcoMapOptions.h>
#include <DirectionHelper.h>
#include <UCLIDException.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

DrawingToolStateDeflectionAngle* DrawingToolStateDeflectionAngle::m_pInstance = NULL;

//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateDeflectionAngle::sGetInstance(void)
{
	if (!m_pInstance)
	{
		m_pInstance = new DrawingToolStateDeflectionAngle(kAngle, "Enter the deflection/internal angle");
	}
	return m_pInstance->reset();
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateDeflectionAngle::sDelete(void)
{
	if (m_pInstance)
	{
		delete m_pInstance;
	}
}
//--------------------------------------------------------------------------------------------------
DrawingToolStateDeflectionAngle::DrawingToolStateDeflectionAngle(EInputType eInputType,std::string strPrompt) : 
	DrawingToolState(eInputType,strPrompt) 
{
}
//--------------------------------------------------------------------------------------------------
DrawingToolState* DrawingToolStateDeflectionAngle::reset(void)
{
	if (IcoMapOptions::sGetInstance().isDefinedAsDeflectionAngle())
	{
		setCurrentCurveParameter(kLineDeflectionAngle);
	}
	else
	{
		setCurrentCurveParameter(kLineInternalAngle);
	}

	return this;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateDeflectionAngle::cancel(DrawingToolFSM* pFSM)
{
	changeState(pFSM,DrawingToolStateLine::sGetInstance());
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateDeflectionAngle::changeToProperStartState(DrawingToolFSM* pFSM, bool bHasAPoint)
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
		// keep current state
	}
	catch (...)
	{
		// If you get here, that means there's no segment in current drawing sketch
		// Change state to line bearing
		changeState(pFSM,DrawingToolStateLineBearing::sGetInstance());
	}
}
//--------------------------------------------------------------------------------------------------
string DrawingToolStateDeflectionAngle::getPrompt()
{
	// check current status: whether its default to deflection angle (true)
	// or internal angle (false)
	if (IcoMapOptions::sGetInstance().isDefinedAsDeflectionAngle())
	{
		m_strPrompt = "Enter the deflection angle";
	}
	else
	{
		m_strPrompt = "Enter the internal angle";
	}

	return m_strPrompt;
}
//--------------------------------------------------------------------------------------------------
void DrawingToolStateDeflectionAngle::processInput(DrawingToolFSM* pFSM, ITextInputPtr ipTextInput)
{
	if (m_eInputType == kAngle)
	{
		try
		{
			DynamicInputGridWnd* pDIG = pFSM->getDIG();
			// get text of the input
			string strText = pFSM->getText(m_eInputType, ipTextInput);
			ECurveParameterType eParamType = 
				IcoMapOptions::sGetInstance().isDefinedAsDeflectionAngle() ?
				kLineDeflectionAngle : kLineInternalAngle;
			
			pDIG->setSegmentParameter(UCLID_FEATUREMGMTLib::kLine, eParamType, strText, true);
			
			changeState(pFSM, DrawingToolStateDeflectionDistance::sGetInstance());
		}
		catch(UCLIDException uclidException)
		{
			pFSM->startLineDeflectionAngleDrawing();

			uclidException.addHistoryRecord("ELI12150", "IcoMap was unable to draw the requested line segment.");
			throw uclidException;
		}
	}
	else
	{
		UCLIDException uclidException("ELI19474","DrawingToolStateDeflectionAngle received invalid input.");
		throw uclidException;
	}
}
//--------------------------------------------------------------------------------------------------
ECurveParameterType DrawingToolStateDeflectionAngle::getCurrentCurveParameter()
{
	if (IcoMapOptions::sGetInstance().isDefinedAsDeflectionAngle())
	{
		return kLineDeflectionAngle;
	}

	return kLineInternalAngle;
}
//--------------------------------------------------------------------------------------------------
