/*What test driver application do?
Test Driver application will read Input text file containing test cases.
Test case input gives the CurveParameter to Set or Test and its Value
Application reads the CurveParameter and do the following 
1)Unsets the Curve Parameter values
2)Sets the Curve Parameter Values
3)Tests the Curve Parameter Values with the expected Values
*/
#pragma warning(disable:4786)

#include <stdio.h>
#include <time.h>
#include <math.h>
#include <cstring>
#include <conio.h>
#include <afxwin.h>
#include <UCLIDException.hpp>
#include <CurveCalculationEngineImpl.h>
#include "CCETestDriver.h"

using namespace std;
#define NUMBEROFVARIABLES 25

//convert angle, bearing or distance from double format to string format
std::string DoubleToString(double dValue);
//convert point to string format
std::string PointToString(double dX, double dY);

int main(int iargc,char* pszTestCasesFile[])
{
	//Expected Value of the Curve Parameter
	string strReal="";
	//For storing the result whether the test is passed or failed
	string strResult="";
	//to store the Curve Parameter value
	string strCurveParam="";
	//Array to hold each line read in getline function
	char inputLine[255]="";
	//Seperators for dividing the Input line in to tokens
	char seps[]=" =,";
	//Each token in the Input line
	char *token = NULL;
	//Array to store the toaken in the Input line
	char *pInput[3];
	//'pi' value
	const double gdPI = 3.1415926535897932384626433832795;
	//Storing all the Curve Parameter in an array
	char *CurveParameters[NUMBEROFVARIABLES]={"ArcConcaveLeft",
		"ArcDeltaGreaterThan180Degrees","ArcDelta","ArcStartAngle",
		"ArcEndAngle","ArcDegreeOfCurveChordDef","ArcDegreeOfCurveArcDef",
		"ArcTangentInBearing","ArcTangentOutBearing","ArcChordBearing",
		"ArcRadialInBearing","ArcRadialOutBearing","ArcRadius","ArcLength",
		"ArcChordLength","ArcExternalDistance","ArcMiddleOrdinate",
		"ArcTangentDistance","ArcStartingPoint","ArcMidPoint","ArcEndingPoint",
		"ArcCenter","ArcExternalPoint","ArcChordMidPoint",
		"canCalculateAllParameters"};
	//used for point parameter type
	bool bPointParameter=false;
	//to store the point param X value
	string strRealX=" ";
	//to store the point param Y value
	string strRealY=" ";
	

	//To insert a decimal point unconditionally in a generated 
	//floating-point field. 
	cout.setf(ios::showpoint);
	//Instantiation of the exported class and creating an object 'TestClass'
	CurveCalculationEngineImpl TestClass;
	if(iargc==2)
	{
		cout<<"Process Starting...\n";
	}
	else if( iargc > 2 )
	{
		cout<<"Too many arguments supplied.\n";
		cout<<"The Syntax is:\n\n";
		cout<<"CCETestDriver [ InputTextfile ]\n";
		return 0;
	}
	else if(iargc<2)
	{
		cout<<"One argument expected.\n";
		cout<<"The Syntax is:\n\n";
		cout<<"CCETestDriver [ InputTextfile.txt ]\n";
		return 0;
	}
	
	//Input text file name and path is supplied as a command line to 
	//the main function.So we can open input file using filestreams. 
		
	ifstream TestCaseInput_file;
	
	//Opening Test Case Input file using open function
	TestCaseInput_file.open(pszTestCasesFile[1]);
	if(!TestCaseInput_file.is_open())
	{
		cout<<"Can't open "<<pszTestCasesFile[1]<<" file for reading\n";
		return 0;
	}
	cout<<"ID"<<", "<<"Status"<<", "<<"Line"<<", "<<"CurveParameter"<<", "<<"Calc.Value"<<", "<<"Exp.Value"<<endl;
	cout<<"----------------------------------------------------------"<<endl;
	
	//Creating a Test Case object 
	TESTCASE TestCaseType;
	//Number of Test Cases
	int iNCases=0;
	//Number of Failed Cases
	int iFailedCases=0;
	//Total Time taken for processing the Test Cases
	int iTotalTime=0;
	//Line Number in the Test Case Input File
	int iLineNumber=0;  
	//Percentage Result
	float fPercResult=0.0;
	//Time in Seconds for Processing each Test Case
	float secs=0.0;
	clock_t time,difftime;
	
	//Reading the type of Input till EOF occurs.
	while(!TestCaseInput_file.eof())
	{
		iLineNumber++;
		//Reading a line from the test case input file
		TestCaseInput_file.getline(inputLine,255,'\n');
		TestCaseType.strType.assign(inputLine);
		
		//To Unset the Values associated with all Curve Parameters
		if(((TestCaseType.strType.find("RESET"))!=string::npos)&&
			((TestCaseType.strType.find("RESET"))==0))
		{
			TestClass.reset();
		}
		
		// pass through any comment lines to the screen and the output file
		if(((TestCaseType.strType.find("COMMENT"))!=string::npos)&&
			((TestCaseType.strType.find("COMMENT"))==0))
		{
			cout<< inputLine <<endl;
		}

		//Setting the Curve Parameter Values
		if(((TestCaseType.strType.find("SET"))!=string::npos)&&
			((TestCaseType.strType.find("SET"))==0))
		{
			int i=0;
			token=strtok(inputLine,seps);
			while( token != NULL )
			{
				//While there are tokens in Input Line, Get next token
				token = strtok( NULL, seps );
				pInput[i]=token;
				i++;
			}
			if(strcmp(pInput[0],"ArcConcaveLeft")==0)
			{
				TestCaseType.ArcConcaveLeft=atof(pInput[1]);
				TestClass.setCurveBooleanParameter(kArcConcaveLeft,TestCaseType.ArcConcaveLeft==1 ? true : false);
			}
			else if(strcmp(pInput[0],"ArcDeltaGreaterThan180Degrees")==0)
			{
				TestCaseType.ArcDeltaGreaterThan180Degrees=atof(pInput[1]);
				TestClass.setCurveBooleanParameter(kArcDeltaGreaterThan180Degrees,TestCaseType.ArcDeltaGreaterThan180Degrees==1 ? true : false);
			}
			else if(strcmp(pInput[0],"ArcDelta")==0)
			{
				TestCaseType.ArcDelta=atof(pInput[1]);
				TestClass.setCurveAngleOrBearingParameter(kArcDelta,TestCaseType.ArcDelta);
			}
			else if(strcmp(pInput[0],"ArcStartAngle")==0)
			{
				TestCaseType.ArcStartAngle=atof(pInput[1]);
				TestClass.setCurveAngleOrBearingParameter(kArcStartAngle,TestCaseType.ArcStartAngle);
			}
			else if(strcmp(pInput[0],"ArcEndAngle")==0)
			{
				TestCaseType.ArcEndAngle=atof(pInput[1]);
				TestClass.setCurveAngleOrBearingParameter(kArcEndAngle,TestCaseType.ArcEndAngle);
			}
			else if(strcmp(pInput[0],"ArcDegreeOfCurveChordDef")==0)
			{
				TestCaseType.ArcDegreeOfCurveChordDef=atof(pInput[1]);
				TestClass.setCurveAngleOrBearingParameter(kArcDegreeOfCurveChordDef,TestCaseType.ArcDegreeOfCurveChordDef);
			}
			else if(strcmp(pInput[0],"ArcDegreeOfCurveArcDef")==0)
			{
				TestCaseType.ArcDegreeOfCurveArcDef=atof(pInput[1]);
				TestClass.setCurveAngleOrBearingParameter(kArcDegreeOfCurveArcDef,TestCaseType.ArcDegreeOfCurveArcDef);
			}
			else if(strcmp(pInput[0],"ArcTangentInBearing")==0)
			{
				if(strcmp(pInput[1],"East")==0)
					//East '0' degrees
					pInput[1]="0";
				else if(strcmp(pInput[1],"West")==0)
					//West '180' degrees
					pInput[1]="3.1415926535897932384626433832795";
				else if(strcmp(pInput[1],"North")==0)
					//North '90' degrees
					pInput[1]="1.5707963267948966192313216916398";
				else if(strcmp(pInput[1],"South")==0)
					//South '270' degrees
					pInput[1]="4.7123889803846898576939650749193";
				
				TestCaseType.ArcTangentInBearing=atof(pInput[1]);
				TestClass.setCurveAngleOrBearingParameter(kArcTangentInBearing,TestCaseType.ArcTangentInBearing);
				
			}
			else if(strcmp(pInput[0],"ArcTangentOutBearing")==0)
			{
				if(strcmp(pInput[1],"East")==0)
					//East '0' degrees
					pInput[1]="0";
				else if(strcmp(pInput[1],"West")==0)
					//West '180' degrees
					pInput[1]="3.1415926535897932384626433832795";
				else if(strcmp(pInput[1],"North")==0)
					//North '90' degrees
					pInput[1]="1.5707963267948966192313216916398";
				else if(strcmp(pInput[1],"South")==0)
					//South '270' degrees
					pInput[1]="4.7123889803846898576939650749193";
				
				TestCaseType.ArcTangentOutBearing=atof(pInput[1]);
				TestClass.setCurveAngleOrBearingParameter(kArcTangentOutBearing,TestCaseType.ArcTangentOutBearing);
			}
			else if(strcmp(pInput[0],"ArcChordBearing")==0)
			{
				TestCaseType.ArcChordBearing=atof(pInput[1]);
				TestClass.setCurveAngleOrBearingParameter(kArcChordBearing,TestCaseType.ArcChordBearing);
			}
			else if(strcmp(pInput[0],"ArcRadialInBearing")==0)
			{
				TestCaseType.ArcRadialInBearing=atof(pInput[1]);
				TestClass.setCurveAngleOrBearingParameter(kArcRadialInBearing,TestCaseType.ArcRadialInBearing);
			}
			else if(strcmp(pInput[0],"ArcRadialOutBearing")==0)
			{
				TestCaseType.ArcRadialOutBearing=atof(pInput[1]);
				TestClass.setCurveAngleOrBearingParameter(kArcRadialOutBearing,TestCaseType.ArcRadialOutBearing);
			}
			else if(strcmp(pInput[0],"ArcRadius")==0)
			{
				TestCaseType.ArcRadius=atof(pInput[1]);
				TestClass.setCurveDistanceParameter(kArcRadius,TestCaseType.ArcRadius);
			}
			else if(strcmp(pInput[0],"ArcLength")==0)
			{
				TestCaseType.ArcLength=atof(pInput[1]);
				TestClass.setCurveDistanceParameter(kArcLength,TestCaseType.ArcLength);
			}
			else if(strcmp(pInput[0],"ArcChordLength")==0)
			{
				TestCaseType.ArcChordLength=atof(pInput[1]);
				TestClass.setCurveDistanceParameter(kArcChordLength,TestCaseType.ArcChordLength);
			}
			else if(strcmp(pInput[0],"ArcExternalDistance")==0)
			{
				TestCaseType.ArcExternalDistance=atof(pInput[1]);
				TestClass.setCurveDistanceParameter(kArcExternalDistance,TestCaseType.ArcExternalDistance);
			}
			else if(strcmp(pInput[0],"ArcMiddleOrdinate")==0)
			{
				TestCaseType.ArcMiddleOrdinate=atof(pInput[1]);
				TestClass.setCurveDistanceParameter(kArcMiddleOrdinate,TestCaseType.ArcMiddleOrdinate);
			}
			else if(strcmp(pInput[0],"ArcTangentDistance")==0)
			{
				TestCaseType.ArcTangentDistance=atof(pInput[1]);
				TestClass.setCurveDistanceParameter(kArcTangentDistance,TestCaseType.ArcTangentDistance);
			}
			else if(strcmp(pInput[0],"ArcStartingPoint")==0)
			{
				TestCaseType.ArcStartingPoint.m_XValue=atof(pInput[1]);
				TestCaseType.ArcStartingPoint.m_YValue=atof(pInput[2]);
				TestClass.setCurvePointParameter(kArcStartingPoint,TestCaseType.ArcStartingPoint.m_XValue,TestCaseType.ArcStartingPoint.m_YValue);
			}
			else if(strcmp(pInput[0],"ArcMidPoint")==0)
			{
				TestCaseType.ArcMidPoint.m_XValue=atof(pInput[1]);
				TestCaseType.ArcMidPoint.m_YValue=atof(pInput[2]);
				TestClass.setCurvePointParameter(kArcMidPoint,TestCaseType.ArcMidPoint.m_XValue,TestCaseType.ArcMidPoint.m_YValue);
			}
			else if(strcmp(pInput[0],"ArcEndingPoint")==0)
			{
				TestCaseType.ArcEndingPoint.m_XValue=atof(pInput[1]);
				TestCaseType.ArcEndingPoint.m_YValue=atof(pInput[2]);
				TestClass.setCurvePointParameter(kArcEndingPoint,TestCaseType.ArcEndingPoint.m_XValue,TestCaseType.ArcEndingPoint.m_YValue);
			}
			else if(strcmp(pInput[0],"ArcCenter")==0)
			{
				TestCaseType.ArcCenter.m_XValue=atof(pInput[1]);
				TestCaseType.ArcCenter.m_YValue=atof(pInput[2]);
				TestClass.setCurvePointParameter(kArcCenter,TestCaseType.ArcCenter.m_XValue,TestCaseType.ArcCenter.m_YValue);
			}
			else if(strcmp(pInput[0],"ArcExternalPoint")==0)
			{
				TestCaseType.ArcExternalPoint.m_XValue=atof(pInput[1]);
				TestCaseType.ArcExternalPoint.m_YValue=atof(pInput[2]);
				TestClass.setCurvePointParameter(kArcExternalPoint,TestCaseType.ArcExternalPoint.m_XValue,TestCaseType.ArcExternalPoint.m_YValue);
			}
			else if(strcmp(pInput[0],"ArcChordMidPoint")==0)
			{
				TestCaseType.ArcChordMidPoint.m_XValue=atof(pInput[1]);
				TestCaseType.ArcChordMidPoint.m_YValue=atof(pInput[2]);
				TestClass.setCurvePointParameter(kArcChordMidPoint,TestCaseType.ArcChordMidPoint.m_XValue,TestCaseType.ArcChordMidPoint.m_YValue);
			}
		}
		
		//Testing the Calculated Curve Parameter Value with the Expected Value
		if(((TestCaseType.strType.find("TEST"))!=string::npos)&&((TestCaseType.strType.find("TEST"))==0))
		{
			bPointParameter=false;
			//Increase the Number of Test Cases by one
			iNCases++;
			cout<<(iNCases-1)<<", ";			 
			int i=0;
			token=strtok(inputLine,seps);
			//Store the Test Case Input Line in pInput[]
			while( token != NULL )
			{
				//While there are tokens in "inputLine",Get next token
				token = strtok( NULL, seps );
				pInput[i]=token;
				i++;
			}
			//convert angle, bearing or distance from double format to string format
			strReal = DoubleToString(atof(pInput[1]));
			strRealX = DoubleToString(atof(pInput[1]));
			
			//If the Input is a Point Parameter
			if(pInput[2])
			{
				//convert point to string format
				bPointParameter=true;
				strReal = PointToString(atof(pInput[1]),atof(pInput[2]));
				strRealY = DoubleToString(atof(pInput[2]));
			}
			
			//Check for all the Curve Parameters
			for(i=0;i<NUMBEROFVARIABLES;i++)
			{
				// if the expected output is of type angle or bearing
				bool bIsAngle = false;

				//Test the Curve Parameter of the Input file if it is in the list of the CurveParameters
				if(strcmpi(pInput[0],CurveParameters[i])==0)
				{
					time = clock();
					try{
						if(CurveParameters[i]=="ArcConcaveLeft")
						{
							strCurveParam=TestClass.getCurveParameter(kArcConcaveLeft);
						}
						else if(CurveParameters[i]=="ArcDeltaGreaterThan180Degrees")
						{
							strCurveParam=TestClass.getCurveParameter(kArcDeltaGreaterThan180Degrees);
						}
						else if(CurveParameters[i]=="ArcDelta")
						{
							strCurveParam=TestClass.getCurveParameter(kArcDelta);
							bIsAngle = true;
						}
						else if(CurveParameters[i]=="ArcStartAngle")
						{
							strCurveParam=TestClass.getCurveParameter(kArcStartAngle);
							bIsAngle = true;
						}
						else if(CurveParameters[i]=="ArcEndAngle")
						{
							strCurveParam=TestClass.getCurveParameter(kArcEndAngle);
							bIsAngle = true;
						}
						else if(CurveParameters[i]=="ArcDegreeOfCurveChordDef")
						{
							strCurveParam=TestClass.getCurveParameter(kArcDegreeOfCurveChordDef);
							bIsAngle = true;
						}
						else if(CurveParameters[i]=="ArcDegreeOfCurveArcDef")
						{
							strCurveParam=TestClass.getCurveParameter(kArcDegreeOfCurveArcDef);
							bIsAngle = true;
						}
						else if(CurveParameters[i]=="ArcTangentInBearing")
						{
							strCurveParam=TestClass.getCurveParameter(kArcTangentInBearing);
							bIsAngle = true;
						}
						else if(CurveParameters[i]=="ArcTangentOutBearing")
						{
							strCurveParam=TestClass.getCurveParameter(kArcTangentOutBearing);
							bIsAngle = true;
						}
						else if(CurveParameters[i]=="ArcChordBearing")
						{
							strCurveParam=TestClass.getCurveParameter(kArcChordBearing);
							bIsAngle = true;
						}
						else if(CurveParameters[i]=="ArcRadialInBearing")
						{
							strCurveParam=TestClass.getCurveParameter(kArcRadialInBearing);
							bIsAngle = true;
						}
						else if(CurveParameters[i]=="ArcRadialOutBearing")
						{
							strCurveParam=TestClass.getCurveParameter(kArcRadialOutBearing);
							bIsAngle = true;
						}
						else if(CurveParameters[i]=="ArcRadius")
						{
							strCurveParam=TestClass.getCurveParameter(kArcRadius);
						}
						else if(CurveParameters[i]=="ArcLength")
						{
							strCurveParam=TestClass.getCurveParameter(kArcLength);
						}
						else if(CurveParameters[i]=="ArcChordLength")
						{
							strCurveParam=TestClass.getCurveParameter(kArcChordLength);
						}
						else if(CurveParameters[i]=="ArcExternalDistance")
						{
							strCurveParam=TestClass.getCurveParameter(kArcExternalDistance);
						}
						else if(CurveParameters[i]=="ArcMiddleOrdinate")
						{
							strCurveParam=TestClass.getCurveParameter(kArcMiddleOrdinate);
						}
						else if(CurveParameters[i]=="ArcTangentDistance")
						{
							strCurveParam=TestClass.getCurveParameter(kArcTangentDistance);
						}
						else if(CurveParameters[i]=="ArcStartingPoint")
						{
							strCurveParam=TestClass.getCurveParameter(kArcStartingPoint);
						}
						else if(CurveParameters[i]=="ArcMidPoint")
						{
							strCurveParam=TestClass.getCurveParameter(kArcMidPoint);
						}
						else if(CurveParameters[i]=="ArcEndingPoint")
						{
							strCurveParam=TestClass.getCurveParameter(kArcEndingPoint);
						}
						else if(CurveParameters[i]=="ArcCenter")
						{
							strCurveParam=TestClass.getCurveParameter(kArcCenter);
						}
						else if(CurveParameters[i]=="ArcExternalPoint")
						{
							strCurveParam=TestClass.getCurveParameter(kArcExternalPoint);
						}
						else if(CurveParameters[i]=="ArcChordMidPoint")
						{
							strCurveParam=TestClass.getCurveParameter(kArcChordMidPoint);
						}
						else if(CurveParameters[i]=="canCalculateAllParameters")
						{
							if(TestClass.canCalculateAllParameters())
								strCurveParam.assign("1");
							else
								strCurveParam.assign("0");
						}
					}
					catch (UCLIDException &ue)
					{
						string str= ue.getTopText();
						cout<< "Exception caught:"<< str << endl;
					}
					catch (...)
					{
						cout<< "unknown exception in cce.dll"<< endl;
					}
					
					difftime = clock() - time;
					secs = (float) difftime/CLOCKS_PER_SEC;
					iTotalTime += difftime;
					
					if(bPointParameter)
					{
						int iPos=strCurveParam.find(',');
						int len=strCurveParam.length();
						string strTemp1,strTemp2;
						
						strTemp1=strCurveParam.substr(0,iPos);
						int iPos1=strTemp1.find('.');				
						
						strTemp2=strCurveParam.substr(iPos+1,len);
						int iPos2=strTemp2.find('.');
						
						double dCurvePointParamX=(double)atof((const char*)strTemp1.data());
						double dCurvePointParamY=(double)atof((const char*)strTemp2.data());
						double dRealX=(double)atof(strRealX.data());
						double dRealY=(double)atof(strRealY.data());
						
						//comparing the calculated and expected values in the form of doubles with
						//fault tolerance set to 0.0000005.
						if((fabs(dCurvePointParamX-dRealX))>0.0000005||(fabs(dCurvePointParamY-dRealY))>0.0000005)
						{
							//If Expected Parameter Value extracted from input file and the Real Parameter Value is not same
							//Incrementing number of Failed cases
							iFailedCases++;
							strResult="Failed";
						}
						else 
							strResult="Passed";
						//writing to the display 
						cout<<strResult<<", "<<iLineNumber<<", "<<CurveParameters[i]<<", ("<</*dCurvePointParamX*/strTemp1.c_str()<<", "<</*dCurvePointParamY*/strTemp2.c_str()<<"), ("<</*dRealX*/strRealX<<", "<</*dRealY*/strRealY<<")\n";
					}
					else
					{
						double dReal=(double)atof(strReal.data());
						double dCurveParam=(double)atof((const char*)strCurveParam.data());
						//comparing the calculated and expected values in the form of doubles with
						//fault tolerance set to 0.0000005.
						if(fabs(dCurveParam-dReal)>0.0000005)
						{
							if (bIsAngle)
							{
								if (fabs(fabs(dCurveParam - dReal) - 2*gdPI) > 0.0000005)
								{
									iFailedCases++;
									strResult="Failed";
								}
								else
								{
									strResult = "Passed";
								}
							}
							else
							{
								//If Expected Parameter Value extracted from input file and the Real Parameter Value is not same
								//Incrementing number of Failed cases
								iFailedCases++;
								strResult="Failed";
							}
						}
						else 
						{
							strResult="Passed";
						}
						//writing to the display 
						cout<<strResult<<", "<<iLineNumber<<", "<<CurveParameters[i]<<", "<<strCurveParam.c_str() /*dCurveParam*/<<", "<<strReal.c_str() /*dReal*/<<"\n";
					}
				}
				strCurveParam.~basic_string();
			}
			
		}
}
fPercResult = (float)(iNCases-iFailedCases)/(float)iNCases;
fPercResult = fPercResult * 100;
secs = (float) iTotalTime/CLOCKS_PER_SEC;

cout<<"\nTotal test cases processed : "<<iNCases;
cout<<"\nTotal test cases passed    : "<<iNCases-iFailedCases;
cout<<"\nTotal test cases failed    : "<<iFailedCases;
cout<<"\nPercentage success	   : "<<fPercResult<<"%";
cout<<"\nTotal processing time      : "<<secs<<"S\n";

TestCaseInput_file.close(); 

if(iFailedCases>0)
return EXIT_FAILURE;
else
return EXIT_SUCCESS;
}
//==========================================================================================
string DoubleToString(double dValue)
{
	CString pszBuffer("");
	pszBuffer.Format("%.10f", dValue);
	return (LPCTSTR)pszBuffer;
}
//==========================================================================================
string PointToString(double dX, double dY)
{
	CString pszBuffer("");
	pszBuffer.Format("%.10f,%.10f", dX, dY);

	return (LPCTSTR)pszBuffer;
}
//==========================================================================================
