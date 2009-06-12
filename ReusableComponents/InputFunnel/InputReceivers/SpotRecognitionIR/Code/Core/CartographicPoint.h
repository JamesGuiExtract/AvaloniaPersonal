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
#pragma once

class CartographicPoint
{
public:
	CartographicPoint();
	CartographicPoint(double dX, double dY);

	friend bool operator == (const CartographicPoint& a, const CartographicPoint& b);
	friend bool operator < (const CartographicPoint& a, const CartographicPoint& b);

	double m_dX;
	double m_dY;
};
