//**** Distance class, distance.h file****
//Copyright 1998-2000 UCLID Software, LLC
#include "stdafx.h"
#include "Distance.hpp"

#include <TPPoint.h>
#include <cpputil.h>

#include <UCLIDException.h>

using namespace std;


//--------------------------------------------------------------------------------------------------
AbstractMeasurement::EType Distance::getType() const
{
	return AbstractMeasurement::kDistance; 
}
//--------------------------------------------------------------------------------------------------
const string& Distance::getTypeAsString() const
{
	static string strType = "Distance";
	return strType;
}
//--------------------------------------------------------------------------------------------------
Distance::Distance()
{
}

//-----------------------------------------------------------------------------
Distance::Distance(const Distance& distance)
{
	m_DistanceCore = distance.m_DistanceCore;
}
//--------------------------------------------------------------------------------------------------
Distance::~Distance()
{
	//no pointer need delete
}
//--------------------------------------------------------------------------
Distance& Distance::operator =(const Distance& distance)
{
	m_DistanceCore = distance.m_DistanceCore;

	return *this;
}
//--------------------------------------------------------------------------------------------------
string Distance::asStringInUnit(EDistanceUnitType eOutUnit)
{
	return m_DistanceCore.asStringInUnit(eOutUnit);
}
//----------------------------------------------------
AbstractMeasurement* Distance::createNew()
{
	return new Distance();
}
//-------------------------------------------------------------------------------------
void Distance::evaluate(const TPPoint&p1, const TPPoint& p2)
{
	double dDistance = p1.distanceTo(p2);
	evaluate(::asString(dDistance).c_str());
}
//--------------------------------------------------------------------------------------------------
//Set distance string 
//Tokenize distance string, check how many units in this distance string.
//Put each unit to unit array, put each dist string to dist array
void Distance::evaluate(const char *pszDistance)
{
	m_DistanceCore.evaluate(pszDistance);
}
//-------------------------------------------------------------------------------------
//
bool Distance::isValid(void)
{
	return m_DistanceCore.isValid();
}

//-------------------------------------------------------------------------------------
vector<string> Distance::getAlternateStrings(void)
{
	vector<string> vecTemp;
	vecTemp.push_back(m_DistanceCore.getOriginalInputString());
	return vecTemp;
}
//-------------------------------------------------------------------------------------
EDistanceUnitType Distance::getDefaultDistanceUnit()
{
	return m_DistanceCore.getDefaultDistanceUnit();
}
//-------------------------------------------------------------------------------------
double Distance::getDistanceInUnit(EDistanceUnitType eOutUnit)
{
	return m_DistanceCore.getDistanceInUnit(eOutUnit);
}
//-------------------------------------------------------------------------------------
const vector<string>& Distance::getDistanceUnitStrings() const
{
	return m_DistanceCore.getDistanceUnitStrings();
}
//-------------------------------------------------------------------------------------
//Get the standard distance string if it's valid
string Distance::asString(void)
{
	throw UCLIDException("ELI02962", "asString() is no longer supported in Distance class");
}
//-------------------------------------------------------------------------------------
string Distance::interpretedValueAsString(void)
{
	throw UCLIDException("ELI02961", "interpretedValueAsString() is no longer supported in Distance class");
}
//-------------------------------------------------------------------------------------
string Distance::getEvaluatedString()
{
	return m_DistanceCore.getOriginalInputString();
}
//--------------------------------------------------------------------------------------------------
void Distance::resetVariables()
{
	m_DistanceCore.reset();
}
//--------------------------------------------------------------------------------------------------
void Distance::setDefaultDistanceUnit(EDistanceUnitType eDefaultUnit)
{
	m_DistanceCore.setDefaultDistanceUnit(eDefaultUnit);
}
//--------------------------------------------------------------------------------------------------
string Distance::getStringFromUnit(EDistanceUnitType eUnit)
{
	return m_DistanceCore.getStringFromUnit(eUnit);
}
//--------------------------------------------------------------------------------------------------
EDistanceUnitType Distance::getUnitFromString(const string& strUnit)
{
	return m_DistanceCore.getUnitFromString(strUnit);
}
//--------------------------------------------------------------------------------------------------

 
