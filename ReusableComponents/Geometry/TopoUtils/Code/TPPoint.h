//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	TPPoint.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#ifndef TPPoint_H
#define TPPoint_H

#include "TopoUtils.h"

class EXPORT_TopoUtils TPPoint 
{
public:
	TPPoint();
	TPPoint(double x, double y);
	TPPoint(const TPPoint& p);
	TPPoint& operator = (const TPPoint& p);

	double distanceTo(const TPPoint& p) const;
	double angleTo(const TPPoint& p) const;

	EXPORT_TopoUtils friend bool operator == (const TPPoint& a, const TPPoint& b);
	EXPORT_TopoUtils friend bool operator < (const TPPoint& a, const TPPoint& b);

	// the following variables purposely set with public scoping
	double m_dX, m_dY;
};

#endif // TPPoint_H
