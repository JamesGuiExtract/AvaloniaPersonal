// HTIRCommand.cpp : Implementation of CHTIRCommand
#include "stdafx.h"
#include "ArcGISUtils.h"
#include "HTIRCommand.h"


#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <TemporaryResourceOverride.h>
#include <IcoMapOptions.h>

static const string g_strHTIRName( "Highlighted Text Window" );

//-------------------------------------------------------------------------------------------------
// CHTIRCommand
//-------------------------------------------------------------------------------------------------
CHTIRCommand::CHTIRCommand()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_bitmap = ::LoadBitmap(_Module.m_hInst, MAKEINTRESOURCE(IDB_BMP_HTIR));
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI03922")
}
//-------------------------------------------------------------------------------------------------
CHTIRCommand::~CHTIRCommand()
{
	try
	{
		// Since this uses the validateIcoMapLicensed function any concurrent license
		// should be released
		IcoMapOptions::sGetInstance().releaseConcurrentIcoMapLicense();

		// delete the singleton instance of the inputManager
//		IInputManagerSingletonPtr ipSingleton(CLSID_InputManagerSingleton);
//		ASSERT_RESOURCE_ALLOCATION("ELI07585", ipSingleton != NULL);
//		ipSingleton->DeleteInstance();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07582")
}

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ICommand
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ICommand
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::get_Enabled(VARIANT_BOOL *Enabled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		if (!Enabled) 
		{
			return E_POINTER;
		}
		
		*Enabled = VARIANT_TRUE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03934")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::get_Checked(VARIANT_BOOL *Checked)
{
	if (!Checked)
	{
		return E_POINTER;
	}

	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::get_Name(BSTR *Name)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		if (!Name)
		{
			return E_POINTER;
		}

		*Name = _bstr_t( g_strHTIRName.c_str() ).Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03935")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::get_Caption(BSTR *Caption)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		if (!Caption)
		{
			return E_POINTER;
		}

		*Caption = _bstr_t( g_strHTIRName.c_str() ).Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03936")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::get_Tooltip(BSTR *Tooltip)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		if (!Tooltip)
		{
			return E_POINTER;
		}

		*Tooltip = _bstr_t( g_strHTIRName.c_str() ).Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03937")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::get_Message(BSTR *Message)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		if (!Message)
		{
			return E_POINTER;
		}

		*Message = _bstr_t( g_strHTIRName.c_str() ).Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03938")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::get_HelpFile(BSTR *HelpFile)
{
	if (!HelpFile)
	{
		return E_POINTER;
	}

	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::get_HelpContextID(long *helpID)
{
	if (!helpID)
	{
		return E_POINTER;
	}

	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::get_Bitmap(OLE_HANDLE *Bitmap)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		if (!Bitmap)
		{
			return E_POINTER;
		}

		*Bitmap = (OLE_HANDLE) m_bitmap;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03939")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::get_Category(BSTR *categoryName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		if (!categoryName)
		{
			return E_POINTER;
		}

		// Put in the ExtractTools category
		_bstr_t bstrName("ExtractTools");
		*categoryName = bstrName.Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03940")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::raw_OnCreate(IDispatch* hook)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// Initialize any individually licensed components
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07594");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::raw_OnClick()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		validateLicense();

		IInputManagerSingletonPtr ipSingleton(CLSID_InputManagerSingleton);
		ASSERT_RESOURCE_ALLOCATION("ELI10369", ipSingleton != NULL);
		IInputManagerPtr ipInputManager = ipSingleton->GetInstance();
		// create highlight text window using input manager
		if (ipInputManager)
		{
			CWaitCursor wait;

			// Create new Spot Recognition Window input receiver
			IInputReceiverPtr ipInputReceiver(CLSID_HighlightedTextWindow);
			if (ipInputReceiver)
			{
				ipInputManager->ConnectInputReceiver(ipInputReceiver);
				
				if (!::isVirtKeyCurrentlyPressed(VK_SHIFT))
				{
					IHighlightedTextWindowPtr ipHTIR(ipInputReceiver);
					ipHTIR->ShowOpenDialogBox();
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03942")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CHTIRCommand::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		// if validateLicense doesn't throw any exception, then pbValue is true
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
void CHTIRCommand::validateLicense()
{
	IcoMapOptions::sGetInstance().validateIcoMapLicensed();
}
//-------------------------------------------------------------------------------------------------
