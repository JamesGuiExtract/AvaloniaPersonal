//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ThreePoints.cpp
//
// PURPOSE:	To store start, mid and end point (of a curve)
//
// NOTES:	
//
// AUTHORS:	Duan Wang
//
//==================================================================================================

#include "stdafx.h"
#include "ThreePoints.h"

#include <mathUtil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

ThreePoints::ThreePoints(const TPPoint& startPoint, 
						 const TPPoint& midPoint,
						 const TPPoint& endPoint)
{
	m_startPoint = startPoint;
	m_midPoint = midPoint;
	m_endPoint = endPoint;
}

bool operator == (const ThreePoints& TP1, const ThreePoints& TP2)
{
	// set temporary fault tolerance for points comparison
	MathVars::TemporaryZeroDefinition tempZero(1E-4);
	
	if ( TP1.m_startPoint == TP2.m_startPoint
		&& TP1.m_midPoint == TP2.m_midPoint
		&& TP1.m_endPoint == TP2.m_endPoint )
	{
		return true;
	}
	
	return false;
}

bool operator < (const ThreePoints& TP1, const ThreePoints& TP2)
{
	// set temporary fault tolerance for points comparison
	MathVars::TemporaryZeroDefinition tempZero(1E-4);

	if (!(TP1.m_startPoint == TP2.m_startPoint))
	{
		return TP1.m_startPoint < TP2.m_startPoint;
	}
	else 
	{
		if (!(TP1.m_midPoint == TP2.m_midPoint))
		{
			return TP1.m_midPoint < TP2.m_midPoint;
		}
		else
		{
			if (!(TP1.m_endPoint == TP2.m_endPoint))
			{
				return TP1.m_endPoint < TP2.m_endPoint;
			}
		}
	}

	return false;
}