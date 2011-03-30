// AFDocument.cpp : Implementation of CAFDocument
#include "stdafx.h"
#include "AFCore.h"
#include "AFDocument.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CAFDocument
//-------------------------------------------------------------------------------------------------
CAFDocument::CAFDocument()
: m_ipText(__nullptr),
  m_ipStringTags(__nullptr),
  m_ipObjectTags(__nullptr)
{
}
//-------------------------------------------------------------------------------------------------
CAFDocument::~CAFDocument()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16298");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IAFDocument,
		&IID_ILicensedComponent,
		&IID_ICopyableObject
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IAFDocument
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::get_Text(ISpatialString **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipText == __nullptr)
		{
			m_ipText.CreateInstance(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI05870", m_ipText != __nullptr);
		}

		ISpatialStringPtr ipShallowCopy = m_ipText;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05845");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::put_Text(ISpatialString *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipText = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05846");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::get_StringTags(IStrToStrMap **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipStringTags == __nullptr)
		{
			m_ipStringTags.CreateInstance(CLSID_StrToStrMap);
			ASSERT_RESOURCE_ALLOCATION("ELI05871", m_ipStringTags != __nullptr);
		}

		IStrToStrMapPtr ipShallowCopy = m_ipStringTags;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05847");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::put_StringTags(IStrToStrMap *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipStringTags = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05848");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::get_ObjectTags(IStrToObjectMap **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (m_ipObjectTags == __nullptr)
		{
			m_ipObjectTags.CreateInstance(CLSID_StrToObjectMap);
			ASSERT_RESOURCE_ALLOCATION("ELI05872", m_ipObjectTags != __nullptr);
		}

		IStrToObjectMapPtr ipShallowCopy = m_ipObjectTags;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05849");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::put_ObjectTags(IStrToObjectMap *newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_ipObjectTags = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05850");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::raw_IsLicensed(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
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
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		UCLID_AFCORELib::IAFDocumentPtr ipSource(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI08201", ipSource != __nullptr);
	
		ISpatialStringPtr ipSS = ipSource->GetText();
		if (ipSS)
		{
			ICopyableObjectPtr ipCopyObj(ipSS);
			ASSERT_RESOURCE_ALLOCATION("ELI08326", ipCopyObj != __nullptr);
			m_ipText = ipCopyObj->Clone();
		}
		IStrToStrMapPtr ipSTS = ipSource->GetStringTags();
		if (ipSTS)
		{
			ICopyableObjectPtr ipCopyObj(ipSTS);
			ASSERT_RESOURCE_ALLOCATION("ELI08327", ipCopyObj != __nullptr);
			m_ipStringTags = ipCopyObj->Clone();
		}
		IStrToObjectMapPtr ipSTOM = ipSource->GetObjectTags();
		if (ipSTOM)
		{
			ICopyableObjectPtr ipCopyObj(ipSTOM);
			ASSERT_RESOURCE_ALLOCATION("ELI08328", ipCopyObj != __nullptr);
			m_ipObjectTags = ipCopyObj->Clone();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08202");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFDocument::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Create a new IAFDocument object
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_AFDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI05853", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05854");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CAFDocument::validateLicense()
{
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI05851", "AF Document" );
}
//-------------------------------------------------------------------------------------------------
