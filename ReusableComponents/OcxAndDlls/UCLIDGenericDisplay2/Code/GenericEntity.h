//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericEntity.h
//
// PURPOSE:	This is an header file for GenericEntity class
//			where this has been declared as base class.
//			The code written in this file makes it possible for
//			initialize to set the view.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================

#pragma once

#include "stdafx.h"
#include "Rectangle.h"
#include "EntityAttributes.h"
#include "Point.h"

#include <afxwin.h>
#include <string>

using namespace std;

//==================================================================================================
//
// CLASS:	GenericEntity
//
// PURPOSE:	To encapsulate the concept of a generic entity, which is used by the GenericDisplay
//			class.
//
// REQUIRE:	Nothing.
// 
// INVARIANTS:
//			Any GenericEntity derived object that has been created will always have two special
//			attributes associated with it, in addition to the application specific attributes that
//			could be associated with the entity. The first special attribute's name is "Type" and
//			the value of this attribute will be the result of the getDesc() call on this object.
//			The second special attribute's name is "Visible", and the default value of this
//			attribute during creation will always be "1".  Any GenericEntity derived entity, whose
//			"Visible" attribute has a value of "0" will not be visible in the GenericDisplay view.
//			On the contrary, any GenericEntity derived entity, whose "Visible" attribute has a value
//			of "1" will always be visible in the GenericDisplay view.  
//
// EXTENSIONS:
//			None.
//
// NOTES:	For this class and all its derived classes, mostly only class attributes are defined
//			and few member functions have been defined because it is not clear as to how these
//			classes will be best represented in the ActiveX/COM framework.  It is important to
//			realize that although mostly only attributes are defined here,the final implementation
//			(to be proposed by InfoTech to UCLID) will need to provide some mechanism to set/get 
//			these attributes.
//

class CGenericDisplayCtrl;

class GenericEntity
{
public:

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Initializes an entity using the specified id.
	GenericEntity(unsigned long id);
	GenericEntity(unsigned long id, EntityAttributes& , COLORREF );

public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To obtain the ID of this entity.
	// REQUIRE: Nothing.
	// PROMISE: To return the ID of this entity.  The returned ID will be NULL if this object
	//			has not yet been added to the internal "database" of entities.  If this
	//			entity is already in the "database" of entities, the its unique ID will be returned.
	unsigned long getID();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To retrieve the attributes associated with this entity.
	// REQUIRE: Nothing.
	// PROMISE: To return all attributes associated with this entity.
	const EntityAttributes& getAttributes() const;
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To modify a specific attribute associated with this entity.
	// REQUIRE: An attribute with the specified attribute name (strAttributeName) will already
	//			be associated with this entity.
	//			strNewValue != __nullptr.
	// PROMISE: To modify the value of the specified attribute to the specified new value.  Any
	//			functionality of associated with the entity that depends upon the value of a certain
	//			attribute will operate based upon the new value of the attribute.  For instance,
	//			if the "Visible" attribute is turned on/off through this method, this entity will
	//			become visible/invisible in the GenericDisplay view accordingly.
	int modifyAttribute(const string& strAttributeName, const string& strNewValue);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To add a new attribute to be associated with this entity.
	// REQUIRE: An attribute with the specified attribute name (strAttributeName) is not already
	//			associated with this entity.
	// PROMISE: To add the specified attribute name/value to the list of attributes associated with
	//			this GenericEntity.  A subsequent call to getAttributes() will contain the newly
	//			added attribute in the list of returned attributes.
	void addAttribute(const string& strAttributeName, const string& strValue);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To delete a specific attribute associated with this entity.
	// REQUIRE: An attribute with the specified attribute name (strAttributeName) will already
	//			be associated with this entity.
	// PROMISE: To delete the specified attribute from the list of attributes associated with this
	//			GenericEntity.
	bool deleteAttribute(const string& strAttributeName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To retrieve the color associated with this entity.
	// REQUIRE: Nothing.
	// PROMISE: To return the color with which this entity is drawn in the GenericDisplay view.
	COLORREF getColor() ;
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To modify the color associated with this entity.
	// REQUIRE: Nothing.
	// PROMISE: To modify the color with which this entity is drawn in the GenericDisplay view to
	//			newColor.
	void setColor(COLORREF newColor);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To retrieve the graphical extents associated with this entity.
	// REQUIRE: Nothing.
	// PROMISE: To return the topLeft and bottomRight coordinates of the smallest rectangle that
	//			can bound this entity.
	virtual void getExtents(GDRectangle& rBoundingRectangle) = 0;
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To retrieve the description of this entity.
	// REQUIRE: Nothing.
	// PROMISE: To return a string that describes the type of this entity.
	virtual string getDesc()= 0;
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To offset this entity by a given amount
	// REQUIRE:	Nothing.
	// PROMISE:	To move this entity horizontally and vertically by dX and dY respectively.
	// NOTE:	The following method has been made pure virtual.
	virtual void offsetBy(double dX, double dY) = 0;
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the visibility flag for the entity
	// REQUIRE:	Nothing.
	// PROMISE:	To draw the entities only if the visibility flag is set
	bool getVisible() {return m_bVisible;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To set the visibility flag for the entity
	// REQUIRE:	Nothing.
	// PROMISE:	To define the visiblity for the entity. Checked while drawing the entities
	void setVisible(bool bEntVis) {m_bVisible = bEntVis;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get whether this highlight is selected
	BOOL getSelected() {return m_bSelected;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To set whether this highlight is selected
	void setSelected(BOOL bSelected) { m_bSelected = bSelected;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To draw the entities
	// REQUIRE:	Nothing.
	// PROMISE:	To draw the entity
	// NOTE:	The following method has been made pure virtual.
	virtual void EntDraw(BOOL bDraw) = 0;
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To compute the entity extents
	// REQUIRE:	Nothing.
	// PROMISE:	To compute the entity extents for each entity
	// NOTE:	The following method has been made pure virtual.
	virtual void ComputeEntExtents() = 0;	
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get info of the entity in the string format
	// REQUIRE:	Nothing.
	// PROMISE:	Return the length of the string
	virtual int getEntDataString(CString &zDataString) = 0;
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To Compute incremental angle based on the radius and chord height
	// REQUIRE:	Nothing.
	// PROMISE:	To compute an incremental angle such that the curve drawing is smooth
	double incrementAngle(double dRadius);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the attribute string
	// REQUIRE:	Nothing.
	// PROMISE:	None
	virtual int getAttributeString(CString &zAttrStr, BOOL bFlag);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To set the attributes
	// REQUIRE:	Nothing.
	// PROMISE:	None
	void setAttributesFromFile(CString zAttrFileStr);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To check the entity attributes
	// REQUIRE:	Nothing.
	// PROMISE:	None
	BOOL checkForEntAttr(CString zStrAttr);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the distance from entity
	// REQUIRE:	Nothing.
	// PROMISE:	None
	virtual double getDistanceToEntity(double dX, double dY) = 0;
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the distance between a given point and line
	// REQUIRE:	 valid point and line entities.
	// PROMISE:	returns the distance
	double getDistanceBetweenPointAndLine (Point point, Point lineStpt, Point lineEndPt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to set current related CGenericDisplayCtrl object. This is required to enable UGD to
	// REQUIRE: create multiple instances and to link view and frame of corresponding ctrl object
	// PROMISE: 
	void setGenericDisplayCtrl(CGenericDisplayCtrl* pGenericDisplayCtrl) {m_pGenericDisplayCtrl = pGenericDisplayCtrl;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To assign the Current Page number to the entities page number attribute 
	// REQUIRE: current page number of the loaded image
	// PROMISE: 
	void setPage(unsigned long ulPageNumber);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return the Page number to which this entity belongs to
	// REQUIRE: Nothing
	// PROMISE: returns the Entity page number 
	unsigned long getPage(){ return m_ulPageNumber;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To check the entity's page number with the current page number 
	// REQUIRE: current page number of the loaded image
	// PROMISE: returns 'true' if the entity's page number is equal to the current page number 
	//			otherwise returns 'false'
	bool checkPageNumber();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: 
	// REQUIRE:
	// PROMISE: 
	bool getVisibilityAsInGddFile() { return m_bVisibleAsInGddFile;}

	// PURPOSE: 
	// REQUIRE:
	// PROMISE: 
	bool isVisibilityChangedAfterReadingFromFile() { return m_bVisibilityChanged;}

	// PURPOSE: 
	// REQUIRE:
	// PROMISE: 
	void setVisibilityChangedAfterReadingFromFile(bool bValue) { m_bVisibilityChanged = bValue;}

protected:
	// the ID associated with this object, if any.  If no ID is associated with this object (i.e
	// if the object is not yet added to the list of displayed entities, the ID will be NULL.
	unsigned long m_ulID;

	// all entities in the generic display can be associated with one or more attributes.  This
	// class attribute keeps track of the application-specific attributes associated with each
	// entity.
	EntityAttributes m_attributes;

	// the color used to draw this entity in the GenericDisplay view
	COLORREF m_color;

	// Set if the entity is made visible.It should be initialized to 1.
	bool	m_bVisible;

	bool m_bVisibleAsInGddFile;

	bool m_bVisibilityChanged;

	// TRUE if the entity is selected; FALSE by default.
	BOOL m_bSelected;

	// Variable to store the entity extents. This will be calculated at the time of creating
	// each one of the entites. This will also be returned in the call to getEntityExtents.
	GDRectangle	m_gdEntExtents;

	// the page number on which the entity is to be displayed
	unsigned long m_ulPageNumber;

public:
	virtual ~GenericEntity();

	// have a local variable for the control
	CGenericDisplayCtrl *m_pGenericDisplayCtrl;
};
//==================================================================================================