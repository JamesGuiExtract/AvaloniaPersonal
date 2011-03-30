//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericDisplayCtl.cpp
//
// PURPOSE:	This is an implementation file for GenericDisplayCtl() class.
//			Where the GenericDisplayCtl() class has been derived from CActiveXDocControl()
//			which is derived from COleControl() class.
//			The code written in this file makes it possible to implement the various
//			application methods in the control.
// NOTES:	
//
// AUTHORS:	
//
//-------------------------------------------------------------------------------------------------
// GenericDisplayCtl.cpp : Implementation of the CGenericDisplayCtrl ActiveX Control class.

#include "stdafx.h"
#include "GenericDisplay.h"
#include "GenericDisplayCtl.h"
#include "GenericDisplayPpg.h"
#include "GenericDisplayDocument.h"
#include "GenericDisplayView.h"
#include "GenericDisplayFrame.h"
#include "RubberBandThrd.h"

#include "LineEntity.h"
#include "SpCirEntity.h"
#include "TextEntity.h"
#include "CurveEntity.h"
#include "Rectangle.h"
#include "Point.h"
#include "ZoneEntity.h"
#include "UCLIDException.h"

#include <math.h>
#include <string.h>
#include <io.h>  // to use access method

const double PI = 3.1415926535897932384626433832795;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

IMPLEMENT_DYNCREATE(CGenericDisplayCtrl, CActiveXDocControl)

extern CGenericDisplayApp NEAR theApp;

//-------------------------------------------------------------------------------------------------
// Message map

BEGIN_MESSAGE_MAP(CGenericDisplayCtrl, CActiveXDocControl)
//{{AFX_MSG_MAP(CGenericDisplayCtrl)
ON_WM_CREATE()
ON_WM_DESTROY()
	//}}AFX_MSG_MAP
ON_OLEVERB(AFX_IDS_VERB_PROPERTIES, OnProperties)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Dispatch map
//-------------------------------------------------------------------------------------------------

BEGIN_DISPATCH_MAP(CGenericDisplayCtrl, CActiveXDocControl)
//{{AFX_DISPATCH_MAP(CGenericDisplayCtrl)
	DISP_FUNCTION(CGenericDisplayCtrl, "setDefaultUnits", setDefaultUnits, VT_EMPTY, VTS_BSTR)
	DISP_FUNCTION(CGenericDisplayCtrl, "zoomPush", zoomPush, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "zoomPop", zoomPop, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "zoomExtents", zoomExtents, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "zoomCenter", zoomCenter, VT_EMPTY, VTS_R8 VTS_R8)
	DISP_FUNCTION(CGenericDisplayCtrl, "zoomWindow", zoomWindow, VT_EMPTY, VTS_R8 VTS_R8 VTS_R8 VTS_R8 VTS_PI4)
	DISP_FUNCTION(CGenericDisplayCtrl, "setImage", setImage, VT_EMPTY, VTS_BSTR)
	DISP_FUNCTION(CGenericDisplayCtrl, "setScaleFactor", setScaleFactor, VT_EMPTY, VTS_R8)
	DISP_FUNCTION(CGenericDisplayCtrl, "getScaleFactor", getScaleFactor, VT_R8, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "setStatusBarText", setStatusBarText, VT_EMPTY, VTS_BSTR)
	DISP_FUNCTION(CGenericDisplayCtrl, "setRubberbandingParameters", setRubberbandingParameters, VT_EMPTY, VTS_I4 VTS_R8 VTS_R8)
	DISP_FUNCTION(CGenericDisplayCtrl, "enableRubberbanding", enableRubberbanding, VT_EMPTY, VTS_BOOL)
	DISP_FUNCTION(CGenericDisplayCtrl, "deleteEntity", deleteEntity, VT_EMPTY, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "clear", clear, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "getEntityAttributes", getEntityAttributes, VT_BSTR, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "modifyEntityAttribute", modifyEntityAttribute, VT_EMPTY, VTS_I4 VTS_BSTR VTS_BSTR)
	DISP_FUNCTION(CGenericDisplayCtrl, "addEntityAttribute", addEntityAttribute, VT_EMPTY, VTS_I4 VTS_BSTR VTS_BSTR)
	DISP_FUNCTION(CGenericDisplayCtrl, "deleteEntityAttribute", deleteEntityAttribute, VT_EMPTY, VTS_I4 VTS_BSTR)
	DISP_FUNCTION(CGenericDisplayCtrl, "getEntityColor", getEntityColor,  VT_COLOR, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "setEntityColor", setEntityColor, VT_EMPTY, VTS_I4 VTS_COLOR)
	DISP_FUNCTION(CGenericDisplayCtrl, "getEntityType", getEntityType, VT_BSTR, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "moveEntityBy", moveEntityBy, VT_EMPTY, VTS_I4 VTS_R8 VTS_R8)
	DISP_FUNCTION(CGenericDisplayCtrl, "getEntityNearestTo", getEntityNearestTo, VT_I4, VTS_R8 VTS_R8 VTS_PR8)
	DISP_FUNCTION(CGenericDisplayCtrl, "selectEntity", selectEntity, VT_EMPTY, VTS_I4 VTS_BOOL)
	DISP_FUNCTION(CGenericDisplayCtrl, "selectAllEntities", selectAllEntities, VT_EMPTY, VTS_BOOL)
	DISP_FUNCTION(CGenericDisplayCtrl, "getSelectedEntities", getSelectedEntities, VT_BSTR, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "setEntityText", setEntityText, VT_EMPTY, VTS_I4 VTS_BSTR)
	DISP_FUNCTION(CGenericDisplayCtrl, "queryEntities", queryEntities, VT_BSTR, VTS_BSTR)
	DISP_FUNCTION(CGenericDisplayCtrl, "getEntityText", getEntityText, VT_BSTR, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "getCurrentViewExtents", getCurrentViewExtents, VT_EMPTY, VTS_PR8 VTS_PR8 VTS_PR8 VTS_PR8)
	DISP_FUNCTION(CGenericDisplayCtrl, "getEntityExtents", getEntityExtents, VT_EMPTY, VTS_I4 VTS_PR8 VTS_PR8 VTS_PR8 VTS_PR8)
	DISP_FUNCTION(CGenericDisplayCtrl, "getDefaultUnits", getDefaultUnits, VT_BSTR, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "getImageName", getImageName, VT_BSTR, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "getStatusBarText", getStatusBarText, VT_BSTR, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "isRubberbandingEnabled", isRubberbandingEnabled, VT_BOOL, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "getRubberbandingParameters", getRubberbandingParameters, VT_BOOL, VTS_PI4 VTS_PR8 VTS_PR8)
	DISP_FUNCTION(CGenericDisplayCtrl, "convertWorldToImagePixelCoords", convertWorldToImagePixelCoords, VT_EMPTY, VTS_R8 VTS_R8 VTS_PI4 VTS_PI4 VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "convertImagePixelToWorldCoords", convertImagePixelToWorldCoords, VT_EMPTY, VTS_I4 VTS_I4 VTS_PR8 VTS_PR8 VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "getImageExtents", getImageExtents, VT_EMPTY, VTS_PR8 VTS_PR8)
	DISP_FUNCTION(CGenericDisplayCtrl, "setZoomFactor", setZoomFactor, VT_EMPTY, VTS_R8)
	DISP_FUNCTION(CGenericDisplayCtrl, "getZoomFactor", getZoomFactor, VT_R8, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "zoomIn", zoomIn, VT_EMPTY, VTS_PI4)
	DISP_FUNCTION(CGenericDisplayCtrl, "zoomOut", zoomOut, VT_EMPTY,VTS_PI4)
	DISP_FUNCTION(CGenericDisplayCtrl, "enableEntitySelection", enableEntitySelection, VT_EMPTY, VTS_BOOL)
	DISP_FUNCTION(CGenericDisplayCtrl, "isEntitySelectionEnabled", isEntitySelectionEnabled, VT_BOOL, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "enableZoneEntityCreation", enableZoneEntityCreation, VT_EMPTY, VTS_BOOL)
	DISP_FUNCTION(CGenericDisplayCtrl, "isZoneEntityCreationEnabled", isZoneEntityCreationEnabled, VT_BOOL, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "getZoneEntityParameters", getZoneEntityParameters, VT_EMPTY, VTS_I4 VTS_PI4 VTS_PI4 VTS_PI4 VTS_PI4 VTS_PI4)
	DISP_FUNCTION(CGenericDisplayCtrl, "setMinZoneWidth", setMinZoneWidth, VT_EMPTY, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "getMinZoneWidth", getMinZoneWidth, VT_I4, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "setZoneHighlightHeight", setZoneHighlightHeight, VT_EMPTY, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "getZoneHighlightHeight", getZoneHighlightHeight, VT_I4, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "setZoneHighlightColor", setZoneHighlightColor, VT_EMPTY, VTS_COLOR)
	DISP_FUNCTION(CGenericDisplayCtrl, "getZoneHighlightColor", getZoneHighlightColor, VT_COLOR, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "convertWorldToClientWindowPixelCoords", convertWorldToClientWindowPixelCoords, VT_EMPTY, VTS_R8 VTS_R8 VTS_PI4 VTS_PI4 VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "convertClientWindowPixelToWorldCoords", convertClientWindowPixelToWorldCoords, VT_EMPTY, VTS_I4 VTS_I4 VTS_PR8 VTS_PR8 VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "zoomInAroundPoint", zoomInAroundPoint, VT_EMPTY, VTS_R8 VTS_R8)
	DISP_FUNCTION(CGenericDisplayCtrl, "zoomOutAroundPoint", zoomOutAroundPoint, VT_EMPTY, VTS_R8 VTS_R8)
	DISP_FUNCTION(CGenericDisplayCtrl, "setXYTrackingOption", setXYTrackingOption, VT_EMPTY, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "documentIsModified", documentIsModified, VT_BOOL, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "getCurrentPageNumber", getCurrentPageNumber, VT_I4, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "setCurrentPageNumber", setCurrentPageNumber, VT_EMPTY, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "getTotalPages", getTotalPages, VT_I4, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "getEntityAttributeValue", getEntityAttributeValue, VT_BSTR, VTS_I4 VTS_BSTR)
	DISP_FUNCTION(CGenericDisplayCtrl, "getEntityInfo", getEntityInfo, VT_BSTR, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "addCurveEntity", addCurveEntity, VT_I4, VTS_R8 VTS_R8 VTS_R8 VTS_R8 VTS_R8 VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "addLineEntity", addLineEntity, VT_I4, VTS_R8 VTS_R8 VTS_R8 VTS_R8 VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "addSpecializedCircleEntity", addSpecializedCircleEntity, VT_I4, VTS_R8 VTS_R8 VTS_I4 VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "addTextEntity", addTextEntity, VT_I4, VTS_R8 VTS_R8 VTS_BSTR VTS_I2 VTS_R8 VTS_R8 VTS_BSTR VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "addZoneEntity", addZoneEntity, VT_I4, VTS_I4 VTS_I4 VTS_I4 VTS_I4 VTS_I4 VTS_I4 VTS_BOOL VTS_BOOL VTS_COLOR)
	DISP_FUNCTION(CGenericDisplayCtrl, "getBaseRotation", getBaseRotation, VT_R8, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "setBaseRotation", setBaseRotation, VT_EMPTY, VTS_I4 VTS_R8)
	DISP_FUNCTION(CGenericDisplayCtrl, "setDocumentModified", setDocumentModified, VT_EMPTY, VTS_BOOL)
	DISP_FUNCTION(CGenericDisplayCtrl, "setCursorHandle", setCursorHandle, VT_EMPTY, VTS_PHANDLE)
	DISP_FUNCTION(CGenericDisplayCtrl, "extractZoneEntityImage", extractZoneEntityImage, VT_EMPTY, VTS_I4 VTS_BSTR)
	DISP_FUNCTION(CGenericDisplayCtrl, "extractZoneImage", extractZoneImage, VT_EMPTY, VTS_I4 VTS_I4 VTS_I4 VTS_I4 VTS_I4 VTS_I4 VTS_BSTR)
	DISP_FUNCTION(CGenericDisplayCtrl, "getCurrentPageSize", getCurrentPageSize, VT_EMPTY, VTS_PI4 VTS_PI4)
	DISP_FUNCTION(CGenericDisplayCtrl, "isZoneBorderVisible", isZoneBorderVisible, VT_BOOL, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "showZoneBorder", showZoneBorder, VT_EMPTY, VTS_I4 VTS_BOOL)
	DISP_FUNCTION(CGenericDisplayCtrl, "getZoneBorderColor", getZoneBorderColor, VT_COLOR, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "setZoneBorderColor", setZoneBorderColor, VT_EMPTY, VTS_I4 VTS_COLOR)
	DISP_FUNCTION(CGenericDisplayCtrl, "isZoneFilled", isZoneFilled, VT_BOOL, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "setZoneFilled", setZoneFilled, VT_EMPTY, VTS_I4 VTS_BOOL)
	DISP_FUNCTION(CGenericDisplayCtrl, "setZoneEntityCreationType", setZoneEntityCreationType, VT_EMPTY, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "getZoneEntityCreationType", getZoneEntityCreationType, VT_I4, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "setZoom", setZoom, VT_EMPTY, VTS_R8)
	DISP_FUNCTION(CGenericDisplayCtrl, "getZoom", getZoomFactor, VT_R8, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "getZoneBorderStyle", getZoneBorderStyle, VT_I4, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "setZoneBorderStyle", setZoneBorderStyle, VT_EMPTY, VTS_I4 VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "isZoneBorderSpecial", isZoneBorderSpecial, VT_BOOL, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "setZoneBorderSpecial", setZoneBorderSpecial, VT_EMPTY, VTS_I4 VTS_BOOL)
	DISP_FUNCTION(CGenericDisplayCtrl, "getEntityPage", getEntityPage, VT_I4, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "zoomFitToWidth", zoomFitToWidth, VT_EMPTY, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "enableDisplayPercentage", enableDisplayPercentage, VT_EMPTY, VTS_BOOL)
	DISP_FUNCTION(CGenericDisplayCtrl, "isZoneEntityAdjustmentEnabled", isZoneEntityAdjustmentEnabled, VT_BOOL, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "enableZoneEntityAdjustment", enableZoneEntityAdjustment, VT_EMPTY, VTS_BOOL)
	DISP_FUNCTION(CGenericDisplayCtrl, "setFittingMode", setFittingMode, VT_EMPTY, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "getFittingMode", getFittingMode, VT_I4, VTS_NONE)
	DISP_FUNCTION(CGenericDisplayCtrl, "setSelectionColor", setSelectionColor, VT_EMPTY, VTS_I4)
	DISP_FUNCTION(CGenericDisplayCtrl, "getSelectionColor", getSelectionColor, VT_I4, VTS_NONE)
	//}}AFX_DISPATCH_MAP
DISP_FUNCTION_ID(CGenericDisplayCtrl, "AboutBox", DISPID_ABOUTBOX, AboutBox, VT_EMPTY, VTS_NONE)
END_DISPATCH_MAP()

//-------------------------------------------------------------------------------------------------
// Event map
//-------------------------------------------------------------------------------------------------

BEGIN_EVENT_MAP(CGenericDisplayCtrl, CActiveXDocControl)
//{{AFX_EVENT_MAP(CGenericDisplayCtrl)
EVENT_CUSTOM("EntitySelected", FireEntitySelected, VTS_I4)
EVENT_CUSTOM("MouseWheel", FireMouseWheel, VTS_I4  VTS_I4)
EVENT_CUSTOM("ZoneEntityMoved", FireZoneEntityMoved, VTS_I4)
EVENT_CUSTOM("ZoneEntitiesCreated", FireZoneEntitiesCreated, VTS_UNKNOWN)
EVENT_STOCK_CLICK()
EVENT_STOCK_DBLCLICK()
EVENT_STOCK_KEYDOWN()
EVENT_STOCK_KEYUP()
EVENT_STOCK_KEYPRESS()
EVENT_STOCK_MOUSEMOVE()
EVENT_STOCK_MOUSEDOWN()
EVENT_STOCK_MOUSEUP()
//}}AFX_EVENT_MAP
END_EVENT_MAP()

//-------------------------------------------------------------------------------------------------
// Property pages
//-------------------------------------------------------------------------------------------------

// TODO: Add more property pages as needed.  Remember to increase the count!
BEGIN_PROPPAGEIDS(CGenericDisplayCtrl, 1)
PROPPAGEID(CGenericDisplayPropPage::guid)
END_PROPPAGEIDS(CGenericDisplayCtrl)

//-------------------------------------------------------------------------------------------------
// Initialize class factory and guid
//-------------------------------------------------------------------------------------------------

IMPLEMENT_OLECREATE_EX(CGenericDisplayCtrl, "UCLIDGENERICDISPLAY.GenericDisplayCtrl.1",
					   0x14981576, 0x9117, 0x11d4, 0x97, 0x25, 0, 0x80, 0x48, 0xfb, 0xc9, 0x6e)
					   
					   
//-------------------------------------------------------------------------------------------------
// Type library ID and version
//-------------------------------------------------------------------------------------------------
					   
IMPLEMENT_OLETYPELIB(CGenericDisplayCtrl, _tlid, _wVerMajor, _wVerMinor)
					   
					   
//-------------------------------------------------------------------------------------------------
// Interface IDs
//-------------------------------------------------------------------------------------------------
					   
					   const IID BASED_CODE IID_DUCLIDGenericDisplay =
{ 0x14981574, 0x9117, 0x11d4, { 0x97, 0x25, 0, 0x80, 0x48, 0xfb, 0xc9, 0x6e } };
const IID BASED_CODE IID_DUCLIDGenericDisplayEvents =
{ 0x14981575, 0x9117, 0x11d4, { 0x97, 0x25, 0, 0x80, 0x48, 0xfb, 0xc9, 0x6e } };


//-------------------------------------------------------------------------------------------------
// Control type information
//-------------------------------------------------------------------------------------------------

static const DWORD BASED_CODE _dwGenericDisplayOleMisc =
OLEMISC_ACTIVATEWHENVISIBLE |
OLEMISC_SETCLIENTSITEFIRST |
OLEMISC_INSIDEOUT |
OLEMISC_CANTLINKINSIDE |
OLEMISC_RECOMPOSEONRESIZE;

IMPLEMENT_OLECTLTYPE(CGenericDisplayCtrl, IDS_GENERICDISPLAY, _dwGenericDisplayOleMisc)

//-------------------------------------------------------------------------------------------------
// CGenericDisplayCtrl::CGenericDisplayCtrlFactory::UpdateRegistry -
// Adds or removes system registry entries for CGenericDisplayCtrl
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayCtrl::CGenericDisplayCtrlFactory::UpdateRegistry(BOOL bRegister)
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
		IDS_GENERICDISPLAY,
		IDB_GENERICDISPLAY,
		afxRegApartmentThreading,
		_dwGenericDisplayOleMisc,
		_tlid,
		_wVerMajor,
		_wVerMinor);
	else
		return AfxOleUnregisterClass(m_clsid, m_lpszProgID);
}

//-------------------------------------------------------------------------------------------------
// call back function to enumerate the font families installed on the system. This call back method 
// is called from addTextEntity method and this method is mainly intended to raise an exception whenever
// an invalid font name is passed to create a text entity
//-------------------------------------------------------------------------------------------------
BOOL CALLBACK EnumFamCallBack(LPLOGFONT lplf, LPNEWTEXTMETRIC lpntm, DWORD FontType, LPVOID iaFontCount) 
{
	// this function will not be called if the font name passed in EnumFontFamilies() is not 
	// a valid font name to enumerate. Based on the aiFontCount[0] value, validity of the 
	// font name can be found
	
	// type cast to int pointer to increment the count
	int far * aiFontCount = (int far *) iaFontCount; 
	
	// increment the count, so that font name is valid name to enumerate
	aiFontCount[0]++;
	
    if (aiFontCount[0]) 
        return TRUE; 
    else 
        return FALSE; 
	
    UNREFERENCED_PARAMETER( lplf ); 
    UNREFERENCED_PARAMETER( lpntm ); 
}

//-------------------------------------------------------------------------------------------------
// CGenericDisplayCtrl::CGenericDisplayCtrl - Constructor
//-------------------------------------------------------------------------------------------------
CGenericDisplayCtrl::CGenericDisplayCtrl()
: m_bEntityAdjustment(FALSE), 
  m_ulMinZoneWidth(1), 
  m_eFittingMode(1) // OnCtrlDown
{
	InitializeIIDs(&IID_DUCLIDGenericDisplay, &IID_DUCLIDGenericDisplayEvents);
	
	//	set the initial size to control
	SetInitialSize(485, 400);
	
	//	set base rotation to zero
	m_mapPageNumToBaseRotation.clear();
	
	m_dScaleFactor = 1;
	
	//	set the default units
	m_iCurrentUnits = 1;
	
	//	set entity ID to zero
	m_ulLastUsedEntityID = 0;
	
	//	set stack size to -1
	m_iZoomStackSize = -1;
	
	m_zStatusBarText = "";	
	
	m_zCursorFileName = "";	
	
	m_bDocumentModified=FALSE;
	
	// disable the entity selection
	m_bEntitySelection = FALSE;
	
	// disable the zone creation
	m_bEnableZoneCreation = FALSE;
	
	// the minimum zone highlight height is 5
	m_ulZoneHighlightHt = 5;
	
	// initialize the zone highlight color with white
	m_colorZoneHighlight = 0xffffff; //RGB(255,255,255);
	
	// set the default value to false
	m_bScaleFactorChanged = FALSE;
	
	// set the minimum distance in view pixels is 3
	m_dMaxDistForEntSel = 3;
	
	// set the default tracking option as 1
	m_iTrackingOption = 1;

	// Set the display percentage flag as false
	m_bDisplayPercentage = false;
	
	m_pUGDFrame = NULL;
	
	m_pUGDView = NULL;
	
	m_hCursor = LoadCursor(NULL, IDC_ARROW);
	
	m_hIcon = NULL;
	
	m_lCurPageNumber = 0;

	m_zBackImageName = "";

	m_eZoneEntityCreationType = 1; // kAnyAngleZone

	//	add document template
    AddDocTemplate(new CActiveXDocTemplate(
        RUNTIME_CLASS(CGenericDisplayDocument),
        RUNTIME_CLASS(CGenericDisplayFrame),
        RUNTIME_CLASS(CGenericDisplayView)));
}

//-------------------------------------------------------------------------------------------------
// CGenericDisplayCtrl::~CGenericDisplayCtrl - Destructor
//-------------------------------------------------------------------------------------------------
CGenericDisplayCtrl::~CGenericDisplayCtrl()
{
	try
	{
		// delete all entities
		deleteAllEntities();

		// destroy the icon to release memory, loaded from files if any
		if (m_hIcon != __nullptr)
		{
			// destroys an icon and frees any memory the icon occupied
			DestroyIcon(m_hIcon);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16449");
}
//-------------------------------------------------------------------------------------------------
// CGenericDisplayCtrl::DoPropExchange - Persistence support
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::DoPropExchange(CPropExchange* pPX)
{
	ExchangeVersion(pPX, MAKELONG(_wVerMinor, _wVerMajor));
	COleControl::DoPropExchange(pPX);
}

//-------------------------------------------------------------------------------------------------
// CGenericDisplayCtrl::OnResetState - Reset control to default state
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::OnResetState()
{
	COleControl::OnResetState();  // Resets defaults found in DoPropExchange
}

//-------------------------------------------------------------------------------------------------
// CGenericDisplayCtrl::AboutBox - Display an "About" box to the user
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::AboutBox()
{
	CDialog dlgAbout(IDD_ABOUTBOX_GENERICDISPLAY);
	dlgAbout.DoModal();
}

//-------------------------------------------------------------------------------------------------
// CGenericDisplayCtrl message handlers
//-------------------------------------------------------------------------------------------------
int CGenericDisplayCtrl::OnCreate(LPCREATESTRUCT lpCreateStruct) 
{
	if (CActiveXDocControl::OnCreate(lpCreateStruct) == -1)
		return -1;
	
	m_pUGDFrame = (CGenericDisplayFrame*)CActiveXDocControl::m_pFrameWnd;
	
	m_pUGDView = (CGenericDisplayView*)m_pUGDFrame->GetActiveView();

	// link this control object to its view
	m_pUGDView->setGenericDisplayCtrl(this);

	// link this control object to its rubberband objects
	CRubberBandThrd* pRBThread = m_pUGDView->getRubberBandThread();
	if (pRBThread != __nullptr)
	{
		pRBThread->setGenericDisplayCtrl(this);
	}

	// link this control object to its zone objects
	CZoneHighlightThrd* pZHThread = m_pUGDView->getZoneObjectOfView();
	if (pZHThread != __nullptr)
	{
		pZHThread->setGenericDisplayCtrl(this);
	}

	// Link this control object to its zone adjustment object
	CZoneAdjustmentThrd* pZAThread = m_pUGDView->getZoneAdjustmentThread();
	if (pZAThread != __nullptr)
	{
		pZAThread->setGenericDisplayCtrl(this);
	}
	
	SetModifiedFlag(FALSE);

	return 0;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::OnDestroy() 
{
	CActiveXDocControl::OnDestroy();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setDefaultUnits(LPCTSTR zDefaultUnits) 
{
	double dScaleFactor = getScaleFactor();
	
	if(_stricmp(zDefaultUnits, "m") == 0)
	{
		// Set units to Meters
		m_iCurrentUnits = 2;
	}
	else if(_stricmp(zDefaultUnits, "ft") == 0)
	{
		// Set units to Feet
		m_iCurrentUnits = 1;
	}
	else
	AfxThrowOleDispatchException (0, "ELI90049: The specified units to set are not valid.The supported\
									units are 'ft' and 'm' only");

	// Update Multiplication factors
	updateMultFactors ();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::zoomPush() 
{
	//	check for maximum stack size
	if(m_iZoomStackSize < ZOOMSTACKSZ)
	{
		//	increment stack size by 1
		m_iZoomStackSize++;

		try
		{
			//	store the values in ZOOMPARAM structure of stack array
			m_stkZoomParam[m_iZoomStackSize].dZoomFactor	=	m_pUGDView->m_ImgEdit.GetZoom();
			m_stkZoomParam[m_iZoomStackSize].lScrollX		=	m_pUGDView->m_ImgEdit.GetScrollPositionX();
			m_stkZoomParam[m_iZoomStackSize].lScrollY		=	m_pUGDView->m_ImgEdit.GetScrollPositionY();
		}
		catch(COleDispatchException* ptr)
		{
			CString str = ptr->m_strDescription; 
			
			ptr->Delete();
			AfxThrowOleDispatchException(0, str);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::zoomPop() 
{
	long lScrollXValue;
	long lScrollYValue;
	
	//	check whether the parameters are avialable in the stack or not
	if(m_iZoomStackSize > -1)
	{
		//	get the corresponding values from the structure
		lScrollXValue = m_stkZoomParam[m_iZoomStackSize].lScrollX;
		lScrollYValue = m_stkZoomParam[m_iZoomStackSize].lScrollY;
		
		try
		{
			if(fabs (m_stkZoomParam[m_iZoomStackSize].dZoomFactor - m_pUGDView->getZoomPercentOfZoomExtents()) < 0.001)
				m_pUGDView->ZoomExtents();
			else
				m_pUGDView->setZoomFactor(m_stkZoomParam[m_iZoomStackSize].dZoomFactor);

			m_pUGDView->m_ImgEdit.SetScrollPositionX(lScrollXValue);
			m_pUGDView->m_ImgEdit.SetScrollPositionY(lScrollYValue);

			// update the scroll positions also
			m_pUGDView->OnScrollEditctrl1();
		}
		catch(COleDispatchException* ptr)
		{
			CString str = ptr->m_strDescription; 
			
			ptr->Delete();
			AfxThrowOleDispatchException(0, str);
		}
		
		//	decrement the stack size by 1
		m_iZoomStackSize--;
		
		// draw the entities
		m_pUGDView->m_ImgEdit.Invalidate();
		DrawEntities(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::zoomExtents() 
{
	checkForImageLoading("zoomExtents");

	//	set the image to the extents
	m_pUGDView->ZoomExtents();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::zoomFitToWidth() 
{
	checkForImageLoading("zoomFitToWidth");

	//	Set the image to fit to width
	m_pUGDView->ZoomToWidth();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::zoomCenter(double dCenterX, double dCenterY) 
{
	// apply multiplication factor
	dCenterX /= getXMultFactor();
	dCenterY /= getYMultFactor();
	
	//	set the image to the center to the given point
	m_pUGDView->zoomCenter(dCenterX, dCenterY);
}
//-------------------------------------------------------------------------------------------------

void CGenericDisplayCtrl::zoomWindow(double dBottomLeftX, double dBottomLeftY, 
									 double dTopRightX, double dTopRightY, long FAR* pnFitStatus)
{
	// NOTE: It is observed that when width and height of zoomWindow are not equal ( a ractangle)
	// the zoomwindow  will take maximum size for both width and height. For example if width is 100
	// and height is 50, then zoomWindow will zoom for 100 units in width and height. Better take a 
	// square window.
	
	// the input values are real world co-ordinates that means they are with respect to left bottom corner as
	// origin and the values mentioned are in the default units like "ft" or "meters"
	// Apply Multiplication factor

	checkForImageLoading("zoomWindow");

	// if a point is sent to the zoomWindow there will be no width and height
	// of the window to be zoomed. Then return the call. It might be an accidental click in ZoomWinndow mode.
	if (fabs(dBottomLeftX - dTopRightX) < 0.000001 && fabs (dBottomLeftY - dTopRightY) < 0.000001)
		return;

	//	check the given coordinates and set them properly
	if( dBottomLeftX > dTopRightX )
	{
		double dTempX	=	dBottomLeftX;
		dBottomLeftX	=	dTopRightX;
		dTopRightX		=	dTempX;
	}

	if( dBottomLeftY > dTopRightY )
	{
		double dTempY	=	dBottomLeftY;
		dBottomLeftY	=	dTopRightY;
		dTopRightY		=	dTempY;
	}

	double dImgHeight = m_pUGDView->getLoadedImageHeight();
	double dImgWidth = m_pUGDView->getLoadedImageWidth();

	// adjust the zoomwindow coordinates to fit into the zoomextents. Meaning if zoom window
	// coordinates are beyond the Image extents, set the zoom window coordinates to the 
	// maximum possible		
	if (dBottomLeftX < 0.001)
		dBottomLeftX = 0;
	if (dBottomLeftY < 0.001)
		dBottomLeftY = 0;

	double dXMax = getXMultFactor();
	double dYMax = getYMultFactor();

	dXMax *= dImgWidth;
	dYMax *= dImgHeight;

	if (dTopRightX > dXMax)
		dTopRightX = dXMax;
	if (dTopRightY > dYMax)
		dTopRightY = dYMax;

	// if the incoming ZoomWindow coordinates are equal to the image extents, then call
	// zoomExtents and return the call (SCR#268)

	if (dBottomLeftX == 0 
		&& fabs(dTopRightX - dXMax) < 0.001
		&& dBottomLeftY == 0 
		&& fabs(dTopRightY - dYMax) < 0.001)
	{
		// zoom window coordinates are either matching to the image extents or exceeding the
		// image extents. So call zoom extents
		zoomExtents();
		// Set pnFitStatus to 1 to indicate that it is now in fit to page status
		if (pnFitStatus)
		{
			*pnFitStatus = 1;
		}
		return;
	}

	if (dBottomLeftX == 0 
		&& fabs(dTopRightX - dXMax) < 0.001)
	{
		// zoom window coordinates are either matching to the image width or exceeding the
		// image width. So call zoom fit to width
		zoomFitToWidth();
		// Set pnFitStatus to 0 to indicate that it is now in fit to width status
		if (pnFitStatus)
		{
			*pnFitStatus = 0;
		}
		return;
	}

	double dminX = 0.0, dminY =0.0;
	long lminX = 0, lminY = 0;

	double dZoomWidth = 0.0, dZoomHeight = 0.0;

	double dRotation = getBaseRotation(m_lCurPageNumber);

	//	If base rotation is existing then calculate the bounding box co-ordinates.
	if(dRotation != 0.000000)
	{		
		// let us assume the given lower left points as x1, y1 and top right co-ordinates 
		// as x3, y3. Let us calculate x2, y2 and x4, y4 which will form the other two corners 
		// of the bounding box. After calculating the 4 corners in rotated image, assign the 
		// lower left and top right such that they occupy the bounding box corners in the non
		// rotated image.
		long lx1 = 0, lx2 = 0, lx3 = 0, lx4 = 0;
		long ly1 = 0, ly2 = 0, ly3 = 0, ly4 = 0;

		double x1 = dBottomLeftX, y1 = dBottomLeftY;
		double x2 = dTopRightX, y2 = dBottomLeftY;
		double x3 = dTopRightX, y3 = dTopRightY;
		double x4 = dBottomLeftX, y4 = dTopRightY;
		
		//convertWorldToClientWindowPixelCoords
		convertWorldToClientWindowPixelCoords(x1, y1, &lx1, &ly1, m_lCurPageNumber);
		convertWorldToClientWindowPixelCoords(x2, y2, &lx2, &ly2, m_lCurPageNumber);
		convertWorldToClientWindowPixelCoords(x3, y3, &lx3, &ly3, m_lCurPageNumber);
		convertWorldToClientWindowPixelCoords(x4, y4, &lx4, &ly4, m_lCurPageNumber);

		//	minimum values
		lminX = min ( lx1, lx2);
		lminX = min (lminX, lx3);
		lminX = min (lminX, lx4);
		
		lminY = min ( ly1, ly2);
		lminY = min (lminY, ly3);
		lminY = min (lminY, ly4);

		//	maximum values
		long lmaxX = 0, lmaxY = 0;
		lmaxX = max ( lx1, lx2);
		lmaxX = max (lmaxX, lx3);
		lmaxX = max (lmaxX, lx4);
		
		lmaxY = max ( ly1, ly2);
		lmaxY = max (lmaxY, ly3);
		lmaxY = max (lmaxY, ly4);

		double dX1 = lminX, dY1 = lminY;
		double dX2 = lmaxX, dY2 = lmaxY;

		//	get the zoom width and zoom height in view co-ordinates.
		dZoomWidth = (dX2 - dX1 ) / (m_pUGDView->m_ImgEdit.GetZoom()/100.0);
		dZoomHeight = (dY2 - dY1) / (m_pUGDView->m_ImgEdit.GetZoom()/100.0);

		//	increase the zoom width and zoom height by 1 pixel.
		dZoomWidth ++;
		dZoomHeight++;

		//	convert the minimum x and y values in world coordinates
		//	and save them.
		convertClientWindowPixelToWorldCoords(lminX, lminY, &dminX, &dminY, m_lCurPageNumber);
	}
	else
	{
		long lLeftX = 0, lLeftY = 0;
		long lRightX = 0, lRightY = 0;
		// get the image coordinates for the world coordinates
		convertWorldToImagePixelCoords(dBottomLeftX, dBottomLeftY, &lLeftX, &lLeftY, m_lCurPageNumber);
		convertWorldToImagePixelCoords(dTopRightX, dTopRightY, &lRightX, &lRightY, m_lCurPageNumber);
		
		// get the width and height of the window to zoom in pixels
		dZoomWidth = fabs((double)lRightX - lLeftX);
		dZoomHeight = fabs((double)lLeftY - lRightY);
	}
		
	// calculate the zoom percent to set to accommodate dZoomWidth or dZoomHeight that
	// is minimum among the both
	CRect rect;
	m_pUGDView->m_ImgEdit.GetClientRect(rect);

	double dClientRectWidth = 0.0;
	double dClientRectHeight = 0.0;

	if( m_pUGDView->m_ImgEdit.GetZoom() == m_pUGDView->getZoomPercentOfZoomExtents() )
	{
		int scrollHeightHorizontal	=  GetSystemMetrics(SM_CYHSCROLL);
		int scrollHeightVertical	=  GetSystemMetrics(SM_CXVSCROLL);

		//	get the width and height of the client rect.
		//	reduce width and height values by scroll bar size.
		dClientRectWidth = rect.Width() - scrollHeightHorizontal;
		dClientRectHeight = rect.Height() - scrollHeightVertical;

		// being the current zoom is too low, let us
		// decrement the increased values
		dZoomWidth --;
		dZoomHeight --;
	}
	else
	{
		//	get the width and height of the client rect.
		dClientRectWidth = rect.Width();
		dClientRectHeight = rect.Height();
	}

	double dZoomPercent = 0.0;

	//	get the maximum size from zoom height and zoom width.
	double dMaxSize = dZoomWidth > dZoomHeight ? dZoomWidth : dZoomHeight;


	//	get the zoom window ratio.
	double dZoomWndRatio = dZoomWidth/dZoomHeight;
	double dClientWndRatio = dClientRectWidth/dClientRectHeight;

	//	calculate the zoom percent value.
	if( dZoomWndRatio > dClientWndRatio )
		dZoomPercent = max(dClientRectWidth,dClientRectHeight) / dMaxSize;
	else
	{
		double dZoomWidthPercent = dClientRectWidth / dZoomWidth;
		double dZoomHeightPercent = dClientRectHeight / dZoomHeight;
		dZoomPercent = min (dZoomWidthPercent, dZoomHeightPercent);
	}

	// hide image before zoom
	m_pUGDView->HideImageEditWindow();

	// restrict the zoom percent to a maximum of 6490 to enable CTRL + MOUSE to work
	if (dZoomPercent > 65.0)
		dZoomPercent = 64.9;

	// set the zoom percent to the image and update the zoom factor
	m_pUGDView->setZoomFactor(dZoomPercent * 100);

	// set the scrolls so that the left and top of the window zoomed matches 
	// with the left and top of the client window respectively
	if(dRotation != 0.000000)
	{
		// get the top left corner for setting the scrolls
		convertWorldToClientWindowPixelCoords(dminX, dminY, &lminX, &lminY, m_lCurPageNumber);

		m_pUGDView->m_ImgEdit.SetScrollPositionX(lminX);
		m_pUGDView->m_ImgEdit.SetScrollPositionY(lminY);
	}
	else
	{
		// calculate how much we need to scroll to set the top left position of the zoomed
		// window to match with top left position of clent rect
		long lX = 0, lY = 0;

		// get the top left corner for setting the scrolls
		convertWorldToClientWindowPixelCoords(dBottomLeftX, dTopRightY, &lX, &lY, m_lCurPageNumber);
		
		m_pUGDView->m_ImgEdit.SetScrollPositionX(lX);
		m_pUGDView->m_ImgEdit.SetScrollPositionY(lY);
	}

	// update the scroll positions and draw the entities also
	m_pUGDView->OnScrollEditctrl1();

	// show image after zoom
	m_pUGDView->ShowImageEditWindow();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setImage(LPCTSTR zImageFileName) 
{
	//	check for the file existance
	if (!isValidFile(zImageFileName))
	{
		CString zError;
		zError.Format ("ELI90033: %s File not found.", zImageFileName);
		AfxThrowOleDispatchException (0, LPCTSTR(zError));
	}

	// for loading the first page as the default in the image
	m_pUGDView->initializeImageLoading(true);

	// the container application is responsible to save the contents, before
	// calling this method inorder to save the document
	if(m_zBackImageName.IsEmpty() == FALSE)
		clear();
	
	//	set the image name to empty
	m_zBackImageName.Empty();
	
	//	set current image
	m_zBackImageName	= zImageFileName;
		
	//	Set the image control to enable
	//m_pUGDView->m_ImgEdit.EnableWindow(TRUE);

	try
	{
		m_pUGDView->m_ImgEdit.SetImage (LPCTSTR(m_zBackImageName));

		m_ulTotalPages = m_pUGDView->m_ImgEdit.GetPageCount();
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
		ptr->Delete();

		AfxThrowOleDispatchException(0, str);
	}

	m_pUGDView->HideImageEditWindow();
	
	//load image as background
	m_pUGDView->LoadImage ();
	
	m_pUGDView->ShowImageEditWindow();
	m_pUGDView->m_ImgEdit.Invalidate();

	//	Update Image parameters
	updateImageParameters();

	// always set m_bDocumentModified to false when a new image is opened.
	m_bDocumentModified = false;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::updateMultFactors ()
{
	int iXRes, iYRes;
	double dScale = getScaleFactor();
	int iUnits = getCurrentUnits();
	
	getImageResolution (&iXRes, &iYRes);
	
	if (iUnits == 1)
	{
		// scale factor / (Dots per inch * No. of inches per feet)
		m_dXMultFactor = dScale / (iXRes * 12);
		m_dYMultFactor = dScale / (iYRes * 12);
	}
	if (iUnits == 2)
	{
		// scale factor / (Dots per inch * No. of inches per meter )
		m_dXMultFactor = dScale / (iXRes * 39.37007874);
		m_dYMultFactor = dScale / (iYRes * 39.37007874);
	}
	
	return;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setScaleFactor(double dScaleFactor) 
{
	//	check for the scale factor value
	if(!validateScaleFactorToSet(dScaleFactor))
	{
		AfxThrowOleDispatchException (0, "ELI90041: Invalid Scale Factor.");
	}
	
	if(getScaleFactor()!=dScaleFactor)
	{
		
		m_dScaleFactor = dScaleFactor;
		
		// Update Multiplication factors
		updateMultFactors ();
		
		// set the modified flag as TRUE
		m_bDocumentModified=TRUE;
		
		//	set modified flag
		SetModifiedFlag();
		
		DrawEntities(TRUE);
		
		// notify that scale facor has changed
		notifyScaleFactorChanged(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
double CGenericDisplayCtrl::getScaleFactor() 
{
	return m_dScaleFactor;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setBaseRotation(long nPageNum, double dBaseRotation) 
{
	CWaitCursor wait;

	//	check for the base rotation value range
	if(!validateBaseRotationToSet(dBaseRotation))
	{
		AfxThrowOleDispatchException (0, "ELI90003: Specify Base Rotation angle in the range [-360, 360].");
	}

	bool bModified = true;
	map<long, double>::iterator iter1 = m_mapPageNumToBaseRotation.find(nPageNum);
	// if the base rotation value for this page already set to the same value
	if (iter1 != m_mapPageNumToBaseRotation.end() && iter1->second == dBaseRotation)
	{
		// set modified flag to false
		bModified = false;
	}

	//	set base rotation value
	m_mapPageNumToBaseRotation[nPageNum] = dBaseRotation;
	
	map<unsigned long, GenericEntity *>::iterator iter;
	for (iter = mapIDToEntity.begin(); iter != mapIDToEntity.end(); iter++)
	{
		GenericEntity *pGenEnt = iter->second;

		// if the entity is already deleted continue
		if(pGenEnt == NULL)
			continue;

		//	get the entity name
		string strEntityName = pGenEnt->getDesc();
		
		if(strcmp(strEntityName.c_str(), "TEXT") == 0)
			pGenEnt->ComputeEntExtents();
	}

	// The setBaseRotation Haiku:
	// set base rotation call is to the current page, do refresh the window
	if (nPageNum == getCurrentPageNumber())
	{
		m_pUGDView->HideImageEditWindow();

		m_pUGDView->setBaseRotation(dBaseRotation);

		m_pUGDView->ShowImageEditWindow();
		updateImageParameters();

		m_pUGDView->m_ImgEdit.Invalidate();

		if (bModified)
		{
			// set the modified flag as TRUE
			m_bDocumentModified=TRUE;
			//	set modified flag
			SetModifiedFlag();
		}
	}

	updateMultFactors ();

	DrawEntities(TRUE);
}
//-------------------------------------------------------------------------------------------------
double CGenericDisplayCtrl::getBaseRotation(long nPageNum) 
{
	map<long, double>::iterator iter = m_mapPageNumToBaseRotation.find(nPageNum);
	if (iter == m_mapPageNumToBaseRotation.end())
	{
		m_mapPageNumToBaseRotation[nPageNum] = 0.0;
	}
	
	return m_mapPageNumToBaseRotation[nPageNum];
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::addCurveEntity(double dCenterX, double dCenterY, double dRadius, 
										 double dStartAngle, double dEndAngle, long nPageNum) 
{
	checkForImageLoading("addCurveEntity");

	// validate the parameters
	if(!validateCurveAnglesEquality(dStartAngle, dEndAngle))
		AfxThrowOleDispatchException (0, "ELI90034: Start and End angles are equal");

	if(!validateCurveAnglesToCreate(dStartAngle, dEndAngle))
	  AfxThrowOleDispatchException (0, "ELI90037: Input angles are not valid.Angles should be in the range of -360 to 360");

	if(!validateCurveRadiusToCreate(dRadius))
		AfxThrowOleDispatchException (0, "ELI90035: Radius value can not be negative.");
	
	//	Apply Multiplication factor
	dCenterX /= getXMultFactor();
	dCenterY /= getYMultFactor();
	dRadius /= ((getXMultFactor() + getXMultFactor())/2.0);
	
	//	create point object
	Point centerPt (dCenterX, dCenterY);
	
	if (dStartAngle <0.0 || dEndAngle <0.0)
	{
		if (dStartAngle <0.0) dStartAngle += 360;
		if (dEndAngle <0.0) dEndAngle += 360;
	}
	
	//	Increment the entity id by one
	m_ulLastUsedEntityID++;

	//	create a curve entity object with the obtained details
	CurveEntity *pCurveEnt = 
		new CurveEntity(m_ulLastUsedEntityID, centerPt, dRadius, dStartAngle, dEndAngle);
	
	//	Store the LineEntity object in map
	mapIDToEntity[m_ulLastUsedEntityID] = (GenericEntity *) pCurveEnt;
	
	// Now multiple instances of UGD can be created. so
	// connect this entity to the current control object.
	pCurveEnt->setGenericDisplayCtrl(this);
	
	//setting the Curve entity page number as that of the current page number
	pCurveEnt->setPage(nPageNum);
	
	// if the zone is added to the current page
	if (nPageNum == getCurrentPageNumber())
	{
		// set the modified flag as TRUE
		m_bDocumentModified=TRUE;
		
		// Compute curve extents
		pCurveEnt->ComputeEntExtents();
		
		//	Draw curve on the screen
		pCurveEnt->EntDraw (TRUE);
		
		//	set modified flag
		SetModifiedFlag();
	}
	
	//	return entity ID
	return m_ulLastUsedEntityID;
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::addTextEntity(double dInsertionPointX, double dInsertionPointY, LPCTSTR zText,
										short ucAlignment, double dRotationInDegrees, double dHeight, 
										LPCTSTR zFontName, long nPageNum) 
{
	checkForImageLoading("addTextEntity");

	// validate the parameters alignment, text height and the font name before creating the text entity
	// and raise exceptions if any of them are not valid to create a text entity.
	
	//	check for alignment value
	if(!validateTextAlignmentAnglesToCreate(ucAlignment))
	{
		AfxThrowOleDispatchException (0, "ELI90004: Invalid text alignment value.Must be within 1 and 9");
	}

	// Check for the validity of Text height
	if(!validateTextHeightToCreate(dHeight))
	{
		AfxThrowOleDispatchException (0, "ELI90022: Text height should be greater than zero.");
	}
	
	if (!ValidateFontName(zFontName))
	{
		// so call back function is not called and zFontName is an invalid font name
		// raise an exception and return
		CString zError;
		zError.Format("ELI90021: [%s] is not a valid font name", zFontName);
		AfxThrowOleDispatchException (0, LPCTSTR(zError));
	}
	
	//	Apply multiplication factor
	dInsertionPointX /= getXMultFactor();
	dInsertionPointY /= getYMultFactor();
	dHeight/= getXMultFactor();

	//	create point with the given values
	Point txtInsPt (dInsertionPointX, dInsertionPointY);
	
	//	Increment the entity id by one
	m_ulLastUsedEntityID++;

	//	create a text entity object with the obtained details
	TextEntity *pTextEnt = new TextEntity (m_ulLastUsedEntityID, txtInsPt, zText, 
		(unsigned char)ucAlignment, dRotationInDegrees, dHeight, zFontName);
	
	//	Store the LineEntity object in map
	mapIDToEntity[m_ulLastUsedEntityID] = (GenericEntity *) pTextEnt;
	
	// . Now multiple instances of UGD can be created. so
	// connect this entity to the current control object.
	pTextEnt->setGenericDisplayCtrl(this);
	
	//setting the Text entity page number as that of the current page number
	pTextEnt->setPage(nPageNum);
	
	// if the zone is added to the current page
	if (nPageNum == getCurrentPageNumber())
	{
		// set the modified flag as TRUE
		m_bDocumentModified=TRUE;
		
		//	Compute entity extents for text
		pTextEnt->ComputeEntExtents ();
		
		//	Draw Text on the screen
		pTextEnt->EntDraw (TRUE);
		
		//	set modified flag
		SetModifiedFlag();
	}
	
	//	return Entity ID
	return m_ulLastUsedEntityID;
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::addSpecializedCircleEntity(double dCenterX, double dCenterY, 
													 long iPercentRadius, long nPageNum) 
{
	checkForImageLoading("addSpecializedCircleEntity");

	//	Apply Multiplication factor 
	dCenterX /= getXMultFactor();
	dCenterY /= getYMultFactor();
	
	// to validate the percent radius 
	if(!validateSpCirclePercentRadiusToCreate(iPercentRadius))
	{
		CString zError = "ELI90038: The value of percentage radius is invalid.It should be greater than 0 and less than or equal to 100";
		AfxThrowOleDispatchException (0, LPCTSTR(zError)); 
	}
	
	//	create point with the given values
	Point centerPt (dCenterX, dCenterY);
	
	//	Increment the entity id by one
	m_ulLastUsedEntityID ++;

	//	create a circle entity object with the obtained details
	SpecializedCircleEntity *pSpCircEnt = 
		new SpecializedCircleEntity(m_ulLastUsedEntityID, centerPt, iPercentRadius);
	
	//	Store the LineEntity object in map
	mapIDToEntity[m_ulLastUsedEntityID] = (GenericEntity *) pSpCircEnt;
	
	// . Now multiple instances of UGD can be created. so
	// connect this entity to the current control object.
	pSpCircEnt->setGenericDisplayCtrl(this);
	
	//setting the SpCircle entity page number as that of the current page number
	pSpCircEnt->setPage(nPageNum);
	
	// if the zone is added to the current page
	if (nPageNum == getCurrentPageNumber())
	{
		// set the modified flag as TRUE
		m_bDocumentModified=TRUE;
		
		// Compute curve extents
		pSpCircEnt->ComputeEntExtents();
		
		//	set SpCircle entity draw vale to true
		pSpCircEnt->EntDraw (TRUE);
		
		//	set modified flag
		SetModifiedFlag();
	}

	//	return Entity Id
	return m_ulLastUsedEntityID;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setStatusBarText(LPCTSTR zStatusBarText) 
{
	//	set the status text
	m_pUGDFrame->statusText (0, zStatusBarText);
	
	m_zStatusBarText = CString(zStatusBarText);
	
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setRubberbandingParameters(long eRubberbandType, 
													 double dStartPointX, 
													 double dStartPointY)
{
	// Rubber banding lines are to be removed once a new set of parameters are
	// defined. So update the Rubberband to fix the bug reported at SCR#180
	// to delete the rubber banding lines drawn for the earlier setting.
	CRubberBandThrd* pThread = m_pUGDView->getRubberBandThread();
	if(pThread->IsRubberBandingEnabled() && pThread->IsRubberBandingParametersSet())
	{
		pThread->removeEarlierLines();
		enableRubberbanding(FALSE);
	}
	
	//	set rubber banding parameters
	pThread->setThreadParameters(eRubberbandType, dStartPointX, dStartPointY);
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setFittingMode(long eFittingMode)
{
	m_eFittingMode = eFittingMode;
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::getFittingMode()
{
	return m_eFittingMode;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setSelectionColor(long lColor)
{
	m_colorSelectionBorder = lColor;
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::getSelectionColor()
{
	return m_colorSelectionBorder;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::enableRubberbanding(BOOL bValue) 
{
	if(bValue)
	{
		if (!m_pUGDView->getRubberBandThread ()->IsRubberBandingParametersSet())
			AfxThrowOleDispatchException (0, "ELI90063: Rubber banding parameters are not set to enable rubber banding");
	}

	// get the current rubberbanding enability
	// if it is already disabled and the bValue is even false (disable), then we don't 
	// need to do anything. Just return the call
	if (m_pUGDView->getRubberBandThread ()->IsRubberBandingEnabled() == bValue)
		return;
	
	//	set enable for the rubber banding
	m_pUGDView->getRubberBandThread ()->enableRubberBand (bValue);
	
	// this is the clean up of rubber banding lines that are left over in control
	if(m_pUGDView->getRubberBandThread ()->IsRubberBandingParametersSet() && !bValue)
	{
		CRect rect;
		m_pUGDView->GetClientRect (&rect);
		m_pUGDView->InvalidateRect(&rect);
		m_pUGDView->BringWindowToTop();
	}
	
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::deleteEntity(long ulEntityID) 
{	
	// check for the element
	map<unsigned long, GenericEntity *>::iterator iter;
	iter = mapIDToEntity.find(ulEntityID);

	if (iter != mapIDToEntity.end())
	{
		GenericEntity *pGenEnt = iter->second;
		if (pGenEnt)
		{
			// only update the display if it's the current page
			if (pGenEnt->checkPageNumber())
			{
				// if the element is found
				// Remove the entity from display
				pGenEnt->EntDraw (FALSE);
			}
			
			// delete the object first
			delete pGenEnt;
			pGenEnt = NULL;
			
			// remove the key
			mapIDToEntity.erase(ulEntityID);
			
			// reduce the count 
			// if one entity is deleted, the ID of the deleted entity will
			// never be used again
			m_bDocumentModified=TRUE;
			
			DrawEntities(TRUE);
		}
		else
		{
			CString strText;
			strText.Format("ELI90005: Invalid entity ID passed to deleteEntity()."); 
			AfxThrowOleDispatchException (0, LPCTSTR(strText));
		}
	}
	else
	{
		CString strText;
		strText.Format("ELI90020: No entity found with id = %d" , ulEntityID); 
		AfxThrowOleDispatchException (0, LPCTSTR(strText));
	}

	//	set modifed flag
	SetModifiedFlag();
	
	//	refresh view
	m_pUGDView->Invalidate();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::deleteAllEntities() 
{
	map<unsigned long, GenericEntity *>::iterator iter;
	for (iter = mapIDToEntity.begin(); iter != mapIDToEntity.end(); iter++)
	{
		//	get the pointer to the generic entity object
		GenericEntity *pGenEnt = iter->second;

		// if the entity is already deleted continue
		if(pGenEnt == NULL)
			continue;

		// delete the object first
		delete pGenEnt;
		pGenEnt = NULL;
	}

	// clear the map
	mapIDToEntity.clear();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::clear() 
{
	// delete all the displayed entities
	deleteAllEntities();

	try
	{
		//	clear display of image control
		m_pUGDView->m_ImgEdit.ClearDisplay();
		
		//	disable the image edit control
		//m_pUGDView->m_ImgEdit.EnableWindow(FALSE);
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 

		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}

	//	set the status text
	m_pUGDFrame->statusText(1, " ");
	m_pUGDFrame->statusText(2, " ");
	m_pUGDFrame->statusText(3, " ");
	
	//	set the Back image to null
	m_zBackImageName.Empty();
			
	//	set number of entity ID's to zero
	m_ulLastUsedEntityID = 0;

	//	set base rotation to zero
	m_mapPageNumToBaseRotation.clear();
	
	//	set stack size to -1
	m_iZoomStackSize = -1;
	
	//	set modifed flag to FALSE
	SetModifiedFlag(FALSE);
	
	//	disable rubber banding
	enableRubberbanding(FALSE);
	
	//	set default rubberbanding parameters
	initRubberbandingParameters();
	
	// invalidete the view's client area to show the 
	// background color as white (SCR #175)
	CRect rect;
	m_pUGDView->GetClientRect (&rect);
	m_pUGDView->InvalidateRect(&rect);
	m_pUGDView->BringWindowToTop();
	m_pUGDView->setDeltaOriginX(0);
	m_pUGDView->setDeltaOriginY(0);
	
	// clear the status bar text
	//	get the Frame pointer and reset the text
	m_pUGDFrame->statusText(0,"");
	
	m_lCurPageNumber = 0;
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::addLineEntity(double dStartPointX, double dStartPointY, 
										double dEndPointX, double dEndPointY, long nPageNum) 
{
	checkForImageLoading("addLineEntity");

	//	Apply Multiplication factor
	dStartPointX	/= getXMultFactor();
	dStartPointY	/= getYMultFactor();
	dEndPointX		/= getXMultFactor();
	dEndPointY		/= getYMultFactor();
	
	//	store the start point and end point
	Point Pt1 (dStartPointX, dStartPointY);
	Point Pt2 (dEndPointX, dEndPointY);
	
	//	Increment the entity id by one
	m_ulLastUsedEntityID++;
	
	//	create a lineentity object with the obtained details
	LineEntity *pLineEnt = new LineEntity (m_ulLastUsedEntityID, Pt1, Pt2);
	
	//	Store the LineEntity object in map
	mapIDToEntity[m_ulLastUsedEntityID] = (GenericEntity *) pLineEnt;
	
	// . Now multiple instances of UGD can be created. so
	// connect this entity to the current control object.
	pLineEnt->setGenericDisplayCtrl(this);
	
	//setting the Line entity page number as that of the current page number
	pLineEnt->setPage(nPageNum);

	// if the zone is added to the current page
	if (nPageNum == getCurrentPageNumber())
	{	
		// set the modified flag as TRUE
		m_bDocumentModified=TRUE;
		
		//	Compute line extents
		pLineEnt->ComputeEntExtents();
		
		//	draw entities
		pLineEnt->EntDraw (TRUE);
		
		//	set modifed flag
		SetModifiedFlag();
	}	
	// Create and Return Entity ID
	return m_ulLastUsedEntityID;
}
//-------------------------------------------------------------------------------------------------
BSTR CGenericDisplayCtrl::getEntityAttributes(long ulEntityID) 
{
	CString zAttrStr;
	
	checkForImageLoading("getEntityAttributes");
	
	//	get the pointer to the generic entity object
	GenericEntity *pGenEnt = NULL;
	
	validateEntityID(ulEntityID, pGenEnt);
	
	//	get the attribute string
	pGenEnt->getAttributeString(zAttrStr, 0);
	
	//	return attribute string
	return zAttrStr.AllocSysString();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::modifyEntityAttribute(long ulEntityID, LPCTSTR zAttributeName, LPCTSTR zNewValue) 
{
	//	get the pointer to the generic entity object
	GenericEntity *pGenEnt = NULL;
	validateEntityID (ulEntityID, pGenEnt);
	
	//check whether attribute exists for the entity
	string strName;
	string strValue;
	
	//	assign the user defined attribute name and value to the string
	strName.assign(zAttributeName);
	strValue.assign(zNewValue);
	
	int iReturn = pGenEnt->modifyAttribute(strName, strValue) ;
	
	//	modify the user defined attribute value for the given attribute name
	if (iReturn == -1)
	{
		//if attribute is not defined for this entity throw an exception
		CString zError ;
		zError.Format("ELI90015: Entity %d does not have the [%s] attribute.",ulEntityID,zAttributeName);
		AfxThrowOleDispatchException (0, LPCTSTR(zError));
		
	}
	else if (iReturn == 1)
		m_bDocumentModified = true;
	
	//check whether attribute is hidden
	if (_strcmpi(zAttributeName, "Visible") == 0)
	{
		//check whether new value to set is either "1" or "0"
		if (_strcmpi(zNewValue, "1") == 0 || _strcmpi(zNewValue, "0") == 0)
		{
			// get the visible attribute as a boolean value
			bool bVal = false;
			if (_strcmpi(zNewValue, "1") == 0)
				bVal = true;
			
			bool bExistingVisibility = pGenEnt->getVisible();
			
			if (bExistingVisibility == bVal)
				return;
			
			// if bVal is true set the visibility of the entity before selectEntity()
			// is called so that the entity will be redrawn 
			if (bVal)
				pGenEnt->setVisible(bVal);
			
			//	draw the refreshed entities
			pGenEnt->EntDraw(TRUE);
			
			// if visible attribute is false, visibility is to be set after 
			// selectEntity() call is made
			if (!bVal)
			{
				pGenEnt->setVisible(bVal);
				pGenEnt->EntDraw(TRUE);
				m_pUGDView->m_ImgEdit.Invalidate();
				DrawEntities(TRUE);
			}
			
			// if the visibility flag changes, set the modified flag
			if (bVal != pGenEnt->getVisibilityAsInGddFile())
				pGenEnt->setVisibilityChangedAfterReadingFromFile(TRUE);
			else
				pGenEnt->setVisibilityChangedAfterReadingFromFile(FALSE);
			
		}
		else
			AfxThrowOleDispatchException (0, "ELI90008: The new value for 'Visible' attribute is not valid. Enter 0 or 1");
	}
	
	//	set modified flag
	SetModifiedFlag();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::addEntityAttribute(long ulEntityID, LPCTSTR zAttributeName, LPCTSTR zValue) 
{
	checkForImageLoading("addEntityAttribute");

	//	get the pointer to the generic entity object
	GenericEntity *pGenEnt = NULL;
	
	// validates the ID passed and raises an exception if invalid
	validateEntityID(ulEntityID, pGenEnt);
	
	string strName;
	string strValue;
	
	//	assign the user defined attribute name and value to the string
	strName.assign(zAttributeName);
	strValue.assign(zValue);
	
	EntityAttributes entAttrib = pGenEnt->getAttributes();
	
	map<string, string>::iterator iter = entAttrib.find(strName);
	if (iter != entAttrib.end())
		AfxThrowOleDispatchException (0, "ELI90225: Attribute name already defined");
	
	//	add the new attribute name and value
	pGenEnt->addAttribute(strName, strValue);
	
	m_bDocumentModified=TRUE;
	
	//	set modified flag
	SetModifiedFlag();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::deleteEntityAttribute(long ulEntityID, LPCTSTR zAttributeName) 
{
	//	get the pointer to the generic entity object
	GenericEntity *pGenEnt = NULL;
	
	// validates the ID passed and raises an exception if invalid
	validateEntityID(ulEntityID, pGenEnt);
	
	string strName;
	
	//	assign the user defined attribute name and value to the string
	strName.assign(zAttributeName);
	
	//	delete the given user defined attribute name and value
	if (!pGenEnt->deleteAttribute(strName))
	{
		// the attribute is not defined for this entity. So raise an exception
		CString zError;
		zError.Format ("ELI90031: Entity %d does not have [%s] attribute.",ulEntityID,zAttributeName); 
		AfxThrowOleDispatchException (0, LPCTSTR(zError));
	}
	
	m_bDocumentModified=TRUE;
	
	//	set modified flag
	SetModifiedFlag();
}
//-------------------------------------------------------------------------------------------------
OLE_COLOR CGenericDisplayCtrl::getEntityColor(long ulEntityID) 
{
	//	get the pointer to the generic entity object
	GenericEntity *pGenEnt = NULL;
	
	// validates the ID passed and raises an exception if invalid
	validateEntityID(ulEntityID, pGenEnt);
	
	// if the entity is a zone, return the current zone highlight color
	if(pGenEnt->getDesc() == "ZONE")
		return GetXORColor(pGenEnt->getColor());
	
	return (pGenEnt->getColor ());
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setEntityColor(long ulEntityID, OLE_COLOR newColor) 
{
	//	get the pointer to the generic entity object
	GenericEntity *pGenEnt = NULL;
	
	// validates the ID passed and raises an exception if invalid
	validateEntityID(ulEntityID, pGenEnt);
	
	// we need to set the color to XOR mode for zone entity. This is required to view 
	// the zone in the user requested color.
	if (pGenEnt->getDesc() == "ZONE")
	{
		COLORREF zoneColor = GetXORColor ((COLORREF)newColor);
		pGenEnt->setColor (zoneColor);
		if (pGenEnt->getPage() == getCurrentPageNumber())
		{
			pGenEnt->EntDraw(TRUE);
			m_pUGDView->m_ImgEdit.Invalidate();
			DrawEntities(TRUE);
		}
	}
	else
	{
		pGenEnt->setColor ((COLORREF)newColor);
		if (pGenEnt->getPage() == getCurrentPageNumber())
		{
			pGenEnt->EntDraw(TRUE);
		}
	}
	
	m_bDocumentModified=TRUE;
	
	//	set modified flag
	SetModifiedFlag();
}
//-------------------------------------------------------------------------------------------------
BSTR CGenericDisplayCtrl::getEntityType(long ulEntityID) 
{
	//	get the pointer to the generic entity object
	GenericEntity *pGenEnt = NULL;
	
	// validates the ID passed and raises an exception if invalid
	validateEntityID(ulEntityID, pGenEnt);
	
	CString strResult;
	
	strResult.Format("%s",(pGenEnt->getDesc()).c_str());
	
	return strResult.AllocSysString();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::moveEntityBy(long ulEntityID, double dX, double dY) 
{
	//	get the pointer to the generic entity object
	GenericEntity *pGenEnt = NULL;
	
	// validates the ID passed and raises an exception if invalid
	validateEntityID(ulEntityID, pGenEnt);
	
	double dtempX = getXMultFactor();
	double dtempY = getYMultFactor();
	
	pGenEnt->EntDraw(FALSE);
	
	// if the entity is a zone entity move the entity by image pixels
	if (pGenEnt->getDesc() == "ZONE")
		pGenEnt->offsetBy (dX, dY);
	else
		// move the entity by view pixels
		pGenEnt->offsetBy (dX/dtempX, dY/dtempY);
	
	pGenEnt->EntDraw(TRUE);
	m_pUGDView->m_ImgEdit.Invalidate();
	DrawEntities(TRUE);
	
	m_bDocumentModified=TRUE;
	
	//	set modified flag
	SetModifiedFlag();
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::getEntityNearestTo(double dX, double dY, double FAR* rDistance) 
{
	double dDistance;
	double dMinDistance = 0xFFFFFFFF;
	long lNearestEntID = 0XFFFFFFFF;
	
	dX /= getXMultFactor();
	dY /= getYMultFactor();
	
	map<unsigned long, GenericEntity *>::iterator iter;
	for (iter = mapIDToEntity.begin(); iter != mapIDToEntity.end(); iter++)
	{
		GenericEntity *pGenEnt = iter->second;
		
		// if the entity is already deleted continue
		if(pGenEnt == NULL)
			continue;
		
		//calculate the distance from the given pt to an entity
		dDistance = pGenEnt->getDistanceToEntity(dX, dY);
		
		//if the distance is less than the previous distance store the distance
		//store the entity id 
		if(dDistance <= dMinDistance)
		{
			dMinDistance = dDistance;
			lNearestEntID = iter->first;
		}
	}
	
	*rDistance = dMinDistance*((getXMultFactor() + getYMultFactor())/2.0);
	
	return lNearestEntID;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::selectEntity(long ulEntityID, BOOL bSelect) 
{
	//	get the pointer to the generic entity object
	GenericEntity *pGenEnt = NULL;
	
	// validates the ID passed and raises an exception if invalid
	validateEntityID(ulEntityID, pGenEnt);
	
	// If the entity is not changing selected status, it does not need to be redrawn
	if (pGenEnt->getSelected() == bSelect)
	{
		return;
	}

	// Set selected value
	pGenEnt->setSelected(bSelect);
	
	// Draw the refreshed entity only if it is on the same page [FlexIDSCore #3416]
	if (pGenEnt->getPage() == m_lCurPageNumber)
	{
		pGenEnt->EntDraw(FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::selectAllEntities(BOOL bSelect) 
{
	map<unsigned long, GenericEntity *>::iterator iter;
	for (iter = mapIDToEntity.begin(); iter != mapIDToEntity.end(); iter++)
	{
		GenericEntity *pGenEntity = iter->second;
		
		// if the entity is already deleted, continue
		if(pGenEntity == NULL)
			continue;
		
		selectEntity(iter->first, bSelect);
	}
}
//-------------------------------------------------------------------------------------------------
BSTR CGenericDisplayCtrl::getSelectedEntities() 
{
	CString strResult;

	map<unsigned long, GenericEntity *>::iterator iter;
	for (iter = mapIDToEntity.begin(); iter != mapIDToEntity.end(); iter++)
	{
		GenericEntity *pGenEntity = iter->second;
		
		// if the entity is already deleted continue
		if(pGenEntity == NULL)
			continue;
		
		if(pGenEntity->getSelected() == TRUE)
		{
			//	store the entity id whose highlight option is TRUE
			CString strTemp;
			strTemp.Format("%d", iter->first);
			strResult += strTemp + ",";
		}
	}

	// remove the appended "," from the end of the string. If 3 entities are highlighted
	// now it should return 1,2,3 instead of 1,2,3,
	if (!strResult.IsEmpty())
	{
		int iPos = strResult.ReverseFind (',');
		strResult = strResult.Left(iPos);
	}

	return strResult.AllocSysString();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setEntityText(long ulTextEntityID, LPCTSTR zNewText) 
{
	GenericEntity *pGenEnt = getEntity(ulTextEntityID);
	
	//	check for the element
	if (pGenEnt)
	{
		TextEntity *pTextEnt = (TextEntity *) pGenEnt;
		
		if (pTextEnt->getDesc() != "TEXT")
		{
			CString zError;
			zError.Format ("ELI90032: Entity %d is not a text entity", ulTextEntityID);
			AfxThrowOleDispatchException (0, LPCTSTR(zError));
		}
		
		//	Set new text
		string strText(zNewText);
		pTextEnt->setText(strText);
		
		//	Erase the earlier text
		pTextEnt->EntDraw(FALSE);
		
		//	Display the new text
		pTextEnt->EntDraw(TRUE);
		m_pUGDView->m_ImgEdit.Invalidate();
		DrawEntities(TRUE);
	}
	else
		AfxThrowOleDispatchException (0, "ELI90009: Invalid ID passed to setEntityText()");
	
	//	set modified flag
	SetModifiedFlag();
}
//-------------------------------------------------------------------------------------------------
BSTR CGenericDisplayCtrl::queryEntities(LPCTSTR zQuery) 
{	
	CString zResult;
	CString zEntId;
	CString zTemp;
	
	//	check for entity id = 0
	if(m_ulLastUsedEntityID == 0)
	{
		return zResult.AllocSysString();
	}
	
	for(unsigned int iEntId = 1; iEntId <= m_ulLastUsedEntityID; iEntId++)
	{
		GenericEntity *pGenEnt = NULL;
		
		//	get the entity using entity ID
		pGenEnt = mapIDToEntity[iEntId];
		
		// if the entity is already deleted continue
		if(pGenEnt == NULL)
			continue;
		
		if(pGenEnt->checkForEntAttr(zQuery) == TRUE)
		{
			zEntId.Empty();
			zEntId.Format("%d,", iEntId);
			zTemp += zEntId;
		}
	}

	//	set the string
	zResult = zTemp.Left(zTemp.GetLength() - 1);
	zResult.TrimRight();
	
	return zResult.AllocSysString();
}
//-------------------------------------------------------------------------------------------------
BSTR CGenericDisplayCtrl::getEntityText(long ulTextEntityID) 
{
	GenericEntity *pGenEnt = getEntity(ulTextEntityID);
		
	// check for the element
	if (pGenEnt)
	{		
		TextEntity *pTextEnt = (TextEntity *) pGenEnt;
		
		if (pTextEnt->getDesc() != "TEXT")
		{
			CString zError;
			zError.Format("ELI90029: The entity with ID = %d is not a text entity", ulTextEntityID);
			
			AfxThrowOleDispatchException (0, LPCTSTR(zError));
		}
		
		// Get text
		string strTemp;
		strTemp = pTextEnt->getText();
		CString strText(strTemp.c_str());
		return strText.AllocSysString();
	}
	else
		AfxThrowOleDispatchException (0, "ELI90010: Invalid ID passed to getEntityText()");
	
	return NULL;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::DrawEntities(BOOL bDraw)
{
	//	get the count of entities
	int iEntCount = mapIDToEntity.size();
	
	// get the number of pages in the loaded image
	// if more than 1 draw only entities saved in the current
	// page otherwise follow the existing logic	
	long ulTotalPages = getTotalPages();
	
	if (ulTotalPages > 1)
	{
		//	m_ulLastUsedEntityID is the latest entity ID  that was alloted.
		//  So if any entities are deleted, this ID ensures that 
		//  all the entities are drawn properly.
		map<unsigned long, GenericEntity *>::iterator iter;
		for (iter = mapIDToEntity.begin(); iter != mapIDToEntity.end(); iter++)
		{
			GenericEntity *pGenEnt = iter->second;
			
			if (pGenEnt)
			{
				if(pGenEnt->checkPageNumber())
				{
					pGenEnt->EntDraw(bDraw);
				}
			}
		}
	}
	else
	{
		//	m_ulLastUsedEntityID is the latest entity ID  that was alloted.
		//  So if any entities are deleted, this ID ensures that 
		//  all the entities are drawn properly.
		map<unsigned long, GenericEntity *>::iterator iter;
		for (iter = mapIDToEntity.begin(); iter != mapIDToEntity.end(); iter++)
		{
			GenericEntity *pGenEnt = iter->second;
			
			if (pGenEnt)
			{
				pGenEnt->EntDraw(bDraw);
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::getCurrentViewExtents(double FAR* rdBottomLeftX, double FAR* rdBottomLeftY, double FAR* rdTopRightX, double FAR* rdTopRightY) 
{
	
 	CRect rectView;
	
	//	get the current view rect in view coordinates
	m_pUGDView->m_ImgEdit.GetClientRect(&rectView);

	//	set the bottomleft and topright coordinates
	double dBottomLeftX		=	rectView.left;
	double dBottomLeftY		=	rectView.bottom;
	double dTopRightX		=	rectView.right;
	double dTopRightY		=	rectView.top;

	if (getBaseRotation(getCurrentPageNumber()))
	{
		// When base rotation is '0', minimum world coordinates are the lower left corner
		// of the view and the maximum world are at top-right corner. But,we are not sure 
		// about the minimum and maximum world coordinates when base rotation exists. 
		// So get the world coordinates of all the corners of view.

		// LEFT-TOP corner of the view
		double dLeftTopX = rectView.left , dLeftTopY = rectView.top;
		//	convert the view coordinates from view to image
		m_pUGDView->ViewToImageInWorldCoordinates(&dLeftTopX, &dLeftTopY, m_lCurPageNumber);
	
		// LEFT-BOTTOM corner of the view
		double dLeftBottomX = rectView.left , dLeftBottomY = rectView.bottom;
		//	convert the view coordinates from view to image
		m_pUGDView->ViewToImageInWorldCoordinates(&dLeftBottomX, &dLeftBottomY, m_lCurPageNumber);

		// RIGHT-BOTTOM corner of the view
		double dRightBottomX = rectView.right , dRightBottomY = rectView.bottom;
		//	convert the view coordinates from view to image
		m_pUGDView->ViewToImageInWorldCoordinates(&dRightBottomX, &dRightBottomY, m_lCurPageNumber);
	
		// RIGHT-TOP corner of the view
		double dRightTopX = rectView.right , dRightTopY = rectView.top;
		//	convert the view coordinates from view to image
		m_pUGDView->ViewToImageInWorldCoordinates(&dRightTopX, &dRightTopY, m_lCurPageNumber);

		// assign the minimum X and Y to the dBottomLeftX and dBottomLeftY respectively
		dBottomLeftX = dBottomLeftY = 0.0;

		dBottomLeftX = min (dLeftTopX, dLeftBottomX);
		dBottomLeftX = min (dBottomLeftX, dRightBottomX);
		dBottomLeftX = min (dBottomLeftX, dRightTopX);

		dBottomLeftY = min (dLeftTopY, dLeftBottomY);
		dBottomLeftY = min (dBottomLeftY, dRightBottomY);
		dBottomLeftY = min (dBottomLeftY, dRightTopY);

		// assign the maximum values of X and Y to the dTopRightX and dTopRightY respectively
		dTopRightX = dTopRightY = 0.0;

		dTopRightX = max (dLeftTopX, dLeftBottomX);
		dTopRightX = max (dTopRightX, dRightBottomX);
		dTopRightX = max (dTopRightX, dRightTopX);

		dTopRightY = max (dLeftTopY, dLeftBottomY);
		dTopRightY = max (dTopRightY, dRightBottomY);
		dTopRightY = max (dTopRightY, dRightTopY);
	}
	else
	{
		//	convert the view coordinates from view to image
		m_pUGDView->ViewToImageInWorldCoordinates(&dBottomLeftX, &dBottomLeftY, m_lCurPageNumber);
		m_pUGDView->ViewToImageInWorldCoordinates(&dTopRightX, &dTopRightY, m_lCurPageNumber);
	}
	
	// multiply the co-ordinate values with multiplication factors
	dBottomLeftX *= getXMultFactor();
    dBottomLeftY *= getYMultFactor();
	dTopRightX	*= getXMultFactor();
	dTopRightY *= getYMultFactor();

	// get the loaded image height and width
	double dImgWidth = m_pUGDView->getLoadedImageWidth();
	double dImgHeight = m_pUGDView->getLoadedImageHeight();
	
	// see that the values of the view extents don't exceed the
	// image boundaries.
	if( dBottomLeftX < 0.0001)
		dBottomLeftX = 0.0;
	
	if (dBottomLeftY < 0.0001)
		dBottomLeftY = 0.0;
	
	if (dTopRightX > dImgWidth * getXMultFactor())
		dTopRightX = dImgWidth * getXMultFactor();
	
	if (dTopRightY > dImgHeight * getYMultFactor())
		dTopRightY = dImgHeight * getYMultFactor();
	
	// now assign the values	
	*rdBottomLeftX	=	dBottomLeftX ;
	*rdBottomLeftY	=	dBottomLeftY;
	*rdTopRightX	=	dTopRightX;
	*rdTopRightY	=	dTopRightY;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::getEntityExtents(long ulEntityID, double FAR* rdBottomLeftX, double FAR* rdBottomLeftY, double FAR* rdTopRightX, double FAR* rdTopRightY) 
{
	GenericEntity *pGenEnt = NULL;
	
	// validates the ID passed and raises an exception if invalid
	validateEntityID(ulEntityID, pGenEnt);
	
	GDRectangle gdRect;
	
	//	get the entity extents
	pGenEnt->getExtents(gdRect);
	
	//	get the bottom left point of the extents rectangle
	Point btLeft = gdRect.getBottomLeft();
	
	//	get the top right point of the extents rectangle
	Point topRt = gdRect.getTopRight();
	
	//	copy the Bottom Left and Top Right coordinates of the ractangle
	//	into the given address.
	*rdBottomLeftX = btLeft.dXPos * getXMultFactor();
	*rdBottomLeftY = btLeft.dYPos * getYMultFactor();
	*rdTopRightX = topRt.dXPos * getXMultFactor(); 
	*rdTopRightY = topRt.dYPos * getYMultFactor();
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::getEntityPage(long ulEntityID) 
{
	GenericEntity *pGenEnt = getEntity(ulEntityID);
		
	// check for the element
	if (pGenEnt)
	{
		return pGenEnt->getPage();
	}

	return -1;
}
//-------------------------------------------------------------------------------------------------
GenericEntity *CGenericDisplayCtrl::getEntityByID (long ulEntityID)
{
	GenericEntity *pGenEnt = NULL;
	
	// validates the ID passed and raises an exception if invalid
	validateEntityID(ulEntityID, pGenEnt);
	
	return pGenEnt;
}
//-------------------------------------------------------------------------------------------------
COLORREF CGenericDisplayCtrl::getColorRefValue(CString zReadString)
{
	//	get the position upto color (RGB) value
	int iColorPos = zReadString.Find(',', 0);
	
	//	get the Red color value in the RGB
	int iColorRed = zReadString.Find('.', 0);
	int iColorRedValue = atoi(zReadString.Mid(0, iColorRed));
	
	//	get the Green color value in the RGB
	int iColorGreen = zReadString.Find('.', iColorRed + 1);
	int iColorGreenValue = atoi(zReadString.Mid(iColorRed + 1, iColorGreen - iColorRed));
	
	//	get the Blue color value in the RGB
	int iColorBlueValue = atoi(zReadString.Mid(iColorGreen + 1, iColorPos - iColorGreen));
	
	//	Convert the values into COLORREF
	return RGB(iColorRedValue, iColorGreenValue, iColorBlueValue);
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::initRubberbandingParameters()
{
	//	set rubber banding parameters
	m_pUGDView->getRubberBandThread ()->initThreadParameters();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::MoveCursorToNearestEntity()
{
	//	Get the present position of cursor
	POINT CurPoint;
	GetCursorPos(&CurPoint);
	
	// check whether cursor is within the client rect or not.Just return if cursor
	// is not in the client's rectangle
	CRect rect;
	m_pUGDView->m_ImgEdit.GetClientRect(&rect);
	m_pUGDView->m_ImgEdit.ScreenToClient(&CurPoint);
	
	if (!rect.PtInRect(CurPoint))
		return;
	
	//	convert the co-ordinates
	m_pUGDView->ViewToImageInWorldCoordinates ((int*)&CurPoint.x, (int*)&CurPoint.y, m_lCurPageNumber);
	
	// apply the scale factor to coordinate
	double dXCord, dYCord;
	dXCord = (double)CurPoint.x * getXMultFactor ();
	dYCord = (double)CurPoint.y * getYMultFactor ();
	
	//	get the nearest line or curve entity ID to the cursor position
	long lEntityID = 0;
	lEntityID = getNearestLineOrCurveEntity(dXCord, dYCord);
	
	//	if no entities are existing, return the function
	if(!lEntityID)
		return;
	
	GenericEntity* pEnt = NULL;

	//	get entity from the ID
	pEnt = getEntityByID(lEntityID);
	
	//	if the entity is a line or curve entity move the cursor to the start or end
	//	point which is nearer. this is again an additional check and not required
	if (pEnt && (pEnt->getDesc() == "LINE" || pEnt->getDesc() == "CURVE"))
		MoveCursorToStartOrEndPoint (pEnt, CurPoint);
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::MoveCursorToStartOrEndPoint (GenericEntity *pEnt, POINT& CurPoint)
{	
	// This function moves the cursor to the nearest Line or Curve entity's start point
	// or end point. If no line or curve entities exist no change is made to the cursor position
	
	// start and endpoints of the nearest entity
	Point stPt, endPt;
	
	// If the nearest entity is "LINE" entity.Move the cursor to the start or end point 
	// of the line which is nearest to the present cursor position.
	if (pEnt->getDesc() == "LINE")
	{
		LineEntity* pLineEnt = NULL;
		pLineEnt = (LineEntity*) pEnt;
		stPt = pLineEnt->getStartPoint();
		endPt = pLineEnt->getEndPoint();
	}
	// If the nearest entity is "CURVE" entity.Move the cursor to the start or end point 
	// of the curve which is nearest to the present cursor position.
	else if (pEnt->getDesc() == "CURVE")
	{
		CurveEntity* pCurveEnt = NULL;
		pCurveEnt = (CurveEntity*) pEnt;
		stPt  = pCurveEnt->getStartPoint();
		endPt = pCurveEnt->getEndPoint();
	}
	else
	{
		// the following code should never be executed.added here for additional
		// safety measure. If the entity is neither line nor curve return.
		return;
	}
	
	double stPtDistance  = sqrt((stPt.dXPos - CurPoint.x)*(stPt.dXPos - CurPoint.x) + (stPt.dYPos - CurPoint.y)*(stPt.dYPos - CurPoint.y)) ;
	double endPtDistance = sqrt((endPt.dXPos - CurPoint.x)*(endPt.dXPos - CurPoint.x) + (endPt.dYPos - CurPoint.y)*(endPt.dYPos - CurPoint.y));
	
	//	if start point is nearer move the cursor to the start point
	if (stPtDistance < endPtDistance)
	{
		// The original clicked point is transformed by ViewToImageInWorldCoordinates and ScreenToClient
		// methods. Now reverse the process before setting the cursor to new position.
		m_pUGDView->ImageInWorldToViewCoordinates(&stPt.dXPos, &stPt.dYPos, m_lCurPageNumber);
		
		POINT pt;
		// TESTTHIS casting to longs
		pt.x = (long)stPt.dXPos;
		pt.y = (long)stPt.dYPos;
		m_pUGDView->m_ImgEdit.ClientToScreen(&pt);
		SetCursorPos(pt.x, pt.y);
	}
	else
	{
		// The original clicked point is transformed by ViewToImageInWorldCoordinates and ScreenToClient
		// methods. Now reverse the process before setting the cursor to new position.
		m_pUGDView->ImageInWorldToViewCoordinates(&endPt.dXPos, &endPt.dYPos, m_lCurPageNumber);
		
		POINT pt;
		// TESTTHIS casting to longs
		pt.x = (long)endPt.dXPos;
		pt.y = (long)endPt.dYPos;
		m_pUGDView->m_ImgEdit.ClientToScreen(&pt);
		SetCursorPos(pt.x, pt.y);
	}
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::getNearestLineOrCurveEntity(double dX, double dY)
{
	double dDistance;
	double dMinDistance = 0xFFFFFFFF;
	long lNearestEntID = 0;
	
	dX	/= getXMultFactor();
	dY	/= getYMultFactor();
	
	map<unsigned long, GenericEntity *>::iterator iter;
	for (iter = mapIDToEntity.begin(); iter != mapIDToEntity.end(); iter++)
	{
		GenericEntity *pGenEnt = iter->second;
			
		// if the entity is already deleted or entity is not line entity and not a curve entity 
		// skip iteration as we are only concerned with line or Curve entities
		if(pGenEnt == NULL || (pGenEnt->getDesc() != "LINE" && pGenEnt->getDesc() != "CURVE"))
			continue;
		
		// if the entity is not in the current page, continue iteration
		if (pGenEnt->getPage() != getCurrentPageNumber())
			continue;
		
		// if the entity is not visible, continue iteration
		if (pGenEnt->getVisible() == false)
			continue;
		
		//calculate the distance from the given pt to an entity
		dDistance = pGenEnt->getDistanceToEntity(dX, dY);
		
		//if the distance is less than the previous distance store the distance
		//store the entity id 
		if(dDistance <= dMinDistance)
		{
			dMinDistance = dDistance;
			lNearestEntID = iter->first;
		}
	}

	return lNearestEntID;
}
//-------------------------------------------------------------------------------------------------
BSTR CGenericDisplayCtrl::getDefaultUnits() 
{
	CString strResult ;

	//	get default units as per the current units value.
	if(m_iCurrentUnits == 1)
		strResult ="ft";
	else if(m_iCurrentUnits == 2)
		strResult ="m";
	
	return strResult.AllocSysString();
}
//-------------------------------------------------------------------------------------------------
BSTR CGenericDisplayCtrl::getImageName() 
{
	//return the currently loaded image name. It can be even empty. 
	CString strResult = m_zBackImageName;
	
	return strResult.AllocSysString();
}
//-------------------------------------------------------------------------------------------------
BSTR CGenericDisplayCtrl::getStatusBarText() 
{
	// return the status bar text, it can even be empty
	return m_zStatusBarText.AllocSysString();
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayCtrl::isRubberbandingEnabled() 
{
	CRubberBandThrd* pRBThread = m_pUGDView->getRubberBandThread();
	if (pRBThread == NULL)
	{
		AfxThrowOleDispatchException (0, "ELI15625: Unable to retrieve rubber band thread.");
	}

	return pRBThread->IsRubberBandingEnabled();
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayCtrl::getRubberbandingParameters(long FAR* eRubberbandType, double FAR* dStartPointX, double FAR* dStartPointY) 
{
	CRubberBandThrd* pRBThread = m_pUGDView->getRubberBandThread();
	if (pRBThread == NULL)
	{
		AfxThrowOleDispatchException (0, "ELI15626: Unable to retrieve rubber band thread.");
	}

	//	check whether the rubber banding parameters were set or not.
	if (!pRBThread->IsRubberBandingParametersSet())
	{
		AfxThrowOleDispatchException (0, "ELI90062: Rubber banding parameters are not set.");
	}

	pRBThread->getThreadParameters( eRubberbandType, dStartPointX, dStartPointY );

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::convertWorldToImagePixelCoords(double dWorldX, double dWorldY, 
														 long FAR* rulImagePixelPosX, 
														 long FAR* rulImagePixelPosY, 
														 long nPageNum) 
{
	// input values are world coordinates. There origin is bottom left. input values can not be negative
	// convert the input values from
	// 1. real world coordinates to image world coordinates (which are also in world with bottom left as origin
	// 2. image world coordinates (with origin at lower left corner) to image pixel coordinate with
	//    origin at top left corner
	
	// validate the input parameters
	if ((dWorldX < 0.0) || (dWorldY < 0.0))
		AfxThrowOleDispatchException (0, "ELI90055: World coordinates can not be negative. Please check the input parameters");
	
	// check for the memory allocation
	if (rulImagePixelPosX == NULL || rulImagePixelPosY == NULL)
		AfxThrowOleDispatchException (0, "ELI90056: Memory not allocated for parameters to return results");
	
	// assign with the input values
	double dXVal = dWorldX;
	double dYVal = dWorldY;
	
	// 1.real world to image world
	dXVal /= getXMultFactor();
	dYVal /= getYMultFactor();
	
	// 2.change the height so as to change the origin from lower left to top left corner
	double dImageHeight = m_pUGDView->getLoadedImageHeight();
	dYVal = dImageHeight - dYVal;
	
	// Compute the X and Y value and ensure they are within the bounds of the image
	// [LRCAU #4726]
	long lXVal = min((long)ceil(dXVal), (long) m_pUGDView->getLoadedImageWidth());
	long lYVal = min((long)ceil(dYVal), (long) dImageHeight);
	
	// assign the results.
	// TESTTHIS cast to longs
	*rulImagePixelPosX = lXVal;
	*rulImagePixelPosY = lYVal;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::convertImagePixelToWorldCoords(long ulImagePixelPosX, long ulImagePixelPosY,
														 double FAR* rdWorldX, double FAR* rdWorldY, 
														 long nPageNum) 
{
	// input values are in image pixel coordinates. There origin is topleft corner.
	// input values can not be negative
	// convert the input values from
	// 1. image pixel to image world coordinates  (which are in world but bottom leftas origin
	// 2. image world coordinates (with origin at lower left corner) to world coordinates
	
	// validate the input parameters
	if ((ulImagePixelPosX < 0.0) || (ulImagePixelPosY < 0.0))
		AfxThrowOleDispatchException (0, "ELI90057: Image pixel coordinates can not be negative. Please check the input parameters");
	
	// check for the memory allocation
	if (rdWorldX == NULL || rdWorldY == NULL)
		AfxThrowOleDispatchException (0, "ELI90058: Memory not allocated for parameters to return results");
	
	// assign with input values
	*rdWorldX = ulImagePixelPosX;
	*rdWorldY = ulImagePixelPosY;
	
	// 1.set the origin as lower-left from top-left
	*rdWorldY = m_pUGDView->getLoadedImageHeight() - ulImagePixelPosY;
	
	//2. Image world to real world with scaling and image resolutions
	*rdWorldX *= getXMultFactor();
	*rdWorldY *= getYMultFactor();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::getImageExtents(double FAR* dWidth, double FAR* dHeight) 
{
	// returns the width and height of the current image in world units (taking the current
	// scale factor into account
	
	// get the client window rect from the view
	
	// the coordiante with maximum X and mimimum Y (in image coordinates)
	// is the top right corner in world coordinates. The image extents will 
	// be those coordinates
	
	// get current image width 
	double dImageWidth = m_pUGDView->getLoadedImageWidth(); // getLoadedImageWidth();
	double dImageHeight = 0;
	
	convertImagePixelToWorldCoords((long)dImageWidth, (long)dImageHeight, &dImageWidth, &dImageHeight, m_lCurPageNumber);
	
	*dWidth  = dImageWidth;
	*dHeight = dImageHeight;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setZoomFactor(double dZoomFactor) 
{
	if (dZoomFactor <= 0.0)
		AfxThrowOleDispatchException (0, "ELI90064: Invalid zoom factor.Should be greater than 0");
	
	checkForImageLoading("set zoom factor");
	
	// set the zoom factor
	m_pUGDView->setPercentChangeInMagnification (dZoomFactor);
}
//-------------------------------------------------------------------------------------------------
double CGenericDisplayCtrl::getZoomFactor() 
{
	// set the zoom factor
	return m_pUGDView->getPercentChangeInMagnification();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::zoomIn(long FAR* pnFitStatus) 
{
	checkForImageLoading("zoom In");
	
	m_pUGDView->ZoomIn();

	// Get the zoom percent of zooming extents
	double dPercentOfExtents = m_pUGDView->getZoomPercentOfZoomExtents();
	// Get the zoom percent of zooming to fit width
	double dPercentOfFitToWidth = m_pUGDView->getZoomPercentOfZoomFitToWidth();
	// Get the current zoom percent
	double dCurrentZoomFactor = m_pUGDView->getZoomFactor();

	if (fabs(dPercentOfExtents - dCurrentZoomFactor) < 0.001)
	{
		// If the current zoom percent is equal to zoom percent of zooming extents
		// set pnFitStatus to 1 to indicate it is in fit to page status
		*pnFitStatus = 1;
	}
	else if (fabs(dPercentOfFitToWidth - dCurrentZoomFactor) < 0.001)
	{
		// If the current zoom percent is equal to zoom percent of zooming width
		// set pnFitStatus to 0 to indicate it is in fit to width status
		*pnFitStatus = 0;
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::zoomOut(long FAR* pnFitStatus) 
{
	checkForImageLoading("zoom out");
	
	m_pUGDView->ZoomOut();

	// Get the zoom percent of zooming extents
	double dPercentOfExtents = m_pUGDView->getZoomPercentOfZoomExtents();
	// Get the zoom percent of zooming to fit width
	double dPercentOfFitToWidth = m_pUGDView->getZoomPercentOfZoomFitToWidth();
	// Get the current zoom percent
	double dCurrentZoomFactor = m_pUGDView->getZoomFactor();

	if (fabs(dPercentOfExtents - dCurrentZoomFactor) < 0.001)
	{
		// If the current zoom percent is equal to zoom percent of zooming extents
		// set pnFitStatus to 1 to indicate it is in fit to page status
		*pnFitStatus = 1;
	}
	else if (fabs(dPercentOfFitToWidth - dCurrentZoomFactor) < 0.001)
	{
		// If the current zoom percent is equal to zoom percent of zooming width
		// set pnFitStatus to 0 to indicate it is in fit to width status
		*pnFitStatus = 0;
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::enableEntitySelection(BOOL bEnable) 
{
	if (bEnable == TRUE)
	{
		// check whether image is currently loaded 
		checkForImageLoading("enable entity selection");
	}
	
	if (m_bEntitySelection != bEnable)
		m_bEntitySelection = bEnable;
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayCtrl::isEntitySelectionEnabled() 
{
	return m_bEntitySelection;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::enableZoneEntityAdjustment(BOOL bEnable) 
{
	if (bEnable == TRUE)
	{
		// check whether image is currently loaded 
		checkForImageLoading("enable entity adjustment");
	}
	
	if (m_bEntityAdjustment != bEnable)
	{
		m_bEntityAdjustment = bEnable;
		
		// Redraw entities so grip handles are shown/hidden immediately [FlexIDSCore #3373]
		DrawEntities(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayCtrl::isZoneEntityAdjustmentEnabled()
{
	return m_bEntityAdjustment;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::enableZoneEntityCreation(BOOL bEnable) 
{
	if (bEnable == TRUE)
	{
		// if bEnable is TRUE, check whether image is loaded into the control
		checkForImageLoading("enable zone entity creation");
	}
	m_bEnableZoneCreation = bEnable;
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayCtrl::isZoneEntityCreationEnabled() 
{
	return m_bEnableZoneCreation;
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::addZoneEntity(long ulStartPosX, long ulStartPosY, long ulEndPosX, 
										long ulEndPosY, long ulTotalZoneHeight, long nPageNum,
										BOOL bFill, BOOL bBorderVisible, COLORREF borderColor) 
{
	checkForImageLoading("addZoneEntity");

	// raise an exception if the width of the zone entity to be created is less than 
	// the minimum zone width.
	if(!validateMinZoneHeightToCreate(ulTotalZoneHeight))
	{
		AfxThrowOleDispatchException (0, "ELI90044: Zone height should be greater than or equal to 5");
	}
	if(!validateOddNumberZoneHeightToCreate(ulTotalZoneHeight))
	{
		AfxThrowOleDispatchException (0, "ELI90045: Zone height should be an odd number with minimum possible value of 5");
	}
	if(!validateZoneWidthToCreate(ulStartPosX, ulStartPosY, ulEndPosX, ulEndPosY))
	{
		AfxThrowOleDispatchException (0, "ELI90051: The width of zone to create is less than minimum allowed zone width");
	}
	
	//	Increment the entity id by one
	m_ulLastUsedEntityID++;

	// create a zone entity
	ZoneEntity* pZoneEnt = new ZoneEntity(m_ulLastUsedEntityID, ulStartPosX, ulStartPosY, 
		ulEndPosX, ulEndPosY, ulTotalZoneHeight, bFill == TRUE, bBorderVisible == TRUE, borderColor);
	
	// add to the map of generic entities
	//	Store the ZoneEntity object in map
	mapIDToEntity[m_ulLastUsedEntityID] = (GenericEntity *) pZoneEnt;
	
	// Now multiple instances of UGD can be created. so
	// connect this entity to the current control object.
	pZoneEnt->setGenericDisplayCtrl(this);
	
	//setting the Zone entity page number as that of the nPageNum
	pZoneEnt->setPage(nPageNum);
	
	//set the zone color to the default color (i.e. bright yellow)
	pZoneEnt->setColor ((COLORREF) getZoneColorInXORMode());
	
	// if the zone is added to the current page
	if (nPageNum == getCurrentPageNumber())
	{
		// set the modified flag as TRUE
		m_bDocumentModified=TRUE;

		// calculate the extents and updates the world coordinates of the zone
		pZoneEnt->ComputeEntExtents();
		
		// draw the entity
		pZoneEnt->EntDraw(TRUE);
	}
	
	// return the ID
	return m_ulLastUsedEntityID;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::getZoneEntityParameters(long nZoneEntityID, long FAR* rulStartPosX, long FAR* rulStartPosY, long FAR* rulEndPosX, long FAR* rulEndPosY, long FAR* rulTotalZoneHeight) 
{
	//get entity by query with ID
	GenericEntity* pEnt = NULL;
	pEnt = getEntityByID(nZoneEntityID);
	
	if (pEnt->getDesc() != "ZONE")
		AfxThrowOleDispatchException (0, "ELI90050: This is not a zone entity to return zone entity parameters");
	else
	{
		ZoneEntity * pzoneEnt = (ZoneEntity *)pEnt;
		
		pzoneEnt->getZoneEntParameters(*rulStartPosX, *rulStartPosY, *rulEndPosX, *rulEndPosY, *rulTotalZoneHeight);
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setMinZoneWidth(long ulMinZoneWidth) 
{
	if (ulMinZoneWidth <= 0)
		AfxThrowOleDispatchException (0, "ELI90043: The zone width to set should be greater than zero");

	//	set the minimum zone width with the given value.
	m_ulMinZoneWidth = ulMinZoneWidth;
}
//-------------------------------------------------------------------------------------------------
unsigned long CGenericDisplayCtrl::getMinZoneWidth() 
{
	return m_ulMinZoneWidth;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setZoneHighlightHeight(long ulNewHeight) 
{
	// validate the zone height to set before assigning
	if(!validateMinZoneHeightToCreate(ulNewHeight))
	{
		AfxThrowOleDispatchException (0, "ELI19452: Zone height should be greater than or equal to 5");
	}

	if(!validateOddNumberZoneHeightToCreate(ulNewHeight))
	{
		AfxThrowOleDispatchException (0, "ELI19453: Zone height should be an odd number with minimum possible value of 5");
	}
	
	//	set zone highlight height with the given value.
	m_ulZoneHighlightHt = ulNewHeight;
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::getZoneHighlightHeight() 
{
	// return the current zone highlight height value
	return m_ulZoneHighlightHt;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setZoneHighlightColor(OLE_COLOR newColor) 
{
	// the draw mode of zone is XOR which inverts the given color. In order to view
	// the zone in the given color invert and store for drawing the zone.
	// the zone drawing code will have to invert the value again. Finally the zone will
	// be drawn with the given color due to two inversions. 
	m_colorZoneHighlight = GetXORColor ((COLORREF) newColor);
}
//-------------------------------------------------------------------------------------------------
OLE_COLOR CGenericDisplayCtrl::getZoneHighlightColor() 
{
	return (OLE_COLOR)m_colorZoneHighlight^ 0xFFFFFF;
}

//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::convertWorldToClientWindowPixelCoords(double dWorldX, double dWorldY, 
																long FAR* rulClientWndPosX, 
																long FAR* rulClientWndPosY,
																long nPageNum) 
{
	// input values are world coordinates. There origin bottomleft. input values can not be negative
	// convert the input values from
	// 1. real world coordinates to image coordinates (which are also in world with 0,0 as origin
	// 2. image coordinates to the view coordinates
	
	// validate the input parameters
	if ((dWorldX < 0.0) || (dWorldY < 0.0))
		AfxThrowOleDispatchException (0, "ELI90054: World coordinates can not be negative. Please check the input parameters");
	
	// check for the memory allocation
	if (rulClientWndPosX == NULL || rulClientWndPosY == NULL)
		AfxThrowOleDispatchException (0, "ELI90052: Memory not allocated for parameters to return results");
	
	// these local variables are required here to pass with ImageInWorldToViewCoordinates()
	// call which takes either int* or double* as arguments
	double dXVal = dWorldX;
	double dYVal = dWorldY;
	
	// 1. real world to image in world coordinates
	dXVal /= getXMultFactor();
	dYVal /= getYMultFactor();
	
	// 2. image to view or client window coordinates
	m_pUGDView->ImageInWorldToViewCoordinates (&dXVal, &dYVal, nPageNum);
	
	// TESTTHIS casting to longs
	*rulClientWndPosX = (long)dXVal;
	*rulClientWndPosY = (long)dYVal;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::convertClientWindowPixelToWorldCoords(long ulClientWndPosX, 
																long ulClientWndPosY, 
																double FAR* rdWorldX, 
																double FAR* rdWorldY,
																long nPageNum) 
{
	// input values are view coordinates. convert them from
	// 1. view to image in world coordinates. then from 
	// 2. Image coordinates (which are world) to real world coordinates
	
	// check for the memory allocation
	if (rdWorldX == NULL || rdWorldY == NULL)
		AfxThrowOleDispatchException (0, "ELI90053: Memory not allocated for parameters to return results");
	
//	if (ulClientWndPosX < 0 || ulClientWndPosY < 0)
//		AfxThrowOleDispatchException (0, "ELI90059: View coordinates can not be negative");
	
	*rdWorldX = ulClientWndPosX;
	*rdWorldY = ulClientWndPosY;
	
	// 1. view to image
	m_pUGDView->ViewToImageInWorldCoordinates (rdWorldX, rdWorldY, nPageNum);
	
	// 2.image to real world with scaling and image resolutions
	*rdWorldX *= getXMultFactor();
	*rdWorldY *= getYMultFactor();
	
	//  world coordinates are not negative. Reset the values to zero if there is any minor differences
	if (*rdWorldX < 0.0)
		*rdWorldX = 0.0;
	if (*rdWorldY < 0.0)
		*rdWorldY = 0.0;
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayCtrl::validateMinZoneHeightToCreate(long ulTotalZoneHeight)
{
	//	check for the minimum zone height.
	if (ulTotalZoneHeight < 5)
		return false;

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayCtrl::validateOddNumberZoneHeightToCreate(long ulTotalZoneHeight)
{
	//	check for the zone height should be odd number.
	if (ulTotalZoneHeight % 2 == 0)
		return false;

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayCtrl::validateZoneWidthToCreate(unsigned long ulStPosX, unsigned long ulStPosY, unsigned long ulEndPosX, unsigned long ulEndPosY)
{
	//	calculate the zone width.
	unsigned long ulZoneWidth = (unsigned long)sqrt(pow((long double)ulEndPosX - (long double)ulStPosX, 2) + pow((long double)ulEndPosY - (long double)ulStPosY, 2));
	
	if (ulZoneWidth < getMinZoneWidth())
		return false;
	
	return true;
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::getNearestEntityIDFromPoint(double dX, double dY)
{
	double dMinDistance = 0xFFFFFFFF;
	double dDistance;
	long lNearestEntID = 0;
	
	bool bNearestEntIsLineOrCurve = FALSE;
	
	map<unsigned long, GenericEntity *>::iterator iter;
	for (iter = mapIDToEntity.begin(); iter != mapIDToEntity.end(); iter++)
	{
		GenericEntity *pGenEntity = iter->second;
					
		// if the entity is already deleted, continue
		if(pGenEntity == NULL)
			continue;

		// if the entity is a Text or Zone check whether the point is within
		// the region
		if(pGenEntity->getDesc() == "TEXT" && pGenEntity->getPage() == getCurrentPageNumber())
		{
			TextEntity *pTextEnt = (TextEntity*)pGenEntity;
			// TESTTHIS casting to int
			if(pTextEnt->isPtOnText((int)dX, (int)dY))
				return iter->first;
		}
		else if (pGenEntity->getDesc() == "ZONE" && pGenEntity->getPage() == getCurrentPageNumber())
		{
			ZoneEntity *pZoneEnt = (ZoneEntity*)pGenEntity;
			// TESTTHIS casting to ints
			if(pZoneEnt->isPtOnZone((int)dX, (int)dY))
				return iter->first;
		}
		else if (pGenEntity->getPage() == getCurrentPageNumber())
		{
			bNearestEntIsLineOrCurve = TRUE;
			
			dDistance = pGenEntity->getDistanceToEntity(dX, dY);
			
			//if the distance is less than the previous distance store the distance
			if(dDistance <= dMinDistance)
			{
				dMinDistance = dDistance;
				lNearestEntID = iter->first;
			}
		}
	}

	if(bNearestEntIsLineOrCurve)
	{
		dMinDistance *= m_pUGDView->getZoomFactor()/100;
		// if distance is '0', the point is on the entity with ID as lEntityId
		// else if distance is not equal to '0', then see whether distance is within
		// a limit of m_dMaxDistForEntSel. This is pirticularly required for line or curve entities
		
		if (dMinDistance >= 0.0 && dMinDistance<= m_dMaxDistForEntSel)
			return lNearestEntID;
	}

	return 0;
}
//-------------------------------------------------------------------------------------------------
EGripHandle CGenericDisplayCtrl::getGripHandle(double dX, double dY)
{
	ZoneEntity* pZoneEntity = NULL;
	int gripHandleId = getGripHandleId(dX, dY, &pZoneEntity);
	if (gripHandleId >= 0)
	{
		return pZoneEntity->getGripHandle(gripHandleId);
	}

	return kNone;
}
//-------------------------------------------------------------------------------------------------
int CGenericDisplayCtrl::getGripHandleId(double dX, double dY, ZoneEntity** ppZoneEntity)
{
	if (ppZoneEntity == NULL)
	{
		AfxThrowOleDispatchException(0, "Invalid zone entity.");
	}

	map<unsigned long, GenericEntity*>::iterator iter;
	for (iter = mapIDToEntity.begin(); iter != mapIDToEntity.end(); iter++)
	{
		GenericEntity *pGenEntity = iter->second;
					
		// Skip this entity, if it is any one of the following:
		// 1) Not a zone entity
		// 2) Not selected
		// 3) Not on the current page
		if (pGenEntity == NULL || !pGenEntity->getSelected() || 
			pGenEntity->getPage() != getCurrentPageNumber() || pGenEntity->getDesc() != "ZONE")
		{
			continue;
		}

		// Check if the specified point is over a grip handle
		ZoneEntity* pZoneEnt = (ZoneEntity*)pGenEntity;
		int gripHandle = pZoneEnt->getGripHandleId(dX, dY);
		if (gripHandle >= 0)
		{
			*ppZoneEntity = pZoneEnt;
			return gripHandle;
		}
	}

	return -1;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::checkForImageLoading(CString zFunctionName)
{
	// raises an exception if image is NOT currently loaded into the control
	if(m_zBackImageName.IsEmpty())
	{
		CString zError = "ELI90048: No image is currently loaded ";
		// add the function name to the error
		if (!zFunctionName.IsEmpty())
		{
			zError += "to ";
			zError += zFunctionName;
		}
		AfxThrowOleDispatchException (0, LPCTSTR(zError));
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::validateEntityID (long ulEntityID, GenericEntity*& pGenEnt)
{
	//	search the given entity ID.
	pGenEnt = getEntity(ulEntityID);
	if (!pGenEnt)
	{
		CString strText;
		strText.Format("ELI90007: No entity found with ID = %d" , ulEntityID); 
		AfxThrowOleDispatchException (0, LPCTSTR(strText));
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::zoomInAroundPoint(double dX, double dY) 
{
	long lClientWndPosX=0;
	long lClientWndPosY=0;

	// Converting the World coordinates to the View coordinates
	convertWorldToClientWindowPixelCoords(dX, dY, &lClientWndPosX, &lClientWndPosY, m_lCurPageNumber);

	// change the client window coordinates to the screen coordinates to suit to the 
	// ZoomInAroundPoint() implemented in View class
	POINT pt;
	pt.x = lClientWndPosX;
	pt.y = lClientWndPosY;
	ClientToScreen(&pt);

	// Passing the View coordinates 'ZoomInAroundTheClickedPoint'
	m_pUGDView->ZoomInAroundTheClickedPoint(pt.x,pt.y);
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::zoomOutAroundPoint(double dX, double dY) 
{
	long lClientWndPosX=0;
	long lClientWndPosY=0;

	// Converting the World coordinates to the View coordinates
	convertWorldToClientWindowPixelCoords(dX, dY, &lClientWndPosX, &lClientWndPosY, m_lCurPageNumber);

	// change the client window coordinates to the screen coordinates to suit to the 
	// ZoomOutAroundPoint() implemented in View class
	POINT pt;
	pt.x = lClientWndPosX;
	pt.y = lClientWndPosY;
	ClientToScreen(&pt);

	// Passing the View coordinates 'ZoomOutAroundTheClickedPoint'
	m_pUGDView->ZoomOutAroundTheClickedPoint(pt.x,pt.y);
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setXYTrackingOption(long iTrackingOption) 
{
	//	set the X and Y coordinates tracking option.
	if((iTrackingOption>=0)&&(iTrackingOption<=3))
	{
		m_iTrackingOption=iTrackingOption;
	}
	else
	{
		CString zError = "ELI90229: Reset Tracking option in the range 0 - 3";
		AfxThrowOleDispatchException (0, LPCTSTR(zError));
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::enableDisplayPercentage(BOOL bEnable) 
{
	m_bDisplayPercentage = bEnable;
}
//-------------------------------------------------------------------------------------------------
unsigned long CGenericDisplayCtrl::getXYTrackingOption()
{
	return m_iTrackingOption;
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayCtrl::getDisplayPercentage()
{
	return m_bDisplayPercentage;
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayCtrl::documentIsModified() 
{
	//get the loaded image and check whether the image is changed with setImage()
	//Check whether any entity has been added or deleted in the loaded image
	//Check whether any entity attribute has been modified
	
	bool bVisibilityModified = false;
	
	//	get the count of entities
	int iEntCount = mapIDToEntity.size();
	
    // Checking whether the image is changed or any entity is added or entity is deleted
	if (m_bDocumentModified || bVisibilityModified)
		return 	true;
	else 
		return false;
	
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::getCurrentPageNumber() 
{
	checkForImageLoading("getCurrentPageNumber");
	
	try
	{
		//	get the currently displayed page number.
		if (m_lCurPageNumber <= 0)
		{
			m_lCurPageNumber = m_pUGDView->m_ImgEdit.GetPage();
		}
	}
	catch(COleDispatchException* ptr)
	{
		CString str = ptr->m_strDescription; 
	
		ptr->Delete();
		
		AfxThrowOleDispatchException(0, str);
	}

	return m_lCurPageNumber;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setCurrentPageNumber(long ulPageNumber) 
{
	// 1 check whether the page falls in the page count of the image
	// 2 other wise throw an exception
	// 3 then set the page
	// 4 then set the dependency parameters like showing the entities of the page to set
	// 5 also set the current base rotation to the new page 
	if(getTotalPages() < 1)
	{
		CString zError = "ELI90233: There are no pages in the Loaded Image ";
		AfxThrowOleDispatchException (0, LPCTSTR(zError));
	}
	else if (ulPageNumber > getTotalPages())
	{
		CString zError = "ELI90234: The Page Number exceeds the Page Count of the Loaded Image ";
		AfxThrowOleDispatchException (0, LPCTSTR(zError));
	}
	else if (ulPageNumber < 1)
	{
		AfxThrowOleDispatchException (0, "ELI90235: Page Number can not be less than 1");
	}
	else 
	{
		if (ulPageNumber == m_lCurPageNumber)
		{
			return;
		}
		
		try
		{
			// in case if the page has a base rotation angle other than
			// 0, then we need to rotate the image before showing the
			// actual view of the page so that we can avoid the ellapsing
			// view of un-rotated page.
			m_pUGDView->HideImageEditWindow();
			//	set the current page of the image.
			m_pUGDView->m_ImgEdit.SetPage(ulPageNumber);
			m_pUGDView->FitToParent(TRUE);
			m_pUGDView->setZoomFactor(m_pUGDView->m_ImgEdit.GetZoom());
			m_pUGDView->LoadImage();
			updateImageParameters();
		}
		catch(COleDispatchException* ptr)
		{
			m_pUGDView->ShowImageEditWindow();

			CString str = ptr->m_strDescription; 
			ptr->Delete();
			AfxThrowOleDispatchException(0, str);
		}
		
		m_lCurPageNumber = ulPageNumber;
			
		// set base rotation of the page if rotation angle is other than 0
		double dRotation = getBaseRotation(ulPageNumber);
		setBaseRotation(ulPageNumber, dRotation);
		
		m_pUGDView->ShowImageEditWindow();

		//	disable rubber band effect.
		CRubberBandThrd* pRBThread = m_pUGDView->getRubberBandThread();
		if (pRBThread == NULL)
		{
			AfxThrowOleDispatchException (0, "ELI15627: Unable to retrieve rubber band thread.");
		}

		pRBThread->enableRubberBand(false);
	}
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::getTotalPages() 
{
	//returning the total number of pages in the opened image
	checkForImageLoading("getTotalPages");
	return m_ulTotalPages;
}
//-------------------------------------------------------------------------------------------------
COLORREF CGenericDisplayCtrl::GetXORColor(COLORREF color)
{
	COLORREF XORColor;

	// there is a problem if the color is oxFFFFFF or nearing to that
	// value in such a case keep the color as Black color
	if (color > (COLORREF)0xFFFFBE && color < (COLORREF)0xFFFFFF)
		XORColor = (COLORREF) 0xFFFFFF;
	else
		XORColor = (COLORREF) color ^ 0xFFFFFF;
	
	return XORColor;	
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayCtrl::validateCurveAnglesEquality(double dStartAngle, double dEndAngle)
{
	// validate the parameters
	if (dStartAngle == dEndAngle)
		return false;
	
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayCtrl::validateCurveAnglesToCreate(double dStartAngle, double dEndAngle)
{
	// validate the parameters
	if (dStartAngle > 360 || dEndAngle >360.0 || dStartAngle < -360 || dEndAngle < -360)
		return false;

	return true;
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayCtrl::validateCurveRadiusToCreate(double dRadius)
{
	// validate the Curve Radius
	if(dRadius <= 0.0)
		return false;
	
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayCtrl::validateTextAlignmentAnglesToCreate(short ucAlignment)
{
	// validate the Text Alignment
	if((ucAlignment < 1) || (ucAlignment > 9))
	{
		return false;
	}
	
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayCtrl::validateTextHeightToCreate(double dHeight)
{
	// validate the Text Height
	if(dHeight <= 0)
	{
		return false;
	}
	
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayCtrl::validateSpCirclePercentRadiusToCreate(long iPercentRadius)
{
	// validate the Special Circle Percent Radius
	if(iPercentRadius <=0 || iPercentRadius>100)
	{
		return false;
	}
	
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayCtrl::validateBaseRotationToSet(double dBaseRotation)
{
	//	check for the base rotation value range
	if(dBaseRotation < -360.0 || dBaseRotation > 360)
	{
		return false;
	}
	
	return true;
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayCtrl::validateScaleFactorToSet(double dScaleFactor)
{
	//	check for the scale factor value
	if(dScaleFactor <= 0.0)
	{
		return false;
	}
	
	return true;
}
//-------------------------------------------------------------------------------------------------
BSTR CGenericDisplayCtrl::getEntityAttributeValue(long ulEntityID, LPCTSTR zAttributeName) 
{
	checkForImageLoading("getEntityAttributeValue");
	
	// validate the entity ID
	GenericEntity *pGenEnt = NULL;
	validateEntityID(ulEntityID, pGenEnt);

	// if the attribute name is Visible, get the visibility of the entity and return
	// as a string
	if (_strcmpi (zAttributeName, "Visible") == 0)
	{
		CString zAttrbValue;
		if (pGenEnt->getVisible())
		{
			zAttrbValue.Format ("1");
			return zAttrbValue.AllocSysString();
		}
		else
		{
			zAttrbValue.Format ("0");
			return zAttrbValue.AllocSysString();
		}
	}
	// if the attribute name is page, get the page number of the entity and return as string
	else if (_strcmpi (zAttributeName, "page") == 0)
	{
		unsigned long ulPage = pGenEnt->getPage();
		CString zAttrbValue;
		zAttrbValue.Format ("%d", ulPage);
		return zAttrbValue.AllocSysString();
	}
	else if (_strcmpi(zAttributeName, "type") == 0)
	{
		CString zAttrbValue = pGenEnt->getDesc().c_str();
		return zAttrbValue.AllocSysString();
	}

	// validate the attribute name
	EntityAttributes entAttrib = pGenEnt->getAttributes();

	STR2STR::iterator itAttFind = entAttrib.find(zAttributeName);

	if(itAttFind == entAttrib.end())
	{
		CString zError;
		zError.Format ("ELI90252 : %s is not a valid attribute name", zAttributeName);
		AfxThrowOleDispatchException (0, zError);
	}

	// get the attribute value
	string strValue = entAttrib[zAttributeName] ;

	// return the attribute value
	CString zAttrbValue = (LPCTSTR)strValue.c_str() ;

	return zAttrbValue.AllocSysString();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::handleAllFileExceptions(CString zStrBuffer)
{
	clear();
	CString zStrError;
	zStrError.Format("ELI90253: %s",zStrBuffer);
	AfxThrowOleDispatchException (0, LPCTSTR(zStrError));
}
//-------------------------------------------------------------------------------------------------
bool CGenericDisplayCtrl::ValidateFontName(CString zFontName)
{
	// NOTE: CreateFontIndirect() function creates a font with default font name like system
	// whenever an invalid font name is passed through LOGFONT structure. So in order to raise
	// an exception whenever an invalid font name is passed, a callback function is defined here
	// which will enumerate the installed fonts with the fontname passed. If the fontname is invalid
	// the call back function will not be called and thereby iaFontCount[0]= 0. Eventhough the callback 
	// function effects the performance, it works fine in finding the invalid font names.So before 
	// creating the text entity, validate the font name and throw exception if it is invalid

	// Get Device context pointer from Image Control
	CDC* pDC = m_pUGDView->m_ImgEdit.GetDC();
	
	// additional data to know whether callback function is called or not
	int iaFontCount[1] = {0};

	// enumerate the font family based on the zFontName
	EnumFontFamilies (pDC->GetSafeHdc(),					// handle to DC
		zFontName,							// font family
		(FONTENUMPROC)EnumFamCallBack,		// callback function
		(LPARAM) iaFontCount);			   	// additional data
	
	if (iaFontCount[0] == 0)
	{
		return false;
	}						
	return true;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::updateImageParameters()
{	
	int nXRes = m_pUGDView->m_ImgEdit.GetXResolution();
	int nYRes = m_pUGDView->m_ImgEdit.GetYResolution();
	//	Set Image Resolution
	setImageResolution(nXRes, nYRes);	

	// Since a new image has been loaded, define the new multiplication factors
	updateMultFactors ();
	
	//	set modified flag
	SetModifiedFlag(FALSE);
	
	char pszBuf[255];
	
	//sprintf (pszBuf, "Scale : %.2f",m_dScaleFactor);
	sprintf_s (pszBuf, sizeof(pszBuf), "Resolution : %d x %d", nXRes, nYRes);
	
	//	Show the scale in the pane 1
	m_pUGDFrame->statusText (1, pszBuf);
}
//-------------------------------------------------------------------------------------------------
BSTR CGenericDisplayCtrl::getEntityInfo(long nEntityID) 
{
	CString strResult;

	// get entity at nEntityID from the map
	GenericEntity *pGenericEntity = getEntity(nEntityID);

	if (pGenericEntity)
	{
		pGenericEntity->getEntDataString(strResult);
	}

	return strResult.AllocSysString();
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setDocumentModified(BOOL bModified) 
{
	// set modified flag
	m_bDocumentModified = bModified;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setCursorHandle(OLE_HANDLE* hCursor) 
{
	if (!m_zBackImageName.IsEmpty())
	{
		m_pUGDView->m_ImgEdit.SetCursorHandle((long*)hCursor);
		m_hCursor = (HCURSOR)hCursor;
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::restoreCursorHandle()
{
	if (!m_zBackImageName.IsEmpty())
	{
		m_pUGDView->m_ImgEdit.SetCursorHandle((long*)m_hCursor);
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::extractZoneEntityImage(long nZoneEntityID, 
												 LPCTSTR strFileName)
{
	//get entity by query with ID
	GenericEntity* pEnt = NULL;
	pEnt = getEntityByID(nZoneEntityID);
	
	if (pEnt->getDesc() != "ZONE")
		AfxThrowOleDispatchException (0, "ELI04939: This is not a zone entity to return zone entity parameters");
	else
	{
		ZoneEntity * pzoneEnt = (ZoneEntity *) pEnt;
		long nStartPosX, nStartPosY, nEndPosX, nEndPosY, nTotalZoneHeight;
		pzoneEnt->getZoneEntParameters(nStartPosX, nStartPosY, nEndPosX, nEndPosY, 
			nTotalZoneHeight);

		// get the entity page number
		CString zPage = getEntityAttributeValue(nZoneEntityID, "Page");
		long nPageNum = atoi(zPage);

		extractZoneImage(nStartPosX, nStartPosY, nEndPosX, nEndPosY, nTotalZoneHeight,
			nPageNum, strFileName);
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::extractZoneImage(long nX1, long nY1, long nX2, 
										   long nY2, long nHeight, long nPageNum,
										   LPCTSTR strFileName)
{
	if (!m_zBackImageName.IsEmpty())
	{
		m_pUGDView->m_ImgEdit.ExtractZoneImage(nX1, nY1, nX2, nY2, nHeight,
			nPageNum, strFileName);
	}
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::getCurrentPageSize(long *pWidth, long *pHeight)
{
	if (!m_zBackImageName.IsEmpty())
	{
		*pWidth = (long) m_pUGDView->getLoadedImageWidth();
		*pHeight = (long) m_pUGDView->getLoadedImageHeight();
	}
	else
	{
		AfxThrowOleDispatchException (0, "ELI06910: No image open!");
	}
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayCtrl::isZoneBorderVisible(long ulZoneEntityID)
{
	//get entity by query with ID
	GenericEntity* pEnt = NULL;
	pEnt = getEntityByID(ulZoneEntityID);
	
	if (pEnt->getDesc() != "ZONE")
	{
		AfxThrowOleDispatchException (0, "ELI11346: Specified ID does not represent a zone entity!");
	}

	ZoneEntity * pZoneEnt = (ZoneEntity *) pEnt;
	return pZoneEnt->m_bBorderVisible ? TRUE : FALSE;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::showZoneBorder(long ulZoneEntityID, BOOL bShow)
{
	//get entity by query with ID
	GenericEntity* pEnt = NULL;
	pEnt = getEntityByID(ulZoneEntityID);
	
	if (pEnt->getDesc() != "ZONE")
	{
		AfxThrowOleDispatchException (0, "ELI11347: Specified ID does not represent a zone entity!");
	}

	// update the zone entity
	ZoneEntity * pZoneEnt = (ZoneEntity *) pEnt;
	pZoneEnt->m_bBorderVisible = (bShow == TRUE);

	// set the modified flag as TRUE
	m_bDocumentModified=TRUE;
	
	//	refresh view
	m_pUGDView->Invalidate();
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::getZoneBorderStyle(long ulZoneEntityID)
{
	//get entity by query with ID
	GenericEntity* pEnt = NULL;
	pEnt = getEntityByID(ulZoneEntityID);
	
	if (pEnt->getDesc() != "ZONE")
	{
		AfxThrowOleDispatchException (0, "ELI11754: Specified ID does not represent a zone entity!");
	}

	ZoneEntity * pZoneEnt = (ZoneEntity *) pEnt;
	return pZoneEnt->m_nBorderStyle;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setZoneBorderStyle(long ulZoneEntityID, long nStyle)
{
	//get entity by query with ID
	GenericEntity* pEnt = NULL;
	pEnt = getEntityByID(ulZoneEntityID);
	
	if (pEnt->getDesc() != "ZONE")
	{
		AfxThrowOleDispatchException (0, "ELI11755: Specified ID does not represent a zone entity!");
	}

	// update the zone entity
	ZoneEntity * pZoneEnt = (ZoneEntity *) pEnt;
	pZoneEnt->m_nBorderStyle = nStyle;

	// set the modified flag as TRUE
	m_bDocumentModified=TRUE;
	
	// refresh view
	m_pUGDView->Invalidate();
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayCtrl::isZoneBorderSpecial(long ulZoneEntityID)
{
	//get entity by query with ID
	GenericEntity* pEnt = NULL;
	pEnt = getEntityByID(ulZoneEntityID);
	
	if (pEnt->getDesc() != "ZONE")
	{
		AfxThrowOleDispatchException (0, "ELI11756: Specified ID does not represent a zone entity!");
	}

	ZoneEntity * pZoneEnt = (ZoneEntity *) pEnt;
	return pZoneEnt->m_bBorderIsSpecial ? TRUE : FALSE;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setZoneBorderSpecial(long ulZoneEntityID, BOOL bSpecial)
{
	//get entity by query with ID
	GenericEntity* pEnt = NULL;
	pEnt = getEntityByID(ulZoneEntityID);
	
	if (pEnt->getDesc() != "ZONE")
	{
		AfxThrowOleDispatchException (0, "ELI11757: Specified ID does not represent a zone entity!");
	}

	// update the zone entity
	ZoneEntity * pZoneEnt = (ZoneEntity *) pEnt;
	pZoneEnt->m_bBorderIsSpecial = (bSpecial == TRUE);

	// set the modified flag as TRUE
	m_bDocumentModified=TRUE;
	
	//	refresh view
	m_pUGDView->Invalidate();
}
//-------------------------------------------------------------------------------------------------
OLE_COLOR CGenericDisplayCtrl::getZoneBorderColor(long ulZoneEntityID)
{
	//get entity by query with ID
	GenericEntity* pEnt = NULL;
	pEnt = getEntityByID(ulZoneEntityID);
	
	if (pEnt->getDesc() != "ZONE")
	{
		AfxThrowOleDispatchException (0, "ELI11348: Specified ID does not represent a zone entity!");
	}

	ZoneEntity * pZoneEnt = (ZoneEntity *) pEnt;
	return pZoneEnt->m_borderColor;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setZoneBorderColor(long ulZoneEntityID, OLE_COLOR newColor)
{
	//get entity by query with ID
	GenericEntity* pEnt = NULL;
	pEnt = getEntityByID(ulZoneEntityID);
	
	if (pEnt->getDesc() != "ZONE")
	{
		AfxThrowOleDispatchException (0, "ELI19396: Specified ID does not represent a zone entity!");
	}

	// update the zone entity
	ZoneEntity * pZoneEnt = (ZoneEntity *) pEnt;
	pZoneEnt->m_borderColor = (COLORREF) newColor;

	// set the modified flag as TRUE
	m_bDocumentModified=TRUE;
	
	//	refresh view
	m_pUGDView->Invalidate();
}
//-------------------------------------------------------------------------------------------------
BOOL CGenericDisplayCtrl::isZoneFilled(long ulZoneEntityID)
{
	//get entity by query with ID
	GenericEntity* pEnt = NULL;
	pEnt = getEntityByID(ulZoneEntityID);
	
	if (pEnt->getDesc() != "ZONE")
	{
		AfxThrowOleDispatchException (0, "ELI11328: Specified ID does not represent a zone entity!");
	}

	ZoneEntity * pZoneEnt = (ZoneEntity *) pEnt;
	return pZoneEnt->m_bFill ? TRUE : FALSE;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setZoneFilled(long ulZoneEntityID, BOOL bFilled)
{
	//get entity by query with ID
	GenericEntity* pEnt = NULL;
	pEnt = getEntityByID(ulZoneEntityID);
	
	if (pEnt->getDesc() != "ZONE")
	{
		AfxThrowOleDispatchException (0, "ELI11330: Specified ID does not represent a zone entity!");
	}

	// update the zone entity
	ZoneEntity * pZoneEnt = (ZoneEntity *) pEnt;
	pZoneEnt->m_bFill = (bFilled == TRUE);

	// set the modified flag as TRUE
	m_bDocumentModified=TRUE;
	
	//	refresh view
	m_pUGDView->Invalidate();
}
//-------------------------------------------------------------------------------------------------
GenericEntity* CGenericDisplayCtrl::getEntity(unsigned long ulEntityID)
{
	map<unsigned long, GenericEntity *>::iterator iter;
	iter = mapIDToEntity.find(ulEntityID);
	if (iter == mapIDToEntity.end())
	{
		AfxThrowOleDispatchException (0, "ELI11353: The specified entity ID is not valid!");
	}

	return iter->second;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setZoneEntityCreationType(long eType)
{
	m_eZoneEntityCreationType = eType;
}
//-------------------------------------------------------------------------------------------------
long CGenericDisplayCtrl::getZoneEntityCreationType()
{
	return m_eZoneEntityCreationType;
}
//-------------------------------------------------------------------------------------------------
void CGenericDisplayCtrl::setZoom(double dZoom)
{
	m_pUGDView->setZoom(dZoom);
}
//-------------------------------------------------------------------------------------------------
double CGenericDisplayCtrl::getZoom()
{
	return m_pUGDView->getZoomFactor();
}
//-------------------------------------------------------------------------------------------------