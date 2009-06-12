//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CartographicPoint.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (Aug 2001 to present)
//
//==================================================================================================

#include "stdafx.h"
#include "CartographicPoint.h"

#include <mathUtil.h>
#include <math.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

CartographicPoint::CartographicPoint()
{
	m_dX = 0.0;
	m_dY = 0.0;
}

CartographicPoint::CartographicPoint(double dX, double dY)
{
	m_dX = dX;
	m_dY = dY;
}

bool operator == (const CartographicPoint& a, const CartographicPoint& b)
{
	if ( MathVars::isEqual(a.m_dX, b.m_dX)	&& MathVars::isEqual(a.m_dY, b.m_dY) )
	{
		return true;
	}

	return false;
}

bool operator < (const CartographicPoint& a, const CartographicPoint& b)
{
	if (fabs(a.m_dX - b.m_dX) > MathVars::ZERO)
	{
		return (a.m_dX < b.m_dX);
	}
	else
	{
		if (fabs(a.m_dY - b.m_dY) > MathVars::ZERO)
		{
			return (a.m_dY < b.m_dY);
		}
	}

	return false;
}