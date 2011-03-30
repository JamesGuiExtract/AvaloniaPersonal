//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ImageEditCtl.h
//
// PURPOSE:	The purpose of this file is to implement the interface that needs to be supported by
//			the Image Edit OLE control.  This is an header file for CImageEditCtrl 
//			where this class has been derived from the COleControl()class.  
//			The code written in this file makes it possible to implement the various methods.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================

#include <ltwrappr.h>
#include "CUCDBitmapWindow.h"
#include "ImageEditCfg.h"
#include <string>
// ImageEditCtl.h : Declaration of the CImageEditCtrl ActiveX Control class.

/////////////////////////////////////////////////////////////////////////////
// CImageEditCtrl : See ImageEditCtl.cpp for implementation.
//==================================================================================================
//
// CLASS:	ImageEdit
//
// PURPOSE:	To provide all the functionality of a simple and generic CAD-type display window.
//
// REQUIRE:	Nothing.
// 
// INVARIANTS:
//			This class will provide the functionality like Image Edit control.  This will give
//			some of the like functionalities.
//
// EXTENSIONS:
//			None.
//
// NOTES:	
//
class CImageEditCtrl : public COleControl
{
	DECLARE_DYNCREATE(CImageEditCtrl)

// Constructor
public:
	CImageEditCtrl();

	// the instance of Lead tools window. image is loaded into this window. All image
	// functions like zoom, rotation is supported by this window
	CUCDBitmapWindow 	m_LeadWnd;

	//----------------------------------------------------------------------------------------------
	// PURPOSE: Fires the scroll bar movement.
	// REQUIRE: Scroll bars should exist.
	// PROMISE: Control class's scroll moment will fire.
	void FireScrollFromCtrlClass();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Fires the mouse wheel event.
	// PROMISE: Control class's mouse wheel event will fire.
	void FireMouseWheelFromCtrlClass(long wParam, long lParam);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: check function for image.
	// REQUIRE: Nothing
	// PROMISE: Checks the application is having any background image or not.
	void CheckForImage(CString zFunctionName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To convert the coordinates from View to Image
	// REQUIRE: X and Y coordinate values
	// PROMISE: To convert the given X and Y coordinate values from
	//			View co ordinates to Image coordinates.  
	void ViewToImageCoordinates (double *dX, double *dY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To convert the coordinates from Image to View
	// REQUIRE: X and Y coordinate values
	// PROMISE: To convert the given X and Y coordinate values from
	//			Image coordinates to View coordinates.
	void ImageToViewCoordinates (double *dX, double *dY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: to update the zoom factor on load image
	// REQUIRE: Image must have been loaded
	// PROMISE: update by calculating the zoom factor
	void UpdateTheZoomOnImageLoad(BOOL bFitPage);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: The zoom factor may change whenever control size is changed but definitely 
	//          the zoom that fits in client rect for width and height changes. Update all
	//			these zoom factors. All these zoom factors contributes to calculate the zoom 
	//			while zoomIn and zoomOut dynamically.
	// REQUIRE: Image must have been loaded
	// PROMISE: Calculates all the zoom factors on ctrl size change.
	void UpdateTheZoomOnSize ();
	//----------------------------------------------------------------------------------------------
	
private:
	//----------------------------------------------------------------------------------------------
	void translateOriginalImagePoint(long& rnX, long& rnY);
	//----------------------------------------------------------------------------------------------
	// PROMSE: Throws an exception if the iLeadToolsRetCode corresponds to a LeadTools error.
	//         Otherwise, completes successfully.
	void throwOnLeadToolsError(const char* pszELICode, const L_INT iLeadToolsRetCode);

	//	Image File Path.
	CString			m_zImageName;

	//	Current percentage of zoom
	double				m_dZoomPercent;

	//	Indicates whether the image is fit to parent
	bool			m_bFitToParent;

	// indicates whether image is currently loaded into the control
	bool			m_bImageLoaded;

	// lead tools data structure contains loaded file info.
	FILEINFO		m_fInfo;

	//	Base Rotation
	double			m_dBaseRotation;

	//	Set currently displayed page number
	L_INT	m_dCurPageNumber;

	// image page count. More than 1 in case of Multi page images
	int m_iPageCount;

	//	currently displayed image
	bool m_bDisplayImage;

	//	sets image height and width
	long	m_lImageHeight;
	long	m_lImageWidth;

	// whether release is captured
	bool	m_bReleaseCapture;

	// zoom values for fitting the width of the image
	// or for image height
	double m_dZoomFitHeight;
	double m_dZoomFitWidth;

	// the width and height of the ImageEdit control on initilisation
	double m_dRectWidth;
	double m_dRectHeight; 

	// current zoom magnifying factor like 1.2
	double m_dZoomMagnifyFactor;

	// Configuration object
	std::unique_ptr<ImageEditCtrlCfg> m_apSettings;

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CImageEditCtrl)
public:
	virtual void OnDraw(CDC* pdc, const CRect& rcBounds, const CRect& rcInvalid);
	virtual void DoPropExchange(CPropExchange* pPX);
	virtual void OnResetState();
	virtual BOOL Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CCreateContext* pContext = NULL);
	//}}AFX_VIRTUAL

// Implementation
protected:
	~CImageEditCtrl();

	DECLARE_OLECREATE_EX(CImageEditCtrl)    // Class factory and guid
	DECLARE_OLETYPELIB(CImageEditCtrl)      // GetTypeInfo
	DECLARE_PROPPAGEIDS(CImageEditCtrl)     // Property page IDs
	DECLARE_OLECTLTYPE(CImageEditCtrl)		// Type name and misc status

// Message maps
	//{{AFX_MSG(CImageEditCtrl)
	afx_msg BOOL OnMouseWheel(UINT nFlags, short zDelta, CPoint pt);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg BOOL OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

// Dispatch maps
	//{{AFX_DISPATCH(CImageEditCtrl)
	afx_msg long GetPage();
	afx_msg void SetPage(long nNewValue);
	afx_msg long GetScrollPositionX();
	afx_msg void SetScrollPositionX(long nNewValue);
	afx_msg long GetScrollPositionY();
	afx_msg void SetScrollPositionY(long nNewValue);
	afx_msg long GetMousePointer();
	afx_msg void SetMousePointer(long nNewValue);
	afx_msg double GetBaseRotation();
	afx_msg void SetBaseRotation(double newValue);
	afx_msg void SetImage(LPCTSTR szImagePath);
	afx_msg void Display();
	afx_msg void ClearDisplay();
	afx_msg void LockLeadWndUpdate();
	afx_msg void UnlockLeadWndUpdate();
	afx_msg void Scroll(short iType, long iDistance);
	afx_msg long GetImageWidth();
	afx_msg long GetPageCount();
	afx_msg long GetImageHeight();
	afx_msg BSTR GetImage();
	afx_msg void FitToParent(BOOL bFitPage);
	afx_msg void DrawSelectionRect(long Left, long Top, long Right, long Bottom);
	afx_msg long GetXResolution();
	afx_msg long GetYResolution();
	afx_msg void ZoomInAroundPoint(long PosX, long PosY);
	afx_msg void ZoomOutAroundPoint(long PosX, long PosY);
	afx_msg void SetZoomMagnifyFactor(long lPercentMagnify);
	afx_msg long GetZoomMagnifyFactor();
	afx_msg double GetZoom();
	afx_msg void SetZoom(long newValue);
	afx_msg void SetCursorHandle(OLE_HANDLE* hCursor);
	afx_msg void ExtractZoneImage(long nX1, long nY1, long nX2, long nY2, long nHeight, long nPageNum, LPCTSTR strFileName);
	afx_msg void EnableVerticalScroll(BOOL bEnable);
	afx_msg void RefreshRect(long lLeft, long lTop, long lRight, long lBottom);
	afx_msg void Refresh();
	//}}AFX_DISPATCH
	DECLARE_DISPATCH_MAP()

	afx_msg void AboutBox();

// Event maps
	//{{AFX_EVENT(CImageEditCtrl)
	void FireScroll()
		{FireEvent(eventidScroll,EVENT_PARAM(VTS_NONE));}
	void FireMouseWheel(long wParam, long lParam)
		{FireEvent(eventidMouseWheel,EVENT_PARAM(VTS_I4  VTS_I4), wParam, lParam);}
	//}}AFX_EVENT
	DECLARE_EVENT_MAP()

// Dispatch and event IDs
public:
	enum {
	//{{AFX_DISP_ID(CImageEditCtrl)
	dispidPage = 1L,
	dispidScrollPositionX = 2L,
	dispidScrollPositionY = 3L,
	dispidMousePointer = 4L,
	dispidBaseRotation = 5L,
	dispidSetImage = 6L,
	dispidDisplay = 7L,
	dispidClearDisplay = 8L,
	dispidLockLeadWndUpdate = 9L,
	dispidUnlockLeadWndUpdate = 10L,
	dispidScroll = 11L,
	dispidGetImageWidth = 12L,
	dispidGetPageCount = 13L,
	dispidGetImageHeight = 14L,
	dispidGetImage = 15L,
	dispidFitToParent = 16L,
	dispidDrawSelectionRect = 17L,
	dispidGetXResolution = 18L,
	dispidGetYResolution = 19L,
	dispidZoomInAroundPoint = 20L,
	dispidZoomOutAroundPoint = 21L,
	dispidSetZoomMagnifyFactor = 22L,
	dispidGetZoomMagnifyFactor = 23L,
	dispidGetZoom = 24L,
	dispidSetZoom = 25L,
	dispidSetCursorHandle = 26L,
	dispidExtractZoneImage = 27L,
	dispidEnableVerticalScroll = 28L,
	dispidRefreshRect = 29L,
	eventidScroll = 1L,
	eventidMouseWheel = 2L
	//}}AFX_DISP_ID
	};
};
//==================================================================================================
//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
