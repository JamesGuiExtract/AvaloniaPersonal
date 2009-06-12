//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	CurveVariable.cpp
//
// PURPOSE	:	This is an implementation file for CurveVariable class.
//				The code written in this file makes it possible to implement 
//				the functionality to add all the Variable relation ships 
//				and to get desired variable value from the respective 
//				variable relation ship
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// CurveVariable.cpp : implementation file
//
#include "stdafx.h"
#include "CurveVariable.h"
#include "CurveVariableRelationship.h"

#include <algorithm>
#include <math.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

using namespace std;

//==========================================================================================
// CurveVariable
//==========================================================================================
CurveVariable::CurveVariable()
{
	m_dValue=0.0;
	m_xValue=0.0;
	m_yValue=0.0;
	m_bAssigned=false;
	m_iDecimal = 0;
	m_iSign = 0;
	m_pszBuffer="";
}
//==========================================================================================
//CurveVariable::CurveVariable(const CurveVariable& cv)
//{
//	// Copy data members
//	m_dValue = cv.m_dValue;
//	m_xValue = cv.m_xValue;
//	m_yValue = cv.m_yValue;
//	m_bAssigned = cv.m_bAssigned;
//	m_iDecimal = cv.m_iDecimal;
//	m_iSign = cv.m_iSign;
//
//	// Copy buffer into new buffer
//	unsigned long ulCopyLength = sizeof( cv.m_pszBuffer );
//	if (ulCopyLength > 0)
//	{
//		m_pszBuffer = new char[ulCopyLength + 1];
//		memcpy( m_pszBuffer, cv.m_pszBuffer, ulCopyLength );
//	}
//	else
//	{
//		// No buffer in cv, just make this an empty string
//		m_pszBuffer = "";
//	}
//}
//==========================================================================================
void CurveVariable::addVariableRelationship(CurveVariableRelationship *pVR)
{
	//adding the variable relationships to the vector
	vecVarRelationships.push_back(pVR);
}
//==========================================================================================
void CurveVariable::reset()
{
	m_bAssigned=false;
}
//==========================================================================================
bool CurveVariable::isCalculatable(vector<CurveVariableRelationship*> vecVRToExempt)
{
	//This variable is calculatable if it has been assigned a value
	//or if one of its equations are calculatable
	if(isAssigned())
	{
		return true;
	}

	//If one of the equations used to determine this variable is calculatable
	//then return true
	vector<CurveVariableRelationship *>::const_iterator iter;
	for(iter=vecVarRelationships.begin(); iter!=vecVarRelationships.end(); iter++)
	{
		// if the variable relationship is not in the exempt variable relationships vector
		if (find(vecVRToExempt.begin(), vecVRToExempt.end(), *iter) == vecVRToExempt.end())
		{
			if ((*iter)->isCalculatable(this, vecVRToExempt))
			{
				return true;
			}
		}
	}

	return false;
}
//==========================================================================================
bool CurveVariable::isAssigned()
{
	return m_bAssigned;
}
//==========================================================================================
void CurveVariable::assign(double dValue)
{
	m_dValue=dValue;
	m_bAssigned=true;
}
//==========================================================================================
void CurveVariable::assignPointParameter(double dxValue,double dyValue)
{
	m_xValue=dxValue;
	m_yValue=dyValue;
	m_bAssigned=true;
}
//==========================================================================================
bool CurveVariable::getBooleanValue(vector<CurveVariableRelationship*> vecVRToExempt)
{
	return getValue(vecVRToExempt) == 1.0;
}
//==========================================================================================
double CurveVariable::checkCurveAngle(double dValue)
{
	//checking for 0<Curve Parameter Angle<360
	double dMult = floor(dValue / 360);
	dValue = dValue - dMult * 360;
	return dValue;
}  
//==========================================================================================
double CurveVariable::getValue(vector<CurveVariableRelationship*> vecVRToExempt)
{
	//If a value has been assigned to this Curve Parameter,then return that value
	if(isAssigned())
	{
		return m_dValue;
	}
	else
	{
		//Return the value of the first equation that is claculatable if one of the
		//equations used to determine this variable is calculatable
		vector<CurveVariableRelationship *>::const_iterator iter;
		for(iter=vecVarRelationships.begin();iter!=vecVarRelationships.end();iter++)
		{
			// exclude the relationship that is already used to calculate another variable
			if (find(vecVRToExempt.begin(), vecVRToExempt.end(), *iter) == vecVRToExempt.end())
			{
				//Check whether this variable is calculatable, if so return 'True' other wise 'False'
				if((*iter)->isCalculatable(this, vecVRToExempt))
				{
					//Return the Value of the Curve Parameter
					return (*iter)->calculateX(this, vecVRToExempt);
				}
			}
		}
		UCLIDException e("ELI90222","Please Input sufficient parameters to get the required Curve Parameter");
		throw e;
	}
}
//==========================================================================================
double CurveVariable::getPointParamXValue(vector<CurveVariableRelationship*> vecVRToExempt)
{
	//If 'X' coordinate has been assigned to this variable,then return that value
	if(isAssigned())
	{
		return m_xValue;
	}
	else
	{
		//Return the value of the first equation that is claculatable if one of the
		//equations used to determine this variable is calculatable
		vector<CurveVariableRelationship *>::const_iterator iter;
		for(iter=vecVarRelationships.begin();iter!=vecVarRelationships.end();iter++)
		{
			// exclude the relationship that is already used to calculate another variable
			if (find(vecVRToExempt.begin(), vecVRToExempt.end(), *iter) == vecVRToExempt.end())
			{
				//Check whether this variable is calculatable, if so return 'True' other wise 'False'
				if((*iter)->isCalculatable(this, vecVRToExempt))
				{
					//Return the Value of the Curve Parameter
					return (*iter)->calculateX(this, vecVRToExempt);
				}
			}
		}
		UCLIDException e("ELI90223","Please Input sufficient parameters to get the required Curve Parameter X Co-ordinate");
		throw e;
	}
}
//==========================================================================================
double CurveVariable::getPointParamYValue(vector<CurveVariableRelationship*> vecVRToExempt)
{
	//If 'Y' coordinate has been assigned to this variable,then return that value
	if(isAssigned())
	{
		return m_yValue;
	}
	else
	{
		//Return the value of the first equation that is claculatable if one of the
		//equations used to determine this variable is calculatable
		vector<CurveVariableRelationship *>::const_iterator iter;
		for(iter=vecVarRelationships.begin();iter!=vecVarRelationships.end();iter++)
		{
			//This variable is unknown
			// exclude the relationship that is already used to calculate another variable
			if (find(vecVRToExempt.begin(), vecVRToExempt.end(), *iter) == vecVRToExempt.end())
			{
				//Check whether this variable is calculatable, if so return 'True' other wise 'False'
				if((*iter)->isCalculatable(this, vecVRToExempt))
				{
					//Return the Value of the Curve Parameter
					return (*iter)->calculateY(this, vecVRToExempt);
				}
			}
		}
		UCLIDException e("ELI90224","Please Input sufficient parameters to get the required Curve Parameter Y Co-ordinate");
		throw e;
	}
}
//==========================================================================================
bool CurveVariable::canGetValue(vector<CurveVariableRelationship*> vecVRToExempt)
{
	//If a value has been assigned to this Curve Parameter, then return 'True'
	if(isAssigned())
	{
		return m_bAssigned;
	}
	else
	{
		//Return 'True' if one of the equations used to determine this variable 
		//is calculatable
		vector<CurveVariableRelationship *>::const_iterator iter;
		for(iter=vecVarRelationships.begin();iter!=vecVarRelationships.end();iter++)
		{
			//this variable is unknown
			// exclude the relationship that is already used to calculate another variable
			if (find(vecVRToExempt.begin(), vecVRToExempt.end(), *iter) == vecVRToExempt.end())
			{
				//Check whether this variable is calculatable, if so return 'True' other wise 'False'
				if((*iter)->isCalculatable(this, vecVRToExempt))
				{
					//Return 'True' if this Curve Parameter can be calculatable
					return (*iter)->canCalculateX(this, vecVRToExempt);
				}
			}
		}
		return false;
	}
}
//==========================================================================================
bool CurveVariable::canGetPointParamXValue(vector<CurveVariableRelationship*> vecVRToExempt)
{
	//If 'X' coordinate has been assigned to this variable,then return 'True'
	if(isAssigned())
	{
		return m_bAssigned;
	}
	else
	{
		//Return 'True' if one of the equations used to determine this variable 
		//is calculatable
		vector<CurveVariableRelationship *>::const_iterator iter;
		for(iter=vecVarRelationships.begin();iter!=vecVarRelationships.end();iter++)
		{
			// exclude the relationship that is already used to calculate another variable
			if (find(vecVRToExempt.begin(), vecVRToExempt.end(), *iter) == vecVRToExempt.end())
			{
				//Check whether this variable is calculatable, if so return 'True' other wise 'False'
				if((*iter)->isCalculatable(this, vecVRToExempt))
				{
					//Return 'True' if this Curve Parameter can be calculatable
					return (*iter)->canCalculateX(this, vecVRToExempt);
				}
			}
		}
		return false; 
	}
}
//==========================================================================================
bool CurveVariable::canGetPointParamYValue(vector<CurveVariableRelationship*> vecVRToExempt)
{
	//If 'Y' coordinate has been assigned to this variable,then return 'True'
	if(isAssigned())
	{
		return m_bAssigned;
	}
	else
	{
		//Return 'True' if one of the equations used to determine this variable 
		//is calculatable
		vector<CurveVariableRelationship *>::const_iterator iter;
		for(iter=vecVarRelationships.begin();iter!=vecVarRelationships.end();iter++)
		{
			//This variable is unknown
			// exclude the relationship that is already used to calculate another variable
			if (find(vecVRToExempt.begin(), vecVRToExempt.end(), *iter) == vecVRToExempt.end())
			{
				//Check whether this variable is calculatable, if so return 'True' other wise 'False'
				if((*iter)->isCalculatable(this, vecVRToExempt))
				{
					//Return 'True' if this Curve Parameter can be calculatable
					return (*iter)->canCalculateY(this, vecVRToExempt);
				}
			}
		}
		return false; 
	}
}
//==========================================================================================

