#pragma once

#include "LeadUtils.h"

///////////////////////////
// LineRect
///////////////////////////
// PURPOSE: To allow CRects used to indicate line area to be easily
// converted for use in vertical orientation as well as horizontal
// orientation
class LEADUTILS_API LineRect : public CRect
{
public:
	// Initializes with a new unique ID
	LineRect(bool bHorizontal);
	// Initializes with a new unique ID
	LineRect(const RECT& srcRect, bool bHorizontal);
	// Assumes the ID of rect rather than initializing with a new unique ID
	LineRect(const LineRect &rect);

	// NOTE: It is currently not possible for the assignment operator to change the existing line
	// orientation. The ID of source will overwrite this objects existing ID.
	LineRect& operator =(const LineRect &source);

	//-------------------------------------------------------------------------------------------------
	// PROMISE: To provide the length of the line, whether the line is vertical or horizontal
	int LineLength() const;
	//-------------------------------------------------------------------------------------------------
	// PROMISE: To provide the width of the line, whether the line is vertical or horizontal
	int LineWidth() const;
	//-------------------------------------------------------------------------------------------------
	// PROMISE: To inflate the length/width of the line by the specified amounts, whether the line is 
	// vertical or horizontal
	void InflateLine(int nLength, int nWidth);
	//-------------------------------------------------------------------------------------------------
	// PROMISE: To return the y position of a horizontal line or the x position of a vertical line
	int LinePosition() const;
	//-------------------------------------------------------------------------------------------------
	// PROMISE: To return the x component of the center point of a horizontal line or the y position 
	// of the center point of a vertical line
	int LineMiddle() const;
	//-------------------------------------------------------------------------------------------------
	// PROMISE: To return whether the line is horizontal or vertical
	bool IsHorizontal() const { return m_bHorizontal; }
	//-------------------------------------------------------------------------------------------------
	// PROMISE: To return an ID to uniquely identify this line rect.
	long GetID() const { return m_nID; }
	//-------------------------------------------------------------------------------------------------

	// Refers to left for a horizontal line, top for a vertical line
	LONG &m_nLineTopOrLeftEnd;

	// Refers to right for a horizontal line, bottom for a vertical line
	LONG &m_nLineBottomOrRightEnd;

	// Refers to top for a horizontal line, left for a vertical line
	LONG &m_nLineTopOrLeftEdge;

	// Refers to bottom for a horizontal line, right for a vertical line
	LONG &m_nLineBottomOrRightEdge;

private:

	/////////////////
	// Variables
	/////////////////
	
	bool m_bHorizontal;

	long m_nID;

	static long m_nLastAssignedId;
};
