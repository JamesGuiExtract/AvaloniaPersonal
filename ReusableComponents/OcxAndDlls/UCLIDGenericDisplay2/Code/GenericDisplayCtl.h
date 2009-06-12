//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericDisplayCtl.h
//
// PURPOSE:	The purpose of this file is to implement the interface that needs to be supported by
//			the GenericDisplay ActiveX control.  This is an header file for GenericDisplayCtl 
//			where this class has been derived from the CActiveXDocControl()class.  
//			The code written in this file makes it possible to implement the various methods.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#pragma once

#include "Activdoc.h"
#include "imageedit.h"
#include "ZoneEntity.h"

#include <afxtempl.h>
#include <string>
#include <map>

using namespace std;

class CGenericDisplayView;
class CGenericDisplayFrame;

//	to store the zoom push values
typedef struct ZOOM
{
	double		dZoomFactor;
	long		lScrollX;
	long		lScrollY;

} ZOOMPARAM;

// size of the zoom stack
const int ZOOMSTACKSZ = 50;

// GenericDisplayCtl.h : Declaration of the CGenericDisplayCtrl ActiveX Control class.

/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayCtrl : See GenericDisplayCtl.cpp for implementation.

class CGenericDisplayDocument;
class LineEntity;
class SpecializedCircleEntity;
class TextEntity;
class CurveEntity;
class Point;
class CGenericDisplayFrame;
class GenericEntity;

//==================================================================================================
//
// CLASS:	GenericDisplay
//
// PURPOSE:	To provide all the functionality of a simple and generic CAD-type display window.
//
// REQUIRE:	Nothing.
// 
// INVARIANTS:
//			The scale associated with the current GenericDisplay view is always displayed in
//			the scale indicator status control whenever a background image has been specified.
//			The last position of the mouse cursor within the GenericDisplay view is always
//			displayed in the X/Y position indicator status control whenever a background imgae has
//			been specified.
//			If the user clicks within the GenericDisplay view with the left mouse button while
//			the CTRL button is pressed and the SHIFT or ALT keys are not pressed, the control
//			will automatically zoom in around the clicked point by a programmable zoom factor.
//			If the user clicks within the GenericDisplay view with the left mouse button while
//			the SHIFT button is pressed and the CTRL or ALT keys are not pressed, the control
//			will automatically zoom out around the clicked point by the same programmable zoom
//			factor.
//			If the user clicks within the GenericDisplay view with the left mouse button while
//			the ALT button is pressed and the CTRL or SHIFT keys are not pressed, the control
//			will automatically keep the mouse cursor snapped to the nearest endpoint
//			of a line or curve entity.
//
// EXTENSIONS:
//			None.
//
// NOTES:	In the specifications below the cursor being regarded as the "SelectTextCursor" is 
//			expected to be loaded from the Windows cursor file "SelectText.cur", which will be
//			in the current directory.  Likewise, "SelectPointCursor" will be loaded from the 
//			"SelectPoint.cur" file, expected to also be in the current directory.
//
//			The file format for "GenericDisplayData (GDD)" is specified in GenericDisplayData.txt.
//
class CGenericDisplayCtrl : public CActiveXDocControl
{
	DECLARE_DYNCREATE(CGenericDisplayCtrl)

public:
	//	Constructor
	CGenericDisplayCtrl();	

public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CGenericDisplayCtrl)
	public:
	virtual void DoPropExchange(CPropExchange* pPX);
	virtual void OnResetState();
	//}}AFX_VIRTUAL

// Implementation
protected:
	~CGenericDisplayCtrl();

	//	to get the current units
	int getCurrentUnits() 
	{
		return m_iCurrentUnits; 
	}

	DECLARE_OLECREATE_EX(CGenericDisplayCtrl)    // Class factory and guid
	DECLARE_OLETYPELIB(CGenericDisplayCtrl)      // GetTypeInfo
	DECLARE_PROPPAGEIDS(CGenericDisplayCtrl)     // Property page IDs
	DECLARE_OLECTLTYPE(CGenericDisplayCtrl)		// Type name and misc status

// Message maps
	//{{AFX_MSG(CGenericDisplayCtrl)
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnDestroy();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
public:
// Dispatch maps
	//{{AFX_DISPATCH(CGenericDisplayCtrl)
	afx_msg void setDefaultUnits(LPCTSTR zDefaultUnits);
	afx_msg void zoomPush();
	afx_msg void zoomPop();
	afx_msg void zoomExtents();
	afx_msg void zoomCenter(double dCenterX, double dCenterY);
	afx_msg void zoomWindow(double dBottomLeftX, double dBottomLeftY, 
		double dTopRightX, double dTopRightY, long FAR* pnFitStatus);
	afx_msg void setImage(LPCTSTR zImageFileName);
	afx_msg void setScaleFactor(double dScaleFactor);
	afx_msg double getScaleFactor();
	afx_msg void setStatusBarText(LPCTSTR zStatusBarText);
	afx_msg void setRubberbandingParameters(long eRubberbandType, double dStartPointX, double dStartPointY);
	afx_msg void enableRubberbanding(BOOL bValue);
	afx_msg void deleteEntity(long ulEntityID);
	afx_msg void clear();
	afx_msg BSTR getEntityAttributes(long ulEntityID);
	afx_msg void modifyEntityAttribute(long ulEntityID, LPCTSTR zAttributeName, LPCTSTR zNewValue);
	afx_msg void addEntityAttribute(long ulEntityID, LPCTSTR zAttributeName, LPCTSTR zValue);
	afx_msg void deleteEntityAttribute(long ulEntityID, LPCTSTR zAttributeName);
	afx_msg OLE_COLOR getEntityColor(long ulEntityID);
	afx_msg void setEntityColor(long ulEntityID, OLE_COLOR newColor);
	afx_msg BSTR getEntityType(long ulEntityID);
	afx_msg void moveEntityBy(long ulEntityID, double dX, double dY);
	afx_msg long getEntityNearestTo(double dX, double dY, double FAR* rDistance);
	afx_msg void selectEntity(long ulEntityID, BOOL bSelect);
	afx_msg void selectAllEntities(BOOL bSelect);
	afx_msg BSTR getSelectedEntities();
	afx_msg void setEntityText(long ulTextEntityID, LPCTSTR zNewText);
	afx_msg BSTR queryEntities(LPCTSTR zQuery);
	afx_msg BSTR getEntityText(long ulTextEntityID);
	afx_msg void getCurrentViewExtents(double FAR* rdBottomLeftX, double FAR* rdBottomLeftY, double FAR* rdTopRightX, double FAR* rdTopRightY);
	afx_msg void getEntityExtents(long ulEntityID, double FAR* rdBottomLeftX, double FAR* rdBottomLeftY, double FAR* rdTopRightX, double FAR* rdTopRightY);
	afx_msg BSTR getDefaultUnits();
	afx_msg BSTR getImageName();
	afx_msg BSTR getStatusBarText();
	afx_msg BOOL isRubberbandingEnabled();
	afx_msg BOOL getRubberbandingParameters(long FAR* bRubberbandWithLine, double FAR* dStartPointX, double FAR* dStartPointY);
	afx_msg void convertWorldToImagePixelCoords(double dWorldX, double dWorldY, long FAR* rulImagePixelPosX, long FAR* rulImagePixelPosY, long nPageNum);
	afx_msg void convertImagePixelToWorldCoords(long ulImagePixelPosX, long ulImagePixelPosY, double FAR* rdWorldX, double FAR* rdWorldY, long nPageNum);
	afx_msg void getImageExtents(double FAR* dWidth, double FAR* dHeight);
	afx_msg void setZoomFactor(double dZoomFactor);
	afx_msg double getZoomFactor();
	afx_msg void zoomIn(long FAR* pnFitStatus);
	afx_msg void zoomOut(long FAR* pnFitStatus);
	afx_msg void enableEntitySelection(BOOL bEnable);
	afx_msg BOOL isEntitySelectionEnabled();
	afx_msg void enableZoneEntityCreation(BOOL bEnable);
	afx_msg BOOL isZoneEntityCreationEnabled();
	afx_msg void getZoneEntityParameters(long nZoneEntityID, long FAR* rulStartPosX, long FAR* rulStartPosY, long FAR* rulEndPosX, long FAR* rulEndPosY, long FAR* rulTotalZoneHeight);
	afx_msg void setMinZoneWidth(long ulMinZoneWidth);
	afx_msg unsigned long getMinZoneWidth();
	afx_msg void setZoneHighlightHeight(long ulNewHeight);
	afx_msg long getZoneHighlightHeight();
	afx_msg void setZoneHighlightColor(OLE_COLOR newColor);
	afx_msg OLE_COLOR getZoneHighlightColor();
	afx_msg void convertWorldToClientWindowPixelCoords(double dWorldX, double dWorldY, long FAR* rulClientWndPosX, long FAR* rulClientWndPosY, long nPageNum);
	afx_msg void convertClientWindowPixelToWorldCoords(long ulClientWndPosX, long ulClientWndPosY, double FAR* rdWorldX, double FAR* rdWorldY, long nPageNum);
	afx_msg void zoomInAroundPoint(double dX, double dY);
	afx_msg void zoomOutAroundPoint(double dX, double dY);
	afx_msg void setXYTrackingOption(long iTrackingOption);
	afx_msg BOOL documentIsModified();
	afx_msg long getCurrentPageNumber();
	afx_msg void setCurrentPageNumber(long ulPageNumber);
	afx_msg long getTotalPages();
	afx_msg BSTR getEntityAttributeValue(long ulEntityID, LPCTSTR zAttributeName);
	afx_msg BSTR getEntityInfo(long nEntityID);
	afx_msg long addCurveEntity(double dCenterX, double dCenterY, double dRadius, double dStartAngle, double dEndAngle, long nPageNum);
	afx_msg long addLineEntity(double dStartPointX, double dStartPointY, double dEndPointX, double dEndPointY, long nPageNum);
	afx_msg long addSpecializedCircleEntity(double dCenterX, double dCenterY, long nPercentRadius, long nPageNum);
	afx_msg long addTextEntity(double dInsertionPointX, double dInsertionPointY, LPCTSTR zText, short ucAlignment, double dRotationInDegrees, double dHeight, LPCTSTR zFontName, long nPageNum);
	afx_msg long addZoneEntity(long ulStartPosX, long ulStartPosY, long ulEndPosX, long ulEndPosY, long ulTotalZoneHeight, long nPageNum, BOOL bFill, BOOL bBorderVisible, COLORREF borderColor);
	afx_msg double getBaseRotation(long nPageNum);
	afx_msg void setBaseRotation(long nPageNum, double dBaseRotation);
	afx_msg void setDocumentModified(BOOL bModified);
	afx_msg void setCursorHandle(OLE_HANDLE* hCursor);
	afx_msg void extractZoneEntityImage(long nZoneEntityID, LPCTSTR zFileName);
	afx_msg void extractZoneImage(long nX1, long nY1, long nX2, long nY2, long nHeight, long nPageNum, LPCTSTR zFileName);
	afx_msg void getCurrentPageSize(long *pWidth, long *pHeight);
	afx_msg BOOL isZoneBorderVisible(long ulZoneEntityID);
	afx_msg void showZoneBorder(long ulZoneEntityID, BOOL bShow);
	afx_msg OLE_COLOR getZoneBorderColor(long ulZoneEntityID);
	afx_msg void setZoneBorderColor(long ulZoneEntityID, OLE_COLOR newColor);
	afx_msg BOOL isZoneFilled(long ulZoneEntityID);
	afx_msg void setZoneFilled(long ulZoneEntityID, BOOL bFilled);
	afx_msg void setZoneEntityCreationType(long eType);
	afx_msg long getZoneEntityCreationType();
	afx_msg void setZoom(double dZoom);
	afx_msg double getZoom();
	afx_msg long getZoneBorderStyle(long ulZoneEntityID);
	afx_msg void setZoneBorderStyle(long ulZoneEntityID, long nStyle);
	afx_msg BOOL isZoneBorderSpecial(long ulZoneEntityID);
	afx_msg void setZoneBorderSpecial(long ulZoneEntityID, BOOL bSpecial);
	afx_msg long getEntityPage(long ulEntityID);
	afx_msg void zoomFitToWidth();
	afx_msg void enableDisplayPercentage(BOOL bEnable);
	afx_msg BOOL isZoneEntityAdjustmentEnabled();
	afx_msg void enableZoneEntityAdjustment(BOOL bEnable);
	afx_msg void setFittingMode(long eFittingMode);
	afx_msg long getFittingMode();
	afx_msg void setSelectionColor(long lColor);
	afx_msg long getSelectionColor();
	//}}AFX_DISPATCH
	DECLARE_DISPATCH_MAP()

	afx_msg void AboutBox();
public:
// Event maps
	//{{AFX_EVENT(CGenericDisplayCtrl)
	void FireEntitySelected(long ulEntityID)
		{FireEvent(eventidEntitySelected,EVENT_PARAM(VTS_I4), ulEntityID);}
	void FireMouseWheel(long wParam, long lParam)
		{FireEvent(eventidMouseWheel,EVENT_PARAM(VTS_I4  VTS_I4), wParam, lParam);}
	void FireZoneEntityMoved(long ulEntityID)
		{FireEvent(eventidZoneEntityMoved,EVENT_PARAM(VTS_I4), ulEntityID);}
	void FireZoneEntitiesCreated(IUnknown* pZoneIDs)
		{FireEvent(eventidZoneEntitiesCreated,EVENT_PARAM(VTS_UNKNOWN), pZoneIDs);}
	//}}AFX_EVENT
	DECLARE_EVENT_MAP()

// Dispatch and event IDs
public:
	enum {
	//{{AFX_DISP_ID(CGenericDisplayCtrl)
	dispidSetDefaultUnits = 1L,
	dispidZoomPush = 2L,
	dispidZoomPop = 3L,
	dispidZoomExtents = 4L,
	dispidZoomCenter = 5L,
	dispidZoomWindow = 6L,
	dispidSetImage = 7L,
	dispidSetScaleFactor = 8L,
	dispidGetScaleFactor = 9L,
	dispidSetStatusBarText = 10L,
	dispidSetRubberbandingParameters = 11L,
	dispidEnableRubberbanding = 12L,
	dispidDeleteEntity = 13L,
	dispidClear = 14L,
	dispidGetEntityAttributes = 15L,
	dispidModifyEntityAttribute = 16L,
	dispidAddEntityAttribute = 17L,
	dispidDeleteEntityAttribute = 18L,
	dispidGetEntityColor = 19L,
	dispidSetEntityColor = 20L,
	dispidGetEntityType = 21L,
	dispidMoveEntityBy = 22L,
	dispidGetEntityNearestTo = 23L,
	dispidSelectEntity = 24L,
	dispidSelectAllEntities = 25L,
	dispidGetSelectedEntities = 26L,
	dispidSetEntityText = 27L,
	dispidQueryEntities = 28L,
	dispidGetEntityText = 29L,
	dispidGetCurrentViewExtents = 30L,
	dispidGetEntityExtents = 31L,
	dispidGetDefaultUnits = 32L,
	dispidGetImageName = 33L,
	dispidGetStatusBarText = 34L,
	dispidIsRubberbandingEnabled = 35L,
	dispidGetRubberbandingParameters = 36L,
	dispidConvertWorldToImagePixelCoords = 37L,
	dispidConvertImagePixelToWorldCoords = 38L,
	dispidGetImageExtents = 39L,
	dispidSetZoomFactor = 40L,
	dispidGetZoomFactor = 41L,
	dispidZoomIn = 42L,
	dispidZoomOut = 43L,
	dispidEnableEntitySelection = 44L,
	dispidIsEntitySelectionEnabled = 45L,
	dispidEnableZoneEntityCreation = 46L,
	dispidIsZoneEntityCreationEnabled = 47L,
	dispidGetZoneEntityParameters = 48L,
	dispidSetMinZoneWidth = 49L,
	dispidGetMinZoneWidth = 50L,
	dispidSetZoneHighlightHeight = 51L,
	dispidGetZoneHighlightHeight = 52L,
	dispidSetZoneHighlightColor = 53L,
	dispidGetZoneHighlightColor = 54L,
	dispidConvertWorldToClientWindowPixelCoords = 55L,
	dispidConvertClientWindowPixelToWorldCoords = 56L,
	dispidZoomInAroundPoint = 57L,
	dispidZoomOutAroundPoint = 58L,
	dispidSetXYTrackingOption = 59L,
	dispidDocumentIsModified = 60L,
	dispidGetCurrentPageNumber = 61L,
	dispidSetCurrentPageNumber = 62L,
	dispidGetTotalPages = 63L,
	dispidGetEntityAttributeValue = 64L,
	dispidGetEntityInfo = 65L,
	dispidAddCurveEntity = 66L,
	dispidAddLineEntity = 67L,
	dispidAddSpecializedCircleEntity = 68L,
	dispidAddTextEntity = 69L,
	dispidAddZoneEntity = 70L,
	dispidGetBaseRotation = 71L,
	dispidSetBaseRotation = 72L,
	dispidSetDocumentModified = 73L,
	dispidSetCursorHandle = 74L,
	dispidExtractZoneEntityImage = 75L,
	dispidExtractZoneImage = 76L,
	dispidGetCurrentPageSize = 77L,
	dispidIsZoneFilled = 78L,
	dispidSetZoneFilled = 79L,
	dispidIsZoneTransparentInside = 80L,
	dispidSetZoneTransparentInside = 81L,
	dispidZoomFitToWidth = 93L,
	dispidEnableDisplayPercentage= 94L,
	dispidIsZoneEntityAdjustmentEnabled = 95L,
	dispidEnableZoneEntityAdjustment = 96L,
	eventidEntitySelected = 1L,
	eventidMouseWheel = 3L,
	eventidZoneEntityMoved = 4L,
	eventidZoneEntitiesCreated = 5L,
	//}}AFX_DISP_ID
	};

	//----------------------------------------------------------
	// PURPOSE: To get the image name and path
	// REQUIRE: Nothing.
	// PROMISE: None
	CString GetImage ()	{return m_zBackImageName;}
	//----------------------------------------------------------
	// PURPOSE: To set the image name and path
	// REQUIRE: Nothing.
	// PROMISE: None
	void setBackImageName(CString zBackImageName){m_zBackImageName = zBackImageName;}
	//----------------------------------------------------------
	// PURPOSE: to set the image resolution
	// REQUIRE: Resolution values should be valid
	// PROMISE: sets the image resolution with the given values
	void setImageResolution (int iXRes, int iYRes) {m_iXRes = iXRes; m_iYRes = iYRes;}
	//----------------------------------------------------------
	// PURPOSE: to retrive the image resolution in the passed parameters
	// REQUIRE: Pointers receiving the resolutions should not be null
	// PROMISE: retrieves the values into the passed parameters
	void getImageResolution (int *iXRes, int *iYRes) {*iXRes = m_iXRes; *iYRes = m_iYRes;}
	//----------------------------------------------------------
	// PURPOSE: to update the multiplication factors depending upon the scale or units
	// REQUIRE: Nothing
	// PROMISE: update the multiplication factors for both width and height of the image
	void updateMultFactors ();
	//----------------------------------------------------------
	// PURPOSE: to return the current X multiplication factor
	// REQUIRE: Nothing 
	// PROMISE: returns the current multiplication factor for X direction
	double getXMultFactor () {return m_dXMultFactor;}
	//----------------------------------------------------------
	// PURPOSE: to return the current Y multiplication factor
	// REQUIRE: Nothing
	// PROMISE: returns the current multiplication factor for Y direction
	double getYMultFactor () {return m_dYMultFactor;}
	//----------------------------------------------------------
	// PURPOSE: To initialize the rubberbanding parameters
	// REQUIRE: Nothing
	// PROMISE: initializes the rubber banding parameters
	void initRubberbandingParameters();
	//----------------------------------------------------------
	// PURPOSE: To move the cursor to the nearest line or curve entity.This function
	//			in turn calls MoveCursorToStartOrEndPoint() to move the cursor to 
	//          the start or end point of the nearest entity
	// REQUIRE: Nothing.
	// PROMISE: Moves the cursor if the entity is a line or curve entity
	void MoveCursorToNearestEntity();
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To move the cursor to the nearest start or end point of line 
	//          or curve entity
	// REQUIRE: Nothing.
	// PROMISE: Compares the start and end point of the nearest entity and moves
	//          the cursor to the nearest among them
	void MoveCursorToStartOrEndPoint (GenericEntity *pEnt, POINT& CurPoint);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To get the ID of the nearest Line or Curve entity to the given point
	//          This method is basically called to move the cursor when ALT key and left
	//          mouse button is clicked
	// REQUIRE: Nothing.
	// PROMISE: gets the ID of the nearest line or curve entity
	//          
	long getNearestLineOrCurveEntity(double dX, double dY);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To validate the zone height passed. It should not be less than 5 
	// REQUIRE: Nothing.
	// PROMISE: validates the zone hight passed
	bool validateMinZoneHeightToCreate(long ulTotalZoneHeight);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To validate the zone height passed. It should be an odd number
	// REQUIRE: Nothing.
	// PROMISE: validates the zone hight passed
	bool validateOddNumberZoneHeightToCreate(long ulTotalZoneHeight);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To validate the zone width for the zone to create. It should not be less than the
	//          minimum zone width returned from getMinZoneWidth()
	// REQUIRE: Nothing.
	// PROMISE: validates the zone width calculated from the start and end points passed
	bool validateZoneWidthToCreate(unsigned long ulStPosX, unsigned long ulStPosY, unsigned long ulEndPosX, unsigned long ulEndPosY);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To return the nearest entity to the point passed in world coordiantes
	// REQUIRE: Nothing.
	// PROMISE: returns valid ID if enities exist or any of the entities are quite nearer to the point 
	//          otherwise returns zero.
	long getNearestEntityIDFromPoint(double dX, double dY);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return the grip handle that appears at the specified world coordinates, or 
	//          kNone if the point is not over a grip handle.
	EGripHandle getGripHandle(double dX, double dY);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return the zero-based grip handle id that appears at the specified world 
	//          coordinates, or -1 if the point is not over a grip handle. If the id returned is 
	//          not -1, also sets pZoneEntity to the zone entity to which the handle corresponds.
	// PROMISE: pZoneEntity will be set to the zone entity with the selected grip handle. If the 
	//          point is not over a grip handle, pZoneEntity will be unchanged.
	int getGripHandleId(double dX, double dY, ZoneEntity** ppZoneEntity);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: notifies whether scale factor has changed
	// REQUIRE: Nothing.
	// PROMISE: sets m_bScaleFactorChanged to true or false 
	void notifyScaleFactorChanged(bool bValue) {m_bScaleFactorChanged = bValue;}
	//--------------------------------------------------------------------------------------------
	// PURPOSE: to return whether scale factor has changed or not
	// REQUIRE: nothing
	// PROMISE: returns true if scale factor has changed after last call to 
	//          notifyScaleFactorChanged(FALSE)
	bool isScaleFactorChanged() { return m_bScaleFactorChanged;}
	//--------------------------------------------------------------------------------------------
	// PURPOSE: to check whether image is currently loaded into the control
	// REQUIRE: nothing
	// PROMISE: raises an exception if image is not loaded 
	void checkForImageLoading(CString zFunctionName);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: to return the zone highlight color in XOR mode.
	// REQUIRE: nothing
	// PROMISE: returns the zone highlight color in Exclusive OR mode, so that
	//          that zone will be shown in the user set color
	COLORREF getZoneColorInXORMode() {return m_colorZoneHighlight;}
	//--------------------------------------------------------------------------------------------
	// PURPOSE: validates the entity ID and returns the entity pointer if 
	//          the entity ID is valid
	// REQUIRE: nothing
	// PROMISE: raises an exception if the entity ID is not valid
	void validateEntityID (long ulEntityID, GenericEntity*& pGenEnt);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To get the XY Tracking option 
	// REQUIRE: nothing
	// PROMISE: returns the XY tracking option 
	unsigned long getXYTrackingOption();
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To enable/disable display percentage option 
	// REQUIRE: nothing
	// PROMISE: returns whether percentage should be displayed
	BOOL getDisplayPercentage();
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To get current related UGDFrame object
	// REQUIRE:	
	// PROMISE:
	CGenericDisplayFrame* getUGDFrame(){return m_pUGDFrame;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get current related UGDView object
	// REQUIRE:	
	// PROMISE:
	CGenericDisplayView* getUGDView(){return m_pUGDView;}
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To get the Cursor File name and path
	// REQUIRE: Nothing.
	// PROMISE: None
	CString GetCursorFileName()	{return m_zCursorFileName;}
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To get the XOR color for the given color
	// REQUIRE: Nothing.
	// PROMISE: returns the XOR color for the given color. For example, if the given 
	//			color is 0x000000 ie WHITE, the returned color will be 0xffffff ie RED
	COLORREF GetXORColor(COLORREF color);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To validate the Curve angles euqality
	// REQUIRE: Nothing.
	// PROMISE: validates the Curve angles
	bool validateCurveAnglesEquality(double dStartAngle, double dEndAngle);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To validate the Curve angles passed.
	// REQUIRE: Nothing.
	// PROMISE: validates the Curve angles
	bool validateCurveAnglesToCreate(double dStartAngle, double dEndAngle);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To validate the Curve radius for the Curve to create. It should not be less than 0
	// REQUIRE: Nothing.
	// PROMISE: validates the radius 
	bool validateCurveRadiusToCreate(double dRadius);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To validate the Text Alignment.
	// REQUIRE: Nothing.
	// PROMISE: validates the Text Alignment
	bool validateTextAlignmentAnglesToCreate(short ucAlignment);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To validate the Text Height
	// REQUIRE: Nothing.
	// PROMISE: validates the Text Height
	bool validateTextHeightToCreate(double dHeight);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To validate specialized circle percent radius 
	// REQUIRE: Nothing.
	// PROMISE: validates the specialized circle percent radius 
	bool validateSpCirclePercentRadiusToCreate(long iPercentRadius);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To validate the Base Rotation
	// REQUIRE: Nothing.
	// PROMISE: validates the Base Rotation
	bool validateBaseRotationToSet(double dBaseRotation);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To validate the Scale Factor
	// REQUIRE: Nothing.
	// PROMISE: validates the Scale Factor
	bool validateScaleFactorToSet(double dScaleFactor);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To Handle all the exceptions
	// REQUIRE: Nothing.
	// PROMISE: handles all the exceptions 
	void handleAllFileExceptions(CString zStrBuffer);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: To Validate the given font name
	// REQUIRE: Nothing.
	// PROMISE: validates the fontname and raises exception if the the name is invalid or the
	//			font is currently not installed
	bool ValidateFontName(CString zFontName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To read the COLORREF value
	// REQUIRE: string from GDD file.
	// PROMISE: To get the COLORREF value given in the string
	COLORREF getColorRefValue(CString zReadString);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To Draw the Entities
	// REQUIRE: None
	// PROMISE: To draw the entities when parameters changed
	void DrawEntities (BOOL bDraw);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the entity
	// REQUIRE: ulEntityID should be in the list of the current control
	// PROMISE: To get the Entity for the given ID
	GenericEntity *getEntityByID (long ulEntityID);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to format absolute path of image file from relative path saved in gdd file
	// REQUIRE: open must have been called
	// PROMISE: formats the relative path based on relative path w.r.t. gdd file path
	void updateImageParameters();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Retrieves the entity with the specified id.
	GenericEntity* getEntity(unsigned long ulEntityID);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Restores the current cursor handle to the current default.
	void restoreCursorHandle();

private:
	//----------------------------------------------------------------------------------------------
	// Helper functions
	//----------------------------------------------------------------------------------------------
	void deleteAllEntities();
	//----------------------------------------------------------------------------------------------
	
	// Image name to be used for displaying as background
	CString m_zBackImageName;

	// Image resolution for the loaded image. This is used to compute the
	//  scale factor
	int		m_iXRes;
	int		m_iYRes;

	// Multiplication factors computed from Image resolution, scale factor and units
	// These will be used while displaying the coordinates, returning the coordinates
	// while getting the coordinates for adding entities.
	// Updated in setUnits, LoadImage, setScaleFactor
	double	m_dXMultFactor;
	double	m_dYMultFactor;

	// Scale Factor for displaying the Image
	double m_dScaleFactor;

	// Base rotation angle for displaying the image
	map<long, double> m_mapPageNumToBaseRotation;

	// Global Value for Entity ID
	unsigned long m_ulLastUsedEntityID;

	//	Stack
	ZOOMPARAM m_stkZoomParam[ZOOMSTACKSZ];

	//	stack size
	int m_iZoomStackSize;

	//	Image relative path
	CString m_zImageRelativePath;

	// BOOL value for checking whether the document is modified
	BOOL m_bDocumentModified;
	
	//	current units. Defined as Integer.
	//	1 - Feet
	//	2 - Meters
	int m_iCurrentUnits;

	// $The ActiveX control internally stores and operates on all entities in a nice object-oriented
	// fashion.  The ActiveX control maintains the following map and adds/deletes from the map
	// as necessary.  The ActiveX control will delete any GenericEntity objects before removing the
	// objects from this internal map.  For all incoming queries/modification function calls
	// regarding entities, the ActiveX control finds the GenericEntity that corresponds to the 
	// specified entity ID, performs the operation on that object, refreshes the screen as necessary,
	// and returns any necessary values to the caller using one of the standard ActiveX return types.
	std::map<unsigned long, GenericEntity *> mapIDToEntity;
	
	// as per GD enhancement specs, the OCX control should have a variable for setting the
	// status bar text and SetStatusBarText() should not be called by this OCX internally
	// and it is the responsibility of the container application.
	CString m_zStatusBarText;

	// indicates whether entity selection is enabled or not
	BOOL m_bEntitySelection;

	// Indicated whether zone entity adjustment is enabled or not
	BOOL m_bEntityAdjustment;

	// indicates whether zone entity creation is enabled or not
	BOOL m_bEnableZoneCreation;

	// Indicates whether highlights are movable or resizable when the highlight tool is used.
	BOOL m_bHighlightsAdjustable;

	// the value of minimum zone width set
	unsigned long m_ulMinZoneWidth;

	// the height of the zone highlight
	unsigned long m_ulZoneHighlightHt;
	
	// the color of highlighting zone
	COLORREF  m_colorZoneHighlight;

	// The color of the border to indicate selection
	COLORREF m_colorSelectionBorder;

	// indicates whether scale factor has changed 
	bool m_bScaleFactorChanged;

	// indicates maximum distance between left click and entity that is allowed to
	// select entities defined in pixels 
	double m_dMaxDistForEntSel;

	// to store the XYtracking option 
	unsigned long m_iTrackingOption;

	// indicate whether XY percentage should be displayed 
	BOOL m_bDisplayPercentage;

	// added following pointers to enable UGD multiple instances
	CGenericDisplayFrame*  m_pUGDFrame;

	CGenericDisplayView*  m_pUGDView;

	// The current default mouse cursor
	// Note: That the actual cursor may differ from this value for a temporarily displayed cursor,
	// for instance when a wait cursor is shown or during an interactive mouse event.
	HCURSOR m_hCursor;

	HICON m_hIcon;

	//	currently displayed page number
	long m_lCurPageNumber;

	// the total no. of pages in the loaded image
	unsigned long m_ulTotalPages;

	// Cursor File name
	CString m_zCursorFileName;

	long m_eZoneEntityCreationType;

	// Indicates under what circumstances manual redactions are auto-fitted
	long m_eFittingMode;
};