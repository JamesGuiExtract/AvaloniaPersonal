//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	CCEHelper.h
//
// PURPOSE	:	 
//
// NOTES	:	Nothing
//
// AUTHORS	:	Duan Wang
//
//==================================================================================================
#pragma once

#include "CCE.H"
#include "ECurveParameter.h"

#include <string>

class ICurveCalculationEngine;

class EXP_CCE_DLL CCEHelper
{
public:
	// Take the pass-in cce and do operations upon it
	CCEHelper(ICurveCalculationEngine *pCCE);
	// Take a string in the format of bearing or angle (ex. "N23d34m23sE", or "12d34m12s"), 
	// and convert it into double in the format of radians, and use the double value to
	// set parameter of the cce
	void setCurveParameter(ECurveParameterType eParameterType, const std::string& strParamValue);
	// Return back a bearing, or an angle as a string in the format of bearing or angle
	std::string getCurveParameter(ECurveParameterType eParameterType);

	// Returns curve parameter as Bearing, Angle or Distance in double value
	// Require: eParameterType must be one of the Bearing, Angle or Distance type
	// Note: The bearing here is in terms of polar angle (0° = East)
	double getCurveBearingAngleOrDistanceInDouble(ECurveParameterType eParameterType);

private:
	ICurveCalculationEngine *m_pCCE;
};

