#include<iostream>
#include<fstream>
#include<stdlib.h>
using namespace std;

struct STARTINGPOINT
{
	double m_XValue;
	double m_YValue;
};	

struct MIDPOINT
{
	double m_XValue;
	double m_YValue;
};	

struct ENDINGPOINT
{
	double m_XValue;
	double m_YValue;
};	

struct CENTERPOINT
{
	double m_XValue;
	double m_YValue;
};	

struct EXTERNALPOINT
{
	double m_XValue;
	double m_YValue;
};	

struct CHORDMIDPOINT
{
	double m_XValue;
	double m_YValue;
};	

//This Testcase structure stores values given in Test case input file
typedef struct tag_TESTCASE
{
	//Type of the Input in the input file like 'RESET' or 'SET' or 'TEST'
	string strType;
	
	// boolean parameters:
	double ArcConcaveLeft;					// concavity depends on direction of arc w.r.t. the chord
	double ArcDeltaGreaterThan180Degrees;	// true if ArcDelta is greater than 180 degrees

	// angle/bearing parameters:
	// (bearings are a type of angle)
	double ArcDelta;						// center angle for the circular arc
	double ArcStartAngle;					// angle from ArcCenter to ArcStartingPoint
	double ArcEndAngle;						// angle from ArcCenter to ArcEndingpoint
	double ArcDegreeOfCurveChordDef;		// using this definition, radius = 5729.6506/DegreeOfCurve
	double ArcDegreeOfCurveArcDef;			// using this definition, radius = 5729.5780/DegreeOfCurve
	double ArcTangentInBearing;				// in-tangent for the arc, touching arc at ArcStartingPoint
	double ArcTangentOutBearing;			// out-tangent for the arc, touching arc at ArcEndingPoint
	double ArcChordBearing;					// bearing from ArcStartingPoint to ArcEndingPoint
	double ArcRadialInBearing;				// bearing from ArcStartingPoint to ArcCenter
	double ArcRadialOutBearing;				// bearing from ArcCenter to ArcEndingPoint

	// distance paramters:
	double ArcRadius;						// radius of the circle associated with the arc
	double ArcLength;						// length of the circular arc
	double ArcChordLength;					// distance from ArcStartingPoint to ArcEndingPoint
	double ArcExternalDistance;				// distance from ArcExternalPoint to ArcMidPoint
	double ArcMiddleOrdinate;				// distance from ArcChordMidPoint to ArcMidPoint
	double ArcTangentDistance;				// distance from StartingPoint to ArcExternalPoint

	// point parameters:
	struct STARTINGPOINT ArcStartingPoint;	// starting point of arc
	struct MIDPOINT ArcMidPoint;			// mid point of arc
	struct ENDINGPOINT ArcEndingPoint;		// ending point of arc
	struct CENTERPOINT ArcCenter;			// center of the circle related to the arc
	struct EXTERNALPOINT ArcExternalPoint;	// intersection point of the in-tangent and out-tangent
	struct CHORDMIDPOINT ArcChordMidPoint;	// mid point of the curve chord

}TESTCASE;









