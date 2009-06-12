#include "stdafx.h"
#include "Angle.hpp"

#include <TPPoint.h>
#include <cpputil.h>
#include <mathUtil.h>
#include <iostream>
#include <fstream>

#include <UCLIDException.h>
using namespace std;

//--------------------------------------------------------------------------------------------------
AbstractMeasurement::EType Angle::getType() const
{
	return AbstractMeasurement::kAngle; 
}
//--------------------------------------------------------------------------------------------------
const string& Angle::getTypeAsString() const
{
	static const string strType = "Angle";
	return strType;
}
//--------------------------------------------------------------------------------------------------
Angle::Angle()
{
	resetVariables();    //set private member value to default value
	setUnit();          //set valid unit to vectors
}
//------------------------------------------------------------------------------
Angle& Angle::operator=(const Angle& angle)
{
	AbstractMeasurement::operator=(angle);

	this->dDegree = angle.dDegree;        
    this->dMinute = angle.dMinute;      
	this->dSecond = angle.dSecond;       

	this->dAngle = angle.dAngle;       
	this->dRadians = angle.dRadians;     

	this->bValid = angle.bValid;

	// ARVIND ADDED 7/18/1999
	this->bIsOriginalAngleValid = angle.bIsOriginalAngleValid;

	this->bNegative = angle.bNegative;
	

	this->strAngleString = angle.strAngleString;  
	this->strStandString = angle.strStandString;  

	this->strDegString = angle.strDegString ;   
	this->strMinString = angle.strMinString;    
	this->strSecString = angle.strSecString;    

	this->strDegUnit = angle.strDegUnit;      
	this->strMinUnit = angle.strMinUnit;     
	this->strSecUnit = angle.strSecUnit;     

	this->strTempAlt = angle.strTempAlt;    

	
	 
	this->strVecDegUnit = angle.strVecDegUnit;   
	this->strVecMinUnit = angle.strVecMinUnit;    
	this->strVecSecUnit = angle.strVecMinUnit;    
	this->vecStrAlt = angle.vecStrAlt; 
	
	return *this;
}
//----------------------------------------------------------------
Angle::Angle(const Angle &angle)
{
	 
	dDegree = angle.dDegree;        
    dMinute = angle.dMinute;      
	dSecond = angle.dSecond;       

	dAngle = angle.dAngle;       
	dRadians = angle.dRadians;     

	bValid = angle.bValid;

	// ARVIND ADDED 7/18/1999
	bIsOriginalAngleValid = angle.bIsOriginalAngleValid;
	
	bNegative = angle.bNegative;
	

	strAngleString = angle.strAngleString;  
	strStandString = angle.strStandString;  

	strDegString = angle.strDegString ;   
	strMinString = angle.strMinString;    
	strSecString = angle.strSecString;    

	strDegUnit = angle.strDegUnit;      
	strMinUnit = angle.strMinUnit;     
	strSecUnit = angle.strSecUnit;     

	strTempAlt = angle.strTempAlt;    

	
	 
	strVecDegUnit = angle.strVecDegUnit;   
	strVecMinUnit = angle.strVecMinUnit; 
	//#################################################
	strVecSecUnit = angle.strVecSecUnit;    
	vecStrAlt = angle.vecStrAlt;         
}

//--------------------------------------------------------------------------------------------------
Angle::Angle(const char *pszAngle)
{
	resetVariables();
	setUnit();
	
	evaluate(pszAngle);   
}
//--------------------------------------------------------------------------------------------------
Angle::~Angle()
{
	//no pointer need delete
}
//--------------------------------------------------------------------------------------------------
AbstractMeasurement* Angle::createNew()
{
	return new Angle();
}
//--------------------------------------------------------------------------------------------------
void Angle::setUnit()
{ 
	//set degree uint
	strVecDegUnit.push_back("");
	strVecDegUnit.push_back("_");
	strVecDegUnit.push_back("-");
	strVecDegUnit.push_back("D");
	strVecDegUnit.push_back("DEG");
	strVecDegUnit.push_back("DEG.");
	strVecDegUnit.push_back("DEGREE");
	strVecDegUnit.push_back("DEGREES");

	strVecDegUnit.push_back("*");
	strVecDegUnit.push_back("`");
	strVecDegUnit.push_back("%%D");
	strVecDegUnit.push_back("°");
	strVecDegUnit.push_back("º");
	 
	strVecDegUnit.push_back("'");
	strVecDegUnit.push_back("’");
	strVecDegUnit.push_back("\"");

	strVecDegUnit.push_back("O");  //this is string "o"
	 
	
	//set minute unit
	strVecMinUnit.push_back("");
	strVecMinUnit.push_back("_");
	strVecMinUnit.push_back("-");
	strVecMinUnit.push_back("'");
	strVecMinUnit.push_back("M");
	strVecMinUnit.push_back("MIN");
	strVecMinUnit.push_back("MIN.");
	strVecMinUnit.push_back("MINUTE");
	strVecMinUnit.push_back("MINUTES");
	strVecMinUnit.push_back("\"");
	strVecMinUnit.push_back("*");
	strVecMinUnit.push_back("`");
	strVecMinUnit.push_back("‘");	//145 <ANSI>
	strVecMinUnit.push_back("’");	//146 <ANSI>

	//set second unit
	strVecSecUnit.push_back("");
	strVecSecUnit.push_back("_");
	strVecSecUnit.push_back("-");
	strVecSecUnit.push_back("\"");
	strVecSecUnit.push_back("S");
	strVecSecUnit.push_back("SEC");
	strVecSecUnit.push_back("SEC.");
	strVecSecUnit.push_back("SECOND");
	strVecSecUnit.push_back("SECONDS");
	strVecSecUnit.push_back("'");
	strVecSecUnit.push_back("‘");	//145 <ANSI>
	strVecSecUnit.push_back("’");	//146 <ANSI>
	strVecSecUnit.push_back("*");
	strVecSecUnit.push_back("`");
	strVecSecUnit.push_back("“");	//147 <ANSI>
	strVecSecUnit.push_back("”");	//148 <ANSI>
}

//-------------------------------------------------------------------------------------
//Set private member
//Set boolean variable to false
//Set string to empty ""
void Angle::resetVariables()
{
	// ARVIND ADDED 7/18/1999
	bIsOriginalAngleValid = false;

	bValid = false;    //for whole input angle string
	bNegative = false;

	//init string
	strAngleString = "";
	strStandString = "";
	strDegString = "";
	strDegUnit   = "";
	strMinString = "";
	strMinUnit   = "";
	strSecString = "";
	strSecUnit   = "";

	strTempAlt = "";

	dDegree = -1;
	dMinute = 0;
	dSecond = 0;

	//clear alternate string vector
	//vecStrAlt.erase(vecStrAlt.begin(), vecStrAlt.end());
	vecStrAlt.clear(); 
}	

//--------------------------------------------------------------------------------------------------
void Angle::evaluate(const char *pszAngle)
{
	resetVariables();

	strEvaluatedString = pszAngle;

	// THIS IS A DEBUG INITIATIVE FOR THE INTERMITTENT CASES WHERE ANGLE
	// EVALUATION WORKS PECULIARY
	// ADDED BY ARVIND on 7/21/1999
/*	{
		string strOutFile = icoMap.getApplicationSettings()->getIcoMapDirectory();
		strOutFile += "\\bin\\angles.dat";

		string strTime = CTime::GetCurrentTime().Format("%H:%M:%S");
		string strDate = CTime::GetCurrentTime().Format("%m/%d/%Y");

		ofstream outfile(strOutFile.c_str(), ios::app | ios::out);
		if (outfile)
		{
			outfile << strDate << " " << strTime << " " << strlen(pszAngle) << " " << pszAngle << endl;
			outfile.close();
		}

	}
*/
    // just create a vector of all "invalid" character positions in case
    // the string is not valid
    setInvalidPositions(pszAngle);

    // ARVIND ADDED ON 7/17/1999
	// no matter what, we want the first string in the 
	// alternate vectors to be the bearing string itself.
	// we don't want to automatically ignore the tilde's
	// before evaluating.
	vecStrAlt.push_back(string(pszAngle));	

	//scan the string replace the confused char
	//char 's' should be 5 when it's not in first char
	//char 'c', 'w', 'j', ')', 'b' should be 3
	//char 'l' should be 7 or 1, 
	//in bearing string each digit should less then 6
	//7 to 1
	//8 to 3
	//9 to 4
	//6 to 5
	
	//ARVIND COMMENTED ON 7/18/1999
	//strAngleString = pszAngle;
	//tokenString(); 	
	//checkValid();

	// ARVIND ADDED 7/18/1999
	// first parse the string as it is.  If it is valid, great.
	// if not, remove the tilde's and then parse again
	{
		strAngleString = pszAngle;

		preProcessString(strAngleString);

		tokenString();
		checkValid();

		// store whether the string is valid or not in bIsOriginalAngleValid
		// which is the bool that isValid() returns!
		bIsOriginalAngleValid = bValid;
	}

	// ARVIND ADDED ON 7/18/1999
	// create a vector to store the alterate strings
	// from the original strings
	vector<string> vecStrOrigAlt = vecStrAlt;

	// ARVIND ADDED 7/18/1999
	// the current string is invalid. Remove
	// the tilde's and re-determine if the string
	// could be valid -> purpose is to populate the
	// alternate strings vector accordingly.
	if (!bValid)
	{
		// reset variables as if we are re-evaluating the string
		// without the tilde.
		resetVariables();

		strAngleString = checkTilde(string(pszAngle));

		preProcessString(strAngleString);

		tokenString();
		checkValid();

		// now prepend the original alternating strings vector
		// to the current set of alternating strings vector
		vecStrAlt.insert(vecStrAlt.begin(), vecStrOrigAlt.begin(), vecStrOrigAlt.end());
	}
}
//--------------------------------------------------------------------------------------------------
void Angle::evaluate(const TPPoint&p1, const TPPoint& p2)
{
	double dRadians = p1.angleTo(p2);
	evaluateRadians(dRadians);
}
//--------------------------------------------------------------------------------------------------
void Angle::evaluateRadians(const double& dRadians)
{
	//first, check the radians is valid.
	//second, translate radians to angle
	double dTempAngle = -1;  

	double dValue = dRadians;

	// make sure 0 <= dValue < 2PI
	// No matter dValue > 2PI or dValue < 0 or is any other values, 
	// use the following expression will be sufficient
	// For instance: dValue = 65, dMult = floor (65/(2*PI)) = 10.0
	// dValue = 65 - 2*PI*10.0 = 2.168147...
	// For instance: dValue = 2, dMult = floor (2/(2*PI)) = 0.0
	// dValue = 2 - 2*PI*0.0 = 2
	// For instance: dValue = -20, dMult = floor (-20/(2*PI)) = -4.0
	// dValue = -20 - 2*PI*(-4.0) = 5.132741...
	double dMult = floor(dValue / (2 * MathVars::PI));
	dValue = dValue - dMult * 2 * MathVars::PI;

	if(dValue < 0 || dValue >= 2*MathVars::PI)
	{
		//should not be here
		bValid = false;
	}
	else
	{
		bValid = true;
		
		//1)get angle from radians
		dTempAngle = (dValue * 180/MathVars::PI);  //translate radians to angle

		//2)get ^^deg^^min^^sec from radians
		truncateAngle(dTempAngle);
		//set alternate vector
		setAltVec();
	}

	// ARVIND ADDED 7/18/1999
	// the bIsOriginalAngleValid variable is used in isValid()
	bIsOriginalAngleValid = bValid;
			 
	strEvaluatedString = asString();
}
//--------------------------------------------------------------------------------------------------
bool Angle::isValid(void)
{
	// ARVIND COMMENTED ON 7/18/1999
	// return bValid;

	// ARVIND ADDED ON 7/18/1999
	return bIsOriginalAngleValid;
}
//--------------------------------------------------------------------------------------------------
vector<string> Angle::getAlternateStrings(void)
{
	// ARVIND ADDED ON 7/18/1999
	// TODO: the following concept of cleaning up at this point needs to be
	// taken care of.
	{
		vector <string> vecTemp = vecStrAlt;
		vector <string>::const_iterator iter;
		
		vecStrAlt.clear();

		for (iter = vecTemp.begin(); iter != vecTemp.end(); iter++)
		{
			if (!vectorContainsElement(vecStrAlt, *iter))
			{
				vecStrAlt.push_back(*iter);
			}
		}

		return vecStrAlt;
	}
}
//--------------------------------------------------------------------------------------------------
double Angle::getDegrees(void)
{
	//first check valid
	if(!isValid())
	{
		throw UCLIDException ("ELI00424", "ERROR: Invalid Angle!, No degree");
		return -1;
	}
	else
	{
		double dTemp = dAngle;
		
        // ARVIND MODIFIED ON 9/9/1999
        // in reverse mode, angle's don't get effected
        /*
        if(bReverseMode == true)
		{
			dTemp = dTemp+180; 
			if(dTemp >= 360)
				dTemp = dTemp - 360;
		}
        */
		return dTemp;
	}
}

double Angle::getRadians()
{
	//first check valid
	if(!isValid())
	{
		throw UCLIDException ("ELI00425", "ERROR: Invalid Angle!, No degree");
		return -1;
	}
	else
	{
		double dTemp = dAngle;

        // ARVIND MODIFIED ON 9/9/1999
        // in reverse mode, angle's don't get effected
        /*
        if(bReverseMode == true)
		{
			dTemp = dTemp+180; 
			if(dTemp >= 360)
				dTemp = dTemp - 360;
		}
        */

		dRadians = truncateRadians(dTemp*MathVars::PI/180);
		return dRadians;
	}
}
//--------------------------------------------------------------------------------------------------
string Angle::asString(void)
{
	if(!isValid())
	{
		return "ERROR: Invalid Angle, No standard string";	  
	}

	// we need the original string input if it's valid
	return getAlternateStrings()[0];
}
//--------------------------------------------------------------------------------------------------
string Angle::interpretedValueAsString(void)
{
	if(!isValid())
	{
		return "ERROR: Invalid Angle, No standard string";	  
	}

	return strStandString;
}
//--------------------------------------------------------------------------------------------------
string Angle::asStringDecimalDegree(void)
{
	if(!isValid())
	{
		return "ERROR: Invalid Angle, No standard string";
	}

	double dAngleVal = getDegrees();
	CString cstrAngle("");
	cstrAngle.Format("%f Degrees", dAngleVal);

	return (string)cstrAngle;
}
//--------------------------------------------------------------------------------------------------
string Angle::asStringDMS(void)
{
	if(!isValid())
	{
		return "ERROR: Invalid Angle, No standard string";
	}
	
	// get the degree in double
	double dAngleVal = getDegrees();
	if (bNegative)
	{
		dAngleVal = fabs(dAngleVal);
	}

	int nDeg = (int)floor(dAngleVal);
	// get the remaining minute and second part (i.e. value after decimal point)
	// for example: if original is 23.342, nDeg = 23, dRemainMS = 0.342 * 60 = 20.52
	double dRemainMS = (dAngleVal - floor(dAngleVal)) * 60;
	
	// ex. nMin = (int)floor(20.52) = 20
	int nMin = (int)floor(dRemainMS);

	// ex. nSec = (int)floor((20.52 - 20.0) * 60 + 0.5) = (int)floor(31.2 + 0.5) = 31
	int nSec = (int)floor((dRemainMS - floor(dRemainMS)) * 60 + 0.5);  

	// if second is 60", then set nSec to 0 and add 1 to nMin
	if (nSec == 60)
	{
		nMin += 1;
		nSec = 0;
		// if nMin is 60', then set nMin to 0 and add 1 to nDeg
		if (nMin == 60)
		{
			nDeg += 1;
			nMin = 0;
		}
	}

	CString cstrAngle("");
	if (bNegative)
	{
		cstrAngle.Format("-%dd%dm%ds", nDeg, nMin, nSec); 
	}
	else
	{
		cstrAngle.Format("%dd%dm%ds", nDeg, nMin, nSec); 
	}

	return (string)cstrAngle;
}
//--------------------------------------------------------------------------------------------------
void Angle::tokenString()
{
	//angle looks like the middle part of bearing string
	
	//if the angle string is ^^deg^^min^^sec, the value of minute and 
	//second must lees then 60. sometime translate 6-5, 7-1, 8-3,9-4
	int iIndex = 0;
	int iLength = strAngleString.length();
	char chTemp;
	char chTemp2;  //char for alternate string
	string strTemp = "";   //hold the filted angle string

	while(iIndex<iLength)
	{
		chTemp2 = strAngleString[iIndex];
		

		chTemp = strAngleString[iIndex];
		
		// Check if the character is larger than zero first in order
		// to ignore some symbole out of normal ASCII range, which cause crash in isspace() [P10: 3097]
		if(chTemp > 0 && !isspace(chTemp) && chTemp != '-')  //skip space
		{
			chTemp2 = filterChar(chTemp2); 

			strTempAlt += chTemp2;

			strTemp += chTemp;
		}
		iIndex++;
	}
	
	//reset iIndex, iLength
	iIndex = 0;
	//check first char in string, make sure it's valid
	if (strAngleString[0] == '-')
	{
		strTemp.insert(0, strAngleString, 0, 1);
		iIndex++;
		bNegative = true;
	}
	//when isdigit() is called in a DLL, sometimes it returns true while it should return false -- Duan 05/25/01
	//else if (!isdigit(strTemp[0]))
	else if (!(IsCharAlphaNumeric(strTemp[0]) && (!IsCharAlpha(strTemp[0]))))
	{
		strDegString += strTemp[0];  //save first char to degree string
	}

	iLength = strTemp.length();
	//get the degree string
	//while (iIndex<iLength && (isdigit(strTemp[iIndex])||strTemp[iIndex] == '.'))
	while (iIndex < iLength && 
			((IsCharAlphaNumeric(strTemp[iIndex]) && (!IsCharAlpha(strTemp[iIndex])))
				|| strTemp[iIndex] == '.'))
	{
		strDegString += strTemp[iIndex];
		iIndex++;
	}
	//*************
	//cout<<"*** degree string "<<strDegString<<endl;
	//get the degree unit
	//while(iIndex<iLength && !isdigit(strTemp[iIndex]))
	while (iIndex < iLength && !(IsCharAlphaNumeric(strTemp[iIndex]) && (!IsCharAlpha(strTemp[iIndex]))))
	{
		strDegUnit += toupper(strTemp[iIndex]);
		iIndex++;
	}
	//get the minute string
	//while(iIndex<iLength && isdigit(strTemp[iIndex]))
	while (iIndex < iLength && (IsCharAlphaNumeric(strTemp[iIndex]) && (!IsCharAlpha(strTemp[iIndex]))))
	{
		strMinString += strTemp[iIndex];
		iIndex++;
	}
	//cout<<"*** minute string "<<strMinString<<endl;
	//get the minute unit
	//while(iIndex<iLength && !isdigit(strTemp[iIndex]))
	while (iIndex<iLength && !(IsCharAlphaNumeric(strTemp[iIndex]) && (!IsCharAlpha(strTemp[iIndex]))))
	{
		strMinUnit += toupper(strTemp[iIndex]);
		iIndex++;
	}
	//get the second string
	//while(iIndex<iLength && (isdigit(strTemp[iIndex]) || strTemp[iIndex] == '.'))
	while (iIndex < iLength && 
			((IsCharAlphaNumeric(strTemp[iIndex]) && (!IsCharAlpha(strTemp[iIndex]))) 
				|| strTemp[iIndex] == '.'))
	{
		strSecString += strTemp[iIndex];
		iIndex++;
	}
	//get the second unit
	//while(iIndex<iLength && !isdigit(strTemp[iIndex]))
	while (iIndex < iLength && !(IsCharAlphaNumeric(strTemp[iIndex]) && (!IsCharAlpha(strTemp[iIndex]))))
	{
		strSecUnit += toupper(strTemp[iIndex]);
		iIndex++;
	}

	//*****
	//cout<<"degree string: "<<strDegString<<endl;
	//cout<<"minute string: "<<strMinString<<endl;
	//cout<<"second string: "<<strSecString<<endl;
}

void Angle::checkValid()
{
	//check Unit of degree, minute, second
	bool bUnit = false;  //if all unit are valid, bUnit is true
	if(checkUnit(strDegUnit,strVecDegUnit) && checkUnit(strMinUnit, strVecMinUnit)
		&& checkUnit(strSecUnit,strVecSecUnit))
	{
		bUnit = true;
	}

	//check digits of minute and second should not more than 2,
	//The first digit of minute and second should less than 6
	//if they have two digits
	bool bDigit = false;
	//if(isdigit(strDegString[0]) && strMinString.length()<3 )
	if ((IsCharAlphaNumeric(strDegString[0]) && (!IsCharAlpha(strDegString[0]))) 
			&& strMinString.length() < 3)
	{
		bDigit = true;
	}
	
	if (bUnit && bDigit)
	{
		bValid = true;

		//calculate angle value

		// Protect against empty string
		dDegree = 0.0;
		if (strDegString.length() > 0)
		{
			dDegree = asDouble( strDegString );
		}
		
		// make sure 0 <= dDegree < 360°
		double dMult = floor(dDegree / 360);
		dDegree = dDegree - dMult * 360;

		// Protect against empty string
		dMinute = 0.0;
		if (strMinString.length() > 0)
		{
			dMinute = asDouble( strMinString );
		}

		// Protect against empty string
		dSecond = 0.0;
		if (strSecString.length() > 0)
		{
			dSecond = asDouble( strSecString );
		}

		if (dMinute >= 60 || dSecond >= 60)
		{
			bValid = false;
		}

		double dTemp = dDegree + dMinute/60 + dSecond/3600;
		if (bNegative)
		{
			dTemp = 360-dTemp;
		}

		//truncate the dAngle, get the standard format double value
		//Get the standard string  ??any function in C++??
		truncateAngle(dTemp);
	}
	//set alternate string vector
	setAltVec();
}

void Angle::truncateAngle(double dValue) //??any function in C++??
{

	CString cstrTemp("");
	cstrTemp.Format("%.4f Degrees", dValue);

	strStandString = (string)cstrTemp;
	
	//get the standart format value
	//dAngle = iTemp1 + (double)iTemp2/100;
	dAngle = dValue;
}

double Angle::truncateRadians(double dValue)
{
	return dValue;
}

char Angle::checkDigit(char ch)
{
	char chTemp = ch;
	if(ch ==  '6')
		chTemp = '5';
	if(ch == '7')
		chTemp = '1';
	if(ch == '8')
		chTemp = '3';
	if(ch == '9')
		chTemp = '4';
	return chTemp;
}

void Angle::setAltVec()
{
	// ARVIND MODIFIED ON 7/18/1999 from: if (isValid())
	if(bValid)
		vecStrAlt.push_back(string(strStandString));
	else
	{
		//eliminate the space in strAngleString, then push to the vector
		string strTemp = "";
		for(unsigned int i = 0; i < strAngleString.length(); i++)
		{
			// Check if the character is larger than zero first in order
			// to ignore some symbole out of normal ASCII range, which cause crash in isspace() [P10: 3097]
			if(strAngleString[i] > 0 && !isspace(strAngleString[i]) && strAngleString[i] != '-')
				strTemp += strAngleString[i];
		}
		//*****
		//cout<<"strTemp: "<<strTemp<<endl;
		vecStrAlt.push_back(checkTilde(strTemp));
		if(!strcmp(strTemp.c_str(), strTempAlt.c_str()) == 0) 
			vecStrAlt.push_back(checkTilde(strTempAlt));
		//cout<<"strAngleString: "<<strAngleString<<endl;
	}
}

bool Angle::checkUnit(const string& strUnit, const vector<string> &strVecUnit)
{
	bool bTemp = false;
	vector<string>::const_iterator theIterator;
	
	for(theIterator = strVecUnit.begin(); theIterator != strVecUnit.end(); theIterator++)
	{
		if(strUnit == *theIterator)
		{
			bTemp = true;
			break;
		}
	}

	return bTemp;
}

char Angle::filterChar(char ch)
{
	char chTemp = ch;
	if(toupper(ch) == 'L' )
	{
		chTemp = '7';
		return chTemp;
	}
	if(toupper(ch) == 'J' || toupper(ch) == ')'
		|| toupper(ch) == 'W')
		
	{
		chTemp = '3';
		return chTemp;
	}
	if(toupper(ch) == 'B')
	{
		chTemp = '8';
		return chTemp;
	}
 
	return chTemp;
}

string Angle::getEvaluatedString()
{
	return strEvaluatedString;
}
