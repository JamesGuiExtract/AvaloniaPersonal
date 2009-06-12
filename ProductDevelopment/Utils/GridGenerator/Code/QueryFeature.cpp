// QueryFeature.cpp : Implementation of CQueryFeature
#include "stdafx.h"
#include "GridGenerator.h"
#include "QueryFeature.h"
#include "QueryDlg.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CQueryFeature
//-------------------------------------------------------------------------------------------------
CQueryFeature::CQueryFeature()
: m_ipApp(NULL),
  m_ipEditor(NULL),
  m_apQueryDlg(NULL)
{
	try
	{
		m_bitmap = ::LoadBitmap(_Module.m_hInst, MAKEINTRESOURCE(IDB_BMP_QUERY));	
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08254")
}
//-------------------------------------------------------------------------------------------------
CQueryFeature::~CQueryFeature()
{
	try
	{		
		DeleteObject(m_bitmap);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08255");
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryFeature::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IQueryFeature
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ICommand
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryFeature::raw_get_Enabled(VARIANT_BOOL * Enabled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		if (Enabled == NULL)
		{
			return E_POINTER;
		}

		validateLicense();

		// icomap tool on the arcmap toolbar shall always be enabled
		*Enabled = VARIANT_TRUE;
	}
	catch(...)
	{
		*Enabled = VARIANT_FALSE;
	}

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryFeature::raw_get_Checked(VARIANT_BOOL * Checked)
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
STDMETHODIMP CQueryFeature::raw_get_Name(BSTR * Name)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (Name == NULL)
		return E_POINTER;
	
	_bstr_t bstrName("Query Feature");
	*Name = bstrName.Detach();

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryFeature::raw_get_Caption(BSTR * Caption)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (Caption == NULL)
		return E_POINTER;

	string strCaption = "Query Feature";
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
STDMETHODIMP CQueryFeature::raw_get_Tooltip(BSTR * Tooltip)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (Tooltip == NULL)
		return E_POINTER;

	string strToolTip = "Query Feature";
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
STDMETHODIMP CQueryFeature::raw_get_Message(BSTR * Message)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (Message == NULL)
		return E_POINTER;

	string strMsg = "Query a specific feature.";
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
STDMETHODIMP CQueryFeature::raw_get_HelpFile(BSTR * HelpFile)
{
	if (HelpFile == NULL)
		return E_POINTER;
	
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryFeature::raw_get_HelpContextID(LONG * helpID)
{
	if (helpID == NULL)
		return E_POINTER;
	
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryFeature::raw_get_Bitmap(OLE_HANDLE * Bitmap)
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
STDMETHODIMP CQueryFeature::raw_get_Category(BSTR * categoryName)
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
STDMETHODIMP CQueryFeature::raw_OnCreate(IDispatch * hook)
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
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08253");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryFeature::raw_OnClick(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		showQueryDlg();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08260")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CQueryFeature::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Private methods
//-------------------------------------------------------------------------------------------------
void CQueryFeature::showQueryDlg()
{
	if (m_apQueryDlg.get() == NULL)
	{
		m_apQueryDlg = auto_ptr<QueryDlg>(new QueryDlg(m_ipApp));
		ASSERT_RESOURCE_ALLOCATION("ELI08304", m_apQueryDlg.get() != NULL);
		m_apQueryDlg->Create(QueryDlg::IDD, NULL);
	}

	// show the query dialog
	m_apQueryDlg->ShowWindow(SW_SHOW);
}
//-------------------------------------------------------------------------------------------------
void CQueryFeature::validateLicense()
{
	static const unsigned long QUERY_FEATURE_TOOL_ID = gnGRIDTOOL_QUERY_FEATURE;

	VALIDATE_LICENSE( QUERY_FEATURE_TOOL_ID, "ELI12664", "Query Feature Tool" );
}
//-------------------------------------------------------------------------------------------------
