// SRIRCommand.cpp : Implementation of CSRIRCommand
#include "stdafx.h"
#include "ArcGISUtils.h"
#include "SRIRCommand.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <TemporaryResourceOverride.h>
#include <IcoMapOptions.h>

static const string g_strSRIRName( "Spot Recognition Window" );

//-------------------------------------------------------------------------------------------------
// CSRIRCommand
//-------------------------------------------------------------------------------------------------
CSRIRCommand::CSRIRCommand()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_bitmap = ::LoadBitmap(_Module.m_hInst, MAKEINTRESOURCE(IDB_BMP_SRIR));
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI03921")
}
//-------------------------------------------------------------------------------------------------
CSRIRCommand::~CSRIRCommand()
{
	try
	{
		// Since this uses the validateIcoMapLicensed function any concurrent license
		// should be released
		IcoMapOptions::sGetInstance().releaseConcurrentIcoMapLicense();
		// delete the singleton instance of the input manager
//		IInputManagerSingletonPtr ipSingleton(CLSID_InputManagerSingleton);
//		ASSERT_RESOURCE_ALLOCATION("ELI07586", ipSingleton != NULL);
//		ipSingleton->DeleteInstance();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07583")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRCommand::InterfaceSupportsErrorInfo(REFIID riid)
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
STDMETHODIMP CSRIRCommand::get_Enabled(VARIANT_BOOL *Enabled)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03924")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRCommand::get_Checked(VARIANT_BOOL *Checked)
{
	// Check parameter
	if (!Checked)
	{
		return E_POINTER;
	}

	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRCommand::get_Name(BSTR *Name)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		if (!Name)
		{
			return E_POINTER;
		}

		*Name = _bstr_t( g_strSRIRName.c_str() ).Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03925")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRCommand::get_Caption(BSTR *Caption)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		if (!Caption)
		{
			return E_POINTER;
		}

		*Caption = _bstr_t( g_strSRIRName.c_str() ).Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03926")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRCommand::get_Tooltip(BSTR *Tooltip)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		if (!Tooltip)
		{
			return E_POINTER;
		}

		*Tooltip = _bstr_t( g_strSRIRName.c_str() ).Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03927")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRCommand::get_Message(BSTR *Message)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check parameter
		if (!Message)
		{
			return E_POINTER;
		}

		*Message = _bstr_t( g_strSRIRName.c_str() ).Detach();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03928")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRCommand::get_HelpFile(BSTR *HelpFile)
{
	// Check parameter
	if (!HelpFile)
	{
		return E_POINTER;
	}
	
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRCommand::get_HelpContextID(long *helpID)
{
	// Check parameter
	if (!helpID)
	{
		return E_POINTER;
	}
	
	return E_NOTIMPL;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRCommand::get_Bitmap(OLE_HANDLE *Bitmap)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03929")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRCommand::get_Category(BSTR *categoryName)
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03930")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRCommand::raw_OnCreate(IDispatch* hook)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Initialize any individually licensed components
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07595");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRCommand::raw_OnClick()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		validateLicense();

		IInputManagerSingletonPtr ipSingleton(CLSID_InputManagerSingleton);
		ASSERT_RESOURCE_ALLOCATION("ELI10370", ipSingleton != NULL);
		IInputManagerPtr ipInputManager = ipSingleton->GetInstance();
		if (ipInputManager)
		{
			CWaitCursor wait;

			// Create new Spot Recognition Window input receiver
			IInputReceiverPtr ipInputReceiver(CLSID_SpotRecognitionWindow);
			if (ipInputReceiver)
			{
				ipInputManager->ConnectInputReceiver(ipInputReceiver);
				
				if (!::isVirtKeyCurrentlyPressed(VK_SHIFT))
				{
					ISpotRecognitionWindowPtr ipSpotRecIR(ipInputReceiver);
					ipSpotRecIR->ShowOpenDialogBox();
				}
			}
		}

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03932")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRCommand::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CSRIRCommand::validateLicense()
{
	IcoMapOptions::sGetInstance().validateIcoMapLicensed();
}
//-------------------------------------------------------------------------------------------------
