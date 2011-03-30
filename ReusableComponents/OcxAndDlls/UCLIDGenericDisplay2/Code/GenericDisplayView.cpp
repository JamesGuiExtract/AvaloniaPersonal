//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericDisplayView.cpp
//
// PURPOSE:	This is an implementation file for CGenericDisplayView() class.
//			Where the CGenericDisplayView() class has been derived from CView() class.
//			The code written in this file makes it possible to implement the various
//			application methods in the user interface.
// NOTES:	
//
// AUTHORS:	
//
//-------------------------------------------------------------------------------------------------
// GenericDisplayView.cpp : implementation file
//
#include "stdafx.h"
#include "GenericDisplay.h"
#include "GenericDisplayView.h"
#include "GenericDisplayDocument.h"
#include "GenericDisplayFrame.h"
#include "GenericDisplayCtl.h"
#include "GenericEntity.h"

#include <cpputil.h>
#include <UCLIDException.h>
#include <LeadToolsBitmap.h>
#include <COMUtils.h>

#include <cmath>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giMouseDragAllowed = 20;

// number of allowed Image types like .tif, .bmp etc
const int NUMALLOWEDTYPES = 7;

// When a highlight is split during line fitting, the minimum 
// height in image pixels of the resultant highlights.
const int MIN_SPLIT_HEIGHT = 10;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif
#define IDC_EDITIMGCTRL 10000

#define ID_REPAINT_ON_RESIZE 25400

//-------------------------------------------------------------------------------------------------
// CGenericDisplayView
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CGenericDisplayView, CView)
//-------------------------------------------------------------------------------------------------
CGenericDisplayView::CGenericDisplayView()
{
	//	set zoom factor to 100
	m_dCurZoomFactor	= 100.0;

	// initialize the prev zoom factor
	m_dPrevZoomFactor = 0.0;

	//	set the scroll values to zero
	m_iScrollX			= 0;
	m_iScrollY			= 0;

	//	set the create value to FALSE
	m_bCreate			= FALSE;
	
	// set the left mouse button clicked to FALSE
	m_bLeftBtnClicked = false;

	// the percentage of image magnification that applies
	// for zoomIn and zoomOut
	m_dPercentMagnify = 20.0;

	m_RBThrd.m_pGenView = this;
	m_zoneHighltThrd.m_pGenView = this;
	m_zoneAdjustmentThrd.setGenericDisplayView(this);

	//	initialize the current zone height.
	m_lCurrentZoneHt = 0;

	//	initialize the LButton down point.
	m_LBtnDownPt.dXPos = 0;
	m_LBtnDownPt.dYPos = 0;

	// Define cursors
	m_ldefCursor = 0;
	m_hMoveCursor = LoadCursor(NULL, IDC_SIZEALL);
	m_hHorizontalResize = LoadCursor(NULL, IDC_SIZEWE);
	m_hVerticalResize = LoadCursor(NULL, IDC_SIZENS);
	m_hNwseResize = LoadCursor(NULL, IDC_SIZENWSE);
	m_hNeswResize = LoadCursor(NULL, IDC_SIZENESW); 

	m_bImageLoadedForFirstTime = true;

	m_pGenericDisplayCtrl = NULL;

	//	set image width and height
	m_iImgWidth = 0;
	m_iImgHeight = 0;

	//	set Delta origin.
	m_iDeltaOriginX = 0;
	m_iDeltaOriginY = 0;

	m_bOnSizeInProgress = false;

	// define zoom percent of zoom extents;
	m_dZoomPercentOnExtents = 0.0;

	// define zoom percent of zoom fit to width;
	m_dZoomPercentOnFitToWidth = 0.0;
}
//-------------------------------------------------------------------------------------------------
CGenericDisplayView::~CGenericDisplayView()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16583");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CGenericDisplayView, CView)
	//{{AFX_MSG_MAP(CGenericDisplayView)
	ON_WM_SIZE()
	ON_WM_MOUSEWHEEL()
	ON_WM_TIMER()
	ON_WM_HSCROLL()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
BEGIN_EVENTSINK_MAP(CGenericDisplayView, CView)
    //{{AFX_EVENTSINK_MAP(CGenericDisplayView)
	ON_EVENT(CGenericDisplayView, IDC_EDITIMGCTRL, 4 /* SelectionRectDrawn */, OnSelectionRectDrawnEditctrl1, VTS_I2 VTS_I2 VTS_I4 VTS_I4)
	ON_EVENT(CGenericDisplayView, IDC_EDITIMGCTRL, 1 /* Scroll */, OnScrollEditctrl1, VTS_NONE)
	ON_EVENT(CGenericDisplayView, IDC_EDITIMGCTRL, 2 /* MouseWheel */, OnMouseWheelEditctrl1, VTS_I4 VTS_I4)
	ON_EVENT(CGenericDisplayView, IDC_EDITIMGCTRL, -607 /* MouseUp */, OnMouseUpEditctrl1, VTS_I2 VTS_I2 VTS_I4 VTS_I4)
	ON_EVENT(CGenericDisplayView, IDC_EDITIMGCTRL, -605 /* MouseDown */, OnMouseDownEditctrl1, VTS_I2 VTS_I2 VTS_I4 VTS_I4)
	ON_EVENT(CGenericDisplayView, IDC_EDITIMGCTRL, -606 /* MouseMove */, OnMouseMoveEditctrl1, VTS_I2 VTS_I2 VTS_I4 VTS_I4)
	ON_EVENT(CGenericDisplayView, IDC_EDITIMGCTRL, -601 /* DblClick */, OnDblClickEditctrl1, VTS_NONE)
	ON_EVENT(CGenericDisplayView, IDC_EDITIMGCTRL, -604 /* KeyUp */, OnKeyUpImageeditctrl1, VTS_PI2 VTS_I2)
	ON_EVENT(CGenericDisplayView, IDC_EDITIMGCTRL, -602 /* KeyDown */, OnKeyDownImageeditctrl1, VTS_PI2 VTS_I2)
	//}}AFX_EVENTSINK_MAP
END_EVENTSINK_MAP()

//-------------------------------------------------------------------------------------------------
// CGenericDisplayView drawing
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::OnDraw(CDC* pDC)
{
	//	get the control pointer
	if(m_bCreate)
	{
		// Update Rubber band
		m_RBThrd.updateRubberBand();

		// Update the invalidated area
		RECT rect = {0, 0, 0, 0};
		RECT* pRect = &rect;
		GetUpdateRect(pRect);
		if (pRect == NULL)
		{
			m_ImgEdit.Refresh();
		}
		else
		{
			m_ImgEdit.RefreshRect(rect.left, rect.top, rect.right, rect.bottom);
		}

		// Redraw the entitities
		m_pGenericDisplayCtrl->DrawEntities(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
// CGenericDisplayView diagnostics
//-------------------------------------------------------------------------------------------------
#ifdef _DEBUG
void CGenericDisplayView::AssertValid() const
{
	CView::AssertValid();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
}
#endif //_DEBUG
//-------------------------------------------------------------------------------------------------
// CGenericDisplayView message handlers
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayView::CreateImageCtrl()
{
	//	create ImgEdit and insert into the project
	CRect viewRect(0, 0, 600, 180);

	int iStatus = 0;

	try 
	{
		//	create Wang Image control
		iStatus = m_ImgEdit.Create(NULL, WS_CHILD | WS_VISIBLE | WS_VSCROLL | WS_HSCROLL, viewRect, this, IDC_EDITIMGCTRL);

		if (iStatus == 0)
		{
			AfxThrowOleDispatchException (0, "ELI90040: Image Edit Control OCX could not be found");
			return FALSE;
		}
		m_ImgEdit.SetWindowPos (&wndBottom, 0,0,viewRect.Width (), viewRect.Height (), SWP_SHOWWINDOW);
	}
	catch(...)
	{
		AfxThrowOleDispatchException (0, "ELI90023: Error in Wang Image Control Initialization.");
	}

	SetWindowPos (&wndTop, 0,0,viewRect.Width (), viewRect.Height (), SWP_SHOWWINDOW);
	m_bCreate = TRUE;

	//	display window
	m_ImgEdit.ShowWindow (FALSE);

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::LoadImage()
{
	try
	{
		// Display and FitToParent methods redraw the image edit control. Redrawing two times
		// leads to flicker the image. Hence disable and hide the image edit control till
		// FitToParent is called. Unless or otherwise Display() is called on Image control, FitToParent()
		// can not be called ( Constraint with Lead tools).
		m_ImgEdit.Display();
		
		//	get image width and height.
		m_iImgWidth = m_ImgEdit.GetImageWidth();
		m_iImgHeight = m_ImgEdit.GetImageHeight();

		//	update zoom extents zoom factor
		m_dCurZoomFactor = m_ImgEdit.GetZoom();
		
		if (m_bImageLoadedForFirstTime)
			m_bImageLoadedForFirstTime = false;
		
		//	get the current scroll postions.
		m_iScrollX = m_ImgEdit.GetScrollPositionX();
		m_iScrollY = m_ImgEdit.GetScrollPositionY();

		m_ImgEdit.Refresh();

	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 

		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayView::Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CCreateContext* pContext) 
{
	
	if((CWnd::Create(lpszClassName, lpszWindowName, dwStyle, rect, pParentWnd, nID, pContext)) == 0)
		return FALSE;

	//	create image control
	if (!CreateImageCtrl())
		return FALSE;

	try
	{
		// get the default cursor of the image edit control
		m_ldefCursor = m_ImgEdit.GetMousePointer();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();

		AfxThrowOleDispatchException(0, str);
	}
 
	//	set the window on top
	BringWindowToTop ();
	return 1;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ZoomIn() 
{
	// if no image is loaded, nothing to do with
	m_pGenericDisplayCtrl->checkForImageLoading("ZoomInAroundPoint");

	// Check if the current zoom factor  is within limits(2 to 6500)
	if((m_dCurZoomFactor >= 2.0) && (m_dCurZoomFactor < 6500.0))
	{
		if((m_dCurZoomFactor*((100.0 + m_dPercentMagnify)/100.0)) < 6500.0)
		{
			try
			{
				CRect rectClient;
				m_ImgEdit.GetClientRect(rectClient);
				
				long iClientX = rectClient.Width() / 2;
				long iClientY = rectClient.Height() / 2;

				//	zoom in the image with respect to the middle of the client area.
				m_ImgEdit.ZoomInAroundPoint(iClientX, iClientY);
			
//				::ReleaseCapture();

				//	get the current zoom factor
				m_dCurZoomFactor = m_ImgEdit.GetZoom();

				//	get the current scroll positions of the image
				m_iScrollX = m_ImgEdit.GetScrollPositionX();
				m_iScrollY = m_ImgEdit.GetScrollPositionY();
			}
			catch(COleDispatchException* ptr)
			{
				CString str = ptr->m_strDescription; 
				
				ptr->Delete();

				AfxThrowOleDispatchException(0, str);
			}

			BringWindowToTop();

			//	draw the entities
			m_pGenericDisplayCtrl->DrawEntities (TRUE);

			//	Update Rubber band
			m_RBThrd.updateRubberBand();
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ZoomInAroundTheClickedPoint(int iX, int iY)
{
	// if no image is loaded, nothing to do with
	m_pGenericDisplayCtrl->checkForImageLoading("ZoomInAroundPoint");

	// Check if the current zoom factor  is within limits(2 to 6500)
	if((m_dCurZoomFactor >= 2.0) && (m_dCurZoomFactor < 6500.0))
	{
		if((m_dCurZoomFactor*((100.0 + m_dPercentMagnify)/100.0)) < 6500.0)
		{
			try
			{
				CRect rectClient;
				GetWindowRect(rectClient);
				
				// convert from screen to client coordinates			
				int	iClientX = iX - rectClient.left;
				int iClientY = iY - rectClient.top;

				//	zoom in the image with respect to the mouse point.
				m_ImgEdit.ZoomInAroundPoint(iClientX, iClientY);

//				::ReleaseCapture();
				
				//	get the current zoom factor
				m_dCurZoomFactor = m_ImgEdit.GetZoom();
				
				//	get the current scroll positions of the image
				m_iScrollX = m_ImgEdit.GetScrollPositionX();
				m_iScrollY = m_ImgEdit.GetScrollPositionY();
			}
			catch(COleDispatchException* ptr)
			{
				CString str = ptr->m_strDescription; 
		
				ptr->Delete();
				
				AfxThrowOleDispatchException(0, str);
			}

			BringWindowToTop();

			//	draw the entities
			m_pGenericDisplayCtrl->DrawEntities (TRUE);

			//	Update Rubber band
			m_RBThrd.updateRubberBand();
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ZoomOut() 
{
	// if no image is loaded, nothing to do with
	m_pGenericDisplayCtrl->checkForImageLoading("ZoomOutAroundPoint");

	// Check if the current zoom factor  is within limits(2 to 6500)//
	if((m_dCurZoomFactor >= 2.0) && (m_dCurZoomFactor < 6500.0))
	{
		if((m_dCurZoomFactor* (1/((100.0 + m_dPercentMagnify)/100.0))) > 2.0)
		{
			CRect rectClient;
			m_ImgEdit.GetClientRect(rectClient);
			
			int	iClientX = rectClient.Width() / 2;
			int iClientY = rectClient.Height() / 2;
			
			try
			{
				//	zoom out the image with respect to the center of window.
				m_ImgEdit.ZoomOutAroundPoint(iClientX, iClientY);

//				::ReleaseCapture();

				//	get the current zoom factor
				m_dCurZoomFactor = m_ImgEdit.GetZoom();

				//	get the current scroll positions
				m_iScrollX = m_ImgEdit.GetScrollPositionX();
				m_iScrollY = m_ImgEdit.GetScrollPositionY();
			}
			catch(COleDispatchException* ptr)
			{
				CString str = ptr->m_strDescription; 
	
				ptr->Delete();
				
				AfxThrowOleDispatchException(0, str);
			}

			BringWindowToTop();

			//	draw the entities
			m_pGenericDisplayCtrl->DrawEntities (TRUE);

			// Update Rubber band
			m_RBThrd.updateRubberBand ();
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ZoomOutAroundTheClickedPoint(int iX, int iY)
{
	// if no image is loaded, nothing to do with
	m_pGenericDisplayCtrl->checkForImageLoading("ZoomOutAroundPoint");

	// Check if the current zoom factor  is within limits(2 to 6500)//
	if((m_dCurZoomFactor >= 2.0) && (m_dCurZoomFactor < 6500.0))
	{
		if((m_dCurZoomFactor* (1/((100.0 + m_dPercentMagnify)/100.0))) > 2.0)
		{
			CRect rectClient;
			GetWindowRect(rectClient);
			
			// convert from screen to client coordinates			
			int	iClientX = iX - rectClient.left;
			int iClientY = iY - rectClient.top;
			
			try
			{
				//	zoom out the image with respect to the mouse point.
				m_ImgEdit.ZoomOutAroundPoint(iClientX, iClientY);

//				::ReleaseCapture();
				
				//	get the current zoom factor
				m_dCurZoomFactor = m_ImgEdit.GetZoom();

				//	get the current scroll positions
				m_iScrollX = m_ImgEdit.GetScrollPositionX();
				m_iScrollY = m_ImgEdit.GetScrollPositionY();
			}
			catch(COleDispatchException* ptr)
			{
				CString str = ptr->m_strDescription; 
				
				ptr->Delete();

				AfxThrowOleDispatchException(0, str);
			}

			BringWindowToTop();

			//	draw the entities
			m_pGenericDisplayCtrl->DrawEntities (TRUE);

			// Update Rubber band
			m_RBThrd.updateRubberBand ();
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ZoomExtents() 
{
	try
	{
		FitToParent(true);

		// set the zoom factor of the zoom extents
		m_dCurZoomFactor = m_ImgEdit.GetZoom();
		
		//	refresh the control
		m_ImgEdit.Refresh();
		
		//	get x scroll value
		m_iScrollX = m_ImgEdit.GetScrollPositionX();
		
		//	get y scroll value
		m_iScrollY = m_ImgEdit.GetScrollPositionY();
		
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
	
		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}

	//	draw the entities
	m_pGenericDisplayCtrl->DrawEntities (TRUE);

	//	Update Rubber band
	m_RBThrd.updateRubberBand ();

}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ZoomToWidth() 
{
	try
	{
		FitToParent(false);

		// set the zoom factor of the zoom extents
		m_dCurZoomFactor = m_ImgEdit.GetZoom();
		
		//	refresh the control
		m_ImgEdit.Refresh();
		
		//	get x scroll value
		m_iScrollX = m_ImgEdit.GetScrollPositionX();
		
		//	get y scroll value
		m_iScrollY = m_ImgEdit.GetScrollPositionY();		
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
	
		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}

	//	draw the entities
	m_pGenericDisplayCtrl->DrawEntities (TRUE);

	//	Update Rubber band
	m_RBThrd.updateRubberBand ();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::OnSize(UINT nType, int cx, int cy) 
{
	CView::OnSize(nType, cx, cy);

	if(m_bCreate)
	{
		m_bOnSizeInProgress = true;

		//	set window position
		//m_ImgEdit.SetWindowPos(&wndBottom, 0, 0, cx, cy, SWP_SHOWWINDOW);
		m_ImgEdit.SetWindowPos(&wndBottom, 0, 0, cx, cy, SWP_NOREDRAW|SWP_NOMOVE);
		m_dCurZoomFactor = m_ImgEdit.GetZoom();
	}

	BringWindowToTop ();

	//	set the view sizes
	m_iViewSizeX = cx;
	m_iViewSizeY = cy;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::OnScrollEditctrl1() 
{
	try
	{
		//	get the current scroll positions
		m_iScrollX = m_ImgEdit.GetScrollPositionX();
		m_iScrollY = m_ImgEdit.GetScrollPositionY();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 

		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}

	BringWindowToTop ();

	if(m_bCreate)
	{
		m_ImgEdit.Refresh();

		//	draw the entities
		m_pGenericDisplayCtrl->DrawEntities (TRUE);

		//	Update Rubber band
		m_RBThrd.updateRubberBand ();
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::OnMouseWheelEditctrl1(long wParam, long lParam)
{
	CPoint point(LOWORD(lParam), HIWORD(lParam));
	OnMouseWheel(LOWORD(wParam), HIWORD(wParam), point);
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::OnSelectionRectDrawnEditctrl1(long Left, long Top, long Width, long Height) 
{
	if(Width == 0 && Height == 0)
		return;

	try
	{
		//	get the current zoom factor
		double fCurZoom = m_ImgEdit.GetZoom();
		if (fCurZoom < 2.0 || fCurZoom > 6500.0)
		{
			return;
		}
		else
		{
			//	get the current scroll positions
			m_iScrollX = m_ImgEdit.GetScrollPositionX();
			m_iScrollY = m_ImgEdit.GetScrollPositionY();
		}
		
		//	no selection rectangle
		m_ImgEdit.DrawSelectionRect(0,0,0,0);
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 

		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}

	//	bring the window to top
	BringWindowToTop ();

	//	draw the entities
	m_pGenericDisplayCtrl->DrawEntities (TRUE);

	//	Update Rubber band
	m_RBThrd.updateRubberBand ();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::OnMouseMoveEditctrl1(short Button, short Shift, long x, long y)
{
	char pszBufX[255];
	char pszBufY[255];

	// fire the mouse move event
	m_pGenericDisplayCtrl->FireMouseMove (Button, Shift, x,y);

	//	set cursor type
	setCursorType(x, y);

	double	dWorldX ;
	double	dWorldY ;
	long	dImagePixelX ;
	long	dImagePixelY ;
	double	dXPos ;
	double	dYPos ;
	long	ulTrackingOption;
	BOOL	bDisplayPercentage;

	// check whether the point is within the image boundaries.
	bool bPtInImageBoundaries = IsPtInImageBoundaries(x, y) == TRUE;

	//To store the tracking option 
	ulTrackingOption = m_pGenericDisplayCtrl->getXYTrackingOption();

	// Get the the flag to control if we want to display percentage in statusbar
	bDisplayPercentage = m_pGenericDisplayCtrl->getDisplayPercentage();

	// set the tracking coordinates only if the option is not 0
	if(ulTrackingOption != 0 && bPtInImageBoundaries)
	{
		// get the current tracking option
		// convert the view coordinates to required track option
		// then set the value in the status bar of the frame
		switch(ulTrackingOption)
		{
		case 1: 
			{
				// X/Y status indicator fields in the status bar shall display the world coordinates 
				// of the current mouse cursor's position.
				m_pGenericDisplayCtrl->convertClientWindowPixelToWorldCoords(x, y, &dWorldX, &dWorldY, m_pGenericDisplayCtrl->getCurrentPageNumber());
				dXPos=dWorldX;
				dYPos=dWorldY;
			}
			break;
		case 2:
			{
				long nPageNum = m_pGenericDisplayCtrl->getCurrentPageNumber();
				// X/Y status indicator fields in the status bar shall display the image pixel
				// coordinates of the current mouse cursor's position.
				m_pGenericDisplayCtrl->convertClientWindowPixelToWorldCoords(x, y, &dWorldX, &dWorldY, nPageNum);
				m_pGenericDisplayCtrl->convertWorldToImagePixelCoords(dWorldX, dWorldY, &dImagePixelX, &dImagePixelY, nPageNum);
				dXPos=dImagePixelX;
				dYPos=dImagePixelY;
			}
			break;
		case 3:
			{
				// X/Y status indicator fields in the status bar shall display the client window pixel
				// coordinates of the current mouse cursor's position.
				dXPos=x;
				dYPos=y;
			}
			break;
		default: 
			{
				CString zError = "ELI90236: The specified Tracking option is not available";
				AfxThrowOleDispatchException (0, LPCTSTR(zError));
			}
			break;
		}

		// Get Frame pointer
		CGenericDisplayFrame *pFrame = NULL;
		pFrame = m_pGenericDisplayCtrl->getUGDFrame();
		
		if (ulTrackingOption == 1)
		{
			// set the coordinate values in the frame
			sprintf_s (pszBufX, sizeof(pszBufX), "X : %.4f", dXPos);
			sprintf_s (pszBufY, sizeof(pszBufY), "Y : %.4f", dYPos);

			// If we need to display percentage on the status bar
			if (bDisplayPercentage)
			{
				// Get the image resolution
				int iResX = 0;
				int iResY = 0;
				m_pGenericDisplayCtrl->getImageResolution(&iResX, &iResY);

				if (iResX !=0 && iResY !=0)
				{
					// Get the image width and height in feet
					double dImgWidth = m_iImgWidth/(iResX*12.0);
					double dImgHeight = m_iImgHeight/(iResY*12.0);

					if (dImgWidth != 0.0 && dImgHeight != 0.0)
					{
						// Append the percentage to the coordinate value
						sprintf_s (pszBufX, sizeof(pszBufX), "%s, %d%%", pszBufX, round(dXPos/dImgWidth*100));
						sprintf_s (pszBufY, sizeof(pszBufY), "%s, %d%%", pszBufY, round(dYPos/dImgHeight*100));
					}
				}
			}

			//	Show the X, Y coordinate in pane 2 and 3
			pFrame->statusText (2, pszBufX);
			pFrame->statusText (3, pszBufY);
		}
		else if (ulTrackingOption == 2)
		{
			// If percentage need to be displayed
			if (bDisplayPercentage && m_iImgWidth != 0 && m_iImgHeight != 0)
			{
				// set the X, Y coordinate value with the percentage
				sprintf_s (pszBufX, sizeof(pszBufX), "X : %d, %d%%", (int)dXPos, round(dXPos/m_iImgWidth*100));
				sprintf_s (pszBufY, sizeof(pszBufY), "Y : %d, %d%%", (int)dYPos, round(dYPos/m_iImgHeight*100));
			}
			else
			{
				// set the X, Y coordinate value
				sprintf_s (pszBufX, sizeof(pszBufX), "X : %d", (int)dXPos);
				sprintf_s (pszBufY, sizeof(pszBufY), "Y : %d", (int)dYPos);
			}

			//	Show the X, Y coordinate in pane 2 and 3
			pFrame->statusText (2, pszBufX);
			pFrame->statusText (3, pszBufY);
		}
		else if (ulTrackingOption == 3)
		{
			// If percentage need to be displayed
			if (bDisplayPercentage && m_iViewSizeX != 0 && m_iViewSizeY != 0)
			{
				// set the X, Y coordinate value with the percentage
				sprintf_s (pszBufX, sizeof(pszBufX), "X : %d, %d%%", (int)dXPos, round(dXPos/m_iViewSizeX*100));
				sprintf_s (pszBufY, sizeof(pszBufY), "Y : %d, %d%%", (int)dYPos, round(dYPos/m_iViewSizeY*100));
			}
			else
			{
				// set the X, Y coordinate value
				sprintf_s (pszBufX, sizeof(pszBufX), "X : %d", (int)dXPos);
				sprintf_s (pszBufY, sizeof(pszBufY), "Y : %d", (int)dYPos);
			}

			//	Show the X, Y coordinate in pane 2 and 3
			pFrame->statusText (2, pszBufX);
			pFrame->statusText (3, pszBufY);
		}
	}
	else
	{
		// the current X/Y status indicator fields in the status bar will be cleared and no 
		// more mouse-position tracking will be performed.
		CGenericDisplayFrame *pFrame = NULL;
		pFrame = m_pGenericDisplayCtrl->getUGDFrame();
		pFrame->statusText (2, "");
		pFrame->statusText (3, "");
	}

	// IMPORTANT: Either rubber banding or zone entity is to be drawn within 
	// the limits of the image. So the following calls are made after
	// ensuring that the cursor is well within the image boundaries.
	if(m_RBThrd.IsRubberBandingParametersSet() && m_RBThrd.IsRubberBandingEnabled())
	{
		// if we are not within the image bounds, then
		// set x and y to the maximum extent of the image bounds as appropriate
		limitPoint(x, y);
		//	create thread
		m_RBThrd.Draw(x, y);

		return;
	}
	
	// Update the zone endpoint if and only if the new zone bounds with the new end point to set 
	// are well within the image bounds.
	if (bPtInImageBoundaries && m_bLeftBtnClicked && !m_RBThrd.IsRubberBandingEnabled())
	{
		// Check what type of interactive event this is
		if (m_zoneHighltThrd.IsInteractiveZoneEntCreationEnabled())
		{
			CPoint * pvertexPts = NULL;
			
			// get the bounds for the new height to set
			pvertexPts = m_zoneHighltThrd.getZoneBoundsForThisEndPoint(x, y); 
			
			m_zoneHighltThrd.SetEndPoint(x,y);
			
			m_zoneHighltThrd.Draw();
			
			if(!m_zoneHighltThrd.MouseMovedAfterZoneInitialization())
				m_zoneHighltThrd.setMouseMovedAfterZoneInitialization(true);
			pvertexPts = NULL;
		}
		else if(m_zoneAdjustmentThrd.isInteractiveZoneEntAdjustmentEnabled())
		{
			m_zoneAdjustmentThrd.update(x,y);
		}
	}

	return;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::OnMouseDownEditctrl1(short Button, short Shift, long x, long y) 
{
	// if the button is Lbutton, set m_bLeftBtnClicked variable to true
	if (Button == 1)
	{
		m_bLeftBtnClicked = true;
		m_LBtnDownPt.dXPos = x;
		m_LBtnDownPt.dYPos = y;
	}

	// fire the mouse down event here
	m_pGenericDisplayCtrl->FireMouseDown (Button, Shift, x, y);

	if(m_bLeftBtnClicked && m_pGenericDisplayCtrl->isEntitySelectionEnabled() && 
		!m_pGenericDisplayCtrl->isRubberbandingEnabled())
	{
		// Convert the point clicked to world coordinates
		double dXPos = x, dYPos = y;
		ViewToImageInWorldCoordinates(&dXPos, &dYPos, m_pGenericDisplayCtrl->getCurrentPageNumber());

		// Check if interactive zone adjustment is enabled
		if (m_pGenericDisplayCtrl->isZoneEntityAdjustmentEnabled())
		{
			// Check if a grip handle was clicked.
			ZoneEntity* pZoneEntity = NULL;
			int gripHandle = m_pGenericDisplayCtrl->getGripHandleId(dXPos, dYPos, &pZoneEntity);
			if (gripHandle >= 0)
			{
				// Start an interactive resize
				m_zoneAdjustmentThrd.start(pZoneEntity, x, y, gripHandle);
				return;
			}
		}
		
		// Get the nearest entity to the mouse up point 
		long lEntityID = m_pGenericDisplayCtrl->getNearestEntityIDFromPoint(dXPos, dYPos);

		// If the selected entity is a valid one, fire the entity selected event
		if (lEntityID != 0)
		{
			// Check if this entity was already selected
			GenericEntity* pEntity = m_pGenericDisplayCtrl->getEntity(lEntityID);
			if (pEntity->getSelected() == FALSE)
			{
				// Deselect all other entities
				m_pGenericDisplayCtrl->selectAllEntities(FALSE);

				// Select this entity
				m_pGenericDisplayCtrl->selectEntity(lEntityID, TRUE);
			}

			// Notify others that this entity was clicked
			m_pGenericDisplayCtrl->FireEntitySelected(lEntityID);

			// Check if interactive zone entity adjustment is enabled and this is a zone entity.
			if (m_pGenericDisplayCtrl->isZoneEntityAdjustmentEnabled() && 
				pEntity->getDesc() == "ZONE")
			{
				// Start tracking
				m_zoneAdjustmentThrd.start((ZoneEntity*) pEntity, x, y);
			}

			return;
		}
	}

	if (m_bLeftBtnClicked && m_pGenericDisplayCtrl->isZoneEntityCreationEnabled())
	{
		// start creating zone entity if zone entity selection is enabled and the point(x,y) 
		// is within the image boundaries.
		// save the point as zone entity start point.
		// set the height of the zone and color by getting the current zone highlight height
		// and color from the control

		if(!IsPtInImageBoundaries(x,y))
			AfxThrowOleDispatchException (0, "ELI90060: Zone start point should be within the image bounds");
		
		m_lCurrentZoneHt = m_pGenericDisplayCtrl->getZoneHighlightHeight();
		m_zoneHighltThrd.enableInteractiveZoneEntCreation(true);
		m_zoneHighltThrd.SetStartPoint(x, y);
		m_zoneHighltThrd.SetHighlightHeight(m_lCurrentZoneHt);
		m_zoneHighltThrd.SetHighlightColor((COLORREF)m_pGenericDisplayCtrl->getZoneColorInXORMode());
	}
	else if (m_bLeftBtnClicked && m_pGenericDisplayCtrl->isRubberbandingEnabled()
			 && !m_pGenericDisplayCtrl->isEntitySelectionEnabled())
	{
		limitPoint(x, y);
		m_RBThrd.setStartPoint2(x, y);
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::OnMouseUpEditctrl1(short Button, short Shift, long x, long y) 
{
	m_pGenericDisplayCtrl->FireMouseUp(Button, Shift, x, y);
	m_pGenericDisplayCtrl->FireClick();

	bool bAltPressed = (GetKeyState(VK_MENU) & 0x8000) != 0;
	bool bShiftPressed = (GetKeyState(VK_SHIFT) & 0x8000) != 0;
	bool bCtrlPressed = (GetKeyState(VK_CONTROL) & 0x8000) != 0;
	if (bAltPressed && !bShiftPressed && !bCtrlPressed)
	{
		if(!m_zoneHighltThrd.MouseMovedAfterZoneInitialization())
		{
			// if ALT button is keyed down and SHIFT and CTRL buttons are not keyed down
			// then, call MoveCursorToNearestEntity()
			if (m_bLeftBtnClicked)
			{
				m_bLeftBtnClicked = false;
				m_pGenericDisplayCtrl->MoveCursorToNearestEntity();
			}
		}
	}
	
	// reset the flags if not already done.
	m_bLeftBtnClicked = false;
	
	if(m_zoneHighltThrd.IsInteractiveZoneEntCreationEnabled() && 
		!m_pGenericDisplayCtrl->isRubberbandingEnabled())
	{
		// if start point and end points are same no need to implement zone entity creation
		if (!m_zoneHighltThrd.MouseMovedAfterZoneInitialization())
		{
			m_zoneHighltThrd.stop();
			return;
		}

		if ( m_zoneHighltThrd.getZoneWidth() < m_pGenericDisplayCtrl->getMinZoneWidth())
		{
			m_zoneHighltThrd.cancel();
			AfxThrowOleDispatchException(0, 
				"ELI90047: Width of zone entity to create is less than minimum zone width.");
		}

		// retrieve the current values of the zone highlight thread to create
		// the zone entity
		int istartX, istartY, iendX, iendY;

		m_zoneHighltThrd.GetStartPoint(istartX, istartY);
		m_zoneHighltThrd.GetEndPoint(iendX, iendY);

		// now reset the highlighting thread parameters. this will erase the 
		// non-persistent highlight zone. The zone entity is redrawn in the following 
		// addZoneEntity() call in which zone entity is persistent.
		m_zoneHighltThrd.stop();

		long eType = m_pGenericDisplayCtrl->getZoneEntityCreationType();
		if (eType == 1) // kAnyAngleZone
		{
			addZoneEntity(istartX, istartY, iendX, iendY, m_lCurrentZoneHt, bCtrlPressed, bShiftPressed);
		}
		else if(eType == 2) // kRectZone
		{
			long nHeight = iendY - istartY;
			long nNewY = istartY + nHeight / 2;

			// Create a zone entity here after validating the requirements		
			addZoneEntity(istartX, nNewY, iendX, nNewY, nHeight, bCtrlPressed, bShiftPressed);
		}
	}
	else if(m_zoneAdjustmentThrd.isInteractiveZoneEntAdjustmentEnabled() && 
		!m_pGenericDisplayCtrl->isRubberbandingEnabled())
	{
		// Ensure zone is minimum width after adjustment
		if (!m_zoneAdjustmentThrd.isValid())
		{
			m_zoneAdjustmentThrd.cancel();
			AfxThrowOleDispatchException(0, 
				"ELI23477: Width of zone entity to create is less than minimum zone width.");
		}

		m_zoneAdjustmentThrd.stop();

		// No additional processing is required. The zone has already been adjusted.
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::OnKeyUpImageeditctrl1(short FAR* KeyCode, short Shift) 
{
	if (!UpdateInteractiveZoneCreation(*KeyCode))
	{
		// this message handler may not be really required to handle. Sometimes the 
		// lead image window does not send the KEY UP messages. This handler is an additional 
		// catch to the messages sent by Image edit control built on Lead tools
		m_pGenericDisplayCtrl->FireKeyUp((unsigned short*)KeyCode, Shift);
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::OnKeyDownImageeditctrl1(short FAR* KeyCode, short Shift) 
{
	// up/down/left/right arrow key up to move image
	short nKeyCode = *KeyCode;
	if (!UpdateInteractiveZoneCreation(*KeyCode))
	{
		// [LegacyRCAndUtils:5032] Don't fire key events in the display control (thereby disabling
		// shortcut keys) during rubberbanding or zone creation/modification events.
		if(m_zoneHighltThrd.IsInteractiveZoneEntCreationEnabled() ||
		   m_zoneAdjustmentThrd.isInteractiveZoneEntAdjustmentEnabled() ||
		   m_pGenericDisplayCtrl->isRubberbandingEnabled())
		{
			return;
		}

		if (scrollImage(getScrollDirectionType(nKeyCode)))
		{
			// if the key is processed, then no more process
			return;
		}
		
		// this message handler may not be really required to handle. Sometimes the 
		// lead image window does not send the KEY UP messages. This handler is an additional 
		// catch to the messages sent by Image edit control built on Lead tools
		m_pGenericDisplayCtrl->FireKeyDown((unsigned short*)KeyCode, Shift);
	}
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayView::UpdateInteractiveZoneCreation(short KeyCode) 
{			
	bool bRet = false;

	// if zone entity creation is enabled and user wants to increase/decrease
	// the zone height by pressing '+' or '-' keys, change the height if and only
	// if the new height doesn't go beyond the image bounds.
	if (m_pGenericDisplayCtrl->isZoneEntityCreationEnabled())
	{
		if (KeyCode == VK_ADD || KeyCode == 187)
		{
			// here the zone height is only to be increased if and only if
			// the zone boundaries with the updated highlight height will
			// not go beyond the image boundaries.
			CPoint * pvertexPts = NULL;

			// get the bounds for the new height to set
			pvertexPts = m_zoneHighltThrd.getZoneBoundsForThisHeight(m_lCurrentZoneHt+2) ; 

			// increment the zone height if all the 4 new bounds are withing image bounds
			if ((IsPtInImageBoundaries(pvertexPts[0].x, pvertexPts[0].y )) &&
				(IsPtInImageBoundaries(pvertexPts[1].x, pvertexPts[1].y )) &&
				(IsPtInImageBoundaries(pvertexPts[2].x, pvertexPts[2].y )) &&
				(IsPtInImageBoundaries(pvertexPts[3].x, pvertexPts[3].y)))
			{
				m_lCurrentZoneHt += 2;
				m_pGenericDisplayCtrl->setZoneHighlightHeight(m_lCurrentZoneHt);
				m_zoneHighltThrd.SetHighlightHeight(m_lCurrentZoneHt);
				m_zoneHighltThrd.notifyHighlightHtChanged(TRUE);
				m_zoneHighltThrd.Draw();
			}
			pvertexPts = NULL;

			bRet = true;
		}
		else if(KeyCode == VK_SUBTRACT || KeyCode == 189)
		{
			if (m_lCurrentZoneHt != 5)
			{
				m_lCurrentZoneHt -= 2;
				m_pGenericDisplayCtrl->setZoneHighlightHeight(m_lCurrentZoneHt);
				m_zoneHighltThrd.SetHighlightHeight(m_lCurrentZoneHt);
				m_zoneHighltThrd.notifyHighlightHtChanged(TRUE);
				m_zoneHighltThrd.Draw();
			}

			bRet = true;
		}
		else if (KeyCode == 81)		// character q or Q to quit zone creation
		{
			// user is not happy with the current zone highlight and would like to quit
			// the zone entity creation. So stop and reset the interactive zone creation process
			m_zoneHighltThrd.cancel();
			
			bRet = true;
		}
	}

	return bRet;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::OnDblClickEditctrl1() 
{
	m_pGenericDisplayCtrl->FireDblClick();
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayView::CheckValidImageType (CString zStr)
{
	//	Allowed Image types for Image Edit control
	char pszallowedTypes[NUMALLOWEDTYPES][4]	= {"AWD", "DCX", "BMP", "JPG", "PCX", "TIF", "XIF"};

	//	Get the file extension
	int iPos	= zStr.ReverseFind('.');
	CString zExt	= zStr.Mid(iPos+1);

	//	Change file extension to upper case
	zExt.MakeUpper();

	LPCTSTR	zImgType = LPCTSTR(zExt);
	
	//	check whether the image type is supported or not
	for(int i=0; i<NUMALLOWEDTYPES; i++)
	{
		if(strcmp(zImgType, pszallowedTypes[i]) == 0)
			return TRUE;
	}

	//	The image type is not Supported by Image edit control
	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ViewToImageInWorldCoordinates (int *iX, int *iY, long nPageNum)
{
	double iNewX;
	double iNewY;

	iNewX = *iX;
	iNewY = *iY;

	long lImgHeight;

	try
	{
		//	Scale the point
		lImgHeight = m_ImgEdit.GetImageHeight ();
		
		iNewX /= (m_dCurZoomFactor/100.0);
		iNewY /= (m_dCurZoomFactor/100.0);

		if (!m_bOnSizeInProgress)
		{
			m_iScrollX = m_ImgEdit.GetScrollPositionX();
			m_iScrollY = m_ImgEdit.GetScrollPositionY();
		}
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 

		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}

	//	Translate the point
	iNewX += (m_iScrollX/(m_dCurZoomFactor/100.0));
	iNewY += (m_iScrollY/(m_dCurZoomFactor/100.0));

	//	Change the origin to the bottom left corner of the image
	iNewY = lImgHeight - iNewY + m_iDeltaOriginY;
	iNewX += m_iDeltaOriginX;

	//	Apply Rotation to the point
	//	Get angle of the point
	double dPtAng = atan2(iNewY, iNewX);

	//	Get distance from the point to the origin
	double dDist = sqrt (iNewY * iNewY + iNewX * iNewX);

	//	Rotate the point with reverse angle of rotation
	double dRotAng = m_pGenericDisplayCtrl->getBaseRotation(nPageNum) * MathVars::PI / -180.0;
	double dCosAng = cos (dRotAng + dPtAng);
	double dSinAng = sin (dRotAng + dPtAng);

	*iX = (int)(dDist * dCosAng);
	*iY = (int)(dDist * dSinAng);

	return;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ImageInWorldToViewCoordinates (int *iX, int *iY, long nPageNum)
{
	double iNewX;
	double iNewY;

	iNewX = *iX;
	iNewY = *iY;

	//	Get angle of the point
	double dPtAng = atan2(iNewY, iNewX);

	//	Get distance from the point to the origin
	double dDist = sqrt (iNewY * iNewY + iNewX * iNewX);

	//	Rotate the point with  angle of rotation
	double dRotAng = m_pGenericDisplayCtrl->getBaseRotation(nPageNum) * MathVars::PI / 180.0;
	double dCosAng = cos (dRotAng + dPtAng);
	double dSinAng = sin (dRotAng + dPtAng);

	//	Get the rotated image coordinate
	iNewX = dDist * dCosAng;
	iNewY = dDist * dSinAng;

	long lImgHeight;

	try
	{
		//	Get image height
		lImgHeight = m_ImgEdit.GetImageHeight ();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 

		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}
	
	//	Move the Origin to the top left of the image
	iNewX -= m_iDeltaOriginX;
	iNewY = lImgHeight - iNewY + m_iDeltaOriginY;

	iNewX -= (m_iScrollX/(m_dCurZoomFactor/100.0));
	iNewY -= (m_iScrollY/(m_dCurZoomFactor/100.0));

	iNewX *= (m_dCurZoomFactor/100.0);
	iNewY *= (m_dCurZoomFactor/100.0);

	// TESTTHIS cast to int
	*iX = (int)iNewX;
	*iY = (int)iNewY;

	return;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::zoomCenter(double dCenterX, double dCenterY)
{
	CRect rectView;

	//	get the client rect
	GetClientRect(&rectView);

	//	convert the given point from view to image co-ordinates
	ImageInWorldToViewCoordinates(&dCenterX, &dCenterY, m_pGenericDisplayCtrl->getCurrentPageNumber());

	long lXDiff = 0, lYDiff = 0;

	//	find the difference between the current center and required center
	// TESTTHIS cast to long
	lXDiff = (long)(dCenterX - rectView.Width() / 2);
	lYDiff = (long)(dCenterY - rectView.Height() / 2);

	try
	{
		//	get the current scroll positions
		long lScrollX = m_ImgEdit.GetScrollPositionX();
		long lScrollY = m_ImgEdit.GetScrollPositionY();	

		//	set the scroll values
		lScrollX += lXDiff;
		lScrollY += lYDiff;
		
		//	set the scroll positions
		m_ImgEdit.SetScrollPositionX(lScrollX);
		m_ImgEdit.SetScrollPositionY(lScrollY);
		
		//	refresh the control
		m_ImgEdit.Refresh();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 

		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}

	try
	{
		//	get the current zoom factor
		m_dCurZoomFactor = m_ImgEdit.GetZoom();
		
		//	get the current scroll positions
		m_iScrollX = m_ImgEdit.GetScrollPositionX();
		m_iScrollY = m_ImgEdit.GetScrollPositionY();
		
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 

		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}

	//	draw the entities
	m_pGenericDisplayCtrl->DrawEntities (TRUE);

	//	Update Rubber band
	m_RBThrd.updateRubberBand ();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::setZoomFactor (double dZoomFactor)
{
	try
	{
		// TESTTHIS cast to long
		m_ImgEdit.SetZoom ((long)dZoomFactor);
		
		//	refresh the control
		m_ImgEdit.Refresh();
		
		//	get the current scroll positions
		m_iScrollX = 0;
		m_iScrollY = 0;
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 

		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}

	m_dCurZoomFactor = m_ImgEdit.GetZoom();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::setZoom(double dZoom)
{
	try
	{
		m_ImgEdit.SetZoom ((long)dZoom);
		
		//	refresh the control
		m_ImgEdit.Refresh();
		
		//	get the current scroll positions
		m_iScrollX = m_ImgEdit.GetScrollPositionX();
		m_iScrollY = m_ImgEdit.GetScrollPositionY();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 

		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}

	m_dCurZoomFactor = m_ImgEdit.GetZoom();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ViewToImageInWorldCoordinates (double *iX, double *iY, long nPageNum)
{
	double iNewX;
	double iNewY;

	iNewX = *iX;
	iNewY = *iY;

	long lImgHeight;

	try
	{
		// Scale the point
		lImgHeight = m_ImgEdit.GetImageHeight ();
		
		iNewX /= (m_dCurZoomFactor/100.0);
		iNewY /= (m_dCurZoomFactor/100.0);
				
		if (!m_bOnSizeInProgress)
		{
			m_iScrollX = m_ImgEdit.GetScrollPositionX();
			m_iScrollY = m_ImgEdit.GetScrollPositionY();
		}
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 

		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}

	//	Translate the point
	iNewX += (m_iScrollX/(m_dCurZoomFactor/100.0));
	iNewY += (m_iScrollY/(m_dCurZoomFactor/100.0));

	//	Change the origin to the bottom left corner of the image
	iNewY = lImgHeight - iNewY + m_iDeltaOriginY;
	iNewX += m_iDeltaOriginX;

	//	Apply Rotation to the point
	//	Get angle of the point
	double dPtAng = atan2(iNewY, iNewX);

	//	Get distance from the point to the origin
	double dDist = sqrt (iNewY * iNewY + iNewX * iNewX);

	//	Rotate the point with reverse angle of rotation
	double dRotAng = m_pGenericDisplayCtrl->getBaseRotation(nPageNum) * MathVars::PI / -180.0;
	double dCosAng = cos (dRotAng + dPtAng);
	double dSinAng = sin (dRotAng + dPtAng);

	*iX = dDist * dCosAng;
	*iY = dDist * dSinAng;

	return;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ImageInWorldToViewCoordinates (double *iX, double *iY, long nPageNum)
{
	double iNewX;
	double iNewY;

	iNewX = *iX;
	iNewY = *iY;

	//	Get angle of the point
	double dPtAng = atan2(iNewY, iNewX);

	//	Get distance from the point to the origin
	double dDist = sqrt (iNewY * iNewY + iNewX * iNewX);

	//	Rotate the point with angle of rotation
	double dRotAng = m_pGenericDisplayCtrl->getBaseRotation(nPageNum) * MathVars::PI / 180.0;
	double dCosAng = cos (dRotAng + dPtAng);
	double dSinAng = sin (dRotAng + dPtAng);

	//	Get the rotated image coordinate
	iNewX = dDist * dCosAng;
	iNewY = dDist * dSinAng;
	
	long lImgHeight;

	try
	{
		//	Get image height
		lImgHeight = m_ImgEdit.GetImageHeight ();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();

		AfxThrowOleDispatchException(0, str);
	}

	//	Move the Origin to the top left of the image
	iNewX -= m_iDeltaOriginX;
	iNewY = lImgHeight - iNewY + m_iDeltaOriginY;

	iNewX -= (m_iScrollX/(m_dCurZoomFactor/100.0));
	iNewY -= (m_iScrollY/(m_dCurZoomFactor/100.0));

	iNewX *= (m_dCurZoomFactor/100.0);
	iNewY *= (m_dCurZoomFactor/100.0);

	*iX = iNewX;
	*iY = iNewY;

	return;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::setCursorType(int x, int y)
{
	// Change cursor type if zone adjustment is enabled and we are not currently adjusting a zone
	if (m_pGenericDisplayCtrl->isZoneEntityAdjustmentEnabled() && 
		!m_zoneAdjustmentThrd.isInteractiveZoneEntAdjustmentEnabled())
	{
		HCURSOR hNewCursor = NULL;

		// Convert the point clicked to world coordinates
		double dXPos = x, dYPos = y;
		ViewToImageInWorldCoordinates(&dXPos, &dYPos, m_pGenericDisplayCtrl->getCurrentPageNumber());
		
		// Check if the mouse is over a grip handle
		EGripHandle gripHandle = m_pGenericDisplayCtrl->getGripHandle(dXPos, dYPos);
		switch (gripHandle)
		{
		case kLeft:
		case kRight:
			hNewCursor = m_hHorizontalResize;
			break;

		case kTop:
		case kBottom:
			hNewCursor = m_hVerticalResize;
			break;

		case kLeftTop:
		case kRightBottom:
			hNewCursor = m_hNwseResize;
			break;

		case kRightTop:
		case kLeftBottom:
			hNewCursor = m_hNeswResize;
			break;

		case kNone:
		default:
			// Get the nearest entity to the mouse up point 
			long lEntityID = m_pGenericDisplayCtrl->getNearestEntityIDFromPoint(dXPos, dYPos);

			// Set the cursor based on whether the mouse is over an entity
			if (lEntityID != 0)
			{
				// Use the move cursor
				hNewCursor = m_hMoveCursor;
			}

			break;
		}

		if (hNewCursor == NULL)
		{
			// Use the default cursor
			m_pGenericDisplayCtrl->restoreCursorHandle();
		}
		else
		{
			// Set the cursor
			m_ImgEdit.SetCursorHandle((long*)hNewCursor);
		}
	}
}
//-------------------------------------------------------------------------------------------------
LRESULT CGenericDisplayView::WindowProc(UINT message, WPARAM wParam, LPARAM lParam) 
{
	// what's the current shift state (1 - shift, 2 - Ctrl, 4 - Alt, etc)
	short shiftState = 0;
	shiftState += (::GetKeyState(VK_SHIFT) == -127 || ::GetKeyState(VK_SHIFT) == -128) ? 1 : 0;
	shiftState += (::GetKeyState(VK_CONTROL) == -127 || ::GetKeyState(VK_CONTROL) == -128) ? 2 : 0;
	shiftState += (::GetKeyState(VK_MENU) == -127 || ::GetKeyState(VK_MENU) == -128) ? 4 : 0;

	// analysis shows that when we click ( for that matter all window messages) in the GenericDisplay ocx control,
	// the message is received by image edit control and not by the view. To make the view to send the events,
	// this is the right place to check for the messages.
	switch (message)
	{		
		// if the message is WM_MOUSEWHEEL, fire the mouse wheel event
	case WM_MOUSEWHEEL:
		{
			// convert the lParam and wParam to long values so as to pass
			// them to the FireMouseWheel() event. The container of this OCX 
			// has to convert back the long values to WPARAM and LPARAM datatypes
			// to utilize them.
			m_pGenericDisplayCtrl->FireMouseWheel ((long)wParam, (long)lParam);

			break;
		}
	case WM_KEYDOWN:
	case WM_SYSKEYDOWN:
		{
			// up/down/left/right arrow key to move image
			unsigned short nKeyCode = (unsigned short)wParam;
			if (scrollImage(getScrollDirectionType(nKeyCode)))
			{
				// if the key is processed, then no more process
				break;
			}

			m_pGenericDisplayCtrl->FireKeyDown((unsigned short*)&wParam, shiftState);
			break;	
		}
	case WM_KEYUP:
	case WM_SYSKEYUP:
		{
			if (!UpdateInteractiveZoneCreation((short)wParam))
			{
				m_pGenericDisplayCtrl->FireKeyUp((unsigned short*)&wParam, shiftState);
			}
		}
		break;	
	case WM_CHAR:
		m_pGenericDisplayCtrl->FireKeyPress((unsigned short*)&wParam);
		break;			
	case WM_LBUTTONDBLCLK:
	case WM_MBUTTONDBLCLK:
	case WM_RBUTTONDBLCLK:
		m_pGenericDisplayCtrl->FireDblClick();
		break;	
	case WM_SETCURSOR:
		{
			//SetCursor(::LoadCursor(NULL, IDC_WAIT));
			return 0;
			break;
		}
	case WM_SIZE:
		
		if (!m_bCreate)
			return CView::WindowProc(message, wParam, lParam);
		
		// if no image is currently displayed, return the function.Otherwise
		// calling paint method with no image will cause serious problems (PVCS SCR#165)
		if (m_pGenericDisplayCtrl == NULL) 
			return CView::WindowProc(message, wParam, lParam);
		
		if (m_pGenericDisplayCtrl->GetImage().IsEmpty())
			return CView::WindowProc(message, wParam, lParam);
		
		// added by MSR
		
		// FIX to PVCS SCR#236 -- This is the problem with zone entities displacement when the control
		// is resized.It does only exist in Windows 2000. The root cause of the problem is scroll bar 
		// positions are not updated on Resize occassionally and there by resulting in displacement in
		// zone entities. This problem is fixed with the help of timer that sets the scroll bar poisitions
		
		// Hide the window to avoid painting problems 
		m_ImgEdit.ShowWindow(SW_HIDE);
			
		SetTimer(ID_REPAINT_ON_RESIZE, 0, NULL);
		
		// end of addition by MSR
	
		break;	
	}

	return CView::WindowProc(message, wParam, lParam);
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayView::IsPtInImageBoundaries(int x, int y)
{
	double dX = x, dY = y;
	ViewToImageInWorldCoordinates(&dX, &dY, m_pGenericDisplayCtrl->getCurrentPageNumber());
	
	// get the multiplication factors for both x and y directions
	double dXFactor = m_pGenericDisplayCtrl->getXMultFactor();
	double dYFactor = m_pGenericDisplayCtrl->getYMultFactor();

	// multiply the point with multiplication factors
	double dXPos = (double)dX * dXFactor;
	double dYPos = (double)dY * dYFactor;

	// get the width and height of the image in image co-ordinates
	// now dXFactor and dYFactor contains the image limits
	dXFactor *= m_iImgWidth;
	dYFactor *= m_iImgHeight;

	// if the cursor is within the image boundaries return true
	if ((dXPos>= 0.0 && dXPos<=dXFactor) && (dYPos>= 0.0 && dYPos<=dYFactor))
		return TRUE;

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ClientWindowToImagePixelCoordinates(int *iX, int *iY, long nPageNum)
{
	double dNewX;
	double dNewY;

	dNewX = *iX;
	dNewY = *iY;

	ViewToImageInWorldCoordinates(&dNewX, &dNewY, nPageNum);

	// reckon the height wrt top left corner as image pexel coordinates are wrt to top left
	dNewY = m_iImgHeight - dNewY;

	// TESTTHIS cast to int
	*iX = (int)dNewX;
	*iY = (int)dNewY;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ImagePixelToClientWindowCoordinates(int *iX, int *iY, long nPageNum)
{
	double dNewX;
	double dNewY;

	dNewX = *iX;
	dNewY = *iY;

	// translate the point to the origin
	dNewY = m_iImgHeight - dNewY;
	ImageInWorldToViewCoordinates(&dNewX, &dNewY, nPageNum);

	// TESTTHIS cast to int
	*iX = (int)dNewX;
	*iY = (int)dNewY;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ClientWindowToImagePixelCoordinates(double *dX, double *dY, long nPageNum)
{
	double dNewX;
	double dNewY;

	dNewX = *dX;
	dNewY = *dY;

	ViewToImageInWorldCoordinates(&dNewX, &dNewY, nPageNum);

	// reckon the height wrt top left corner as image pexel coordinates are wrt to top left
	dNewY = m_iImgHeight - dNewY;

	*dX = dNewX;
	*dY = dNewY;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ImagePixelToClientWindowCoordinates(double *dX, double *dY, long nPageNum)
{
	double dNewX;
	double dNewY;

	dNewX = *dX;
	dNewY = *dY;

	// translate the point to the origin
	dNewY = m_iImgHeight - dNewY;
	ImageInWorldToViewCoordinates(&dNewX, &dNewY, nPageNum);

	*dX = dNewX;
	*dY = dNewY;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ViewToRotatedImageCoordinates(double *dX, double *dY, long nPageNumber)
{
	double dNewX;
	double dNewY;

	dNewX = *dX;
	dNewY = *dY;

	long lImgHeight;

	try
	{
		// Scale the point
		lImgHeight = m_ImgEdit.GetImageHeight ();
		
		dNewX /= (m_dCurZoomFactor/100.0);
		dNewY /= (m_dCurZoomFactor/100.0);
		
		m_iScrollX = m_ImgEdit.GetScrollPositionX();
		m_iScrollY = m_ImgEdit.GetScrollPositionY();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 

		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}

	//	Translate the point
	dNewX += (m_iScrollX/(m_dCurZoomFactor/100.0));
	dNewY += (m_iScrollY/(m_dCurZoomFactor/100.0));

	//	Change the origin to the bottom left corner of the image
	dNewY = lImgHeight - dNewY;

	//	Apply Rotation to the point
	//	Get angle of the point
	double dPtAng = atan2(dNewY, dNewX);

	//	Get distance from the point to the origin
	double dDist = sqrt (dNewY * dNewY + dNewX * dNewX);

	//	Rotate the point with reverse angle of rotation
	double dRotAng = 0.0; 
	double dCosAng = cos (dRotAng + dPtAng);
	double dSinAng = sin (dRotAng + dPtAng);

	*dX = dDist * dCosAng;
	*dY = dDist * dSinAng;
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayView::OnMouseWheel(UINT nFlags, short zDelta, CPoint pt) 
{
	//	Check for the mouse pointer as it is in the client area or not.
	CRect ImageEditRect;

	//	get the image window rectangle.
	m_ImgEdit.GetWindowRect(ImageEditRect);

	//	check for the mouse point is in window rect or not.
	if( !ImageEditRect.PtInRect( pt ) )
	{
		return CView::OnMouseWheel(nFlags, zDelta, pt);
	}

	// check if the left button is pressed
	if((nFlags & MK_LBUTTON) != 0)
	{	
		// To prevent the CUCDBitmapWindow's base class from visibly scrolling the image during
		// mousewheel interactive zone creation, the vertical line step is set to zero such 
		// that CUCDBitmapWindow scrolls the image by zero. A side effect of this fix [P16 #2338] 
		// is that the previous zone highlights and interactive zone have been cleared.
		// UpdateInteractiveZoneCreation will XOR the previous location of the zone, creating a 
		// new "phantom" zone. To prevent this from happening, set the previous end points to zero 
		// to signal that the previous zone was already cleared. [P16 #2923]
		m_zoneHighltThrd.SetPrevEndPoint(0,0);

		// redraw zone highlights
		m_pGenericDisplayCtrl->DrawEntities(TRUE);

		// if the zone creation is enabled, the zone height needs to be increased/decreased.		
		if (zDelta > 0)
		{
			UpdateInteractiveZoneCreation(VK_ADD);
		}
		else
		{
			UpdateInteractiveZoneCreation(VK_SUBTRACT);
		}

		return TRUE;
	}
	else if (!m_zoneHighltThrd.IsInteractiveZoneEntCreationEnabled() && 
		!m_zoneAdjustmentThrd.isInteractiveZoneEntAdjustmentEnabled())
	{
		// the left mouse button is not clicked. handle a scroll or zoom event.

		// Zoom
		if ((nFlags & MK_CONTROL))
		{
			if (zDelta < 0)
			{
				ZoomOutAroundTheClickedPoint(pt.x, pt.y);
			}
			else
			{
				ZoomInAroundTheClickedPoint(pt.x, pt.y);
			}
			return TRUE;
		}

		// Horizontal scroll
		if (nFlags & MK_SHIFT)
		{
			return scrollImage(zDelta < 0 ? kScrollRight : kScrollLeft);
		}

		// Vertical scroll
		if (nFlags == 0)
		{
			return scrollImage(zDelta < 0 ? kScrollDown : kScrollUp);
		}
	}

	return CView::OnMouseWheel(nFlags, zDelta, pt);
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::OnTimer(UINT nIDEvent) 
{
	if (nIDEvent == ID_REPAINT_ON_RESIZE)
	{
		//Invalidate();

		try
		{

			m_ImgEdit.SetScrollPositionX (m_iScrollX);
			m_ImgEdit.SetScrollPositionY (m_iScrollY);
			OnScrollEditctrl1();
			// Image edit window was set to Hide. Show the window now
			m_ImgEdit.ShowWindow(SW_SHOW);
		}
		catch(COleDispatchException* ptr)
		{
			CString str = ptr->m_strDescription; 
			
			ptr->Delete();

			AfxThrowOleDispatchException(0, str);
		}

		m_bOnSizeInProgress = false;

		KillTimer(ID_REPAINT_ON_RESIZE);
	}
	
	CView::OnTimer(nIDEvent);
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::setPercentChangeInMagnification(double dperctMagnify)
{
	m_dPercentMagnify = dperctMagnify;

	try
	{
		// TESTTHIS cast to long
		m_ImgEdit.SetZoomMagnifyFactor((long)(100 + m_dPercentMagnify));
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 

		ptr->Delete();

		AfxThrowOleDispatchException(0, str);
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::setBaseRotation(double dBaseRotation)
{
	try
	{
		m_ImgEdit.SetBaseRotation(dBaseRotation);
		FitToParent(true);
		m_ImgEdit.Refresh();

		// get current zoom factor
		m_dCurZoomFactor = m_ImgEdit.GetZoom();
		
		//	get the current scroll positions
		m_iScrollX = m_ImgEdit.GetScrollPositionX();
		m_iScrollY = m_ImgEdit.GetScrollPositionY();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		
		ptr->Delete();

		AfxThrowOleDispatchException(0, str);
	}
	
	if (dBaseRotation == 0.0)
	{
		setDeltaOriginX(0);
		setDeltaOriginY(0);
	}
	else
	{		
		// Compute the new origin location
		double dCosVal = cos(dBaseRotation* MathVars::PI / 180.0);
		double dSinVal = sin(dBaseRotation* MathVars::PI / 180.0);
		
		// when an image is rotated, origin will be shifted or rotated
		// based on the value of rotation. New corners of the rotated image
		// are set in the values of x1,y1 to x4,y4. These 4 corner values are 
		// utilized for the calculation of shift in the origin.
		int x1 = 0;
		int y1 = 0;
		
		// TESTTHIS cast to int
		int x2 = (int)(m_iImgWidth * dCosVal);
		int y2 = (int)(m_iImgWidth * dSinVal);
		
		int x3 = (int)(x2 - m_iImgHeight * dSinVal);
		int y3 = (int)(y2 + m_iImgHeight * dCosVal);
		
		int x4 = (int)(m_iImgHeight * dSinVal * (-1.0));
		int y4 = (int)(m_iImgHeight * dCosVal);
		
		setDeltaOriginX(min (min (x2, x3), x4));
		setDeltaOriginY(min (min (y2, y3), y4));
		
		// it is observed that when rotation is positive (Couter Clockwise rotation) and the rotation value
		// is less than or equal to 90 degrees, the minimum of y2,y3 and y4 is greater than y1 and the actual 
		// minimum is y1.So set the  m_iDeltaOriginY equals to zero. The similar case for minimum X value
		// is in 3rd quadrant.
		
		// When it is negative rotation the minimum X and Y value are to be set as zero in 
		// 1st and 4th quadrants respectively
		if (dBaseRotation > 0)
		{
			if (dBaseRotation <= 90.0)
				setDeltaOriginY(0);
			
			if (dBaseRotation > 270.0 && dBaseRotation <= 360)
				setDeltaOriginX(0);
		}
		else if(dBaseRotation < 0)
		{
			if (dBaseRotation <= -270.0 && dBaseRotation >= -360.0)
				setDeltaOriginY(0);
			
			if (dBaseRotation<0 && dBaseRotation>= -90.0)
				setDeltaOriginX(0);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::FitToParent(bool bFitPage)
{
	m_ImgEdit.FitToParent(bFitPage);

	// Get the zoom percent according different fit-to status
	if (bFitPage)
	{
		m_dZoomPercentOnExtents = m_ImgEdit.GetZoom();
	}
	else
	{
		m_dZoomPercentOnFitToWidth = m_ImgEdit.GetZoom();
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::HideImageEditWindow()
{
	//	Hide the Image Edit window.
	m_ImgEdit.ShowWindow(SW_HIDE);
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::ShowImageEditWindow()
{
	//	Show the Image Edit window
	m_ImgEdit.ShowWindow(SW_SHOW);
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
CGenericDisplayView::EScrollDirection CGenericDisplayView::getScrollDirectionType(int nVirtualKeyCode)
{
	EScrollDirection eScrollDir = kNoScroll;
	
	switch (nVirtualKeyCode)
	{
	case VK_LEFT:
		eScrollDir = kScrollLeft;
		break;
	case VK_RIGHT:
		eScrollDir = kScrollRight;
		break;
	case VK_UP:
		eScrollDir = kScrollUp;
		break;
	case VK_DOWN:
		eScrollDir = kScrollDown;
		break;
	}

	return eScrollDir;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::limitPoint(long& nX, long& nY)
{
	if (!IsPtInImageBoundaries(nX, nY))
	{
		int nMaxX1 = m_iImgWidth;
		int nMaxY1 = m_iImgHeight;
		int nMaxX2 = 0;
		int nMaxY2 = 0;
		ImagePixelToClientWindowCoordinates(&nMaxX1, &nMaxY1, m_pGenericDisplayCtrl->getCurrentPageNumber());
		ImagePixelToClientWindowCoordinates(&nMaxX2, &nMaxY2, m_pGenericDisplayCtrl->getCurrentPageNumber());
		
		// get the biggest number
		int nMaxX = max(nMaxX1, nMaxX2);
		int nMaxY = max(nMaxY1, nMaxY2);
		
		if (nX > nMaxX)
		{
			nX = nMaxX;
		}
		
		if (nY > nMaxY)
		{
			nY = nMaxY;
		}
	}
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayView::scrollImage(EScrollDirection eScrollDirection)
{
	// disable image edit's base class from handling mousewheel events [P16 #2338]
	m_ImgEdit.EnableVerticalScroll(FALSE);

	// default to left
	int nScrollType = SB_HORZ;
	int nScrollCode = SB_LINELEFT;
	UINT scrollMessage = WM_HSCROLL;

	switch (eScrollDirection)
	{
	case kScrollLeft:
		break;
	case kScrollRight:
			nScrollCode = SB_LINERIGHT;
		break;
	case kScrollUp:
		{
			nScrollType = SB_VERT;
			nScrollCode = SB_LINEUP;
			scrollMessage = WM_VSCROLL;
			m_ImgEdit.EnableVerticalScroll(TRUE);
		}
		break;
	case kScrollDown:
		{
			nScrollType = SB_VERT;
			nScrollCode = SB_LINEDOWN;
			scrollMessage = WM_VSCROLL;
			m_ImgEdit.EnableVerticalScroll(TRUE);
		}
		break;
	default:
		// no scroll
		return FALSE;
	}

	// send windows message to image edit to scroll the image
	int minpos = 0, maxpos = 0;
	m_ImgEdit.GetScrollRange(nScrollType, &minpos, &maxpos);
	
	// current position of the scroll thumb
	int curpos = m_ImgEdit.GetScrollPos(nScrollType);
	
	// only scroll if the current position of the thumb
	// is within the scroll bar range
	if (curpos <= maxpos && curpos >= minpos)
	{
		try
		{
			// send message to scroll bar to scroll a unit
			m_ImgEdit.SendMessage(scrollMessage, nScrollCode, NULL);
			
			// Set the new position of the thumb (scroll box).
			m_iScrollX	= m_ImgEdit.GetScrollPositionX();
			m_iScrollY	= m_ImgEdit.GetScrollPositionY();
			
			m_ImgEdit.Refresh();
			
		}
		catch(COleDispatchException* ptr)
		{
			CString str = ptr->m_strDescription; 
			
			ptr->Delete();
			
			AfxThrowOleDispatchException(0, str);
		}
		
		// draw all entities
		m_pGenericDisplayCtrl->DrawEntities(TRUE);
		m_RBThrd.updateRubberBand();

		// disable image edit's base class from handling mousewheel events [P16 #2338]
		m_ImgEdit.EnableVerticalScroll(FALSE);

		return TRUE;
	}

	// disable image edit's base class from handling mousewheel events [P16 #2338]
	m_ImgEdit.EnableVerticalScroll(FALSE);

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::addZoneEntity(int iStartX, int iStartY, int iEndX, int iEndY, 
										long lHeight, bool bCtrlPressed, bool bShiftPressed)
{
	// Get the fitting mode (0 = Never, 1 = OnCtrlDown, 2 = Always)
	long fittingMode = m_pGenericDisplayCtrl->getFittingMode();

	// Check if auto fitting is enabled
	long lEntityID = 0;
	if (fittingMode > 0 && ((fittingMode == 2) ^ bCtrlPressed))
	{
		// Perform block fitting or line fitting
		if (bShiftPressed)
		{
			// Block fit
			lEntityID = blockFit(iStartX, iStartY, iEndX, iEndY, lHeight);
		}
		else
		{
			// Line fit
			lineFit(iStartX, iStartY, iEndX, iEndY, lHeight);

			// Line fitting raises its own events. We are done.
			return;
		}
	}
	else
	{
		// Create the zone entity
		lEntityID = addZoneEntity(iStartX, iStartY, iEndX, iEndY, lHeight);
	}

	// Fire an event that zone entity is created
	if (lEntityID != 0)
	{
		IVariantVectorPtr ipVariantVector(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI23769", ipVariantVector != __nullptr);
		_variant_t var(lEntityID);
		ipVariantVector->PushBack(var);
		m_pGenericDisplayCtrl->FireZoneEntitiesCreated(ipVariantVector);
	}
	else
	{
		AfxThrowOleDispatchException(0, "ELI90046: Error in zone entity creation.");
	}
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayView::addZoneEntity(int iStartX, int iStartY, int iEndX, int iEndY, long lHeight)
{
	// Height must be positive
	if (lHeight < 0)
	{
		lHeight *= -1;
	}

	// Height must be odd
	if (lHeight % 2 == 0)
	{
		lHeight++;
	}

	// Create a zone entity and return the ID
	return m_pGenericDisplayCtrl->addZoneEntity((unsigned long)iStartX, (unsigned long)iStartY, 
		(unsigned long)iEndX, (unsigned long)iEndY, lHeight, 
		m_pGenericDisplayCtrl->getCurrentPageNumber(), TRUE, FALSE, RGB(0,0,0));	
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayView::blockFit(int iStartX, int iStartY, int iEndX, int iEndY, long lHeight)
{
	// TODO: Lazy instantiation of bitmap?
	string imageName = asString(m_pGenericDisplayCtrl->getImageName());
	LeadToolsBitmap bitmap(imageName, m_pGenericDisplayCtrl->getCurrentPageNumber());

	// Get the fitting data
	FittingData data = getFittingData(iStartX, iStartY, iEndX, iEndY, lHeight);

	// Complete the block fitting
	return blockFit(data.leftTop, data.rightBottom, data.theta, bitmap);
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayView::blockFit(Point &leftTop, Point &rightBottom, Point theta, 
								   LeadToolsBitmap &bitmap)
{
	// Shrink each side of the highlight
	shrinkLeftSide(leftTop, rightBottom, theta, bitmap);
	shrinkTopSide(leftTop, rightBottom, theta, bitmap);
	shrinkRightSide(leftTop, rightBottom, theta, bitmap);
	shrinkBottomSide(leftTop, rightBottom, theta, bitmap);

	// Find the corners of the fitted highlight
	Point point1 = rotate(leftTop, theta);
	Point point2 = rotate(leftTop.dXPos, rightBottom.dYPos, theta);
	Point point3 = rotate(rightBottom, theta);
	Point point4 = rotate(rightBottom.dXPos, leftTop.dYPos, theta);

	// Create the zone entity and return the ID
	return addZoneEntity(
		(int)((point1.dXPos + point2.dXPos) / 2), (int)((point1.dYPos + point2.dYPos) / 2), 
		(int)((point3.dXPos + point4.dXPos) / 2), (int)((point3.dYPos + point4.dYPos) / 2), 
		(long)(rightBottom.dYPos - leftTop.dYPos));
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::lineFit(int iStartX, int iStartY, int iEndX, int iEndY, long lHeight)
{
	// TODO: Lazy instantiation of bitmap?
	string imageName = asString(m_pGenericDisplayCtrl->getImageName());
	LeadToolsBitmap bitmap(imageName, m_pGenericDisplayCtrl->getCurrentPageNumber());

	// Get the fitting data
	FittingData data = getFittingData(iStartX, iStartY, iEndX, iEndY, lHeight);

	// Trim whitespace at the top and bottom
	shrinkLeftSide(data.leftTop, data.rightBottom, data.theta, bitmap);
	shrinkTopSide(data.leftTop, data.rightBottom, data.theta, bitmap);
	shrinkRightSide(data.leftTop, data.rightBottom, data.theta, bitmap);
	shrinkBottomSide(data.leftTop, data.rightBottom, data.theta, bitmap);

	// Attempt to split the highlight as many times as possible
	vector<long> entityIDs;
	double dY = findRowToSplit(data.leftTop, data.rightBottom, data.theta, bitmap);
	while (dY > 0)
	{
		// Create the zone entity
		Point leftTop(data.leftTop);
		Point rightBottom(data.rightBottom.dXPos, dY);
		long lEntityID = blockFit(leftTop, rightBottom, data.theta, bitmap);

		// Check if there was an error
		if (lEntityID < 0)
		{
			// This is a non-serious logic error. End the loop to 
			// create the highlight without any further line fitting.
			break;
		}

		// Store the entity ID
		entityIDs.push_back(lEntityID);

		// Store the new highlight's top
		data.leftTop.dYPos = dY + 1;

		// Trim whitespace from the top
		shrinkTopSide(data.leftTop, data.rightBottom, data.theta, bitmap);

		// Look for a place to split the highlight
		dY = findRowToSplit(data.leftTop, data.rightBottom, data.theta, bitmap);
	}

	// Create the last highlight and store the ID
	long lEntityID = blockFit(data.leftTop, data.rightBottom, data.theta, bitmap);
	if (lEntityID > 0)
	{
		entityIDs.push_back(lEntityID);
	}
	else
	{
		// If no highlights were created, throw an exception
		if (entityIDs.size() == 0)
		{
			AfxThrowOleDispatchException(0, "ELI23757: Could not create line fitted highlight.");
		}
	}

	// Create a variant vector with the zone ids
	IVariantVectorPtr ipZoneIDs(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI23765", ipZoneIDs != __nullptr);
	for (unsigned int i = 0; i < entityIDs.size(); i++)
	{
		_variant_t id(entityIDs[i]);
		ipZoneIDs->PushBack(id);
	}

	// Fire the creation event
	m_pGenericDisplayCtrl->FireZoneEntitiesCreated(ipZoneIDs);
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::shrinkLeftSide(Point &leftTop, Point &rightBottom, Point theta, 
										 LeadToolsBitmap &bitmap)
{
	// Iterate over each column, checking if the left side can be shrunk by one pixel
	while (leftTop.dXPos < rightBottom.dXPos)
	{
		// Iterate over each pixel in this column
		for (double dY = leftTop.dYPos; dY <= rightBottom.dYPos; dY++)
		{
			// Check whether this pixel is black
			if (isBlackPixel(leftTop.dXPos, dY, theta, bitmap))
			{
				// We are done.
				return;
			}
		}

		leftTop.dXPos++;
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::shrinkRightSide(Point &leftTop, Point &rightBottom, Point theta, 
										  LeadToolsBitmap &bitmap)
{
	// Iterate over each column, checking if the right side can be shrunk by one pixel
	while (rightBottom.dXPos > leftTop.dXPos)
	{
		// Iterate over each pixel in this column
		for (double dY = leftTop.dYPos; dY <= rightBottom.dYPos; dY++)
		{
			// Check whether this pixel is black
			if (isBlackPixel(rightBottom.dXPos, dY, theta, bitmap))
			{
				// We are done.
				return;
			}
		}

		rightBottom.dXPos--;
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::shrinkTopSide(Point &leftTop, Point &rightBottom, Point theta, 
										LeadToolsBitmap &bitmap)
{
	// Iterate over each row, checking if the top side can be shrunk by one pixel
	while (leftTop.dYPos < rightBottom.dYPos)
	{

		// Check whether this row contains a black pixel
		if (rowContainsBlackPixel(leftTop.dXPos, leftTop.dYPos, rightBottom.dXPos, theta, bitmap))
		{
			// We are done.
			return;
		}

		leftTop.dYPos++;
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayView::shrinkBottomSide(Point &leftTop, Point &rightBottom, Point theta, 
										   LeadToolsBitmap &bitmap)
{
	// Iterate over each row, checking if the bottom side can be shrunk by one pixel
	while (rightBottom.dYPos > leftTop.dYPos)
	{
		// Check whether this row contains a black pixel
		if (rowContainsBlackPixel(leftTop.dXPos, rightBottom.dYPos, rightBottom.dXPos, theta, bitmap))
		{
			// We are done.
			return;
		}

		rightBottom.dYPos--;
	}
}
//-------------------------------------------------------------------------------------------------
double CGenericDisplayView::getAngleFromHorizontal(double dAngle)
{
	if (dAngle >= MathVars::PI / 2)
	{
		return dAngle - MathVars::PI;
	}
	else if (dAngle < -MathVars::PI / 2)
	{
		return dAngle + MathVars::PI;
	}

	return dAngle;
}
//-------------------------------------------------------------------------------------------------
Point CGenericDisplayView::rotate(Point point, Point theta)
{
	return rotate(point.dXPos, point.dYPos, theta.dXPos, theta.dYPos);
}
//-------------------------------------------------------------------------------------------------
Point CGenericDisplayView::rotate(Point point, double sinTheta, double cosTheta)
{
	return rotate(point.dXPos, point.dYPos, sinTheta, cosTheta);
}
//-------------------------------------------------------------------------------------------------
Point CGenericDisplayView::rotate(double x, double y, Point theta)
{
	return rotate(x, y, theta.dXPos, theta.dYPos);
}
//-------------------------------------------------------------------------------------------------
Point CGenericDisplayView::rotate(double x, double y, double sinTheta, double cosTheta)
{
	return Point(x*cosTheta - y*sinTheta, x*sinTheta + y*cosTheta);
}
//-------------------------------------------------------------------------------------------------
CGenericDisplayView::FittingData CGenericDisplayView::getFittingData(
	int iStartX, int iStartY, int iEndX, int iEndY, long lHeight)
{
	// Calculate the angle of the line dy/dx
	double dAngle = atan2((double)(iEndY - iStartY), (double)(iEndX  - iStartX));

	// Express the angle from horizontal as a number between -PI/2 and PI/2 [LegacyRCAndUtils #5205]
	dAngle = getAngleFromHorizontal(dAngle);

	// Calculate the horizontal and vertical increments
	Point theta(sin(dAngle), cos(dAngle));

	// Calculate the x and y modifiers
	Point modifier((lHeight / 2) * theta.dXPos, (lHeight / 2) * theta.dYPos);

	// Calculate opposing corners of the highlight as Points
	Point leftTop(iStartX - modifier.dXPos, iStartY + modifier.dYPos);
	Point rightBottom(iEndX + modifier.dXPos, iEndY - modifier.dYPos);

	// Thetas should be relative to the side that is closest to the viewer's horizontal. 
	// [LegacyRCAndUtils #5066]
	long lPage = m_pGenericDisplayCtrl->getCurrentPageNumber();
	dAngle += m_pGenericDisplayCtrl->getBaseRotation(lPage) * MathVars::PI / 180.0;

	// Express the angle from horizontal as a number between -PI/2 and PI/2 [LegacyRCAndUtils #5205]
	dAngle = getAngleFromHorizontal(dAngle);

	// Calculate the thetas
	theta.dXPos = sin(dAngle);
	theta.dYPos = cos(dAngle);

	// Calculate the min/max x/y of the deskewed highlight
	Point negativeTheta(sin(-dAngle), cos(-dAngle));
	leftTop = rotate(leftTop, negativeTheta);
	rightBottom = rotate(rightBottom, negativeTheta);

	if (leftTop.dXPos > rightBottom.dXPos)
	{
		swap(leftTop.dXPos, rightBottom.dXPos);
	}
	if (leftTop.dYPos > rightBottom.dYPos)
	{
		swap(leftTop.dYPos, rightBottom.dYPos);
	}

	// Return the fitting data
	FittingData data = {leftTop, rightBottom, theta};
	return data;
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayView::rowContainsBlackPixel(double dLeft, double dTop, double dRight, 
												Point theta, LeadToolsBitmap &bitmap)
{
	for (double dX = dLeft; dX <= dRight; dX++)
	{
		// Check whether this pixel is black
		if(isBlackPixel(dX, dTop, theta, bitmap))
		{
			return true;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayView::isBlackPixel(double dX, double dY, Point theta, LeadToolsBitmap &bitmap)
{
	Point pixel = rotate(dX, dY, theta);
	CPoint point((int)pixel.dXPos, (int)pixel.dYPos);
	return bitmap.contains(point) && bitmap.isPixelBlack(point);
}
//-------------------------------------------------------------------------------------------------
double CGenericDisplayView::findRowToSplit(Point leftTop, Point rightBottom, Point theta, 
										   LeadToolsBitmap &bitmap)
{
	// Starting at the minimum split height, find the first row containing a black pixel
	double dRow = leftTop.dYPos + MIN_SPLIT_HEIGHT - 2;
	do
	{
		dRow++;

		// Ensure both highlights meet the minimum splitting height
		if (dRow >= rightBottom.dYPos - MIN_SPLIT_HEIGHT)
		{
			return -1;
		}
	}
	while (!rowContainsBlackPixel(leftTop.dXPos, dRow, rightBottom.dXPos, theta, bitmap));

	// Look for the next row of white pixels
	do
	{
		dRow++;

		// Ensure both highlights meet the minimum splitting height
		if (dRow > rightBottom.dYPos - MIN_SPLIT_HEIGHT)
		{
			return -1;
		}
	}
	while (rowContainsBlackPixel(leftTop.dXPos, dRow, rightBottom.dXPos, theta, bitmap));

	// Store this point. This is the row to split if minimum requirements have been met.
	double dRowToSplit = dRow;

	// Starting at the minimum split height for the second highlight, 
	// find the first row containing a black pixel.
	dRow = rightBottom.dYPos - MIN_SPLIT_HEIGHT + 2;
	do
	{
		dRow--;

		// Ensure both highlights meet the minimum splitting height
		if (dRow <= dRowToSplit)
		{
			return -1;
		}
	}
	while (!rowContainsBlackPixel(leftTop.dXPos, dRow, rightBottom.dXPos, theta, bitmap));

	// If we reached this point, we have found a row that can split the highlight
	return dRowToSplit;
}
//-------------------------------------------------------------------------------------------------

