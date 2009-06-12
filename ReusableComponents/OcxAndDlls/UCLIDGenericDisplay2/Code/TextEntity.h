//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	TextEntity.h
//
// PURPOSE:	This is an header file for TextEntity 
//			where this class has been derived from the GenericEntity()
//			class.  The code written in this file makes it possible for
//			initialize the combo controls.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#include "stdafx.h"
#include "GenericEntity.h"
#include <string>


//struct to store text box dimensions
struct TextBoxDims
{
	Point leftBottom;
	Point rightBottom;
	Point rightTop;
	Point leftTop;
};

//==================================================================================================
//
// CLASS:	TextEntity
//
// PURPOSE:	To encapsulate the concept of a text entity, which is used by the GenericDisplay
//			class.
//
// REQUIRE:	Nothing.
// 
// INVARIANTS:
//			None.
//
// EXTENSIONS:
//			None.
//
// NOTES:	This class would probably be converted to a structure when used in the ActiveX framework.
//
class TextEntity : public GenericEntity
{
public:
	TextEntity(unsigned long id);
	virtual ~TextEntity();

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To initialize the Text entity.
	// REQUIRE: Nothing.
	// PROMISE: To assign the arguments to corresponding class datamembers.
	TextEntity(unsigned long id, Point insertionPoint, string strText, unsigned char ucAlignment, 
				double RotationAngInDeg, double dTextHeight, string strFontName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To display the Text entity.
	// REQUIRE: visibility of the entity. True - visible; False -- invisible.
	// PROMISE: check the visibilty of an entity and return if it is false othervise draws the line.
	//			create the font with the specified font height and all if the font height is > 0
	virtual void EntDraw (BOOL bDraw);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the extents of the Text entity.
	// REQUIRE: GDRectangle object to fill the extents of an entity.
	// PROMISE: Get the extents from Textbox and caluculates the bounding rectangle dimensions.
	void getExtents(GDRectangle& rBoundingRectangle);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the description of the Text entity.
	// REQUIRE: Nothing.
	// PROMISE: return the text "text" to let the users that this is text entity.
	string getDesc();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To move the Text entity.
	// REQUIRE: no. of units to move in x and y directions.
	// PROMISE: adds movex and movey to the insertions point.
	void offsetBy(double dX, double dY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To calculate the extents of the Text entity.
	// REQUIRE: Nothing explicitly except the class data members.
	// PROMISE: caluculate the textbox that exactly bounds the text and stores the dimensions is 
	//			data member m_textboxDims strcuture.
	virtual void ComputeEntExtents ();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To check whether the given point is on the Text entity.
	// REQUIRE: point to test.
	// PROMISE: returns true if the given point is on the text entity otherwise false.
	BOOL isPtOnText(int, int);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the string of the Text entity.
	// REQUIRE: Must be in the format of 
	//			"TextInsertionPointX:TextInsertionPointY,Text,TextAlignment,TextRotation,TextHeight,TextFont"
	//			
	// PROMISE: 
	virtual int getEntDataString(CString &zDataString);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get distance from the given point to the Text entity.
	// REQUIRE: x and y co-ordinates of the point .
	// PROMISE: To return the distance of the entity from the given point.
	virtual double getDistanceToEntity(double dX, double dY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To set the new text to the current Text entity.
	// REQUIRE: new data string to set.
	// PROMISE: sets new string to the existing entity string.
	void setText(string strText) { m_strText = strText; }
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the text from the current Text entity.
	// REQUIRE: Nothing.
	// PROMISE: gets string from the existing entity.
	string getText() { return m_strText; }
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the distance from the given point a line 
	// REQUIRE: start and end points of a line and a point from which it has to calculate distance.
	// PROMISE: calculate distance and return it.
	double distanceToLine(int iX1, int iX2, int iY1, int iY2, int ix, int iY);	
protected:
	// the insertion point of the text
	Point m_insertionPoint;
	
	// the actual text of the entity
	string m_strText;

	// the alignment of the text entity.  More details about this attribute are
	// in the REQUIRE statement of the addTextEntity() method.
	unsigned char m_ucAlignment;
	
	// the rotation angle of this text entity, specified in degrees.
	double m_dRotationAngleInDegees;
	
	// the height of this text entity, specified in terms of the default units associated with
	// the GenericDisplay view.
	double m_dTextHeight;

	// the font used to display this text in the GenericDisplay view.
	string m_strFontName;

	// strucure variable to store text box dimensions
	TextBoxDims m_textBoxDims;

	// center point of text 
	Point m_centerPoint;

	// Text region. contains 4 corner points of the text. This is not same as m_textBoxDims which
	// is rectangle bonding the text.Region may not be rectangle.It can be angular. Added to test
	// whether clicked point is on text entity in isPointOnText() method.
	//CRgn m_textRgn;
	CPoint m_textVertices[4];
};
//==================================================================================================
