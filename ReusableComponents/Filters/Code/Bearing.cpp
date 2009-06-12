//===================================================================
//COPYRIGHT UCLID SOFTWARE, LLC. 1998
//
//FILE:		bearing.hpp
//
//PURPOSE:	Bearing string filter, evalute the bearing string
//
//NOTES:
//
//AUTHOR:	Jinsong Ye
//
//===================================================================
//
//===================================================================

#include "stdafx.h"
#include "Bearing.hpp"

#include <TPPoint.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <cassert>

using namespace std;

vector<string> Bearing::m_svecStrStartUnit;  //vector hold all valid start direction "North" "nor"...
vector<string> Bearing::m_svecStrEndUnit;    //vector hold all valid end direction   "east" "E"....
vector<string> Bearing::m_svecStrDegUnit;    //vector hold all valid degree unit " deg ", "D" ...
vector<string> Bearing::m_svecStrMinUnit;    //vector hold all valid minute unit " min ", minutes"
vector<string> Bearing::m_svecStrSecUnit;    //vectro hold all valid second unit " SECOND " "s"..
vector<string> Bearing::m_svecStrPure;       //vector hold all valid pure direction "east" "W" "nor"...
CMutex Bearing::m_sMutex;

//-------------------------------------------------------------------------------------
AbstractMeasurement::EType Bearing::getType() const
{
	return AbstractMeasurement::kBearing; 
}
//--------------------------------------------------------------------------------------------------
const string& Bearing::getTypeAsString() const
{
	static const string strType = "Bearing";
	return strType;
}
//--------------------------------------------------------------------------------------------------
Bearing::Bearing()
{
	resetVariables();   //set private member value to default value
	setUnit();          //set valid unit to vectors
}
//--------------------------------------------------------------------------
Bearing::Bearing(const Bearing& bearing)
{
	m_dRad = bearing.m_dRad;
	m_dDeg = bearing.m_dDeg;
	
	m_dDegree = bearing.m_dDegree;
	m_dMinute = bearing.m_dMinute;
	m_dSecond = bearing.m_dSecond;

	m_chStartDir = bearing.m_chStartDir;
	m_chEndDir = bearing.m_chEndDir;

	m_bValid = bearing.m_bValid;       
	
	// ARVIND ADDED 7/18/1999
	m_bIsOriginalBearingValid = bearing.m_bIsOriginalBearingValid;

	m_bPureFlag = bearing.m_bPureFlag;     
	m_bValidPure = bearing.m_bValidPure;     
	m_bPossibleFlag = bearing.m_bPossibleFlag;  
	m_bStartOption = bearing.m_bStartOption;   
	m_bEndOption = bearing.m_bEndOption;    

	m_strStartDir = bearing.m_strStartDir;    
	m_strEndDir = bearing.m_strEndDir;      
	m_strMidPart = bearing.m_strMidPart;     

	m_strPure = bearing.m_strPure;        
	m_strBearString = bearing.m_strBearString;  
	m_strStandString = bearing.m_strStandString; 

	m_strDegString = bearing.m_strDegString;   
	m_strMinString = bearing.m_strMinString;   
	m_strSecString = bearing.m_strSecString;   
	m_strDegUnit = bearing.m_strDegUnit;     
	m_strMinUnit = bearing.m_strMinUnit;     
	m_strSecUnit = bearing.m_strSecUnit;     
	
	m_vecStrAlt = bearing.m_vecStrAlt;
}
//--------------------------------------------------------------------------------------------------
Bearing::Bearing(const char *pszBearing)
{
	resetVariables();
	setUnit();
	
	evaluate(pszBearing);   
}

//--------------------------------------------------------------------------------------------------
Bearing::~Bearing()
{
	//no pointer need delete
}
//----------------------------------------------------------------------------
Bearing& Bearing::operator =(const Bearing& bearing)
{
	AbstractMeasurement::operator=(bearing);

	this->m_dRad = bearing.m_dRad;  
	this->m_dDeg = bearing.m_dDeg;      
	
	this->m_dDegree = bearing.m_dDegree;
	this->m_dMinute = bearing.m_dMinute;
	this->m_dSecond = bearing.m_dSecond;

	
	this->m_chStartDir = bearing.m_chStartDir;
	this->m_chEndDir = bearing.m_chEndDir;

	this->m_bValid = bearing.m_bValid;
	
	// ARVIND ADDED 7/18/1999
	this->m_bIsOriginalBearingValid = bearing.m_bIsOriginalBearingValid;

	this->m_bPureFlag = bearing.m_bPureFlag;     
	this->m_bValidPure = bearing.m_bValidPure;     
	this->m_bPossibleFlag = bearing.m_bPossibleFlag;  
	this->m_bStartOption = bearing.m_bStartOption;   
	this->m_bEndOption = bearing.m_bEndOption;    

	this->m_strStartDir = bearing.m_strStartDir;    
	this->m_strEndDir = bearing.m_strEndDir;      
	this->m_strMidPart = bearing.m_strMidPart;     

	this->m_strPure = bearing.m_strPure;        
	this->m_strBearString = bearing.m_strBearString;  
	this->m_strStandString = bearing.m_strStandString; 

	
	this->m_strDegString = bearing.m_strDegString;   
	this->m_strMinString = bearing.m_strMinString;   
	this->m_strSecString = bearing.m_strSecString;   
	this->m_strDegUnit = bearing.m_strDegUnit;     
	this->m_strMinUnit = bearing.m_strMinUnit;     
	this->m_strSecUnit = bearing.m_strSecUnit;     
	
	this->m_vecStrAlt = bearing.m_vecStrAlt;

	return *this;
}
//--------------------------------------------------------------------------------------------------
AbstractMeasurement* Bearing::createNew()
{
	return new Bearing();
}
//--------------------------------------------------------------------------------------------------
void Bearing::setUnit()
{
	CSingleLock lg(&m_sMutex, TRUE);

	static bool bFirstTime = true;

	if (bFirstTime)
	{
		bFirstTime = false;

		//set valid start direction
		m_svecStrStartUnit.push_back("");
		m_svecStrStartUnit.push_back("N");
		m_svecStrStartUnit.push_back("N-");
		m_svecStrStartUnit.push_back("N.");
		m_svecStrStartUnit.push_back("NOR");
		m_svecStrStartUnit.push_back("NOR-");
		m_svecStrStartUnit.push_back("NTH");
		m_svecStrStartUnit.push_back("NTH-");
		m_svecStrStartUnit.push_back("NORTH");
		m_svecStrStartUnit.push_back("NORTHERLY");
		m_svecStrStartUnit.push_back("NORTH-");
		m_svecStrStartUnit.push_back("S");
		m_svecStrStartUnit.push_back("S-");
		m_svecStrStartUnit.push_back("S.");
		m_svecStrStartUnit.push_back("SOU");
		m_svecStrStartUnit.push_back("SOU-");
		m_svecStrStartUnit.push_back("STH");
		m_svecStrStartUnit.push_back("STH-");
		m_svecStrStartUnit.push_back("SOUTH");
		m_svecStrStartUnit.push_back("SOUTH-");
		m_svecStrStartUnit.push_back("SOUTHERLY");
		
		//set valid end direction
		m_svecStrEndUnit.push_back("");
		m_svecStrEndUnit.push_back("E");
		m_svecStrEndUnit.push_back("-E");
		m_svecStrEndUnit.push_back("E.");
		m_svecStrEndUnit.push_back("EST");
		m_svecStrEndUnit.push_back("-EST");
		m_svecStrEndUnit.push_back("EAST");
		m_svecStrEndUnit.push_back("-EAST");
		m_svecStrEndUnit.push_back("EASTERLY");
		m_svecStrEndUnit.push_back("W");
		m_svecStrEndUnit.push_back("-W");
		m_svecStrEndUnit.push_back("W.");
		m_svecStrEndUnit.push_back("WST");
		m_svecStrEndUnit.push_back("-WST");
		m_svecStrEndUnit.push_back("WEST");
		m_svecStrEndUnit.push_back("-WEST");
		m_svecStrEndUnit.push_back("WESTERLY");

		//set degree uint
		m_svecStrDegUnit.push_back("");
		m_svecStrDegUnit.push_back("*");
		m_svecStrDegUnit.push_back("`");
		m_svecStrDegUnit.push_back("_");
		m_svecStrDegUnit.push_back("-");
		m_svecStrDegUnit.push_back("D");
		m_svecStrDegUnit.push_back("DEG");
		m_svecStrDegUnit.push_back("DEG.");
		m_svecStrDegUnit.push_back("DEGREE");
		m_svecStrDegUnit.push_back("DEGREES");
		m_svecStrDegUnit.push_back("%%D");
		m_svecStrDegUnit.push_back("°");
		m_svecStrDegUnit.push_back("º");
		m_svecStrDegUnit.push_back("'");
		m_svecStrDegUnit.push_back("’");
		m_svecStrDegUnit.push_back("\"");
		m_svecStrDegUnit.push_back("O");  //this is string "o"
		m_svecStrDegUnit.push_back(".");
		m_svecStrDegUnit.push_back("?");
		
		//set minute unit
		m_svecStrMinUnit.push_back("");
		m_svecStrMinUnit.push_back("_");
		m_svecStrMinUnit.push_back("-");
		m_svecStrMinUnit.push_back("'");
		m_svecStrMinUnit.push_back("M");
		m_svecStrMinUnit.push_back("MIN");
		m_svecStrMinUnit.push_back("MIN.");
		m_svecStrMinUnit.push_back("MINUTE");
		m_svecStrMinUnit.push_back("MINUTES");
		m_svecStrMinUnit.push_back("\"");
		m_svecStrMinUnit.push_back("*");
		m_svecStrMinUnit.push_back("`");
		m_svecStrMinUnit.push_back("‘");	//145 <ANSI>
		m_svecStrMinUnit.push_back("’");	//146 <ANSI>
		 
		//set second unit
		m_svecStrSecUnit.push_back("");
		m_svecStrSecUnit.push_back("_");
		m_svecStrSecUnit.push_back("-");
		m_svecStrSecUnit.push_back("\"");
		m_svecStrSecUnit.push_back("\"-");
		m_svecStrSecUnit.push_back("---");
		m_svecStrSecUnit.push_back("S");
		m_svecStrSecUnit.push_back("SEC");
		m_svecStrSecUnit.push_back("SEC.");
		m_svecStrSecUnit.push_back("SECOND");
		m_svecStrSecUnit.push_back("SECONDS");
		m_svecStrSecUnit.push_back("'");
		m_svecStrSecUnit.push_back("‘");	//145 <ANSI>
		m_svecStrSecUnit.push_back("’");	//146 <ANSI>
		m_svecStrSecUnit.push_back("*");
		m_svecStrSecUnit.push_back("`");
		m_svecStrSecUnit.push_back("“");	//147 <ANSI>
		m_svecStrSecUnit.push_back("”");	//148 <ANSI>

		//set pure direction
		m_svecStrPure.push_back("NORTHERLY");
		m_svecStrPure.push_back("SOUTHERLY");
		m_svecStrPure.push_back("WESTERLY");
		m_svecStrPure.push_back("EASTERLY");
		m_svecStrPure.push_back("NORTH");
		m_svecStrPure.push_back("NOR");
		m_svecStrPure.push_back("SOU");
		m_svecStrPure.push_back("SOUTH");
		m_svecStrPure.push_back("EAST");
		m_svecStrPure.push_back("WEST");
		m_svecStrPure.push_back("N");
		m_svecStrPure.push_back("S");
		m_svecStrPure.push_back("E");
		m_svecStrPure.push_back("W");
		m_svecStrPure.push_back("NTH");
		m_svecStrPure.push_back("STH");
		m_svecStrPure.push_back("EST");
		m_svecStrPure.push_back("WST");
	}
}	

//-------------------------------------------------------------------------------------
//Set private member
//Set boolean variable to false
//Set string to empty ""
void Bearing::resetVariables()
{
	m_bSuppressAlternates = false;
	m_bValid = false;    //for whole input bearing string
	
	// ARVIND ADDED 7/18/1999
	m_bIsOriginalBearingValid = false;

	m_bPureFlag = false;      //if nnnn35, the bPureFlag will be true, but
	m_bValidPure = false;		//bValidPure will be false.

	
	m_bPossibleFlag = false;
	m_bStartOption  = false;
	m_bEndOption    = false;

	//init string
	m_strPure       = "";
	m_strBearString = "";
	m_strStandString = "";
	m_strDegString = "00";
	m_strDegUnit   = "";
	m_strMinString = "00";
	m_strMinUnit   = "";
	m_strSecString = "00";
	m_strSecUnit   = "";

	m_strStartDir = "";
	m_strEndDir   = "";
	m_strMidPart  = "";
	
	m_dDegree = -1;
	m_dMinute = 0;
	m_dSecond = 0;

	m_dRad = 0;
	m_dDeg = 0;

	//clear alternate string vector
	m_vecStrAlt.clear(); 
}

//--------------------------------------------------------------------------------------------------
void Bearing::evaluate(const TPPoint&p1, const TPPoint& p2)
{
	double dRadians = p1.angleTo(p2);
	evaluateRadians(dRadians);
}
//--------------------------------------------------------------------------------------------------
//Copy char* pszBearing to bearing string
//Spilt string to 3 parts
//Check each part 
void Bearing::evaluate(const char *pszBearing)
{
	resetVariables();

	m_strEvaluatedString = pszBearing;

	// THIS IS A DEBUG INITIATIVE FOR THE INTERMITTENT CASES WHERE BEARINGS
	// EVALUATION WORKS PECULIARY
	// ADDED BY ARVIND on 7/20/1999
/*
	{
		string strOutFile = icoMap.getApplicationSettings()->getIcoMapDirectory();
		strOutFile += "\\bin\\bearings.dat";

		string strTime = CTime::GetCurrentTime().Format("%H:%M:%S");
		string strDate = CTime::GetCurrentTime().Format("%m/%d/%Y");

		ofstream outfile(strOutFile.c_str(), ios::app | ios::out);
		if (outfile)
		{
			outfile << strDate << " " << strTime << " " << strlen(pszBearing) << " " << pszBearing << endl;
			outfile.close();
		}
	}
*/
	//debugPrint("eval. bear\n");
	//debugPrint(bValid ? "yes":"no");
	//debugPrint("\n");

	//setUnit();

    // just create a vector of all "invalid" character positions in case
    // the string is not valid
    setInvalidPositions(pszBearing);
	
	string strTemp = pszBearing;

	// ARVIND ADDED ON 7/17/1999
	// no matter what, we want the first string in the 
	// alternate vectors to be the bearing string itself.
	// we don't want to automatically ignore the tilde's
	// before evaluating.
	m_vecStrAlt.push_back(strTemp);	

	// ARVIND ADDED 7/18/1999
	// first parse the string as it is.  If it is valid, great.
	// if not, remove the tilde's and then parse again
	{
		m_strBearString = strTemp;

		preProcessString(m_strBearString);

		//cout<<"*bearing string* "<<strBearString<<endl;
		tokenString();  //spilt the string to three parts, 1)start direction. 2)degree part
						//3)end direction
	
		checkString();  //check three parts

		// store whether the string is valid or not in bIsOriginalBearingValid
		// which is the bool that isValid() returns!
		m_bIsOriginalBearingValid = m_bValid;
	}

	//debugPrint("eval. bear 1\n");
	//debugPrint(bValid ? "yes":"no");
	//debugPrint("\n");

	// ARVIND ADDED ON 7/18/1999
	// create a vector to store the alterate strings
	// from the original strings
	vector<string> vecStrOrigAlt = m_vecStrAlt;

	// ARVIND ADDED 7/18/1999
	// the current string is invalid. Remove
	// the tilde's and re-determine if the string
	// could be valid -> purpose is to populate the
	// alternate strings vector accordingly.
	if (!m_bValid)
	{
		// reset variables as if we are re-evaluating the string
		// without the tilde.
		resetVariables();

		m_strBearString = checkTilde(strTemp);

		preProcessString(m_strBearString);

		//cout<<"*bearing string* "<<strBearString<<endl;
		tokenString();  //spilt the string to three parts, 1)start direction. 2)degree part
						//3)end direction

		checkString();  //check three parts

		// now prepend the original alternating strings vector
		// to the current set of alternating strings vector
		if (!m_bSuppressAlternates)
		{
			m_vecStrAlt.insert(m_vecStrAlt.begin(), vecStrOrigAlt.begin(), vecStrOrigAlt.end());
		}

		//debugPrint("eval. bear z\n");
		//debugPrint(bValid ? "yes":"no");
		//debugPrint("\n");
	}

	//debugPrint("eval. bear. end.\n");
	//debugPrint(bValid ? "yes":"no");
	//debugPrint("\n");
}
//--------------------------------------------------------------------------------------------------
//Accept any value (greater than 2*PI or negative value
//if dRadians > 2*PI, newValue = dRadians - 2*PI
//if dRadians <0, newValue = dRadians + 2*PI
void Bearing::evaluateRadians(const double& dRadians)
{
	m_bValid = m_bValidPure = false;

	// TODO: is this appropriate?
	m_strEvaluatedString = "";

	//first, check the radians is valid.
	//second, translate radians to angle
	//third, get direction 

	double dTempAngle = -1;  
	int    iTempAngle = -1;

	double dValue = dRadians;
	m_dRad = dRadians;

	// make sure radians is valid
	// No matter dValue > 2PI or dValue < 0 or any other values, 
	// use the following one expression will be sufficient
	// For instance: dValue = 65, dMult = floor (65/(2*PI)) = 10.0
	// dValue = 65 - 2*PI*10.0 = 2.168147...
	// For instance: dValue = 2, dMult = floor (2/(2*PI)) = 0.0
	// dValue = 2 - 2*PI*0.0 = 2
	// For instance: dValue = -20, dMult = floor (-20/(2*PI)) = -4.0
	// dValue = -20 - 2*PI*(-4.0) = 5.132741...
	double dMult = floor(dValue / (2 * MathVars::PI));
	dValue = dValue - dMult * 2 * MathVars::PI;

	if ((dValue < 0) || (dValue >= 2 * MathVars::PI))
	{
		//should not be here
		m_bValid = false;
	}
	else
	{
		m_bValid = true;

		dTempAngle = (dValue * 180 / MathVars::PI);  //translate radians to angle

	    iTempAngle = (int) dTempAngle;       //for if conditionn (int == int)
		 
		//double d = 89.99    int i = 90
		//double d = 90.01    int i = 90
		//double d = 90.45    int i = 89
		//double d = 89.45    int 1 = 88
		//only d - i <= 0.1,  i does not change
		double dDecimal = dTempAngle - iTempAngle;
		const double dOneTenthOfASecond = 1.0 / 3600.0 / 10.0;
	
		if (dDecimal > (1 - dOneTenthOfASecond))
		{
			iTempAngle++;
		}
		
		// ARVIND's CHANGE ON 12/13/2000
		// the following two lines were commented out, as they don't make sense
		// (seems like these 2 lines are related to IcoMap defect # 1440)
		//else if (dDecimal > 0.01) // TODO: where is this 0.01 coming from?
		//	iTempAngle--;
		
		// the smallest angle we can represent in our [N|S]XXDXX'XX"[E|W] is one 
		// second, which is equal to 1/3600 of a degree.  The tolerance we wish
		// to use to determine if an angle is zero or not is 1/10 of a second.
		bool bDecimalLessThanOneTenthOfASecond = dDecimal < dOneTenthOfASecond;

		//get direction, and degree part(string)
		//special case when angle = 0, 90, 180, 270, it's pure direction
		if (bDecimalLessThanOneTenthOfASecond && (iTempAngle == 0 || iTempAngle == 360))
		{
			m_bValidPure = true;
			m_strPure = "EAST";
		}
		else if (dTempAngle  < 90)
		{
			dTempAngle = 90 - dTempAngle;
		  	m_chStartDir = 'N';   
			m_chEndDir   = 'E';
			radiansToString(dTempAngle);
		}
		else if (bDecimalLessThanOneTenthOfASecond && iTempAngle == 90)
		{
			m_bValidPure = true;
			m_strPure = "NORTH";
		}
		else if(dTempAngle < 180)
		{	 
			m_chStartDir = 'N';
			m_chEndDir   = 'W';
			radiansToString(dTempAngle - 90);
		}
		else if (bDecimalLessThanOneTenthOfASecond && iTempAngle == 180)
		{
			m_bValidPure = true;
			m_strPure = "WEST";
		}
		else if (dTempAngle < 270)
		{
			dTempAngle = 270 - dTempAngle;
			m_chStartDir = 'S';
			m_chEndDir   = 'W';
			radiansToString(dTempAngle);
		}
		else if (bDecimalLessThanOneTenthOfASecond && iTempAngle == 270)
		{
			m_bValidPure = true;
			m_strPure = "SOUTH";
		}
		else if (dTempAngle < 360)
		{
			m_chStartDir = 'S';
			m_chEndDir   = 'E';
			radiansToString(dTempAngle - 270);
		}
		
		setStandString();
		//set alternate string
		m_vecStrAlt.push_back(m_strStandString);
	}

	// ARVIND ADDED 7/18/1999
	// the bIsOriginalBearingValid variable is used in isValid()
	m_bIsOriginalBearingValid = m_bValid;

	m_strEvaluatedString = asString();
}

//--------------------------------------------------------------------------------------------------
bool Bearing::isValid(void)
{
	// ARVIND COMMENTED ON 7/18/1999
	// return bValid;

	// ARVIND ADDED ON 7/18/1999
	return m_bIsOriginalBearingValid;
}

//--------------------------------------------------------------------------------------------------
vector<string> Bearing::getAlternateStrings(void)
{
	if (m_bSuppressAlternates)
	{
		return vector<string>();
	}

	// ARVIND ADDED ON 7/18/1999
	// TODO: the following concept of cleaning up at this point needs to be
	// taken care of.
	{
		vector <string> vecTemp = m_vecStrAlt;
		vector <string>::const_iterator iter;
		
		m_vecStrAlt.clear();

		for (iter = vecTemp.begin(); iter != vecTemp.end(); iter++)
		{
			if (!vectorContainsElement(m_vecStrAlt, *iter))
			{
				m_vecStrAlt.push_back(*iter);
			}
		}

		return m_vecStrAlt;
	}
}

//-------------------------------------------------------------------------------------------------
double Bearing::getDegrees(void)
{
	//first check valid
	//second check if it's pure direction, get special degrees
	//third it's not pure direction, then use direction to get degree
	//convert_angle(...) function is help to convert bearing angle to AutoCad angle

	if(!isValid())
	{
		throw UCLIDException ("ELI00426", "ERROR: Invalid bearing!, No degree");
		return -1;
	}

	double angle = 0;

	//second if it's pure direction
	if(m_bValidPure)   
	{
		return getPureDegrees();
	}

	if(m_chStartDir == 'N' && m_chEndDir == 'E')
	{
		angle = 90 - convert_angle(m_dDegree, m_dMinute, m_dSecond);
	}
	else if(m_chStartDir == 'N' && m_chEndDir == 'W')
	{
		angle = 90 + convert_angle(m_dDegree, m_dMinute, m_dSecond);
	}
	else if(m_chStartDir == 'S' && m_chEndDir == 'W')
	{
		angle = 270 - convert_angle(m_dDegree, m_dMinute, m_dSecond);
	}
	else if(m_chStartDir == 'S' && m_chEndDir == 'E')
	{
		angle = 270 + convert_angle(m_dDegree, m_dMinute, m_dSecond);
	}
	else //start or end direction is not specifed
	{
		//cout<<"dir** "<<chStartDir<<" End dir** "<<chEndDir<<endl;
		UCLIDException uclidEx("ELI00427", "ERROR: Invalid bearing, need direction");
		uclidEx.addDebugInfo("chStartDir", ValueTypePair(m_chStartDir));
		throw uclidEx;
	}

	m_dDeg = angle;
	
	if(isInReverseMode())
	{
		double dTemp = m_dDeg + 180;
		
		if(dTemp >= 360)
		{
			dTemp = dTemp - 360;
		}

		return dTemp;
	}

	return m_dDeg;
}

double Bearing::getRadians()
{
	//first check valid
	if(!m_bValid) // ARVIND MODIFIED 7/18/1999 - FROM if (!isValid())
	{
		throw UCLIDException ("ELI00428", "ERROR: Invalid Angle!, No degree");
		return -1;
	}
	else
	{
		double dTemp = getDegrees();

		m_dRad = dTemp * MathVars::PI / 180;

		return m_dRad;
	}
}

//-------------------------------------------------------------------------------------------------
double Bearing::getPureDegrees()
{
	//calculate pure direction by check the first char in pure string
	//East   is 0
	//North  is 90
	//West   is 180
	//South  is 270
	double dAngle = -11;

	if (m_strPure[0] == 'N')
	{
		dAngle = 90;
	}
	else if (m_strPure[0] == 'E')
	{
		dAngle = 0;
	}
	else if (m_strPure[0] == 'W')
	{
		dAngle = 180;
	}
	else if (m_strPure[0] == 'S')
	{
		dAngle = 270;
	}

	if (isInReverseMode())
	{
		dAngle = dAngle + 180; 

		if (dAngle >= 360)
		{
			dAngle = dAngle - 360;
		}
	}

	return dAngle;
}

//-------------------------------------------------------------------------------------------------
string Bearing::asString(void)
{
	if(!isValid()) 
	{
		return "ERROR: Invalid bearing, No standard string";  
	}

	return getAlternateStrings()[0];
}
//--------------------------------------------------------------------------------------------------
string Bearing::interpretedValueAsString(void)
{
	string strRet("");

	if(!isValid()) 
	{
		strRet = "ERROR: Invalid bearing, No standard string";
		  
	}

	if(m_bValidPure)
	{
		if(m_strPure[0] == 'N')
		{
			strRet = "N";

			if (isInReverseMode())
			{
				strRet = "S";
			}
		}
		else if(m_strPure[0] == 'E')
		{
			strRet = "E";

			if (isInReverseMode())
			{
				strRet = "W";
			}
		}
		else if(m_strPure[0] == 'W')
		{
			strRet = "W";

			if (isInReverseMode())
			{
				strRet = "E";
			}
		}
		else if(m_strPure[0] =='S')
		{
			strRet = "S";

			if (isInReverseMode())
			{
				strRet = "N";
			}
		}
		else
		{
			strRet = "    error: invalid direction";		   
		}
	}
	else
	{
		string strTemp("");
		strTemp = m_chStartDir;
		
		if (isInReverseMode())
		{
			if (_stricmp(strTemp.c_str(), "N") == 0)
			{
				strTemp = "S";
			}
			else if (_stricmp(strTemp.c_str(), "S") == 0)
			{
				strTemp = "N";
			}
		}

		strRet =  strTemp;
		strRet +=  m_strDegString;
		
		strRet +=  'D';
		strRet +=  m_strMinString;
		strRet +=  '\'';
		strRet +=  m_strSecString;
		strRet +=  '\"';
		
		strTemp = m_chEndDir;

		if (isInReverseMode())
		{
			if (_stricmp(strTemp.c_str(), "E") == 0)
			{
				strTemp = "W";
			}
			else if (_stricmp(strTemp.c_str(), "W") == 0)
			{
				strTemp = "E";
			}
		}

		strRet +=  strTemp;	
	}

	return strRet;

}
//--------------------------------------------------------------------------------------------------
void Bearing::tokenString()
{
	//split the input string to three parts, start dir, degree, end dir
	int iIndex = 0;
	int iCount1 = 0;  //begin for second part
	int iCount2 = 0;  //end for second part

	string strTemp("");
	int iLength = m_strBearString.length();
	//skip space in strBearString
	while (iIndex < iLength)
	{
		// Check if the character is larger than zero first in order
		// to ignore some symbole out of normal ASCII range, which cause crash in isspace() [P10: 3097]
		if(m_strBearString[iIndex] >= 0 && !isspace(m_strBearString[iIndex]) && m_strBearString[iIndex] != '-')
		{
			strTemp += m_strBearString[iIndex];
		}

		iIndex++;
	}

	iIndex =0;
	iLength = strTemp.length();

	//get the first part(start direction), scan the string from first char
	//while(iIndex < iLength && !isdigit(strTemp[iIndex]) )
	while(iIndex < iLength && !(IsCharAlphaNumeric(strTemp[iIndex]) && (!IsCharAlpha(strTemp[iIndex]))))
	{
		// Check if the character is larger than zero first in order
		// to ignore some symbole out of normal ASCII range, which cause crash in isspace() [P10: 3097]
		if(strTemp[iIndex] > 0 && !isspace(strTemp[iIndex]) && strTemp[iIndex] != '-')
		{
			m_strStartDir += toupper(strTemp[iIndex]);
		}

		iIndex ++;
	}

	//****check pure direction
	if( iIndex > 0 && iIndex == iLength)
	{
		m_bPureFlag = true;
	}
	else
	{
		iCount1 = iIndex;

		//get the third part(end direction), check the last word
		string strForCheck;
		iCount2 = iLength;
		
		bool bFindEndDir = false;

		for (int i = 0; i < 5; i++)
		{
			if (iCount2 <= 0)
			{
				break;
			}
			char ch = toupper(strTemp[iCount2-1]);
			strForCheck = ch + strForCheck;
			//cout<<"string for checking is ' "<<strForCheck<<endl;
			if(inEndVector(strForCheck))  //check this string if in End vector
			{
				if((toupper(strTemp[iCount2-2]) != 'E') && (toupper(strTemp[iCount2-2]) != 'T')
					&& (toupper(strTemp[iCount2-2]) != 'W'))
				{	//avoid MINUTE AND DEGREE and WEST
					//find the end direction, set iCount2 for second part
					if(strTemp[iCount2-2] == '-')
					{
						m_strEndDir = strTemp[iCount2-2] + strForCheck;
						iCount2--;
					}
					else
					{
						m_strEndDir = strForCheck;  //set end direction
					}

					iCount2--;
					bFindEndDir = true;
					break;
				}

			}
			iCount2--;
		}

		if(!bFindEndDir)
		{
			iCount2 = iLength;
		}

		//cout<<"end dir is: "<<strEndDir<<endl;
		//get the second part(digit degree string)
		iIndex = iCount1;

		while(iIndex < iCount2)
		{
			// Check if the character is larger than zero first in order
			// to ignore some symbole out of normal ASCII range, which cause crash in isspace() [P10: 3097]
			if(strTemp[iIndex] > 0 && !isspace(strTemp[iIndex]) && strTemp[iIndex] != '-')
			{
		 		m_strMidPart += strTemp[iIndex];
			}

			iIndex++;
		}
		//cout<<"String mid part "<<strMidPart<<endl;
	}
}

//-------------------------------------------------------------------------------------------------
void Bearing::checkString()
{
	//**** if pure direction
	if (m_bPureFlag)
	{
		m_strPure = m_strStartDir;  

		CSingleLock lg(&m_sMutex, TRUE);
		vector<string>::iterator theIterator;

		//check strPure is valid, from pure vector.
		for (theIterator = m_svecStrPure.begin(); theIterator != m_svecStrPure.end(); theIterator++)
		{
			if (m_strPure == *theIterator)
			{
				m_bValidPure = true;
				m_bValid = true;
				break;
			}
		}

		if (m_bValidPure)
		{
			setStandString();

			if (!m_bSuppressAlternates)
			{
				m_vecStrAlt.push_back(string(m_strStandString));
			}
		}
		else
		{
			// ARVIND COMMENTED ON 7/18/1999
			// (we don't need this anymore - see evaluate())
			//string strT = checkTilde(strBearString);
			//vecStrAlt.push_back(string(strT));

			moreAlternateString();
		}
	}
	else
	{
		translateString();
	}
}
//-------------------------------------------------------------------------------------------------
void Bearing::translateString()
{
	int iLength = 0;
	int iIndex  = 0;
	 
	bool bValidStart = false;   //true:  "n", "north".... but not "",
	bool bStart      = false;	//true: "", "n", "north".....
								//if (start == "")
								//		bStartOption = true.

	bool bValidEnd   = false;	 
	bool bEnd        = false;

	bool bValidMid   = false;	//must have digit between start dir and end dir,   
								//and no more than 6 digits

	bool bValidDegUnit = false; //true if unit belone the unit vector
	bool bValidMinUnit = false;
	bool bValidSecUnit = false;
	 
	bool bSpecial = false;  //special case: N123'45"E

	bool bMinDigit = true;  //for minute digite (first char < 6)
	bool bSecDigit = true;  //for second digite (first char < 6)
	bool bPreValid = false;
	 
	vector<string>::iterator theIterator;

	CSingleLock lg(&m_sMutex, TRUE);
	//translate start direction
	for(theIterator = m_svecStrStartUnit.begin(); theIterator != m_svecStrStartUnit.end(); theIterator++)
	{
		if(m_strStartDir == *theIterator)
		{
			bStart = true;
			 
			if(m_strStartDir == "")
			{
			 	m_bStartOption  = true;
			}
			else 
			{
				m_chStartDir = m_strStartDir[0];
				 
				bValidStart = true;
			}
			break;
		}
	}

	//translate end direction
	for(theIterator = m_svecStrEndUnit.begin(); theIterator != m_svecStrEndUnit.end(); theIterator++)
	{
		if(m_strEndDir == *theIterator)
		{
			bEnd = true;

			if(m_strEndDir == "")
			{
			 	m_bEndOption  = true;
			}
			else 
			{
				m_chEndDir = m_strEndDir[0];
				
				if(m_chEndDir == '-')
				{
					m_chEndDir = m_strEndDir[1];
				}

				bValidEnd = true;
			}
			break;
		}
	}

	//translate middle part string
    iLength = m_strMidPart.length();

	// Check if the number of digits before the last period 
	// is larger than 6, if it is, it is invalid
	unsigned int iLengthBeforePeriod = m_strMidPart.rfind('.');
	if (iLengthBeforePeriod == string::npos)
	{
		iLengthBeforePeriod = iLength;
	}

	int iDigitLength = 0;
	for (unsigned int i = 0; i < iLengthBeforePeriod; i++)
	{
		if (isDigitChar(m_strMidPart[i]) )
		{
			iDigitLength++;
		}
	}
	if (iDigitLength > 6)
	{
		m_bValid = false;
	    bValidMid = false;
		return;
	}

	//cout<<"*** Mid part length "<<iLength<<endl;
	iIndex = 0;
	//if there are three digits before the unit of minute, ex. N123'45"E
	//It should be have two alternate string
	//1) N1D23'45"E
	//2) N12D3'45"E
	//check string if string has more than 3 digits, and string[3] is not digit,
	//treat as special case
	int iCountDigit = 0;

	for(int j = 0; ((j < iLength) && (j < 3)); j++)
	{
		//if(isdigit(strMidPart[j]))
		if (IsCharAlphaNumeric(m_strMidPart[j]) && (!IsCharAlpha(m_strMidPart[j])))
		{
			iCountDigit++;
		}
	}

	//if((iLength >3 && iCountDigit == 3 && !isdigit(strMidPart[3])) 
	if ((iLength > 3 && iCountDigit == 3 && !(IsCharAlphaNumeric(m_strMidPart[3]) && (!IsCharAlpha(m_strMidPart[3]))))
		|| (iCountDigit == 3 && iLength == 3))
	{
		bSpecial = true;
	}

	//first get the degree digit
	//if(iIndex<iLength && isdigit(strMidPart[iIndex]))
	if (iIndex<iLength && (IsCharAlphaNumeric(m_strMidPart[iIndex]) && (!IsCharAlpha(m_strMidPart[iIndex]))))
	{
		//Mid part must has a digit, otherwise it's invalid,
		//later check if >6 digits, it's still invalid
		bValidMid = true;
		m_dDegree = m_strMidPart[iIndex] - '0';
		//set degree string
		m_strDegString = '0';
		m_strDegString += m_strMidPart[iIndex];
		iIndex++;

		if (iIndex < iLength)
		{
			//if(isdigit(strMidPart[iIndex])) //two digits degree
			if (IsCharAlphaNumeric(m_strMidPart[iIndex]) && (!IsCharAlpha(m_strMidPart[iIndex])))
			{
				m_dDegree = m_dDegree * 10 + (m_strMidPart[iIndex] - '0');
				m_strDegString = m_strMidPart[iIndex - 1];
				m_strDegString += m_strMidPart[iIndex];
				iIndex++;
			}
		}

		// ARVIND ADDED on 7/25/1999
		if (m_dDegree > 90)
		{
			m_bValid = false;
		    bValidMid = false;
		}		
	}	 

	//get the degree unit
	//while(iIndex<iLength && !isdigit(strMidPart[iIndex]))
	while (iIndex<iLength && !(IsCharAlphaNumeric(m_strMidPart[iIndex]) && (!IsCharAlpha(m_strMidPart[iIndex]))))
	{
		m_strDegUnit += toupper(m_strMidPart[iIndex]);
		iIndex++;
	}

	//check degree unit is valid
	for (theIterator = m_svecStrDegUnit.begin(); theIterator != m_svecStrDegUnit.end(); theIterator++)
	{
		if (m_strDegUnit == *theIterator)
		{
			bValidDegUnit = true;
		 	break;
		}
	}

	//get the digit of the minute
	if (iIndex < iLength)
	{
		m_dMinute = m_strMidPart[iIndex] - '0';
		m_strMinString = '0';
		m_strMinString += m_strMidPart[iIndex];
		iIndex++;

		if (iIndex < iLength)
		{
			//if(isdigit(strMidPart[iIndex]))  //two digits minute
			if (IsCharAlphaNumeric(m_strMidPart[iIndex]) && (!IsCharAlpha(m_strMidPart[iIndex])))
			{
				m_dMinute = m_dMinute*10 + (m_strMidPart[iIndex]-'0');
				m_strMinString = m_strMidPart[iIndex - 1];
				m_strMinString += m_strMidPart[iIndex];
				iIndex++;
			}
		}
		
		if (m_dMinute >= 60)
		{
			m_bValid = false;
			bValidMid = false;
		}
		
		//*** check first char of degree string, it should not great than 6
		if(m_strMinString[0] > '6')
		{
			bMinDigit = false;
		}
		//changeChar(strMinString); //first digit must less than 6
	}
	//get the minute unit
	//while(iIndex<iLength && !isdigit(strMidPart[iIndex]))
	while (iIndex<iLength && !(IsCharAlphaNumeric(m_strMidPart[iIndex]) && (!IsCharAlpha(m_strMidPart[iIndex]))))
	{
		m_strMinUnit += toupper(m_strMidPart[iIndex]);
		iIndex++;
	}

	//check minute unit is valid
	for(theIterator = m_svecStrMinUnit.begin(); theIterator != m_svecStrMinUnit.end(); theIterator++)
	{
		if (m_strMinUnit == *theIterator)
		{
			bValidMinUnit = true;
			break;
		}
	}

	//get the digit of the second
	if(iIndex < iLength)
	{
		m_dSecond = m_strMidPart[iIndex] - '0';
		m_strSecString = '0';
		m_strSecString += m_strMidPart[iIndex];
		iIndex++;

		if(iIndex < iLength)
		{
			//if(isdigit(strMidPart[iIndex]))  //two digits second
			if (IsCharAlphaNumeric(m_strMidPart[iIndex]) && (!IsCharAlpha(m_strMidPart[iIndex])))
			{
				m_dSecond = m_dSecond*10 + (m_strMidPart[iIndex]-'0');
				m_strSecString = m_strMidPart[iIndex - 1];
				m_strSecString += m_strMidPart[iIndex];
				iIndex++;
			}
		}
	
		if (m_dSecond >= 60)
		{
			m_bValid = false;
			bValidMid = false;
		}
		
		//*** check first char of degree string, it should not great than 6
		if(m_strMinString[0] > '6')
		{
			bMinDigit = false;
		}
	}

	/*
	if(isdigit(strMidPart[iIndex]))  //this mean more than six digit
		bValidMid = false;
	*/
	//new: need handle decimal second
	if(m_strMidPart[iIndex] == '.')  //has decimal second
	{
		iIndex++;
		m_strSecString += ".";
		if (iIndex < iLength)
		{
			//keep getting digits
			int j = 1;
			
			while (true)
			{
				//if(isdigit(strMidPart[iIndex]))
				if (IsCharAlphaNumeric(m_strMidPart[iIndex]) && (!IsCharAlpha(m_strMidPart[iIndex])))
				{
					m_dSecond = m_dSecond + (m_strMidPart[iIndex]-'0')/pow((double)10, j);
					j++;
					m_strSecString += m_strMidPart[iIndex];
				}

				iIndex++;
				//if(iIndex==iLength || !isdigit(strMidPart[iIndex]))
				if (iIndex == iLength || !(IsCharAlphaNumeric(m_strMidPart[iIndex]) && (!IsCharAlpha(m_strMidPart[iIndex]))))
				{
					break;
				}
			}
		}
	}

	//get the second unit
	//while(iIndex<iLength && !isdigit(strMidPart[iIndex]))
	while (iIndex<iLength && !(IsCharAlphaNumeric(m_strMidPart[iIndex]) && (!IsCharAlpha(m_strMidPart[iIndex]))))
	{
		m_strSecUnit += toupper(m_strMidPart[iIndex]);
		iIndex++;
	}

	//check second unit is valid
	for(theIterator = m_svecStrSecUnit.begin(); theIterator != m_svecStrSecUnit.end(); theIterator++)
	{
		if(m_strSecUnit == *theIterator)
		{
			bValidSecUnit = true;
			break;
		}
	}

	//final check the input string is valid
	if(bValidStart && bValidEnd && bValidMid && bValidDegUnit && bValidMinUnit && bValidSecUnit )
	{
		bPreValid = true;   //need check first char of minute and second
	}

	if(bPreValid && bMinDigit && bSecDigit)
	{
		m_bValid = true;
	}
	
	//bStart and bEnd must be true, avoid some string like "nnn35".
	if(iLength > 0 && (m_bStartOption || m_bEndOption) && bStart && bEnd && bValidSecUnit
		&&bValidDegUnit &&bValidMinUnit && bValidMid)
	{
		m_bPossibleFlag = true;
	}
	else
	{
		m_bPossibleFlag = false;
	}
 
	//set standard stirng
	setStandString();
 
	//set alternate string vector
	setAltVec(m_strDegString, m_strMinString, m_strSecString); //3 parameter just for alternate string
 
	//handle special case
	if(bSpecial)  //n123' east
	{
		//cout<<"** hi"<<endl;
		//cout<<"flag "<<bPossibleFlag<<endl;
		m_bValid = false;
		m_bPossibleFlag = true;

		//reset degree string and minute string
		string str1 = "0";
		str1 += m_strDegString[0];
		string str2 = "";
		str2 += m_strDegString[1];
		str2 += m_strMinString[1];
		setAltVec(str1, str2, m_strSecString);
	}

	//modify 12-12
	if(bPreValid && (bMinDigit || bSecDigit))
	{
		string strS = m_strStartDir;
		strS += m_strDegString;
		strS += m_strDegUnit;
		changeChar(m_strMinString);
		strS += m_strMinString;
		strS += m_strMinUnit;
		changeChar(m_strSecString);
		strS += m_strSecString;
		strS += m_strSecUnit;
		strS += m_strEndDir;

		if (!m_bSuppressAlternates)
		{
			m_vecStrAlt.push_back(string(strS));
		}
	}
 
	//modify 12-12
	moreAlternateString(); 
}

//--------------------------------------------------------------------------------------------------
double Bearing::convert_angle(double d, double m, double s)
{
	double angle = 0;

	angle = d + m/60.0 + s/3600.0;

	return angle;
}

//---------------------------------------------------------------------------------------------------
void Bearing::radiansToString(double angle)
{
	//angle must <= 360
	assert(angle <= 360);

	double min, sec;
	int d, m, s;      //int for degree, minute, second

	d = static_cast<int>( floor(angle) );
	min = (angle - d)*60;
	m = static_cast<int>( floor(min) );
	sec = (min - m)*60 + 0.5;
	s = static_cast<int>( floor(sec) );

	// if second is 60", then set nSec to 0 and add 1 to nMin
	if (s == 60)
	{
		m += 1;
		s = 0;
		// if nMin is 60', then set nMin to 0 and add 1 to nDeg
		if (m == 60)
		{
			d += 1;
			m = 0;
		}
	}

	m_dDegree = d;
	m_dMinute = m;
	m_dSecond = s;

	char buffer[10];  //for translate int to string

	_itoa_s(d, buffer, 10);
	m_strDegString = buffer;
	
	if (d < 10)          //if d only has one digit, put '0' before this digit for degree string
	{
		m_strDegString = "0" + m_strDegString;
	}

	_itoa_s(m, buffer, 10);
	m_strMinString = buffer;
	
	if( m < 10)
	{
		m_strMinString = "0" + m_strMinString;
	}

	_itoa_s(s, buffer, 10);
	m_strSecString = buffer;

	if (s < 10)
	{
		m_strSecString = "0" + m_strSecString;
	}
}

void Bearing::setAltVec(string str1, string str2, string str3)
{
	//str1: strDegString
	//str2: strMinString
	//str3: strSecString

	//check string is valid, change to possiable string....
	//put standart string format to vector, N35D12'24"
	 
	//if bearing string is valid, just push one string to vector
	if (m_bValid) // ARVIND MODIFIED 7/18/1999 - FROM if (isValid())
	{
		m_vecStrAlt.push_back(string(m_strStandString));
	}
	else if (m_bPossibleFlag)
	{
		//push some possible string to the vector
		//if start and end option is fail, that's mean it's total invalid bearing string,
		//just push " input string " to vector

		string s;   //sting for digit degree (middle part)
		s =  str1;
		s += 'D';
		s += str2;
		s += '\'';
		s += str3;
		s += '\"';

		if (m_bStartOption)
		{
			//start direction could be north or south
			if (m_bEndOption)
			{
				//start dir and end dir are empty, so there are four possible string 
				m_vecStrAlt.push_back(string('N' + s + 'E'));
				m_vecStrAlt.push_back(string('S' + s + 'E'));
				m_vecStrAlt.push_back(string('N' + s + 'W'));
				m_vecStrAlt.push_back(string('S' + s + 'W'));
			}
			else
			{
				//only start dir is empty, so there are two possible string
				m_vecStrAlt.push_back(string('N' + s + m_chEndDir));
				m_vecStrAlt.push_back(string('S' + s + m_chEndDir));
			}
		}
		else //startOption is false
		{ 
			//check end direction is empty
			if (m_bEndOption)
			{
				//since only end direction is empty, so there are two possible string
				m_vecStrAlt.push_back(string(m_chStartDir + s + 'E'));
				m_vecStrAlt.push_back(string(m_chStartDir + s + 'W'));
			}
			else //in case N123'45"E, first set bPossible flag true, put N12D03'45"E
				//to alternate string vector, later put N01D23'45"E to vector
			{
				m_vecStrAlt.push_back(string(m_chStartDir + s + m_chEndDir));
				//cout<<"**** here ***** "<<chStartDir<<" "<<s<<" "<<chEndDir<<endl;
			}
		}
	}

	// ARVIND COMMENTED ON 7/18/1999
	// ( we don't need this here, as we are always storing the
	// (original string in the alternate strings vector anyway)
	//else  //no possible sting, push orignal string
	//{
	//		vecStrAlt.push_back(checkTilde(strBearString));
	//		//cout<<"input stirng "<<strBearString<<endl;
	//}
}

void Bearing::setStandString()
{
	//first check if bearing string is valid
	//second check if bearing string is pure direction, return special standard string
	//third get standard string by adding start direction + degree + deg_unit + minute
	//+ min_unit + second + sec_unit + end direction

	if(m_bValidPure)
	{
		if(m_strPure[0] == 'N')
		{
			m_strStandString = "N";
		}
		else if(m_strPure[0] == 'E')
		{
			m_strStandString = "E";
		}
		else if(m_strPure[0] == 'W')
		{
			m_strStandString = "W";
		}
		else if(m_strPure[0] =='S')
		{
			m_strStandString = "S";
		}
		else
		{
			m_strStandString = "    error: invalid direction";		   
		}
	}
	else
	{
		m_strStandString =   m_chStartDir;
		m_strStandString +=  m_strDegString;
		m_strStandString +=  'D';
		m_strStandString +=  m_strMinString;
		m_strStandString +=  '\'';
		m_strStandString +=  m_strSecString;
		m_strStandString +=  '\"';
		m_strStandString +=  m_chEndDir;	
	}
}


char Bearing::filterChar(char ch)
{
	char chTemp;
	if (toupper(ch) == 'L' )
	{
		chTemp = '1';
		return chTemp;
	}
	if (toupper(ch) == 'J')
		
	{
		chTemp = '3';
		return chTemp;
	}
	if (toupper(ch) == 'B')
	{
		chTemp = '8';
		return chTemp;
	}
	if (toupper(ch) == 'Q')
	{
		chTemp = '0';
		return chTemp;
	}
/*	if(toupper(ch) == 'O')
	{
		chTemp = '0';
		return chTemp;
	}
	*/
	if (toupper(ch) == 'Z')
	{
		chTemp = '2';
		return chTemp;
	}
	if (ch == 'g')
	{
		chTemp = '9';
		return chTemp;
	}
	if (ch == 'G')
	{
		chTemp = '6';
		return chTemp;
	}
	if (ch == '°')
	{
		chTemp = 0;
		return chTemp;
	}

	return ch;
}

void Bearing::changeChar(string& str1)
{
	assert(str1.length() != 0);
	if(str1[0] == '6')
		str1[0] = '5';
	if(str1[0] == '7')
		str1[0] = '1';
	if(str1[0] == '8')
		str1[0] = '3';
	if(str1[0] == '9')
		str1[0] = '4';
}

double Bearing::truncateRadians(double dValue)
{
	//this time no need truncate radians value
	/*
	//truncate the double value to standard format value
	//ex 3.14159 to 3.14
	double dTemp;
	int iTemp1, iTemp2;
	double  dTemp1, dTemp2;
 
    iTemp1 = dValue;  //iTemp1 = 3
	dTemp1 = dValue-iTemp1;  //dTemp1 = 0.14159
	dTemp1 = 100*dTemp1;       //dTemp = 14.159
	iTemp2 = dTemp1;         //iTemp2 = 14
	dTemp2 = dTemp1 - iTemp2;   //dTemp2 = .159
	if(dTemp2>=0.5)
	{
		if(iTemp2==99)
		{
			iTemp2 = 0;
			iTemp1++;
		}
		else
			iTemp2++;
	}
	dTemp = iTemp1 + (double)iTemp2/100;
	return dTemp;
	*/
	return dValue;
}

void Bearing::moreAlternateString()
{
	string strTemp = m_strBearString;
	//scan the string replace the confused char
	//char 's' should be 5 when it's not in first char
	//char 'c', 'w', 'j', ')', 'b' should be 3
	//char 'l' should be 7 or 1,
	//cout<<"** bearing string is ** "<<strBearString<<endl;
	
	string strTemp2 = "";
	int iLength = strTemp.length();
	char chTemp;

	for ( int i = 0; i < iLength; i++)
	{
		chTemp = filterChar(strTemp[i]);

		if (chTemp != 0)
		{
			strTemp2 += chTemp;
		}
	}

	if(strTemp != strTemp2)
	{
		m_vecStrAlt.push_back(string(strTemp2));
	}
}

bool Bearing::inEndVector(const string& strEndPart)
{
	CSingleLock lg(&m_sMutex, TRUE);
	vector<string>::iterator theIterator;

	for(theIterator = m_svecStrEndUnit.begin(); theIterator != m_svecStrEndUnit.end(); theIterator++)
	{
		if(strEndPart == *theIterator)
		{
			return true;
		}
	}
	return false;
}

string Bearing::getEvaluatedString()
{
	return m_strEvaluatedString;
}