//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveTool.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#pragma once

#include "CurveToolManager.h"

#include <ECurveParameter.h>
#include <ECurveToolID.h>

#include <list>
#include <map>
#include <string>

class CurveTool  
{
public:
	CurveTool(ECurveToolID id);
	virtual ~CurveTool();

	static std::string sDescribeCurveParameter(ECurveParameterType eCurveParameter);
	bool isToggleCurveDeltaAngleEnabled(void) {return m_bToggleCurveDeltaAngleEnabled;}
	bool isToggleCurveDirectionEnabled(void) {return m_bToggleCurveDirectionEnabled;}
	std::string generateToolTip(void);
	ECurveParameterType getCurveParameter(int iParameterID) const;
	CurveParameters getCurveParameters(void) const;
	ECurveToolID getCurveToolID() {return m_eCurveToolID;}
	std::list<int> getSortedCurveParameterIDList(void) const;
	std::string getToolTip(void) const {return m_strToolTip;}
	bool restoreState(void);
	void reset(void);
	bool saveState(void) const;
	void setCurveParameter(int iParameterID,ECurveParameterType eParameterType);
	void updateStateOfCurveToggles(void);

protected:
	bool m_bToggleCurveDeltaAngleEnabled;						// whether or not the ToggleDeltaAngle feature is enabled for this curve
	bool m_bToggleCurveDirectionEnabled;						// whether or not the ToggleCurveDirection feature is enabled for ths icurve
	typedef std::map<int,ECurveParameterType> ParameterMap;
	ParameterMap m_mapParameter;								// associates a curve parameter type with an curveParameterID

	void enableToggleCurveDeltaAngle(bool bEnable) {m_bToggleCurveDeltaAngleEnabled = bEnable;}
	void enableToggleCurveDirection(bool bEnable) {m_bToggleCurveDirectionEnabled = bEnable;}

private:
	ECurveToolID m_eCurveToolID;			// identifies this curve tool for persitent storage
	std::string m_strToolTip;
};
