//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	EInputType.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan (Aug 2001 to present)
//			John Hurd (till July 2001)
//
//==================================================================================================
#pragma once

enum EInputType
{
	kNone = 0,		// not expecting any input
	kAngle,			// expecting an angle as input
	kBearing,		// expecting a bearing as input
	kDistance,		// expecting a distance as input
	kPoint,			// expecting a point as input
	kToggleCurve,	// expecting the curve direction or bulge (either > or < 180°)
	kToggleAngle
};

enum ECurveToggleInput
{
	kToggleUndefined = 0,
	kToggleDeltaIsLessThan180Degrees,
	kToggleDeltaIsGreaterThan180Degrees,
	kToggleCurveDirectionIsLeft,
	kToggleCurveDirectionIsRight
};

enum EDeflectionAngleToggleInput
{
	kToggleAngleUndefined = 0,
	kToggleAngleLeft,
	kToggleAngleRight
};