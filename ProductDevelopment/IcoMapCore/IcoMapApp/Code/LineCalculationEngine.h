//==================================================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// $Archive: /Engineering/ProductDevelopment/IcoMapCore/IcoMapApp/Code/LineCalculationEngine.h $
//
// AUTHORS:	Duan Wang
//
//==================================================================================================
#pragma once

#include <TPPoint.h>

//==================================================================================================
//
// CLASS:	LineCalculationEngine
//
// PURPOSE:	To take either a point, an angle and a distance or two points to calculate a line.
//			The Start point, End point, Line Angle (which is formed from start point to end point, 
//			relative to the X axis for the line with positive angles going counterclockwise when 
//			looking down the Z axis towards the origin) can be retrived afterwards.
//
//==================================================================================================

class LineCalculationEngine
{
public:
	//----------------------------------------------------------------------------------------------
	// Purpose: Default constructor, none of the private memeber variables is initialized
	//
	// Require: Nothing.
	//
	LineCalculationEngine();
	//----------------------------------------------------------------------------------------------
	// Purpose: Constructs a line with a start point, an angle and a distance
	//
	// Require: Nothing
	//
	// Arg:		startPoint: start point of the line
	//			dAngleRadian: angle of the line in Radian (not degree)
	//			dLength: length of the line
	//
	LineCalculationEngine(const TPPoint& startPoint, double dAngleRadian, double dLength);
	//----------------------------------------------------------------------------------------------
	// Purpose: Constructs a line with two points
	//
	// Require: Nothing
	//			
	LineCalculationEngine(const TPPoint& startPoint, const TPPoint& endPoint);
	//----------------------------------------------------------------------------------------------
	// Purpose: Copy constructor
	//
	// Require: Nothing
	//			
	LineCalculationEngine(const LineCalculationEngine& lineCalculationEngine);
	//----------------------------------------------------------------------------------------------
	// Purpose: Overloads the = operator
	//
	// Require: Nothing
	//			
	LineCalculationEngine& operator = (const LineCalculationEngine& lineCalculationEngine);
	//----------------------------------------------------------------------------------------------
	// Purpose: Default destructor
	//
	// Require: Nothing.
	//
	virtual ~LineCalculationEngine();
	//----------------------------------------------------------------------------------------------
	// Purpose: Sets Start point of the line
	//
	// Require: 
	//			
	void setStartPoint(const TPPoint& startPoint);
	//----------------------------------------------------------------------------------------------
	// Purpose: Sets End point of the line
	//
	// Require: 
	//			
	void setEndPoint(const TPPoint& endPoint);
	//----------------------------------------------------------------------------------------------
	// Purpose: Sets the angle in radian for the line
	//
	// Require: angle must be set
	//
	// Arg:		dAngleRadian: returns the value for the angle in radian
	//
	void setAngleRadians(double dAngleRadian);
	//----------------------------------------------------------------------------------------------
	// Purpose: Sets the angle in degrees for the line
	//
	// Require: angle must be set
	//
	// Arg:		dDegrees: the angle in degrees
	//
	void setAngleDegrees(double dDegrees);
	//----------------------------------------------------------------------------------------------
	// Purpose: Sets the length of the line
	//
	// Require: length must be set
	//
	// Arg:		dLength: the length for the line
	//
	void setLength(double dLength);	
	//----------------------------------------------------------------------------------------------
	// Purpose: Returns Start point of the line
	//
	// Require: start point must have been set
	//			
	TPPoint getStartPoint();
	//----------------------------------------------------------------------------------------------
	// Purpose: Returns End point of the line 
	//
	// Require: end point must have been set
	//			
	TPPoint getEndPoint();
	//----------------------------------------------------------------------------------------------
	// Purpose: Returns the angle in radian for the line
	//
	// Require: angle must be set
	//
	double getAngleRadians();
	//----------------------------------------------------------------------------------------------
	// Purpose: Returns the length of the line
	//
	// Require: length must be set
	//
	// Arg:		dLength: the length for the line
	//
	double getLength();
	//----------------------------------------------------------------------------------------------
	// Purpose: Set the previous line/curve tangent out angle
	//
	void setPrevTangentOutAngle(double dPrevTangentOutInRadians);
	//----------------------------------------------------------------------------------------------
	// Purpose: Set internal or deflection angle
	//
	void setInternalOrDeflectionAngle(double dInternDefAngleInRadians);
	//----------------------------------------------------------------------------------------------
	// Purpose: Along the previous line/curve tangent-out angle, the internal/deflection 
	//			angle is to the left or right
	//
	void internalOrDeflectionAngleIsLeft(bool bIsLeft = true);
	//----------------------------------------------------------------------------------------------
	// Purpose: Set current angle interpretation, whether it's deflection angle or internal angle
	//
	// Args:	bIsDeflectionAngle: true -- is deflection angle
	//								false -- is internal angle
	void setAngleInterpretation(bool bIsDeflectionAngle = true);
	//----------------------------------------------------------------------------------------------
	// Purpose: To reset the line calculation engine back to start state (i.e. nothing is set yet)
	//
	// Require: Nothing
	//
	void reset();
	//----------------------------------------------------------------------------------------------
	// Purpose: Whehter or not the line is calculated based upon deflection/internal angle
	//
	// Require: Nothing
	//
	const bool isDeflectionInternalAngleSet() const {return m_bInternDefAngleSet;}

private:

	TPPoint m_StartPoint;
	TPPoint m_EndPoint;
	// the polar angle value for the line in radians
	double m_dLineAngle;
	double m_dLength;

	// the polar angle value for the tangent-out angle of the previous line/curve
	// Require: there must be at least one line/curve ahead of current line
	double m_dPrevTangentOutAngle;
	// the polar angle value for the internal/deflection angle to form the actual line
	double m_dInternDefAngle;
	// whether or not the internal/deflection angle is on the left side of the line
	bool m_bInternDefAngleLeft;
	// whether or not the internal/deflection angle is a small angle
	bool m_bIsDeflectionAngle;


	//if start, end point, angle , Length are set
	bool m_bStartPointSet;
	bool m_bEndPointSet;
	bool m_bLineAngleSet;
	bool m_bLengthSet;
	bool m_bInternDefAngleSet;
	bool m_bPrevTangentOutAngleSet;

	// Helper functions
	// get angle value in between 0 and 2*PI
	double getAngleWithinFirstPeriodInRadians(double dInAngle);

};
