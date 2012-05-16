// IcoMapDrawingCtrl.cpp : Implementation of CIcoMapDrawingCtrl
#include "stdafx.h"
#include "ArcGISIcoMap.h"
#include "IcoMapDrawingCtrl.h"
#include "EditEventsSink.h"

#include <LicenseMgmt.h>
#include <SpecialIcoMap.h>
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <RegistryPersistenceMgr.h>
#include <cpputil.h>
#include <IcoMapOptions.h>
#include <RegConstants.h>
#include <ComponentLicenseIDs.h>

#include <string>
#include <comdef.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const static string ICOMAP_TOOL_NAME = "IcoMap";

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CIcoMapDrawingCtrl
//-------------------------------------------------------------------------------------------------
CIcoMapDrawingCtrl::CIcoMapDrawingCtrl():
	m_ipApp(NULL),
	m_ipActivePoint(CLSID_Point),
	m_ipEditEventsSink(NULL),
	m_bIsFeatureCreationEnabled(false),
	m_bIsInEditMode(false),
	m_bStartADoc(true),
	m_dwEditCookie(0),
	m_dwDocumentCookie(0),
	m_ExtState(esriESEnabled),
	m_bIsDlgVisible(false),
	m_ipEditor(NULL),
	m_ipDocument(NULL),
	m_ipMxDoc(NULL)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// This is the entry point for IcoMap....execute the global exception
		// related code here
		// Set up the exception handling aspect.
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler(&exceptionDlg);
		UCLIDException::setApplication(IcoMapOptions::sGetInstance().getProductVersion());

		m_ipIcoMapApp.CreateInstance(CLSID_IcoMap);
		ASSERT_RESOURCE_ALLOCATION("ELI11524", m_ipIcoMapApp != NULL);
		m_ipDisplayAdapter.CreateInstance(CLSID_ArcGISDisplayAdapter);
		ASSERT_RESOURCE_ALLOCATION("ELI11736", m_ipDisplayAdapter != NULL);
		m_ipAttributeManager.CreateInstance(CLSID_ArcGISAttributeManager);
		ASSERT_RESOURCE_ALLOCATION("ELI11777", m_ipAttributeManager != NULL);

		m_bitmap = ::LoadBitmap(_Module.m_hInst, MAKEINTRESOURCE(IDB_BMP_ICOMAP_DRAWING));	
		m_cursor = ::LoadCursor(_Module.m_hInst, MAKEINTRESOURCE(IDC_CURSOR_ICOMAP_DRAWING));
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI10530")
}
//-------------------------------------------------------------------------------------------------
CIcoMapDrawingCtrl::~CIcoMapDrawingCtrl()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		DeleteObject(m_bitmap);
		DeleteObject(m_cursor);
		
		IcoMapOptions::sDeleteInstance();

		// delete the input manager instance
		IInputManagerSingletonPtr ipSingleton;
		ipSingleton.CreateInstance(CLSID_InputManagerSingleton);
		ASSERT_RESOURCE_ALLOCATION("ELI07589", ipSingleton != NULL);
		ipSingleton->DeleteInstance();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07588")
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::InterfaceSupportsErrorInfo(REFIID riid)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	// none of the interfaces support IErrorInfo right now
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ICommand
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::get_Enabled(VARIANT_BOOL * Enabled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	if (Enabled == NULL)
	{
		return E_POINTER;
	}

	// icomap tool on the arcmap toolbar shall always be enabled
	*Enabled = VARIANT_TRUE;
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::get_Checked(VARIANT_BOOL * Checked)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	// evaluate Checked to see if icomap tool is the current active tool
	if (Checked == NULL)
	{
		return E_POINTER;
	}

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::get_Name(BSTR * Name)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (Name == NULL)
		return E_POINTER;
	
	_bstr_t bstrName("IcoMapDrawingControl");
	*Name = bstrName.Detach();

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::get_Caption(BSTR * Caption)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (Caption == NULL)
		return E_POINTER;
		
	_bstr_t bstrName("IcoMap Window");
	*Caption = bstrName.Detach();
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::get_Tooltip(BSTR * Tooltip)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (Tooltip == NULL)
		return E_POINTER;
		
	_bstr_t bstrName("IcoMap Window");
	*Tooltip = bstrName.Detach();
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::get_Message(BSTR * Message)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (Message == NULL)
		return E_POINTER;
		
	_bstr_t bstrName("Select an IcoMap drawing tool.");
	*Message = bstrName.Detach();
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::get_HelpFile(BSTR * HelpFile)
{
	if (HelpFile == NULL)
		return E_POINTER;
	
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::get_HelpContextID(LONG * helpID)
{

	if (helpID == NULL)
		return E_POINTER;
	
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::get_Bitmap(OLE_HANDLE * Bitmap)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (Bitmap == NULL)
	{
		return E_POINTER;
	}
		
	*Bitmap = (OLE_HANDLE) m_bitmap;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::get_Category(BSTR * categoryName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (categoryName == NULL)
	{
		return E_POINTER;
	}
	
	// put in the ExtractTools category
	_bstr_t bstrName("ExtractTools");
	
	*categoryName = bstrName.Detach();
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::raw_OnCreate(IDispatch * hook)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Initialize licensed components inside the special IcoMap Components license file
		// using unique passwords as applied to the UCLID String
		LicenseManagement::sGetInstance().initializeLicenseFromFile( 
			g_strIcoMapComponentsLicenseName, gulIcoMapKey1, gulIcoMapKey2, 
			gulIcoMapKey3, gulIcoMapKey4, false );

		// Create ArcMap-related objects
		// These are necessary even if IcoMap is not licensed
 		m_ipApp = hook;
		ASSERT_RESOURCE_ALLOCATION("ELI15918", m_ipApp != NULL);

		IArcGISDependentComponentPtr ipDependentComponent(m_ipDisplayAdapter);
		ASSERT_RESOURCE_ALLOCATION("ELI11464", ipDependentComponent != NULL);
		ipDependentComponent->SetApplicationHook(hook);
		ipDependentComponent = m_ipAttributeManager;
		ASSERT_RESOURCE_ALLOCATION("ELI11465", ipDependentComponent != NULL);
		ipDependentComponent->SetApplicationHook(hook);

		_bstr_t sName("ESRI Object Editor");
		IExtensionPtr ipExtension(m_ipApp->FindExtensionByName(sName));
		m_ipEditor = ipExtension;

		// Initialize any individually licensed components
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder();

		// cocreate instance of IEditEventsSink
		CEditEventsSink* pSink	= new CComObject<CEditEventsSink>(this);
		pSink->SetParentCtrl(this);

		//Pass application to the event sink so that it can
		//respond to the events by itself
		m_ipEditEventsSink = pSink->GetUnknown();
		m_ipEditEventsSink->SetApplicationHook(m_ipApp);

		// Isolate the license validation here but do not display the 
		// exception because the user hasn't chosen to start an unlicensed 
		// IcoMap yet
		try
		{
			// check if this component is licensed
			validateLicense();
			
			// Set Extension state to enabled
			m_ExtState = esriESEnabled;
			
			// Prepare to receive events from ArcMap
			ConnectToEventsSink();
		}
		catch (...)
		{
			// Counld not get license so disabled
			m_ExtState = esriESDisabled;
		}
	
		// lastly, get access to the singleton instance of the inputmanager
		// and set the parent window to be the arcmap window
		IInputManagerSingletonPtr ipSingleton;
		ipSingleton.CreateInstance(CLSID_InputManagerSingleton);
		IInputManagerPtr ipInputManager = ipSingleton->GetInstance();
		ipInputManager->ParentWndHandle = m_ipApp->hWnd;

		// store current tool name guid pair
		storeToolNameGuidPair(ICOMAP_TOOL_NAME, "{D80801D0-7AC8-11D5-817F-0050DAD4FF55}");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01774");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::raw_OnClick(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		try
		{
			// validate license
			validateLicense();
			validateIcoMapLicense();

			// Set to enabled
			m_ExtState = esriESEnabled;

			// Connect to EventsSink
			ConnectToEventsSink();
		}
		catch (...)
		{
			// Undepress the toolbar button
			selectDefaultArcMapTool();
			
			// No License so set state to Disabled
			m_ExtState = esriESDisabled;

			// Disconnect from Events Sink 
			DisconnectFromEventsSink();

			// Rethrow the license failure exception
			// to be caught and displayed down below
			throw;
		}

		// whether or not in edit mode
		m_bIsInEditMode = isInEditMode();

		ShowIcoMapDlg();
		
		setIcoMapAsCurrentTool(true);

		processEnableFeatureEdit();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03839")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ITool
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::get_Cursor(OLE_HANDLE * Cursor)
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (Cursor == NULL)
		{
			return E_POINTER;
		}
		
		// if feature creation is enabled, make cursor look like a cross hair with a 
		// blue puck at the center
		if (m_bIsInEditMode && !isFeatureSelectionEnabled() && m_bIsFeatureCreationEnabled) 
		{
			m_cursor = ::LoadCursor(_Module.m_hInst, MAKEINTRESOURCE(IDC_CURSOR_ICOMAP_DRAWING)); 
		}
		else if (isFeatureSelectionEnabled()) //make cursor look like a square box
		{
			m_cursor = ::LoadCursor(_Module.m_hInst, MAKEINTRESOURCE(IDC_CURSOR_SELECTION));
		}
		else // make the cursor into a white pointer
		{
			m_cursor = ::LoadCursor(_Module.m_hInst, MAKEINTRESOURCE(IDC_CURSOR_NOTHING));
		}
		
		*Cursor = (OLE_HANDLE)m_cursor;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03840")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::raw_OnMouseUp(LONG Button, LONG Shift, LONG X, LONG Y)
{
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::raw_OnMouseMove(LONG Button, LONG Shift, LONG X, LONG Y)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	HRESULT hr(S_OK);	
	try
	{
		// only feature creation is enabled
		if (m_bIsInEditMode && !isFeatureSelectionEnabled() && m_bIsFeatureCreationEnabled)
		{
			if (m_ipEditor)
			{
			/*
			Draw the editor's agent at the active point location.
			If snapping is enabled, then snap the editor's agent accordingly.
				*/
				m_ipEditor->InvertAgent(m_ipActivePoint,0);
				
				ConvertMouseToMapPoint(X, Y, m_ipActivePoint);
				ISnapEnvironmentPtr ipSnapPoint(m_ipEditor);
				if (ipSnapPoint)
				{
					VARIANT_BOOL bSnapped(ipSnapPoint->SnapPoint(m_ipActivePoint));
					
					if (m_ipEditor)
					{
						m_ipEditor->InvertAgent(m_ipActivePoint,0);
					}
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03841")

	return hr;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::raw_OnMouseDown(LONG Button, LONG Shift, LONG X, LONG Y)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	// The mouse up coordinates for the mouse point might not 
	// be the active point coordinates if you have snap turned on. In other words,
	// X, Y might not be the same as x, y from m_ipActivePoint if snap is turned on.
	try
	{	
		static bool bSendingEvent = false;
		
		if (!bSendingEvent)
		{
			// lock the process if icomap is currently processing the mouse up event
			bSendingEvent = true;
			
			if (m_ipIcoMapApp)
			{
				if (isFeatureSelectionEnabled())
				{
					selectFeatureAround(X, Y);
				}
				else if (m_bIsInEditMode && m_bIsFeatureCreationEnabled)
				{
					double xPtMap = m_ipActivePoint->GetX();
					double yPtMap = m_ipActivePoint->GetY();
					
					// set the point as the active point's coordinates
					m_ipIcoMapApp->SetPoint(xPtMap,yPtMap);
				}
			}
			
			// release the lock
			bSendingEvent = false;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01708");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::raw_OnDblClick(void)
{
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::raw_OnKeyDown(LONG keyCode, LONG Shift)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (m_bIsInEditMode)
		{
			if (m_ipIcoMapApp)
			{
				m_ipIcoMapApp->ProcessKeyDown(keyCode, Shift);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03842")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::raw_OnKeyUp(LONG keyCode, LONG Shift)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_bIsInEditMode)
		{
			if (m_ipIcoMapApp)
			{
				m_ipIcoMapApp->ProcessKeyUp(keyCode, Shift);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03843")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::raw_OnContextMenu(LONG X, LONG Y, VARIANT_BOOL * handled)
{
	if (handled == NULL)
	{
		return E_POINTER;
	}
	
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::raw_Refresh(OLE_HANDLE hDC)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	HRESULT hr(S_OK);

	if (!isFeatureSelectionEnabled())
	{
		if (m_ipEditor)
		{
			m_ipEditor->InvertAgent(m_ipActivePoint, hDC);
		}
	}

	return hr;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapDrawingCtrl::raw_Deactivate(VARIANT_BOOL * complete)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	HRESULT hr(S_OK);
	try
	{
		// when ever icomap tool is deactivated, this method will be called.
		
		if (complete != NULL)
		{
			*complete = VARIANT_TRUE;
			
			if (m_ipEditor)
			{
				m_ipEditor->InvertAgent(m_ipActivePoint,0);
			}			

			try
			{
				validateLicense();

				// notify icomap that current tool is not icomap tool
				setIcoMapAsCurrentTool(false);
			}
			catch (...)
			{
				return S_OK;
			}
		}
		else
		{
			hr = E_POINTER;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03844")
		
	return hr;
}

//-------------------------------------------------------------------------------------------------	
// IExtension Methods
//-------------------------------------------------------------------------------------------------	
STDMETHODIMP CIcoMapDrawingCtrl::raw_Startup(VARIANT * initializationData)
{
	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15916");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------	
STDMETHODIMP CIcoMapDrawingCtrl::raw_Shutdown()
{
	try
	{
		// Disconnect from Event Sink
		DisconnectFromEventsSink();

		// Set smart pointers to NULL to release in this order
		m_ipEditEventsSink = NULL;
		m_ipIcoMapApp = NULL;
		m_ipAttributeManager = NULL;
		m_ipDisplayAdapter = NULL;
		m_ipEditor = NULL;
		m_ipApp = NULL;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15917");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------	
// IExtensionConfig Methods
//-------------------------------------------------------------------------------------------------	
STDMETHODIMP CIcoMapDrawingCtrl::get_ProductName(BSTR * Name)
{
	try
	{
		*Name = _bstr_t("Extract Systems IcoMap").Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15904");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------	
STDMETHODIMP CIcoMapDrawingCtrl::get_Description(BSTR * Description)
{
	try
	{
		*Description = _bstr_t("Extract Systems IcoMap").Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15905");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------	
STDMETHODIMP CIcoMapDrawingCtrl::get_State(esriExtensionState * State)
{
	try
	{
		*State = m_ExtState;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15906");

	return S_OK;	
}
//-------------------------------------------------------------------------------------------------	
STDMETHODIMP CIcoMapDrawingCtrl::put_State(esriExtensionState State)
{
	try
	{
		// Save the previous state
		esriExtensionState prevState = m_ExtState;

		// Update the current state
		m_ExtState = State;

		// Check registry for node-locked or concurrent mode
		ELicenseManagementMode eMode = IcoMapOptions::sGetInstance().getLicenseManagementMode();

		// If state is unchanged just return
		if ( m_ExtState == prevState )
		{
			return S_OK;
		}

		// State has changed to enabled or is node locked
		if ( m_ExtState == esriESEnabled || eMode == kNodeLocked)
		{
			try
			{
				try
				{
					// Validate the license - this will obtain license if avaliable otherwise will 
					// throw exception
					IcoMapOptions::sGetInstance().validateIcoMapLicensed();
					
					// Connect to the Event sink
					ConnectToEventsSink();

					// If node locked and get to this point there is a valid license so
					// IcoMap should be enabled. If the state change is to make it disabled need
					// to return COM error so that ArcMap knows it should not be disabled.
					if ( eMode == kNodeLocked && m_ExtState == esriESDisabled )
					{
						// State should be enabled because node license file is valid
						m_ExtState = esriESEnabled;

						// Return error so state will not be changed
						return Error("Cannot disable IcoMap.");
					}
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15920")
			}
			catch(UCLIDException &ue)
			{
				// License was not available
				
				// Set extension state to disabled
				m_ExtState = esriESDisabled;
				
				// Disconnect from Event Sink
				DisconnectFromEventsSink();

				// Hide icomap dialog
				HideIcoMapDlg();

				// Log the exception
				ue.addDebugInfo("License Mode", (eMode == kNodeLocked) ? "Node Locked" : "Concurrent");
				ue.log();

				// If node locked return error with message that there is no valid file
				if ( eMode == kNodeLocked )
				{
					return Error("\r\nThere is no valid IcoMap license file.");
				}
				else
				{
					// Release license if HideIcoMapDlg call just obtained one
					IcoMapOptions::sGetInstance().releaseConcurrentIcoMapLicense();

					// Return error that there is not a license available
					return Error("\r\nThere is no IcoMap license available.");
				}
			}
		}

		// Check for disabled state
		if ( m_ExtState == esriESDisabled )
		{
			// Disconnect from Event Sink
			DisconnectFromEventsSink();

			// Hide icomap dialog
			HideIcoMapDlg();

			// Release license if HideIcoMapDlg call just obtained one
			IcoMapOptions::sGetInstance().releaseConcurrentIcoMapLicense();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15907");
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// CIcoMapDrawingCtrl
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::HideIcoMapDlg(void)
{
	// If Dlg is visible and IcoMapApp is not NULL hide the window
	if (m_bIsDlgVisible && m_ipIcoMapApp)
	{
		m_ipIcoMapApp->ShowIcoMapWindow(VARIANT_FALSE);
		m_bIsDlgVisible = false;
	}
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::ShowIcoMapDlg(void)
{
	// Set Visible flag to default of false
	m_bIsDlgVisible = false;

	// If IcoMapApp has been created
	if (m_ipIcoMapApp)
	{
		// Setup Window 
		m_ipIcoMapApp->SetDisplayAdapter(m_ipDisplayAdapter); 
		m_ipIcoMapApp->SetAttributeManager(m_ipAttributeManager); 

		// Show window
		m_ipIcoMapApp->ShowIcoMapWindow(VARIANT_TRUE);
		m_bIsDlgVisible = true;
	}
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::selectDefaultArcMapTool(void)
{
	try
	{
		ICommandItemPtr ipCmdItem(m_ipApp->CurrentTool);
		if (ipCmdItem)
		{
			if (FindCommandItem(L"PageLayout_SelectTool",ipCmdItem))
			{
				ipCmdItem->Execute();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03087")
}
//-------------------------------------------------------------------------------------------------
esriGeometryType CIcoMapDrawingCtrl::QueryCurrentLayerGeometryType(void)
{
	esriGeometryType geometryType(esriGeometryNull);

	if (m_ipEditor)
	{
		IEditLayersPtr ipEditLayers(m_ipEditor);
		IFeatureLayerPtr ipFeatureLayer(ipEditLayers->CurrentLayer);

		if (ipFeatureLayer)
		{
			IFeatureClassPtr ipFeatureClass(ipFeatureLayer->FeatureClass);
			if (ipFeatureClass)
			{
				geometryType = ipFeatureClass->ShapeType;
			}
		}
	}

	return geometryType;
}
//-------------------------------------------------------------------------------------------------
esriFeatureType CIcoMapDrawingCtrl::QueryCurrentLayerFeatureType(void)
{
	esriFeatureType featureType(esriFTSimple);

	if (m_ipEditor)
	{
		// Get the IEditLayer and IFeatureLayer pointer
		IEditLayersPtr ipEditLayers(m_ipEditor);
		IFeatureLayerPtr ipFeatureLayer(ipEditLayers->CurrentLayer);
		
		if (ipFeatureLayer)
		{
			// Get IFeatureClass pointer
			IFeatureClassPtr ipFeatureClass(ipFeatureLayer->FeatureClass);
			if (ipFeatureClass)
			{
				// Get the feature type of the current layer
				featureType = ipFeatureClass->FeatureType;
			}
		}
	}

	return featureType;
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::ConvertMouseToMapPoint(LONG xPt,LONG yPt,IPointPtr& ipPoint)
{
	IScreenDisplayPtr ipScreenDisplay(m_ipEditor->Display);

	IDisplayTransformationPtr ipDisplayTransformation(ipScreenDisplay->DisplayTransformation);
	ipPoint = ipDisplayTransformation->ToMapPoint(xPt,yPt);
}
//-------------------------------------------------------------------------------------------------
bool CIcoMapDrawingCtrl::FindCommandItem(LPCOLESTR sProgID,ICommandItemPtr& ipCmdItem)
{
	bool bSuccess(false);

	// Make sure the m_ipApp pointer is valid
	ASSERT_RESOURCE_ALLOCATION("ELI15921", m_ipApp != NULL );

	// Get the current document
	IDocumentPtr ipDocument = m_ipApp->Document;
	ASSERT_RESOURCE_ALLOCATION("ELI15922", ipDocument != NULL );
	
	if (ipDocument)
	{
		ICommandBarsPtr ipCmdBars(ipDocument->CommandBars);
		CComVariant vName(sProgID);
		ipCmdItem = ipCmdBars->Find(vName, VARIANT_FALSE, VARIANT_FALSE);
		if (ipCmdItem == NULL)
		{
			bSuccess = false;
		}
		else
		{
			bSuccess = true;
		}
	}

	return bSuccess;
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::ConnectToEventsSink()
{
	// avoid multiple connections
	if (m_dwEditCookie == 0)
	{
		if (m_ipEditor)
		{
			//connect the event to this client sink
			HRESULT hr = AtlAdvise(m_ipEditor, (IUnknown*)m_ipEditEventsSink, IID_IEditEvents, &m_dwEditCookie);
			if (FAILED(hr))
			{
				throw _com_error(hr);
			}
		}
	}

	if (m_dwDocumentCookie == 0)
	{
		if (m_ipApp)
		{
			// Update the document pointer
			m_ipMxDoc = m_ipApp->Document;
			m_ipDocument = m_ipApp->Document;

			//connect the event to this client sink
			HRESULT hr = AtlAdvise(m_ipMxDoc, (IUnknown*)m_ipEditEventsSink, IID_IDocumentEvents, &m_dwDocumentCookie);
			if (FAILED(hr))
			{
				throw _com_error(hr);
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::DisconnectFromEventsSink()
{
	if (m_dwEditCookie)
	{
		if (m_ipEditor)
		{
			//Disconnect sink from document events
			HRESULT hr = AtlUnadvise(m_ipEditor, IID_IEditEvents, m_dwEditCookie);
			if (FAILED(hr))
			{
				throw _com_error(hr);
			}
			m_dwEditCookie = 0;
		}
	}
	
	if (m_dwDocumentCookie)
	{
		if (m_ipMxDoc)
		{
			//Disconnect sink from document events
			HRESULT hr = AtlUnadvise(m_ipMxDoc, IID_IDocumentEvents, m_dwDocumentCookie);
			if (FAILED(hr))
			{
				throw _com_error(hr);
			}
			m_ipMxDoc = NULL;
			m_dwDocumentCookie = 0;
		}

		// Release the document
		if (m_ipDocument != NULL)
		{
			m_ipDocument = NULL;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::reset()
{
	if (m_ipIcoMapApp)
	{
		m_ipIcoMapApp->Reset();
	}
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::notifySketchModified(long nActualNumOfSegments)
{
	if (m_ipIcoMapApp)
	{
		m_ipIcoMapApp->NotifySketchModified(nActualNumOfSegments);
	}
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::createIcoMapAttributeField()
{
	if (m_ipApp)
	{
		IMxDocumentPtr ipMxDoc(m_ipDocument);
		if (ipMxDoc)
		{
			IActiveViewPtr ipActiveView(ipMxDoc->ActiveView);
			if (ipActiveView)
			{
				IMapPtr ipMap(ipActiveView->FocusMap);
				if (ipMap)
				{
					long count = ipMap->LayerCount;
					for (int i = 0; i < count; i++)
					{
						ILayerPtr ipLayer(ipMap->GetLayer(i));
						if (ipLayer)
						{
							IFeatureLayerPtr ipFeatureLayer(ipLayer);
							if (ipFeatureLayer)
							{
								IFeatureClassPtr ipFeatureClass(ipFeatureLayer->FeatureClass);
								if (ipFeatureClass)
								{
									findIcoMapAttributeField(ipFeatureClass, true);	 
								}
							}
						}
					}
				}
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
long CIcoMapDrawingCtrl::findIcoMapAttributeField(IFeatureClassPtr ipFeatureClass, bool bCreate)
{
	long index = -1;
	
	// lazy init
	if (m_apIcoMapCfgMgr.get() == NULL)
	{
		static const string strRoot( gstrREG_ROOT_KEY + "\\IcoMap for ArcGIS\\Options");
		m_apIcoMapCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_CURRENT_USER, strRoot));
		ASSERT_RESOURCE_ALLOCATION("ELI11839", m_apIcoMapCfgMgr.get() != NULL);
	}
	// first check to see if the option for creating the 
	// field is on or off
	static const string strGeneralFolder("\\General");
	static const string strAttrFieldCreation("CreateAttrField");
	
	// creation of a field only takes effect
	// right after the start of an edit session
	static bool bExistsCreate = false;
	if (!bExistsCreate)
	{
		if (!m_apIcoMapCfgMgr->keyExists(strGeneralFolder, strAttrFieldCreation))
		{
			// default to create the field
			m_apIcoMapCfgMgr->createKey(strGeneralFolder, strAttrFieldCreation, "1");
		}
		
		bExistsCreate = true;
	}
	
	string strCreate = m_apIcoMapCfgMgr->getKeyValue(strGeneralFolder, strAttrFieldCreation);
	// creation of the field is turned off
	if (strCreate != "1")
	{
		return -1;
	}
	
	// do not create the icomapattr field if it's a shape file
	// since shape file has limitation of 254 characters for text field.
	if (getWorkspaceType(ipFeatureClass) == kShapefile)
	{
		return -1;
	}

	if (ipFeatureClass)
	{
		esriGeometryType geometryType(ipFeatureClass->ShapeType);
		
		if (geometryType == esriGeometryPolyline || geometryType == esriGeometryPolygon)
		{
			_bstr_t bstrFieldName("IcoMapAttr");
			index = ipFeatureClass->FindField(bstrFieldName);
			// if the field doesn't exist, add one if bCreate is true
			if (index < 0 && bCreate)
			{
				// create a new field
				IFieldPtr ipField(__uuidof(Field));
				ASSERT_RESOURCE_ALLOCATION("ELI19486", ipField != NULL);
				IFieldEditPtr ipFieldEdit(ipField);
				if (ipFieldEdit)
				{
					ipFieldEdit->Name = bstrFieldName;
					ipFieldEdit->Type = esriFieldTypeString;

					static const string strLenKey("AttrFieldLen");
					// get the IcoMapAttr field len from registry
					static bool bExists = false;
					if (!bExists)
					{
						if (!m_apIcoMapCfgMgr->keyExists(strGeneralFolder, strLenKey))
						{
							// default field len to 8000 characters
							m_apIcoMapCfgMgr->createKey(strGeneralFolder, strLenKey, "8000");
						}

						bExists = true;
					}

					// set the length
					long len = 8000;
					try
					{
						len = asLong(m_apIcoMapCfgMgr->getKeyValue(strGeneralFolder, strLenKey));
					}
					catch (...)
					{
						// if persistent data is wrong, reset it
						m_apIcoMapCfgMgr->setKeyValue(strGeneralFolder, strLenKey, "8000");
						len = 8000;
					}

					ipFieldEdit->Length = len;
					ipFieldEdit->IsNullable = VARIANT_TRUE;
					ipFieldEdit->Required = VARIANT_FALSE;
					
					try
					{
						// now add the field
						ipFeatureClass->AddField(ipField);

						// do a quick stop and start editing in order to save the 
						// field created
						quickStartOrStopEditing(ipFeatureClass, false, true);
						quickStartOrStopEditing(ipFeatureClass, true);
					}
					catch (...)
					{
						// if the field can not be created, ignore it.
					}
				}
			}
		}
	}

	return index;
}
//-------------------------------------------------------------------------------------------------
bool CIcoMapDrawingCtrl::isFeatureSelectionEnabled()
{
	if (m_ipIcoMapApp)
	{
		VARIANT_BOOL bEnableSelection(VARIANT_FALSE);
		m_ipIcoMapApp->get_EnableFeatureSelection(&bEnableSelection);
		bool bEnabled = (bEnableSelection == VARIANT_TRUE) ? true : false;

		return bEnabled;
	}
	
	return false;
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::enableEditMode(bool bEnable) 
{
	m_bIsInEditMode = bEnable;

	processEnableFeatureEdit();
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::enableFeatureCreation(bool bEnable) 
{
	m_bIsFeatureCreationEnabled = bEnable;

	processEnableFeatureEdit();
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::processEnableFeatureEdit()
{
	if (m_ipEditor)
	{
		if (m_ipIcoMapApp)
		{
			VARIANT_BOOL bEnableFeatureCreationTool 
				= (m_bIsInEditMode && m_bIsFeatureCreationEnabled) ? VARIANT_TRUE : VARIANT_FALSE;
			m_ipIcoMapApp->EnableFeatureCreation(bEnableFeatureCreationTool);
		}			
	}
}
//-------------------------------------------------------------------------------------------------
bool CIcoMapDrawingCtrl::isInEditMode()
{
	bool bIsInEditMode = false;
	if (m_ipEditor)
	{
		IEditLayersPtr ipEditLayers(m_ipEditor);
		IFeatureLayerPtr ipFeatureLayer(ipEditLayers->CurrentLayer);
		
		if (ipFeatureLayer)
		{
			IFeatureClassPtr ipFeatureClass(ipFeatureLayer->FeatureClass);
			if (ipFeatureClass)
			{
				IDatasetEditPtr ipDatasetEdit(ipFeatureClass);
				if (ipDatasetEdit)
				{
					// check to see if it's in edit session
					VARIANT_BOOL bEdit = ipDatasetEdit->IsBeingEdited();

					bIsInEditMode = (bEdit == VARIANT_TRUE) ? true : false;
				}
			}
		}
	}

	return bIsInEditMode;
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::setIcoMapAsCurrentTool(bool bAsCurrent)
{
	if (m_ipIcoMapApp)
	{
		m_ipIcoMapApp->SetIcoMapAsCurrentTool(bAsCurrent? VARIANT_TRUE : VARIANT_FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::selectFeatureAround(LONG X, LONG Y)
{
	if (m_ipDocument)
	{
		IMxDocumentPtr ipMxDoc(m_ipDocument);
		if (ipMxDoc)
		{
			IActiveViewPtr ipActiveView(ipMxDoc->ActiveView);
			if (ipActiveView)
			{
				IMapPtr ipMap(ipActiveView->FocusMap);
				if (ipMap)
				{
					IActiveViewPtr ipActiveView(ipMap);
					IScreenDisplayPtr ipScreenDisplay(ipActiveView->ScreenDisplay);
					if (ipScreenDisplay)
					{
						IDisplayTransformationPtr ipTrans(ipScreenDisplay->DisplayTransformation);
						if (ipTrans)
						{
							// get the envelope which is about 5 x 5 square around (X, Y) point
							IPointPtr ipLowerLeft(ipTrans->ToMapPoint(X-5, Y-5));
							IPointPtr ipUpperRight(ipTrans->ToMapPoint(X+5, Y+5));
							
							IEnvelopePtr ipEnvelope;
							ipEnvelope.CreateInstance(__uuidof(Envelope));
							ipEnvelope->PutLowerLeft(ipLowerLeft);
							ipEnvelope->PutUpperRight(ipUpperRight);
							
							// clear all the selections first
							HRESULT hr = ipMap->ClearSelection();
							ipActiveView->Refresh();
							// select one and only one feature
							hr = ipMap->SelectByShape(ipEnvelope, NULL, VARIANT_TRUE);
							ipActiveView->PartialRefresh(esriViewGeoSelection, NULL, NULL);
							
							long nSelectionCount = ipMap->SelectionCount;
							if (nSelectionCount == 1)
							{
								// only start a temp edit session if a shapefile workspace just 
								// created (or opened)
								bool bStartTempEditing = false;

								IFeatureClassPtr ipFeatureClass;

								if (m_bStartADoc)
								{
									// get the selected feature class
									ISelectionPtr ipSelection(ipMap->FeatureSelection);
									if (ipSelection)
									{
										IEnumFeaturePtr ipEnumFeature(ipSelection);
										if (ipEnumFeature)
										{
											// get the selected feature
											ipEnumFeature->Reset();
											IFeaturePtr ipFeature(ipEnumFeature->Next());
											if (ipFeature)
											{
												IObjectClassPtr ipObjectClass(ipFeature->Class);
												if (ipObjectClass)
												{
													ipFeatureClass = ipObjectClass;
													if (ipFeatureClass)
													{
														// before notifying icomap about the feature selection, check if the
														// selected feature class is in shapefile workspace, and it's the beginning
														// of a document and it's not in editing mode (of ArcMap), 
														// then do a quick start and stop editing
														if (getWorkspaceType(ipFeatureClass) == kShapefile)
														{
															bStartTempEditing = true;
															// start a temp editing session
															quickStartOrStopEditing(ipFeatureClass, true);
														}
													}
												}
											}
										}
									}
								}

								if (m_ipIcoMapApp)
								{
									// notify icomap that a feature is selected
									// if it's not in edit mode, make feature attributes read only
									m_ipIcoMapApp->OnFeatureSelected(!m_bIsInEditMode ? VARIANT_TRUE : VARIANT_FALSE);
									
									// after the feature is selected, if the edit session is a temp start for 
									// the shapefiles, stop it
									if (bStartTempEditing)
									{
										if (ipFeatureClass)
										{
											// stop the temp editing session
											quickStartOrStopEditing(ipFeatureClass, false);
										}
									}
								}
							}
						}
					}
				}
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
EWorkspaceType CIcoMapDrawingCtrl::getWorkspaceType(IFeatureClassPtr ipFeatureClass)
{
	EWorkspaceType eType(kNone);
	if (ipFeatureClass)
	{
		IDatasetPtr ipDataset(ipFeatureClass);
		IWorkspacePtr ipWorkspace(ipDataset->Workspace);
		if (ipWorkspace)
		{
			IWorkspaceFactoryPtr ipWorkspaceFactory(ipWorkspace->WorkspaceFactory);
			if (ipWorkspaceFactory)
			{
				_bstr_t _bstrName = ipWorkspaceFactory->GetWorkspaceDescription(VARIANT_FALSE);
				CString zName((char*)_bstrName);
				if (zName == "Spatial Database Connection")   // SDE connection
				{
					eType = kGeodatabase;
				}
				else if (zName == "ArcInfo Workspace")		// Coverages
				{
					eType = kCoverage;
				}
				else if (zName == "Personal Geodatabase")	// Access database
				{
					eType = kPersonalGeodatabase;
				}
				else if (zName == "Shapefiles")			// Shapefiles
				{
					eType = kShapefile;
				}
			}
		}
	}

	return eType;
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::quickStartOrStopEditing(IFeatureClassPtr ipFeatureClass, 
												 bool bStart,
												 bool bSaveEdits)
{
	// Story about this method:
	// A defect is found with shapefiles if user wants to use IcoMap view/edit feature tool 
	// when a document is just opened but the edit session has not been started yet. If that selected
	// feature has stored original attributes in the database, in feature attributes dialog 
	// the stored attributes are not shown as if they are not there. The only way to make them 
	// show is to start an editing session from ArcMap. From then on, no matter it's in edit 
	// session or not, the original attributes (if any) will always show in the feature attributes dialog.
	// Therefore, the workaround solution for this defect is to make a quick start and then stop after
	// that feature attributes are displayed.
	// This defect may be inherited from ArcMap where the shapefiles behave oddly from the time 
	// a document is started till the very first edit session is started. This method could 
	// be removed whenever is appropriate.
	try
	{
		if (ipFeatureClass)
		{
			IDatasetPtr ipDataset(ipFeatureClass);
			IWorkspacePtr ipWorkspace(ipDataset->Workspace);
			if (ipWorkspace)
			{
				IWorkspaceEditPtr ipWorkspaceEdit(ipWorkspace);
				VARIANT_BOOL bIsInEditing(ipWorkspaceEdit->IsBeingEdited());
				if (bIsInEditing == VARIANT_FALSE && bStart)
				{
					// if it's not in editing mode
					ipWorkspaceEdit->StartEditing(VARIANT_FALSE);
				}
				else if (bIsInEditing == VARIANT_TRUE && !bStart)
				{
					// if it's in editing mode, do a quit stop
					ipWorkspaceEdit->StopEditing(bSaveEdits ? VARIANT_TRUE : VARIANT_FALSE);
				}
			}
		}
	}
	catch (...)
	{
		// ignore any exception here
	}
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::storeToolNameGuidPair(const string& strToolName, const string& strGUID)
{
	const static string ROOT_FOLDER = gstrREG_ROOT_KEY + "\\ArcGISUtils\\ArcGISDisplayAdapter";
	const static string TOOLNAME_GUID_FOLDER = "\\ToolNameToGUID";	
	if (m_apArcGISUtilsCfgMgr.get() == NULL)
	{
		m_apArcGISUtilsCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_CURRENT_USER, ROOT_FOLDER));
		ASSERT_RESOURCE_ALLOCATION("ELI11734", m_apArcGISUtilsCfgMgr.get() != NULL);
	}

	if (!m_apArcGISUtilsCfgMgr->keyExists(TOOLNAME_GUID_FOLDER, strToolName))
	{
		// create the key
		m_apArcGISUtilsCfgMgr->createKey(TOOLNAME_GUID_FOLDER, strToolName, strGUID);
		return;
	}

	// if the key exists, get the existing GUID value in string format
	string strExistGUIDValue = m_apArcGISUtilsCfgMgr->getKeyValue(TOOLNAME_GUID_FOLDER, strToolName);
	// check if the values are same
	if (_stricmp(strExistGUIDValue.c_str(), strGUID.c_str()) != 0)
	{
		// reset the value
		m_apArcGISUtilsCfgMgr->setKeyValue(TOOLNAME_GUID_FOLDER, strToolName, strGUID);
	}
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::validateLicense()
{
	// Call validateIcoMapLicensed() in IcoMapOptions in order to check 
	// either license file or USB key license
	IcoMapOptions::sGetInstance().validateIcoMapLicensed();
}
//-------------------------------------------------------------------------------------------------
void CIcoMapDrawingCtrl::validateIcoMapLicense()
{
	IESLicensedComponentPtr ipLicense = m_ipIcoMapApp;
	if (ipLicense != NULL)
	{
		if ( ipLicense->IsLicensed() == VARIANT_FALSE )
		{
			UCLIDException ue("ELI13468", "IcoMap is not licensed"  );
			throw ue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
