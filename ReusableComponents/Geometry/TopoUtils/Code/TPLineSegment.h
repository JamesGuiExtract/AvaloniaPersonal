//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	TPLineSegment.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#ifndef TP_LINE_SEGMENT_H
#define TP_LINE_SEGMENT_H

#include "TopoUtils.h"

#include "TPPoint.h"

class EXPORT_TopoUtils TPLineSegment
{
public:
	TPLineSegment();
	TPLineSegment(const TPPoint& p1, const TPPoint& p2);
	TPLineSegment(const TPLineSegment& lineToCopy);
	TPLineSegment& operator=(const TPLineSegment& lineToAssign);

	bool contains(const TPPoint& p) const;
	bool intersects(const TPLineSegment& line2, TPPoint& intersectionTPPoint) const;
	
	double getSlope(void) const;
	TPPoint getMidTPPoint(void) const;
	double getLength(void) const;

	bool startsOrEndsAt(const TPPoint& p, bool& bStarts) const;

	EXPORT_TopoUtils friend bool operator == (const TPLineSegment& l1, const TPLineSegment& l2);
	// stub function for the purpose of exporting STL
	EXPORT_TopoUtils friend bool operator < (const TPLineSegment& l1, const TPLineSegment& l2);

	// the following two variables are purposely set to public scope.
	TPPoint m_p1, m_p2;
};


#endif // TP_LINE_SEGMENT_H