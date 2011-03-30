
#include "stdafx.h"
#include "resource.h"
#include "UCLIDCOMUtils.h"
#include "ObjectPropertiesUI.h"
#include "ObjPropertiesDlg.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectPropertiesUI::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IObjectPropertiesUI,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IObjectPropertiesUI
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectPropertiesUI::DisplayProperties1(IUnknown *pObj, BSTR strTitle,
													 VARIANT_BOOL *pAppliedChanges)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	TemporaryResourceOverride rcOverride(_Module.m_hInstResource);

	try
	{
		ASSERT_ARGUMENT("ELI29891", pAppliedChanges != __nullptr);

		// validate license
		validateLicense();

		// Check to see if configuration is supported via IConfigurableObject.
		UCLID_COMUTILSLib::IConfigurableObjectPtr ipConfigurableObject(pObj);
		if (ipConfigurableObject != __nullptr)
		{
			// If so, run the configuration.
			bool bSettingsApplied = asCppBool(ipConfigurableObject->RunConfiguration());

			// If settings were applied and the object implements IMustBeConfiguredObject,
			// ensure that the object is now properly configured.
			UCLID_COMUTILSLib::IMustBeConfiguredObjectPtr ipMustBeConfiguredObject(pObj);
			while (bSettingsApplied && ipMustBeConfiguredObject != __nullptr)
			{
				// Allow the UI to update to avoid any artifacts from the closed config screen.
				emptyWindowsMessageQueue();

				// If the object is correctly configured, exit the loop; configuration is complete.
				if (asCppBool(ipMustBeConfiguredObject->IsConfigured()))
				{
					break;
				}

				// If configuration is not complete, prompt the user...
				MessageBox(NULL, "Object has not been configured completely.  "
					"Please specify all required properties.", "Configuration", MB_ICONEXCLAMATION);
				
				// Then re-run configuration until the object is correctly configured or
				// configuration is cancelled or aborted.
				bSettingsApplied = asCppBool(ipConfigurableObject->RunConfiguration());
			}

			*pAppliedChanges = bSettingsApplied ? VARIANT_TRUE : VARIANT_FALSE;
			
			return S_OK;
		}

		// create and show the property page container dialog 
		ObjPropertiesDlg dlg(pObj, asString( strTitle ).c_str() );
		int ret = dlg.DoModal();
		
		*pAppliedChanges = (ret == IDOK) ? VARIANT_TRUE : VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04175")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CObjectPropertiesUI::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
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
// Private methods
//-------------------------------------------------------------------------------------------------
void CObjectPropertiesUI::validateLicense()
{
	static const unsigned long OBJECT_PROPERTIES_UI_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( OBJECT_PROPERTIES_UI_COMPONENT_ID, "ELI04178", "ObjectPropertiesUI" );
}
//-------------------------------------------------------------------------------------------------
