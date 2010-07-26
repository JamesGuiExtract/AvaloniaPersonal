//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ImageEditCtrl.cpp
//
// PURPOSE:	This is an implementation file for CImageEditCtrl() class.
//			Where the CImageEditCtrl() class has been derived from COleControl() class.
//			The code written in this file makes it possible to implement the various
//			application methods in the user interface.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// ImageEditCtl.cpp : Implementation of the CImageEditCtrl ActiveX Control class.
//

#include "stdafx.h"
#include "ImageEdit.h"
#include "ImageEditCtl.h"
#include "ImageEditPpg.h"

#include <ltWrappr.h>

#include <ExtractZoneAsImage.h>
#include <MiscLeadUtils.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>

#include <string>
using namespace std;


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

IMPLEMENT_DYNCREATE(CImageEditCtrl, COleControl)

//-------------------------------------------------------------------------------------------------
// Message map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CImageEditCtrl, COleControl)
	//{{AFX_MSG_MAP(CImageEditCtrl)
	ON_WM_MOUSEWHEEL()
	ON_WM_SIZE()
	ON_WM_CREATE()
	ON_WM_SETCURSOR()
	ON_WM_LBUTTONDOWN()
	ON_WM_RBUTTONDOWN()
	//}}AFX_MSG_MAP
	ON_OLEVERB(AFX_IDS_VERB_PROPERTIES, OnProperties)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Dispatch map
//-------------------------------------------------------------------------------------------------
BEGIN_DISPATCH_MAP(CImageEditCtrl, COleControl)
	//{{AFX_DISPATCH_MAP(CImageEditCtrl)
	DISP_PROPERTY_EX(CImageEditCtrl, "Page", GetPage, SetPage, VT_I4)
	DISP_PROPERTY_EX(CImageEditCtrl, "ScrollPositionX", GetScrollPositionX, SetScrollPositionX, VT_I4)
	DISP_PROPERTY_EX(CImageEditCtrl, "ScrollPositionY", GetScrollPositionY, SetScrollPositionY, VT_I4)
	DISP_PROPERTY_EX(CImageEditCtrl, "MousePointer", GetMousePointer, SetMousePointer, VT_I4)
	DISP_PROPERTY_EX(CImageEditCtrl, "BaseRotation", GetBaseRotation, SetBaseRotation, VT_R8)
	DISP_FUNCTION(CImageEditCtrl, "SetImage", SetImage, VT_EMPTY, VTS_BSTR)
	DISP_FUNCTION(CImageEditCtrl, "Display", Display, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CImageEditCtrl, "ClearDisplay", ClearDisplay, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CImageEditCtrl, "LockLeadWndUpdate", LockLeadWndUpdate, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CImageEditCtrl, "UnlockLeadWndUpdate", UnlockLeadWndUpdate, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CImageEditCtrl, "Scroll", Scroll, VT_EMPTY, VTS_I2 VTS_I4)
	DISP_FUNCTION(CImageEditCtrl, "GetImageWidth", GetImageWidth, VT_I4, VTS_NONE)
	DISP_FUNCTION(CImageEditCtrl, "GetPageCount", GetPageCount, VT_I4, VTS_NONE)
	DISP_FUNCTION(CImageEditCtrl, "GetImageHeight", GetImageHeight, VT_I4, VTS_NONE)
	DISP_FUNCTION(CImageEditCtrl, "GetImage", GetImage, VT_BSTR, VTS_NONE)
	DISP_FUNCTION(CImageEditCtrl, "FitToParent", FitToParent, VT_EMPTY, VTS_BOOL)
	DISP_FUNCTION(CImageEditCtrl, "DrawSelectionRect", DrawSelectionRect, VT_EMPTY, VTS_I4 VTS_I4 VTS_I4 VTS_I4)
	DISP_FUNCTION(CImageEditCtrl, "GetXResolution", GetXResolution, VT_I4, VTS_NONE)
	DISP_FUNCTION(CImageEditCtrl, "GetYResolution", GetYResolution, VT_I4, VTS_NONE)
	DISP_FUNCTION(CImageEditCtrl, "ZoomInAroundPoint", ZoomInAroundPoint, VT_EMPTY, VTS_I4 VTS_I4)
	DISP_FUNCTION(CImageEditCtrl, "ZoomOutAroundPoint", ZoomOutAroundPoint, VT_EMPTY, VTS_I4 VTS_I4)
	DISP_FUNCTION(CImageEditCtrl, "SetZoomMagnifyFactor", SetZoomMagnifyFactor, VT_EMPTY, VTS_I4)
	DISP_FUNCTION(CImageEditCtrl, "GetZoomMagnifyFactor", GetZoomMagnifyFactor, VT_I4, VTS_NONE)
	DISP_FUNCTION(CImageEditCtrl, "GetZoom", GetZoom, VT_R8, VTS_NONE)
	DISP_FUNCTION(CImageEditCtrl, "SetZoom", SetZoom, VT_EMPTY, VTS_I4)
	DISP_FUNCTION(CImageEditCtrl, "SetCursorHandle", SetCursorHandle, VT_EMPTY, VTS_PHANDLE)
	DISP_FUNCTION(CImageEditCtrl, "ExtractZoneImage", ExtractZoneImage, VT_EMPTY, VTS_I4 VTS_I4 VTS_I4 VTS_I4 VTS_I4 VTS_I4 VTS_BSTR)
	DISP_FUNCTION(CImageEditCtrl, "EnableVerticalScroll", EnableVerticalScroll, VT_EMPTY, VTS_BOOL)
	DISP_FUNCTION(CImageEditCtrl, "RefreshRect", RefreshRect, VT_EMPTY, VTS_I4 VTS_I4 VTS_I4 VTS_I4)
	DISP_FUNCTION_ID(CImageEditCtrl, "Refresh", DISPID_REFRESH, Refresh, VT_EMPTY, VTS_NONE)
	//}}AFX_DISPATCH_MAP
	DISP_FUNCTION_ID(CImageEditCtrl, "AboutBox", DISPID_ABOUTBOX, AboutBox, VT_EMPTY, VTS_NONE)
END_DISPATCH_MAP()

//-------------------------------------------------------------------------------------------------
// Event map
//-------------------------------------------------------------------------------------------------
BEGIN_EVENT_MAP(CImageEditCtrl, COleControl)
	//{{AFX_EVENT_MAP(CImageEditCtrl)
	EVENT_CUSTOM("Scroll", FireScroll, VTS_NONE)
	EVENT_CUSTOM("MouseWheel", FireMouseWheel, VTS_I4  VTS_I4)
	EVENT_STOCK_MOUSEDOWN()
	EVENT_STOCK_MOUSEUP()
	EVENT_STOCK_MOUSEMOVE()
	EVENT_STOCK_DBLCLICK()
	EVENT_STOCK_KEYUP()
	EVENT_STOCK_KEYDOWN()
	//}}AFX_EVENT_MAP
END_EVENT_MAP()

//-------------------------------------------------------------------------------------------------
// Property pages
//-------------------------------------------------------------------------------------------------
// TODO: Add more property pages as needed.  Remember to increase the count!
BEGIN_PROPPAGEIDS(CImageEditCtrl, 1)
	PROPPAGEID(CImageEditPropPage::guid)
END_PROPPAGEIDS(CImageEditCtrl)

//-------------------------------------------------------------------------------------------------
// Initialize class factory and guid
//-------------------------------------------------------------------------------------------------
IMPLEMENT_OLECREATE_EX(CImageEditCtrl, "IMAGEEDIT.ImageEditCtrl.1",
	0x96a61232, 0x6749, 0x4b65, 0x82, 0x3f, 0x4a, 0x6e, 0xe6, 0xd0, 0x3, 0xc4)

//-------------------------------------------------------------------------------------------------
// Type library ID and version
//-------------------------------------------------------------------------------------------------
IMPLEMENT_OLETYPELIB(CImageEditCtrl, _tlid, _wVerMajor, _wVerMinor)

//-------------------------------------------------------------------------------------------------
// Interface IDs
//-------------------------------------------------------------------------------------------------
const IID BASED_CODE IID_DImageEdit =
		{ 0x3c1eb7a4, 0x7f27, 0x4d6e, { 0xba, 0xe5, 0x34, 0x13, 0xbd, 0xba, 0x5c, 0xcd } };
const IID BASED_CODE IID_DImageEditEvents =
		{ 0x9d62e079, 0x291a, 0x4a5c, { 0x90, 0x7b, 0xb, 0x78, 0xe, 0x1a, 0xe3, 0x17 } };

//-------------------------------------------------------------------------------------------------
// Control type information
//-------------------------------------------------------------------------------------------------
static const DWORD BASED_CODE _dwImageEditOleMisc =
	OLEMISC_ACTIVATEWHENVISIBLE |
	OLEMISC_SETCLIENTSITEFIRST |
	OLEMISC_INSIDEOUT |
	OLEMISC_CANTLINKINSIDE |
	OLEMISC_RECOMPOSEONRESIZE;

IMPLEMENT_OLECTLTYPE(CImageEditCtrl, IDS_IMAGEEDIT, _dwImageEditOleMisc)

//-------------------------------------------------------------------------------------------------
// CImageEditCtrl::CImageEditCtrlFactory::UpdateRegistry -
// Adds or removes system registry entries for CImageEditCtrl
BOOL CImageEditCtrl::CImageEditCtrlFactory::UpdateRegistry(BOOL bRegister)
{
	// TODO: Verify that your control follows apartment-model threading rules.
	// Refer to MFC TechNote 64 for more information.
	// If your control does not conform to the apartment-model rules, then
	// you must modify the code below, changing the 6th parameter from
	// afxRegApartmentThreading to 0.

	if (bRegister)
		return AfxOleRegisterControlClass(
			AfxGetInstanceHandle(),
			m_clsid,
			m_lpszProgID,
			IDS_IMAGEEDIT,
			IDB_IMAGEEDIT,
			afxRegApartmentThreading,
			_dwImageEditOleMisc,
			_tlid,
			_wVerMajor,
			_wVerMinor);
	else
		return AfxOleUnregisterClass(m_clsid, m_lpszProgID);
}

//-------------------------------------------------------------------------------------------------
// CImageEditCtrl
//-------------------------------------------------------------------------------------------------
CImageEditCtrl::CImageEditCtrl()
{
	InitializeIIDs(&IID_DImageEdit, &IID_DImageEditEvents);

	//	Set initial size
	SetInitialSize(485, 400);

	m_zImageName.Empty();
	m_dZoomPercent = 100;
	m_bFitToParent = false; 
	m_bImageLoaded = false;

	//	Initially set base rotation to 0.0
	m_dBaseRotation = 0.0;

	//	set previous page number to 0
	m_dCurPageNumber = 0;

	// set the default page count to 1
	m_iPageCount = 1;

	// set the default value here
	m_bDisplayImage = false;

	//	set Image height and width to 0
	m_lImageHeight = 0;
	m_lImageWidth = 0;

	// default magnification factor
	m_dZoomMagnifyFactor = 1.2;

	m_dRectHeight = 0;
	m_dRectWidth = 0;

	// Create the Configuration object
	m_apSettings = auto_ptr<ImageEditCtrlCfg>(new ImageEditCtrlCfg());

	// Get initialized FILEINFO struct
	m_fInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

	// if PDF is licensed initialize support
	if ( LicenseManagement::sGetInstance().isPDFLicensed() )
	{
		// Init pdf support (use default PDF initialization settings)
		initPDFSupport();
	}
}
//-------------------------------------------------------------------------------------------------
CImageEditCtrl::~CImageEditCtrl()
{
	try
	{
		// clean up if any
		ClearDisplay();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16586");
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::OnDraw(
			CDC* pdc, const CRect& rcBounds, const CRect& rcInvalid)
{
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::DoPropExchange(CPropExchange* pPX)
{
	ExchangeVersion(pPX, MAKELONG(_wVerMinor, _wVerMajor));
	COleControl::DoPropExchange(pPX);
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::OnResetState()
{
	COleControl::OnResetState();  // Resets defaults found in DoPropExchange
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::AboutBox()
{
	CDialog dlgAbout(IDD_ABOUTBOX_IMAGEEDIT);
	dlgAbout.DoModal();
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::SetImage(LPCTSTR szImagePath) 
{
	// if image is already loaded we need to unload the loaded 
	// image before setting the new image
	if (m_bImageLoaded)
	{
		ClearDisplay();
	}

	// if file is pdf make sure PDF Support is enabled
	try
	{
		LicenseManagement::sGetInstance().verifyFileTypeLicensed( szImagePath );
	}
	catch(...)
	{
		AfxThrowOleDispatchException(0,"ELI13427: PDF read/write support is not licensed");
	}
	
	//	Set image file name
	m_zImageName = szImagePath;

	//	set image file to bitmap window
	m_LeadWnd.SetFileName((L_TCHAR *)szImagePath);
	
	//	Get File Info
	if(!m_zImageName.IsEmpty())
	{
		L_INT	nRetCode = m_LeadWnd.File()->GetInfo(&m_fInfo, sizeof(FILEINFO));
		throwOnLeadToolsError("ELI90501", nRetCode);
	}

	m_LeadWnd.EnableCenterOnZoom(FALSE);
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::Display() 
{
	CheckForImage("Display");

	L_INT nRetCode = SUCCESS;

	// Retrieve settings
	bool bEnableAA = m_apSettings->isAntiAliasingEnabled();

	// Display is being called at various places like SetbaseRotation, SetPageNumber etc.,
	// so optimize the setting and load the image with correct page number
	if (m_bImageLoaded && m_fInfo.TotalPages > 1)
	{
		// Set the bits per pixel to 1 if AntiAliasing is enabled
		L_INT nBitsPerPixel = bEnableAA ? 1 : 0;

		// Get initialized LOADFILEOPTION struct (11/1/07 SNK).  Incoroporates previous change:
		// Provide page number and IgnoreViewPerspective flag via LOADFILEOPTIONS
		// per suggestion from LeadTools support to fix problem with display of 
		// black or negative region - 09/18/07.
		LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_IGNOREVIEWPERSPECTIVE);

		lfo.PageNumber = m_dCurPageNumber;
		nRetCode = m_LeadWnd.Load( (LPSTR)(LPCTSTR)m_zImageName, nBitsPerPixel, 
			ORDER_BGR, &lfo );

		// check the return code of loading and raise an exception if loading failed 
		throwOnLeadToolsError("ELI19811", nRetCode);

		// Add anti-aliasing to enhance readability
		if (bEnableAA)
		{
			nRetCode = m_LeadWnd.SetDisplayMode( DISPLAYMODE_SCALETOGRAY, DISPLAYMODE_SCALETOGRAY );

			// check the return code of loading and raise an exception if loading failed 
			throwOnLeadToolsError("ELI19812", nRetCode);
		}
	}
	else if (!m_bImageLoaded || m_bDisplayImage == 1)
	{
		// Set the bits per pixel to 1 if AntiAliasing is enabled
		L_INT nBitsPerPixel = bEnableAA ? 1 : 0;

		// Get initialized LOADFILEOPTION struct (11/1/07).  Incoroporates previous change:
		// Provide page number and IgnoreViewPerspective flag via LOADFILEOPTIONS
		// per suggestion from LeadTools support to fix problem with display of 
		// black or negative region - 09/18/07.
		LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_IGNOREVIEWPERSPECTIVE);

		nRetCode = m_LeadWnd.Load( nBitsPerPixel, 4, &lfo );
		throwOnLeadToolsError("ELI19813", nRetCode);

		// Add anti-aliasing to enhance readability
		if (bEnableAA)
		{
			nRetCode = m_LeadWnd.SetDisplayMode( DISPLAYMODE_SCALETOGRAY, DISPLAYMODE_SCALETOGRAY );
			throwOnLeadToolsError("ELI19814", nRetCode);
		}

		m_dCurPageNumber = 0;
	}
	else
	{
		// we don't really need to do anything when image is not loaded for first time
		// or the image is a multipage image or there is no base rotation to set back.

		return;
	}

	// check the return code of loading and raise an exception if loading failed 
	throwOnLeadToolsError("ELI90502", nRetCode);
	
	// if image is loaded successfully set a flag
	m_bImageLoaded = true;
	
	m_iPageCount = m_fInfo.TotalPages;
	
	m_dBaseRotation = 0.0;

	//	get the image width and height
	m_lImageWidth = m_LeadWnd.GetWidth();
	m_lImageHeight = m_LeadWnd.GetHeight();
}
//-------------------------------------------------------------------------------------------------
BOOL CImageEditCtrl::OnMouseWheel(UINT nFlags, short zDelta, CPoint pt) 
{
	return COleControl::OnMouseWheel(nFlags, zDelta, pt);
}
//-------------------------------------------------------------------------------------------------
long CImageEditCtrl::GetPage() 
{
	CheckForImage("GetPage");

	//	return the currently displayed image page number
	if (m_dCurPageNumber == 0 || m_dCurPageNumber ==1)
		return 1;
	
	return m_dCurPageNumber;
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::SetPage(long nNewValue) 
{
	CheckForImage("SetPage");

	if(nNewValue > m_fInfo.TotalPages)
	{
		CString zMsg;
		zMsg.Format ("ELI90504:Invalid page number for '%s'", m_zImageName);
		AfxThrowOleDispatchException (0, LPCTSTR(zMsg));
	}

	if (nNewValue == m_dCurPageNumber)
	{
		return;
	}

	m_fInfo.PageNumber = nNewValue;
	m_dBaseRotation = 0.0;
	m_dCurPageNumber = nNewValue;

	// display will take care of displaying the correct page number 
	Display();

	SetModifiedFlag();
}
//-------------------------------------------------------------------------------------------------
long CImageEditCtrl::GetScrollPositionX() 
{
	return ::GetScrollPos(m_hWnd, SB_HORZ);
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::SetScrollPositionX(long nNewValue) 
{
	CheckForImage("SetScrollPositionX");

	int iMin = 0, iMax = 0;

	//	Check whether the given value is within the range.
	::GetScrollRange(m_hWnd, SB_HORZ, &iMin, &iMax); 

	if( nNewValue < iMin )
	{
		nNewValue = iMin;
	}
	else if( nNewValue > iMax )
	{
		nNewValue = iMax;
	}

	L_INT iY = GetScrollPositionY();

	//	Set the scroll postion
	m_LeadWnd.ScrollTo(nNewValue, iY);

	SetModifiedFlag();
}
//-------------------------------------------------------------------------------------------------
long CImageEditCtrl::GetScrollPositionY() 
{
	return ::GetScrollPos(m_hWnd, SB_VERT);
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::SetScrollPositionY(long nNewValue) 
{
	CheckForImage("SetScrollPositionY");

	int iMin = 0, iMax = 0;

	//	Check whether the given value is within the range.
	::GetScrollRange(m_hWnd, SB_VERT, &iMin, &iMax); 

	if( nNewValue < iMin )
	{
		nNewValue = iMin;
	}
	else if( nNewValue > iMax )
	{
		nNewValue = iMax;
	}

	L_INT iX = GetScrollPositionX();

	//	Set the scroll postion
	m_LeadWnd.ScrollTo(iX, nNewValue);

	SetModifiedFlag();
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::ClearDisplay() 
{
	L_INT	nRetCode = SUCCESS;
	
	if (!m_zImageName.IsEmpty() && m_bImageLoaded)
	{
		m_dZoomPercent = 100;
		m_bFitToParent = false; 
		
		//	Initially set base rotation to 0.0
		m_dBaseRotation = 0.0;
		
		//	set previous page number to 0
		m_dCurPageNumber = 0;
		
		// set the default page count to 1
		m_iPageCount = 1;
		
		// set the default value here
		m_bDisplayImage = false;
		
		//	set Image height and width to 0
		m_lImageHeight = 0;
		m_lImageWidth = 0;
		
		// Make sure that a bitmap has been allocated
		if (m_LeadWnd.IsAllocated())
		{
			nRetCode = m_LeadWnd.Clear();
			throwOnLeadToolsError("ELI90505", nRetCode);
			
			// set the image name to NULL before clearing the image in the window
			m_zImageName.Empty(); 
			
			// reset the image loaded flag
			m_bImageLoaded = false;
			
			// P13 #4621 JDS -
			// free the bitmap instead of just setting background to white.
			// this prevents future loads of bad images from appearing to
			// succeed (see investigation notes). 
			// future calls to IsAllocated will return false.
			nRetCode = m_LeadWnd.Free();
			throwOnLeadToolsError("ELI90506", nRetCode);
		}
		// image name is not empty and bitmap loaded flag is true, but no bitmap has been allocated
		else
		{
			// P13 #4621 - JDS
			// part of the issue is that the image name is never cleared if we get to this point and so
			// all future attempts to load an image fail since they each issue a call to clear first
			// empty the loaded image name and set image loaded to false.  
			// solution: empty image name and set image loaded to false (since there is no image loaded).
			m_zImageName.Empty();
			m_bImageLoaded = false;

			// build error message and throw exception
			CString zError;
			zError.Format ("ELI14114: Unable to allocate bitmap for and open image '%s'.\r\nIf PDF image, try reducing the load resolution.", m_zImageName);
			AfxThrowOleDispatchException (0, LPCTSTR(zError));
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::RefreshRect(long lLeft, long lTop, long lRight, long lBottom) 
{
	RECT rect = {lLeft, lTop, lRight, lBottom};
	m_LeadWnd.Repaint(&rect);
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::Refresh() 
{
	// repaint the image
	m_LeadWnd.Repaint();
}
//-------------------------------------------------------------------------------------------------
long CImageEditCtrl::GetMousePointer() 
{
	return (long)m_LeadWnd.GetMousePointer();
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::SetMousePointer(long nNewValue) 
{
	CheckForImage("SetMousePointer");

	if (nNewValue < 0)
	{
		nNewValue = 0;
	}

	m_LeadWnd.SetMousePointer(nNewValue);
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::OnSize(UINT nType, int cx, int cy) 
{
	L_BOOL	bPrevState;

	if(!m_zImageName.IsEmpty())
	{
  		bPrevState = m_LeadWnd.EnableCenterOnZoom(FALSE);
	}

	if(!m_zImageName.IsEmpty() && m_bImageLoaded)
	{
		int scrollHeightHorizontal =  GetSystemMetrics(SM_CYHSCROLL);
		int scrollHeightVertical =  GetSystemMetrics(SM_CXVSCROLL);

		if (m_dRectWidth != cx && m_dRectWidth - scrollHeightHorizontal != cx && m_dRectWidth != 0.0 || m_dRectHeight != cy && cy != m_dRectHeight - scrollHeightVertical && m_dRectHeight != 0.0)
			UpdateTheZoomOnSize ();

		m_bFitToParent = false;
	}
}
//-------------------------------------------------------------------------------------------------
BOOL CImageEditCtrl::Create(LPCTSTR lpszClassName, LPCTSTR lpszWindowName, DWORD dwStyle, const RECT& rect, CWnd* pParentWnd, UINT nID, CCreateContext* pContext) 
{
	return CWnd::Create(lpszClassName, lpszWindowName, dwStyle, rect, pParentWnd, nID, pContext);
}
//-------------------------------------------------------------------------------------------------
int CImageEditCtrl::OnCreate(LPCREATESTRUCT lpCreateStruct) 
{
	L_INT nRetCode = SUCCESS;

	if (COleControl::OnCreate(lpCreateStruct) == -1)
	{
		return -1;
	}

	// set the lead window handle here. The Image control and lead 
	// window are tied here
	nRetCode = m_LeadWnd.SetWndHandle(GetSafeHwnd());
	throwOnLeadToolsError("ELI90508", nRetCode);
	
	// we have to see whether this is really required or not
	nRetCode = m_LeadWnd.EnableKeyBoard(false);
	throwOnLeadToolsError("ELI90509", nRetCode);

	m_LeadWnd.SetImageControl(this);

	// set 120 % as default magnification factor for zooming
	SetZoomMagnifyFactor(120);

	return 0;
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::LockLeadWndUpdate() 
{
	// locks the Image edit control from updating. By locking
	// the control, we can set properties like zoom, scrolls etc, 
	// and after doing every thing update the view once. helps in 
	// avoiding the image flickering
	::LockWindowUpdate(m_hWnd);
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::UnlockLeadWndUpdate() 
{
	// unlocks the image update. As soon as this method is called,
	// Image edit control will get repainted.
	::LockWindowUpdate(NULL);
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::Scroll(short iType, long iDistance) 
{
	CheckForImage("Scroll");

	// scrolls horizotally or vertically by the iDistance 
	// iType decides whether to move horizontally or vertically

	if( iType == 0) //	Horizontal
		m_LeadWnd.ScrollBy(iDistance, 0);
	else if(iType == 1)	// Vertical
		m_LeadWnd.ScrollBy(0, iDistance);
}
//-------------------------------------------------------------------------------------------------
long CImageEditCtrl::GetImageWidth() 
{
	CheckForImage("GetImageWidth");

	if (m_bImageLoaded)
	{
		// returns loaded image width.
		return m_lImageWidth;
	}

	AfxThrowOleDispatchException(0,"ELI90510:Image not loaded");

	return 0;
}
//-------------------------------------------------------------------------------------------------
double CImageEditCtrl::GetBaseRotation() 
{
	CheckForImage("GetBaseRotation");

	return m_dBaseRotation;
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::SetBaseRotation(double newValue) 
{
	CWaitCursor wait;

	CheckForImage("SetBaseRotation");

	double dBaseRotation = newValue;

	L_INT	nRetCode	= SUCCESS;

	m_bDisplayImage = true;

	// Display will reload the image with current page number with
	// base rotation equal to 0.0
	Display();

	// Display has already loaded the image with base rotation = 0.0.
	// so if base rotation to set is 0, then nothing to do with rotating
	// the image. Return from here after UnLocking the image drawing
	if(0 == (int) newValue && 0 == (int)m_dBaseRotation)
	{
		m_dBaseRotation = 0.0;
		return;
	}

	//	check rotation against zero degrees and the image type is supported or not
	if((fabs(dBaseRotation) > 0.000001))
	{
		//	If rotation is specified, rotate the image by that many degrees.
		if(fabs(dBaseRotation - 0.000001) > 0)
		{
			COLORREF backColor	= m_fInfo.GlobalBackground;

			int iBaseRotation = (int)(dBaseRotation * -100.0);
			
			nRetCode = m_LeadWnd.Rotate(iBaseRotation, ROTATE_RESIZE, backColor);

			if( nRetCode != SUCCESS)
			{
				// Free the temporary bitmap 
				L_INT nRetCode1 = m_LeadWnd.Free();
				throwOnLeadToolsError("ELI90511", nRetCode1);

				m_zImageName = "";

				throwOnLeadToolsError("ELI90512", nRetCode);
			}
		}
	}
	
	m_dBaseRotation = newValue;
	m_bDisplayImage = false; 

	//	update the height and width of the image loaded
	m_lImageWidth = m_LeadWnd.GetWidth();
	m_lImageHeight = m_LeadWnd.GetHeight();
	
	SetModifiedFlag();
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::CheckForImage(CString zFunctionName)
{
	//	raises an exception if image is NOT currently loaded into the control
	if(m_zImageName.IsEmpty())
	{
		CString zError = "ELI90513: No image is currently loaded ";
		//	add the function name to the error
		if (!zFunctionName.IsEmpty())
		{
			zError += "to ";
			zError += zFunctionName;
		}

		AfxThrowOleDispatchException (0, LPCTSTR(zError));
	}
}
//-------------------------------------------------------------------------------------------------
long CImageEditCtrl::GetPageCount() 
{
	CheckForImage("GetPageCount");

	//	return the total pages of the image
	return m_fInfo.TotalPages;
}
//-------------------------------------------------------------------------------------------------
long CImageEditCtrl::GetImageHeight() 
{
	// returns loaded image height
	return m_lImageHeight;
}
//-------------------------------------------------------------------------------------------------
BSTR CImageEditCtrl::GetImage() 
{
	CheckForImage("GetImage");

	CString strResult;

	//	return the loaded image name
	strResult = m_zImageName;
	return strResult.AllocSysString();
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::FitToParent(BOOL bFitPage) 
{
	CheckForImage("FitToParent");

	L_INT	nRetCode;

	if(!m_zImageName.IsEmpty())
	{
		//	set zoom mode.
		if (m_LeadWnd.IsAllocated())
		{
			nRetCode = m_LeadWnd.SetZoomMode(bFitPage? ZOOM_FIT: ZOOM_FITWIDTH);
			throwOnLeadToolsError("ELI90516", nRetCode);

			m_bFitToParent = true;
			UpdateTheZoomOnImageLoad(bFitPage);
		}
		else
		{
			CString zError;
			zError.Format ("ELI14115: Unable to allocate bitmap for and open image '%s'",m_zImageName);
			AfxThrowOleDispatchException (0, LPCTSTR(zError));
		}
	}
}
//-------------------------------------------------------------------------------------------------
long CImageEditCtrl::GetXResolution() 
{
	CheckForImage("GetXResolution");

	//	return the X resolution
	return m_LeadWnd.GetXResolution();
}
//-------------------------------------------------------------------------------------------------
long CImageEditCtrl::GetYResolution() 
{
	CheckForImage("GetYResolution");

	//	return the Y resolution
	return m_LeadWnd.GetYResolution();
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::FireScrollFromCtrlClass()
{
	//	Fire Image Edit scroll event.
	FireScroll();
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::FireMouseWheelFromCtrlClass(long wParam, long lParam)
{
	FireMouseWheel(wParam, lParam);
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::DrawSelectionRect(long Left, long Top, long Right, long Bottom) 
{
	// NOTE : ZoomToRect() API supported by Lead Tools does not really zoom the selected rectangle 
	// A work around is implemented here to zoom the selected rectangle
	
	//	check the given coordinates and set them properly
	if( Left > Right )
	{
		long dTempLR = Left;
		Left = Right;
		Right = dTempLR;
	}

	if( Top > Bottom )
	{
		long dTempTB = Top;
		Top = Bottom;
		Bottom = dTempTB;
	}

	// the given cordinates are view coordinates 
	// have a copy of the selected window coordinates to convert to view coordinates
	double dLeft = Left;
	double dTop = Top;
	double dRight = Right;
	double dBottom = Bottom;

	// check whether the given window coordinates are well within image bounds
	CRect clientRect;
	GetClientRect(clientRect);
	
	// convert the given view coordinates into image coordinates
	ViewToImageCoordinates(&dLeft, &dTop);
	ViewToImageCoordinates(&dRight, &dBottom);
	
	if (dLeft < 0
		|| dTop < 0
		|| dRight > GetImageWidth()
		|| Bottom > GetImageHeight())
		
	{
		AfxThrowOleDispatchException (0, " ELI90517 : Coordinates are not within image bounds");
	}
	
	double dZoomWidth = dRight - dLeft;
	double dZoomHeight = dBottom - dTop;
	
	double dMaxSize = (dRight - dLeft) > (dBottom - dTop) ? (dRight - dLeft) : (dBottom - dTop);

	// caclulate the zoomfactor that can be set to show the selected window
	// completely. The zoom factor is to be set based on the maximum size 
	// among the zoom window width and height.
	double dMaxSelectableZoom = 0.0;

	double dClientRectWidth = clientRect.Width();
	double dClientRectHeight = clientRect.Height();

	double dZoomWndRatio = dZoomWidth/dZoomHeight;
	double dClientWndRatio = dClientRectWidth/dClientRectHeight;

	if( dZoomWndRatio > dClientWndRatio )
		dMaxSelectableZoom = (max(dClientRectWidth,dClientRectHeight) / dMaxSize) * 100;
	else
	{
		double dZoomWidthPercent = dClientRectWidth / dZoomWidth;
		double dZoomHeightPercent = dClientRectHeight / dZoomHeight;
		dMaxSelectableZoom = min(dZoomWidthPercent, dZoomHeightPercent) * 100;
	}

	// set the maximum zoom percent that can accommodate the window selected
	SetZoom ((int)dMaxSelectableZoom);
	
	// update the class varible
	m_dZoomPercent = (int)GetZoom();
	
	// now convert back the image coordinates to the view coordianates
	ImageToViewCoordinates (&dLeft, &dTop);
	ImageToViewCoordinates (&dRight, &dBottom);
	
	// scroll the image such that left - top of the selected window is matched
	// with client window  left - top
	SetScrollPositionX((int)dLeft);
	SetScrollPositionY((int)dTop);
}
// =========================================================================
void CImageEditCtrl::ViewToImageCoordinates(double *dX, double *dY)
{
	double dScrollX = GetScrollPositionX();
	double dScrollY = GetScrollPositionY();
	
	// convert the view coordinates to the image coordinates
	*dX /= (m_dZoomPercent/100.0);
	*dY /= (m_dZoomPercent/100.0);
	
	// translate the view point
	*dX += (dScrollX/(m_dZoomPercent/100.0));
	*dY += (dScrollY/(m_dZoomPercent/100.0));
}
// =============================================================================
void CImageEditCtrl::ImageToViewCoordinates (double *dX, double *dY)
{
	double dScrollX = GetScrollPositionX();
	double dScrollY = GetScrollPositionY();
	
	// translate the image point
	*dX -= (dScrollX/(m_dZoomPercent/100.0));
	*dY -= (dScrollY/(m_dZoomPercent/100.0));
	
	// convert the image coordinates to the view coordinates
	*dX *= (m_dZoomPercent/100.0);
	*dY *= (m_dZoomPercent/100.0);
}
// =============================================================================

BOOL CImageEditCtrl::OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message) 
{
	//	update the cursor
	m_LeadWnd.UpdateCursor();
	
	return COleControl::OnSetCursor(pWnd, nHitTest, message);
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::ZoomInAroundPoint(long PosX, long PosY) 
{
	m_LeadWnd.EnableAutoPaint(FALSE) ;
	m_LeadWnd.Repaint();

	double dX1 = PosX, dY1 = PosY;

	// get this point's current image coordinates (lower left corner is the origin)
	// before zoom in
	ViewToImageCoordinates(&dX1, &dY1);

	//	display Off the cursor
	m_LeadWnd.SetShowCursor(false);

	// call ZoomIn here
	m_LeadWnd.ZoomIn();

	m_LeadWnd.UpdateCursor();
	//	display On the cursor
	m_LeadWnd.SetShowCursor(true);

	// update the zoom factor
	m_dZoomPercent *= m_dZoomMagnifyFactor;

	// get this point's current view coordinates (upper left corner is the origin)
	// after zoom in
	ImageToViewCoordinates(&dX1, &dY1);
	int scrollX = (int)dX1 - PosX;
	int scrollY = (int)dY1 - PosY;


	m_LeadWnd.ScrollBy(scrollX, scrollY);

	m_LeadWnd.EnableAutoPaint(TRUE) ;
	m_LeadWnd.Repaint();

}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::ZoomOutAroundPoint(long PosX, long PosY) 
{
	m_LeadWnd.EnableAutoPaint(FALSE) ;
	m_LeadWnd.Repaint();
	
	double dX1 = PosX, dY1 = PosY;
	
	// get this point's current image coordinates (lower left corner is the origin)
	// before zoom out
	ViewToImageCoordinates(&dX1, &dY1);
	
	//	display Off the cursor
	m_LeadWnd.SetShowCursor(false);
	
	// call ZoomOut here
	m_LeadWnd.ZoomOut();
	
	m_LeadWnd.UpdateCursor();
	//	display On the cursor
	m_LeadWnd.SetShowCursor(true);
	
	// update the zoomfactor
	m_dZoomPercent /= m_dZoomMagnifyFactor;
	
	// get this point's current view coordinates (upper left corner is the origin)
	// after zoom out
	ImageToViewCoordinates(&dX1, &dY1);
	int scrollX = (int)dX1 - PosX;
	int scrollY = (int)dY1 - PosY;
	
	m_LeadWnd.ScrollBy(scrollX, scrollY);
	
	
	m_LeadWnd.EnableAutoPaint(TRUE) ;
	m_LeadWnd.Repaint();

}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::SetZoomMagnifyFactor(long lPercentMagnify) 
{
	//	check for the percent maginfy cannot be less than 100.
	if (lPercentMagnify < 100)
	{
		AfxThrowOleDispatchException(0, "ELI90518 : Percent zoom can not be less than 100");
	}

	m_dZoomMagnifyFactor = (double)lPercentMagnify/100.0 ;

	//	Set the zoom factor
	m_LeadWnd.SetZoomFactor((float)m_dZoomMagnifyFactor);
}
//-------------------------------------------------------------------------------------------------
long CImageEditCtrl::GetZoomMagnifyFactor() 
{
	CheckForImage("GetZoomMagnifyFactor");

	//	return the zoom magnify factor.
	return ((long)(m_LeadWnd.GetZoomFactor() * 100.0));
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::UpdateTheZoomOnImageLoad(BOOL bFitPage)
{
	// calculate the zoom percent to set to accommodate dWidth or dHeight that
	// is minimum among the both
	CRect rect;
	GetClientRect(rect);

	m_dRectWidth = rect.Width();
	m_dRectHeight = rect.Height();
	
	m_dZoomFitWidth		= m_dRectWidth / GetImageWidth() * 100.0;
	m_dZoomFitHeight	= m_dRectHeight / GetImageHeight() * 100.0;

	//	Set the minimum zoom factor as current zoom factor
	if (bFitPage)
	{
		m_dZoomPercent = m_dZoomFitWidth < m_dZoomFitHeight ? m_dZoomFitWidth :m_dZoomFitHeight;
	}
	else
	{
		m_dZoomPercent = m_dZoomFitWidth;
	}

	m_LeadWnd.SetZoomMode(ZOOM_FACTOR);

	// here is a new check added to ensure that the zoom percent never
	// goes beyond the ZoomIn and ZoomOut limits. The limits are 2 and 6500
	if (m_dZoomPercent < 2.0 )
		SetZoom(2);
	else if (m_dZoomPercent > 6500.0)
		SetZoom(6500);
}

double CImageEditCtrl::GetZoom() 
{
	// we should have to return the double value here 
	return m_dZoomPercent;
}

void CImageEditCtrl::SetZoom(long newValue) 
{
	CheckForImage("SetZoom");

	if (newValue < 1.0)
		return;

	// Lead Tools loads Arrow cursor whenever SetZoomPercent() is called. As a matter of fix,
	// hide the cursor before calling the function and show the cursor back after the call
	m_LeadWnd.SetShowCursor(false);

	//	Set the zoom factor
	m_LeadWnd.SetZoomPercent((int)newValue);
	m_LeadWnd.UpdateCursor();

	m_LeadWnd.SetShowCursor(true);

	m_dZoomPercent = newValue;
	
	SetModifiedFlag();
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::UpdateTheZoomOnSize ()
{
	CRect rect;
	GetClientRect(rect);
	
	int iX = rect.Width() ;
	int iY = rect.Height();

	bool bWidthChanged = false;
	bool bHeightChanged = false; 

	int scrollHeightHorizontal =  GetSystemMetrics(SM_CYHSCROLL);
	int scrollHeightVertical =  GetSystemMetrics(SM_CXVSCROLL);

	int iMinX = 0, iMaxX = 0;
	int iMinY = 0, iMaxY = 0;

	//	Check whether the given value is within the range.
	::GetScrollRange(m_hWnd, SB_HORZ, &iMinX, &iMaxX);

	//	Check whether the given value is within the range.
	::GetScrollRange(m_hWnd, SB_VERT, &iMinY, &iMaxY);

	if( iMaxX != 0 && iMaxX != 100)
		iY += scrollHeightHorizontal;

	if( iMaxY != 0 && iMaxY != 100 )
		iX += scrollHeightVertical;

	if (iX != m_dRectWidth)
	{
		m_dRectWidth = iX;
		bWidthChanged = true;
	}

	if (iY != m_dRectHeight)
	{
		m_dRectHeight = iY;
		bHeightChanged = true;
	}

	if ( !bHeightChanged && !bWidthChanged)
		return;

	m_dZoomFitWidth		= m_dRectWidth / GetImageWidth() * 100.0;
	m_dZoomFitHeight	= m_dRectHeight / GetImageHeight() * 100.0;
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::SetCursorHandle(OLE_HANDLE* hCursor) 
{
	m_LeadWnd.SetCursorHandle((HANDLE)hCursor);
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::throwOnLeadToolsError(const char* pszELICode, const L_INT iLeadToolsRetCode)
{
	// if the return code is not successful, throw an exception
	if(iLeadToolsRetCode != SUCCESS)
	{
		// get a user-readable description of the return code
		char *pszError;
		pszError = LBase::GetErrorString(iLeadToolsRetCode);
		
		// throw the ELI code, image name, and error description
		CString zError;
		zError.Format("%s: '%s' %s (%d).", pszELICode, m_zImageName, pszError, iLeadToolsRetCode);
		AfxThrowOleDispatchException(0, LPCTSTR(zError));
	}
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::translateOriginalImagePoint(long& rnX, long& rnY)
{
	double dZero = 1e-7;

	if (fabs(m_dBaseRotation) < dZero || fabs(m_dBaseRotation - 360) < dZero ||
		fabs(m_dBaseRotation + 360) < dZero)
	{
		// nothing to do.  image is not rotated (image angle is 0 degrees, or
		// 360 degrees, or -360 degrees
	}
	else if (fabs(m_dBaseRotation - 90) < dZero || fabs(m_dBaseRotation + 270) < dZero)
	{
		// image is rotated +90 degrees or -270 degrees
		long nXCopy = rnX;
		rnX = rnY;
		rnY = m_lImageHeight - nXCopy;
	}
	else if (fabs(m_dBaseRotation - 180) < dZero || fabs(m_dBaseRotation + 180) < dZero)
	{
		// image is rotated +180 degrees or -180 degrees
		rnX = m_lImageWidth - rnX;
		rnY = m_lImageHeight - rnY;
	}
	else if (fabs(m_dBaseRotation - 270) < dZero || fabs(m_dBaseRotation + 90) < dZero)
	{
		// image is rotated +270 degrees or -90 degrees
		long nYCopy = rnY;
		rnY = rnX;
		rnX = m_lImageWidth - nYCopy;
	}
	else
	{
		// image is rotated at some other angle, which is not supported
		// at this time
		AfxThrowOleDispatchException(0, "ELI05015: Invalid rotation angle!");
	}
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::ExtractZoneImage(long nX1, long nY1, long nX2, long nY2,
									  long nHeight, long nPageNum, 
									  LPCTSTR strFileName)
{
	// verify that an image is currently loaded
	CheckForImage("ExtractZoneImage");

	// for now we are only supporting extracting of images from the current page
	if (GetPage() != nPageNum)
	{
		AfxThrowOleDispatchException(0, "Extracting images from non-current pages is not yet supported.");
	}

	// rotate the coordinates by the current base rotation of the image
	// NOTE: we need to do this because the coordinates being passed to this
	// function are with the assumption that the image is unrotated and the
	// topleft of the image is (0,0).  However, with rotation done, the top left
	// of the rotated image still stays as (0,0) as for subsequent LeadTools
	// API calls...and so we need to rotate the coordinates correspondingly
	// so that we extract the correct area as the zone.
	translateOriginalImagePoint(nX1, nY1);
	translateOriginalImagePoint(nX2, nY2);

	try
	{
		try
		{
			extractZoneAsImage(m_LeadWnd.GetHandle(), nX1, nY1, nX2, nY2, nHeight, strFileName);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16647")
	}
	catch(UCLIDException &ue)
	{
		// Log the error before throwing an OleDispatchException
		ue.log();
		AfxThrowOleDispatchException(0, "ELI13428: Could not extract the zone.");
	}
}
//-------------------------------------------------------------------------------------------------
void CImageEditCtrl::EnableVerticalScroll(BOOL bEnable) 
{
	m_LeadWnd.SetVertLineStep(bEnable ? 5 : 0);
}
//-------------------------------------------------------------------------------------------------
