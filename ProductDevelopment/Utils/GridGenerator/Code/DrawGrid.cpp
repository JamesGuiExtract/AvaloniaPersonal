// DrawGrid.cpp : Implementation of CDrawGrid
#include "stdafx.h"
#include "GridGenerator.h"
#include "DrawGrid.h"
#include "EventSink.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CDrawGrid
//-------------------------------------------------------------------------------------------------
CDrawGrid::CDrawGrid()
: m_ipApp(NULL),
  m_ipEditor(NULL),
  m_ipEventSink(NULL),
  m_dwEditCookie(0), 
  m_bEnbleTool(false)
{
	try
	{
		m_bitmap = ::LoadBitmap(_Module.m_hInst, MAKEINTRESOURCE(IDB_BMP_GRID));	
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08140")
}
//-------------------------------------------------------------------------------------------------
CDrawGrid::~CDrawGrid()
{
	try
	{
		disconnectFromEventsSink();
		
		DeleteObject(m_bitmap);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08142");
}
//-------------------------------------------------------------------------------------------------
void CDrawGrid::createAllFields()
{
	m_GridDrawer.createAllFields();
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDrawGrid::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IDrawGrid
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IESRICommand
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDrawGrid::raw_get_Enabled(VARIANT_BOOL * Enabled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{

		if (Enabled == NULL)
		{
			return E_POINTER;
		}

		// icomap tool on the arcmap toolbar shall always be enabled
		*Enabled = m_bEnbleTool ? VARIANT_TRUE : VARIANT_FALSE;

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13466");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDrawGrid::raw_get_Checked(VARIANT_BOOL * Checked)
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
STDMETHODIMP CDrawGrid::raw_get_Name(BSTR * Name)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (Name == NULL)
			return E_POINTER;
	
	_bstr_t bstrName("Draw Grid");
	*Name = bstrName.Detach();
		
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDrawGrid::raw_get_Caption(BSTR * Caption)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (Caption == NULL)
		return E_POINTER;

	string strCaption = "Draw Grid";
	try
	{
		validateLicense();
	}
	catch(...)
	{
		strCaption = strCaption + "(Not Licensed)";
	}

	_bstr_t bstrName(strCaption.c_str());
	*Caption = bstrName.Detach();
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDrawGrid::raw_get_Tooltip(BSTR * Tooltip)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

		
	if (Tooltip == NULL)
		return E_POINTER;

	string strToolTip = "Draw Grid";
	try
	{
		validateLicense();
	}
	catch(...)
	{
		strToolTip = strToolTip + "(Not Licensed)";
	}

	_bstr_t bstrName(strToolTip.c_str());
	*Tooltip = bstrName.Detach();

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDrawGrid::raw_get_Message(BSTR * Message)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
		
	if (Message == NULL)
		return E_POINTER;

	string strMsg = "Select Draw Grid tool.";
	try
	{
		validateLicense();
	}
	catch(...)
	{
		strMsg = strMsg + "(Not Licensed)";
	}

	_bstr_t bstrName(strMsg.c_str());
	*Message = bstrName.Detach();

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDrawGrid::raw_get_HelpFile(BSTR * HelpFile)
{

	if (HelpFile == NULL)
		return E_POINTER;
		
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDrawGrid::raw_get_HelpContextID(LONG * helpID)
{

	if (helpID == NULL)
		return E_POINTER;

	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDrawGrid::raw_get_Bitmap(OLE_HANDLE * Bitmap)
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
STDMETHODIMP CDrawGrid::raw_get_Category(BSTR * categoryName)
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
STDMETHODIMP CDrawGrid::raw_OnCreate(IDispatch * hook)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Create ArcMap-related objects
 		m_ipApp = hook;
		
		_bstr_t sName("ESRI Object Editor");
		IExtensionPtr ipExtension(m_ipApp->FindExtensionByName(sName));
		m_ipEditor = ipExtension;


		// Initialize any individually licensed components
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder();

		// Isolate the license validation here but do not display the 
		// exception because the user hasn't chosen to start an unlicensed 
		// IcoMap yet
		try
		{
			// check if this component is licensed
			validateLicense();
		}
		catch (...)
		{
			return S_OK;
		}

		// cocreate instance of IEventSink
		CEventSink* pSink	= new CComObject<CEventSink>(this);
		pSink->SetParentCtrl(this);
		
		//Pass application to the event sink so that it can
		//respond to the events by itself
		m_ipEventSink = pSink->GetUnknown();
		m_ipEventSink->SetApplicationHook(m_ipApp);

		connectToEventsSink();

		m_GridDrawer.setEditor(m_ipEditor);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08094");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDrawGrid::raw_OnClick(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		// bring up the dialog for entering values
		m_GridDrawer.drawGrid();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08097")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDrawGrid::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CDrawGrid::connectToEventsSink()
{
	// avoid multiple connections
	if (m_dwEditCookie == 0)
	{
		if (m_ipEditor)
		{
			//connect the event to this client sink
			HRESULT hr = AtlAdvise(m_ipEditor, (IUnknown*)m_ipEventSink, IID_IEditEvents, &m_dwEditCookie);
			if (FAILED(hr))
			{
				throw _com_error(hr);
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CDrawGrid::disconnectFromEventsSink()
{
	if (m_dwEditCookie)
	{
		if (m_ipEditor)
		{
			//Disconnect sink from edit events
			HRESULT hr = AtlUnadvise(m_ipEditor, IID_IEditEvents, m_dwEditCookie);
			if (FAILED(hr))
			{
				throw _com_error(hr);
			}
			m_ipEditor = 0;
			m_dwEditCookie = 0;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CDrawGrid::validateLicense()
{
	static const unsigned long GRID_GENERATOR_ID = gnGRIDTOOL_DRAW_FEATURE;

	VALIDATE_LICENSE( GRID_GENERATOR_ID, "ELI12653", "Grid Generator" );
}
//-------------------------------------------------------------------------------------------------
	