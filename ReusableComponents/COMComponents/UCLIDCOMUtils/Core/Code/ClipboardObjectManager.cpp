// ClipboardObjectManager.cpp : Implementation of CClipboardObjectManager

#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "ClipboardObjectManager.h"
#include "ClipboardManagerWnd.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//-------------------------------------------------------------------------------------------------
// CClipboardObjectManager
//-------------------------------------------------------------------------------------------------
CClipboardObjectManager::CClipboardObjectManager()
{
	try
	{
		m_apCBMWnd = unique_ptr<ClipboardManagerWnd>(new ClipboardManagerWnd());
		ASSERT_RESOURCE_ALLOCATION("ELI05553", m_apCBMWnd.get() != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI05551")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CClipboardObjectManager::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IClipboardObjectManager,
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
// IClipboardObjectManager
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CClipboardObjectManager::Clear()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// call the corresponding method on the underlying clipboard manager
		// window object
		m_apCBMWnd->clear();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05543");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CClipboardObjectManager::GetObjectInClipboard(IUnknown **pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// call the corresponding method on the underlying clipboard manager
		// window object
		IUnknownPtr ipObj = m_apCBMWnd->getObjectFromClipboard();
		if (ipObj)
		{
			// NotifyCopyFromClipboard if necessary
			notifyCopiedFromClipboard(ipObj);

			*pObj = ipObj.Detach();
		}
		else
		{
			*pObj = NULL;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05544");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CClipboardObjectManager::CopyObjectToClipboard(IUnknown *pObj)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// call the corresponding method on the underlying clipboard manager
		// window object
		m_apCBMWnd->copyObjectToClipboard(pObj);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05545");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CClipboardObjectManager::ObjectIsOfType(IID riid, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// call the corresponding method on the underlying clipboard manager
		// window object
		*pbValue =  m_apCBMWnd->objectIsOfType(riid) ? 
			VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05546");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CClipboardObjectManager::ObjectIsIUnknownVectorOfType(IID riid, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// call the corresponding method on the underlying clipboard manager
		// window object
		*pbValue =  m_apCBMWnd->objectIsIUnknownVectorOfType(riid) ? 
			VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05547");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CClipboardObjectManager::IUnknownVectorIsOWDOfType(IID riid, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI17576", pbValue != __nullptr);

		// call the corresponding method on the underlying clipboard manager
		// window object
		*pbValue = asVariantBool(m_apCBMWnd->vectorIsOWDOfType(riid));
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17577");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CClipboardObjectManager::ObjectIsTypeWithDescription(IID riid, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// call the corresponding method on the underlying clipboard manager
		// window object
		*pbValue =  m_apCBMWnd->objectIsTypeWithDescription(riid) ? 
			VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05548");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CClipboardObjectManager::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Helper functions
//-------------------------------------------------------------------------------------------------
void CClipboardObjectManager::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_COMPONENT_ID, "ELI05552", "ClipboardObjectManager" );
}
//-------------------------------------------------------------------------------------------------
void CClipboardObjectManager::notifyCopiedFromClipboard(const IUnknownPtr& ipObj)
{
	try
	{
		// Check if the object is a clipboard copyable object
		UCLID_COMUTILSLib::IClipboardCopyablePtr ipClip = ipObj;
		if (ipClip != __nullptr)
		{
			// Call notify
			ipClip->NotifyCopiedFromClipboard();
			return;
		}

		// Check if the object is an object with description object
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipOWD = ipObj;
		if (ipOWD != __nullptr)
		{
			notifyCopiedFromClipboard(ipOWD->Object);
			return;
		}

		// Check if this object is an IUnknownVector
		UCLID_COMUTILSLib::IIUnknownVectorPtr ipVec = ipObj;
		if (ipVec != __nullptr)
		{
			// Check each item in the IUnknownVector
			long nSize = ipVec->Size();
			for (long i=0; i < nSize; i++)
			{
				// Check the item
				notifyCopiedFromClipboard(ipVec->At(i));
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27259");
}
//-------------------------------------------------------------------------------------------------
