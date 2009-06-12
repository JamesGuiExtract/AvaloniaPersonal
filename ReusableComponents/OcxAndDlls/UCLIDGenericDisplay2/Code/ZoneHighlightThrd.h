//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ZoneHighlightThrd.h
//
// PURPOSE:	This is an header file for CZoneHighlightThrd class
//			The code written in this file makes it possible for
//			creating the Zone entity interactively. zone entity 
//          drawn throgh this class is not persistant.This class
//          makes user to feel something like rubber banding for 
//          zone entity.
// NOTES:	
//
// AUTHORS:	M.Srinivasa Rao
//
//==================================================================================================
#if !defined(AFX_ZONEHIGHLIGHTTHRD_H__D333A616_6EAD_11D5_820C_00B0D0176808__INCLUDED_)
#define AFX_ZONEHIGHLIGHTTHRD_H__D333A616_6EAD_11D5_820C_00B0D0176808__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

class CGenericDisplayView;
class CGenericDisplayCtrl;
// ZoneHighlightThrd.h : header file
//
//==================================================================================================
//
// CLASS:	CZoneHighlightThrd
//
// PURPOSE:	To create and show a zone entity interactively to highlight rectangular (which are 
//          possibly rotated) regions of the image. The zone entities shown by this class are not
//          persistant. To create persistant zone entities zoneEntity class is to be used. This clas
//          makes the user to feel like rubber band for zone entity. Thrd in the name of the class is
//			misleading here as it does not have anything related with threads.
//
// REQUIRE:	Nothing.
// 
// INVARIANTS:
//			None.
//
// EXTENSIONS:
//			None.
//
// NOTES:	The Zone parameters are all defined in terms of view pixel positions.
/////////////////////////////////////////////////////////////////////////////

class CZoneHighlightThrd 
{

public:
	CZoneHighlightThrd(); 
	~CZoneHighlightThrd();  

	//	GenericDisplayView
	CGenericDisplayView	*m_pGenView;

	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the start point
	// REQUIRE: X and Y coordinate values
	// PROMISE: None
	void GetStartPoint (int& iEX, int& iEY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To set the start point
	// REQUIRE: X and Y coordinate values
	// PROMISE: None
	void SetStartPoint (int iEX, int iEY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the end point
	// REQUIRE: X and Y coordinate values
	// PROMISE: None
	void GetEndPoint (int& iEX, int& iEY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To set the end point
	// REQUIRE: X and Y coordinate values
	// PROMISE: None
	void SetEndPoint (int iEX, int iEY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To set the previous end point. This method is used for deleting the 
	//          the previous region
	// REQUIRE: X and Y coordinate values
	// PROMISE: None
	void SetPrevEndPoint (int iEX, int iEY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the previous end point. This method is used for deleting the 
	//          the previous region.
	// REQUIRE: X and Y coordinate values
	// PROMISE: None
	void GetPrevEndPoint (int& iEX, int& iEY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To draw the zone area as highlighted
	// REQUIRE: SetHighlightHeight() and SetHighlightColor() must have been called
	// PROMISE: updates the highlighted zone and deletes the earlier zone if any
	void Draw ();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To set the zone highlight color
	// REQUIRE: Nothing
	// PROMISE: None
	void SetHighlightColor (COLORREF color);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To set the zone highlight height
	// REQUIRE: Nothing
	// PROMISE: None
	void SetHighlightHeight (long ht);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To calculate the region bounding the zone
	// REQUIRE: Nothing
	// PROMISE: None
	void CalculateRegion(CRgn& region, double endPtX, double endPtY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return the zone highlight color
	// REQUIRE: Nothing
	// PROMISE: None
	COLORREF GetZoneHighlightColor() {return m_zoneColor;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return the existion region for the zone
	// REQUIRE: Nothing
	// PROMISE: None
	CRgn& GetRegion() { return m_zoneRegion; }
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To set the boolean variable that indicates whether highlight height
	//          has changed during the zone creation through + or - keys 
	// REQUIRE: Nothing
	// PROMISE: None
	void notifyHighlightHtChanged(BOOL bValue) { m_bHighltHtChanged = bValue;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to return whether highlight height is changed during interactive zone
	//          highlight process
	// REQUIRE: Nothing
	// PROMISE: None
	BOOL IsHighltHtChanged() { return (m_bHighltHtChanged == TRUE)? TRUE:FALSE;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To reset the thread class variables to a state where they can be reused. Meant to 
	//          be called after a successful interactive highlight creation.
	// REQUIRE: Nothing
	// PROMISE: None
	void stop();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To reset the thread class variables to the state as though the thread event had 
	//          never started. Meant to be called after an unsuccessful interactive highlight 
	//			creation.
	// REQUIRE: Nothing
	// PROMISE: None
	void cancel();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to erase the previously drawn zone highlight. Required to show the 
	//          rubberband effect for highlighting 
	// REQUIRE: Nothing
	// PROMISE: None
	void eraseOldRegion();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to return the zone width. Width is obviously equal to the length of 
	//          line joining the start and end points of the zone
	// REQUIRE: Nothing
	// PROMISE: None
	double getZoneWidth();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to calculate and return the region for the height passed. This method 
	//          is required to see that the zone highlight is well within the image bounds.
	//          before updating the zone with this new height
	// REQUIRE: Nothing
	// PROMISE: None
	CPoint* getZoneBoundsForThisHeight(long lHeight);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to calculate and return the region for the end point passed. This method 
	//          is required to see that the zone highlight is well within the image bounds.
	//			before updating with this new end point
	// REQUIRE: Nothing
	// PROMISE: None
	CPoint* getZoneBoundsForThisEndPoint(int x, int y);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to calculate and return the region for the parameters passed. This method 
	//          is required to see that the zone highlight is well within the image bounds.
	//			before updating with this new end point and zone height.
	// REQUIRE: Nothing
	// PROMISE: None
	CPoint* getZoneBounds(double x1, double y1, double x2, double y2, long lHeight);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to enable interactive zone entity creation.
	// REQUIRE: Nothing
	// PROMISE: None
	void enableInteractiveZoneEntCreation (bool bValue) {m_bzoneCreationEnabled = bValue;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to return status of the interactive zone entity creation
	// REQUIRE: Nothing
	// PROMISE: None
	bool IsInteractiveZoneEntCreationEnabled () {return m_bzoneCreationEnabled;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to set Mouse moved after interactive zone initialization (Lbutton down).
	// REQUIRE: Nothing
	// PROMISE: None
	void setMouseMovedAfterZoneInitialization (bool bValue) {m_bMouseMovedAfterZoneInitialization = bValue;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to return whether mouse is moved after zone initialization
	// REQUIRE: Nothing
	// PROMISE: None
	bool MouseMovedAfterZoneInitialization () {return m_bMouseMovedAfterZoneInitialization;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to set current related CGenericDisplayCtrl object. This is required to enable UGD to
	// REQUIRE: create multiple instances and to link view and frame of corresponding ctrl object
	// PROMISE: 
	void setGenericDisplayCtrl(CGenericDisplayCtrl* pGenericDisplayCtrl) {m_pGenericDisplayCtrl = pGenericDisplayCtrl;}
	//----------------------------------------------------------------------------------------------


private:

	// start point X co-ordinate
	long m_lstartX;

	// start point Y co-ordinate
	long m_lstartY;

	// end point X co-ordinate
	long m_lendPtX;

	// end point Y co-ordinate
	long m_lendPtY;

	// prev end point X co-ordinate
	long m_lprevEndPtX;

	// prev end point Y co-ordinate
	long m_lprevEndPtY;

	// zone highlight height. this height and previous height are in world coordinates
	long m_lzoneHeight;

	// previous zone highlight height. needs to erase previous highlight zone
	// when user uses + or - key to increase or decrease the height of the highlight zone
	long m_lprevZoneHeight;

	// color of the highlight zone
	COLORREF m_zoneColor;

	// zone region
	CRgn m_zoneRegion;

	// indicates whether highlight height changed during zone creation
	BOOL m_bHighltHtChanged;

	// stores the zone bounding points
	CPoint  zoneBoundPts[4];

	// indicates whether interactive zone creation enabled or not
	bool m_bzoneCreationEnabled;

	// basically required to check whether mouse moved after mouse is down 
	// and before mouse is up
	bool m_bMouseMovedAfterZoneInitialization;

	// have a local variable for the control
	CGenericDisplayCtrl *m_pGenericDisplayCtrl;
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_ZONEHIGHLIGHTTHRD_H__D333A616_6EAD_11D5_820C_00B0D0176808__INCLUDED_)
