//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericDisplayView.h
//
// PURPOSE:	This is an header file for CGenericDisplayView class
//			where this has been derived from the CView()
//			class.  The code written in this file makes it possible for
//			initialize to set the view.
// NOTES:	
//
// AUTHORS:	
//
//-------------------------------------------------------------------------------------------------
#pragma once

// GenericDisplayView.h : header file
//
#include "imageedit.h"
#include "RubberBandThrd.h"
#include "ZoneHighlightThrd.h"
#include "ZoneAdjustmentThrd.h"
#include "Point.h"

class CGenericDisplayCtrl;
class LeadToolsBitmap;

//-------------------------------------------------------------------------------------------------
// CGenericDisplayView view
//-------------------------------------------------------------------------------------------------
//
// CLASS:	CGenericDisplayView
//
// PURPOSE:	This class is used to derive View from MFC class CView.
//			This derived view is attached to Frame.  Object of this class is 
//			created in GenericDisplay.cpp.  It has implementation of functions to the 
//			text files.  This class is used to set link between the user and control.
// REQUIRE:	Nothing
// 
// INVARIANTS:
//			
// EXTENSIONS:	Nothing
//			
//
// NOTES:	
//
//-------------------------------------------------------------------------------------------------
class CGenericDisplayView : public CView
{
protected:
	CGenericDisplayView();           // protected constructor used by dynamic creation
	DECLARE_DYNCREATE(CGenericDisplayView)

public:

	//	image control
	CImageEdit m_ImgEdit;

// Operations

	//---------------------------------------------------------------------------------------------
	// PURPOSE: To create image control
	// REQUIRE: Nothing.
	// PROMISE: To create the image control on the view.
	BOOL CreateImageCtrl();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To Load the image
	// REQUIRE: Nothing.
	// PROMISE: To set the image in the image control, to set the properties
	//			and to display the image.
	void LoadImage();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To Check the image type is valid or not
	// REQUIRE: Image type
	// PROMISE: To check the given image is valid to this control or not. 
	//			when the image valid type sends TRUE otherwise FALSE
	BOOL CheckValidImageType (CString );
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To convert the coordinates from View to Image
	// REQUIRE: X and Y coordinate values
	// PROMISE: To convert the given X and Y coordinate values from
	//			View co ordinates to Image coordinates.  In View coordinates
	//			origin will be at the TOPLEFT corner of the control, whereas
	//			in the Image the coordinates should be at BOTTOMLEFT corner of the Image.
	//			for drawing purpose we will use View coordinates and for the 
	//			display purpose will show the image coordinates
	void ViewToImageInWorldCoordinates (int *iX, int *iY, long nPageNum);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To convert the coordinates from View to Image
	// REQUIRE: X and Y coordinate values
	// PROMISE: To convert the given X and Y coordinate values from
	//			View co ordinates to Image coordinates.  In View coordinates
	//			origin will be at the TOPLEFT corner of the control, whereas
	//			in the Image world coordinates should be at BOTTOMLEFT corner of the Image.
	//			for drawing purpose we will use View coordinates and for the 
	//			display purpose will show the image coordinates
	void ViewToImageInWorldCoordinates (double *iX, double *iY, long nPageNum);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To convert the coordinates from Image to View
	// REQUIRE: X and Y coordinate values
	// PROMISE: To convert the given X and Y coordinate values from
	//			Image coordinates to View coordinates.
	void ImageInWorldToViewCoordinates (int *iX, int *iY, long nPageNum);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To convert the coordinates from Image to View
	// REQUIRE: X and Y coordinate values
	// PROMISE: To convert the given X and Y coordinate values from
	//			Image coordinates to View coordinates.
	void ImageInWorldToViewCoordinates (double *iX, double *iY, long nPageNum);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To show the given point as control center
	// REQUIRE: X and Y coordinate values
	// PROMISE: To set the given point to the center of the control
	//			without changing the zoom factor.
	void zoomCenter(double dCenterX, double dCenterY);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To set the cursor type
	// REQUIRE: x and y
	// PROMISE: None
	void setCursorType(int x, int y);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To check whether the cursor point is within image boundaries.
	//			No matter whatever may be the scale factor and zoom factor. The x and y
	//			are view coordinates.
	// REQUIRE: x and y
	// PROMISE: returns TRUE if the given point is within boundaries, otherwise FALSE
	BOOL IsPtInImageBoundaries(int x, int y);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return the loaded image width
	// REQUIRE: image must have been loaded
	// PROMISE: returns the image width
	double getLoadedImageWidth() { return m_iImgWidth;}
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return the loaded image height
	// REQUIRE: Image must have been loaded
	// PROMISE: returns the image height
	double getLoadedImageHeight() { return m_iImgHeight;}
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To zoom in the image
	// REQUIRE: Image must have been loaded
	// PROMISE: zooms in by the set percentage of zooming
	void ZoomIn();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To zoom out the image
	// REQUIRE: Image must have been loaded
	// PROMISE: zooms out by the set percentage of zooming
	void ZoomOut();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To zoom the image to the extents
	// REQUIRE: Image must have been loaded
	// PROMISE: zooms to the extents of the view
	void ZoomExtents();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To zoom the image to fit the width
	// REQUIRE: Image must have been loaded
	// PROMISE: zooms to the width of the view
	void ZoomToWidth();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to return the current zoom factor
	// REQUIRE: 
	// PROMISE: 
	double getZoomFactor ()	{ return m_dCurZoomFactor;}
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To set the zoom factor.
	// REQUIRE: Valid zoom factor
	// PROMISE: To set the given value as zoom factor and to change the zoom
	void setZoomFactor (double dZoomFactor);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to get the RubberBand Thread
	// REQUIRE: 
	// PROMISE: 
	CRubberBandThrd	*getRubberBandThread () { return &m_RBThrd;	}
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To get the interactive zone adjustment thread
	CZoneAdjustmentThrd* getZoneAdjustmentThread () { return &m_zoneAdjustmentThrd; }
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to convert the client window coordinates into image pixel coordinates
	// REQUIRE: 
	// PROMISE: 
	void ClientWindowToImagePixelCoordinates(int *iX, int *iY, long nPageNum);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to convert image pixel coordinates into the client window  coordinates
	// REQUIRE: 
	// PROMISE: 
	void ImagePixelToClientWindowCoordinates(int *iX, int *iY, long nPageNum);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to convert client window coordinates into the image pixel coordinates
	// REQUIRE: parameters as double pointers
	// PROMISE: 
	void ClientWindowToImagePixelCoordinates(double *dX, double *dY, long nPageNum);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to convert image pixel coordinates into the client window  coordinates
	// REQUIRE: parameters as double pointers
	// PROMISE: 
	void ImagePixelToClientWindowCoordinates(double *dX, double *dY, long nPageNum);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to set the percentage of magification that affects zoomIn() and zoomOut()
	// REQUIRE: dperctMagnify must be greater than one. If the value is very less, the effect
	//          may not be there.
	// PROMISE: sets the amount of magnification for zooming functions
	void setPercentChangeInMagnification(double dperctMagnify);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: returns the current percentage of magnification being used in zoomIn and zoomOut
	// REQUIRE: Nothing
	// PROMISE: returns the percentage magnification as percentage
	double getPercentChangeInMagnification(){ return m_dPercentMagnify;}
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to return the rotated image coordinates from the view coordinates
	// REQUIRE: setBaseRoatation >0.0
	// PROMISE: returns the coordinates of rotated image from the view coordinates.
	//          This method is used in calculating the coordinates of the rotated image
	//          for ZoomIn and ZoomOut.
	void ViewToRotatedImageCoordinates(double *dX, double *dY, long nPageNum);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to zoom out by given percercentage around the point passed
	// REQUIRE: valid view point (can not have negative values)
	// PROMISE: zooms out and moves the point passed to the centre of the view.
	//          so that user will feel like zoomed out around the point passed
	void ZoomOutAroundTheClickedPoint(int iX, int iY);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to zoom in by given percercentage around the point passed
	// REQUIRE: valid view point (can not have negative values)
	// PROMISE: zooms in and moves the point passed to the centre of the view.
	//          so that user will feel like zoomed in around the point passed
	void ZoomInAroundTheClickedPoint(int iX, int iY);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to set the zone object used in the view to create zones interactively
	// REQUIRE: the method should only be called after the view class creation
	// PROMISE: returns the zone object pointer
	CZoneHighlightThrd* getZoneObjectOfView() { return &m_zoneHighltThrd;}
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to set current related CGenericDisplayCtrl object. This is required to enable UGD to
	// REQUIRE: create multiple instances and to link view and frame of corresponding ctrl object
	// PROMISE: 
	void setGenericDisplayCtrl(CGenericDisplayCtrl* pGenericDisplayCtrl) {m_pGenericDisplayCtrl = pGenericDisplayCtrl;}
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to return the page count of the image loaded
	// REQUIRE: LoadImage() must have been called at least once
	// PROMISE: returns the total no. of pages in the currently loaded image
	//long getImagePageCount() {return m_ulTotalPages;}
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to reset the page count of the image loaded
	// REQUIRE: 
	// PROMISE: reset the count of the pages in the image edit control
	void initializeImageLoading(bool bValue) {m_bImageLoadedForFirstTime = bValue;}
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to return the default cursor of the image control
	// REQUIRE: Image edit control must have been created in view
	// PROMISE: returns the default cursor for the Generic display as long 
	long getDefaultMousePointer() {return m_ldefCursor;}
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to return the changed X coordinate of origin.
	// REQUIRE: Image edit control must have been created in view
	// PROMISE: returns the X coordinate
	long getDeltaOriginX() { return m_iDeltaOriginX; };
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to set the X coordinate of changed origin.
	// REQUIRE: Image edit control must have been created in view
	// PROMISE: sets the X coordinate
	void setDeltaOriginX(long iDeltaOriginX) { m_iDeltaOriginX = iDeltaOriginX; };
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to return the changed Y coordinate of origin.
	// REQUIRE: Image edit control must have been created in view
	// PROMISE: returns the Y coordinate
	long getDeltaOriginY(){ return m_iDeltaOriginY; };
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to set the Y coordinate of changed origin.
	// REQUIRE: Image edit control must have been created in view
	// PROMISE: sets the X coordinate
	void setDeltaOriginY(long iDeltaOriginY) { m_iDeltaOriginY = iDeltaOriginY; };
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to set the base rotation.
	// REQUIRE: Image edit control must have been created in view
	// PROMISE: sets the given value to the base rotation.
	void setBaseRotation(double dBaseRotation);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: updates the zone interactive creation by incrementing or decrementing the zone 
	//          height or quitting the zone creation process 
	// REQUIRE: Internal method used in WincProc or OnKeyUpEditControl(). Do not use else where
	// PROMISE: Based on the KeyCode value updates the zone Height or quits the creation
	// RETURN:	true - if the key press has already been processed, false - otherwise
	bool UpdateInteractiveZoneCreation(short KeyCode);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To fit the image edit control in view window.
	// REQUIRE: Nothing.
	// PROMISE: Fits the control in view and updates the zoom factor resulting 
	//			by setting the image extents.
	void FitToParent(bool bFitPage);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return zoom percent value of zoom extents.
	// REQUIRE: Nothing.
	// PROMISE: 
	double getZoomPercentOfZoomExtents() { return m_dZoomPercentOnExtents; }
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return zoom percent value of zoom fit to width.
	// REQUIRE: Nothing.
	// PROMISE: 
	double getZoomPercentOfZoomFitToWidth() { return m_dZoomPercentOnFitToWidth; }
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To hide the image edit control to avoid flickering while 
	//			doing multiple operations like image loading, setting base rotation, etc.
	// REQUIRE: Nothing.
	// PROMISE: 
	void HideImageEditWindow();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To show back the image edit control after the multiple operations
	//			like image loading, setting base rotation, etc.
	// REQUIRE: HideImageEditWindow() may have been called.
	// PROMISE: 
	void ShowImageEditWindow();
	//---------------------------------------------------------------------------------------------
	void setZoom(double dZoom);
	//---------------------------------------------------------------------------------------------

	// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CGenericDisplayView)
	public:
	virtual BOOL Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CCreateContext* pContext = NULL);
	protected:
  	virtual void OnDraw(CDC* pDC);      // overridden to draw this view
	virtual LRESULT WindowProc(UINT message, WPARAM wParam, LPARAM lParam);
	//}}AFX_VIRTUAL

// Implementation
protected:
	virtual ~CGenericDisplayView();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

	// Generated message map functions
public:
	//{{AFX_MSG(CGenericDisplayView)
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnSelectionRectDrawnEditctrl1(long Left, long Top, long Width, long Height);
	afx_msg void OnMouseUpEditctrl1(short Button, short Shift, long x, long y);
	afx_msg void OnMouseDownEditctrl1(short Button, short Shift, long x, long y);
	afx_msg void OnMouseMoveEditctrl1(short Button, short Shift, long x, long y);
	afx_msg void OnScrollEditctrl1();
	afx_msg void OnMouseWheelEditctrl1(long wParam, long lParam);
	afx_msg void OnDblClickEditctrl1();
	afx_msg BOOL OnMouseWheel(UINT nFlags, short zDelta, CPoint pt);
	afx_msg void OnTimer(UINT nIDEvent);
	afx_msg void OnKeyUpImageeditctrl1(short FAR* KeyCode, short Shift);
	afx_msg void OnKeyDownImageeditctrl1(short FAR* KeyCode, short Shift);
	DECLARE_EVENTSINK_MAP()
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	// Stores data related to auto fitting highlights
	struct FittingData
	{
		Point leftTop;
		Point rightBottom;
		Point theta;
	};

	//////////
	// enums
	//////////
	enum EScrollDirection
	{
		kNoScroll = 0,
		kScrollLeft, 
		kScrollRight, 
		kScrollUp, 
		kScrollDown
	};

	////////////
	// Variables
	////////////
	// percentage zoom factor
	double	m_dCurZoomFactor;

	//currents scroll positions that indicate the top, 
	//left position of the display window area within the image
	int	m_iScrollX, m_iScrollY;	

	//	flag for the creation of image control
	BOOL m_bCreate;

	//	to save the view size and change in origin
	int	m_iViewSizeX;
	int	m_iViewSizeY;
	int	m_iDeltaOriginX;
	int	m_iDeltaOriginY;

	//	rubber band
	CRubberBandThrd	m_RBThrd;

	// zoneHighlight thread
	CZoneHighlightThrd m_zoneHighltThrd;

	// Zone adjustment thread
	CZoneAdjustmentThrd m_zoneAdjustmentThrd;

	// image width 
	int m_iImgWidth;

	// image height
	int m_iImgHeight;

	// to know whether left mouse button is clicked
	bool m_bLeftBtnClicked;

	// to save the previous zoom factor
	double m_dPrevZoomFactor;
	
	// current zone height 
	long m_lCurrentZoneHt;

	// the percentage of image magnification that changes in zoomIn and zoomOut
	// methods
	double m_dPercentMagnify;

	// mouse move in pixels after LButton Down (mouse drag)
	//int m_iMouseDragCountInPixels;
	Point m_LBtnDownPt;
	
	// the total no. of pages in the loaded image
	//unsigned long m_ulTotalPages;
	// have a local variable for the control
	CGenericDisplayCtrl *m_pGenericDisplayCtrl;

	// to default cursor to set for Generic Display
	long m_ldefCursor;

	// The cursors to use when moving or resizing highlights
	HCURSOR m_hMoveCursor;
	HCURSOR m_hHorizontalResize;
	HCURSOR m_hVerticalResize;
	HCURSOR m_hNwseResize;
	HCURSOR m_hNeswResize;

	bool m_bImageLoadedForFirstTime;

	// draw entities should not be called while OnSize is in progress
	// this is to avoid false update of scroll positions
	bool m_bOnSizeInProgress;

	// To save the zoom percent value on zoom extents.
	double m_dZoomPercentOnExtents;

	// To save the zoom percent value on zoom fit to width
	double m_dZoomPercentOnFitToWidth;

	////////////
	// Methods
	////////////

	// get scroll direction based on the virtual key code
	EScrollDirection getScrollDirectionType(int nVirtualKeyCode);

	// if nX and nY are out of the image boundary, then assign the boundary
	// pixel to them
	// nX and nY are in client window pixels
	void limitPoint(long& nX, long& nY);

	// scroll up/down/left/right by a unit
	// return TRUE if it's scrolled
	BOOL scrollImage(EScrollDirection eScrollDirection);

	// Adds a zone entity using the specified spatial information. Performs auto-fitting if 
	// necessary based on the fitting mode. To be called after a manual redaction has been drawn. 
	void addZoneEntity(int istartX, int istartY, int iendX, int iendY, long lHeight, 
		bool bCtrlPressed, bool bShiftPressed);
	long addZoneEntity(int iStartX, int iStartY, int iEndX, int iEndY, long lHeight);

	// Adjusts a highlight to the smallest highlight with that contains all the black pixels of 
	// the original highlight and retains the angle of the original highlight.
	long blockFit(int iStartX, int iStartY, int iEndX, int iEndY, long lHeight);
	long blockFit(Point &leftTop, Point &rightBottom, Point theta, LeadToolsBitmap &bitmap);

	// Splits a highlight into multiple lines, block fitting each one.
	void lineFit(int iStartX, int iStartY, int iEndX, int iEndY, long lHeight);

	// Gets data necessary for auto fitting algorithms from the specified highlight
	FittingData getFittingData(int iStartX, int iStartY, int iEndX, int iEndY, long lHeight);

	// Determines the first row from the top by which the specified highlight could be split.
	// This function assumes shrinkTopSide and shrinkBottomSide have been called on the data.
	double findRowToSplit(Point leftTop, Point rightBottom, Point theta, LeadToolsBitmap &bitmap);
	
	// Returns true if the specified row contains a black pixel; false otherwise.
	bool rowContainsBlackPixel(double dLeft, double dTop, double dRight, Point theta, 
		LeadToolsBitmap &bitmap);

	// Returns true if the specified point after applying the rotation theta is black; 
	// false if it is off the image or white.
	bool isBlackPixel(double dX, double dY, Point theta, LeadToolsBitmap &bitmap);

	// Shrink a side to exactly fit all the black pixels in the specified region.
	void shrinkLeftSide(Point &leftTop, Point &rightBottom, Point theta, LeadToolsBitmap &bitmap);
	void shrinkTopSide(Point &leftTop, Point &rightBottom, Point theta, LeadToolsBitmap &bitmap);
	void shrinkRightSide(Point &leftTop, Point &rightBottom, Point theta, LeadToolsBitmap &bitmap);
	void shrinkBottomSide(Point &leftTop, Point &rightBottom, Point theta, LeadToolsBitmap &bitmap);

	// Returns the specified angle expressed as an angle between -PI/2 and PI/2 from the horizontal
	static double getAngleFromHorizontal(double dAngle);

	// Returns the specified point rotated about the origin using 
	// the horizontal and vertical components of the angle theta.
	static Point rotate(Point point, Point theta);
	static Point rotate(Point point, double sinTheta, double cosTheta);
	static Point rotate(double x, double y, Point theta);
	static Point rotate(double x, double y, double sinTheta, double cosTheta);
};
