// InputEntity.cpp : Implementation of CInputEntity
#include "stdafx.h"
#include "IFCore.h"
#include "InputEntity.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInputEntity,
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
// CInputEntity
//-------------------------------------------------------------------------------------------------
CInputEntity::CInputEntity()
{
}
//-------------------------------------------------------------------------------------------------
CInputEntity::~CInputEntity()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16461");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::Delete()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		if (m_ipEntityMgr)
		{
			// delegate the call to the specified input entity manager
			m_ipEntityMgr->Delete(m_bstrEntityID);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02430")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::SetText(BSTR strText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipEntityMgr)
		{
			// delegate the call to the specified input entity manager
			m_ipEntityMgr->SetText(m_bstrEntityID, strText);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02432")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::GetText(BSTR *pstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		if (m_ipEntityMgr)
		{
			// delegate the call to the specified input entity manager
			*pstrText = m_ipEntityMgr->GetText(m_bstrEntityID).Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02434")


	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::CanBeDeleted(VARIANT_BOOL *pbCanBeDeleted)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		if (m_ipEntityMgr)
		{
			// delegate the call to the specified input entity manager
			*pbCanBeDeleted = m_ipEntityMgr->CanBeDeleted(m_bstrEntityID);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02856")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::CanBeMarkedAsUsed(VARIANT_BOOL *pbCanBeMarkedAsUsed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipEntityMgr)
		{
			// delegate the call to the specified input entity manager
			*pbCanBeMarkedAsUsed = m_ipEntityMgr->CanBeMarkedAsUsed(m_bstrEntityID);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19277")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::MarkAsUsed(VARIANT_BOOL bValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipEntityMgr)
		{
			// delegate the call to the specified input entity manager
			m_ipEntityMgr->MarkAsUsed(m_bstrEntityID, bValue);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02441")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::IsMarkedAsUsed(VARIANT_BOOL *pbIsMarkedAsUsed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		if (m_ipEntityMgr)
		{
			// delegate the call to the specified input entity manager
			*pbIsMarkedAsUsed = m_ipEntityMgr->IsMarkedAsUsed(m_bstrEntityID);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02444")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::IsFromPersistentSource(VARIANT_BOOL *pbIsFromPersistentSource)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipEntityMgr)
		{
			// delegate the call to the specified input entity manager
			*pbIsFromPersistentSource = m_ipEntityMgr->IsFromPersistentSource(m_bstrEntityID);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02447")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::GetPersistentSourceName(BSTR *pstrSourceName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		if (m_ipEntityMgr)
		{
			// delegate the call to the specified input entity manager	
			*pstrSourceName = m_ipEntityMgr->GetPersistentSourceName(m_bstrEntityID);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02450")


	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::GetOCRImage(BSTR *pbstrImageFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipEntityMgr)
		{
			// delegate the call to the specific input entity manager
			*pbstrImageFileName = m_ipEntityMgr->GetOCRImage(m_bstrEntityID);
		}
		else
		{
			*pbstrImageFileName = _bstr_t("").Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02997")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::HasBeenOCRed(VARIANT_BOOL *pbHasBeenOCRed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipEntityMgr)
		{
			// delegate the call to the specific input entity manager
			*pbHasBeenOCRed = m_ipEntityMgr->HasBeenOCRed(m_bstrEntityID);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02453")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::InitInputEntity(IInputEntityManager *pEntityManager, BSTR strID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Initialize the input entity manager ( NULL is okay here - WEL 10/18/07 )
		m_ipEntityMgr = pEntityManager;

		// store this entity id 
		m_bstrEntityID = strID;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02456")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::HasIndirectSource(VARIANT_BOOL *pbHasIndirectSource)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipEntityMgr)
		{
			*pbHasIndirectSource = m_ipEntityMgr->HasIndirectSource(m_bstrEntityID);
		}

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03126")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::GetIndirectSource(BSTR *pstrIndirectSource)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipEntityMgr)
		{
			*pstrIndirectSource = m_ipEntityMgr->GetIndirectSource(m_bstrEntityID);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03127")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::GetOCRZones(IIUnknownVector **pRasterZones)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipEntityMgr)
		{
			IIUnknownVectorPtr ipRasterZones = m_ipEntityMgr->GetOCRZones(m_bstrEntityID);
			ASSERT_RESOURCE_ALLOCATION("ELI18105", ipRasterZones != __nullptr);

			*pRasterZones = ipRasterZones.Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03307")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputEntity::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
void CInputEntity::validateLicense()
{
	static const unsigned long INPUT_ENTITY_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( INPUT_ENTITY_COMPONENT_ID, "ELI04124", "Input Entity" );
}
//-------------------------------------------------------------------------------------------------
