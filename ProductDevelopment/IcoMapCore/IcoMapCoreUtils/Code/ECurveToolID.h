//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ECurveToolID.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================
#pragma once

enum ECurveToolID  // corresponds to curve tools on the DrawingToolsDlg and CurveWizard
{
	kCurrentCurveTool = -1,
	kCurve1 = 0,		// MUST be zero-based cuz used as an index into a vector, ie vector[kCurve1]
	kCurve2,
	kCurve3,
	kCurve4,
	kCurve5,
	kCurve6,
	kCurve7,
	kCurve8,
	kCurveToolCnt		// MUST BE LAST ENUM - number of curve tools managed by this class
};
