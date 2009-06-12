#include "stdafx.h"
#include "LineRect.h"

#include <cpputil.h>
#include <UCLIDExceptionDlg.h>

//-------------------------------------------------------------------------------------------------
// Statics
//-------------------------------------------------------------------------------------------------
long LineRect::m_nLastAssignedId = 0;

//-------------------------------------------------------------------------------------------------
// LineRect
//-------------------------------------------------------------------------------------------------
LineRect::LineRect(bool bHorizontal) :
	CRect(),
	m_bHorizontal(bHorizontal),
	m_nLineTopOrLeftEnd(bHorizontal ? left : top),
	m_nLineBottomOrRightEnd(bHorizontal ? right : bottom),
	m_nLineTopOrLeftEdge(bHorizontal ? top : left),
	m_nLineBottomOrRightEdge(bHorizontal ? bottom : right)
{
	// NOTE: nLine* variable are not separate variables, they are reference variables
	// nLine reference variables are set to point to top, bottom, left or right here.

	// Assign a new ID
	m_nID = m_nLastAssignedId++;
}
//-------------------------------------------------------------------------------------------------
LineRect::LineRect(const RECT &rect, bool bHorizontal) :
	CRect(rect),
	m_bHorizontal(bHorizontal),
	m_nLineTopOrLeftEnd(bHorizontal ? left : top),
	m_nLineBottomOrRightEnd(bHorizontal ? right : bottom),
	m_nLineTopOrLeftEdge(bHorizontal ? top : left),
	m_nLineBottomOrRightEdge(bHorizontal ? bottom : right)
{
	// NOTE: nLine* variable are not separate variables, they are reference variables
	// nLine reference variables are set to point to top, bottom, left or right here.

	// Assign a new ID
	m_nID = m_nLastAssignedId++;
}
//-------------------------------------------------------------------------------------------------
LineRect::LineRect(const LineRect &rect) :
	CRect(rect),
	m_bHorizontal(rect.m_bHorizontal),
	m_nLineTopOrLeftEnd(rect.m_bHorizontal ? left : top),
	m_nLineBottomOrRightEnd(rect.m_bHorizontal ? right : bottom),
	m_nLineTopOrLeftEdge(rect.m_bHorizontal ? top : left),
	m_nLineBottomOrRightEdge(rect.m_bHorizontal ? bottom : right)
{
	// NOTE: nLine* variable are not separate variables, they are reference variables
	// nLine reference variables are set to point to top, bottom, left or right here.

	// Assume the ID of rect.
	m_nID = rect.m_nID;
}
//-------------------------------------------------------------------------------------------------
LineRect& LineRect::operator =(const LineRect &source)
{
	// I'm not sure how to re-point the nLine* variables in the case that orientation changes.
	// However, since this is a situation that will not arise from FindLines or GroupLines,
	// just assert that this situation is not encountered for the time being.
	if (m_bHorizontal != source.m_bHorizontal)
	{
		UCLIDException ue("ELI19269", "Internal error: LineRect orientation mismatch!");
		throw ue;
	}

	// Call CRect's assignment operator
	CRect::operator = (source);

	// Assume the ID of source.
	m_nID = source.m_nID;
	
	return *this;
}
//-------------------------------------------------------------------------------------------------
int LineRect::LineLength() const
{
	// If the line is horizontal, the length is CRect::Width(); otherwise its CRect::Height()
	if (m_bHorizontal)
	{
		return Width();
	}
	else
	{
		return Height();
	}
}
//-------------------------------------------------------------------------------------------------
int LineRect::LineWidth() const
{
	// If the line is horizontal, the height is CRect::Height(); otherwise its CRect::Width()
	if (m_bHorizontal)
	{
		return Height();
	}
	else
	{
		return Width();
	}
}
//-------------------------------------------------------------------------------------------------
void LineRect::InflateLine(int nLength, int nWidth)
{
	// If the line is horizontal, the length is the x position and y is the width;
	// otherwise, vise-versa
	if (m_bHorizontal)
	{
		CRect::InflateRect(nLength, nWidth);
	}
	else
	{
		CRect::InflateRect(nWidth, nLength);
	}
}
//-------------------------------------------------------------------------------------------------
int LineRect::LinePosition() const
{
	// Obtain the center point of the line width-wise
	if (m_bHorizontal)
	{
		return CenterPoint().y;
	}
	else
	{
		return CenterPoint().x;
	}
}
//-------------------------------------------------------------------------------------------------
int LineRect::LineMiddle() const
{
	// Obtain the center point of the line length-wise
	if (m_bHorizontal)
	{
		return CenterPoint().x;
	}
	else
	{
		return CenterPoint().y;
	}
}
//-------------------------------------------------------------------------------------------------