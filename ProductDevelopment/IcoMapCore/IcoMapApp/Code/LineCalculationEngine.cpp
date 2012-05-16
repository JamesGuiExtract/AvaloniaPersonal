//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LineCalculationEngine.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS: Duan Wang
//
//==================================================================================================

#include "stdafx.h"
#include "LineCalculationEngine.h"

#include <UCLIDException.h>
#include <mathUtil.h>

#include <math.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//--------------------------------------------------------------------------------------------------
LineCalculationEngine::LineCalculationEngine()
:m_bStartPointSet(false), 
 m_bEndPointSet(false),
 m_bLineAngleSet(false), 
 m_bLengthSet(false),
 m_bInternDefAngleSet(false),
 m_bPrevTangentOutAngleSet(false),
 m_bInternDefAngleLeft(true),
 m_bIsDeflectionAngle(true)
{
}
//--------------------------------------------------------------------------------------------------
LineCalculationEngine::LineCalculationEngine(const TPPoint& startPoint, double dAngleRadian, double dLength)
:m_bStartPointSet(true), 
 m_bEndPointSet(false), 
 m_bLineAngleSet(true), 
 m_bLengthSet(true), 
 m_StartPoint(startPoint), 
 m_dLineAngle(dAngleRadian), 
 m_dLength(dLength),
 m_bInternDefAngleSet(false),
 m_bPrevTangentOutAngleSet(false),
 m_bInternDefAngleLeft(true),
 m_bIsDeflectionAngle(true)
{
	// make sure  0 <= line angle < 2*MathVars::PI
	double dMult = floor(m_dLineAngle / (2 * MathVars::PI));
	m_dLineAngle = m_dLineAngle - dMult * 2 * MathVars::PI;
}
//--------------------------------------------------------------------------------------------------
LineCalculationEngine::LineCalculationEngine(const TPPoint& startPoint, 
											 const TPPoint& endPoint)
:m_bStartPointSet(true), 
 m_bEndPointSet(true), 
 m_bLineAngleSet(false), 
 m_bLengthSet(false),
 m_StartPoint(startPoint), 
 m_EndPoint(endPoint),
 m_bInternDefAngleSet(false),
 m_bPrevTangentOutAngleSet(false),
 m_bInternDefAngleLeft(true),
 m_bIsDeflectionAngle(true)
{
}
//--------------------------------------------------------------------------------------------------
LineCalculationEngine::LineCalculationEngine(const LineCalculationEngine& lineCalculationEngine)
{
	m_bStartPointSet = lineCalculationEngine.m_bStartPointSet;
	m_bEndPointSet = lineCalculationEngine.m_bEndPointSet;
	m_bLineAngleSet = lineCalculationEngine.m_bLineAngleSet;
	m_bLengthSet = lineCalculationEngine.m_bLengthSet;
	m_bInternDefAngleSet = lineCalculationEngine.m_bInternDefAngleSet;
	m_bPrevTangentOutAngleSet = lineCalculationEngine.m_bPrevTangentOutAngleSet;
	m_bInternDefAngleLeft = lineCalculationEngine.m_bInternDefAngleLeft;
	m_bIsDeflectionAngle = lineCalculationEngine.m_bIsDeflectionAngle;
	
	if (m_bStartPointSet)
	{
		m_StartPoint = lineCalculationEngine.m_StartPoint;
	}

	if (m_bEndPointSet)
	{
		m_EndPoint = lineCalculationEngine.m_EndPoint;
	}

	if (m_bLineAngleSet)
	{
		m_dLineAngle = lineCalculationEngine.m_dLineAngle;
	}

	if (m_bLengthSet)
	{
		m_dLength = lineCalculationEngine.m_dLength;
	}

	if (m_bInternDefAngleSet)
	{
		m_dInternDefAngle = lineCalculationEngine.m_dInternDefAngle;
	}

	if (m_bPrevTangentOutAngleSet)
	{
		m_dPrevTangentOutAngle = lineCalculationEngine.m_dPrevTangentOutAngle;
	}
}
//--------------------------------------------------------------------------------------------------
LineCalculationEngine& LineCalculationEngine::operator = (const LineCalculationEngine& lineCalculationEngine)
{
	m_bStartPointSet = lineCalculationEngine.m_bStartPointSet;
	m_bEndPointSet = lineCalculationEngine.m_bEndPointSet;
	m_bLineAngleSet = lineCalculationEngine.m_bLineAngleSet;
	m_bLengthSet = lineCalculationEngine.m_bLengthSet;
	m_bInternDefAngleSet = lineCalculationEngine.m_bInternDefAngleSet;
	m_bPrevTangentOutAngleSet = lineCalculationEngine.m_bPrevTangentOutAngleSet;
	m_bInternDefAngleLeft = lineCalculationEngine.m_bInternDefAngleLeft;
	m_bIsDeflectionAngle = lineCalculationEngine.m_bIsDeflectionAngle;

	if (m_bStartPointSet)
	{
		m_StartPoint = lineCalculationEngine.m_StartPoint;
	}

	if (m_bEndPointSet)
	{
		m_EndPoint = lineCalculationEngine.m_EndPoint;
	}

	if (m_bLineAngleSet)
	{
		m_dLineAngle = lineCalculationEngine.m_dLineAngle;
	}

	if (m_bLengthSet)
	{
		m_dLength = lineCalculationEngine.m_dLength;
	}

	return *this;
}
//--------------------------------------------------------------------------------------------------
LineCalculationEngine::~LineCalculationEngine()
{
}
//--------------------------------------------------------------------------------------------------
double LineCalculationEngine::getAngleRadians()
{
	if (!m_bLineAngleSet)
	{
		if (m_bPrevTangentOutAngleSet && m_bInternDefAngleSet)
		{
			// the actual angle will depend on current angle interpretation
			if (m_bIsDeflectionAngle)
			{
				if (m_bInternDefAngleLeft)
				{
					m_dLineAngle = getAngleWithinFirstPeriodInRadians(
								m_dPrevTangentOutAngle + m_dInternDefAngle);
				}
				// if the internal/deflection angle is to the right of the 
				// previous line/curve tangent-out angle (i.e. clockwisely 
				// along the tangent-out angle)
				else
				{
					m_dLineAngle = getAngleWithinFirstPeriodInRadians(
								m_dPrevTangentOutAngle - m_dInternDefAngle);
				}
			}
			else
			{
				if (m_bInternDefAngleLeft)
				{
					m_dLineAngle = getAngleWithinFirstPeriodInRadians(
								m_dPrevTangentOutAngle + MathVars::PI - m_dInternDefAngle);
				}
				else
				{
					m_dLineAngle = getAngleWithinFirstPeriodInRadians(
								m_dPrevTangentOutAngle - MathVars::PI + m_dInternDefAngle);
				}
			}

			m_bLineAngleSet = true;

			return m_dLineAngle;
		}
		else if (m_bStartPointSet && m_bEndPointSet)
		{
			double dXStart = m_StartPoint.m_dX;
			double dYStart = m_StartPoint.m_dY;
			double dXEnd = m_EndPoint.m_dX;
			double dYEnd = m_EndPoint.m_dY;
			// angle of the line is from starting point to ending point
			// value from acos is in radians already
			m_dLineAngle = acos( (dXEnd - dXStart) / getLength() );
			
			// since acos only gives angle in first or second quadrant,
			// need to check if the angle is actually in third or fourth quadrant
			if (dYEnd < dYStart)	
			{
				m_dLineAngle = 2 * MathVars::PI - m_dLineAngle;
			}
			
			m_bLineAngleSet = true;

			return m_dLineAngle;
		}

		UCLIDException uclidException("ELI02934", "Please provide sufficient parameters to calculate line angle");
		uclidException.addDebugInfo("m_bLineAngleSet", m_bLineAngleSet);
		uclidException.addDebugInfo("m_bStartPointSet", m_bStartPointSet);
		uclidException.addDebugInfo("m_bEndPointSet", m_bEndPointSet);
		uclidException.addDebugInfo("m_bLengthSet", m_bLengthSet);
		uclidException.addDebugInfo("m_bInternDefAngleSet", m_bInternDefAngleSet);
		uclidException.addDebugInfo("m_bPrevTangentOutAngleSet", m_bPrevTangentOutAngleSet);
		throw uclidException;
	}	

	return m_dLineAngle;
}
//--------------------------------------------------------------------------------------------------
TPPoint LineCalculationEngine::getEndPoint()
{
	if (!m_bEndPointSet)
	{
		// calculate the end point using start point, length and line angle
		if (m_bStartPointSet && m_bLengthSet)
		{
			// calculate the end point using start point, length, prev tangent-out angle 
			// and internal or deflection angle
			if (!m_bLineAngleSet)
			{
				// calculate line angle in radians
				getAngleRadians();
			}

			if (m_bLineAngleSet)
			{
				m_EndPoint.m_dX = m_StartPoint.m_dX + m_dLength * cos(m_dLineAngle);
				m_EndPoint.m_dY = m_StartPoint.m_dY + m_dLength * sin(m_dLineAngle);
				
				m_bEndPointSet = true;

				return m_EndPoint;
			}
		}
			
		UCLIDException uclidException("ELI01628", "Please provide sufficient parameters to calculate ending point.");
		uclidException.addDebugInfo("m_bStartPointSet", m_bStartPointSet);
		uclidException.addDebugInfo("m_bLineAngleSet", m_bLineAngleSet);
		uclidException.addDebugInfo("m_bLengthSet", m_bLengthSet);
		uclidException.addDebugInfo("m_bPrevTangentOutAngleSet", m_bPrevTangentOutAngleSet);
		uclidException.addDebugInfo("m_bInternDefAngleSet", m_bInternDefAngleSet);
		throw uclidException;
	}

	return m_EndPoint;
}
//--------------------------------------------------------------------------------------------------
double LineCalculationEngine::getLength()
{
	if (!m_bLengthSet)
	{
		if (!m_bStartPointSet || !m_bEndPointSet)
		{
			UCLIDException uclidException("ELI01629", "Please provide sufficient parameters to calculate line distance.");
			uclidException.addDebugInfo("m_bStartPointSet", m_bStartPointSet);
			uclidException.addDebugInfo("m_bEndPointSet", m_bEndPointSet);
			throw uclidException;
		}

		m_dLength = sqrt( pow((m_StartPoint.m_dX - m_EndPoint.m_dX), 2) + pow((m_StartPoint.m_dY - m_EndPoint.m_dY), 2) );

		m_bLengthSet = true;
	}
		
	return m_dLength;
}
//--------------------------------------------------------------------------------------------------
TPPoint LineCalculationEngine::getStartPoint()
{
	if (!m_bStartPointSet)
	{
		// else calculate the starting point from end point, line angle and line distance
		if (!m_bEndPointSet || !m_bLineAngleSet || !m_bLengthSet)
		{
			UCLIDException uclidException("ELI01627", "Please provide sufficient parameters to calculate starting point.");
			uclidException.addDebugInfo("m_bEndPointSet", m_bEndPointSet);
			uclidException.addDebugInfo("m_bLineAngleSet", m_bLineAngleSet);
			uclidException.addDebugInfo("m_bLengthSet", m_bLengthSet);
			throw uclidException;
		}
		
		m_StartPoint.m_dX = m_EndPoint.m_dX - m_dLength * cos(m_dLineAngle);
		m_StartPoint.m_dY = m_EndPoint.m_dY - m_dLength * sin(m_dLineAngle);

		m_bStartPointSet = true;
	}
	
	return m_StartPoint;
}
//--------------------------------------------------------------------------------------------------
void LineCalculationEngine::internalOrDeflectionAngleIsLeft(bool bIsLeft)
{
	m_bInternDefAngleLeft = bIsLeft;
	// whenever the angle is toggled, the line angle needs to be recalculated
	m_bLineAngleSet = false;
}
//--------------------------------------------------------------------------------------------------
void LineCalculationEngine::setAngleInterpretation(bool bIsDeflectionAngle)
{
	m_bIsDeflectionAngle = bIsDeflectionAngle;
	// whenever the angle is toggled, the line angle needs to be recalculated
	m_bLineAngleSet = false;
}
//--------------------------------------------------------------------------------------------------
void LineCalculationEngine::reset()
{
	m_bStartPointSet = false;
	m_bEndPointSet = false;
	m_bLineAngleSet = false;
	m_bLengthSet = false;
	m_bInternDefAngleSet = false;
	m_bPrevTangentOutAngleSet = false;
	m_bInternDefAngleLeft = true;
	m_bIsDeflectionAngle = true;
}
//--------------------------------------------------------------------------------------------------
void LineCalculationEngine::setAngleRadians(double dAngleRadian)
{
	m_dLineAngle = getAngleWithinFirstPeriodInRadians(dAngleRadian);

	m_bLineAngleSet = true;
}
//--------------------------------------------------------------------------------------------------
void LineCalculationEngine::setAngleDegrees(double dDegrees)
{	
	//convert angle in degrees to radians
	m_dLineAngle = getAngleWithinFirstPeriodInRadians(dDegrees * MathVars::PI/180.0);
	m_bLineAngleSet = true;
}
//--------------------------------------------------------------------------------------------------
void LineCalculationEngine::setEndPoint(const TPPoint& endPoint)
{
	m_EndPoint = endPoint;
	m_bEndPointSet = true;
}
//--------------------------------------------------------------------------------------------------
void LineCalculationEngine::setInternalOrDeflectionAngle(double dInternDefAngleInRadians)
{
	m_dInternDefAngle = dInternDefAngleInRadians;
	m_bInternDefAngleSet = true;
}
//--------------------------------------------------------------------------------------------------
void LineCalculationEngine::setLength(double dLength)
{
	m_dLength = dLength;
	m_bLengthSet = true;
}
//--------------------------------------------------------------------------------------------------
void LineCalculationEngine::setPrevTangentOutAngle(double dPrevTangentOutInRadians)
{
	// make sure the angle is in between 0 and 2*PI
	m_dPrevTangentOutAngle = getAngleWithinFirstPeriodInRadians(dPrevTangentOutInRadians);
	m_bPrevTangentOutAngleSet = true;
}
//--------------------------------------------------------------------------------------------------
void LineCalculationEngine::setStartPoint(const TPPoint& startPoint)
{
	m_StartPoint = startPoint;
	m_bStartPointSet = true;
}

////////////////////////////////////////////////////////////////////////////
//			Helper Functions
///////////////////////////////////////////////////////////////////
double LineCalculationEngine::getAngleWithinFirstPeriodInRadians(double dInAngle)
{
	double dRet = dInAngle;
	// make sure the angle is in between 0 and 2*PI
	double dMult = floor( dRet / (MathVars::PI * 2) );
	dRet = dRet - dMult * (MathVars::PI * 2);
	
	return dRet;
}
